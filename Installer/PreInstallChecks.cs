using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BlueStacks.Common;
using Microsoft.Win32;

namespace BlueStacks.Installer
{
	// Token: 0x02000018 RID: 24
	public class PreInstallChecks
	{
		// Token: 0x060000BA RID: 186 RVA: 0x0000BD1C File Offset: 0x00009F1C
		public static int PerformPreInstallChecks()
		{
			Logger.Info("Starting PreInstall Checks");
			bool flag = InstallerProperties.IsMicrosoftHyperVEnabled && SystemUtils.IsOSWin10();
			if (flag)
			{
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion"))
				{
					int num;
					bool flag2 = int.TryParse(registryKey.GetValue("ReleaseId").ToString(), out num) && num < 1903;
					if (flag2)
					{
						Logger.Fatal("Windows 10 ReleaseId is less than required. {0} < {1}", new object[]
						{
							num,
							1903
						});
						return -21;
					}
				}
			}
			bool flag3 = !InstallerProperties.StartedFromMicroInstaller;
			if (flag3)
			{
				System.Version version;
				bool flag4 = !PreInstallChecks.IsOSSupported(out version);
				if (flag4)
				{
					return -10;
				}
				bool flag5 = version.Major == 6 && version.Minor == 1 && version.Build == 7600;
				if (flag5)
				{
					return -25;
				}
				bool flag6 = !PreInstallChecks.IsRequiredRAMAvailable();
				if (flag6)
				{
					return -11;
				}
				bool isFreshInstall = InstallerProperties.IsFreshInstall;
				if (isFreshInstall)
				{
					long num2;
					string text;
					bool flag7 = !PreInstallChecks.IsRequiredDiskSpaceAvailable(out num2, out text);
					if (flag7)
					{
						return -14;
					}
				}
			}
			bool isFreshInstall2 = InstallerProperties.IsFreshInstall;
			if (isFreshInstall2)
			{
				bool flag8 = !PreInstallChecks.CheckGLSupported();
				if (flag8)
				{
					InstallerProperties.FailureCodes = MiFailureCodes.Gl;
					return ((InstallerStats.InstallerComment & InstallerComments.NO_SUPPORT_FOR_AGA) == InstallerComments.NO_SUPPORT_FOR_AGA) ? -26 : -12;
				}
			}
			else
			{
				Logger.Info("Will preserve old GL values");
				GLMode glMode;
				int glRenderMode;
				PreInstallChecks.GetOldGLValues(out glMode, out glRenderMode);
				PreInstallChecks.SetInstallerPropertiesForGL(glRenderMode, "", "", "", glMode);
			}
			bool flag9 = -1 == InstallerProperties.IsSSE4Available;
			if (flag9)
			{
				InstallerProperties.IsSSE4Available = PreInstallChecks.CheckSSE4Support();
			}
			bool isFreshInstall3 = InstallerProperties.IsFreshInstall;
			if (isFreshInstall3)
			{
				bool flag10 = 1 != InstallerProperties.IsSSE4Available;
				if (flag10)
				{
					foreach (string imageName in InstallerProperties.ImageList)
					{
						bool is64Bit = ImageTraits.Get(imageName).Is64Bit;
						if (is64Bit)
						{
							return -23;
						}
					}
				}
			}
			return 0;
		}

		// Token: 0x060000BB RID: 187 RVA: 0x0000BF84 File Offset: 0x0000A184
		private static int CheckSSE4Support()
		{
			int num = (int)RegistryUtils.GetRegistryValue("Software\\BlueStacks_msi5", "sse4Available", -1, RegistryKeyKind.HKEY_CURRENT_USER);
			bool flag = num == 1 || num == 0;
			int result;
			if (flag)
			{
				result = num;
			}
			else
			{
				string cmd = Path.Combine(InstallerProperties.InstallerExtractedDir, "HD-CheckCpu.exe");
				string args = "--cmd checkSSE4";
				int exitCode = RunCommand.RunCmd(cmd, args, true, false, 0).ExitCode;
				result = exitCode;
			}
			return result;
		}

		// Token: 0x060000BC RID: 188 RVA: 0x0000BFF4 File Offset: 0x0000A1F4
		private static bool IsOSSupported(out System.Version osVersion)
		{
			Logger.Info("Checking for supported OS");
			OperatingSystem osversion = Environment.OSVersion;
			osVersion = osversion.Version;
			string text;
			string text2;
			string text3;
			bool osinfo = SystemUtils.GetOSInfo(out text, out text2, out text3);
			bool result;
			if (osinfo)
			{
				Logger.Info("OS Info: Name: {0}, Service Pack: {1}, Arch: {2}, Ver: {3}.{4}", new object[]
				{
					text,
					text2,
					text3,
					osversion.Version.Major,
					osversion.Version.Minor
				});
				bool flag = osversion.Version.Major < 6 || (osversion.Version.Major == 6 && osversion.Version.Minor == 0);
				result = !flag;
			}
			else
			{
				InstallerStats.InstallerComment |= InstallerComments.FAILED_TO_GET_OS_INFO;
				Logger.Warning("Could not get OS info, skipping OS checks");
				result = true;
			}
			return result;
		}

		// Token: 0x060000BD RID: 189 RVA: 0x0000C0D4 File Offset: 0x0000A2D4
		private static void GetOldGLValues(out GLMode glMode, out int glRenderMode)
		{
			glRenderMode = 1;
			glMode = GLMode.PGA;
			try
			{
				string registryPath = Path.Combine(InstallerProperties.RegistryBasePath, "Guests\\Android\\Config");
				glRenderMode = (int)RegistryUtils.GetRegistryValue(registryPath, "GlRenderMode", 1, RegistryKeyKind.HKEY_LOCAL_MACHINE);
				glMode = (GLMode)RegistryUtils.GetRegistryValue(registryPath, "glMode", 1, RegistryKeyKind.HKEY_LOCAL_MACHINE);
			}
			catch
			{
				Logger.Error("Some error while getting old values");
			}
		}

		// Token: 0x060000BE RID: 190 RVA: 0x0000C150 File Offset: 0x0000A350
		private static bool CheckGLSupported()
		{
			InstallerStats.Enqueue("install_checks_gl_initing", false, "", "");
			int videoControllersNum = Utils.GetVideoControllersNum();
			Logger.Info("Number of video controllers: {0}", new object[]
			{
				videoControllersNum
			});
			int num = 10;
			bool flag = !InstallerProperties.IsGlCheckOrderResponseCame;
			if (flag)
			{
				Logger.Info("Waiting for gl order cloud response. Retries {0}", new object[]
				{
					num
				});
				while (num > 0 && !InstallerProperties.IsGlCheckOrderResponseCame)
				{
					num--;
					Thread.Sleep(1000);
				}
			}
			bool glSupportedDataForAllModes = InstallerUtils.GetGlSupportedDataForAllModes(InstallerProperties.FallbackGlCheckOrderList);
			Logger.Info("Gl order recieved {0}", new object[]
			{
				InstallerProperties.IsGlCheckOrderResponseCame
			});
			InstallerStats.Enqueue("install_checks_gl_listready", false, string.Join(", ", InstallerProperties.GlCheckOrderList.ConvertAll<string>((GlProperty x) => string.Format("{0}, {1}", x.Gl_renderer, x.Gl_mode)).ToArray()), "");
			bool flag2 = !glSupportedDataForAllModes;
			bool result;
			if (flag2)
			{
				Logger.Error("GLCheck has failed");
				InstallerStats.InstallerComment |= InstallerComments.GL_CHECK_FAILED;
				PreInstallChecks.SetInstallerPropertiesForGL(-1, "", "", "", GLMode.PGA);
				result = false;
			}
			else
			{
				GlPropertyData glPropertyData = new GlPropertyData
				{
					glProperty = new GlProperty
					{
						Gl_mode = GLMode.PGA,
						Gl_renderer = GLRenderer.DX9
					},
					glRendererString = "",
					glVendor = "",
					glVersion = ""
				};
				bool flag3 = InstallerProperties.GlCheckOrderList.Count == 0;
				if (flag3)
				{
					foreach (KeyValuePair<string, GlPropertyData> keyValuePair in InstallerProperties.GlModeSupportedDataDict)
					{
						bool supported = keyValuePair.Value.supported;
						if (supported)
						{
							glPropertyData = keyValuePair.Value;
							break;
						}
					}
				}
				else
				{
					bool flag4 = false;
					bool flag5 = !InstallerProperties.ImageList.Contains("Nougat32") && !InstallerProperties.ImageList.Contains("Nougat64");
					if (flag5)
					{
						flag4 = true;
					}
					foreach (GlProperty glProperty in InstallerProperties.GlCheckOrderList)
					{
						string key = string.Format("{0} {1}", (int)glProperty.Gl_renderer, (int)glProperty.Gl_mode);
						bool flag6 = InstallerProperties.GlModeSupportedDataDict.ContainsKey(key) && InstallerProperties.GlModeSupportedDataDict[key].supported;
						if (flag6)
						{
							bool flag7 = flag4;
							if (!flag7)
							{
								glPropertyData = InstallerProperties.GlModeSupportedDataDict[key];
								break;
							}
							bool flag8 = glProperty.Gl_mode == GLMode.AGA;
							if (flag8)
							{
								glPropertyData = InstallerProperties.GlModeSupportedDataDict[key];
								break;
							}
						}
					}
					bool flag9 = flag4 && glPropertyData.glProperty.Gl_mode == GLMode.PGA;
					if (flag9)
					{
						Logger.Error(string.Format("Only AGA mode is supported for android image(s) - {0}", InstallerProperties.ImageList));
						InstallerStats.InstallerComment |= InstallerComments.NO_SUPPORT_FOR_AGA;
						PreInstallChecks.SetInstallerPropertiesForGL(-1, "", "", "", GLMode.PGA);
						return false;
					}
				}
				Logger.Info(string.Format("Successfully set gl mode : {0} and gl renderer mode : {1}", glPropertyData.glProperty.Gl_mode, glPropertyData.glProperty.Gl_renderer));
				PreInstallChecks.SetInstallerPropertiesForGL((int)glPropertyData.glProperty.Gl_renderer, glPropertyData.glRendererString, glPropertyData.glVendor, glPropertyData.glVersion, glPropertyData.glProperty.Gl_mode);
				InstallerStats.Enqueue("install_checks_gl_passed", false, string.Format("{0}, GLRenderMode: {1} ", glPropertyData.glProperty.Gl_mode, glPropertyData.glProperty.Gl_renderer), "");
				result = true;
			}
			return result;
		}

		// Token: 0x060000BF RID: 191 RVA: 0x00002291 File Offset: 0x00000491
		private static void SetInstallerPropertiesForGL(int glRenderMode, string glRenderer = "", string glVendor = "", string glVersion = "", GLMode glMode = GLMode.PGA)
		{
			InstallerProperties.GLMode = (int)glMode;
			InstallerProperties.GLRenderMode = glRenderMode;
			InstallerProperties.GLRenderer = glRenderer;
			InstallerProperties.GLVendor = glVendor;
			InstallerProperties.GLVersion = glVersion;
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x0000C574 File Offset: 0x0000A774
		public static bool IsRequiredDiskSpaceAvailable(out long spaceRequired, out string drive)
		{
			spaceRequired = 0L;
			drive = "C";
			int num = 500;
			int num2 = 5120 * InstallerProperties.ImageList.Count;
			int num3 = num2 + num;
			try
			{
				bool flag = IOUtils.IsInSameDrive(InstallerProperties.PDDirNew, InstallerProperties.PFDirNew);
				Logger.Info("Data and InstallDir same: {0}", new object[]
				{
					flag
				});
				Logger.Info("Space required InstallDir: {0} MB", new object[]
				{
					num
				});
				Logger.Info("Space required DataDir: {0} MB", new object[]
				{
					num2
				});
				bool flag2 = flag;
				if (flag2)
				{
					long num4 = IOUtils.GetAvailableDiskSpaceOfDrive(InstallerProperties.PFDirNew) / 1048576L;
					spaceRequired = (long)num3;
					drive = InstallerProperties.PFDirNew;
					Logger.Info("{0} > {1}?", new object[]
					{
						num4,
						num3
					});
					return num4 > (long)num3;
				}
				long num5 = IOUtils.GetAvailableDiskSpaceOfDrive(InstallerProperties.PDDirNew) / 1048576L;
				Logger.Info("{0} > {1}?", new object[]
				{
					num5,
					num2
				});
				bool flag3 = (long)num2 > num5;
				if (flag3)
				{
					spaceRequired = (long)num2;
					drive = InstallerProperties.PDDirNew;
					return false;
				}
				long num6 = IOUtils.GetAvailableDiskSpaceOfDrive(InstallerProperties.PFDirNew) / 1048576L;
				Logger.Info("{0} > {1}?", new object[]
				{
					num6,
					num
				});
				spaceRequired = (long)num;
				drive = InstallerProperties.PFDirNew;
				return num6 > (long)num;
			}
			catch (Exception ex)
			{
				InstallerStats.InstallerComment |= InstallerComments.FAILED_TO_CHECK_DISK_SPACE;
				Logger.Error("Some error calculating disk space: {0}", new object[]
				{
					ex
				});
			}
			return true;
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x0000C74C File Offset: 0x0000A94C
		private static bool IsRequiredRAMAvailable()
		{
			try
			{
				Logger.Info("Checking for physical memory");
				int num = int.Parse(Profile.Ram);
				Logger.Info("Available: {0} MB, Required: {1} MB", new object[]
				{
					num,
					1024
				});
				bool flag = num < 1024;
				if (flag)
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				InstallerStats.InstallerComment |= InstallerComments.FAILED_TO_CHECK_RAM;
				Logger.Warning("Failed to check physical memory, ignoring. Err: " + ex.Message);
			}
			return true;
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x0000C7E8 File Offset: 0x0000A9E8
		internal static bool SpaceAvailableInInstallerExtractDict()
		{
			int num = 500;
			long num2 = IOUtils.GetAvailableDiskSpaceOfDrive(InstallerProperties.InstallerExtractedDir) / 1048576L;
			Logger.Info("installer extract dir space avaialble is: {0} and required is: {1}?", new object[]
			{
				num2,
				num
			});
			return num2 > (long)num;
		}

		// Token: 0x04000067 RID: 103
		private const int MB_MULTIPLIER = 1048576;
	}
}
