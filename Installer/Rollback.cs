using System;
using System.Collections.Generic;
using System.IO;
using BlueStacks.Common;

namespace BlueStacks.Installer
{
	// Token: 0x0200001A RID: 26
	public class Rollback
	{
		// Token: 0x060000C7 RID: 199 RVA: 0x0000C83C File Offset: 0x0000AA3C
		public static int RollbackFresh()
		{
			Logger.Info("Fresh installation failed. Starting rollback.");
			bool flag = false;
			try
			{
				Logger.Info("Deleting all folders...");
				Rollback.DeleteAllFolders();
			}
			catch (Exception ex)
			{
				flag = true;
				InstallerStats.InstallerComment |= InstallerComments.FAILED_TO_DELETE_NEW_DATA;
				Logger.Warning("Failed to delete folders. ex: " + ex.ToString());
			}
			try
			{
				Logger.Info("Deleting registered services...");
				ServiceManager.StopAndUninstallDriver("", 15);
			}
			catch (Exception ex2)
			{
				flag = true;
				InstallerStats.InstallerComment |= InstallerComments.FAILED_TO_DELETE_NEW_SERVICE;
				Logger.Warning("failed to delete services. ex: " + ex2.ToString());
			}
			try
			{
				Logger.Info("Deleting registry hive");
				string path = Path.Combine("SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting", "LocalDumps");
				string[] array = new string[]
				{
					InstallerProperties.RegistryBasePath,
					Path.Combine(path, "HD-Player.exe"),
					Path.Combine(path, "HD-MultiInstanceManager.exe")
				};
				foreach (string hklmPath in array)
				{
					RegistryUtils.DeleteKey(hklmPath, false);
				}
				RegistryUtils.DeleteValue(Path.Combine("SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting", "RuntimeExceptionHelperModules"), Path.Combine(InstallerProperties.PFDir, "HD-WerHandler.dll"), false);
			}
			catch (Exception ex3)
			{
				flag = true;
				InstallerStats.InstallerComment |= InstallerComments.FAILED_TO_DELETE_NEW_REGISTRY;
				Logger.Warning("Failed to delete registry entry. Ex: {0}", new object[]
				{
					ex3
				});
			}
			Rollback.DeleteImages();
			bool flag2 = flag;
			int result;
			if (flag2)
			{
				Logger.Error("Rollback failed. The system may not be in a clean state.");
				result = -52;
			}
			else
			{
				Logger.Info("Rollback was successful.");
				result = 0;
			}
			return result;
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x0000CA08 File Offset: 0x0000AC08
		public static int RollbackUpgrade()
		{
			Logger.Info("Upgrade failed. Starting rollback.");
			Rollback.DeleteImages();
			int num = Rollback.RollbackDirectories();
			bool flag = num != 0;
			int result;
			if (flag)
			{
				result = num;
			}
			else
			{
				result = 0;
			}
			return result;
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x0000CA40 File Offset: 0x0000AC40
		private static void DeleteImages()
		{
			Logger.Info("Deleting Images");
			bool flag = InstallerProperties.ImageList != null;
			if (flag)
			{
				foreach (string imageName in InstallerProperties.ImageList)
				{
					try
					{
						string guestZipPath = MicroInstallerSharedUtils.GetGuestZipPath(imageName, InstallerProperties.PDDir);
						bool flag2 = File.Exists(guestZipPath);
						if (flag2)
						{
							File.Delete(guestZipPath);
						}
					}
					catch
					{
					}
				}
			}
		}

		// Token: 0x060000CA RID: 202 RVA: 0x0000CAE0 File Offset: 0x0000ACE0
		public static void DeleteAllFolders()
		{
			List<string> directories = new List<string>
			{
				InstallerProperties.PDDirNew,
				InstallerProperties.PFDirNew
			};
			Utils.DeleteDirectory(directories, false);
		}

		// Token: 0x060000CB RID: 203 RVA: 0x0000CB14 File Offset: 0x0000AD14
		public static int RollbackDirectories()
		{
			try
			{
				Logger.Info("Rolling back ProgramData");
				string pddir = InstallerProperties.PDDir;
				string pddirNew = InstallerProperties.PDDirNew;
				string text = InstallerProperties.PDDirExisting + ".old";
				bool flag = Directory.Exists(pddirNew);
				if (flag)
				{
					Utils.DeleteDirectory(pddirNew);
				}
				bool flag2 = Directory.Exists(text);
				if (flag2)
				{
					bool flag3 = Directory.Exists(pddir);
					if (flag3)
					{
						Utils.DeleteDirectory(pddir);
					}
					InstallerUtils.MoveDirectory(text, InstallerProperties.PDDirExisting);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Unable to rollback PD folders, ex: " + ex.ToString());
				return -55;
			}
			try
			{
				Logger.Info("Rolling back ProgramFiles");
				string pfdir = InstallerProperties.PFDir;
				string pfdirNew = InstallerProperties.PFDirNew;
				string text2 = InstallerProperties.PFDirExisting + ".old";
				bool flag4 = Directory.Exists(pfdirNew);
				if (flag4)
				{
					Utils.DeleteDirectory(pfdirNew);
				}
				bool flag5 = Directory.Exists(text2);
				if (flag5)
				{
					bool flag6 = Directory.Exists(pfdir);
					if (flag6)
					{
						Utils.DeleteDirectory(pfdir);
					}
					InstallerUtils.MoveDirectory(text2, InstallerProperties.PFDirExisting);
				}
			}
			catch (Exception ex2)
			{
				Logger.Error("Unable to rollback PF folders, ex: " + ex2.ToString());
				return -54;
			}
			return 0;
		}
	}
}
