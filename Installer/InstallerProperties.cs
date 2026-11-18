using System;
using System.Collections.Generic;
using System.IO;
using BlueStacks.Common;

namespace BlueStacks.Installer
{
	// Token: 0x0200001E RID: 30
	public class InstallerProperties
	{
		// Token: 0x17000004 RID: 4
		// (get) Token: 0x060000F2 RID: 242 RVA: 0x000023AD File Offset: 0x000005AD
		// (set) Token: 0x060000F3 RID: 243 RVA: 0x000023B4 File Offset: 0x000005B4
		public static string PFDir { get; set; } = "";

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x060000F4 RID: 244 RVA: 0x000023BC File Offset: 0x000005BC
		// (set) Token: 0x060000F5 RID: 245 RVA: 0x000023C3 File Offset: 0x000005C3
		public static string PDDir { get; set; } = "";

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x060000F6 RID: 246 RVA: 0x000023CB File Offset: 0x000005CB
		// (set) Token: 0x060000F7 RID: 247 RVA: 0x000023D2 File Offset: 0x000005D2
		public static string PFDirNew { get; set; } = "";

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x060000F8 RID: 248 RVA: 0x000023DA File Offset: 0x000005DA
		// (set) Token: 0x060000F9 RID: 249 RVA: 0x000023E1 File Offset: 0x000005E1
		public static string PDDirNew { get; set; } = "";

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x060000FA RID: 250 RVA: 0x000023E9 File Offset: 0x000005E9
		// (set) Token: 0x060000FB RID: 251 RVA: 0x000023F0 File Offset: 0x000005F0
		public static string PDDirOld { get; set; } = "";

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x060000FC RID: 252 RVA: 0x000023F8 File Offset: 0x000005F8
		// (set) Token: 0x060000FD RID: 253 RVA: 0x000023FF File Offset: 0x000005FF
		public static string PFDirOld { get; set; } = "";

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x060000FE RID: 254 RVA: 0x00002407 File Offset: 0x00000607
		// (set) Token: 0x060000FF RID: 255 RVA: 0x0000240E File Offset: 0x0000060E
		public static string PDDirExisting { get; set; } = "";

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000100 RID: 256 RVA: 0x00002416 File Offset: 0x00000616
		// (set) Token: 0x06000101 RID: 257 RVA: 0x0000241D File Offset: 0x0000061D
		public static string PFDirExisting { get; set; } = "";

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000102 RID: 258 RVA: 0x00002425 File Offset: 0x00000625
		// (set) Token: 0x06000103 RID: 259 RVA: 0x0000242C File Offset: 0x0000062C
		public static string InstallerExtractedDir { get; set; } = "";

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000104 RID: 260 RVA: 0x00002434 File Offset: 0x00000634
		// (set) Token: 0x06000105 RID: 261 RVA: 0x0000243B File Offset: 0x0000063B
		public static string DefaultVmName { get; set; } = "";

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000106 RID: 262 RVA: 0x00002443 File Offset: 0x00000643
		// (set) Token: 0x06000107 RID: 263 RVA: 0x0000244A File Offset: 0x0000064A
		public static string VmToLaunch { get; set; } = "";

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000108 RID: 264 RVA: 0x0000E500 File Offset: 0x0000C700
		public static string RegistryBasePath
		{
			get
			{
				bool flag = string.IsNullOrEmpty(InstallerProperties.sRegistryBasePath);
				if (flag)
				{
					InstallerProperties.sRegistryBasePath = Path.Combine("Software", "BlueStacks" + InstallerProperties.RegistrySuffix);
				}
				return InstallerProperties.sRegistryBasePath;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000109 RID: 265 RVA: 0x00002452 File Offset: 0x00000652
		// (set) Token: 0x0600010A RID: 266 RVA: 0x00002459 File Offset: 0x00000659
		public static string OldLogsDir { get; set; } = "";

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600010B RID: 267 RVA: 0x00002461 File Offset: 0x00000661
		// (set) Token: 0x0600010C RID: 268 RVA: 0x00002468 File Offset: 0x00000668
		public static string CampaignMD5 { get; set; } = "";

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600010D RID: 269 RVA: 0x00002470 File Offset: 0x00000670
		// (set) Token: 0x0600010E RID: 270 RVA: 0x00002477 File Offset: 0x00000677
		public static bool IsFreshInstall { get; private set; } = true;

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600010F RID: 271 RVA: 0x0000247F File Offset: 0x0000067F
		// (set) Token: 0x06000110 RID: 272 RVA: 0x00002486 File Offset: 0x00000686
		public static bool IsUpgrade
		{
			get
			{
				return InstallerProperties.mIsUpgrade;
			}
			set
			{
				InstallerProperties.mIsUpgrade = value;
				InstallerProperties.IsFreshInstall = !value;
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000111 RID: 273 RVA: 0x00002499 File Offset: 0x00000699
		// (set) Token: 0x06000112 RID: 274 RVA: 0x000024A0 File Offset: 0x000006A0
		public static bool PreferDedicatedGPU { get; set; } = false;

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000113 RID: 275 RVA: 0x000024A8 File Offset: 0x000006A8
		// (set) Token: 0x06000114 RID: 276 RVA: 0x000024AF File Offset: 0x000006AF
		public static string GUID { get; set; } = "";

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000115 RID: 277 RVA: 0x000024B7 File Offset: 0x000006B7
		// (set) Token: 0x06000116 RID: 278 RVA: 0x000024BE File Offset: 0x000006BE
		public static string CampaignName { get; set; } = "";

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000117 RID: 279 RVA: 0x000024C6 File Offset: 0x000006C6
		// (set) Token: 0x06000118 RID: 280 RVA: 0x000024CD File Offset: 0x000006CD
		public static string CampaignJson { get; set; } = "";

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000119 RID: 281 RVA: 0x000024D5 File Offset: 0x000006D5
		// (set) Token: 0x0600011A RID: 282 RVA: 0x000024DC File Offset: 0x000006DC
		public static string OldGUID { get; set; } = "";

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x0600011B RID: 283 RVA: 0x000024E4 File Offset: 0x000006E4
		// (set) Token: 0x0600011C RID: 284 RVA: 0x000024EB File Offset: 0x000006EB
		public static bool InSilentMode { get; set; }

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600011D RID: 285 RVA: 0x000024F3 File Offset: 0x000006F3
		// (set) Token: 0x0600011E RID: 286 RVA: 0x000024FA File Offset: 0x000006FA
		public static string CloudHost { get; set; } = "https://cloud.bluestacks.com";

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x0600011F RID: 287 RVA: 0x00002502 File Offset: 0x00000702
		// (set) Token: 0x06000120 RID: 288 RVA: 0x00002509 File Offset: 0x00000709
		public static int GLMode { get; set; } = -1;

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000121 RID: 289 RVA: 0x00002511 File Offset: 0x00000711
		// (set) Token: 0x06000122 RID: 290 RVA: 0x00002518 File Offset: 0x00000718
		public static int GLRenderMode { get; set; } = -1;

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000123 RID: 291 RVA: 0x00002520 File Offset: 0x00000720
		// (set) Token: 0x06000124 RID: 292 RVA: 0x00002527 File Offset: 0x00000727
		public static string GLRenderer { get; set; } = "";

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000125 RID: 293 RVA: 0x0000252F File Offset: 0x0000072F
		// (set) Token: 0x06000126 RID: 294 RVA: 0x00002536 File Offset: 0x00000736
		public static string GLVendor { get; set; } = "";

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000127 RID: 295 RVA: 0x0000253E File Offset: 0x0000073E
		// (set) Token: 0x06000128 RID: 296 RVA: 0x00002545 File Offset: 0x00000745
		public static string GLVersion { get; set; } = "";

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000129 RID: 297 RVA: 0x0000254D File Offset: 0x0000074D
		// (set) Token: 0x0600012A RID: 298 RVA: 0x00002554 File Offset: 0x00000754
		public static List<string> VmList { get; set; } = null;

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600012B RID: 299 RVA: 0x0000255C File Offset: 0x0000075C
		// (set) Token: 0x0600012C RID: 300 RVA: 0x00002563 File Offset: 0x00000763
		public static List<string> ImageList { get; set; } = null;

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600012D RID: 301 RVA: 0x0000256B File Offset: 0x0000076B
		// (set) Token: 0x0600012E RID: 302 RVA: 0x00002572 File Offset: 0x00000772
		public static bool SameOrHigherVersionInstalled { get; set; } = false;

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x0600012F RID: 303 RVA: 0x0000257A File Offset: 0x0000077A
		// (set) Token: 0x06000130 RID: 304 RVA: 0x00002581 File Offset: 0x00000781
		public static string OldVersion { get; set; } = string.Empty;

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x06000131 RID: 305 RVA: 0x00002589 File Offset: 0x00000789
		// (set) Token: 0x06000132 RID: 306 RVA: 0x00002590 File Offset: 0x00000790
		public static string CurrentLocale { get; set; } = "en-US";

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x06000133 RID: 307 RVA: 0x00002598 File Offset: 0x00000798
		// (set) Token: 0x06000134 RID: 308 RVA: 0x0000259F File Offset: 0x0000079F
		public static string InstallId { get; set; } = string.Empty;

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000135 RID: 309 RVA: 0x000025A7 File Offset: 0x000007A7
		// (set) Token: 0x06000136 RID: 310 RVA: 0x000025AE File Offset: 0x000007AE
		public static string InstallerArchitecture { get; set; } = InstallerArchitectures.AMD64;

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000137 RID: 311 RVA: 0x000025B6 File Offset: 0x000007B6
		// (set) Token: 0x06000138 RID: 312 RVA: 0x000025BD File Offset: 0x000007BD
		public static string MainInstallerFilePath { get; set; } = string.Empty;

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000139 RID: 313 RVA: 0x000025C5 File Offset: 0x000007C5
		// (set) Token: 0x0600013A RID: 314 RVA: 0x000025CC File Offset: 0x000007CC
		public static string MainInstallerFolderPath { get; set; } = string.Empty;

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x0600013B RID: 315 RVA: 0x000025D4 File Offset: 0x000007D4
		// (set) Token: 0x0600013C RID: 316 RVA: 0x000025DB File Offset: 0x000007DB
		public static string OldInstallId { get; set; } = string.Empty;

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x0600013D RID: 317 RVA: 0x000025E3 File Offset: 0x000007E3
		// (set) Token: 0x0600013E RID: 318 RVA: 0x000025EA File Offset: 0x000007EA
		public static string FLEAssetsDir { get; set; } = Path.Combine(Path.GetTempPath(), "Assets");

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x0600013F RID: 319 RVA: 0x000025F2 File Offset: 0x000007F2
		// (set) Token: 0x06000140 RID: 320 RVA: 0x000025F9 File Offset: 0x000007F9
		public static bool UpgradeFromUnificationBuild { get; set; } = true;

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000141 RID: 321 RVA: 0x00002601 File Offset: 0x00000801
		// (set) Token: 0x06000142 RID: 322 RVA: 0x00002608 File Offset: 0x00000808
		public static List<GlProperty> GlCheckOrderList { get; set; } = new List<GlProperty>();

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000143 RID: 323 RVA: 0x00002610 File Offset: 0x00000810
		public static List<GlProperty> FallbackGlCheckOrderList { get; } = new List<GlProperty>
		{
			new GlProperty
			{
				Gl_mode = BlueStacks.Common.GLMode.AGA,
				Gl_renderer = BlueStacks.Common.GLRenderer.OpenGL
			},
			new GlProperty
			{
				Gl_mode = BlueStacks.Common.GLMode.AGA,
				Gl_renderer = BlueStacks.Common.GLRenderer.DX11FallbackDX9
			},
			new GlProperty
			{
				Gl_mode = BlueStacks.Common.GLMode.AGA,
				Gl_renderer = BlueStacks.Common.GLRenderer.DX9
			},
			new GlProperty
			{
				Gl_mode = BlueStacks.Common.GLMode.PGA,
				Gl_renderer = BlueStacks.Common.GLRenderer.OpenGL
			},
			new GlProperty
			{
				Gl_mode = BlueStacks.Common.GLMode.PGA,
				Gl_renderer = BlueStacks.Common.GLRenderer.DX11FallbackDX9
			},
			new GlProperty
			{
				Gl_mode = BlueStacks.Common.GLMode.PGA,
				Gl_renderer = BlueStacks.Common.GLRenderer.DX9
			}
		};

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x06000144 RID: 324 RVA: 0x00002617 File Offset: 0x00000817
		// (set) Token: 0x06000145 RID: 325 RVA: 0x0000261E File Offset: 0x0000081E
		public static bool IsGlCheckOrderResponseCame { get; set; } = false;

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x06000146 RID: 326 RVA: 0x00002626 File Offset: 0x00000826
		// (set) Token: 0x06000147 RID: 327 RVA: 0x0000262D File Offset: 0x0000082D
		public static bool StartedFromMicroInstaller { get; internal set; } = false;

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x06000148 RID: 328 RVA: 0x00002635 File Offset: 0x00000835
		// (set) Token: 0x06000149 RID: 329 RVA: 0x0000263C File Offset: 0x0000083C
		public static bool VCDependenciesExtracted { get; internal set; } = false;

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x0600014A RID: 330 RVA: 0x0000E548 File Offset: 0x0000C748
		public static bool IsMicrosoftHyperVEnabled
		{
			get
			{
				return string.Equals(InstallerProperties.CurrentHypervisor, Strings.HypervisorConfMap[Hypervisors.MicrosoftHv], StringComparison.InvariantCultureIgnoreCase);
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x0600014B RID: 331 RVA: 0x0000E570 File Offset: 0x0000C770
		// (set) Token: 0x0600014C RID: 332 RVA: 0x00002644 File Offset: 0x00000844
		public static string CurrentHypervisor
		{
			get
			{
				bool flag = InstallerProperties.sCurrentHypervisor == null;
				string result;
				if (flag)
				{
					result = "";
				}
				else
				{
					result = InstallerProperties.sCurrentHypervisor;
				}
				return result;
			}
			set
			{
				InstallerProperties.sCurrentHypervisor = value;
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x0600014D RID: 333 RVA: 0x0000264D File Offset: 0x0000084D
		// (set) Token: 0x0600014E RID: 334 RVA: 0x00002654 File Offset: 0x00000854
		public static int IsSSE4Available { get; set; } = -1;

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x0600014F RID: 335 RVA: 0x0000265C File Offset: 0x0000085C
		// (set) Token: 0x06000150 RID: 336 RVA: 0x00002663 File Offset: 0x00000863
		public static string AppToLaunch { get; set; } = "bs5";

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000151 RID: 337 RVA: 0x0000E59C File Offset: 0x0000C79C
		// (set) Token: 0x06000152 RID: 338 RVA: 0x0000266B File Offset: 0x0000086B
		public static string BsxVersion
		{
			get
			{
				return InstallerProperties.sBsxVersion;
			}
			set
			{
				InstallerProperties.sBsxVersion = value;
			}
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000153 RID: 339 RVA: 0x0000E5B4 File Offset: 0x0000C7B4
		public static string InstallerLogFilePath
		{
			get
			{
				bool flag = InstallerProperties.sInstallerLogFilePath == null;
				if (flag)
				{
					string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
					path = Path.Combine(path, "Bluestacks");
					InstallerProperties.sInstallerLogFilePath = Path.Combine(path, string.Format("BlueStacks-Installer_{0}.log", "5.12.120.6303"));
				}
				return InstallerProperties.sInstallerLogFilePath;
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x06000154 RID: 340 RVA: 0x00002674 File Offset: 0x00000874
		// (set) Token: 0x06000155 RID: 341 RVA: 0x0000267B File Offset: 0x0000087B
		public static MiFailureCodes FailureCodes { get; set; } = MiFailureCodes.Success;

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000156 RID: 342 RVA: 0x00002683 File Offset: 0x00000883
		// (set) Token: 0x06000157 RID: 343 RVA: 0x0000268A File Offset: 0x0000088A
		public static string Country { get; set; } = "";

		// Token: 0x04000090 RID: 144
		public static string RegistrySuffix = "msi5".Equals("bgp") ? "" : "_msi5";

		// Token: 0x0400009C RID: 156
		private static string sRegistryBasePath = "";

		// Token: 0x040000A0 RID: 160
		private static bool mIsUpgrade = false;

		// Token: 0x040000BC RID: 188
		public static Dictionary<string, GlPropertyData> GlModeSupportedDataDict = new Dictionary<string, GlPropertyData>();

		// Token: 0x040000BF RID: 191
		public static string sCurrentHypervisor = null;

		// Token: 0x040000C2 RID: 194
		private static string sBsxVersion = null;

		// Token: 0x040000C3 RID: 195
		private static string sInstallerLogFilePath = null;
	}
}
