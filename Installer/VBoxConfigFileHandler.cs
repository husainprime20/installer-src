using System;
using System.IO;
using System.Xml;
using BlueStacks.Common;

namespace BlueStacks.Installer
{
	// Token: 0x02000006 RID: 6
	public class VBoxConfigFileHandler
	{
		// Token: 0x06000010 RID: 16 RVA: 0x000020A1 File Offset: 0x000002A1
		public VBoxConfigFileHandler(string oldAndroidBstkPath, string newAndroidBstkPath, string vmName, string oldAndroidBstkPrevPath, bool isSrcVdi, bool isSrcVhd)
		{
			this.mNewAndroidBstkpath = newAndroidBstkPath;
			this.mOldAndroidBstkPath = oldAndroidBstkPath;
			this.mOldAndroidBstkPrevPath = oldAndroidBstkPrevPath;
			this.mVmName = vmName;
			this.mIsOldDataDiskVdi = isSrcVdi;
			this.mIsOldDataDiskVhd = isSrcVhd;
			this.Initialization();
		}

		// Token: 0x06000011 RID: 17 RVA: 0x0000473C File Offset: 0x0000293C
		private void Initialization()
		{
			Logger.Info("Loading XMLContent from " + this.mNewAndroidBstkpath);
			string s = File.ReadAllText(this.mNewAndroidBstkpath);
			this.mNewDoc = new XmlDocument
			{
				XmlResolver = null
			};
			using (StringReader stringReader = new StringReader(s))
			{
				using (XmlReader xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings
				{
					XmlResolver = null
				}))
				{
					this.mNewDoc.Load(xmlReader);
					Logger.Info("Loading XMLContent from " + this.mOldAndroidBstkPath);
					try
					{
						this.XMLLoadOldVboxFile();
					}
					catch (Exception ex)
					{
						Logger.Warning("Some error in parsing old vbox file " + this.mOldAndroidBstkPath + ", err: " + ex.ToString());
						try
						{
							bool flag = File.Exists(this.mOldAndroidBstkPrevPath);
							if (flag)
							{
								File.Copy(this.mOldAndroidBstkPrevPath, this.mOldAndroidBstkPath, true);
								this.XMLLoadOldVboxFile();
							}
						}
						catch (Exception ex2)
						{
							Logger.Error("Unable to parse bstk file as it is corrupt err: " + ex2.ToString());
						}
					}
					this.mNsmgr = new XmlNamespaceManager(this.mNewDoc.NameTable);
					this.mNsmgr.AddNamespace("bstk", "http://www.virtualbox.org/");
				}
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x000048B4 File Offset: 0x00002AB4
		private void XMLLoadOldVboxFile()
		{
			string s = File.ReadAllText(this.mOldAndroidBstkPath);
			this.mOldDoc = new XmlDocument
			{
				XmlResolver = null
			};
			using (StringReader stringReader = new StringReader(s))
			{
				using (XmlReader xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings
				{
					XmlResolver = null
				}))
				{
					this.mOldDoc.Load(xmlReader);
				}
			}
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00004940 File Offset: 0x00002B40
		public void FixUuid(bool isDefaultVm)
		{
			try
			{
				Logger.Info("In FixUuid");
				this.mNewDoc.SelectSingleNode("descendant::bstk:Machine", this.mNsmgr).Attributes["uuid"].Value = this.mOldDoc.SelectSingleNode("descendant::bstk:Machine", this.mNsmgr).Attributes["uuid"].Value;
				string oldDataUUiD = this.GetOldDataUUiD();
				XmlNodeList xmlNodeList = this.mNewDoc.SelectNodes("descendant::bstk:Machine//bstk:MediaRegistry//bstk:HardDisks//bstk:HardDisk", this.mNsmgr);
				foreach (object obj in xmlNodeList)
				{
					XmlNode xmlNode = (XmlNode)obj;
					bool flag = xmlNode.Attributes["location"].Value.Equals("Data.vhdx", StringComparison.InvariantCultureIgnoreCase);
					if (flag)
					{
						xmlNode.Attributes["uuid"].Value = oldDataUUiD;
					}
					else
					{
						bool flag2 = !isDefaultVm;
						if (flag2)
						{
							bool flag3 = xmlNode.Attributes["location"].Value.Equals("Root.vhd", StringComparison.InvariantCultureIgnoreCase);
							if (flag3)
							{
								xmlNode.ParentNode.RemoveChild(xmlNode);
							}
							else
							{
								bool flag4 = xmlNode.Attributes["location"].Value.Equals("fastboot.vdi", StringComparison.InvariantCultureIgnoreCase);
								if (flag4)
								{
									xmlNode.ParentNode.RemoveChild(xmlNode);
								}
							}
						}
					}
				}
				XmlNodeList xmlNodeList2 = this.mNewDoc.SelectNodes("descendant::bstk:Machine//bstk:StorageControllers//bstk:StorageController", this.mNsmgr);
				foreach (object obj2 in xmlNodeList2)
				{
					XmlNode xmlNode2 = (XmlNode)obj2;
					bool flag5 = xmlNode2.Attributes["name"].Value.Equals("SATA", StringComparison.InvariantCultureIgnoreCase);
					if (flag5)
					{
						foreach (object obj3 in xmlNode2)
						{
							XmlNode xmlNode3 = (XmlNode)obj3;
							bool flag6 = xmlNode3.Attributes["port"].Value.Equals("1", StringComparison.InvariantCultureIgnoreCase);
							if (flag6)
							{
								bool flag7 = xmlNode3.FirstChild.Name == "Image";
								if (flag7)
								{
									xmlNode3.FirstChild.Attributes["uuid"].Value = oldDataUUiD;
									break;
								}
							}
						}
					}
				}
				this.mNewDoc.Save(this.mNewAndroidBstkpath);
			}
			catch (Exception ex)
			{
				Logger.Error("Unable to fix UUIDs err: " + ex.ToString());
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00004C60 File Offset: 0x00002E60
		private string GetOldDataUUiD()
		{
			XmlNodeList xmlNodeList = this.mOldDoc.SelectNodes("descendant::bstk:Machine//bstk:MediaRegistry//bstk:HardDisks//bstk:HardDisk", this.mNsmgr);
			string text = "";
			foreach (object obj in xmlNodeList)
			{
				XmlNode xmlNode = (XmlNode)obj;
				bool flag = xmlNode.Attributes["location"].Value.Equals("Data.vhdx", StringComparison.InvariantCultureIgnoreCase) || xmlNode.Attributes["location"].Value.Equals("Data.vhd", StringComparison.InvariantCultureIgnoreCase) || xmlNode.Attributes["location"].Value.Equals("Data.vdi", StringComparison.InvariantCultureIgnoreCase);
				if (flag)
				{
					text = xmlNode.Attributes["uuid"].Value;
					break;
				}
			}
			Logger.Info("Old data uuid: {0}", new object[]
			{
				text
			});
			return text;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00004D7C File Offset: 0x00002F7C
		internal void ModifyDiskForVdi()
		{
			bool flag = !this.mIsOldDataDiskVdi && !this.mIsOldDataDiskVhd;
			if (!flag)
			{
				Logger.Info("In ModifyDiskForVdi");
				XmlNodeList xmlNodeList = this.mNewDoc.SelectNodes("descendant::bstk:Machine//bstk:MediaRegistry//bstk:HardDisks//bstk:HardDisk", this.mNsmgr);
				foreach (object obj in xmlNodeList)
				{
					XmlNode xmlNode = (XmlNode)obj;
					bool flag2 = this.mIsOldDataDiskVdi;
					if (flag2)
					{
						bool flag3 = xmlNode.Attributes["location"].Value.Equals("Data.vhd", StringComparison.InvariantCultureIgnoreCase) || xmlNode.Attributes["location"].Value.Equals("Data.vhdx", StringComparison.InvariantCultureIgnoreCase);
						if (flag3)
						{
							xmlNode.Attributes["location"].Value = "Data.vdi";
							xmlNode.Attributes["format"].Value = "VDI";
						}
					}
					else
					{
						bool flag4 = this.mIsOldDataDiskVhd;
						if (flag4)
						{
							bool flag5 = xmlNode.Attributes["location"].Value.Equals("Data.vhdx", StringComparison.InvariantCultureIgnoreCase);
							if (flag5)
							{
								xmlNode.Attributes["location"].Value = "Data.vhd";
								xmlNode.Attributes["format"].Value = "VHD";
							}
						}
					}
				}
				this.mNewDoc.Save(this.mNewAndroidBstkpath);
			}
		}

		// Token: 0x04000005 RID: 5
		private string mNewAndroidBstkpath;

		// Token: 0x04000006 RID: 6
		private string mOldAndroidBstkPath;

		// Token: 0x04000007 RID: 7
		private string mOldAndroidBstkPrevPath;

		// Token: 0x04000008 RID: 8
		private string mVmName;

		// Token: 0x04000009 RID: 9
		private bool mIsOldDataDiskVdi;

		// Token: 0x0400000A RID: 10
		private bool mIsOldDataDiskVhd;

		// Token: 0x0400000B RID: 11
		private XmlDocument mNewDoc;

		// Token: 0x0400000C RID: 12
		private XmlDocument mOldDoc;

		// Token: 0x0400000D RID: 13
		private XmlNamespaceManager mNsmgr;

		// Token: 0x0400000E RID: 14
		private const string ROOT_VDI_UUID = "{fca296ce-8268-4ed7-a57f-d32ec11ab304}";
	}
}
