using System;
using System.IO;
using BlueStacks.Common;

namespace BlueStacks.Installer
{
	// Token: 0x0200001B RID: 27
	public class InstallerOpt : GetOpt
	{
		// Token: 0x0400006A RID: 106
		public bool s;

		// Token: 0x0400006B RID: 107
		public string pf = string.Empty;

		// Token: 0x0400006C RID: 108
		public string pd = string.Empty;

		// Token: 0x0400006D RID: 109
		public string md5 = string.Empty;

		// Token: 0x0400006E RID: 110
		public string parentpath = Directory.GetCurrentDirectory();

		// Token: 0x0400006F RID: 111
		public string app64 = string.Empty;

		// Token: 0x04000070 RID: 112
		public string defaultImageName = "Pie64";

		// Token: 0x04000071 RID: 113
		public string imageToLaunch = "Pie64";

		// Token: 0x04000072 RID: 114
		public string machineID = string.Empty;

		// Token: 0x04000073 RID: 115
		public string versionMachineID = string.Empty;

		// Token: 0x04000074 RID: 116
		public string pddir = string.Empty;

		// Token: 0x04000075 RID: 117
		public bool receiveEmailNotification;

		// Token: 0x04000076 RID: 118
		public bool skipSysPrep;

		// Token: 0x04000077 RID: 119
		public bool skipShortcuts;

		// Token: 0x04000078 RID: 120
		public bool skipBinaryShortcuts;

		// Token: 0x04000079 RID: 121
		public string cpu = "-1";

		// Token: 0x0400007A RID: 122
		public string ram = "-1";

		// Token: 0x0400007B RID: 123
		public string graphicEngine = string.Empty;

		// Token: 0x0400007C RID: 124
		public string graphicRenderer = string.Empty;

		// Token: 0x0400007D RID: 125
		public string deviceProfile = string.Empty;

		// Token: 0x0400007E RID: 126
		public string country = string.Empty;

		// Token: 0x0400007F RID: 127
		public int isSSE4Available = -1;

		// Token: 0x04000080 RID: 128
		public string appToLaunch = "bs5";

		// Token: 0x04000081 RID: 129
		public string bsxVersion = string.Empty;

		// Token: 0x04000082 RID: 130
		public bool isWalletFeatureEnabled;
	}
}
