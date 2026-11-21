using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Navigation;
using System.Xml;
using BlueStacks.Common;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace BlueStacks.Installer
{
	// Token: 0x02000013 RID: 19
	public class InstallerUtils
	{
		// Token: 0x06000054 RID: 84
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool GetExitCodeProcess(IntPtr hProcess, out uint ExitCode);

		// Token: 0x06000055 RID: 85
		[DllImport("user32.dll", SetLastError = true)]
		public static extern int GetSystemMetrics(int nIndex);

		// Token: 0x06000056 RID: 86 RVA: 0x00006ED8 File Offset: 0x000050D8
		public static bool IsTouchSupported()
		{
			int systemMetrics = InstallerUtils.GetSystemMetrics(95);
			bool flag = Marshal.GetLastWin32Error() != 0;
			if (flag)
			{
				Logger.Error("IsTouchSupported failed with error code: {0}", new object[]
				{
					Marshal.GetLastWin32Error()
				});
			}
			return systemMetrics > 0;
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00006F24 File Offset: 0x00005124
		internal static string GenerateUniqueInstallId()
		{
			Logger.Info("Generating install ID");
			return GuidUtils.GenerateGuid();
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00006F48 File Offset: 0x00005148
		internal static string ConvertIntToEnumString(int enumCode)
		{
			return Enum.GetName(typeof(InstallerCodes), (InstallerCodes)enumCode);
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00006F70 File Offset: 0x00005170
		internal static CmdRes ExtractZip(string zipFilePath, string extractDirectory, MainWindow win = null)
		{
			bool exitVar = false;
			bool retryPressed = false;
			CmdRes cmdRes = new CmdRes();
			while (!exitVar)
			{
				string currentDirectory = Directory.GetCurrentDirectory();
				string cmd = Path.Combine(currentDirectory, "7zr.exe");
				bool flag = !Directory.Exists(extractDirectory);
				if (flag)
				{
					Directory.CreateDirectory(extractDirectory);
				}
				Logger.Info("Extracting {0} to {1}", new object[]
				{
					zipFilePath,
					extractDirectory
				});
				string args = string.Format(CultureInfo.InvariantCulture, "x \"{0}\" -o\"{1}\" -aoa", new object[]
				{
					zipFilePath,
					extractDirectory
				});
				cmdRes = RunCommand.RunCmd(cmd, args, true, false, 0);
				bool flag2 = win != null && cmdRes.ExitCode != 0;
				if (flag2)
				{
					ManualResetEvent sLocker = new ManualResetEvent(false);
					win.Dispatcher.Invoke(new Action(delegate()
					{
						CustomMessageWindow customMessageWindow = new CustomMessageWindow();
						Logger.Info("Creating Retry banner");
						InstallerStats.Enqueue("retry_popup_shown", false, "", "");
						customMessageWindow.TitleTextBlock.Text = string.Format(LocaleStrings.GetLocalizedString("STRING_ERROR", ""), new object[0]);
						Uri navigateUri = new Uri("https://support.bluestacks.com/hc/articles/360061493512");
						string localizedString = LocaleStrings.GetLocalizedString("STRING_CONTACT_US", "");
						customMessageWindow.AddInlineHyperlink(string.Format(LocaleStrings.GetLocalizedString("STRING_RETRY_BANNER_MESSAGE", ""), localizedString), localizedString, navigateUri, delegate(object sender, RequestNavigateEventArgs e)
						{
							Process.Start(e.Uri.ToString());
						});
						EventHandler retry = delegate
						{
							retryPressed = true;
							InstallerStats.Enqueue("retry_button_pressed", false, "", "");
						};
						EventHandler abort = delegate { exitVar = true; };
						customMessageWindow.AddButton(ButtonColors.Blue, "STRING_RETRY", retry, null, false, null, true);
						customMessageWindow.AddButton(ButtonColors.WhiteWithBlackFG, "STRING_ABORT", abort, null, false, null, true);
						customMessageWindow.CloseButtonHandle(abort, null);
						customMessageWindow.Owner = win;
						customMessageWindow.ShowDialog();
						customMessageWindow.Close();
						sLocker.Set();
					}), new object[0]);
					sLocker.WaitOne();
				}
				else
				{
					exitVar = true;
				}
			}
			bool flag3 = retryPressed && cmdRes.ExitCode == 0;
			if (flag3)
			{
				InstallerStats.Enqueue("zip_extraction_successful_on_retry", false, "", "");
			}
			return cmdRes;
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000070DC File Offset: 0x000052DC
		internal static void FixDataOrigUuid(string filePath)
		{
			Logger.Info("In FixDataOrigUuid");
			try
			{
				bool flag = !File.Exists(filePath);
				if (flag)
				{
					Logger.Warning("File does not exist {0}", new object[]
					{
						filePath
					});
				}
				int exitCode = RunCommand.RunCmd(Path.Combine(InstallerProperties.PFDirNew, "BstkVMMgr.exe"), "internalcommands sethduuid " + filePath, true, false, 0).ExitCode;
				bool flag2 = exitCode == 0;
				if (flag2)
				{
					Logger.Info("Successfully fixed UUID");
				}
				else
				{
					Logger.Warning("Could not fix UUID. ExitCode: {0}", new object[]
					{
						exitCode
					});
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Exception in sethduuid. Ex: {0}", new object[]
				{
					ex
				});
			}
		}

		// Token: 0x0600005B RID: 91 RVA: 0x000071A4 File Offset: 0x000053A4
		internal static void LogSpaceAvailableInDrive(string pf, string pd)
		{
			Logger.Info("Logging space available for install locations");
			try
			{
				long num = 1048576L;
				long availableDiskSpaceOfDrive = IOUtils.GetAvailableDiskSpaceOfDrive(pd);
				Logger.Info("Space available in {0}: {1} MB", new object[]
				{
					pd,
					availableDiskSpaceOfDrive / num
				});
				availableDiskSpaceOfDrive = IOUtils.GetAvailableDiskSpaceOfDrive(pf);
				Logger.Info("Space available in {0}: {1} MB", new object[]
				{
					pf,
					availableDiskSpaceOfDrive / num
				});
			}
			catch (Exception ex)
			{
				Logger.Warning("Couldn't check disk space. Ex: {0}", new object[]
				{
					ex.Message
				});
			}
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00007248 File Offset: 0x00005448
		internal static void LogDirList(List<string> listOfDirs)
		{
			List<string> list = listOfDirs.Distinct<string>().ToList<string>();
			foreach (string dir in list)
			{
				InstallerUtils.LogDir(dir);
			}
		}

		// Token: 0x0600005D RID: 93 RVA: 0x000072A8 File Offset: 0x000054A8
		internal static void LogDir(string dir)
		{
			try
			{
				RunCommand.RunCmd("cmd", string.Format(CultureInfo.InvariantCulture, "/c dir \"{0}\" /s", new object[]
				{
					dir
				}), true, false, 0);
			}
			catch (Exception ex)
			{
				Logger.Warning("Couldn't log dir {0}, ignoring exception: {1}", new object[]
				{
					dir,
					ex.Message
				});
			}
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00007314 File Offset: 0x00005514
		private static string CreateStagingDir()
		{
			string randomFileName = Path.GetRandomFileName();
			string text = Path.Combine(Path.GetTempPath(), randomFileName);
			bool flag = Directory.Exists(text);
			if (flag)
			{
				Directory.Delete(text);
			}
			Directory.CreateDirectory(text);
			return text;
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00007354 File Offset: 0x00005554
		public static void CopyFiles(string src, string dest)
		{
			foreach (string text in Directory.GetFiles(src))
			{
				string fileName = Path.GetFileName(text);
				string destFileName = Path.Combine(dest, fileName);
				File.Copy(text, destFileName);
			}
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00007398 File Offset: 0x00005598
		private static void ExportBluestacksRegistry(string destination, string fileName)
		{
			try
			{
				string args = string.Format(CultureInfo.InvariantCulture, "EXPORT HKLM\\{0} \"{1}\"", new object[]
				{
					"Software\\BlueStacks_msi5",
					Path.Combine(destination, fileName)
				});
				Utils.RunCmd("reg.exe", args, null);
			}
			catch
			{
			}
		}

		// Token: 0x06000061 RID: 97 RVA: 0x000073F4 File Offset: 0x000055F4
		internal static string ZipLogsAndRegistry(string bsLogsDir, string pdDir = "", bool getRegistry = true)
		{
			Logger.Info("In ZipLogsAndRegistry");
			string result;
			try
			{
				string text = InstallerUtils.CreateStagingDir();
				string text2 = Path.Combine(text, "Logs");
				bool flag = Directory.Exists(bsLogsDir);
				if (flag)
				{
					Directory.CreateDirectory(text2);
					InstallerUtils.CopyFiles(bsLogsDir, text2);
				}
				else
				{
					Logger.Warning("Log folder not found at {0}", new object[]
					{
						bsLogsDir
					});
				}
				if (getRegistry)
				{
					InstallerUtils.ExportBluestacksRegistry(text, "RegHKLM.txt");
				}
				try
				{
					string text3 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
					text3 = Path.Combine(text3, "Bluestacks");
					bool flag2 = Directory.Exists(text3);
					if (flag2)
					{
						InstallerUtils.CopyFiles(text3, text);
					}
					else
					{
						Logger.Warning("installer dir not exist: " + text3);
					}
				}
				catch (Exception ex)
				{
					Logger.Error("exception in copying log dir files.." + ex.ToString());
				}
				try
				{
					string path = Path.Combine(pdDir, "Logs");
					bool flag3 = Directory.Exists(path);
					if (flag3)
					{
						foreach (string text4 in Directory.GetFiles(path))
						{
							string destFileName = Path.Combine(text, Path.GetFileName(text4));
							File.Copy(text4, destFileName);
						}
					}
				}
				catch (Exception ex2)
				{
					Logger.Error("exception in copying log dir files.." + ex2.ToString());
				}
				string[] array = new string[]
				{
					Path.Combine(pdDir, "bluestacks.conf"),
					Path.Combine(Path.GetTempPath(), "BSTCleaner.log")
				};
				foreach (string text5 in array)
				{
					try
					{
						bool flag4 = File.Exists(text5);
						if (flag4)
						{
							File.Copy(text5, Path.Combine(text, Path.GetFileName(text5)), true);
						}
					}
					catch (Exception ex3)
					{
						Logger.Warning("Failed to copy file - {0}. Ex : {1}", new object[]
						{
							text5,
							ex3
						});
					}
				}
				string text6 = Path.Combine(Path.GetTempPath(), "Installer.zip");
				InstallerUtils.CreateZipFile(text, text6);
				Directory.Delete(text, true);
				result = text6;
			}
			catch (Exception ex4)
			{
				Logger.Error("Error in creating zip file " + ex4.ToString());
				result = null;
			}
			return result;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x0000768C File Offset: 0x0000588C
		internal static void CreateDataOrigForDeployedImages()
		{
			foreach (string text in InstallerProperties.ImageList)
			{
				string path = Path.Combine(InstallerProperties.PDDirNew, "Engine\\" + text);
				string text2 = Path.Combine(path, "Data.vhdx");
				string destFileName = Path.Combine(path, "Data_orig.vhdx");
				bool flag = File.Exists(text2);
				if (flag)
				{
					Logger.Info("Creating data_orig.vhdx for {0}", new object[]
					{
						text
					});
					File.Copy(text2, destFileName);
				}
			}
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00007738 File Offset: 0x00005938
		internal static void CreateZipFile(string zipFolderTempPath, string zipFilePath)
		{
			try
			{
				string currentDirectory = Directory.GetCurrentDirectory();
				string prog = Path.Combine(currentDirectory, "7zr.exe");
				string args = string.Format(CultureInfo.InvariantCulture, "a \"{0}\" -m0=LZMA:a=2 \"{1}\\*\"", new object[]
				{
					zipFilePath,
					zipFolderTempPath
				});
				bool flag = File.Exists(zipFilePath);
				if (flag)
				{
					File.Delete(zipFilePath);
				}
				Utils.RunCmd(prog, args, null);
			}
			catch (Exception ex)
			{
				Logger.Error("Error in creating Zip " + ex.ToString());
			}
		}

		// Token: 0x06000064 RID: 100 RVA: 0x000077C4 File Offset: 0x000059C4
		internal static void SendSystemInfoStatsAsync()
		{
			Thread thread = new Thread(delegate()
			{
				Logger.Info("Sending system info stats");
				Dictionary<string, string> supportedGLModes = InstallerUtils.GetSupportedGLModes(InstallerProperties.InstallerExtractedDir);
				supportedGLModes.Add("glmode", Stats.GetURLSafeBase64String(InstallerProperties.GLMode.ToString()));
				supportedGLModes.Add("glrendermode", Stats.GetURLSafeBase64String(InstallerProperties.GLRenderMode.ToString()));
				supportedGLModes.Add("gl_vendor", Stats.GetURLSafeBase64String(InstallerProperties.GLVendor));
				supportedGLModes.Add("gl_renderer", Stats.GetURLSafeBase64String(InstallerProperties.GLRenderer));
				supportedGLModes.Add("gl_version", Stats.GetURLSafeBase64String(InstallerProperties.GLVersion));
				int num;
				int num2;
				Utils.GetDefaultGuestResolution(out num, out num2);
				supportedGLModes.Add("bstr", Stats.GetURLSafeBase64String(num.ToString() + "x" + num2.ToString()));
				supportedGLModes.Add("guid", Stats.GetURLSafeBase64String(InstallerProperties.GUID));
				supportedGLModes.Add("install_id", InstallerProperties.InstallId);
				supportedGLModes.Add("program_data_drive_type", SSDCheck.IsMediaTypeSSD(InstallerProperties.PDDir) ? "ssd" : "hdd");
				supportedGLModes.Add("is_touch_supported", InstallerUtils.IsTouchSupported().ToString());
				Stats.SendSystemInfoStats(supportedGLModes, InstallerProperties.InstallerExtractedDir, InstallerProperties.DefaultVmName, InstallerProperties.CloudHost, InstallerProperties.GUID, InstallerProperties.CurrentHypervisor);
			})
			{
				IsBackground = true
			};
			thread.Start();
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00007808 File Offset: 0x00005A08
		internal static void LogPFPDDirs()
		{
			List<string> listOfDirs = new List<string>
			{
				InstallerProperties.PFDir,
				InstallerProperties.PDDir,
				InstallerProperties.PFDirExisting,
				InstallerProperties.PDDirExisting,
				InstallerProperties.PFDirNew,
				InstallerProperties.PDDirNew
			};
			InstallerUtils.LogDirList(listOfDirs);
		}

		// Token: 0x06000066 RID: 102 RVA: 0x0000786C File Offset: 0x00005A6C
		internal static bool CheckIfTestRollback()
		{
			return (int)RegistryUtils.GetRegistryValue("Software\\BlueStacks_msi5", "TestRollback", 0, RegistryKeyKind.HKEY_CURRENT_USER) != 0;
		}

		// Token: 0x06000067 RID: 103 RVA: 0x000078A4 File Offset: 0x00005AA4
		internal static void DeleteOldBlueStacksData()
		{
			Logger.Info("Deleting old BlueStacks");
			try
			{
				Utils.DeleteDirectory(InstallerProperties.PDDirOld);
			}
			catch (Exception ex)
			{
				InstallerStats.InstallerComment |= InstallerComments.FAILED_TO_DELETE_OLD_DATA;
				Logger.Warning("Failed to delete old PD data, ignoring ex: " + ex.Message);
			}
			try
			{
				Utils.DeleteDirectory(InstallerProperties.PFDirOld);
			}
			catch (Exception ex2)
			{
				InstallerStats.InstallerComment |= InstallerComments.FAILED_TO_DELETE_OLD_DATA;
				Logger.Warning("Failed to delete old PF data, ignoring ex: " + ex2.Message);
			}
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00007950 File Offset: 0x00005B50
		internal static void StartPlayer()
		{
			try
			{
				Logger.Info("Booting Default instance");
				string pfdir = InstallerProperties.PFDir;
				string text = Path.Combine(pfdir, "HD-Player.exe");
				string text2 = "--instance " + InstallerProperties.VmToLaunch + " --warmup --hidden";
				Logger.Info("Starting Player for with args: {0}", new object[]
				{
					text2
				});
				bool launchBlueStacksElevated = Customizations.Instance.LaunchBlueStacksElevated;
				if (launchBlueStacksElevated)
				{
					Process.Start(text, text2);
				}
				else
				{
					ProcessUtils.ExecuteProcessUnelevated(text, text2, pfdir);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in starting Player. Err : " + ex.ToString());
			}
		}

		// Token: 0x06000069 RID: 105 RVA: 0x000079F8 File Offset: 0x00005BF8
		internal static bool RequestBlueStacksShutdownAndWait(Process proc, string vm, bool stopOnlyIfHidden)
		{
			Logger.Info("Requesting shutdown: {0} {1}", new object[]
			{
				vm,
				stopOnlyIfHidden
			});
			bool flag = BridgeApis.BridgeApiInitReturnCode == 0;
			if (flag)
			{
				bool flag2 = string.IsNullOrEmpty(vm);
				if (flag2)
				{
					BridgeApis.RequestShutdown();
				}
				else
				{
					BridgeApis.RequestPlrShutdown(vm, stopOnlyIfHidden);
				}
				for (int i = 0; i < 25; i++)
				{
					Thread.Sleep(1000);
					bool hasExited = proc.HasExited;
					if (hasExited)
					{
						Logger.Info("Process exited");
						return true;
					}
				}
			}
			else
			{
				Logger.Warning("Bridge not inited, could not request shutdown. Bridge return code: {0}", new object[]
				{
					BridgeApis.BridgeApiInitReturnCode
				});
			}
			return proc.HasExited;
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00007ABC File Offset: 0x00005CBC
		internal static void StopBlueStacks(out bool isServiceStopped)
		{
			isServiceStopped = true;
			try
			{
				Process process = null;
				Process[] processesByName = Process.GetProcessesByName("HD-Player");
				try
				{
					foreach (Process process2 in processesByName)
					{
						bool flag = process2.MainModule.FileName.StartsWith(RegistryStrings.InstallDir, StringComparison.InvariantCultureIgnoreCase);
						if (flag)
						{
							Logger.Info("Found running HD-Player.exe");
							isServiceStopped = false;
							process = process2;
							break;
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Error("Could not find running HD-Player. Ex: {0}", new object[]
					{
						ex
					});
				}
				bool flag2 = process != null;
				if (flag2)
				{
					InstallerUtils.RequestBlueStacksShutdownAndWait(process, "", false);
					Logger.Info("Killing player processes");
					foreach (Process process3 in Process.GetProcessesByName("HD-Player"))
					{
						try
						{
							bool flag3 = process3.MainModule.FileName.StartsWith(RegistryStrings.InstallDir, StringComparison.InvariantCultureIgnoreCase);
							if (flag3)
							{
								process3.Kill();
								bool flag4 = !process3.WaitForExit(5000);
								if (flag4)
								{
									Logger.Warning("Timeout waiting for process {0} to die", new object[]
									{
										process3.ProcessName
									});
								}
								else
								{
									Logger.Info(string.Format("Process {0} killed", process3.Id));
								}
							}
						}
						catch (Exception ex2)
						{
							Logger.Error(string.Format("Failed to kill process {0}. Ex : {1}", process3.Id, ex2.Message));
						}
					}
				}
				string installDir = (string)RegistryUtils.GetRegistryValue(string.Format(CultureInfo.InvariantCulture, "Software\\BlueStacks{0}", new object[]
				{
					"_msi5"
				}), "InstallDir", string.Empty, RegistryKeyKind.HKEY_LOCAL_MACHINE);
				Utils.KillBlueStacksProcesses(false, installDir, false);
				Utils.KillProcessByName("BstkSVC", installDir);
				ServiceManager.StopDriver(15);
				isServiceStopped = InstallerUtils.IsDriverStopped();
			}
			catch (Exception ex3)
			{
				Logger.Error("Could not stop BlueStacks. Ex: {0}", new object[]
				{
					ex3
				});
			}
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00007D04 File Offset: 0x00005F04
		private static bool IsDriverStopped()
		{
			try
			{
				Logger.Info("Checking if {0} is stopped", new object[]
				{
					Strings.BlueStacksDriverName
				});
				using (ServiceController serviceController = new ServiceController(Strings.BlueStacksDriverName))
				{
					return serviceController.Status == ServiceControllerStatus.Stopped;
				}
			}
			catch (Exception ex)
			{
				Logger.Error("An error occured while checking if svc is stopped, ex: {0}", new object[]
				{
					ex
				});
			}
			return true;
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00007D8C File Offset: 0x00005F8C
		internal static void BootPlayerAfterInstall()
		{
			try
			{
				InstallerStats.Enqueue("install_sysprep_started", false, "", "");
				Thread thread = new Thread(delegate()
				{
					InstallerUtils.StartPlayer();
					Thread.Sleep(2000);
					Process[] processesByName = Process.GetProcessesByName("HD-Player");
					foreach (Process process in processesByName)
					{
						try
						{
							bool flag = process.MainModule.FileName.StartsWith(RegistryStrings.InstallDir);
							if (flag)
							{
								IntPtr handle = process.Handle;
								bool flag2 = process.WaitForExit(240000);
								bool flag3 = flag2;
								if (flag3)
								{
									try
									{
										uint num;
										bool exitCodeProcess = InstallerUtils.GetExitCodeProcess(handle, out num);
										if (exitCodeProcess)
										{
											Logger.Info(string.Format("BootPlayerAfterInstall. Player exit code {0}", num));
											InstallerStats.Enqueue("install_silentboot_exit_code", false, num.ToString(), "");
											bool flag4 = num == 0U || num == 114U;
											if (flag4)
											{
												Logger.Info("Default instance booting completed");
												InstallerStats.Enqueue("install_silentboot_completed", false, "", "");
											}
											else
											{
												InstallerStats.Enqueue("install_silentboot_failed", false, num.ToString(), "");
											}
										}
										else
										{
											InstallerStats.Enqueue("install_silentboot_failed", false, "GetExitCode failed", "");
											Logger.Error("Error in BootPlayerAfterInstall, GetExitCodeProcess return false");
										}
									}
									catch (Exception ex2)
									{
										InstallerStats.Enqueue("install_silentboot_failed", false, ex2.Message, "");
										Logger.Error("Error in BootPlayerAfterInstall, unable to get exit code: " + ex2.Message);
									}
								}
								else
								{
									Logger.Warning("Player timed out");
									InstallerStats.Enqueue("install_silentboot_timeout", false, "", "");
									bool flag5 = InstallerUtils.RequestBlueStacksShutdownAndWait(process, InstallerProperties.VmToLaunch, true);
									bool flag6 = !flag5;
									if (flag6)
									{
										try
										{
											InstallerStats.Enqueue("install_silentboot_timeout_shutdown_failed", false, "", "");
											Logger.Warning("Killing HD-Player process since it did not exit after shutdown");
											process.Kill();
											bool flag7 = !process.WaitForExit(2000);
											if (flag7)
											{
												Logger.Error("Timeout waiting for process {0} to die", new object[]
												{
													process.ProcessName
												});
											}
										}
										catch (Exception ex3)
										{
											Logger.Error("Exception in killing process. Ex: " + ex3.Message);
										}
									}
								}
							}
						}
						catch (Exception ex4)
						{
							Logger.Error("Failed to perform warmup boot. Ex: {0}", new object[]
							{
								ex4
							});
						}
					}
				});
				thread.Start();
				thread.Join();
			}
			catch (Exception ex)
			{
				Logger.Info("Default instance booting failed");
				Logger.Error(ex.Message);
			}
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00007E14 File Offset: 0x00006014
		internal static int CreateAllVmBstkFileFromTemplate()
		{
			foreach (string vm in InstallerProperties.VmList)
			{
				bool flag = !InstallerUtils.InstallVmConfig(vm);
				if (flag)
				{
					return -42;
				}
			}
			return 0;
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00007E7C File Offset: 0x0000607C
		internal static void SetupForcedDedicatedGPUConfiguration()
		{
			bool flag = true;
			int num = -99;
			bool isUpgrade = InstallerProperties.IsUpgrade;
			if (isUpgrade)
			{
				flag = ConfReader.PreferDedicatedGpu;
			}
			bool flag2 = flag;
			if (flag2)
			{
				bool flag3 = ForceDedicatedGPU.ToggleDedicatedGPU(true, Path.Combine(InstallerProperties.InstallerExtractedDir, "HD-ForceGPU.exe"), InstallerProperties.PFDir, out num);
				if (flag3)
				{
					InstallerProperties.PreferDedicatedGPU = true;
				}
				else
				{
					InstallerProperties.PreferDedicatedGPU = false;
				}
			}
			InstallerStats.Enqueue("install_dedicated_gpu_set", false, num.ToString(), "");
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00007EF4 File Offset: 0x000060F4
		internal static void StartComReregistration()
		{
			try
			{
				ComRegistration comRegistration = new ComRegistration(InstallerProperties.PFDir);
				comRegistration.Unregister();
				comRegistration.Register();
			}
			catch (Exception ex)
			{
				Logger.Warning("Failed to execute com registration. Err: {0}", new object[]
				{
					ex.Message
				});
				InstallerStats.InstallerComment |= InstallerComments.FAILED_TO_REGISTER_COM;
			}
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00007F5C File Offset: 0x0000615C
		internal static int RenameDirectoriesToFinalStructure()
		{
			Logger.Info("In RenameRegistryAndDirectoriesToFinalStructure");
			try
			{
				Logger.Info("Renaming existing dirs to .old");
				bool flag = !InstallerUtils.MoveDirectory(InstallerProperties.PFDirExisting, InstallerProperties.PFDirOld);
				if (flag)
				{
					return -49;
				}
				bool flag2 = !InstallerUtils.MoveDirectory(InstallerProperties.PDDirExisting, InstallerProperties.PDDirOld);
				if (flag2)
				{
					return -49;
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to move old items, Err: " + ex.ToString());
				return -49;
			}
			try
			{
				Logger.Info("Rename .new to actual name folders and registries");
				bool flag3 = !InstallerUtils.MoveDirectory(InstallerProperties.PFDirNew, InstallerProperties.PFDir);
				if (flag3)
				{
					return -51;
				}
				bool flag4 = !InstallerUtils.MoveDirectory(InstallerProperties.PDDirNew, InstallerProperties.PDDir);
				if (flag4)
				{
					return -51;
				}
			}
			catch (Exception ex2)
			{
				Logger.Error("Failed to rename new items, Err: " + ex2.ToString());
				return -51;
			}
			return 0;
		}

		// Token: 0x06000071 RID: 113 RVA: 0x0000806C File Offset: 0x0000626C
		internal static bool CheckIfTestRollbackFail()
		{
			return (int)RegistryUtils.GetRegistryValue("Software\\BlueStacks_msi5", "TestRollbackFail", 0, RegistryKeyKind.HKEY_CURRENT_USER) != 0;
		}

		// Token: 0x06000072 RID: 114 RVA: 0x000080A4 File Offset: 0x000062A4
		internal static void LogParentHash()
		{
			try
			{
				Logger.Info("Top parent filepath: {0}", new object[]
				{
					InstallerProperties.MainInstallerFilePath
				});
				_MD5 md = new _MD5
				{
					ValueAsFile = InstallerProperties.MainInstallerFilePath
				};
				Logger.Info("Computed MD5: " + md.FingerPrint);
			}
			catch (Exception ex)
			{
				Logger.Warning("Couldn't compute MD5, ex: {0}", new object[]
				{
					ex.Message
				});
			}
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00008128 File Offset: 0x00006328
		internal static void SendOSStatAsync()
		{
			try
			{
				Logger.Info("Sending OS stat");
				InstallerStats.Enqueue("install_os_details", true, Profile.Os, "");
			}
			catch (Exception ex)
			{
				Logger.Warning("Error in sending OSStat. Ex: {0}", new object[]
				{
					ex.Message
				});
			}
		}

		// Token: 0x06000074 RID: 116 RVA: 0x0000818C File Offset: 0x0000638C
		internal static void ExtractVCDependencies(string dest, MainWindow window = null)
		{
			string text = Path.Combine(InstallerProperties.InstallerExtractedDir, "Dependencies.7z");
			try
			{
				Logger.Info("Extracting MSVC Dependencies");
				InstallerUtils.ExtractZip(text, dest, window);
				InstallerProperties.VCDependenciesExtracted = true;
			}
			catch (Exception ex)
			{
				Logger.Error("Could not extract VCDependencies from: {0}, Ex: {1}", new object[]
				{
					text,
					ex
				});
			}
		}

		// Token: 0x06000075 RID: 117 RVA: 0x000081F8 File Offset: 0x000063F8
		internal static void InstallVC()
		{
			string str = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Bluestacks"), string.Format("VCInstaller_{0}.log", "5.12.120.6303"));
			string text3;
			try
			{
				Logger.Info("Attempting to install VC++");
				string text = "vcredist_msvc2019_x64.exe";
				bool flag = !SystemUtils.IsOs64Bit();
				if (flag)
				{
					text = "vcredist_msvc2019_x86.exe";
				}
				string text2 = Path.Combine(InstallerProperties.InstallerExtractedDir, text);
				bool flag2 = File.Exists(text2);
				if (flag2)
				{
					Process process = new Process();
					process.StartInfo.FileName = text;
					process.StartInfo.WorkingDirectory = InstallerProperties.InstallerExtractedDir;
					process.StartInfo.Arguments = "/install /quiet /norestart /log \"" + str + "\"";
					process.StartInfo.UseShellExecute = false;
					process.StartInfo.CreateNoWindow = true;
					process.Start();
					process.WaitForExit();
					Logger.Info("{0} exited with exit code: {1}", new object[]
					{
						text,
						process.ExitCode
					});
					text3 = process.ExitCode.ToString();
				}
				else
				{
					Logger.Error("VC setup not found at {0}!", new object[]
					{
						text2
					});
					text3 = "file_not_found";
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Could not install VC++. Ex: {0}", new object[]
				{
					ex
				});
				text3 = ex.Message;
			}
			Logger.Info("VC exit status: {0}", new object[]
			{
				text3
			});
		}

		// Token: 0x06000076 RID: 118 RVA: 0x0000838C File Offset: 0x0000658C
		internal static int BridgeApiInit(int retry)
		{
			int num = 0;
			try
			{
				Logger.Info("Initing BridgeApi, try: {0}", new object[]
				{
					retry
				});
				num = BridgeApis.ApiInitEx("Installer", InstallerProperties.IsFreshInstall, false, InstallerProperties.InstallerLogFilePath);
				bool flag = num != 0;
				if (flag)
				{
					Logger.Warning("BridgeApiInitEx failed. ret: {0}", new object[]
					{
						num
					});
				}
			}
			catch (Exception ex)
			{
				num = int.MaxValue;
				InstallerStats.Enqueue("install_bridge_init_failed", false, ex.Message, retry.ToString());
				Logger.Error("BridgeApiInitEx failed. Ex: {0}", new object[]
				{
					ex
				});
			}
			return num;
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00008444 File Offset: 0x00006644
		internal static void InstallUpdateHelper()
		{
			try
			{
				string text = Path.Combine(InstallerProperties.PFDir, "BlueStacksHelper.exe");
				bool flag = File.Exists(text);
				if (flag)
				{
					Logger.Info("Running helper binary");
					string args = "-Installer";
					ProcessUtils.ExecuteProcessUnelevated(text, args, "");
				}
				else
				{
					Logger.Warning("Helper binary not found in " + text);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in starting BlueStacksHelper. Err: {0}", new object[]
				{
					ex.Message
				});
			}
		}

		// Token: 0x06000078 RID: 120 RVA: 0x000084D4 File Offset: 0x000066D4
		internal static int ExtractAndroidImage(MainWindow window)
		{
			Logger.Info("Extracting android images: {0}", new object[]
			{
				string.Join(", ", InstallerProperties.ImageList.ToArray())
			});
			foreach (string text in InstallerProperties.ImageList)
			{
				Logger.Info("Image: {0}", new object[]
				{
					text
				});
				string path = string.Format("{0}_{1}.exe", text, "5.12.120.6303");
				string text2 = MicroInstallerSharedUtils.GetGuestZipPath(text, InstallerProperties.PDDir);
				bool flag = !File.Exists(text2);
				if (flag)
				{
					Logger.Warning("Image not found at {0}", new object[]
					{
						text2
					});
					text2 = Path.Combine(InstallerProperties.MainInstallerFolderPath, path);
					bool flag2 = !File.Exists(text2);
					if (flag2)
					{
						Logger.Error("Image not found at {0}", new object[]
						{
							text2
						});
						return -60;
					}
				}
				string extractDirectory = Path.Combine(InstallerProperties.PDDirNew, Path.Combine("Engine", text));
				CmdRes cmdRes = InstallerUtils.ExtractZip(text2, extractDirectory, window);
				bool flag3 = cmdRes.ExitCode != 0;
				if (flag3)
				{
					Logger.Info("MD5 of {0} is {1}", new object[]
					{
						text,
						Utils.GetMD5HashFromFile(text2)
					});
					IOUtils.DeleteOrPendMoveFile(text2);
					return -60;
				}
				Logger.Info("Deleting: {0}", new object[]
				{
					text2
				});
				IOUtils.DeleteOrPendMoveFile(text2);
			}
			return 0;
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00008674 File Offset: 0x00006874
		internal static void AttemptToClearSomeSpaceIfShort()
		{
			try
			{
				long availableDiskSpaceOfDrive = IOUtils.GetAvailableDiskSpaceOfDrive(Path.GetTempPath());
				bool flag = availableDiskSpaceOfDrive > 31457280L;
				if (flag)
				{
					Logger.Info("Disk space available > 30MB: {0} KB", new object[]
					{
						availableDiskSpaceOfDrive / 1024L
					});
				}
				else
				{
					string[] directories = Directory.GetDirectories(InstallerProperties.MainInstallerFolderPath);
					List<string> list = new List<string>();
					list.AddRange(directories);
					list.Add(string.Format("{0}.new", InstallerProperties.PFDir));
					list.Add(string.Format("{0}.new", InstallerProperties.PDDir));
					list.Add(string.Format("{0}.old", InstallerProperties.PFDir));
					list.Add(string.Format("{0}.old", InstallerProperties.PDDir));
					Utils.DeleteDirectory(list, false);
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Couldn't free up some space. Ex: {0}", new object[]
				{
					ex.Message
				});
			}
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00008770 File Offset: 0x00006970
		public static void DeleteOldShortcuts()
		{
			try
			{
				Logger.Info("Deleting old shortcuts");
				ShortcutUtils.DeleteDesktopShortcut("MSI App Player Engine.lnk.lnk");
				ShortcutUtils.DeleteCommonDesktopShortcut("MSI App Player Engine.lnk.lnk");
				ShortcutUtils.DeleteStartMenuShortcut("MSI App Player Engine.lnk.lnk");
				ShortcutUtils.DeleteCommonStartMenuShortcut("MSI App Player Engine.lnk.lnk");
				ShortcutUtils.DeleteDesktopShortcut("MSI App Player Multi-Instance Manager.lnk");
				ShortcutUtils.DeleteCommonDesktopShortcut("MSI App Player Multi-Instance Manager.lnk");
				ShortcutUtils.DeleteStartMenuShortcut("MSI App Player Multi-Instance Manager.lnk");
				ShortcutUtils.DeleteCommonStartMenuShortcut("MSI App Player Multi-Instance Manager.lnk");
				ShortcutUtils.DeleteDesktopShortcut("MSI App Player Multi-Instance Manager.lnk");
				ShortcutUtils.DeleteCommonDesktopShortcut("MSI App Player Multi-Instance Manager.lnk");
				ShortcutUtils.DeleteStartMenuShortcut("MSI App Player Multi-Instance Manager.lnk");
				ShortcutUtils.DeleteCommonStartMenuShortcut("MSI App Player Multi-Instance Manager.lnk");
			}
			catch (Exception ex)
			{
				Logger.Warning("Failed to delete old shortcuts. Err: " + ex.ToString());
			}
		}

		// Token: 0x0600007B RID: 123 RVA: 0x0000883C File Offset: 0x00006A3C
		public static bool MoveDirectory(string srcDir, string dstDir)
		{
			Logger.Info("Moving directory {0} to {1}", new object[]
			{
				srcDir,
				dstDir
			});
			try
			{
				bool flag = Directory.Exists(dstDir);
				if (flag)
				{
					Directory.Delete(dstDir, true);
				}
				Directory.Move(srcDir, dstDir);
			}
			catch (Exception ex)
			{
				Logger.Info("------------ FOR DEV TRACKING--------------- Moving failed");
				Logger.Info("Caught exception when moving directory {0} to {1} err :{2}", new object[]
				{
					srcDir,
					dstDir,
					ex.ToString()
				});
				bool flag2 = !Directory.Exists(dstDir);
				if (flag2)
				{
					Directory.CreateDirectory(dstDir);
				}
				string[] files = Directory.GetFiles(srcDir);
				foreach (string text in files)
				{
					FileInfo fileInfo = new FileInfo(text);
					string text2 = Path.Combine(dstDir, fileInfo.Name);
					try
					{
						bool flag3 = File.Exists(text2);
						if (flag3)
						{
							File.SetAttributes(text, FileAttributes.Normal);
							File.Delete(text2);
						}
						File.Move(text, text2);
					}
					catch (Exception ex2)
					{
						Logger.Info("Exception in file move {0} to {1}. Copying instead.. ex:{2}", new object[]
						{
							text,
							text2,
							ex2.ToString()
						});
						try
						{
							File.Copy(text, text2, true);
						}
						catch (Exception ex3)
						{
							Logger.Error("Exception in file copy: THIS WILL RESULT IN DEPLOYMENT FAILURE" + ex3.ToString());
							return false;
						}
					}
				}
				string[] directories = Directory.GetDirectories(srcDir);
				foreach (string path in directories)
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(path);
					string text3 = Path.Combine(dstDir, directoryInfo.Name);
					string text4 = Path.Combine(srcDir, directoryInfo.Name);
					bool flag4 = !InstallerUtils.MoveDirectory(text4, text3);
					if (flag4)
					{
						Logger.Warning("Returing false in directory move for {0} to {1}", new object[]
						{
							text4,
							text3
						});
						return false;
					}
				}
				try
				{
					Directory.Delete(srcDir);
				}
				catch (Exception ex4)
				{
					Logger.Info("Ignoring exception when trying to delete srcDir at the end err:{0}", new object[]
					{
						ex4.ToString()
					});
				}
			}
			return true;
		}

		// Token: 0x0600007C RID: 124
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

		// Token: 0x0600007D RID: 125 RVA: 0x00008ABC File Offset: 0x00006CBC
		public static void ModifyDirectoryPermissionsForEveryone(string dir)
		{
			bool flag = !Directory.Exists(dir);
			if (flag)
			{
				Directory.CreateDirectory(dir);
			}
			try
			{
				string identity = new SecurityIdentifier("S-1-1-0").Translate(typeof(NTAccount)).ToString();
				DirectoryInfo directoryInfo = new DirectoryInfo(dir);
				DirectorySecurity accessControl = directoryInfo.GetAccessControl();
				accessControl.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
				directoryInfo.SetAccessControl(accessControl);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to set permissions. err: " + ex.ToString());
			}
			try
			{
				bool flag2 = !SystemUtils.IsOSWinXP();
				if (flag2)
				{
					string[] files = Directory.GetFiles(dir);
					foreach (string fileName in files)
					{
						string identity2 = new SecurityIdentifier("S-1-1-0").Translate(typeof(NTAccount)).ToString();
						FileInfo fileInfo = new FileInfo(fileName);
						FileSecurity accessControl2 = fileInfo.GetAccessControl();
						accessControl2.AddAccessRule(new FileSystemAccessRule(identity2, FileSystemRights.FullControl, AccessControlType.Allow));
						fileInfo.SetAccessControl(accessControl2);
					}
				}
				string[] directories = Directory.GetDirectories(dir);
				foreach (string dir2 in directories)
				{
					InstallerUtils.ModifyDirectoryPermissionsForEveryone(dir2);
				}
			}
			catch (Exception ex2)
			{
				Logger.Error("Failed to set permissions. err: " + ex2.ToString());
			}
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00008C48 File Offset: 0x00006E48
		public static void CreateDesktopAndStartMenuShortcuts(string shortcutName, string iconPath, string targetBinPath, string args = "", string description = "", string package = "")
		{
			try
			{
				bool flag = string.IsNullOrEmpty(shortcutName);
				if (!flag)
				{
					bool flag2 = string.IsNullOrEmpty(description);
					if (flag2)
					{
						description = shortcutName;
					}
					ShortcutUtils.CreateCommonDesktopShortcut(shortcutName, iconPath, targetBinPath, args, description, package);
					ShortcutUtils.CreateCommonStartMenuShortcut(shortcutName, iconPath, targetBinPath, args, shortcutName, package);
					bool flag3 = !File.Exists(Path.Combine(ShortcutHelper.CommonDesktopPath, shortcutName));
					if (flag3)
					{
						Logger.Warning("Failed to make common desktop shortcut, attempting user desktop");
						ShortcutUtils.CreateDesktopShortcut(shortcutName, iconPath, targetBinPath, args, description, package);
						ShortcutUtils.CreateStartMenuShortcut(shortcutName, iconPath, targetBinPath, args, description, package);
					}
					Utils.SHChangeNotify(134217728, 4096, IntPtr.Zero, IntPtr.Zero);
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Failed to create BlueStacks shortcuts. ex: {0}", new object[]
				{
					ex
				});
			}
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00008D1C File Offset: 0x00006F1C
		public static bool RecreateAddRemoveRegistry(string pfDir, string iconPath, string displayName, string publisher)
		{
			try
			{
				string subkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\BlueStacks_msi5";
				using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(subkey))
				{
					registryKey.SetValue("DisplayName", displayName);
					registryKey.SetValue("DisplayVersion", "5.12.120.6303");
					registryKey.SetValue("DisplayIcon", iconPath);
					registryKey.SetValue("EstimatedSize", 2096128, RegistryValueKind.DWord);
					registryKey.SetValue("InstallDate", string.Format(CultureInfo.InvariantCulture, "{0:yyyyMMdd}", new object[]
					{
						DateTime.Now
					}));
					registryKey.SetValue("NoModify", "1", RegistryValueKind.DWord);
					registryKey.SetValue("NoRepair", "1", RegistryValueKind.DWord);
					registryKey.SetValue("Publisher", publisher);
					registryKey.SetValue("UninstallString", Path.Combine(pfDir, "BlueStacksUninstaller.exe -tmp"));
				}
				return true;
			}
			catch (Exception ex)
			{
				Logger.Info("Error in creating ControlPanel registry: {0}", new object[]
				{
					ex.ToString()
				});
			}
			return false;
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00008E48 File Offset: 0x00007048
		public static void CopyLogFileToLogDir(string logFilePath, string destFileName)
		{
			try
			{
				string destFileName2 = Path.Combine(RegistryManager.Instance.LogDir, destFileName);
				File.Copy(logFilePath, destFileName2, true);
			}
			catch (Exception ex)
			{
				Logger.Error("Got exception when copying isntaller logs in log dir ex :{0}", new object[]
				{
					ex.ToString()
				});
			}
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00008EA4 File Offset: 0x000070A4
		public static void CreateAppUserModelIdRegistry(string logoPath)
		{
			try
			{
				using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(string.Format("SOFTWARE\\Classes\\AppUserModelId\\BlueStacks{0}", "msi5".Equals("bgp", StringComparison.InvariantCultureIgnoreCase) ? "" : "_msi5")))
				{
					registryKey.SetValue("DisplayName", "MSI App Player Engine", RegistryValueKind.ExpandString);
					registryKey.SetValue("IconUri", logoPath, RegistryValueKind.ExpandString);
					registryKey.SetValue("IconBackgroundColor", "00000000", RegistryValueKind.String);
					registryKey.SetValue("CustomActivator", "{3DA0A2FE-AA89-4D56-BB84-3FEBBD0B8D99}", RegistryValueKind.String);
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Failed to create AppUserModelId registry. Err: " + ex.ToString());
			}
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00008F74 File Offset: 0x00007174
		public static void CreateWebUrlScheme(string logoPath)
		{
			try
			{
				string text = "BlueStacksGP_msi5";
				using (RegistryKey registryKey = Registry.ClassesRoot.CreateSubKey(text))
				{
					registryKey.SetValue("", "BlueStacks Web Url Scheme");
					registryKey.SetValue("URL Protocol", "");
				}
				using (RegistryKey registryKey2 = Registry.ClassesRoot.CreateSubKey(text + "\\DefaultIcon"))
				{
					registryKey2.SetValue("", logoPath);
				}
				using (RegistryKey registryKey3 = Registry.ClassesRoot.CreateSubKey(text + "\\Shell"))
				{
					registryKey3.SetValue("", "open");
				}
				using (RegistryKey registryKey4 = Registry.ClassesRoot.CreateSubKey(text + "\\Shell\\open"))
				{
					registryKey4.SetValue("", "Open BlueStacks Game Platform");
				}
				using (RegistryKey registryKey5 = Registry.ClassesRoot.CreateSubKey(text + "\\Shell\\open\\command"))
				{
					string text2 = Path.Combine(RegistryManager.Instance.InstallDir, "HD-Player.exe");
					string value = string.Format(CultureInfo.InvariantCulture, "\"{0}\" %1", new object[]
					{
						text2
					});
					registryKey5.SetValue("", value);
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Failed to create web url scheme. Err: " + ex.ToString());
			}
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00009188 File Offset: 0x00007388
		internal static int CreateAndUpdateConf(string confPath)
		{
			File.Create(confPath).Dispose();
			return ConfManager.CreateProperties(confPath);
		}

		// Token: 0x06000084 RID: 132 RVA: 0x000091AC File Offset: 0x000073AC
		public static void CreateWebUrlSchemeForLogCollector(string logoPath)
		{
			Logger.Info("Creating web url scheme for log collector");
			try
			{
				string text = "BstLogcollector_msi5";
				using (RegistryKey registryKey = Registry.ClassesRoot.CreateSubKey(text))
				{
					registryKey.SetValue("", "BlueStacks Web Url Scheme");
					registryKey.SetValue("URL Protocol", "");
				}
				using (RegistryKey registryKey2 = Registry.ClassesRoot.CreateSubKey(text + "\\DefaultIcon"))
				{
					registryKey2.SetValue("", logoPath);
				}
				using (RegistryKey registryKey3 = Registry.ClassesRoot.CreateSubKey(text + "\\Shell"))
				{
					registryKey3.SetValue("", "open");
				}
				using (RegistryKey registryKey4 = Registry.ClassesRoot.CreateSubKey(text + "\\Shell\\open"))
				{
					registryKey4.SetValue("", "Open BlueStacks Log Collector");
				}
				using (RegistryKey registryKey5 = Registry.ClassesRoot.CreateSubKey(text + "\\Shell\\open\\command"))
				{
					string text2 = Path.Combine(RegistryManager.Instance.InstallDir, "HD-LogCollector.exe");
					string value = string.Format(CultureInfo.InvariantCulture, "\"{0}\" %1", new object[]
					{
						text2
					});
					registryKey5.SetValue("", value);
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Failed to create web url scheme for log collector Err: " + ex.ToString());
			}
		}

		// Token: 0x06000085 RID: 133 RVA: 0x000093C8 File Offset: 0x000075C8
		private static void CreateBlueStacksApkShellRegistry(string baseKeyName, string extn, string installDir, string vmName)
		{
			try
			{
				using (RegistryKey registryKey = Registry.ClassesRoot.CreateSubKey(baseKeyName))
				{
					registryKey.SetValue("", string.Format(CultureInfo.InvariantCulture, "{0} Android Package File", new object[]
					{
						"MSI App Player Engine"
					}));
				}
				using (RegistryKey registryKey2 = Registry.ClassesRoot.CreateSubKey(baseKeyName + "\\DefaultIcon"))
				{
					registryKey2.SetValue("", RegistryStrings.ProductIconCompletePath);
				}
				using (RegistryKey registryKey3 = Registry.ClassesRoot.CreateSubKey(baseKeyName + "\\Shell"))
				{
					registryKey3.SetValue("", "open");
				}
				using (RegistryKey registryKey4 = Registry.ClassesRoot.CreateSubKey(baseKeyName + "\\Shell\\open"))
				{
					registryKey4.SetValue("", string.Format(CultureInfo.InvariantCulture, "Open with {0}", new object[]
					{
						"MSI App Player Engine"
					}));
				}
				using (RegistryKey registryKey5 = Registry.ClassesRoot.CreateSubKey(baseKeyName + "\\Shell\\open\\command"))
				{
					string text = Path.Combine(installDir, "HD-Player.exe");
					string text2 = string.Format(CultureInfo.InvariantCulture, "--instance {0} --cmd installApk --filepath \"%1\"", new object[]
					{
						vmName
					});
					string value = string.Format(CultureInfo.InvariantCulture, "{0} {1}", new object[]
					{
						text,
						text2
					});
					registryKey5.SetValue("", value);
					InstallerUtils.CreateBlueStacksApkMapping(extn, baseKeyName);
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Some error while setting APKHandler extention association, ex: " + ex.Message);
			}
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00009618 File Offset: 0x00007818
		private static bool ConvertVhd(string filePath)
		{
			string cmd = Path.Combine(InstallerProperties.PFDir, "BstkVMMgr.exe");
			string args = string.Concat(new string[]
			{
				"clonemedium ",
				filePath,
				".vdi ",
				filePath,
				".vhd --format VHD"
			});
			bool flag = RunCommand.RunCmd(cmd, args, true, false, 0).ExitCode == 0;
			bool result;
			if (flag)
			{
				File.Delete(filePath + ".vdi");
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00009694 File Offset: 0x00007894
		internal static bool ConvertDisksForMicrosoftHyperV()
		{
			try
			{
				Logger.Info("Converting disks for Microsoft HyperV");
				foreach (string text in InstallerProperties.ImageList)
				{
					Logger.Info("Converting for image: {0}", new object[]
					{
						text
					});
					string path = Path.Combine(InstallerProperties.PDDirNew, "Engine\\" + text);
					string[] array = new string[]
					{
						Path.Combine(path, "Data"),
						Path.Combine(path, "Root")
					};
					foreach (string filePath in array)
					{
						bool flag = !InstallerUtils.ConvertVhd(filePath);
						if (flag)
						{
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Some error while converting disks for hyperv, ex: {0}", new object[]
				{
					ex
				});
				return false;
			}
			return true;
		}

		// Token: 0x06000088 RID: 136 RVA: 0x000097B0 File Offset: 0x000079B0
		private static void CreateBlueStacksApkMapping(string extn, string keyName)
		{
			try
			{
				using (RegistryKey registryKey = Registry.ClassesRoot.CreateSubKey(extn))
				{
					registryKey.SetValue("", keyName, RegistryValueKind.String);
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Some error while setting main {0} extention association for {1}, ex: {2}", new object[]
				{
					extn,
					keyName,
					ex.Message
				});
			}
		}

		// Token: 0x06000089 RID: 137 RVA: 0x000021EF File Offset: 0x000003EF
		public static void CreateInstallApkScheme(string installDir, string vmName)
		{
			Logger.Info("CreateInstallApkScheme start");
			InstallerUtils.CreateBlueStacksApkShellRegistry("BlueStacks.Apk", ".apk", installDir, vmName);
			InstallerUtils.CreateBlueStacksApkShellRegistry("BlueStacks.Xapk", ".xapk", installDir, vmName);
			Logger.Info("CreateInstallApkScheme end");
		}

		// Token: 0x0600008A RID: 138 RVA: 0x0000982C File Offset: 0x00007A2C
		private static void LogServices(ServiceController[] services)
		{
			foreach (ServiceController serviceController in services)
			{
				Logger.Info("ServiceName: {0},\tDisplayName: {1}, status: {2}", new object[]
				{
					serviceController.ServiceName,
					serviceController.DisplayName,
					serviceController.Status
				});
			}
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00009884 File Offset: 0x00007A84
		public static void LogAllServiceNames()
		{
			try
			{
				Logger.Info("Kernel services:-");
				InstallerUtils.LogServices(ServiceController.GetDevices());
				Logger.Info("User services:-");
				InstallerUtils.LogServices(ServiceController.GetServices());
			}
			catch (Exception ex)
			{
				Logger.Warning("Some error occured while logging services, ex: " + ex.Message);
			}
		}

		// Token: 0x0600008C RID: 140 RVA: 0x0000222C File Offset: 0x0000042C
		public static void RemoveAndAddFirewallRule(string ruleName, string binPath)
		{
			SystemUtils.RemoveFirewallRule(ruleName);
			InstallerUtils.AddFirewallRule(ruleName, binPath);
		}

		// Token: 0x0600008D RID: 141 RVA: 0x000098F0 File Offset: 0x00007AF0
		public static void AddFirewallRule(string ruleName, string binPath)
		{
			Logger.Info("Adding firewall rule fo bin {0}", new object[]
			{
				binPath
			});
			try
			{
				string cmd = "netsh.exe";
				string args = string.Format(CultureInfo.InvariantCulture, "advfirewall firewall add rule name=\"{0}\" dir=in action=allow program=\"{1}\" enable=yes", new object[]
				{
					ruleName,
					binPath
				});
				RunCommand.RunCmd(cmd, args, false, false, 0);
			}
			catch (Exception ex)
			{
				Logger.Error("Error in adding firewall rule", new object[]
				{
					ex.ToString()
				});
			}
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00009978 File Offset: 0x00007B78
		public static bool CreateHardLinkForFile(string existingFilePath, string newFilePath)
		{
			try
			{
				Logger.Info("Creating link from " + existingFilePath + " to " + newFilePath);
				bool flag = InstallerUtils.CreateHardLink(newFilePath, existingFilePath, IntPtr.Zero);
				bool flag2 = !flag;
				if (flag2)
				{
					Logger.Error("Failed to create hard link: " + Marshal.GetLastWin32Error().ToString());
					bool flag3 = File.Exists(existingFilePath);
					if (flag3)
					{
						Logger.Error("Copying the file instead");
						File.Copy(existingFilePath, newFilePath, true);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Logger.Error("Failed in creating hard link, Ex: " + ex.ToString());
			}
			return false;
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00009A2C File Offset: 0x00007C2C
		public static bool CreateHardLinkForDirectory(string existingDirPath, string newDirPath)
		{
			try
			{
				Logger.Info("Creating link from " + existingDirPath + " to " + newDirPath);
				bool flag = !Directory.Exists(newDirPath);
				if (flag)
				{
					Directory.CreateDirectory(newDirPath);
				}
				DirectoryInfo directoryInfo = new DirectoryInfo(existingDirPath);
				foreach (FileInfo fileInfo in directoryInfo.GetFiles())
				{
					string fullName = fileInfo.FullName;
					string newFilePath = Path.Combine(newDirPath, fileInfo.Name);
					bool flag2 = InstallerUtils.CreateHardLinkForFile(fullName, newFilePath);
					bool flag3 = !flag2;
					if (flag3)
					{
						Logger.Error("Failed to create hard link for file : " + fileInfo.FullName);
						return false;
					}
				}
				DirectoryInfo directoryInfo2 = new DirectoryInfo(existingDirPath);
				foreach (DirectoryInfo directoryInfo3 in directoryInfo2.GetDirectories())
				{
					bool flag4 = InstallerUtils.CreateHardLinkForDirectory(Path.Combine(existingDirPath, directoryInfo3.Name), Path.Combine(newDirPath, directoryInfo3.Name));
					bool flag5 = !flag4;
					if (flag5)
					{
						return false;
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to create hard link for directory, Err: " + ex.ToString());
			}
			return false;
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00009B84 File Offset: 0x00007D84
		public static bool IsDiskFull(Exception ex)
		{
			int num = Marshal.GetHRForException(ex) & 65535;
			Logger.Info("Win32 error code: " + num.ToString());
			Logger.Info("Disk full error codes are: {0} and {1}", new object[]
			{
				112,
				39
			});
			return num == 39 || num == 112;
		}

		// Token: 0x06000091 RID: 145 RVA: 0x00009BEC File Offset: 0x00007DEC
		public static bool DoesRegistryHKLMKeyExist(string keyPath)
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(keyPath);
			bool flag = registryKey != null;
			bool flag2 = flag;
			if (flag2)
			{
				registryKey.Close();
			}
			return flag;
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00009C20 File Offset: 0x00007E20
		public static Dictionary<string, string> GetSupportedGLModes(string glCheckDir)
		{
			Logger.Info("Checking for supported GL Modes");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			List<string> list = new List<string>();
			List<string> list2 = new List<string>
			{
				"1 1",
				"4 1",
				"1 2",
				"4 2"
			};
			foreach (string text in list2)
			{
				bool flag = InstallerProperties.GlModeSupportedDataDict.ContainsKey(text) && InstallerProperties.GlModeSupportedDataDict[text].supported;
				if (flag)
				{
					list.Add(text);
				}
			}
			dictionary["supported_glmodes"] = string.Join(",", list.ToArray());
			Logger.Info("Supported GL Modes: " + string.Join(",", list.ToArray()));
			return dictionary;
		}

		// Token: 0x06000093 RID: 147 RVA: 0x00009D30 File Offset: 0x00007F30
		internal static bool CopyHypervJson()
		{
			string text = Path.Combine(InstallerProperties.PDDirNew, "Engine");
			string text2 = Path.Combine(text, "HypervVm.json");
			bool flag = !File.Exists(text2);
			bool result;
			if (flag)
			{
				Logger.Error("{0} file does not exist!", new object[]
				{
					text2
				});
				result = false;
			}
			else
			{
				List<string> list = new List<string>();
				using (List<string>.Enumerator enumerator = InstallerProperties.ImageList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string image = enumerator.Current;
						bool flag2 = ImageTraits.Get(image).SupportsHypervisor("hyperv");
						if (flag2)
						{
							IEnumerable<string> enumerable = from directory in Directory.GetDirectories(text)
							select Path.GetFileName(directory) into directory
							where directory.Contains(image)
							select directory;
							foreach (string text3 in enumerable)
							{
								Logger.Info("Creating hypervjson for image: {0}", new object[]
								{
									text3
								});
								bool isUpgrade = InstallerProperties.IsUpgrade;
								if (isUpgrade)
								{
									string path = Path.Combine(Path.Combine(Path.Combine(InstallerProperties.PDDir, "Engine"), text3), "HypervVm.json");
									bool flag3 = File.Exists(path);
									if (flag3)
									{
										JObject jobject = JObject.Parse(File.ReadAllText(path));
										string text4 = jobject["HcsSystem"]["VirtualMachine"]["Devices"]["NetworkAdapters"]["default"]["EndpointId"].ToString();
										bool flag4 = list.Contains(text4);
										if (flag4)
										{
											text4 = GuidUtils.GenerateGuid();
										}
										list.AddIfNotContain(text4);
										string text5 = Path.Combine(Path.Combine(text, text3), "HypervVm.json");
										File.Copy(text2, text5, true);
										JObject jobject2 = JObject.Parse(File.ReadAllText(text5));
										jobject2["HcsSystem"]["VirtualMachine"]["Devices"]["NetworkAdapters"]["default"]["EndpointId"] = text4;
										File.WriteAllText(text5, jobject2.ToString());
										continue;
									}
								}
								File.Copy(text2, Path.Combine(Path.Combine(text, text3), "HypervVm.json"), true);
							}
						}
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x06000094 RID: 148 RVA: 0x0000A008 File Offset: 0x00008208
		internal static bool SetupEngineFilesForHypervisor()
		{
			bool flag = !InstallerUtils.CopyHypervJson();
			bool result;
			if (flag)
			{
				Logger.Fatal("Could not copy HypervVm.json");
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		// Token: 0x06000095 RID: 149 RVA: 0x0000A038 File Offset: 0x00008238
		private static int RunGLCheck(string glCheckDir, string args)
		{
			try
			{
				string cmd = Path.Combine(glCheckDir, "HD-GLCheck.exe");
				CmdRes cmdRes = RunCommand.RunCmd(cmd, args, true, false, 10000);
				return cmdRes.ExitCode;
			}
			catch (Exception ex)
			{
				Logger.Warning("An error occured while running glcheck, ignorning.");
				Logger.Warning(ex.Message);
			}
			return -1;
		}

		// Token: 0x06000096 RID: 150 RVA: 0x0000A09C File Offset: 0x0000829C
		public static int SetupBstkGlobalConfig()
		{
			Logger.Info("In SetupBstkGlobalConfig");
			string path = Path.Combine(Path.Combine(InstallerProperties.PDDir, "Engine"), "Manager");
			string text = Path.Combine(path, "BstkGlobal.xml");
			string text2 = text + ".in";
			try
			{
				File.Copy(text2, text, true);
			}
			catch (Exception ex)
			{
				Logger.Error("Could not copy {0} to {1}. Ex: {2}", new object[]
				{
					text2,
					text,
					ex.Message
				});
				return -41;
			}
			return 0;
		}

		// Token: 0x06000097 RID: 151 RVA: 0x0000A138 File Offset: 0x00008338
		private static bool IsDriverInstalled()
		{
			return ServiceController.GetDevices().Any((ServiceController x) => x.ServiceName.Equals(Strings.BlueStacksDriverName, StringComparison.InvariantCultureIgnoreCase));
		}

		// Token: 0x06000098 RID: 152 RVA: 0x0000A174 File Offset: 0x00008374
		public static int InstallDriver(string driverPath)
		{
			Logger.Info("Will install driver with file path: {0}, DriverName: {1}, DisplayName: {2}", new object[]
			{
				driverPath,
				Strings.BlueStacksDriverName,
				"BlueStacks Hypervisor_msi5"
			});
			bool flag = InstallerUtils.IsDriverInstalled();
			if (flag)
			{
				Logger.Warning("An existing driver with the same name exists. Will uninstall");
				int num = ServiceManager.StopAndUninstallDriver("", 15);
				bool flag2 = num != 0;
				if (flag2)
				{
					return num;
				}
				bool flag3 = InstallerUtils.IsDriverMarkedForDeletion();
				if (flag3)
				{
					return -58;
				}
			}
			bool flag4 = !ServiceManager.InstallDriver(Strings.BlueStacksDriverName, driverPath, "BlueStacks Hypervisor_msi5");
			int result;
			if (flag4)
			{
				Logger.Fatal("Failed to install driver");
				result = -40;
			}
			else
			{
				Logger.Info("Successfully installed driver");
				result = 0;
			}
			return result;
		}

		// Token: 0x06000099 RID: 153 RVA: 0x0000A224 File Offset: 0x00008424
		public static bool IsDriverMarkedForDeletion()
		{
			try
			{
				Logger.Info("Checking for marked for deletion flag in service {0}", new object[]
				{
					Strings.BlueStacksDriverName
				});
				int i = 10;
				while (i > 0)
				{
					i--;
					int num = (int)RegistryUtils.GetRegistryValue(Path.Combine("SYSTEM\\CurrentControlSet\\Services", Strings.BlueStacksDriverName), "DeleteFlag", -99, RegistryKeyKind.HKEY_LOCAL_MACHINE);
					bool flag = num == -99;
					if (flag)
					{
						Logger.Info("DeleteFlag value not set, presuming not marked");
						return false;
					}
					Logger.Info("Delete flag: {0}, retry: {1}", new object[]
					{
						num,
						10 - i
					});
					bool flag2 = num == 1;
					if (!flag2)
					{
						return false;
					}
					bool flag3 = i == 0;
					if (flag3)
					{
						Logger.Warning("Service marked for deletion");
						return true;
					}
					Thread.Sleep(1000);
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Could not check for service marked for deletion. Should be safe to ignore. Ex: ", new object[]
				{
					ex.Message
				});
			}
			return false;
		}

		// Token: 0x0600009A RID: 154 RVA: 0x0000A344 File Offset: 0x00008544
		public static void DumpEventLogs(string name, string dumpFile = "")
		{
			Logger.Info("Dumping event logs for {0}", new object[]
			{
				name
			});
			try
			{
				using (EventLog eventLog = new EventLog(name))
				{
					int count = eventLog.Entries.Count;
					int num = 50;
					int num2 = 0;
					bool flag = count > num;
					if (flag)
					{
						num2 = count - num;
					}
					bool flag2 = !string.IsNullOrEmpty(dumpFile);
					if (flag2)
					{
						InstallerUtils.DumpEventsToFile(eventLog, num2, count, name, dumpFile);
					}
					else
					{
						for (int i = num2; i < count; i++)
						{
							Logger.Info("Event[{0}]:", new object[]
							{
								i
							});
							Logger.Info(" Log Name: {0}", new object[]
							{
								name
							});
							Logger.Info(" Source: {0}", new object[]
							{
								eventLog.Entries[i].Source
							});
							Logger.Info(" Date: {0}", new object[]
							{
								eventLog.Entries[i].TimeGenerated
							});
							Logger.Info(" Event ID: {0}", new object[]
							{
								eventLog.Entries[i].InstanceId
							});
							Logger.Info(" User: {0}", new object[]
							{
								eventLog.Entries[i].UserName
							});
							Logger.Info(" Description:");
							Logger.Info("{0}", new object[]
							{
								eventLog.Entries[i].Message
							});
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Error getting event logs. Ex: {0}", new object[]
				{
					ex
				});
			}
		}

		// Token: 0x0600009B RID: 155 RVA: 0x0000A534 File Offset: 0x00008734
		private static void DumpEventsToFile(EventLog log, int offset, int totalCount, string name, string filePath)
		{
			try
			{
				Logger.Info("Writing events to : " + filePath);
				using (StreamWriter streamWriter = new StreamWriter(filePath, true))
				{
					streamWriter.AutoFlush = true;
					for (int i = offset; i < totalCount; i++)
					{
						streamWriter.WriteLine("Event[{0}]:", i);
						streamWriter.WriteLine(" Log Name: {0}", name);
						streamWriter.WriteLine(" Source: {0}", log.Entries[i].Source);
						streamWriter.WriteLine(" Date: {0}", log.Entries[i].TimeGenerated);
						streamWriter.WriteLine(" Event ID: {0}", log.Entries[i].InstanceId);
						streamWriter.WriteLine(" User: {0}", log.Entries[i].UserName);
						streamWriter.WriteLine(" Description:");
						streamWriter.WriteLine("{0}", log.Entries[i].Message);
						streamWriter.NewLine = "\n";
					}
					streamWriter.Close();
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Error: {0}", new object[]
				{
					ex.Message
				});
			}
		}

		// Token: 0x0600009C RID: 156 RVA: 0x0000A6BC File Offset: 0x000088BC
		public static bool InstallVmConfig(string vm)
		{
			Logger.Info("Creating bstk file for image {0}", new object[]
			{
				vm
			});
			string path = Path.Combine(InstallerProperties.PDDirNew, "Engine");
			string text = Path.Combine(path, vm);
			string text2 = Path.Combine(text, vm + ".bstk");
			string imageFromVmName = Strings.AndroidImages.GetImageFromVmName(vm);
			bool flag = File.Exists(text2);
			bool result;
			if (flag)
			{
				Logger.Info(imageFromVmName + ".bstk already present deleting");
				File.Delete(text2);
				result = true;
			}
			else
			{
				string path2 = Path.Combine(InstallerProperties.PDDir, "Engine");
				string path3 = Path.Combine(path, imageFromVmName);
				string text3 = Path.Combine(path3, "Android.bstk.in");
				string text4 = null;
				bool flag2 = !Directory.Exists(text);
				if (flag2)
				{
					Directory.CreateDirectory(text);
				}
				try
				{
					using (StreamReader streamReader = new StreamReader(text3))
					{
						text4 = streamReader.ReadToEnd();
					}
				}
				catch (Exception ex)
				{
					Logger.Error("Cannot read '{0}'. Ex: {1}", new object[]
					{
						text3,
						ex.Message
					});
					return false;
				}
				text4 = text4.Replace("@@HD_VDES_SERVICE_DLL_PATH@@", SecurityElement.Escape(Path.Combine(InstallerProperties.PFDir, "HD-Vdes-Service.dll")));
				string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				bool flag3 = !string.IsNullOrEmpty(folderPath);
				if (flag3)
				{
					string newValue = string.Format(CultureInfo.InvariantCulture, "<SharedFolder name=\"Documents\" hostPath=\"{0}\" writable=\"true\" autoMount=\"false\"/>", new object[]
					{
						SecurityElement.Escape(folderPath)
					});
					text4 = text4.Replace("@@USER_DOCUMENTS_FOLDER@@", newValue);
				}
				else
				{
					text4 = text4.Replace("@@USER_DOCUMENTS_FOLDER@@", "");
				}
				string folderPath2 = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
				bool flag4 = !string.IsNullOrEmpty(folderPath2);
				if (flag4)
				{
					string newValue2 = string.Format(CultureInfo.InvariantCulture, "<SharedFolder name=\"Pictures\" hostPath=\"{0}\" writable=\"true\" autoMount=\"false\"/>", new object[]
					{
						SecurityElement.Escape(folderPath2)
					});
					text4 = text4.Replace("@@USER_PICTURES_FOLDER@@", newValue2);
				}
				else
				{
					text4 = text4.Replace("@@USER_PICTURES_FOLDER@@", "");
				}
				text4 = text4.Replace("@@INPUT_MAPPER_FOLDER@@", SecurityElement.Escape(Path.Combine(path2, "UserData\\InputMapper")));
				text4 = text4.Replace("@@BST_SHARED_FOLDER@@", SecurityElement.Escape(Path.Combine(path2, "UserData\\SharedFolder")));
				text4 = text4.Replace("@@BST_VM_MEMORY_SIZE@@", SecurityElement.Escape(Utils.GetAndroidVMMemory(true).ToString(CultureInfo.InvariantCulture)));
				try
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(text4);
					XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
					xmlNamespaceManager.AddNamespace("bstk", "http://www.virtualbox.org/");
					XmlNode xmlNode = xmlDocument.SelectSingleNode("descendant::bstk:Machine", xmlNamespaceManager);
					xmlNode.Attributes["name"].Value = vm;
					xmlDocument.Save(text2);
				}
				catch (Exception ex2)
				{
					Logger.Error("Cannot write to '{0}'. Ex: {1}", new object[]
					{
						text2,
						ex2.Message
					});
					return false;
				}
				result = true;
			}
			return result;
		}

		// Token: 0x0600009D RID: 157 RVA: 0x0000A9DC File Offset: 0x00008BDC
		public static bool CheckSupportedGlRenderMode(out int glRenderMode, out string glVendor, out string glRenderer, out string glVersion, out GLMode glMode, List<GlProperty> glCheckOrder, List<GlProperty> fallbackGlCheckOrder)
		{
			glRenderMode = 4;
			glVersion = "";
			glRenderer = "";
			glVendor = "";
			glMode = GLMode.PGA;
			try
			{
				bool flag = glCheckOrder != null && glCheckOrder.Count > 0;
				if (flag)
				{
					foreach (GlProperty glProperty in glCheckOrder)
					{
						Logger.Info("gl check with args.." + glProperty.Gl_renderer.ToString() + " and " + glProperty.Gl_mode.ToString());
						bool flag2 = BlueStacksGL.GLCheckInstallation(glProperty.Gl_renderer, glProperty.Gl_mode, out glVendor, out glRenderer, out glVersion) == 0;
						if (flag2)
						{
							glRenderMode = (int)glProperty.Gl_renderer;
							glMode = glProperty.Gl_mode;
							return true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				string str = "exception in getting gl check from list..";
				Exception ex2 = ex;
				Logger.Error(str + ((ex2 != null) ? ex2.ToString() : null));
			}
			bool flag3 = false;
			foreach (string imageName in InstallerProperties.ImageList)
			{
				bool isAgaModeCompatible = ImageTraits.Get(imageName).IsAgaModeCompatible;
				if (isAgaModeCompatible)
				{
					flag3 = true;
					break;
				}
			}
			bool flag4 = flag3 && InstallerUtils.CheckSupportedGlRenderMode(GLMode.AGA, out glRenderMode, out glVendor, out glRenderer, out glVersion);
			bool result;
			if (flag4)
			{
				Logger.Info("AGA supported!");
				glMode = GLMode.AGA;
				result = true;
			}
			else
			{
				bool flag5 = fallbackGlCheckOrder != null && fallbackGlCheckOrder.Count > 0;
				if (flag5)
				{
					foreach (GlProperty glProperty2 in fallbackGlCheckOrder)
					{
						Logger.Info("gl check with args.." + glProperty2.Gl_renderer.ToString() + " and " + glProperty2.Gl_mode.ToString());
						bool flag6 = BlueStacksGL.GLCheckInstallation(glProperty2.Gl_renderer, glProperty2.Gl_mode, out glVendor, out glRenderer, out glVersion) == 0;
						if (flag6)
						{
							glRenderMode = (int)glProperty2.Gl_renderer;
							glMode = glProperty2.Gl_mode;
							return true;
						}
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x0600009E RID: 158 RVA: 0x0000AC78 File Offset: 0x00008E78
		public static bool GetGlSupportedDataForAllModes(List<GlProperty> glCheckOrder)
		{
			bool result = false;
			foreach (GlProperty glProperty in glCheckOrder)
			{
				Logger.Info("gl check with args.." + glProperty.Gl_renderer.ToString() + " and " + glProperty.Gl_mode.ToString());
				bool flag = false;
				string glVendor = "";
				string glRendererString = "";
				string glVersion = "";
				try
				{
					flag = (BlueStacksGL.GLCheckInstallation(glProperty.Gl_renderer, glProperty.Gl_mode, out glVendor, out glRendererString, out glVersion) == 0);
					bool flag2 = flag;
					if (flag2)
					{
						result = true;
					}
				}
				catch (Exception arg)
				{
					Logger.Error(string.Format("Failed to find gl support for args - {0} and {1}. Ex : {2}", glProperty.Gl_renderer, glProperty.Gl_mode, arg));
				}
				InstallerProperties.GlModeSupportedDataDict[string.Format("{0} {1}", (int)glProperty.Gl_renderer, (int)glProperty.Gl_mode)] = new GlPropertyData
				{
					supported = flag,
					glProperty = new GlProperty
					{
						Gl_mode = glProperty.Gl_mode,
						Gl_renderer = glProperty.Gl_renderer
					},
					glRendererString = glRendererString,
					glVendor = glVendor,
					glVersion = glVersion
				};
			}
			return result;
		}

		// Token: 0x0600009F RID: 159 RVA: 0x0000AE1C File Offset: 0x0000901C
		public static bool CheckSupportedGlRenderMode(GLMode mode, out int glRenderMode, out string glVendor, out string glRenderer, out string glVersion)
		{
			glRenderMode = 4;
			glVersion = "";
			glRenderer = "";
			glVendor = "";
			try
			{
				bool flag = BlueStacksGL.GLCheckInstallation(GLRenderer.OpenGL, mode, out glVendor, out glRenderer, out glVersion) == 0;
				int num;
				if (flag)
				{
					num = 0;
					glRenderMode = 1;
				}
				else
				{
					bool flag2 = SystemUtils.IsOs64Bit();
					if (flag2)
					{
						bool flag3 = InstallerUtils.GpuWithDx9SupportOnly();
						if (flag3)
						{
							Logger.Info("Machine has gpu which runs on DX9 only");
							glRenderMode = 2;
							num = BlueStacksGL.GLCheckInstallation(GLRenderer.DX9, mode, out glVendor, out glRenderer, out glVersion);
						}
						else
						{
							glRenderMode = 4;
							num = BlueStacksGL.GLCheckInstallation(GLRenderer.DX11FallbackDX9, mode, out glVendor, out glRenderer, out glVersion);
							bool flag4 = num != 0;
							if (flag4)
							{
								glRenderMode = 2;
								num = BlueStacksGL.GLCheckInstallation(GLRenderer.DX9, mode, out glVendor, out glRenderer, out glVersion);
							}
						}
					}
					else
					{
						glRenderMode = 2;
						num = BlueStacksGL.GLCheckInstallation(GLRenderer.DX9, mode, out glVendor, out glRenderer, out glVersion);
					}
				}
				bool flag5 = num != 0;
				if (flag5)
				{
					Logger.Info("DirectX not supported.");
					glRenderMode = -1;
					return false;
				}
				return true;
			}
			catch (Exception ex)
			{
				Logger.Error("Some error occured while checking for GL. Ex: {0}", new object[]
				{
					ex
				});
			}
			return false;
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x0000AF28 File Offset: 0x00009128
		public static bool GpuWithDx9SupportOnly()
		{
			string[] array = new string[]
			{
				"Mobile Intel(R) 4 Series Express Chipset Family",
				"Mobile Intel(R) 45 Express Chipset Family",
				"Mobile Intel(R) 965 Express Chipset Family",
				"Intel(R) G41 Express Chipset",
				"Intel(R) G45/G43 Express Chipset",
				"Intel(R) Q45/Q43 Express Chipset"
			};
			string text = "";
			try
			{
				using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DisplayConfiguration"))
				{
					foreach (ManagementBaseObject managementBaseObject in managementObjectSearcher.Get())
					{
						ManagementObject managementObject = (ManagementObject)managementBaseObject;
						foreach (PropertyData propertyData in managementObject.Properties)
						{
							bool flag = propertyData.Name == "Description";
							if (flag)
							{
								text = propertyData.Value.ToString();
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Exception while runninq query. Error: ", new object[]
				{
					ex.ToString()
				});
			}
			Logger.Info("Graphics card: {0}", new object[]
			{
				text
			});
			string text2 = text.ToLower(CultureInfo.InvariantCulture);
			bool flag2 = text2.Contains("intel") && text2.Contains("express chipset");
			bool result;
			if (flag2)
			{
				Logger.Info("graphicsCard : {0} part of the list of graphics card to be forced to dx9", new object[]
				{
					text
				});
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x0000B0E4 File Offset: 0x000092E4
		public static bool IsProcessorIntel()
		{
			string processor = Profile.Processor;
			return !string.IsNullOrEmpty(processor) && processor.Contains("Intel");
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x0000B114 File Offset: 0x00009314
		public static string ReadFile(string path)
		{
			string result = null;
			try
			{
				using (StreamReader streamReader = new StreamReader(path))
				{
					result = streamReader.ReadToEnd();
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Cannot read '{0}', Ex: {1}", new object[]
				{
					path,
					ex
				});
			}
			return result;
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x0000B184 File Offset: 0x00009384
		public static void RestoreOldLogs(string srcLogsDir, string destLogsDir, string oldVersion)
		{
			Logger.Info("RestoreOldLogs start");
			try
			{
				bool flag = string.IsNullOrEmpty(oldVersion);
				if (!flag)
				{
					Logger.Info("Restoring logs with version: " + oldVersion);
					DirectoryInfo directoryInfo = new DirectoryInfo(srcLogsDir);
					foreach (FileInfo fileInfo in directoryInfo.GetFiles())
					{
						bool flag2 = fileInfo.Extension != ".log";
						if (!flag2)
						{
							string text = fileInfo.Name.Replace(".log", "");
							string newFilePath = Path.Combine(destLogsDir, string.Format(CultureInfo.InvariantCulture, "{0}_{1}.log", new object[]
							{
								text,
								oldVersion
							}));
							InstallerUtils.CreateHardLinkForFile(fileInfo.FullName, newFilePath);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Failed in copying old logs. Ex: " + ex.ToString());
			}
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x0000B27C File Offset: 0x0000947C
		public static void CreateNotificationActivatorRegistry()
		{
			string value = Path.Combine(InstallerProperties.PFDir, "HD-Player.exe -notificationActivated");
			RegistryUtils.SetRegistryValue("SOFTWARE\\Classes\\CLSID\\{3DA0A2FE-AA89-4D56-BB84-3FEBBD0B8D99}\\LocalServer32", null, value, RegistryValueKind.String, RegistryKeyKind.HKEY_LOCAL_MACHINE);
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x0000B2AC File Offset: 0x000094AC
		private static void CreateWerRegistry()
		{
			string text = "SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps";
			RegistryUtils.InitKey(text);
			string path = Path.Combine(InstallerProperties.PDDir, "Dumps");
			using (RegistryKey registryKey = RegistryUtils.InitKey(text + "\\HD-Player.exe"))
			{
				registryKey.SetValue("DumpFolder", Path.Combine(path, "HD-Player"));
				registryKey.SetValue("DumpCount", 2);
			}
			using (RegistryKey registryKey2 = RegistryUtils.InitKey(text + "\\HD-MultiInstanceManager.exe"))
			{
				registryKey2.SetValue("DumpFolder", Path.Combine(path, "HD-MultiInstanceManager"));
				registryKey2.SetValue("DumpCount", 2);
			}
			using (RegistryKey registryKey3 = RegistryUtils.InitKey("SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\RuntimeExceptionHelperModules"))
			{
				registryKey3.SetValue(Path.Combine(InstallerProperties.PFDir, "HD-WerHandler.dll"), 0);
			}
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x0000B3CC File Offset: 0x000095CC
		public static bool CreateBlueStacksRegistry()
		{
			Logger.Info("Creating BlueStacks registry");
			try
			{
				string pddir = InstallerProperties.PDDir;
				string str = Path.Combine(pddir, "Engine");
				string str2 = Path.Combine(pddir, "Logs");
				string pfdir = InstallerProperties.PFDir;
				string guid = InstallerProperties.GUID;
				RegistryManager.ClearRegistryMangerInstance();
				RegistryManager.Instance.LogDir = str2 + "\\";
				RegistryManager.Instance.DataDir = str + "\\";
				RegistryManager.Instance.InstallDir = pfdir + "\\";
				RegistryManager.Instance.UserDefinedDir = pddir;
				RegistryManager.Instance.UserGuid = guid;
				RegistryManager.Instance.IsUpgrade = InstallerProperties.IsUpgrade;
				RegistryManager.Instance.Version = "5.12.120.6303";
				RegistryManager.Instance.CampaignJson = InstallerProperties.CampaignJson;
				InstallerUtils.CreateWerRegistry();
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to create registry. Err: " + ex.ToString());
				return false;
			}
			return true;
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x0000B4E4 File Offset: 0x000096E4
		internal static void AssignHypervisorProperties()
		{
			try
			{
				Logger.Info("Assigning hypervisor");
				InstallerProperties.CurrentHypervisor = BridgeApis.GetCompatibleHypervisors().First<string>();
				Logger.Info("Hypervisor: {0}", new object[]
				{
					InstallerProperties.CurrentHypervisor
				});
			}
			catch (Exception ex)
			{
				Logger.Error("Error while getting active hypervisor. Ex: {0}", new object[]
				{
					ex.Message
				});
				InstallerProperties.CurrentHypervisor = "";
			}
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x0000B564 File Offset: 0x00009764
		internal static void WaitForWalletComplition()
		{
			Thread thread = new Thread(delegate()
			{
				try
				{
					Logger.Info("checking lock of download");
					for (int i = 0; i < 600; i++)
					{
						bool flag = !ProcessUtils.IsLockInUse("Global\\Nowgg_Wallet_Download_Lockmsi5");
						if (flag)
						{
							Logger.Info("wallet download completed");
							break;
						}
						Thread.Sleep(2000);
					}
				}
				catch (Exception ex)
				{
					Logger.Error("Error in checking wallet download lock ex: ", new object[]
					{
						ex.Message
					});
				}
				try
				{
					Logger.Info("checking wallet installer process ");
					Process[] processesByName = Process.GetProcessesByName(MicroInstallerSharedUtils.GetNowggWalletProcessName());
					Process[] array = processesByName;
					int j = 0;
					while (j < array.Length)
					{
						Process process = array[j];
						bool flag2 = process.MainModule.FileName.StartsWith(MicroInstallerSharedUtils.GetNowggWalletInstallerPath(), StringComparison.InvariantCultureIgnoreCase);
						if (flag2)
						{
							Logger.Info("wallet installer process found");
							IntPtr handle = process.Handle;
							bool flag3 = process.WaitForExit(240000);
							bool flag4 = flag3;
							if (flag4)
							{
								Logger.Info("wallet installation completed");
								InstallerStats.Enqueue("mi_wallet_install_completed", false, "", "");
								return;
							}
							Logger.Info("wallet installation process timeout");
							InstallerStats.Enqueue("mi_wallet_install_timeout", false, "", "");
							return;
						}
						else
						{
							j++;
						}
					}
					InstallerStats.Enqueue("mi_wallet_install_completed", false, "", "");
				}
				catch (Exception ex2)
				{
					Logger.Error("Error in checking wallet installer process handle ex: {0}", new object[]
					{
						ex2.Message
					});
				}
			});
			thread.Start();
			thread.Join();
		}
	}
}
