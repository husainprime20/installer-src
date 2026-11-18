using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using BlueStacks.Common;

namespace BlueStacks.Installer
{
	// Token: 0x0200000B RID: 11
	public class ImageBasedUpgradeManager
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600002B RID: 43 RVA: 0x00002126 File Offset: 0x00000326
		// (set) Token: 0x0600002C RID: 44 RVA: 0x0000212E File Offset: 0x0000032E
		public string SrcDataDir { get; set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600002D RID: 45 RVA: 0x00002137 File Offset: 0x00000337
		// (set) Token: 0x0600002E RID: 46 RVA: 0x0000213F File Offset: 0x0000033F
		public string DestDataDir { get; set; }

		// Token: 0x0600002F RID: 47 RVA: 0x00005A20 File Offset: 0x00003C20
		public ImageBasedUpgradeManager()
		{
			this.SrcDataDir = Path.Combine(InstallerProperties.PDDirExisting, "Engine");
			this.DestDataDir = Path.Combine(InstallerProperties.PDDirNew, "Engine");
			foreach (ImageTraits imageTraits in ImageTraits.GetAll())
			{
				this.mRestrictedFilesList.Add(string.Format("oem_{0}.cfg", imageTraits.Image));
			}
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00005B14 File Offset: 0x00003D14
		public int HandleEngineDataUpgrade()
		{
			Logger.Info("HandleEngineDataUpgrade for ImageBased builds start");
			bool flag = !this.CopyVBoxManagerFile();
			int result;
			if (flag)
			{
				result = -61;
			}
			else
			{
				bool flag2 = this.RestoreOldAndroidImageData();
				bool flag3 = !flag2;
				if (flag3)
				{
					result = -46;
				}
				else
				{
					this.RestoreOldUserData();
					this.RestoreOldLogs();
					result = 0;
				}
			}
			return result;
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00005B68 File Offset: 0x00003D68
		private void RestoreOldLogs()
		{
			string oldLogsDir = InstallerProperties.OldLogsDir;
			string destLogsDir = Path.Combine(InstallerProperties.PDDirNew, "Logs");
			InstallerUtils.RestoreOldLogs(oldLogsDir, destLogsDir, InstallerProperties.OldVersion);
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00005B9C File Offset: 0x00003D9C
		private void RestoreOldUserData()
		{
			string path = Path.Combine(this.SrcDataDir, "UserData");
			string path2 = Path.Combine(this.DestDataDir, "UserData");
			try
			{
				string srcPath = Path.Combine(path, "SharedFolder");
				string dstPath = Path.Combine(path2, "SharedFolder");
				Utils.CopyRecursive(srcPath, dstPath, true);
				string path3 = Path.Combine(path, "InputMapper");
				string path4 = Path.Combine(path2, "InputMapper");
				foreach (string text in Directory.GetFiles(path3))
				{
					try
					{
						bool flag = Path.GetExtension(text) == ".cfg";
						if (flag)
						{
							File.Copy(text, Path.Combine(path4, Path.GetFileName(text)), false);
						}
					}
					catch
					{
					}
				}
				string srcPath2 = Path.Combine(path, "InputMapper\\UserFiles");
				string dstPath2 = Path.Combine(path2, "InputMapper\\UserFiles");
				Utils.CopyRecursive(srcPath2, dstPath2, true);
				string text2 = Path.Combine(path, "InputMapper\\Crosshair");
				bool flag2 = Directory.Exists(text2);
				if (flag2)
				{
					string dstPath3 = Path.Combine(path2, "InputMapper\\Crosshair");
					Utils.CopyRecursive(text2, dstPath3, true);
				}
				string text3 = Path.Combine(path, "InputMapper\\UserScripts");
				bool flag3 = Directory.Exists(text3);
				if (flag3)
				{
					string dstPath4 = Path.Combine(path2, "InputMapper\\UserScripts");
					Utils.CopyRecursive(text3, dstPath4, true);
				}
				string text4 = Path.Combine(path, "MimMetaData.json");
				string destFileName = Path.Combine(path2, "MimMetaData.json");
				bool flag4 = File.Exists(text4);
				if (flag4)
				{
					File.Copy(text4, destFileName, true);
				}
			}
			catch (Exception ex)
			{
				InstallerStats.InstallerComment |= InstallerComments.FAILED_TO_RESTORE_GADGET_DATA;
				Logger.Warning(ex.ToString());
			}
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00005D80 File Offset: 0x00003F80
		private bool RestoreOldAndroidImageData()
		{
			Logger.Info("In image upgrade method RestoreOldAndroidImageData");
			try
			{
				List<string> list = Directory.GetDirectories(this.SrcDataDir).ToList<string>();
				foreach (string text in list)
				{
					Logger.Info("Path in engine folder list: " + text);
					DirectoryInfo directoryInfo = new DirectoryInfo(text);
					string name = directoryInfo.Name;
					bool flag = !InstallerProperties.VmList.Contains(name);
					if (!flag)
					{
						string srcVmPath = Path.Combine(this.SrcDataDir, name);
						string destVmPath = Path.Combine(this.DestDataDir, name);
						foreach (ImageTraits imageTraits in ImageTraits.GetAll())
						{
							bool flag2 = name.StartsWith(imageTraits.Image);
							if (flag2)
							{
								bool isDefaultVm = false;
								bool flag3 = name == imageTraits.Image;
								if (flag3)
								{
									isDefaultVm = true;
								}
								this.CopyAndroidImageData(name, srcVmPath, destVmPath, isDefaultVm);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Error in copying instance data err: " + ex.ToString());
				return false;
			}
			return true;
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00005F20 File Offset: 0x00004120
		private string GetDataFormatFromBstkFile(XmlDocument doc)
		{
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(doc.NameTable);
			xmlNamespaceManager.AddNamespace("bstk", "http://www.virtualbox.org/");
			XmlNodeList xmlNodeList = doc.SelectNodes("descendant::bstk:Machine//bstk:MediaRegistry//bstk:HardDisks//bstk:HardDisk", xmlNamespaceManager);
			foreach (object obj in xmlNodeList)
			{
				XmlNode xmlNode = (XmlNode)obj;
				bool flag = xmlNode.Attributes["location"].Value.Equals("Data.vhd", StringComparison.InvariantCultureIgnoreCase) || xmlNode.Attributes["location"].Value.Equals("Data.vdi", StringComparison.InvariantCultureIgnoreCase);
				if (flag)
				{
					return xmlNode.Attributes["format"].Value;
				}
			}
			return "";
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00006010 File Offset: 0x00004210
		private XmlDocument ReadBStkFile(string filePath)
		{
			string s = File.ReadAllText(filePath);
			XmlDocument xmlDocument = new XmlDocument
			{
				XmlResolver = null
			};
			XmlDocument result;
			using (StringReader stringReader = new StringReader(s))
			{
				using (XmlReader xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings
				{
					XmlResolver = null
				}))
				{
					xmlDocument.Load(xmlReader);
					result = xmlDocument;
				}
			}
			return result;
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00006090 File Offset: 0x00004290
		private void CopyAndroidImageData(string folderName, string srcVmPath, string destVmPath, bool isDefaultVm)
		{
			Logger.Info("In CopyAndroidImageData: {0} src:{1} dest:{2}", new object[]
			{
				folderName,
				srcVmPath,
				destVmPath
			});
			bool flag = !Directory.Exists(destVmPath);
			if (flag)
			{
				Directory.CreateDirectory(destVmPath);
			}
			ImageBasedUpgradeManager.CopyAppCachedata(srcVmPath, destVmPath);
			List<string> list = Directory.GetFiles(srcVmPath).ToList<string>();
			string text = Path.Combine(srcVmPath, string.Format(ImageBasedUpgradeManager.sAndroidBstkFileName, folderName));
			string text2 = Path.Combine(srcVmPath, string.Format(ImageBasedUpgradeManager.sAndroidBstkPrevFileName, folderName));
			XmlDocument xmlDocument = null;
			try
			{
				xmlDocument = this.ReadBStkFile(text);
			}
			catch (Exception ex)
			{
				Logger.Warning("Some error in parsing old vbox file " + text + ", err: " + ex.ToString());
				try
				{
					bool flag2 = File.Exists(text2);
					if (flag2)
					{
						File.Copy(text2, text, true);
						xmlDocument = this.ReadBStkFile(text);
					}
				}
				catch (Exception ex2)
				{
					Logger.Error("Unable to parse bstk file as it is corrupt err: " + ex2.ToString());
				}
			}
			bool flag3 = xmlDocument == null;
			if (!flag3)
			{
				bool flag4 = false;
				bool flag5 = false;
				bool flag6 = list.Contains(Path.Combine(srcVmPath, "Data.vdi"), StringComparer.InvariantCultureIgnoreCase);
				if (flag6)
				{
					Logger.Info("Old data disk is extn : vdi, format : vdi");
					flag4 = true;
				}
				else
				{
					bool flag7 = string.Equals(this.GetDataFormatFromBstkFile(xmlDocument), "VDI", StringComparison.InvariantCultureIgnoreCase);
					if (flag7)
					{
						Logger.Info("Old data disk is extn : vhd, format : vdi");
						flag4 = true;
						bool flag8 = !InstallerUtils.CreateHardLinkForFile(Path.Combine(srcVmPath, "Data.vhd"), Path.Combine(destVmPath, "Data.vdi"));
						if (flag8)
						{
							throw new Exception("Failed to create hard link for " + Path.Combine(srcVmPath, "Data.vhd"));
						}
						list.Remove(Path.Combine(srcVmPath, "Data.vhd"));
						bool flag9 = isDefaultVm && File.Exists(Path.Combine(srcVmPath, "Data_orig.vhd"));
						if (flag9)
						{
							bool flag10 = !InstallerUtils.CreateHardLinkForFile(Path.Combine(srcVmPath, "Data_orig.vhd"), Path.Combine(destVmPath, "Data_orig.vdi"));
							if (flag10)
							{
								throw new Exception("Failed to create hard link for " + Path.Combine(srcVmPath, "Data_orig.vhd"));
							}
							list.Remove(Path.Combine(srcVmPath, "Data_orig.vhd"));
						}
					}
					else
					{
						bool flag11 = list.Contains(Path.Combine(srcVmPath, "Data.vhd"), StringComparer.InvariantCultureIgnoreCase);
						if (flag11)
						{
							Logger.Info("Old data disk is extn : vhd, format : vhd");
							flag5 = true;
						}
					}
				}
				bool flag12 = flag4 && File.Exists(Path.Combine(destVmPath, "Data.vhd"));
				if (flag12)
				{
					File.Delete(Path.Combine(destVmPath, "Data.vhd"));
				}
				else
				{
					bool flag13 = flag4 && File.Exists(Path.Combine(destVmPath, "Data.vhdx"));
					if (flag13)
					{
						File.Delete(Path.Combine(destVmPath, "Data.vhdx"));
					}
					else
					{
						bool flag14 = flag5 && File.Exists(Path.Combine(destVmPath, "Data.vhdx"));
						if (flag14)
						{
							File.Delete(Path.Combine(destVmPath, "Data.vhdx"));
						}
					}
				}
				foreach (string text3 in list)
				{
					string fileName = Path.GetFileName(text3);
					bool flag15 = fileName == string.Format(ImageBasedUpgradeManager.sAndroidBstkFileName, folderName);
					if (flag15)
					{
						ImageBasedUpgradeManager.HandleAndroidBstkFile(srcVmPath, destVmPath, fileName, folderName, isDefaultVm, flag4, flag5);
					}
					else
					{
						bool flag16 = fileName.Contains(".bstk-prev");
						if (!flag16)
						{
							bool flag17 = this.mRestrictedFilesList.Contains(fileName);
							if (!flag17)
							{
								string text4 = Path.Combine(destVmPath, fileName);
								bool flag18 = File.Exists(text4);
								if (flag18)
								{
									File.Delete(text4);
								}
								bool flag19 = !InstallerUtils.CreateHardLinkForFile(text3, text4);
								if (flag19)
								{
									throw new Exception("Failed to create hard link for " + text3);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00006470 File Offset: 0x00004670
		private static void HandleAndroidBstkFile(string srcFolder, string destFolder, string fileName, string vmName, bool isDefaultVm, bool isOldDataDiskVdi, bool isOldDataDiskVhd)
		{
			Logger.Info("In HandleAndroidBSTKFile for " + vmName);
			string oldAndroidBstkPath = Path.Combine(srcFolder, fileName);
			string text = Path.Combine(destFolder, fileName);
			try
			{
				string path = string.Format(ImageBasedUpgradeManager.sAndroidBstkPrevFileName, vmName);
				string oldAndroidBstkPrevPath = Path.Combine(srcFolder, path);
				VBoxConfigFileHandler vboxConfigFileHandler = new VBoxConfigFileHandler(oldAndroidBstkPath, text, vmName, oldAndroidBstkPrevPath, isOldDataDiskVdi, isOldDataDiskVhd);
				vboxConfigFileHandler.FixUuid(isDefaultVm);
				vboxConfigFileHandler.ModifyDiskForVdi();
			}
			catch (Exception ex)
			{
				Logger.Error("An error occured while fixing Android.bstk");
				Logger.Error(ex.ToString());
				throw new Exception("Failed to fix paths in " + text);
			}
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00006514 File Offset: 0x00004714
		private static void CopyAppCachedata(string srcVmPath, string destVmPath)
		{
			Logger.Info("In CopyAppCachedata");
			string text = Path.Combine(srcVmPath, "AppCache");
			string dstPath = Path.Combine(destVmPath, "AppCache");
			bool flag = Directory.Exists(text);
			if (flag)
			{
				Utils.CopyRecursive(text, dstPath, true);
			}
		}

		// Token: 0x06000039 RID: 57 RVA: 0x0000655C File Offset: 0x0000475C
		private bool CopyVBoxManagerFile()
		{
			Logger.Info("In CopyVBoxManagerFile");
			string destFileName = Path.Combine(this.DestDataDir, "Manager\\BstkGlobal.xml");
			string text = Path.Combine(this.SrcDataDir, "Manager\\BstkGlobal.xml");
			bool flag = !File.Exists(text);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				File.Copy(text, destFileName, true);
				result = true;
			}
			return result;
		}

		// Token: 0x0400002A RID: 42
		private static string sAndroidBstkFileName = "{0}.bstk";

		// Token: 0x0400002B RID: 43
		private static string sAndroidBstkPrevFileName = "{0}.bstk-prev";

		// Token: 0x0400002C RID: 44
		private List<string> mRestrictedFilesList = new List<string>
		{
			"fastboot.vdi",
			"Root.vdi",
			"Android.bstk.in",
			"Root.vhd",
			"bzImage_hyperv",
			"initrd_hyperv.img"
		};
	}
}
