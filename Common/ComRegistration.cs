using System;
using System.IO;

namespace BlueStacks.Common
{
	// Token: 0x02000002 RID: 2
	public class ComRegistration
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public ComRegistration(string dir)
		{
			this.mBinPath = Path.Combine(dir, "HD-ComRegistrar.exe");
		}

		// Token: 0x06000002 RID: 2 RVA: 0x0000312C File Offset: 0x0000132C
		public int Register()
		{
			Logger.Info("Registering COM components");
			return RunCommand.RunCmd(this.mBinPath, "-reg", true, false, 0).ExitCode;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00003164 File Offset: 0x00001364
		public int Unregister()
		{
			Logger.Info("Unregistering COM components");
			return RunCommand.RunCmd(this.mBinPath, "-unreg", true, false, 0).ExitCode;
		}

		// Token: 0x04000001 RID: 1
		private string mBinPath = "HD-ComRegistrar.exe";
	}
}
