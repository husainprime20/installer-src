using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using UnhandledExceptionMode = System.Windows.Forms.UnhandledExceptionMode;
using System.Windows.Markup;
using System.Windows.Media;
using BlueStacks.Common;

namespace BlueStacks.Installer
{
	// Token: 0x0200001C RID: 28
	public partial class App : System.Windows.Application
	{
		// Token: 0x060000CF RID: 207 RVA: 0x0000CF14 File Offset: 0x0000B114
		private static void Init()
		{
			Logger.InitLogAtPath(InstallerProperties.InstallerLogFilePath, "BlueStacksInstaller", false);
			System.Windows.Forms.Application.ThreadException += App.Application_ThreadException;
			System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			AppDomain.CurrentDomain.UnhandledException += App.CurrentDomain_UnhandledException;
			InstallerStats.sStatsQueue.Start();
			App.CheckIfAlreadyRunning();
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x0000CF74 File Offset: 0x0000B174
		private bool IsInstallerArchitectureCompatible()
		{
			bool flag = SystemUtils.IsOs64Bit();
			bool flag2 = File.Exists("64bit");
			bool flag3 = flag2;
			if (flag3)
			{
				InstallerProperties.InstallerArchitecture = InstallerArchitectures.AMD64;
			}
			else
			{
				InstallerProperties.InstallerArchitecture = InstallerArchitectures.X86;
			}
			return flag == flag2;
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x0000CFBC File Offset: 0x0000B1BC
		protected override void OnStartup(StartupEventArgs e)
		{
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
			InstallerProperties.InstallerExtractedDir = AppDomain.CurrentDomain.BaseDirectory;
			App.sClock = Stopwatch.StartNew();
			App.Init();
			ServicePointManager.DefaultConnectionLimit = 1000;
			Logger.Info("OnStartup");
			bool flag = !this.IsInstallerArchitectureCompatible();
			if (flag)
			{
				Logger.Info("Incompatible installer started. Arch: {0}", new object[]
				{
					InstallerProperties.InstallerArchitecture
				});
				string text = LocaleStrings.GetLocalizedString("STRING_INCOMPATIBLE_INSTALLER_STARTED", "");
				text = ((InstallerProperties.InstallerArchitecture == InstallerArchitectures.AMD64) ? string.Format(text, "x86 (32-bit)") : string.Format(text, "x64 (64-bit)"));
				bool flag2 = !App.sOpt.s;
				if (flag2)
				{
					CustomMessageWindow customMessageWindow = new CustomMessageWindow();
					customMessageWindow.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_INCOMPATIBLE_INSTALLER", "");
					customMessageWindow.BodyTextBlock.Text = text;
					customMessageWindow.AddButton(ButtonColors.Blue, LocaleStrings.GetLocalizedString("STRING_EXIT", ""), new EventHandler(this.IncompatibleArchExitHandler), null, false, null, true);
					customMessageWindow.CloseButtonHandle(new EventHandler(this.IncompatibleArchExitHandler), null);
					customMessageWindow.ShowDialog();
				}
				App.ExitApplication(0);
			}
			this.InstallerPropertiesInit();
			base.OnStartup(e);
			InstallerStats.Enqueue("install_properties_inited", true, "", "");
			MainWindow mainWindow = new MainWindow();
			mainWindow.ShowWindow();
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x000022E6 File Offset: 0x000004E6
		private void IncompatibleArchExitHandler(object sender, EventArgs e)
		{
			App.ExitApplication(0);
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x0000D0FC File Offset: 0x0000B2FC
		private void InstallerPropertiesInit()
		{
			InstallerProperties.PFDir = this.GetPFDir(App.sOpt.pf);
			InstallerProperties.PDDir = this.GetPDDir(App.sOpt);
			bool flag = InstallerProperties.InSilentMode || !string.IsNullOrEmpty(App.sOpt.pddir);
			if (flag)
			{
				InstallerProperties.MainInstallerFilePath = App.sOpt.parentpath;
			}
			InstallerProperties.MainInstallerFolderPath = Directory.Exists(App.sOpt.parentpath)
				? Path.GetFullPath(App.sOpt.parentpath)
				: Path.GetDirectoryName(App.sOpt.parentpath);
			InstallerProperties.InSilentMode = App.sOpt.s;
			InstallerProperties.DefaultVmName = App.sOpt.defaultImageName;
			InstallerProperties.VmToLaunch = App.sOpt.imageToLaunch;
			InstallerProperties.CampaignMD5 = App.sOpt.md5;
			bool flag2 = !InstallerProperties.StartedFromMicroInstaller;
			if (flag2)
			{
				InstallerProperties.FLEAssetsDir = Path.Combine(InstallerProperties.InstallerExtractedDir, "Assets");
			}
			InstallerProperties.IsUpgrade = this.IsUpgrade();
			InstallerProperties.CurrentLocale = Utils.GetCurrentLocale();
			InstallerProperties.InstallId = InstallerUtils.GenerateUniqueInstallId();
			bool isUpgrade = InstallerProperties.IsUpgrade;
			if (isUpgrade)
			{
				this.SetInstallerPropertiesForUpgrade();
			}
			else
			{
				this.SetInstallerPropertiesForFresh();
			}
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x000022F0 File Offset: 0x000004F0
		private void SetInstallerPropertiesForFresh()
		{
			InstallerProperties.PFDirNew = InstallerProperties.PFDir;
			InstallerProperties.PDDirNew = InstallerProperties.PDDir;
			InstallerProperties.PDDirExisting = InstallerProperties.PDDir;
			InstallerProperties.PFDirExisting = InstallerProperties.PFDir;
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x0000D1F8 File Offset: 0x0000B3F8
		private void SetInstallerPropertiesForUpgrade()
		{
			InstallerProperties.PFDirExisting = RegistryStrings.InstallDir.TrimEnd(new char[]
			{
				'\\'
			});
			InstallerProperties.PDDirExisting = RegistryManager.Instance.UserDefinedDir.TrimEnd(new char[]
			{
				'\\'
			});
			InstallerProperties.PDDir = InstallerProperties.PDDirExisting;
			InstallerProperties.PDDirNew = InstallerProperties.PDDir + ".new";
			InstallerProperties.PDDirOld = InstallerProperties.PDDirExisting + ".old";
			InstallerProperties.PFDir = InstallerProperties.PFDirExisting;
			InstallerProperties.PFDirNew = InstallerProperties.PFDir + ".new";
			InstallerProperties.PFDirOld = InstallerProperties.PFDirExisting + ".old";
			InstallerProperties.OldLogsDir = RegistryManager.Instance.LogDir;
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x0000D2BC File Offset: 0x0000B4BC
		private string GetPFDir(string optPf)
		{
			string text = string.Empty;
			bool flag = !string.IsNullOrEmpty(optPf);
			if (flag)
			{
				text = Path.Combine(optPf, "ProgramFiles").TrimEnd(new char[]
				{
					'\\'
				});
				bool flag2 = -1 != text.IndexOfAny(IOUtils.DisallowedCharsInDirs);
				if (flag2)
				{
					Logger.Error("Disallowed character found in PFDir, exiting");
					App.ExitApplication(-8);
				}
			}
			else
			{
				text = MicroInstallerSharedUtils.GetDefaultPFDir(InstallerProperties.RegistrySuffix);
			}
			Logger.Info("Returning PFDir: {0}", new object[]
			{
				text
			});
			return text;
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x0000D350 File Offset: 0x0000B550
		private string GetPDDir(InstallerOpt opt)
		{
			string text = string.Empty;
			bool flag = !string.IsNullOrEmpty(opt.pd);
			if (flag)
			{
				text = Path.Combine(opt.pd, "ProgramData").TrimEnd(new char[]
				{
					'\\'
				});
				bool flag2 = -1 != text.IndexOfAny(IOUtils.DisallowedCharsInDirs);
				if (flag2)
				{
					Logger.Error("Disallowed character found in PDDir, exiting");
					App.ExitApplication(-8);
				}
			}
			else
			{
				bool flag3 = !string.IsNullOrEmpty(opt.pddir);
				if (flag3)
				{
					text = opt.pddir;
					InstallerProperties.StartedFromMicroInstaller = true;
				}
				else
				{
					text = MicroInstallerSharedUtils.GetDefaultPDDir(InstallerProperties.RegistrySuffix);
				}
			}
			Logger.Info("Returning PDDir: {0}", new object[]
			{
				text
			});
			return text;
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x0000D414 File Offset: 0x0000B614
		private static void SetCloudHost()
		{
			string host = MicroInstallerSharedUtils.GetHost(InstallerProperties.Country, InstallerProperties.RegistryBasePath);
			InstallerStats.Enqueue("install_cloud_host_set", true, host, "");
			Logger.Info("Setting cloud host: {0}", new object[]
			{
				host
			});
			InstallerProperties.CloudHost = host;
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x0000D460 File Offset: 0x0000B660
		private bool IsUpgrade()
		{
			try
			{
				string text = (string)RegistryUtils.GetRegistryValue("Software\\BlueStacks_msi5", "Version", "", RegistryKeyKind.HKEY_LOCAL_MACHINE);
				bool flag = !string.IsNullOrEmpty(text);
				if (flag)
				{
					InstallerProperties.OldVersion = text;
					Logger.Info("Old version: {0}, new version: {1}", new object[]
					{
						text,
						"5.12.120.6303"
					});
					System.Version v = new System.Version(text.Substring(0, text.LastIndexOf(".")));
					System.Version v2 = new System.Version("5.12.120.6303".Substring(0, "5.12.120.6303".LastIndexOf(".")));
					bool flag2 = v2 > v;
					if (flag2)
					{
						System.Version v3 = new System.Version("5.11.10");
						InstallerProperties.UpgradeFromUnificationBuild = (v3 <= v);
						return true;
					}
					Logger.Warning("Same or higher version already installed.");
					InstallerProperties.SameOrHigherVersionInstalled = true;
					return false;
				}
			}
			catch
			{
			}
			return false;
		}

		// Token: 0x060000DA RID: 218 RVA: 0x0000D55C File Offset: 0x0000B75C
		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Logger.Info("Current domain exception..." + e.ExceptionObject.ToString());
			InstallerStats.SendInstallFailedEvent(InstallerUtils.ConvertIntToEnumString(-56));
			bool flag = !App.sOpt.s;
			if (flag)
			{
				System.Windows.MessageBox.Show(e.ExceptionObject.ToString());
			}
			else
			{
				App.ExitApplication(-56);
			}
		}

		// Token: 0x060000DB RID: 219 RVA: 0x0000D5C4 File Offset: 0x0000B7C4
		private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			Logger.Info("Current thread exception..." + e.Exception.ToString());
			InstallerStats.SendInstallFailedEvent(InstallerUtils.ConvertIntToEnumString(-56));
			bool flag = !App.sOpt.s;
			if (flag)
			{
				System.Windows.MessageBox.Show(e.Exception.ToString());
			}
			else
			{
				App.ExitApplication(-56);
			}
		}

		// Token: 0x060000DE RID: 222 RVA: 0x0000D640 File Offset: 0x0000B840
		private static void CheckIfAlreadyRunning()
		{
			bool flag = ProcessUtils.CheckAlreadyRunningAndTakeLock("Global\\BlueStacks_Installer_Lockmsi5", out App.sInstallerLock);
			if (flag)
			{
				App.ExitApplication(-1);
			}
		}

		// Token: 0x060000DF RID: 223 RVA: 0x0000D66C File Offset: 0x0000B86C
		private void ShowVersionAlreadyInstalledPopupAndExit()
		{
			bool inSilentMode = InstallerProperties.InSilentMode;
			if (inSilentMode)
			{
				this.VersionAlreadyInstalledHandler(null, null);
			}
			else
			{
				CustomMessageWindow customMessageWindow = new CustomMessageWindow();
				customMessageWindow.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_VERSION_ALREADY_INSTALLED", "");
				customMessageWindow.BodyTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_LATEST_VERSION_INSTALLED", "");
				customMessageWindow.AddButton(ButtonColors.Blue, LocaleStrings.GetLocalizedString("STRING_LAUNCH", ""), new EventHandler(this.VersionAlreadyInstalledLaunchBSHandler), null, false, null, true);
				customMessageWindow.AddButton(ButtonColors.White, LocaleStrings.GetLocalizedString("STRING_OK", ""), new EventHandler(this.VersionAlreadyInstalledHandler), null, false, null, true);
				customMessageWindow.CloseButtonHandle(new EventHandler(this.VersionAlreadyInstalledHandler), null);
				customMessageWindow.ShowDialog();
			}
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x0000D738 File Offset: 0x0000B938
		private void ShowUpgradeNotSupportedFromInstalledVersionPopupAndExit()
		{
			Logger.Error("Upgrade from this version not supported");
			bool flag = !InstallerProperties.InSilentMode;
			if (flag)
			{
				CustomMessageWindow customMessageWindow = new CustomMessageWindow();
				customMessageWindow.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_UPGRADE_NOT_SUPPORTED", "");
				customMessageWindow.BodyTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_UNINSTALL_CURRENT_AND_REINSTALL", "");
				customMessageWindow.AddButton(ButtonColors.Blue, "STRING_EXIT", new EventHandler(this.UpgradeNotSupportedHandler), null, false, null, true);
				customMessageWindow.CloseButtonHandle(new EventHandler(this.UpgradeNotSupportedHandler), null);
				customMessageWindow.ShowDialog();
			}
			else
			{
				this.UpgradeNotSupportedHandler(null, null);
			}
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x0000234E File Offset: 0x0000054E
		private void UpgradeNotSupportedHandler(object sender, EventArgs e)
		{
			App.ExitApplication(-4);
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x00002359 File Offset: 0x00000559
		internal void VersionAlreadyInstalledHandler(object sender, EventArgs e)
		{
			App.ExitApplication(-3);
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x0000D7E0 File Offset: 0x0000B9E0
		internal void VersionAlreadyInstalledLaunchBSHandler(object sender, EventArgs e)
		{
			int num = 2;
			Logger.Info("Will launch BlueStacks (exiting with exit code: {0})", new object[]
			{
				num
			});
			App.ExitApplication(num);
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x000022E6 File Offset: 0x000004E6
		private static void Application_Exit(object sender, ExitEventArgs e)
		{
			App.ExitApplication(0);
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x0000D814 File Offset: 0x0000BA14
		public static void ExitApplication(int errorLevel = 0)
		{
			bool isRunning = App.sClock != null && App.sClock.IsRunning;
			if (isRunning)
			{
				App.sClock.Stop();
			}
			Logger.Info("Waiting for stats queue to join");
			InstallerStats.sStatsQueue.Stop();
			InstallerStats.sStatsQueue.Join();
			Environment.Exit(errorLevel);
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x0000D868 File Offset: 0x0000BA68
		internal static void LogInstallerProperties()
		{
			Logger.Info("InstallerProperties");
			Logger.Info("CloudHost: " + InstallerProperties.CloudHost);
			Logger.Info("IsUpgrade: " + InstallerProperties.IsUpgrade.ToString());
			Logger.Info("CampaignMD5: " + InstallerProperties.CampaignMD5);
			Logger.Info("InstallId: " + InstallerProperties.InstallId);
			Logger.Info("IsRunningInSilentMode: " + InstallerProperties.InSilentMode.ToString());
			Logger.Info("InstallerArchitecture: " + InstallerProperties.InstallerArchitecture);
			Logger.Info("PFDir: " + InstallerProperties.PFDir);
			Logger.Info("PDDir: " + InstallerProperties.PDDir);
			Logger.Info("PFDirNew: " + InstallerProperties.PFDirNew);
			Logger.Info("PDDirNew: " + InstallerProperties.PDDirNew);
			bool isUpgrade = InstallerProperties.IsUpgrade;
			if (isUpgrade)
			{
				Logger.Info("PFDirExisting: " + InstallerProperties.PFDirExisting);
				Logger.Info("PDDirExisting: " + InstallerProperties.PDDirExisting);
				Logger.Info("PFDirOld: " + InstallerProperties.PFDirOld);
				Logger.Info("PDDirOld: " + InstallerProperties.PDDirOld);
				Logger.Info("OldLogsDir: " + InstallerProperties.OldLogsDir);
				Logger.Info("OldInstallId: " + InstallerProperties.OldInstallId);
			}
			Logger.Info("InstallerExtractedDir: {0}", new object[]
			{
				InstallerProperties.InstallerExtractedDir
			});
			Logger.Info("DefaultImageName: {0}", new object[]
			{
				InstallerProperties.DefaultVmName
			});
			Logger.Info("CampaignMD5: {0}", new object[]
			{
				InstallerProperties.CampaignMD5
			});
			Logger.Info("IsForcedDedicatedGPU: {0}", new object[]
			{
				InstallerProperties.PreferDedicatedGPU
			});
			Logger.Info("CampaignName: {0}", new object[]
			{
				InstallerProperties.CampaignName
			});
			Logger.Info("IsRunningInSilentMode: {0}", new object[]
			{
				InstallerProperties.InSilentMode
			});
			Logger.Info("CloudHost: {0}", new object[]
			{
				InstallerProperties.CloudHost
			});
			Logger.Info("GLMode: {0}", new object[]
			{
				InstallerProperties.GLMode
			});
			Logger.Info("GLRenderMode: {0}", new object[]
			{
				InstallerProperties.GLRenderMode
			});
			Logger.Info("GLRenderer: {0}", new object[]
			{
				InstallerProperties.GLRenderer
			});
			Logger.Info("GLVendor: {0}", new object[]
			{
				InstallerProperties.GLVendor
			});
			Logger.Info("GLVersion: {0}", new object[]
			{
				InstallerProperties.GLVersion
			});
			Logger.Info("SameOrHigherVersionInstalled: {0}", new object[]
			{
				InstallerProperties.SameOrHigherVersionInstalled
			});
			Logger.Info("OldVersion: {0}", new object[]
			{
				InstallerProperties.OldVersion
			});
			Logger.Info("CurrentLocale: {0}", new object[]
			{
				InstallerProperties.CurrentLocale
			});
			Logger.Info("InstallerArchitecture: {0}", new object[]
			{
				InstallerProperties.InstallerArchitecture
			});
			Logger.Info("MainInstallerFilePath: {0}", new object[]
			{
				InstallerProperties.MainInstallerFilePath
			});
			Logger.Info("MainInstallerFolderPath: {0}", new object[]
			{
				InstallerProperties.MainInstallerFolderPath
			});
			Logger.Info("IsFreshInstall: {0}", new object[]
			{
				InstallerProperties.IsFreshInstall
			});
			Logger.Info("FLEAssetsDir: {0}", new object[]
			{
				InstallerProperties.FLEAssetsDir
			});
			Logger.Info("InstallerProperties End");
		}

		// Token: 0x060000EB RID: 235 RVA: 0x0000DC44 File Offset: 0x0000BE44
		private static void OnAnyElementLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				System.Windows.Controls.Control control = sender as System.Windows.Controls.Control;
				if (control != null)
				{
					control.FontFamily = App.sGlobalFont;
					control.FontWeight = App.sGlobalFontWeight;
				}
				else
				{
					TextBlock textBlock = sender as TextBlock;
					if (textBlock != null)
					{
						textBlock.FontFamily = App.sGlobalFont;
						textBlock.FontWeight = App.sGlobalFontWeight;
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x060000EC RID: 236 RVA: 0x0000DCA8 File Offset: 0x0000BEA8
		private static void ApplyGlobalFont()
		{
			try
			{
				FontFamily value = new FontFamily(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets") + "\\", UriKind.Absolute), "./#Google Sans");
				FontWeight medium = FontWeights.Medium;
				Style style = new Style(typeof(TextBlock));
				style.Setters.Add(new Setter(TextBlock.FontFamilyProperty, value));
				style.Setters.Add(new Setter(TextBlock.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(TextBlock)] = style;
				Style style2 = new Style(typeof(System.Windows.Controls.Label));
				style2.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style2.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(System.Windows.Controls.Label)] = style2;
				Style style3 = new Style(typeof(System.Windows.Controls.TextBox));
				style3.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style3.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(System.Windows.Controls.TextBox)] = style3;
				Style style4 = new Style(typeof(System.Windows.Controls.Button));
				style4.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style4.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(System.Windows.Controls.Button)] = style4;
				Style style5 = new Style(typeof(ContentControl));
				style5.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style5.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(ContentControl)] = style5;
				Style style6 = new Style(typeof(System.Windows.Controls.RadioButton));
				style6.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style6.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(System.Windows.Controls.RadioButton)] = style6;
				Style style7 = new Style(typeof(System.Windows.Controls.CheckBox));
				style7.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style7.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(System.Windows.Controls.CheckBox)] = style7;
				Style style8 = new Style(typeof(System.Windows.Controls.ComboBox));
				style8.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style8.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(System.Windows.Controls.ComboBox)] = style8;
				Style style9 = new Style(typeof(ComboBoxItem));
				style9.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style9.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(ComboBoxItem)] = style9;
				Style style10 = new Style(typeof(System.Windows.Controls.GroupBox));
				style10.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style10.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(System.Windows.Controls.GroupBox)] = style10;
				Style style11 = new Style(typeof(ListBoxItem));
				style11.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style11.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(ListBoxItem)] = style11;
				Style style12 = new Style(typeof(System.Windows.Controls.MenuItem));
				style12.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style12.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(System.Windows.Controls.MenuItem)] = style12;
				Style style13 = new Style(typeof(System.Windows.Controls.ToolTip));
				style13.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style13.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(System.Windows.Controls.ToolTip)] = style13;
				Style style14 = new Style(typeof(CustomRadioButton));
				style14.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style14.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(CustomRadioButton)] = style14;
				Style style15 = new Style(typeof(CustomComboBox));
				style15.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style15.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(CustomComboBox)] = style15;
				Style style16 = new Style(typeof(CustomTextBox));
				style16.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style16.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(CustomTextBox)] = style16;
				Style style17 = new Style(typeof(CustomButton));
				style17.Setters.Add(new Setter(System.Windows.Controls.Control.FontFamilyProperty, value));
				style17.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, medium));
				System.Windows.Application.Current.Resources[typeof(CustomButton)] = style17;
				App.sGlobalFont = value;
				App.sGlobalFontWeight = medium;
				EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.LoadedEvent, new RoutedEventHandler(App.OnAnyElementLoaded));
			}
			catch (Exception ex)
			{
				Logger.Warning("Failed to apply global font: {0}", new object[]
				{
					ex.ToString()
				});
			}
		}

		// Token: 0x04000083 RID: 131
		internal static InstallerOpt sOpt = new InstallerOpt();

		// Token: 0x04000084 RID: 132
		internal static Stopwatch sClock = null;

		// Token: 0x04000085 RID: 133
		internal static Mutex sInstallerLock = null;

		// Token: 0x04000087 RID: 135
		private static FontFamily sGlobalFont;

		// Token: 0x04000088 RID: 136
		private static FontWeight sGlobalFontWeight;
	}
}
