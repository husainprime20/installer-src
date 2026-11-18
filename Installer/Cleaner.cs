using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BlueStacks.Common;

namespace BlueStacks.Installer
{
	// Token: 0x02000008 RID: 8
	internal class Cleaner
	{
		// Token: 0x06000019 RID: 25 RVA: 0x00005134 File Offset: 0x00003334
		public static bool CleanRemnants(MainWindow ParentWindow)
		{
			Cleaner.CheckAndCleanIfCorruptInstallation(ParentWindow);
			bool flag = Cleaner.sIsCorrupt;
			if (flag)
			{
				Cleaner.ZipOldLogsAndSendStat();
			}
			Cleaner.KillRunningProcesses();
			Cleaner.RemoveDriver();
			Cleaner.CleanFolders();
			Cleaner.RemoveRegistry();
			InstallerStats.Enqueue("install_conflicts_cleaned", false, "", "");
			return Cleaner.sIsCorrupt;
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00005190 File Offset: 0x00003390
		private static void CheckAndCleanIfCorruptInstallation(MainWindow ParentWindow)
		{
			try
			{
				bool isUpgrade = InstallerProperties.IsUpgrade;
				if (isUpgrade)
				{
					bool flag = !Directory.Exists(InstallerProperties.PFDirExisting) || !Directory.Exists(InstallerProperties.PDDirExisting);
					if (flag)
					{
						Logger.Info("Either directory {0} or {1} don't exists", new object[]
						{
							InstallerProperties.PFDirExisting,
							InstallerProperties.PDDirExisting
						});
						Cleaner.AskUserToContinueCleaningOrExit(ParentWindow);
						return;
					}
				}
				long directorySize = Cleaner.sPFThresholdMB;
				long directorySize2 = Cleaner.sPDThresholdMB;
				bool flag2 = Directory.Exists(InstallerProperties.PFDirExisting);
				if (flag2)
				{
					directorySize = IOUtils.GetDirectorySize(InstallerProperties.PFDirExisting);
					Logger.Info("PF size: {0}MB", new object[]
					{
						directorySize / Cleaner.MB_MULTIPLIER
					});
				}
				bool flag3 = Directory.Exists(InstallerProperties.PDDirExisting);
				if (flag3)
				{
					directorySize2 = IOUtils.GetDirectorySize(InstallerProperties.PDDirExisting);
					Logger.Info("PD size: {0}MB", new object[]
					{
						directorySize2 / Cleaner.MB_MULTIPLIER
					});
				}
				bool flag4 = directorySize < Cleaner.sPFThresholdMB || directorySize2 < Cleaner.sPDThresholdMB;
				if (flag4)
				{
					Logger.Warning("Detected corrupt installation");
					Logger.Warning("{0} size: {1}MB", new object[]
					{
						InstallerProperties.PFDirExisting,
						directorySize / Cleaner.MB_MULTIPLIER
					});
					Logger.Warning("{0} size: {1}MB", new object[]
					{
						InstallerProperties.PDDirExisting,
						directorySize2 / Cleaner.MB_MULTIPLIER
					});
					bool isUpgrade2 = InstallerProperties.IsUpgrade;
					if (isUpgrade2)
					{
						Cleaner.AskUserToContinueCleaningOrExit(ParentWindow);
					}
					else
					{
						Cleaner.ContinueCleaningHandler(null, null);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("An error occured while checking for corrupt installation, asking user for clean installation or exit");
				Logger.Warning(ex.ToString());
				Cleaner.AskUserToContinueCleaningOrExit(ParentWindow);
				bool flag5 = Cleaner.sThrowError;
				if (flag5)
				{
					throw;
				}
			}
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00005364 File Offset: 0x00003564
		public static void AskUserToContinueCleaningOrExit(MainWindow ParentWindow)
		{
			Logger.Info("Showing popup to user to clean existing installation or exit");
			string bodyText = string.Format("{0}\n{1}: {2}", LocaleStrings.GetLocalizedString("STRING_OLD_INSTALLATION_INTERFERING", ""), LocaleStrings.GetLocalizedString("STRING_WARNING", ""), LocaleStrings.GetLocalizedString("STRING_CLEAN_BLUESTACKS", ""));
			ParentWindow.Dispatcher.Invoke(new Action(delegate()
			{
				CustomMessageWindow customMessageWindow = new CustomMessageWindow();
				customMessageWindow.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_INSTALLATION_ERROR", "");
				customMessageWindow.BodyTextBlock.Text = bodyText;
				customMessageWindow.AddButton(ButtonColors.Blue, LocaleStrings.GetLocalizedString("STRING_UPGRADE_FROM_BS1_CLEAN_OK", ""), new EventHandler(Cleaner.ContinueCleaningHandler), null, false, null, true);
				customMessageWindow.AddButton(ButtonColors.White, LocaleStrings.GetLocalizedString("STRING_UPGRADE_FROM_BS1_CLEAN_CANCEL", ""), new EventHandler(Cleaner.CleanCancelledHandler), null, false, null, true);
				customMessageWindow.CloseButtonHandle(new EventHandler(Cleaner.CleanCancelledHandler), null);
				customMessageWindow.Owner = ParentWindow;
				customMessageWindow.ShowDialog();
				Cleaner.sLocker.Set();
			}), new object[0]);
			Cleaner.sLocker.WaitOne();
		}

		// Token: 0x0600001C RID: 28 RVA: 0x000053F8 File Offset: 0x000035F8
		private static void ContinueCleaningHandler(object sender, EventArgs e)
		{
			Logger.Info("User chose to clean BlueStacks, settings it's properties");
			Cleaner.sIsCorrupt = true;
			bool isUpgrade = InstallerProperties.IsUpgrade;
			if (isUpgrade)
			{
				InstallerStats.Enqueue("upgrade_cleaned", true, "", "");
				InstallerStats.Enqueue("install_launched", true, "", "");
			}
			InstallerStats.InstallerComment |= InstallerComments.INSTALLATION_WAS_CORRUPT;
			Cleaner.ResetInstallerProperties();
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00005464 File Offset: 0x00003664
		private static void ZipOldLogsAndSendStat()
		{
			try
			{
				string text = Path.Combine(Path.GetTempPath(), "BstCorruptFiles");
				bool flag = Directory.Exists(text);
				if (flag)
				{
					Directory.Delete(text, true);
				}
				Directory.CreateDirectory(text);
				string text2 = Path.Combine(InstallerProperties.PDDir, "Logs");
				bool flag2 = Directory.Exists(text2);
				if (flag2)
				{
					try
					{
						string text3 = Path.Combine(text, "Logs");
						Directory.CreateDirectory(text3);
						InstallerUtils.CopyFiles(text2, text3);
					}
					catch (Exception ex)
					{
						string str = "Failed to copy logs dir. Ex : ";
						Exception ex2 = ex;
						Logger.Error(str + ((ex2 != null) ? ex2.ToString() : null));
					}
				}
				string text4 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Bluestacks");
				bool flag3 = Directory.Exists(text4);
				if (flag3)
				{
					try
					{
						string text5 = Path.Combine(text, "InstallerLogs");
						Directory.CreateDirectory(text5);
						InstallerUtils.CopyFiles(text4, text5);
					}
					catch (Exception ex3)
					{
						string str2 = "Failed to copy installer logs dir. Ex : ";
						Exception ex4 = ex3;
						Logger.Error(str2 + ((ex4 != null) ? ex4.ToString() : null));
					}
				}
				string text6 = Path.Combine(InstallerProperties.PDDir, "bluestacks.conf");
				bool flag4 = File.Exists(text6);
				if (flag4)
				{
					try
					{
						File.Copy(text6, Path.Combine(text, Path.GetFileName(text6)));
					}
					catch (Exception ex5)
					{
						Logger.Error("Failed to copy conf file. Ex : {0}", new object[]
						{
							ex5
						});
					}
				}
				InstallerUtils.DumpEventLogs("Application", Path.Combine(text, "ApplicationEvents.txt"));
				InstallerUtils.DumpEventLogs("System", Path.Combine(text, "SystemEvents.txt"));
				string text7 = Path.Combine(Path.GetTempPath(), "BstCorruptFiles.zip");
				InstallerUtils.CreateZipFile(text, text7);
				bool flag5 = text7 != null && File.Exists(text7);
				if (flag5)
				{
					InstallerStats.ReportInstallFailedStats("install_old_corrupt_files", InstallerUtils.ConvertIntToEnumString(-64), text7, "application/x-7z-compressed");
					File.Delete(text7);
				}
				else
				{
					Logger.Error("corrupt files zip(BstCorruptFiles.zip) not found");
				}
				Directory.Delete(text, true);
			}
			catch (Exception ex6)
			{
				string str3 = "Failed to send corrupt logs. Ex : ";
				Exception ex7 = ex6;
				Logger.Error(str3 + ((ex7 != null) ? ex7.ToString() : null));
			}
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000020DF File Offset: 0x000002DF
		private static void CleanCancelledHandler(object sender, EventArgs e)
		{
			Logger.Info("User chose to keep the old and possibly corrupt BlueStacks, exiting");
			App.ExitApplication(-15);
		}

		// Token: 0x0600001F RID: 31 RVA: 0x000020F5 File Offset: 0x000002F5
		private static void ResetInstallerProperties()
		{
			InstallerProperties.IsUpgrade = false;
			InstallerProperties.PFDirNew = InstallerProperties.PFDir;
			InstallerProperties.PDDirNew = InstallerProperties.PDDir;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x000056DC File Offset: 0x000038DC
		private static void RemoveRegistry()
		{
			RegistryUtils.DeleteKey(RegistryStrings.RegistryBaseKeyPath + ".new", Cleaner.sThrowError);
			RegistryUtils.DeleteKey(RegistryStrings.RegistryBaseKeyPath + ".old", Cleaner.sThrowError);
			bool flag = !InstallerProperties.IsUpgrade;
			if (flag)
			{
				RegistryUtils.DeleteKey(RegistryStrings.RegistryBaseKeyPath, Cleaner.sThrowError);
			}
		}

		// Token: 0x06000021 RID: 33 RVA: 0x0000573C File Offset: 0x0000393C
		private static void RemoveDriver()
		{
			bool flag = !InstallerProperties.IsUpgrade || Cleaner.sIsCorrupt;
			if (flag)
			{
				ServiceManager.StopAndUninstallDriver("", 15);
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x0000576C File Offset: 0x0000396C
		private static void KillRunningProcesses()
		{
			bool flag = !InstallerProperties.IsUpgrade || Cleaner.sIsCorrupt;
			if (flag)
			{
				Utils.KillBlueStacksProcesses(true, "", false);
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x0000579C File Offset: 0x0000399C
		private static void CleanFolders()
		{
			List<string> list = new List<string>();
			string pfdir = InstallerProperties.PFDir;
			string pddir = InstallerProperties.PDDir;
			list.Add(string.Format("{0}.new", pfdir));
			list.Add(string.Format("{0}.new", pddir));
			list.Add(string.Format("{0}.old", pfdir));
			list.Add(string.Format("{0}.old", pddir));
			bool flag = !InstallerProperties.IsUpgrade;
			if (flag)
			{
				list.Add(pfdir);
				list.Add(pddir);
			}
			Utils.DeleteDirectory(list, Cleaner.sThrowError);
		}

		// Token: 0x04000018 RID: 24
		private static bool sThrowError = false;

		// Token: 0x04000019 RID: 25
		private static bool sIsCorrupt = false;

		// Token: 0x0400001A RID: 26
		private static long MB_MULTIPLIER = 1048576L;

		// Token: 0x0400001B RID: 27
		private static long sPFThresholdMB = 20L * Cleaner.MB_MULTIPLIER;

		// Token: 0x0400001C RID: 28
		private static long sPDThresholdMB = 900L * Cleaner.MB_MULTIPLIER;

		// Token: 0x0400001D RID: 29
		private static ManualResetEvent sLocker = new ManualResetEvent(false);
	}
}
