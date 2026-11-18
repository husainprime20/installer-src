using System;
using System.IO;
using BlueStacks.Common;
using BstkTypeLib;

namespace BlueStacks.Installer
{
	// Token: 0x02000012 RID: 18
	internal class ImageInstaller
	{
		// Token: 0x06000051 RID: 81 RVA: 0x00006DC0 File Offset: 0x00004FC0
		private bool Init()
		{
			Logger.Info("Initializing virtualbox client");
			try
			{
				bool flag = this.mVirtualBox == null;
				if (flag)
				{
					this.mVirtualBoxClient = new VirtualBoxClientClass();
					this.mVirtualBox = this.mVirtualBoxClient.VirtualBox;
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Virtual box init failed, error: {0}", new object[]
				{
					ex
				});
				return false;
			}
			return true;
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00006E38 File Offset: 0x00005038
		public bool RegisterVm(string imagePath, string imageName)
		{
			bool flag = !this.Init();
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				try
				{
					string text = Path.Combine(imagePath, imageName + ".bstk");
					IMachine machine = this.mVirtualBox.OpenMachine(text);
					this.mVirtualBox.RegisterMachine(machine);
					Logger.Info("{0} registration successful", new object[]
					{
						imageName
					});
					return true;
				}
				catch (Exception ex)
				{
					Logger.Error("Unable to register {0}. Ex: {1}", new object[]
					{
						imageName,
						ex
					});
				}
				result = false;
			}
			return result;
		}

		// Token: 0x04000055 RID: 85
		protected IVirtualBoxClient mVirtualBoxClient;

		// Token: 0x04000056 RID: 86
		protected IVirtualBox mVirtualBox;
	}
}
