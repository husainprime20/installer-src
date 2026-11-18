using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using BlueStacks.Common;
using DiscordRPC;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace BlueStacks.Installer
{
	// Token: 0x0200001F RID: 31
	public partial class MainWindow : Window
	{
		// Token: 0x0600015A RID: 346
		[DllImport("HD-Astcdecoder.dll")]
		public static extern bool cpu_supports_sse20();

		// Token: 0x0600015B RID: 347
		[DllImport("HD-Astcdecoder.dll")]
		public static extern bool cpu_supports_sse42();

		// Token: 0x0600015C RID: 348
		[DllImport("HD-Astcdecoder.dll")]
		public static extern bool cpu_supports_avx1();

		// Token: 0x0600015D RID: 349 RVA: 0x0000E8B0 File Offset: 0x0000CAB0
		public MainWindow()
		{
			this.AccentGold = Color.FromRgb(224, 189, 118);
			this.AccentGoldDeep = Color.FromRgb(168, 130, 68);
			this.AccentGoldPale = Color.FromRgb(250, 232, 190);
			this.mBackgroundWorker = null;
			this.mBackgroundImages = new string[0];
			this.mBackgroundSelectedIndex = -1;
			this.mDontShowClosePopUp = false;
			this.mInstallFailedErrorCode = 0;
			this.InitializeComponent();
			this.CreateCustomWindowButtons();
			Grid grid = this.mInstallerBackgroundImage.Parent as Grid;
			if (grid != null)
			{
				int num = grid.RowDefinitions.Count;
				int num2 = grid.ColumnDefinitions.Count;
				if (num == 0)
				{
					num = 10;
				}
				if (num2 == 0)
				{
					num2 = 10;
				}
				this.mBackgroundMedia = new MediaElement();
				this.mBackgroundMedia.MouseLeftButtonDown += this.HandleMouseDrag;
				this.mBackgroundMedia.LoadedBehavior = MediaState.Manual;
				this.mBackgroundMedia.UnloadedBehavior = MediaState.Manual;
				this.mBackgroundMedia.Stretch = Stretch.UniformToFill;
				this.mBackgroundMedia.ScrubbingEnabled = true;
				this.mBackgroundMedia.Volume = 1.0;
				this.mBackgroundMedia.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
				this.mBackgroundMedia.VerticalAlignment = VerticalAlignment.Stretch;
				string text = AppDomain.CurrentDomain.BaseDirectory + "Assets\\installer_bg.mp4";
				if (File.Exists(text))
				{
					this.mBackgroundMedia.Source = new Uri(text, UriKind.Absolute);
				}
				Grid.SetRow(this.mBackgroundMedia, 0);
				Grid.SetColumn(this.mBackgroundMedia, 0);
				Grid.SetRowSpan(this.mBackgroundMedia, num);
				Grid.SetColumnSpan(this.mBackgroundMedia, num2);
				System.Windows.Controls.Panel.SetZIndex(this.mBackgroundMedia, -999);
				this.mBackgroundMedia.Opacity = 1.0;
				grid.Children.Insert(0, this.mBackgroundMedia);
				this.mInstallerBackgroundImage.Visibility = Visibility.Hidden;
				this.mForegroundMedia = new MediaElement();
				this.mForegroundMedia.MouseLeftButtonDown += this.HandleMouseDrag;
				this.mForegroundMedia.LoadedBehavior = MediaState.Manual;
				this.mForegroundMedia.UnloadedBehavior = MediaState.Manual;
				this.mForegroundMedia.Stretch = Stretch.UniformToFill;
				this.mForegroundMedia.ScrubbingEnabled = true;
				this.mForegroundMedia.Volume = 1.0;
				this.mForegroundMedia.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
				this.mForegroundMedia.VerticalAlignment = VerticalAlignment.Stretch;
				this.mForegroundMedia.Opacity = 0.0;
				string text2 = AppDomain.CurrentDomain.BaseDirectory + "Assets\\installer_bg1.mp4";
				if (File.Exists(text2))
				{
					this.mForegroundMedia.Source = new Uri(text2, UriKind.Absolute);
				}
				Grid.SetRow(this.mForegroundMedia, 0);
				Grid.SetColumn(this.mForegroundMedia, 0);
				Grid.SetRowSpan(this.mForegroundMedia, num);
				Grid.SetColumnSpan(this.mForegroundMedia, num2);
				System.Windows.Controls.Panel.SetZIndex(this.mForegroundMedia, -999);
				grid.Children.Insert(1, this.mForegroundMedia);
				this.mForegroundMedia.MediaOpened += delegate(object sender, RoutedEventArgs e)
				{
					this.mBackgroundMedia.Stop();
					this.mBackgroundMedia.Visibility = Visibility.Hidden;
					this.mForegroundMedia.Visibility = Visibility.Visible;
					this.mForegroundMedia.Position = TimeSpan.Zero;
					this.mForegroundMedia.Play();
				};
				this.mBackgroundMedia.MediaEnded += delegate(object sender, RoutedEventArgs e)
				{
					this.mForegroundMedia.Stop();
					this.mForegroundMedia.Visibility = Visibility.Hidden;
					this.mBackgroundMedia.Visibility = Visibility.Visible;
					this.mBackgroundMedia.Position = TimeSpan.Zero;
					this.mBackgroundMedia.Play();
				};
				this.mForegroundMedia.MediaEnded += delegate(object sender, RoutedEventArgs e)
				{
					this.mBackgroundMedia.Position = TimeSpan.Zero;
					this.mBackgroundMedia.Play();
					this.mBackgroundMedia.Opacity = 1.0;
					this.mForegroundMedia.Opacity = 0.0;
					this.mForegroundMedia.Pause();
				};
				this.mBackgroundMedia.Play();
			}
			base.Width = 800.0;
			base.Height = 500.0;
			base.AllowsTransparency = true;
			base.Background = Brushes.Transparent;
			Border border = base.Content as Border;
			if (border != null)
			{
				border.CornerRadius = new CornerRadius(20.0);
				border.ClipToBounds = true;
				base.Clip = new RectangleGeometry(new Rect(0.0, 0.0, base.Width, base.Height), 20.0, 20.0);
			}
			base.Title = "FRΛMΣx Installer";
			this.mInstallerText.Text = LocaleStrings.GetLocalizedString("STRING_INSTALLER", "");
			this.FetchImageForCampaignAsync();
			string appNameFromBase64String = this.GetAppNameFromBase64String(App.sOpt.app64);
			if (!string.IsNullOrEmpty(appNameFromBase64String))
			{
				this.mAppNameInstallingText.Text = appNameFromBase64String;
				this.mAppNameInstallingText.Visibility = Visibility.Visible;
			}
			this.mInstallProgress.mInstallTips.Text = LocaleStrings.GetLocalizedString("STRING_ANTIVIRUS_PAUSE_INSTALLER", "");
			base.Loaded += this.MainWindow_Loaded;
			this.SetupSettingsPanel();
		}

		// Token: 0x0600015E RID: 350 RVA: 0x0000ED34 File Offset: 0x0000CF34
		private string GetAppNameFromBase64String(string base64String)
		{
			if (!string.IsNullOrEmpty(base64String))
			{
				string empty = string.Empty;
				try
				{
					return Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
				}
				catch (Exception ex)
				{
					Logger.Warning("Error while parsing app64 for installer. Ex: " + ex.Message);
				}
				return empty;
			}
			return "";
		}

		// Token: 0x0600015F RID: 351 RVA: 0x0000ED98 File Offset: 0x0000CF98
		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			InstallerStats.Enqueue("install_window_created", true, "", "");
			if (InstallerProperties.InSilentMode)
			{
				base.Visibility = Visibility.Hidden;
				base.ShowInTaskbar = false;
				base.Topmost = false;
			}
			this.mChooseInstallFolder.mBluestacksDataLocation.Text = InstallerProperties.PDDir;
			if (InstallerProperties.IsUpgrade)
			{
				this.ChangeUIForUpgrade();
			}
			else if (InstallerProperties.SameOrHigherVersionInstalled)
			{
				this.mStartInstall.mInstallNowButton.Visibility = Visibility.Visible;
				this.mStartInstall.mCustomInstallLicensePanel.Visibility = Visibility.Hidden;
				this.mStartInstall.mInstallNowButton.Content = LocaleStrings.GetLocalizedString("STRING_LAUNCH", "");
				this.mStartInstall.mInstallNowButton.IsEnabled = true;
				this.mStartInstall.mInstallNowButton.Click += delegate(object btsender, RoutedEventArgs ec)
				{
					string text = string.Format("--cmd installImage --imageName {0} --noexit --source installer", InstallerProperties.VmToLaunch).Trim();
					RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(InstallerProperties.RegistryBasePath);
					if (registryKey != null)
					{
						string text2 = (string)registryKey.GetValue("InstallDir", null);
						Logger.Info("InstallDir: {0}", new object[]
						{
							text2
						});
						if (!string.IsNullOrEmpty(text2))
						{
							string text3 = System.IO.Path.Combine(text2, "HD-MultiInstanceManager.exe");
							if (File.Exists(text3))
							{
								try
								{
									if (Customizations.Instance.LaunchBlueStacksElevated)
									{
										Process.Start(text3, text);
									}
									else
									{
										ProcessUtils.ExecuteProcessUnelevated(text3, text, System.IO.Path.GetDirectoryName(text3));
									}
								}
								catch (Exception ex)
								{
									Logger.Warning("Couldn't launch BlueStacks, Ex: {0}", new object[]
									{
										ex
									});
								}
							}
						}
					}
					App.ExitApplication(0);
				};
				return;
			}
			this.SetupEventHandlers();
			this.GetGuidInBackgroundThread();
		}

		// Token: 0x06000160 RID: 352 RVA: 0x0000EE98 File Offset: 0x0000D098
		private void FetchImageForCampaignAsync()
		{
			if (!InstallerProperties.InSilentMode)
			{
				if (File.Exists(System.IO.Path.Combine(InstallerProperties.FLEAssetsDir, "installer_bg_1.jpg")))
				{
					this.CampaignImageDownloader_RunWorkerCompleted(null, null);
					return;
				}
				BackgroundWorker backgroundWorker = new BackgroundWorker();
				backgroundWorker.DoWork += this.CampaignImageDownloader_DoWork;
				backgroundWorker.RunWorkerCompleted += this.CampaignImageDownloader_RunWorkerCompleted;
				backgroundWorker.RunWorkerAsync();
			}
		}

		// Token: 0x06000161 RID: 353 RVA: 0x0000EEFC File Offset: 0x0000D0FC
		private void CampaignImageDownloader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				this.mBackgroundImages = Directory.GetFiles(InstallerProperties.FLEAssetsDir, "installer_bg_*.jpg", SearchOption.TopDirectoryOnly);
				this.mBackgroundImages = (from w in this.mBackgroundImages
				where w != System.IO.Path.Combine(InstallerProperties.FLEAssetsDir, "installer_bg_blurred.jpg")
				select w).ToArray<string>();
				if (this.mBackgroundImages.Count<string>() >= 1)
				{
					if (this.mBackgroundImages.Count<string>() == 1)
					{
						base.Dispatcher.Invoke(new Action(delegate()
						{
							if (this.mCustomWallpaperActive)
							{
								return;
							}
							MicroInstallerSharedUtils.SetAnimationInBackgroudImage(this.mInstallerBackgroundImage, this.mBackgroundImages[0]);
						}), new object[0]);
					}
					else
					{
						this.mBackgroundChangeTimer = new System.Timers.Timer(7000.0);
						this.mBackgroundChangeTimer.Elapsed += this.MBackgroundChangeTimer_Elapsed;
						this.mBackgroundChangeTimer.Enabled = true;
						this.MBackgroundChangeTimer_Elapsed(this, null);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Exception while setting image as background. Ex: " + ex.Message);
			}
		}

		// Token: 0x06000162 RID: 354 RVA: 0x0000F000 File Offset: 0x0000D200
		private void MBackgroundChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (this.mCustomWallpaperActive)
			{
				return;
			}
			int num = this.mBackgroundImages.Count<string>();
			string imagePath;
			for (;;)
			{
				if (num <= this.mBackgroundSelectedIndex + 1)
				{
					this.mBackgroundSelectedIndex = -1;
				}
				if (num <= this.mBackgroundSelectedIndex + 1)
				{
					break;
				}
				this.mBackgroundSelectedIndex++;
				imagePath = this.mBackgroundImages[this.mBackgroundSelectedIndex];
				if (File.Exists(imagePath))
				{
					goto Block_4;
				}
			}
			return;
			Block_4:
			base.Dispatcher.Invoke(new Action(delegate()
			{
				MicroInstallerSharedUtils.SetAnimationInBackgroudImage(this.mInstallerBackgroundImage, imagePath);
			}), new object[0]);
		}

		// Token: 0x06000163 RID: 355 RVA: 0x0000F0A8 File Offset: 0x0000D2A8
		private void CampaignImageDownloader_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				JObject jobject = JObject.Parse(HTTP.PostWrapper(string.Format("{0}/bs3/get_installer_images?md5_hash={1}&prod_ver={2}&oem={3}&locale={4}&android_image={5}&bsxversion={6}&default_launch={7}", new object[]
				{
					InstallerProperties.CloudHost,
					InstallerProperties.CampaignMD5,
					"5.12.120.6303",
					"msi5",
					InstallerProperties.CurrentLocale,
					InstallerProperties.VmToLaunch,
					InstallerProperties.BsxVersion,
					InstallerProperties.AppToLaunch
				}), null, null, false, 1, 0, 5000, InstallerStats.GetEarlyHeaders(InstallerProperties.DefaultVmName), Utils.GetUserAgent("msi5", InstallerProperties.GUID)));
				int num = 1;
				foreach (JToken jtoken in ((IEnumerable<JToken>)jobject["installer_images"]))
				{
					Utils.TinyDownloader(jtoken.ToString(), "installer_bg_" + num.ToString() + ".jpg", InstallerProperties.FLEAssetsDir, false);
					num++;
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Error while downloading campaign images for installer. Ex: {0}", new object[]
				{
					ex
				});
			}
		}

		// Token: 0x06000164 RID: 356 RVA: 0x0000F1C0 File Offset: 0x0000D3C0
		private void ShowInsufficientDiskSpaceWarningPopup(long spaceRequiredMb, string driveLetter)
		{
			base.Dispatcher.Invoke(new Action(delegate()
			{
				CustomMessageWindow customMessageWindow = new CustomMessageWindow();
				customMessageWindow.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_INSUFFICIENT_SPACE", "");
				string format = string.Format(CultureInfo.InvariantCulture, "{0}\n{1}\n{2}", new object[]
				{
					LocaleStrings.GetLocalizedString("STRING_PC_INSUFFICIENT_SPACE_WARNING", ""),
					LocaleStrings.GetLocalizedString("STRING_BS_REQUIRES_SPACE_DRIVE_INSTALL", ""),
					LocaleStrings.GetLocalizedString("STRING_AFTER_DISK_SPACE_CONTINUE", "")
				});
				((spaceRequiredMb + 1023L) / 1024L).ToString();
				customMessageWindow.BodyTextBlock.Text = string.Format(CultureInfo.InvariantCulture, format, new object[]
				{
					"5",
					driveLetter
				});
				customMessageWindow.AddButton(ButtonColors.Blue, "STRING_RETRY", null, null, false, null, true);
				CustomMessageWindow customMessageWindow2 = customMessageWindow;
				ButtonColors color = ButtonColors.White;
				string text = "STRING_CLOSE";
				EventHandler handle = delegate(object s, EventArgs ev)
				{
					this.ExitApplication(-9);
				};
				customMessageWindow2.AddButton(color, text, handle, null, false, null, true);
				customMessageWindow.UrlTextBlock.Visibility = Visibility.Visible;
				customMessageWindow.UrlLink.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#047CD2"));
				customMessageWindow.UrlLink.Inlines.Add(LocaleStrings.GetLocalizedString("STRING_FREE_DISK_SPACE", ""));
				string str = WebHelper.GetUrlWithParams(Strings.ImageName, string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[]
				{
					WebHelper.GetServerHost(),
					"help_articles"
				}), "5.12.120.6303", "5.12.120.6303", Customizations.Instance.HelpArticleLocale, InstallerProperties.GUID) + "&article=";
				customMessageWindow.UrlLink.NavigateUri = new Uri(str + "free_disk_space_using_diskcompactiontool");
				customMessageWindow.UrlLink.RequestNavigate += this.MainWindow_UrlNavigationHandler;
				CustomMessageWindow customMessageWindow3 = customMessageWindow;
				EventHandler handle2 = delegate(object s, EventArgs ev)
				{
					this.ExitApplication(-9);
				};
				customMessageWindow3.CloseButtonHandle(handle2, null);
				customMessageWindow.Owner = this;
				customMessageWindow.ShowDialog();
			}), new object[0]);
		}

		// Token: 0x06000165 RID: 357 RVA: 0x00002692 File Offset: 0x00000892
		private void ShowUpgradeToWin7SP1Popup()
		{
			base.Dispatcher.Invoke(new Action(delegate()
			{
				CustomMessageWindow customMessageWindow = new CustomMessageWindow();
				customMessageWindow.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_UPDATE_WIN7_TITLE", "");
				customMessageWindow.BodyTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_UPDATE_WIN7_MESSAGE", "");
				customMessageWindow.AddButton(ButtonColors.Blue, "STRING_READ_ARTICLE", delegate(object s, EventArgs ev)
				{
					string text = string.Format("{0}/bs3/{1}?article={2}&oem={3}&locale={4}&image_name={5}", new object[]
					{
						"https://cloud.bluestacks.com",
						"help_articles",
						"update_to_win7_sp1",
						"msi5",
						string.IsNullOrEmpty(Customizations.Instance.HelpArticleLocale) ? Globalization.FindClosestMatchingLocale(Thread.CurrentThread.CurrentCulture.Name) : Customizations.Instance.HelpArticleLocale,
						Strings.ImageName
					});
					try
					{
						Process.Start(text);
					}
					catch (Exception ex)
					{
						Logger.Error("Not able to launch the url {0}. Ignoring Ex: {1}", new object[]
						{
							text,
							ex.Message
						});
					}
				}, null, false, null, true);
				customMessageWindow.CloseButtonHandle(null, null);
				customMessageWindow.Owner = this;
				customMessageWindow.ShowDialog();
			}), new object[0]);
		}

		// Token: 0x06000166 RID: 358 RVA: 0x000026B2 File Offset: 0x000008B2
		private void MainWindow_UrlNavigationHandler(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.OriginalString));
		}

		// Token: 0x06000167 RID: 359 RVA: 0x000026CA File Offset: 0x000008CA
		private void GetGuidInBackgroundThread()
		{
			BackgroundWorker backgroundWorker = new BackgroundWorker();
			backgroundWorker.DoWork += this.GenerateGuid_DoWork;
			backgroundWorker.RunWorkerCompleted += this.GenerateGuid_RunWorkerCompleted;
			backgroundWorker.RunWorkerAsync();
		}

		// Token: 0x06000168 RID: 360 RVA: 0x000026FA File Offset: 0x000008FA
		private void GenerateGuid_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			this.mStartInstall.mInstallNowButton.IsEnabled = true;
			InstallerStats.SendStartEventAfterShowingMainWindowAsync();
			if (InstallerProperties.InSilentMode || InstallerProperties.StartedFromMicroInstaller)
			{
				this.InstallNowButton_Click(null, null);
			}
		}

		// Token: 0x06000169 RID: 361 RVA: 0x0000F208 File Offset: 0x0000D408
		private void GenerateGuid_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				InstallerStats.Enqueue("install_guid_gen_started", true, "", "");
				string machineID = App.sOpt.machineID;
				string value = App.sOpt.versionMachineID;
				if (string.IsNullOrEmpty(machineID))
				{
					string blueStacksMachineId = GuidUtils.BlueStacksMachineId;
				}
				if (string.IsNullOrEmpty(value))
				{
					value = GuidUtils.BlueStacksVersionId;
				}
				string guidFromBackup = GuidUtils.GetGuidFromBackup();
				if (string.IsNullOrEmpty(guidFromBackup))
				{
					InstallerProperties.GUID = GuidUtils.BlueStacksMachineId;
				}
				else
				{
					InstallerProperties.GUID = guidFromBackup;
				}
				InstallerProperties.OldGUID = guidFromBackup;
				Utils.BackupGuid(InstallerProperties.GUID);
			}
			catch (Exception ex)
			{
				Logger.Error("Error in guid generation. Ex: {0}", new object[]
				{
					ex
				});
			}
		}

		// Token: 0x0600016A RID: 362 RVA: 0x0000F2B4 File Offset: 0x0000D4B4
		private void ChangeUIForUpgrade()
		{
			this.mStartInstall.mInstallNowButton.Visibility = Visibility.Visible;
			this.mStartInstall.mCustomInstallLicensePanel.Visibility = Visibility.Hidden;
			this.mStartInstall.mInstallNowButton.Content = LocaleStrings.GetLocalizedString("STRING_UPGRADE_NOW", "");
		}

		// Token: 0x0600016B RID: 363 RVA: 0x0000F304 File Offset: 0x0000D504
		private void SetupEventHandlers()
		{
			this.mInstallError.MouseLeftButtonDown += this.HandleMouseDrag;
			this.mInstallError.mRestartNowButton.Click += this.RestartNowButton_Click;
			this.mStartInstall.MouseLeftButtonDown += this.HandleMouseDrag;
			this.mStartInstall.mInstallNowButton.Click += this.InstallNowButton_Click;
			this.mStartInstall.mCustomInstallImage.MouseDown += this.ShowCustomInstallUserControl;
			this.mStartInstall.mCustomInstallLabel.MouseDown += this.ShowCustomInstallUserControl;
			this.mInstallFinish.MouseLeftButtonDown += this.HandleMouseDrag;
			this.mInstallFinish.mLaunchBlueStacks.Click += this.LaunchBlueStacks_Click;
			this.mInstallProgress.MouseLeftButtonDown += this.HandleMouseDrag;
			this.mInstallProgress.mInstallProgressBar.ValueChanged += this.InstallProgressBar_ValueChanged;
			this.mChooseInstallFolder.MouseLeftButtonDown += this.HandleMouseDrag;
			this.mChooseInstallFolder.mBackButton.MouseDown += this.BackButton_MouseDown;
			this.mChooseInstallFolder.mBackButtonLabel.MouseDown += this.BackButton_MouseDown;
			this.mChooseInstallFolder.mInstallNowLabel.MouseDown += new MouseButtonEventHandler(this.InstallNowButton_Click);
			this.mChooseInstallFolder.mInstallNowButton.MouseDown += new MouseButtonEventHandler(this.InstallNowButton_Click);
			this.mChooseInstallFolder.mChooseFolderLabel.MouseDown += this.ChooseFolder_MouseDown;
			this.mChooseInstallFolder.mChooseFolderButton.MouseDown += this.ChooseFolder_MouseDown;
			this.mInstallerBackgroundImage.MouseLeftButtonDown += this.HandleMouseDrag;
		}

		// Token: 0x0600016C RID: 364 RVA: 0x0000F4EC File Offset: 0x0000D6EC
		private void RestartNowButton_Click(object sender, RoutedEventArgs e)
		{
			CustomMessageWindow customMessageWindow = new CustomMessageWindow();
			customMessageWindow.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_RESTART_PC", "");
			customMessageWindow.BodyTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_RESTART_MACHINE_PROMPT", "");
			customMessageWindow.AddButton(ButtonColors.Blue, "STRING_RESTART", new EventHandler(this.RestartConfirmationAcceptedHandler), null, false, null, true);
			customMessageWindow.AddButton(ButtonColors.White, "STRING_NO", null, null, false, null, true);
			customMessageWindow.Owner = this;
			customMessageWindow.ShowDialog();
			e.Handled = true;
		}

		// Token: 0x0600016D RID: 365 RVA: 0x0000272B File Offset: 0x0000092B
		private void RestartConfirmationAcceptedHandler(object sender, EventArgs e)
		{
			Logger.Info("Restarting Windows");
			Process.Start("shutdown", "/r /t 0");
		}

		// Token: 0x0600016E RID: 366 RVA: 0x0000F574 File Offset: 0x0000D774
		private void LaunchBlueStacks_Click(object sender, RoutedEventArgs e)
		{
			if (InstallerProperties.InSilentMode)
			{
				Logger.Info("BlueStacks silent installation finished, exiting");
				this.ExitApplication(0);
				return;
			}
			if (!this.mInstallFinish.mLaunchBSCheckBox.IsChecked)
			{
				Logger.Info("Finish button clicked. Not launching BlueStacks.");
				this.ExitApplication(0);
				return;
			}
			if (InstallerProperties.StartedFromMicroInstaller)
			{
				Logger.Info("Finish button clicked, launching BlueStacks client from MicroInstaller");
				this.ExitApplication(5);
				return;
			}
			try
			{
				string text = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bluestacks.conf");
				string userDefinedDir = RegistryManager.Instance.UserDefinedDir;
				string text2 = System.IO.Path.Combine(userDefinedDir, "bluestacks.conf");
				if (File.Exists(text))
				{
					if (!Directory.Exists(userDefinedDir))
					{
						Directory.CreateDirectory(userDefinedDir);
					}
					File.Copy(text, text2, true);
					Logger.Info("bluestacks.conf overwritten in UserDefinedDir: " + text2);
				}
				else
				{
					Logger.Warning("Base directory bluestacks.conf not found: " + text);
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Failed to overwrite bluestacks.conf. Ex: " + ex.Message);
			}
			Logger.Info("Finish button clicked, launching BlueStacks client from BootStrapper");
			this.RunBlueStacksAndExit();
		}

		// Token: 0x0600016F RID: 367 RVA: 0x0000F680 File Offset: 0x0000D880
		private void RunBlueStacksAndExit()
		{
			if (App.sInstallerLock != null)
			{
				App.sInstallerLock.Close();
				App.sInstallerLock = null;
			}
			string pfdir = InstallerProperties.PFDir;
			string text = System.IO.Path.Combine(pfdir, "HD-Player.exe");
			if (File.Exists(text))
			{
				try
				{
					string text2 = "--instance " + InstallerProperties.DefaultVmName;
					if (Customizations.Instance.LaunchBlueStacksElevated)
					{
						Process.Start(text, text2);
					}
					else
					{
						ProcessUtils.ExecuteProcessUnelevated(text, text2, pfdir);
					}
					this.ExitApplication(2);
					goto IL_9F;
				}
				catch (Exception ex)
				{
					Logger.Warning("Unable to launch HD-Player from {0}. Ex: {1}", new object[]
					{
						text,
						ex.Message
					});
					goto IL_9F;
				}
			}
			Logger.Error("{0} not present", new object[]
			{
				text
			});
			IL_9F:
			this.ExitApplication(0);
		}

		// Token: 0x06000170 RID: 368 RVA: 0x0000F744 File Offset: 0x0000D944
		private void InstallProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			this.mInstallProgress.mInstallProgressPercentage.Content = ((int)e.NewValue).ToString() + "%";
		}

		// Token: 0x06000171 RID: 369 RVA: 0x00002747 File Offset: 0x00000947
		private void ShowCustomInstallUserControl(object sender, MouseButtonEventArgs e)
		{
			this.mStartInstall.Visibility = Visibility.Hidden;
			this.mChooseInstallFolder.Visibility = Visibility.Visible;
			this.CheckDiskSpaceAndUpdateLabel();
		}

		// Token: 0x06000172 RID: 370 RVA: 0x0000F77C File Offset: 0x0000D97C
		private void ChooseFolder_MouseDown(object sender, MouseButtonEventArgs e)
		{
			using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
			{
				if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					string text = folderBrowserDialog.SelectedPath.ToString();
					text = System.IO.Path.Combine(text, "BlueStacks" + InstallerProperties.RegistrySuffix);
					text = text.TrimEnd(new char[]
					{
						'\\'
					});
					bool flag = IOUtils.IsDirectoryEmpty(text);
					string value = new DirectoryInfo(InstallerProperties.PFDir).ToString().TrimEnd(new char[]
					{
						'\\'
					});
					int num = text.IndexOfAny(IOUtils.DisallowedCharsInDirs);
					if (text.Equals(value, StringComparison.InvariantCultureIgnoreCase) || !flag)
					{
						CustomMessageWindow customMessageWindow = new CustomMessageWindow();
						customMessageWindow.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_CANNOT_USE_FOLDER", "");
						customMessageWindow.BodyTextBlock.Text = string.Format("{0}. {1}.", LocaleStrings.GetLocalizedString("STRING_CANT_USE_THIS_FOLDER", ""), LocaleStrings.GetLocalizedString("STRING_SELECT_FOLDER", ""));
						customMessageWindow.AddButton(ButtonColors.Blue, "STRING_SELECT_AGAIN", null, null, false, null, true);
						customMessageWindow.Owner = this;
						customMessageWindow.ShowDialog();
					}
					else if (-1 != num)
					{
						string text2 = string.Empty;
						for (int i = 0; i < IOUtils.DisallowedCharsInDirs.Length; i++)
						{
							text2 = text2 + IOUtils.DisallowedCharsInDirs[i].ToString() + "  ";
						}
						text2.TrimEnd(new char[]
						{
							' '
						});
						CustomMessageWindow customMessageWindow2 = new CustomMessageWindow();
						customMessageWindow2.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_CANNOT_USE_FOLDER", "");
						customMessageWindow2.BodyTextBlock.Text = string.Format("{0}\n\t{1}", LocaleStrings.GetLocalizedString("STRING_FOLDER_CANNOT_CONTAIN_FOLLOWING_SPECIAL_CHARS", ""), text2);
						customMessageWindow2.AddButton(ButtonColors.Blue, "STRING_SELECT_AGAIN", null, null, false, null, true);
						customMessageWindow2.Owner = this;
						customMessageWindow2.ShowDialog();
					}
					else
					{
						this.CheckDiskSpaceAndUpdateLabel();
						this.mChooseInstallFolder.mBluestacksDataLocation.Text = text;
						InstallerProperties.PDDir = text;
						this.UpdateDataDirPaths();
					}
				}
			}
		}

		// Token: 0x06000173 RID: 371 RVA: 0x0000F99C File Offset: 0x0000DB9C
		private void UpdateDataDirPaths()
		{
			string pddir = InstallerProperties.PDDir;
			Logger.Info("Updating PDDir to: " + pddir);
			InstallerProperties.PDDir = pddir;
			InstallerProperties.PDDirNew = pddir;
			InstallerProperties.PDDirOld = pddir;
		}

		// Token: 0x06000174 RID: 372 RVA: 0x0000F9D4 File Offset: 0x0000DBD4
		private void InstallNowButton_Click(object sender, RoutedEventArgs e)
		{
			if (!this.mStartInstall.mAgreeCheckBox.IsChecked)
			{
				CustomMessageWindow customMessageWindow = new CustomMessageWindow();
				customMessageWindow.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STIRNG_ACCEPT_TNC", "");
				customMessageWindow.BodyTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_AGREE_WITH_LICENSE", "");
				customMessageWindow.AddButton(ButtonColors.Blue, "STRING_OK", null, null, false, null, true);
				customMessageWindow.Owner = this;
				customMessageWindow.ShowDialog();
				return;
			}
			App.LogInstallerProperties();
			this.mCloseButton.SetDisabledState();
			this.mChooseInstallFolder.Visibility = Visibility.Hidden;
			this.mStartInstall.Visibility = Visibility.Hidden;
			this.mInstallProgress.Visibility = Visibility.Visible;
			if (InstallerProperties.IsUpgrade)
			{
				InstallerStats.Enqueue("upgrade_start", true, "", "");
			}
			else
			{
				InstallerStats.Enqueue("install_license_agreed", true, "", "");
			}
			this.StartDeploymentBackgroundWorker();
		}

		// Token: 0x06000175 RID: 373 RVA: 0x0000FABC File Offset: 0x0000DCBC
		private void StartDeploymentBackgroundWorker()
		{
			this.mBackgroundWorker = new BackgroundWorker();
			this.mBackgroundWorker.DoWork += this.BwInstallProduct_DoWork;
			this.mBackgroundWorker.ProgressChanged += this.BwInstallProduct_ProgressChanged;
			this.mBackgroundWorker.RunWorkerCompleted += this.BwInstallProduct_RunWorkerCompleted;
			this.mBackgroundWorker.WorkerReportsProgress = true;
			this.mBackgroundWorker.RunWorkerAsync();
		}

		// Token: 0x06000176 RID: 374 RVA: 0x00002767 File Offset: 0x00000967
		private void BackButton_MouseDown(object sender, MouseButtonEventArgs e)
		{
			this.mChooseInstallFolder.Visibility = Visibility.Hidden;
			this.mStartInstall.Visibility = Visibility.Visible;
		}

		// Token: 0x06000177 RID: 375 RVA: 0x0000FB30 File Offset: 0x0000DD30
		private void CheckDiskSpaceAndUpdateLabel()
		{
			long num = IOUtils.GetAvailableDiskSpaceOfDrive(InstallerProperties.PDDir) / 1048576L;
			if (num < 5120L)
			{
				CustomMessageWindow customMessageWindow = new CustomMessageWindow();
				customMessageWindow.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_INSUFFICIENT_SPACE", "");
				customMessageWindow.BodyTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_SELECTED_DRIVE_INSUFFICIENT_DISKSPACE", "");
				customMessageWindow.AddButton(ButtonColors.Blue, "STRING_CHOOSE_AGAIN", null, null, false, null, true);
				customMessageWindow.Owner = this;
				customMessageWindow.ShowDialog();
			}
			this.mChooseInstallFolder.mSpaceAvailable.Content = string.Format("{0} MB", num);
		}

		// Token: 0x06000178 RID: 376 RVA: 0x0000FBD4 File Offset: 0x0000DDD4
		private void HandleMouseDrag(object sender, MouseButtonEventArgs e)
		{
			try
			{
				base.DragMove();
			}
			catch
			{
			}
		}

		// Token: 0x06000179 RID: 377 RVA: 0x00002781 File Offset: 0x00000981
		private void MinimizeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			base.WindowState = WindowState.Minimized;
		}

		// Token: 0x0600017A RID: 378 RVA: 0x0000278A File Offset: 0x0000098A
		private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if ((sender as CustomPictureBox).ButtonState == CustomPictureBox.State.normal)
			{
				this.HandleInstallerClosing();
			}
		}

		// Token: 0x0600017B RID: 379 RVA: 0x000027A2 File Offset: 0x000009A2
		private void Installer_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
			this.HandleInstallerClosing();
		}

		// Token: 0x0600017C RID: 380 RVA: 0x0000FBFC File Offset: 0x0000DDFC
		private void HandleInstallerClosing()
		{
			if (this.mDontShowClosePopUp)
			{
				System.Windows.Application.Current.Shutdown();
				return;
			}
			CustomMessageWindow customMessageWindow = new CustomMessageWindow();
			if (this.mBackgroundWorker == null)
			{
				customMessageWindow.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_EXIT_INSTALLER", "");
				customMessageWindow.BodyTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_INSTALLATION_CANCEL_CONFIRMATION", "");
				customMessageWindow.AddButton(ButtonColors.Red, "STRING_EXIT", new EventHandler(this.InstallCancelConfirmation), null, false, null, true);
				customMessageWindow.AddButton(ButtonColors.White, "STRING_NO", null, null, false, null, true);
			}
			else
			{
				customMessageWindow.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_EXIT_INSTALLER", "");
				customMessageWindow.BodyTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_INSTALLER_CANNOT_STOP_AFTER_START", "");
				customMessageWindow.AddButton(ButtonColors.Blue, "STRING_CONTINUE", null, null, false, null, true);
			}
			customMessageWindow.Owner = this;
			customMessageWindow.ShowDialog();
		}

		// Token: 0x0600017D RID: 381 RVA: 0x000027B1 File Offset: 0x000009B1
		private void InstallCancelConfirmation(object sender, EventArgs e)
		{
			if (InstallerProperties.IsUpgrade)
			{
				InstallerStats.Enqueue("upgrade_aborted_by_user", true, "", "");
			}
			else
			{
				InstallerStats.Enqueue("install_aborted_by_user", true, "", "");
			}
			System.Windows.Application.Current.Shutdown();
		}

		// Token: 0x0600017E RID: 382 RVA: 0x0000FCE4 File Offset: 0x0000DEE4
		private void BwInstallProduct_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			this.AnimateProgressJump((double)e.ProgressPercentage);
			this.mInstallProgress.mInstallProgressBar.Value = (double)e.ProgressPercentage;
			this.mInstallProgress.mInstallProgressPercentage.Content = e.ProgressPercentage.ToString() + "%";
			this.mInstallProgress.mInstallProgressStatus.Content = e.UserState.ToString();
		}

		// Token: 0x0600017F RID: 383 RVA: 0x0000FD58 File Offset: 0x0000DF58
		private void StartProgressAnimationTimers()
		{
			this.mProgressUpdateTimer = new System.Timers.Timer
			{
				Enabled = true,
				Interval = 3000.0
			};
			this.mProgressUpdateTimer.Elapsed += this.ProgressUpdateTimer_Elapsed;
			this.mProgressTextAnimationTimer = new System.Timers.Timer
			{
				Enabled = true,
				Interval = 500.0
			};
			this.mProgressTextAnimationTimer.Elapsed += this.MProgressTextAnimationTimer_Elapsed;
			this.mProgressUpdateTimer.Start();
			this.mProgressTextAnimationTimer.Start();
		}

		// Token: 0x06000180 RID: 384 RVA: 0x0000FDEC File Offset: 0x0000DFEC
		private void AnimateProgressJump(double percentage)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(0.5);
			DoubleAnimation animation = new DoubleAnimation(percentage, timeSpan);
			this.mInstallProgress.mInstallProgressBar.BeginAnimation(RangeBase.ValueProperty, animation);
		}

		// Token: 0x06000181 RID: 385 RVA: 0x000027F0 File Offset: 0x000009F0
		private void MProgressTextAnimationTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate()
			{
				string text = (string)this.mInstallProgress.mInstallProgressStatus.Content;
				if (!text.EndsWith("."))
				{
					text += ".";
				}
				else if (text.EndsWith("..."))
				{
					text = text.Replace(".", "");
				}
				else
				{
					text += ".";
				}
				this.mInstallProgress.mInstallProgressStatus.Content = text;
			}));
		}

		// Token: 0x06000182 RID: 386 RVA: 0x0000280F File Offset: 0x00000A0F
		private void ProgressUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate()
			{
				if (this.mInstallProgress.mInstallProgressBar.Value <= (double)this.mCurrentSectionMaxProgress)
				{
					int num = (int)this.mInstallProgress.mInstallProgressBar.Value;
					this.AnimateProgressJump((double)(num + 1));
					this.mInstallProgress.mInstallProgressBar.Value = (double)(num + 1);
					this.mInstallProgress.mInstallProgressPercentage.Content = this.mInstallProgress.mInstallProgressBar.Value.ToString() + "%";
				}
			}));
		}

		// Token: 0x06000183 RID: 387 RVA: 0x0000FE2C File Offset: 0x0000E02C
		private void BwInstallProduct_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			this.mDontShowClosePopUp = true;
			try
			{
				this.mInstallFailedErrorCode = ((e.Result == null) ? -56 : ((int)e.Result));
			}
			catch (Exception ex)
			{
				Logger.Warning("Couldn't typecast e.Result. Ex: {0}", new object[]
				{
					ex.Message
				});
				this.mInstallFailedErrorCode = -56;
			}
			Logger.Info("Installer background worker completed. Result: {0}", new object[]
			{
				this.mInstallFailedErrorCode
			});
			if (e.Error != null || this.mInstallFailedErrorCode != 0)
			{
				if (e.Error != null)
				{
					Logger.Error(e.Error.ToString());
				}
				Logger.Info("Rolling back");
				this.AnimateProgressJump(75.0);
				this.mInstallProgress.mInstallProgressStatus.Content = LocaleStrings.GetLocalizedString("STRING_INSTALLATION_ROLLBACK", "");
				BackgroundWorker backgroundWorker = new BackgroundWorker();
				backgroundWorker.DoWork += this.RollBackWorker_DoWork;
				backgroundWorker.RunWorkerCompleted += this.RollBackWorker_RunWorkerCompleted;
				backgroundWorker.RunWorkerAsync(this.mInstallFailedErrorCode);
				return;
			}
			Logger.Info("Installation completed. Copying logfile to logdir.");
			InstallerUtils.CopyLogFileToLogDir(InstallerProperties.InstallerLogFilePath, string.Format("Installer_{0}.log", "5.12.120.6303"));
			this.mInstallFinish.mLaunchBSCheckBox.Visibility = Visibility.Collapsed;
			if (InstallerProperties.InSilentMode || InstallerProperties.StartedFromMicroInstaller)
			{
				this.LaunchBlueStacks_Click(null, null);
			}
			this.mInstallProgress.Visibility = Visibility.Hidden;
			this.mInstallFinish.Visibility = Visibility.Visible;
		}

		// Token: 0x06000184 RID: 388 RVA: 0x0000FFBC File Offset: 0x0000E1BC
		private void RollBackWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			this.AnimateProgressJump(100.0);
			this.mInstallProgress.mInstallProgressStatus.Content = LocaleStrings.GetLocalizedString("STRING_INSTALLATION_ROLLBACK_COMPLETE", "");
			this.mCloseButton.SetNormalState();
			int num = (int)e.Result;
			string text = InstallerUtils.ConvertIntToEnumString(this.mInstallFailedErrorCode);
			if (num != 0)
			{
				text = string.Format("{0}, {1}", text, InstallerUtils.ConvertIntToEnumString(num));
			}
			else
			{
				text = string.Format("{0}, {1}", text, "ROLLBACK_COMPLETED");
			}
			InstallerStats.SendInstallFailedEvent(text);
			if (InstallerProperties.InSilentMode)
			{
				Logger.Info("Exiting application");
				this.ExitApplication(this.mInstallFailedErrorCode);
			}
			if (!InstallerProperties.IsUpgrade)
			{
				this.ShowErrorMessageWhileRollback(this.mInstallFailedErrorCode, "STRING_INSTALLATION_FAILED");
				return;
			}
			if (num == 0)
			{
				this.ShowErrorMessageWhileRollback(this.mInstallFailedErrorCode, "STRING_UNFORTUNATELY_COULDNT_UPGRADE");
				return;
			}
			this.ShowErrorMessageWhileRollback(this.mInstallFailedErrorCode, "STRING_ROLLBACK_FAILED_SORRY_MESSAGE");
		}

		// Token: 0x06000185 RID: 389 RVA: 0x000100AC File Offset: 0x0000E2AC
		private void ShowErrorMessageWhileRollback(int mInstallFailedErrorCode, string prefixKey)
		{
			string text = InstallerErrorHandling.AssignErrorStringForInstallerExitCodes(mInstallFailedErrorCode, prefixKey);
			CustomMessageWindow customMessageWindow = new CustomMessageWindow();
			if (InstallerProperties.IsUpgrade)
			{
				customMessageWindow.TitleTextBlock.Text = string.Format(LocaleStrings.GetLocalizedString("STRING_UPGRADE_FAILED", ""), new object[0]);
			}
			else
			{
				customMessageWindow.TitleTextBlock.Text = string.Format(LocaleStrings.GetLocalizedString("STRING_INSTALL_FAIL", ""), new object[0]);
			}
			if (mInstallFailedErrorCode == -21)
			{
				customMessageWindow.TitleTextBlock.Text = string.Format(LocaleStrings.GetLocalizedString("STRING_CANT_INSTALL_BLUESTACKS", ""), new object[0]);
			}
			customMessageWindow.BodyTextBlock.Text = text;
			if (mInstallFailedErrorCode != -12 && mInstallFailedErrorCode != -66 && mInstallFailedErrorCode != -26)
			{
				customMessageWindow.AddButton(ButtonColors.Blue, LocaleStrings.GetLocalizedString("STRING_EXIT", ""), new EventHandler(this.RollbackExitHandler), null, false, null, true);
			}
			if (mInstallFailedErrorCode == -66)
			{
				customMessageWindow.BodyTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_VALID_CERT_ERROR", "");
				customMessageWindow.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_UNABLE_TO_INSTALL", "");
				customMessageWindow.AddButton(ButtonColors.Blue, LocaleStrings.GetLocalizedString("STRING_READ_ARTICLE", ""), new EventHandler(this.DriverFailedToStartErrorHandler), null, false, null, true);
			}
			if (mInstallFailedErrorCode != -26)
			{
				switch (mInstallFailedErrorCode)
				{
				case -20:
					customMessageWindow.BodyTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_HYPERV_COMPUTE_PLATFORM_DISABLED_WARNING", "");
					customMessageWindow.AddButton(ButtonColors.White, "STRING_CHECK_FAQ", delegate(object sender1, EventArgs e1)
					{
						Utils.OpenUrl(WebHelper.GetUrlWithParams(Strings.ImageName, string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[]
						{
							WebHelper.GetServerHost(),
							"help_articles"
						}), "5.12.120.6303", "5.12.120.6303", string.IsNullOrEmpty(Customizations.Instance.HelpArticleLocale) ? Globalization.FindClosestMatchingLocale(Thread.CurrentThread.CurrentCulture.Name) : Customizations.Instance.HelpArticleLocale, InstallerProperties.GUID) + "&article=hypervisor_compute_platform_guide");
					}, null, false, null, true);
					break;
				case -19:
					customMessageWindow.BodyTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_HYPERV_DISABLED_MESSAGE", "");
					customMessageWindow.AddButton(ButtonColors.White, "STRING_CHECK_FAQ", delegate(object sender1, EventArgs e1)
					{
						Utils.OpenUrl(WebHelper.GetUrlWithParams(Strings.ImageName, string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[]
						{
							WebHelper.GetServerHost(),
							"help_articles"
						}), "5.12.120.6303", "5.12.120.6303", string.IsNullOrEmpty(Customizations.Instance.HelpArticleLocale) ? Globalization.FindClosestMatchingLocale(Thread.CurrentThread.CurrentCulture.Name) : Customizations.Instance.HelpArticleLocale, InstallerProperties.GUID) + "&article=enable_hyperv");
					}, null, false, null, true);
					break;
				case -18:
					customMessageWindow.AddButton(ButtonColors.Red, "STRING_CHECK_FAQ", new EventHandler(this.CheckHyperVFAQExitHandler), null, false, null, true);
					break;
				case -17:
					customMessageWindow.AddButton(ButtonColors.Red, "STRING_CHECK_FAQ", new EventHandler(this.CheckFAQVTxExitHandler), null, false, null, true);
					break;
				case -12:
					customMessageWindow.AddButton(ButtonColors.Blue, LocaleStrings.GetLocalizedString("STRING_CLOSE", ""), new EventHandler(this.RollbackExitHandler), null, false, null, true);
					break;
				}
			}
			else
			{
				customMessageWindow.TitleTextBlock.Text = string.Format(LocaleStrings.GetLocalizedString("STRING_UPDATE_DRIVERS_TITLE", ""), new object[0]);
				customMessageWindow.AddButton(ButtonColors.Blue, LocaleStrings.GetLocalizedString("STRING_VIEW_ARTICLE", ""), new EventHandler(this.OpenUpdateGraphicsDriverArticleHandler), null, false, null, true);
			}
			customMessageWindow.CloseButtonHandle(new EventHandler(this.RollbackExitHandler), null);
			customMessageWindow.Owner = this;
			customMessageWindow.ShowDialog();
		}

		// Token: 0x06000186 RID: 390 RVA: 0x0001039C File Offset: 0x0000E59C
		private void OpenUpdateGraphicsDriverArticleHandler(object sender, EventArgs e)
		{
			string text = string.Format("{0}/bs3/{1}?article={2}&oem={3}&locale={4}&image_name={5}", new object[]
			{
				"https://cloud.bluestacks.com",
				"help_articles",
				"update_graphics_card_drivers",
				"msi5",
				string.IsNullOrEmpty(Customizations.Instance.HelpArticleLocale) ? Globalization.FindClosestMatchingLocale(Thread.CurrentThread.CurrentCulture.Name) : Customizations.Instance.HelpArticleLocale,
				Strings.ImageName
			});
			try
			{
				Process.Start(text);
			}
			catch (Exception ex)
			{
				Logger.Error("Not able to launch the url {0}. Ignoring Ex: {1}", new object[]
				{
					text,
					ex.Message
				});
			}
			this.RollbackExitHandler(sender, e);
		}

		// Token: 0x06000187 RID: 391 RVA: 0x00010458 File Offset: 0x0000E658
		private void LaunchBsxCloud(object sender, EventArgs e)
		{
			try
			{
				MicroInstallerSharedUtils.LaunchBsx("-cloudgames");
				InstallerStats.Enqueue("install_bsx_launched", false, InstallerProperties.FailureCodes.ToString(), "");
			}
			catch (Exception ex)
			{
				Logger.Error("Couldn't launch BSX. Ex: {0}", new object[]
				{
					ex
				});
				InstallerStats.Enqueue("install_bsx_launch_failed", false, InstallerProperties.FailureCodes.ToString(), "");
			}
			this.ExitApplication(this.mInstallFailedErrorCode);
		}

		// Token: 0x06000188 RID: 392 RVA: 0x000104EC File Offset: 0x0000E6EC
		private void DriverFailedToStartErrorHandler(object sender, EventArgs e)
		{
			string empty = string.Empty;
			try
			{
				Utils.OpenUrl(string.Format("{0}/bs3/{1}?article={2}&oem={3}&locale={4}&guid={5}&image_name={6}", new object[]
				{
					"https://cloud.bluestacks.com",
					"help_articles",
					"valid_cert_update",
					"msi5",
					string.IsNullOrEmpty(Customizations.Instance.HelpArticleLocale) ? Globalization.FindClosestMatchingLocale(Thread.CurrentThread.CurrentCulture.Name) : Customizations.Instance.HelpArticleLocale,
					InstallerProperties.GUID,
					Strings.ImageName
				}));
			}
			catch (Exception ex)
			{
				Logger.Warning("Not able to launch the url {0}. Ignoring Ex: {1}", new object[]
				{
					empty,
					ex.Message
				});
			}
			this.ExitApplication(this.mInstallFailedErrorCode);
		}

		// Token: 0x06000189 RID: 393 RVA: 0x000105B8 File Offset: 0x0000E7B8
		private void CheckFAQVTxExitHandler(object sender, EventArgs e)
		{
			string text = string.Empty;
			try
			{
				text = WebHelper.GetUrlWithParams(Strings.ImageName, string.Format("{0}/{1}", WebHelper.GetServerHost(), "help_articles"), "5.12.120.6303", "5.12.120.6303", Customizations.Instance.HelpArticleLocale, InstallerProperties.GUID);
				text = string.Format("{0}&article={1}", text, "enable_virtualization");
				Utils.OpenUrl(text);
			}
			catch (Exception ex)
			{
				Logger.Warning("Not able to launch the url {0}. Ignoring Ex: {1}", new object[]
				{
					text,
					ex.Message
				});
			}
			this.ExitApplication(this.mInstallFailedErrorCode);
		}

		// Token: 0x0600018A RID: 394 RVA: 0x00010658 File Offset: 0x0000E858
		private void CheckHyperVFAQExitHandler(object sender, EventArgs e)
		{
			string text = string.Empty;
			try
			{
				text = WebHelper.GetUrlWithParams(Strings.ImageName, "https://support.bluestacks.com/hc/articles/115004254383", "5.12.120.6303", "5.12.120.6303", null, InstallerProperties.GUID);
				Utils.OpenUrl(text);
			}
			catch (Exception ex)
			{
				Logger.Warning("Not able to launch the url {0}. Ignoring Ex: {1}", new object[]
				{
					text,
					ex.Message
				});
			}
			this.ExitApplication(this.mInstallFailedErrorCode);
		}

		// Token: 0x0600018B RID: 395 RVA: 0x000106D0 File Offset: 0x0000E8D0
		private void RollbackExitHandler(object sender, EventArgs e)
		{
			InstallerCodes installerCodes = (InstallerCodes)this.mInstallFailedErrorCode;
			if (installerCodes == InstallerCodes.SERVICE_MARKED_FOR_DELETION || installerCodes == InstallerCodes.FAILED_CREATING_REGISTRY || installerCodes == InstallerCodes.PROGRAM_FILES_ZIP_EXTRACT_FAILED || installerCodes == InstallerCodes.PROGRAM_DATA_ZIP_EXTRACT_FAILED || installerCodes == InstallerCodes.FAILED_TO_INSTALL_DRIVER || installerCodes == InstallerCodes.FAILED_TO_CREATE_BSTKGLOBAL || installerCodes == InstallerCodes.FAILED_TO_RESERVE_HTTP_PORTS || installerCodes == InstallerCodes.FAILED_TO_RESTORE_ANDROID_DATA || installerCodes == InstallerCodes.FAILED_TO_MOVE_OLD_ITEMS || installerCodes == InstallerCodes.FAILED_TO_RENAME_NEW_ITEMS)
			{
				this.mInstallError.Visibility = Visibility.Visible;
				this.mInstallError.mRestartNowButton.Visibility = Visibility.Visible;
				this.mInstallProgress.Visibility = Visibility.Hidden;
				this.mInstallFinish.Visibility = Visibility.Hidden;
				this.mProductLogo.Visibility = Visibility.Hidden;
				return;
			}
			this.ExitApplication(this.mInstallFailedErrorCode);
		}

		// Token: 0x0600018C RID: 396 RVA: 0x0001076C File Offset: 0x0000E96C
		private void RollBackWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			Logger.Info("Rollback Worker start");
			InstallerUtils.LogParentHash();
			int num;
			if (InstallerProperties.IsUpgrade)
			{
				num = Rollback.RollbackUpgrade();
			}
			else
			{
				num = Rollback.RollbackFresh();
			}
			if (num != 0)
			{
				Logger.Warning("Rollback wasn't successful");
			}
			if (InstallerUtils.CheckIfTestRollbackFail())
			{
				num = -256;
			}
			e.Result = num;
		}

		// Token: 0x0600018D RID: 397 RVA: 0x000107C8 File Offset: 0x0000E9C8
		private void BwInstallProduct_DoWork(object sender, DoWorkEventArgs e)
		{
			this.UpdateProgress(1, "STRING_PRE_INSTALL_CHECKS", 20);
			this.StartProgressAnimationTimers();
			InstallerUtils.SendOSStatAsync();
			InstallerUtils.LogSpaceAvailableInDrive(InstallerProperties.PDDirNew, InstallerProperties.PFDir);
			if (!PreInstallChecks.SpaceAvailableInInstallerExtractDict())
			{
				e.Result = -14;
				return;
			}
			try
			{
				InstallerUtils.ExtractZip(System.IO.Path.Combine(InstallerProperties.InstallerExtractedDir, "CommonInstallUtils.zip"), InstallerProperties.InstallerExtractedDir, null);
			}
			catch (Exception ex)
			{
				Logger.Fatal("Common Install Utils extraction failed " + ex.ToString());
				e.Result = -5;
				return;
			}
			string text = SystemUtils.IsOs64Bit() ? System.IO.Path.Combine(InstallerProperties.InstallerExtractedDir, "QtRedistx64.zip") : System.IO.Path.Combine(InstallerProperties.InstallerExtractedDir, "QtRedistx86.zip");
			try
			{
				InstallerUtils.ExtractZip(text, InstallerProperties.InstallerExtractedDir, null);
			}
			catch
			{
				Logger.Fatal("Qt DLLs extraction failed");
				e.Result = -5;
				return;
			}
			InstallerStats.Enqueue("install_commonutils_extracted", true, "", "");
			int num = InstallerUtils.BridgeApiInit(1);
			if (num != 0)
			{
				InstallerUtils.ExtractVCDependencies(InstallerProperties.InstallerExtractedDir, null);
				num = InstallerUtils.BridgeApiInit(2);
				if (num != 0)
				{
					InstallerUtils.InstallVC();
					num = InstallerUtils.BridgeApiInit(3);
					if (num != 0)
					{
						Logger.Fatal("Bridge not inited after 3 tries");
						if (num == 2147483647)
						{
							e.Result = -22;
							return;
						}
					}
				}
			}
			if (InstallerProperties.IsUpgrade && (num != 0 || ConfReader.InstalledImages.Count == 0))
			{
				Cleaner.AskUserToContinueCleaningOrExit(this);
				try
				{
					Logger.Info("Trying to init BridgeApi after corrupt install popup");
					num = BridgeApis.ApiInitEx("Installer", InstallerProperties.IsFreshInstall, false, InstallerProperties.InstallerLogFilePath);
				}
				catch (Exception ex2)
				{
					Logger.Error("BridgeApiInitEx failed. Ex: {0}", new object[]
					{
						ex2
					});
					InstallerStats.Enqueue("install_bridge_init_failed", false, ex2.Message, "clean bridge init");
				}
			}
			if (num != 0)
			{
				InstallerStats.Enqueue("install_bridge_init_failed", false, num.ToString(), "");
				e.Result = -22;
				return;
			}
			InstallerStats.Enqueue("install_bridge_inited", false, "", "");
			Logger.Info("BrdigeApi initialized!");
			InstallerUtils.AssignHypervisorProperties();
			if (Cleaner.CleanRemnants(this))
			{
				try
				{
					Logger.Info("Trying to init BridgeApi after cleaning Remnants");
					num = BridgeApis.ApiInitEx("Installer", InstallerProperties.IsFreshInstall, false, InstallerProperties.InstallerLogFilePath);
				}
				catch (Exception ex3)
				{
					Logger.Error("BridgeApiInitEx failed. Ex: {0}", new object[]
					{
						ex3
					});
					InstallerStats.Enqueue("install_bridge_init_failed", false, ex3.Message, "clean bridge init");
				}
				if (num != 0)
				{
					InstallerStats.Enqueue("install_bridge_init_failed", false, num.ToString(), "");
					e.Result = -22;
					return;
				}
			}
			if (InstallerProperties.IsUpgrade)
			{
				bool flag;
				InstallerUtils.StopBlueStacks(out flag);
				if (!flag)
				{
					this.ShowRestartPromptToUser();
				}
			}
			if (InstallerUtils.IsDriverMarkedForDeletion())
			{
				Logger.Fatal("BlueStacks service marked for deletion");
				e.Result = -30;
				return;
			}
			if (InstallerProperties.IsFreshInstall)
			{
				InstallerProperties.VmList = new List<string>
				{
					InstallerProperties.VmToLaunch
				};
				InstallerProperties.ImageList = InstallerProperties.VmList;
			}
			else
			{
				InstallerProperties.VmList = ConfReader.VmList;
				InstallerProperties.ImageList = ConfReader.InstalledImages;
				InstallerProperties.DefaultVmName = InstallerProperties.VmList.FirstOrDefault<string>();
			}
			Logger.Info("Images to install: {0}", new object[]
			{
				string.Join(", ", InstallerProperties.ImageList.ToArray())
			});
			Logger.Info("VMs: {0}", new object[]
			{
				string.Join(", ", InstallerProperties.VmList.ToArray())
			});
			InstallerUtils.SetupForcedDedicatedGPUConfiguration();
			InstallerStats.Enqueue("install_checks_started", false, "", "");
			if (InstallerProperties.IsUpgrade)
			{
				long num2;
				string driveLetter;
				while (!PreInstallChecks.IsRequiredDiskSpaceAvailable(out num2, out driveLetter))
				{
					if (InstallerProperties.InSilentMode)
					{
						e.Result = -14;
						return;
					}
					this.ShowInsufficientDiskSpaceWarningPopup(num2 / 1048576L, driveLetter);
				}
			}
			e.Result = PreInstallChecks.PerformPreInstallChecks();
			if ((int)e.Result != 0)
			{
				if ((int)e.Result == -25)
				{
					this.ShowUpgradeToWin7SP1Popup();
				}
				Logger.Fatal("Preinstall checks failed: {0}", new object[]
				{
					e.Result
				});
				return;
			}
			InstallerStats.Enqueue(InstallerProperties.IsUpgrade ? "upgrade_checks_passed" : "install_checks_passed", false, "", "");
			InstallerUtils.SendSystemInfoStatsAsync();
			if (!InstallerUtils.IsProcessorIntel())
			{
				Logger.Info("Setting the Environment variable VBOX_HWVIRTEX_IGNORE_SVM_IN_USE");
				Environment.SetEnvironmentVariable("VBOX_HWVIRTEX_IGNORE_SVM_IN_USE", "1", EnvironmentVariableTarget.Machine);
			}
			Strings.ImageName = InstallerProperties.DefaultVmName;
			e.Result = this.DeployBlueStacksFiles(text);
			if ((int)e.Result != 0)
			{
				Logger.Fatal("Deployment failed. Ret: {0}", new object[]
				{
					e.Result
				});
				return;
			}
			this.UpdateProgress(56, "STRING_INSTALLING_ENGINE", InstallerProperties.IsFreshInstall ? 70 : 60);
			InstallerUtils.RemoveAndAddFirewallRule("BlueStacks Service", System.IO.Path.Combine(InstallerProperties.PFDir, "HD-Player.exe"));
			InstallerUtils.RemoveAndAddFirewallRule("BlueStacksAppplayerWeb", System.IO.Path.Combine(InstallerProperties.PFDir, "BlueStacksAppplayerWeb.exe"));
			e.Result = InstallerUtils.CreateAndUpdateConf(System.IO.Path.Combine(InstallerProperties.PDDirNew, "bluestacks.conf"));
			if ((int)e.Result != 0)
			{
				Logger.Fatal("Error in setting conf. Ret: {0}", new object[]
				{
					e.Result
				});
				return;
			}
			e.Result = InstallerUtils.CreateAllVmBstkFileFromTemplate();
			if ((int)e.Result != 0)
			{
				Logger.Fatal("Could not generate Android.bstk from template");
				InstallerUtils.LogAllServiceNames();
				return;
			}
			if (InstallerProperties.IsFreshInstall)
			{
				e.Result = InstallerUtils.SetupBstkGlobalConfig();
				if ((int)e.Result != 0)
				{
					Logger.Fatal("Could not setup BstkGlobal");
					return;
				}
			}
			e.Result = InstallerUtils.InstallDriver(System.IO.Path.Combine(InstallerProperties.PFDir, "BstkDrv_msi5.sys"));
			if ((int)e.Result != 0)
			{
				InstallerUtils.LogAllServiceNames();
				return;
			}
			if (!InstallerProperties.IsMicrosoftHyperVEnabled)
			{
				if (ServiceManager.StartService(Strings.BlueStacksDriverName, false) == 577)
				{
					e.Result = -66;
					return;
				}
				ServiceManager.StopDriver(15);
			}
			if (InstallerProperties.IsUpgrade)
			{
				this.UpdateProgress(61, "STRING_INSTALLING_ENGINE", 70);
				ImageBasedUpgradeManager imageBasedUpgradeManager = new ImageBasedUpgradeManager();
				e.Result = imageBasedUpgradeManager.HandleEngineDataUpgrade();
				if ((int)e.Result != 0)
				{
					Logger.Fatal("Could not upgrade engine");
					InstallerUtils.LogPFPDDirs();
					return;
				}
			}
			if (InstallerProperties.IsUpgrade)
			{
				System.Version v = new System.Version(InstallerProperties.OldVersion);
				System.Version v2 = new System.Version("5.8.0");
				string path = System.IO.Path.Combine(System.IO.Path.Combine(InstallerProperties.PDDir, "Engine"), "diskOptimization_onboarding");
				string path2 = System.IO.Path.Combine(System.IO.Path.Combine(InstallerProperties.PDDirNew, "Engine"), "diskOptimization_onboarding");
				if (File.Exists(path) || v < v2)
				{
					using (File.Create(path2))
					{
					}
				}
			}
			if (!InstallerUtils.SetupEngineFilesForHypervisor())
			{
				Logger.Fatal("Could not setup engine files for Hypervisor");
				e.Result = -31;
				return;
			}
			if (InstallerProperties.IsFreshInstall)
			{
				InstallerUtils.CreateDataOrigForDeployedImages();
			}
			if (InstallerProperties.IsUpgrade)
			{
				e.Result = InstallerUtils.RenameDirectoriesToFinalStructure();
				if ((int)e.Result != 0)
				{
					InstallerUtils.LogPFPDDirs();
					return;
				}
			}
			if (InstallerUtils.CheckIfTestRollback())
			{
				e.Result = -255;
				return;
			}
			if (!InstallerUtils.CreateBlueStacksRegistry())
			{
				Logger.Fatal("Registry creation was not successful");
				e.Result = -31;
				return;
			}
			InstallerUtils.StartComReregistration();
			if (InstallerProperties.IsFreshInstall)
			{
				string imagePath = System.IO.Path.Combine(InstallerProperties.PDDir, System.IO.Path.Combine("Engine", InstallerProperties.DefaultVmName));
				if (!new ImageInstaller().RegisterVm(imagePath, InstallerProperties.DefaultVmName))
				{
					Logger.Fatal("Couldn't register VM");
					e.Result = -65;
					return;
				}
				foreach (string str in InstallerProperties.ImageList)
				{
					InstallerUtils.FixDataOrigUuid(System.IO.Path.Combine(System.IO.Path.Combine(InstallerProperties.PDDir, "Engine\\" + str), "Data_orig.vhdx"));
				}
			}
			this.UpdateProgress(71, "STRING_CREATE_SHORTCUTS", 75);
			if (InstallerProperties.IsUpgrade)
			{
				InstallerUtils.DeleteOldBlueStacksData();
				System.Version v3 = new System.Version(InstallerProperties.OldVersion);
				System.Version v4 = new System.Version("5.2.0.0");
				if (v3 < v4)
				{
					MacroConverter.Convert(RegistryStrings.MacroRecordingsFolderPath, 3);
				}
			}
			InstallerUtils.CreateInstallApkScheme(InstallerProperties.PFDir, Strings.ImageName);
			string text2 = System.IO.Path.Combine(InstallerProperties.PFDir, "ProductLogo.ico");
			bool flag2 = !string.IsNullOrEmpty(MicroInstallerSharedUtils.BsxRegistry.Run()) && File.Exists(MicroInstallerSharedUtils.BsxRegistry.Run());
			if ((!App.sOpt.skipShortcuts && !App.sOpt.skipBinaryShortcuts) || (App.sOpt.skipBinaryShortcuts && !flag2) || !InstallerProperties.UpgradeFromUnificationBuild)
			{
				InstallerUtils.DeleteOldShortcuts();
				InstallerUtils.CreateDesktopAndStartMenuShortcuts("FRΛMΣx.lnk", text2, System.IO.Path.Combine(RegistryManager.Instance.InstallDir, "HD-Player.exe"), "--instance " + Strings.ImageName, "", "");
				InstallerUtils.CreateNotificationActivatorRegistry();
				InstallerUtils.CreateAppUserModelIdRegistry(text2);
				InstallerUtils.CreateWebUrlScheme(text2);
				InstallerUtils.CreateWebUrlSchemeForLogCollector(text2);
				InstallerUtils.RecreateAddRemoveRegistry(InstallerProperties.PFDir, text2, "FRΛMΣx", "floatz.gg, Inc.");
				try
				{
					if (InstallerProperties.IsMicrosoftHyperVEnabled)
					{
						MainWindow.AddToHvAdmins();
					}
				}
				catch (Exception ex4)
				{
					Logger.Warning("Some error while ensuring admin privileges. Ex: {0}", new object[]
					{
						ex4.Message
					});
				}
				if (InstallerProperties.IsUpgrade)
				{
					InstallerStats.Enqueue("upgrade_completed", false, "", "");
				}
				else
				{
					InstallerStats.Enqueue("install_completed", false, "", "");
				}
				if (Customizations.Instance.InstallBlueStacksHelper)
				{
					InstallerUtils.InstallUpdateHelper();
				}
				if (!App.sOpt.skipSysPrep && !InstallerProperties.IsUpgrade)
				{
					this.UpdateProgress(76, "STRING_INITIATING_PERFORMANCE_BOOST", 90);
					InstallerUtils.BootPlayerAfterInstall();
				}
				else
				{
					Logger.Info("Skipping sysprep. SkipSysPrep : {0} IsUpgrade : {1}", new object[]
					{
						App.sOpt.skipSysPrep,
						InstallerProperties.IsUpgrade
					});
				}
				this.UpdateProgress(90, "STRING_APPLYING_OPTIMIZATIONS", 98);
				MainWindow.Litemax();
				try
				{
					Logger.Info("ADB optimizations completed successfully");
				}
				catch (Exception ex5)
				{
					Logger.Error("Exception while applying ADB optimizations: " + ex5.ToString());
				}
				if (InstallerProperties.IsUpgrade)
				{
					try
					{
						new ComRegistration(System.IO.Path.Combine(InstallerProperties.InstallerExtractedDir, InstallerProperties.PFDir)).Register();
					}
					catch (Exception ex6)
					{
						Logger.Error(ex6.ToString());
					}
				}
				if (App.sOpt.isWalletFeatureEnabled)
				{
					InstallerUtils.WaitForWalletComplition();
				}
				Logger.Info("Installation completed 100%");
				this.UpdateProgress(99, "STRING_INSTALL_FINISH", 100);
				InstallerStats.Enqueue("install_sysprep_completed", false, "", "");
				e.Result = 0;
			}
		}

		// Token: 0x0600018E RID: 398 RVA: 0x0001129C File Offset: 0x0000F49C
		internal static void AddToHvAdmins()
		{
			try
			{
				Logger.Info("Adding current user to HyperV Administrators group");
				WindowsIdentity current = WindowsIdentity.GetCurrent();
				string cmd = System.IO.Path.Combine(InstallerProperties.PFDir, "HD-Hvutl.exe");
				string args = string.Format("--cmd addToHyperVAdmin --sid {0}", current.User);
				int exitCode = RunCommand.RunCmd(cmd, args, true, false, 0).ExitCode;
				if (exitCode != 0)
				{
					Logger.Warning("Unable to add user to HyperV admin group exitcode: {0}", new object[]
					{
						exitCode
					});
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Couldn't add user to hyperv admin group. Ex: {0}", new object[]
				{
					ex
				});
			}
		}

		// Token: 0x0600018F RID: 399 RVA: 0x0000282E File Offset: 0x00000A2E
		private void ShowRestartPromptToUser()
		{
			base.Dispatcher.Invoke(new Action(delegate()
			{
				string text = string.Format("{0} {1}", LocaleStrings.GetLocalizedString("STRING_OLD_INSTALLATION_INTERFERING", ""), LocaleStrings.GetLocalizedString("STRING_TRY_RESTARTING_MACHINE", ""));
				CustomMessageWindow customMessageWindow = new CustomMessageWindow();
				customMessageWindow.BodyTextBlock.Text = text;
				customMessageWindow.TitleTextBlock.Text = LocaleStrings.GetLocalizedString("STRING_INSTALLATION_ERROR", "");
				customMessageWindow.AddButton(ButtonColors.Blue, LocaleStrings.GetLocalizedString("STRING_RESTART_PC", ""), new EventHandler(this.RestartConfirmationAcceptedHandler), null, false, null, true);
				customMessageWindow.AddButton(ButtonColors.White, LocaleStrings.GetLocalizedString("STRING_CANCEL", ""), null, null, false, null, true);
				customMessageWindow.CloseButtonHandle(null, null);
				customMessageWindow.Owner = this;
				customMessageWindow.ShowDialog();
				this.ExitApplication(-16);
			}), new object[0]);
		}

		// Token: 0x06000190 RID: 400 RVA: 0x00011330 File Offset: 0x0000F530
		private void DeployAstcDecoderLib()
		{
			string[] array = new string[]
			{
				"HD-Astcdecoder_AVX1.dll",
				"HD-Astcdecoder_SSE42.dll",
				"HD-Astcdecoder_SSE20.dll",
				"HD-Astcdecoder.dll"
			};
			bool[] array2 = new bool[]
			{
				MainWindow.cpu_supports_avx1(),
				MainWindow.cpu_supports_sse42(),
				MainWindow.cpu_supports_sse20(),
				true
			};
			int num = array.Length - 1;
			int i = 0;
			while (i < array2.Length)
			{
				if (array2[i])
				{
					Logger.Info("This machine prefers ASTC decoder library {0}.", new object[]
					{
						array[i]
					});
					for (int j = 0; j < array.Length; j++)
					{
						if (j != i)
						{
							string path = System.IO.Path.Combine(InstallerProperties.PFDirNew, array[j]);
							if (File.Exists(path))
							{
								File.Delete(path);
							}
						}
					}
					if (i != num)
					{
						string sourceFileName = System.IO.Path.Combine(InstallerProperties.PFDirNew, array[i]);
						string destFileName = System.IO.Path.Combine(InstallerProperties.PFDirNew, array[num]);
						File.Move(sourceFileName, destFileName);
						return;
					}
					break;
				}
				else
				{
					i++;
				}
			}
		}

		// Token: 0x06000191 RID: 401 RVA: 0x00011424 File Offset: 0x0000F624
		private int DeployBlueStacksFiles(string qtZipFilePath)
		{
			this.UpdateProgress(21, "STRING_EXTRACTING_FILES", 30);
			Logger.Info("Deploying BlueStacks Files");
			string zipFilePath = System.IO.Path.Combine(InstallerProperties.InstallerExtractedDir, "PF.zip");
			string zipFilePath2 = System.IO.Path.Combine(InstallerProperties.InstallerExtractedDir, "PD.zip");
			string pfdirNew = InstallerProperties.PFDirNew;
			int num = InstallerUtils.ExtractZip(zipFilePath, pfdirNew, this).ExitCode;
			int result;
			if (num != 0)
			{
				Logger.Fatal("Failed to extract PF.zip. ExitCode: {0}", new object[]
				{
					num
				});
				InstallerUtils.LogDir(pfdirNew);
				result = -33;
			}
			else
			{
				this.DeployAstcDecoderLib();
				num = InstallerUtils.ExtractZip(qtZipFilePath, pfdirNew, this).ExitCode;
				if (num != 0)
				{
					Logger.Fatal("Failed to Qt to pf. ExitCode: {0}", new object[]
					{
						num
					});
					result = -33;
				}
				else
				{
					if (InstallerProperties.VCDependenciesExtracted)
					{
						InstallerUtils.ExtractVCDependencies(pfdirNew, this);
					}
					this.UpdateProgress(31, "STRING_EXTRACTING_FILES", 35);
					string pddirNew = InstallerProperties.PDDirNew;
					num = InstallerUtils.ExtractZip(zipFilePath2, pddirNew, this).ExitCode;
					if (num != 0)
					{
						Logger.Error("Failed to extract PD.zip. ExitCode: {0}", new object[]
						{
							num
						});
						InstallerUtils.LogDir(pddirNew);
						result = -35;
					}
					else
					{
						this.UpdateProgress(36, "STRING_EXTRACTING_FILES", 55);
						num = InstallerUtils.ExtractAndroidImage(this);
						if (num != 0)
						{
							Logger.Error("Failed to extract android image. ExitCode: {0}", new object[]
							{
								num
							});
							result = num;
						}
						else
						{
							InstallerUtils.ModifyDirectoryPermissionsForEveryone(pddirNew);
							result = 0;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000192 RID: 402 RVA: 0x0000284E File Offset: 0x00000A4E
		private void UpdateProgress(int percentage, string messageToLocalize, int maxProgress)
		{
			if (maxProgress != this.mCurrentSectionMaxProgress)
			{
				this.mCurrentSectionMaxProgress = maxProgress;
			}
			this.mBackgroundWorker.ReportProgress(percentage, LocaleStrings.GetLocalizedString(messageToLocalize, ""));
		}

		// Token: 0x06000193 RID: 403 RVA: 0x0000287C File Offset: 0x00000A7C
		public void ShowWindow()
		{
			base.Visibility = Visibility.Visible;
			base.Show();
			base.BringIntoView();
			if (!base.Topmost)
			{
				base.Topmost = true;
				base.Topmost = false;
			}
		}

		// Token: 0x06000194 RID: 404 RVA: 0x00011588 File Offset: 0x0000F788
		private void ExitApplication(int errorCode = 0)
		{
			this.StopProgressAnimationTimers();
			Logger.Info("Waiting for stats queue to end");
			InstallerStats.sStatsQueue.Stop();
			InstallerStats.sStatsQueue.Join();
			Logger.Info(string.Format("Exiting with error code {0}", errorCode));
			App.ExitApplication(errorCode);
		}

		// Token: 0x06000195 RID: 405 RVA: 0x000115D4 File Offset: 0x0000F7D4
		private void StopProgressAnimationTimers()
		{
			if (this.mProgressUpdateTimer != null && this.mProgressUpdateTimer.Enabled)
			{
				this.mProgressUpdateTimer.Stop();
			}
			if (this.mProgressTextAnimationTimer != null && this.mProgressTextAnimationTimer.Enabled)
			{
				this.mProgressTextAnimationTimer.Stop();
			}
			if (this.mBackgroundChangeTimer != null && this.mBackgroundChangeTimer.Enabled)
			{
				this.mBackgroundChangeTimer.Stop();
			}
		}

		// Token: 0x06000199 RID: 409 RVA: 0x000117B4 File Offset: 0x0000F9B4
		private void SetupSettingsPanel()
		{
			if (InstallerProperties.InSilentMode)
			{
				return;
			}
			try
			{
				this.EnsureMusicPlayer();
				this.CreateSettingsPopup();
				if (this.mSettingsOverlay != null)
				{
					this.AddPremiumSettingsButton();
					this.CreateMuteButton();
					this.StartBackgroundMusic();
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Settings panel setup failed. Ex: " + ex.Message);
			}
		}

		// Token: 0x0600019A RID: 410 RVA: 0x000028B4 File Offset: 0x00000AB4
		private void ToggleSettingsPanel()
		{
			if (this.mSettingsOverlay == null)
			{
				return;
			}
			if (this.mSettingsOverlay.Visibility == Visibility.Visible)
			{
				this.CloseSettingsPanel();
				return;
			}
			this.OpenSettingsPanel();
		}

		// Token: 0x0600019B RID: 411 RVA: 0x0001181C File Offset: 0x0000FA1C
		private void OpenSettingsPanel()
		{
			this.mSettingsOverlay.Visibility = Visibility.Visible;
			DoubleAnimation animation = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(180.0));
			this.mSettingsOverlay.BeginAnimation(UIElement.OpacityProperty, animation);
			ScaleTransform scaleTransform = this.mSettingsPanelBorder.RenderTransform as ScaleTransform;
			if (scaleTransform != null)
			{
				DoubleAnimation animation2 = new DoubleAnimation(0.94, 1.0, TimeSpan.FromMilliseconds(220.0));
				scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation2);
				scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation2);
			}
		}

		// Token: 0x0600019C RID: 412 RVA: 0x000118C8 File Offset: 0x0000FAC8
		private void CloseSettingsPanel()
		{
			DoubleAnimation doubleAnimation = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(150.0));
			doubleAnimation.Completed += delegate(object s, EventArgs e)
			{
				this.mSettingsOverlay.Visibility = Visibility.Collapsed;
			};
			this.mSettingsOverlay.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
			ScaleTransform scaleTransform = this.mSettingsPanelBorder.RenderTransform as ScaleTransform;
			if (scaleTransform != null)
			{
				DoubleAnimation animation = new DoubleAnimation(1.0, 0.94, TimeSpan.FromMilliseconds(150.0));
				scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
				scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
			}
		}

		// Token: 0x0600019D RID: 413 RVA: 0x0001197C File Offset: 0x0000FB7C
		private void AddPremiumSettingsButton()
		{
			if (this.mInstallerCloseMinimizePanel == null)
			{
				return;
			}
			Border button = new Border
			{
				Width = 38.0,
				Height = 38.0,
				CornerRadius = new CornerRadius(19.0),
				Background = this.WhiteA(14),
				BorderBrush = this.WhiteA(38),
				BorderThickness = new Thickness(1.0),
				Cursor = System.Windows.Input.Cursors.Hand,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(0.0, 0.0, 8.0, 0.0),
				ToolTip = "Personalize"
			};
			System.Windows.Shapes.Path child = new System.Windows.Shapes.Path
			{
				Data = Geometry.Parse("M2,4 H7 M11,4 H16 M7,4 A2,2 0 1 1 11,4 A2,2 0 1 1 7,4 M2,9 H11 M15,9 H16 M11,9 A2,2 0 1 1 15,9 A2,2 0 1 1 11,9 M2,14 H4 M8,14 H16 M4,14 A2,2 0 1 1 8,14 A2,2 0 1 1 4,14"),
				Stroke = this.MakeGoldBrush(0.0),
				StrokeThickness = 1.3,
				StrokeStartLineCap = PenLineCap.Round,
				StrokeEndLineCap = PenLineCap.Round,
				Width = 16.0,
				Height = 16.0,
				Stretch = Stretch.Uniform,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			button.Child = child;
			button.MouseEnter += delegate(object sender, System.Windows.Input.MouseEventArgs e)
			{
				button.Background = this.GoldA(28);
				button.BorderBrush = this.GoldA(110);
			};
			button.MouseLeave += delegate(object sender, System.Windows.Input.MouseEventArgs e)
			{
				button.Background = this.WhiteA(14);
				button.BorderBrush = this.WhiteA(38);
			};
			button.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				this.ToggleSettingsPanel();
				e.Handled = true;
			};
			this.mInstallerCloseMinimizePanel.Children.Insert(0, button);
		}

		// Token: 0x0600019E RID: 414 RVA: 0x00011B44 File Offset: 0x0000FD44
		private string ReadConfig(string key, string defaultValue)
		{
			try
			{
				string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bluestacks.conf");
				if (!File.Exists(path))
				{
					return defaultValue;
				}
				string text = key + "=";
				string[] array = File.ReadAllLines(path);
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i].TrimStart(Array.Empty<char>());
					if (text2.StartsWith(text, StringComparison.OrdinalIgnoreCase))
					{
						string text3 = text2.Substring(text.Length).Trim();
						if (text3.Length >= 2 && text3[0] == '"' && text3[text3.Length - 1] == '"')
						{
							text3 = text3.Substring(1, text3.Length - 2);
						}
						return text3;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("ReadConfig failed for " + key + ". Ex: " + ex.Message);
			}
			return defaultValue;
		}

		// Token: 0x0600019F RID: 415 RVA: 0x00011C40 File Offset: 0x0000FE40
		private void ChooseWallpaper_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
			{
				Title = "Choose wallpaper",
				Filter = "Images (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
			};
			bool? flag = openFileDialog.ShowDialog();
			bool flag2 = true;
			if (!(flag.GetValueOrDefault() == flag2 & flag != null))
			{
				return;
			}
			try
			{
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.UriSource = new Uri(openFileDialog.FileName, UriKind.Absolute);
				bitmapImage.EndInit();
				bitmapImage.Freeze();
				this.mCustomWallpaperActive = true;
				this.mInstallerBackgroundImage.Source = bitmapImage;
			}
			catch (Exception ex)
			{
				Logger.Warning("Could not set custom wallpaper. Ex: " + ex.Message);
			}
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x00011CF8 File Offset: 0x0000FEF8
		private void ResetWallpaper()
		{
			try
			{
				this.mCustomWallpaperActive = false;
				int num = (this.mBackgroundImages != null) ? this.mBackgroundImages.Length : 0;
				if (num > 1)
				{
					this.MBackgroundChangeTimer_Elapsed(this, null);
				}
				else if (num == 1 && File.Exists(this.mBackgroundImages[0]))
				{
					MicroInstallerSharedUtils.SetAnimationInBackgroudImage(this.mInstallerBackgroundImage, this.mBackgroundImages[0]);
				}
				else
				{
					this.mInstallerBackgroundImage.Source = ImageUtils.BitmapFromPath(System.IO.Path.Combine("Assets", "installer_bg.jpg"));
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Could not reset wallpaper. Ex: " + ex.Message);
			}
		}

		// Token: 0x060001A1 RID: 417 RVA: 0x000028D9 File Offset: 0x00000AD9
		private void EnsureMusicPlayer()
		{
			if (this.mBgMusicPlayer != null)
			{
				return;
			}
			this.mBgMusicPlayer = new MediaPlayer
			{
				Volume = 0.4
			};
			this.mBgMusicPlayer.MediaEnded += delegate(object s, EventArgs e)
			{
				this.mBgMusicPlayer.Position = TimeSpan.Zero;
				this.mBgMusicPlayer.Play();
			};
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x00002915 File Offset: 0x00000B15
		private void PlayMusicFile(string path)
		{
			this.EnsureMusicPlayer();
			this.mBgMusicPlayer.Stop();
			this.mBgMusicPlayer.Open(new Uri(path, UriKind.Absolute));
			this.mBgMusicPlayer.Play();
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x00011DA0 File Offset: 0x0000FFA0
		private void StartBackgroundMusic()
		{
			try
			{
				string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "installer_music.mp3");
				if (File.Exists(path))
				{
					this.PlayMusicFile(path);
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Could not start background music. Ex: " + ex.Message);
			}
		}

		// Token: 0x060001A4 RID: 420 RVA: 0x00011E00 File Offset: 0x00010000
		private void ChooseMusic_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
			{
				Title = "Choose background audio",
				Filter = "Audio (*.mp3;*.wav;*.wma;*.m4a)|*.mp3;*.wav;*.wma;*.m4a"
			};
			bool? flag = openFileDialog.ShowDialog();
			bool flag2 = true;
			if (!(flag.GetValueOrDefault() == flag2 & flag != null))
			{
				return;
			}
			try
			{
				this.PlayMusicFile(openFileDialog.FileName);
			}
			catch (Exception ex)
			{
				Logger.Warning("Could not play chosen audio. Ex: " + ex.Message);
			}
		}

		// Token: 0x060001A5 RID: 421 RVA: 0x00011E80 File Offset: 0x00010080
		private void ResetMusic()
		{
			try
			{
				if (this.mBgMusicPlayer != null)
				{
					this.mBgMusicPlayer.Stop();
					this.mBgMusicPlayer.Close();
					this.mBgMusicPlayer.Volume = 0.1;
				}
				if (this.mVolumeSlider != null)
				{
					this.mVolumeSlider.Value = 0.1;
				}
				if (this.mVolumePercentLabel != null)
				{
					this.mVolumePercentLabel.Text = "10%";
				}
				this.StartBackgroundMusic();
			}
			catch (Exception ex)
			{
				Logger.Warning("Could not reset music. Ex: " + ex.Message);
			}
		}

		// Token: 0x060001A6 RID: 422 RVA: 0x00011F24 File Offset: 0x00010124
		private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (this.mVolumePercentLabel != null)
			{
				this.mVolumePercentLabel.Text = string.Format("{0}%", (int)Math.Round(e.NewValue * 100.0));
			}
			if (this.mBgMusicPlayer != null)
			{
				this.mBgMusicPlayer.Volume = e.NewValue;
			}
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x00011F84 File Offset: 0x00010184
		private Border BuildOptionRow(UIElement icon, Color[] iconGradient, string title, string subtitle, RoutedEventHandler clickHandler)
		{
			Border row = this.BuildPremiumCard(13.0, 18);
			row.Cursor = System.Windows.Input.Cursors.Hand;
			Grid grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			grid.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			grid.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			Border border = new Border
			{
				Width = 38.0,
				Height = 38.0,
				CornerRadius = new CornerRadius(10.0),
				Background = new LinearGradientBrush(new GradientStopCollection
				{
					new GradientStop(iconGradient[0], 0.0),
					new GradientStop(iconGradient[1], 1.0)
				}, new Point(0.0, 0.0), new Point(1.0, 1.0))
			};
			FrameworkElement frameworkElement = icon as FrameworkElement;
			if (frameworkElement != null)
			{
				frameworkElement.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
				frameworkElement.VerticalAlignment = VerticalAlignment.Center;
			}
			border.Child = icon;
			Grid.SetColumn(border, 0);
			grid.Children.Add(border);
			StackPanel stackPanel = new StackPanel
			{
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(14.0, 0.0, 10.0, 0.0)
			};
			stackPanel.Children.Add(new TextBlock
			{
				Text = title,
				FontSize = 13.0,
				FontWeight = FontWeights.Medium,
				Foreground = new SolidColorBrush(Color.FromArgb(230, byte.MaxValue, byte.MaxValue, byte.MaxValue))
			});
			stackPanel.Children.Add(new TextBlock
			{
				Text = subtitle,
				FontSize = 11.0,
				Margin = new Thickness(0.0, 3.0, 0.0, 0.0),
				Foreground = new SolidColorBrush(Color.FromArgb(120, byte.MaxValue, byte.MaxValue, byte.MaxValue))
			});
			Grid.SetColumn(stackPanel, 1);
			grid.Children.Add(stackPanel);
			System.Windows.Shapes.Path element = new System.Windows.Shapes.Path
			{
				Data = Geometry.Parse("M0,0 L5,5 L0,10"),
				Stroke = new SolidColorBrush(Color.FromArgb(170, 224, 189, 118)),
				StrokeThickness = 1.6,
				StrokeStartLineCap = PenLineCap.Round,
				StrokeEndLineCap = PenLineCap.Round,
				StrokeLineJoin = PenLineJoin.Round,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(0.0, 0.0, 4.0, 0.0)
			};
			Grid.SetColumn(element, 2);
			grid.Children.Add(element);
			row.Child = grid;
			row.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				e.Handled = true;
				if (clickHandler != null)
				{
					clickHandler(row, new RoutedEventArgs());
				}
			};
			return row;
		}

		// Token: 0x060001A8 RID: 424 RVA: 0x000122FC File Offset: 0x000104FC
		private UIElement BuildWallpaperIcon()
		{
			Canvas canvas = new Canvas();
			canvas.Width = 20.0;
			canvas.Height = 20.0;
			canvas.Children.Add(new System.Windows.Shapes.Path
			{
				Data = Geometry.Parse("M2.5,4 H17.5 V16 H2.5 Z"),
				Stroke = Brushes.White,
				StrokeThickness = 1.4,
				StrokeLineJoin = PenLineJoin.Round
			});
			canvas.Children.Add(new System.Windows.Shapes.Path
			{
				Data = Geometry.Parse("M2.5,14 L7.5,9 L11.5,13 L14,10.5 L17.5,14"),
				Stroke = Brushes.White,
				StrokeThickness = 1.4,
				StrokeStartLineCap = PenLineCap.Round,
				StrokeEndLineCap = PenLineCap.Round,
				StrokeLineJoin = PenLineJoin.Round
			});
			Ellipse element = new Ellipse
			{
				Width = 3.0,
				Height = 3.0,
				Fill = Brushes.White
			};
			Canvas.SetLeft(element, 12.5);
			Canvas.SetTop(element, 5.8);
			canvas.Children.Add(element);
			return canvas;
		}

		// Token: 0x060001A9 RID: 425 RVA: 0x00012418 File Offset: 0x00010618
		private UIElement BuildMusicIcon()
		{
			Canvas canvas = new Canvas();
			canvas.Width = 20.0;
			canvas.Height = 20.0;
			canvas.Children.Add(new System.Windows.Shapes.Path
			{
				Data = Geometry.Parse("M6.5,15 L6.5,4.5 L15,3 L15,13"),
				Stroke = Brushes.White,
				StrokeThickness = 1.4,
				StrokeStartLineCap = PenLineCap.Round,
				StrokeEndLineCap = PenLineCap.Round,
				StrokeLineJoin = PenLineJoin.Round
			});
			Ellipse element = new Ellipse
			{
				Width = 4.6,
				Height = 3.6,
				Fill = Brushes.White
			};
			Canvas.SetLeft(element, 2.4);
			Canvas.SetTop(element, 13.4);
			canvas.Children.Add(element);
			Ellipse element2 = new Ellipse
			{
				Width = 4.6,
				Height = 3.6,
				Fill = Brushes.White
			};
			Canvas.SetLeft(element2, 10.9);
			Canvas.SetTop(element2, 11.4);
			canvas.Children.Add(element2);
			return canvas;
		}

		// Token: 0x060001AA RID: 426 RVA: 0x0001254C File Offset: 0x0001074C
		private LinearGradientBrush MakeGoldBrush(double angleDeg = 45.0)
		{
			double num = angleDeg * 3.141592653589793 / 180.0;
			Point startPoint = new Point(0.5 - Math.Cos(num) * 0.5, 0.5 - Math.Sin(num) * 0.5);
			Point endPoint = new Point(0.5 + Math.Cos(num) * 0.5, 0.5 + Math.Sin(num) * 0.5);
			return new LinearGradientBrush
			{
				StartPoint = startPoint,
				EndPoint = endPoint,
				GradientStops = 
				{
					new GradientStop(this.AccentGoldPale, 0.0),
					new GradientStop(this.AccentGold, 0.5),
					new GradientStop(this.AccentGoldDeep, 1.0)
				}
			};
		}

		// Token: 0x060001AB RID: 427 RVA: 0x00002945 File Offset: 0x00000B45
		private LinearGradientBrush MakeLinear(Color startColor, Color endColor, Point start, Point end)
		{
			return new LinearGradientBrush(new GradientStopCollection
			{
				new GradientStop(startColor, 0.0),
				new GradientStop(endColor, 1.0)
			}, start, end);
		}

		// Token: 0x060001AC RID: 428 RVA: 0x00012658 File Offset: 0x00010858
		private void CreateSettingsPopup()
		{
			Border border = base.Content as Border;
			Grid grid = (border != null) ? (border.Child as Grid) : null;
			if (grid == null)
			{
				grid = (base.Content as Grid);
			}
			if (grid == null)
			{
				return;
			}
			this.InitPanelFonts();
			string initialText = this.ReadConfig("bst.instance.Pie64.dpi", "240");
			string initialText2 = this.ReadConfig("bst.instance.Pie64.max_fps", "60");
			string mb = this.ReadConfig("bst.instance.Pie64.ram", "4096");
			string initialText3 = this.ReadConfig("bst.instance.Pie64.display_name", "MSI App Player");
			string str = this.ReadConfig("bst.instance.Pie64.fb_width", "1600");
			string str2 = this.ReadConfig("bst.instance.Pie64.fb_height", "900");
			string text = str + " × " + str2;
			this.mSettingsOverlay = new Grid
			{
				Background = new SolidColorBrush(Color.FromArgb(205, 6, 5, 3)),
				Visibility = Visibility.Collapsed,
				Opacity = 0.0
			};
			System.Windows.Controls.Panel.SetZIndex(this.mSettingsOverlay, 500);
			if (grid.RowDefinitions.Count > 0)
			{
				Grid.SetRowSpan(this.mSettingsOverlay, grid.RowDefinitions.Count);
			}
			if (grid.ColumnDefinitions.Count > 0)
			{
				Grid.SetColumnSpan(this.mSettingsOverlay, grid.ColumnDefinitions.Count);
			}
			this.mSettingsOverlay.MouseLeftButtonUp += delegate(object sender, MouseButtonEventArgs e)
			{
				this.CloseSettingsPanel();
			};
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush
			{
				StartPoint = new Point(0.0, 0.0),
				EndPoint = new Point(0.25, 1.0)
			};
			linearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(26, 23, 17), 0.0));
			linearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(12, 11, 8), 0.6));
			linearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(6, 6, 4), 1.0));
			LinearGradientBrush linearGradientBrush2 = new LinearGradientBrush
			{
				StartPoint = new Point(0.0, 0.0),
				EndPoint = new Point(0.0, 1.0)
			};
			linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(190, 250, 232, 190), 0.0));
			linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(90, 224, 189, 118), 0.45));
			linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(45, 168, 130, 68), 1.0));
			this.mSettingsPanelBorder = new Border
			{
				MaxWidth = 800.0,
				MaxHeight = 500.0,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Margin = new Thickness(16.0),
				CornerRadius = new CornerRadius(16.0),
				BorderThickness = new Thickness(1.0),
				Background = linearGradientBrush,
				BorderBrush = linearGradientBrush2,
				RenderTransformOrigin = new Point(0.5, 0.5),
				RenderTransform = new ScaleTransform(0.97, 0.97),
				ClipToBounds = true
			};
			TextElement.SetFontFamily(this.mSettingsPanelBorder, this.mFontPoppins);
			this.mSettingsPanelBorder.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				e.Handled = true;
			};
			Grid grid2 = new Grid();
			grid2.RowDefinitions.Add(new RowDefinition
			{
				Height = GridLength.Auto
			});
			grid2.RowDefinitions.Add(new RowDefinition
			{
				Height = new GridLength(1.0)
			});
			grid2.RowDefinitions.Add(new RowDefinition
			{
				Height = new GridLength(1.0, GridUnitType.Star)
			});
			grid2.RowDefinitions.Add(new RowDefinition
			{
				Height = GridLength.Auto
			});
			Grid grid3 = new Grid
			{
				Margin = new Thickness(24.0, 16.0, 18.0, 12.0)
			};
			grid3.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			grid3.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			StackPanel stackPanel = new StackPanel
			{
				Orientation = System.Windows.Controls.Orientation.Vertical,
				VerticalAlignment = VerticalAlignment.Center
			};
			stackPanel.Children.Add(new TextBlock
			{
				Text = "★  PREFERENCES",
				FontSize = 9.0,
				FontWeight = FontWeights.Bold,
				Foreground = this.MakeGoldBrush(0.0),
				Margin = new Thickness(0.0, 0.0, 0.0, 3.0)
			});
			stackPanel.Children.Add(new TextBlock
			{
				Text = "Installer Settings",
				FontSize = 18.0,
				FontWeight = FontWeights.Bold,
				Foreground = Brushes.White
			});
			Grid.SetColumn(stackPanel, 0);
			grid3.Children.Add(stackPanel);
			Border closeButton = new Border
			{
				Width = 30.0,
				Height = 30.0,
				CornerRadius = new CornerRadius(15.0),
				Background = this.WhiteA(20),
				BorderBrush = this.WhiteA(38),
				BorderThickness = new Thickness(1.0),
				HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center,
				Cursor = System.Windows.Input.Cursors.Hand
			};
			System.Windows.Shapes.Path closeIcon = new System.Windows.Shapes.Path
			{
				Data = Geometry.Parse("M 0,0 L 9,9 M 9,0 L 0,9"),
				Stroke = this.WhiteA(200),
				StrokeThickness = 1.8,
				StrokeStartLineCap = PenLineCap.Round,
				StrokeEndLineCap = PenLineCap.Round,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			closeButton.Child = closeIcon;
			closeButton.MouseEnter += delegate(object sender, System.Windows.Input.MouseEventArgs e)
			{
				closeButton.Background = new SolidColorBrush(Color.FromArgb(120, 235, 64, 84));
				closeButton.BorderBrush = new SolidColorBrush(Color.FromArgb(160, byte.MaxValue, 110, 128));
				closeIcon.Stroke = Brushes.White;
			};
			closeButton.MouseLeave += delegate(object sender, System.Windows.Input.MouseEventArgs e)
			{
				closeButton.Background = this.WhiteA(20);
				closeButton.BorderBrush = this.WhiteA(38);
				closeIcon.Stroke = this.WhiteA(200);
			};
			closeButton.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				this.CloseSettingsPanel();
				e.Handled = true;
			};
			Grid.SetColumn(closeButton, 1);
			grid3.Children.Add(closeButton);
			Grid.SetRow(grid3, 0);
			grid2.Children.Add(grid3);
			Border element = new Border
			{
				Height = 1.0,
				Background = this.MakeFadeLine(true, 150)
			};
			Grid.SetRow(element, 1);
			grid2.Children.Add(element);
			StackPanel stackPanel2 = new StackPanel
			{
				Orientation = System.Windows.Controls.Orientation.Vertical,
				Margin = new Thickness(0.0, 0.0, 12.0, 4.0)
			};
			ScrollViewer scrollViewer = new ScrollViewer
			{
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
				CanContentScroll = false,
				Focusable = false,
				Margin = new Thickness(24.0, 12.0, 12.0, 4.0),
				Content = stackPanel2
			};
			scrollViewer.Resources.Add(typeof(System.Windows.Controls.Primitives.ScrollBar), this.BuildSlimScrollBarStyle());
			Grid.SetRow(scrollViewer, 2);
			grid2.Children.Add(scrollViewer);
			StackPanel stackPanel3 = this.BuildSection("DISPLAY & PERFORMANCE");
			Border border2 = this.BuildPremiumCard(13.0, 18);
			StackPanel stackPanel4 = new StackPanel();
			stackPanel4.Children.Add(new TextBlock
			{
				Text = "Display / Resolution",
				FontSize = 12.5,
				FontWeight = FontWeights.Medium,
				Foreground = this.WhiteA(230)
			});
			stackPanel4.Children.Add(new TextBlock
			{
				Text = "Configure emulator resolution and DPI",
				FontSize = 10.5,
				Margin = new Thickness(0.0, 2.0, 0.0, 9.0),
				Foreground = this.WhiteA(120)
			});
			Grid grid4 = new Grid();
			grid4.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			grid4.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(12.0)
			});
			grid4.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(104.0)
			});
			StackPanel stackPanel5 = new StackPanel();
			stackPanel5.Children.Add(this.BuildFieldLabel("RESOLUTION"));
			System.Windows.Controls.ComboBox resolutionCombo = this.BuildPremiumComboBox();
			resolutionCombo.Items.Add("1280 × 720");
			resolutionCombo.Items.Add("1366 × 768");
			resolutionCombo.Items.Add("1600 × 900");
			resolutionCombo.Items.Add("1920 × 1080");
			resolutionCombo.SelectedItem = text;
			if (resolutionCombo.SelectedItem == null)
			{
				resolutionCombo.Items.Add(text);
				resolutionCombo.SelectedItem = text;
			}
			stackPanel5.Children.Add(resolutionCombo);
			Grid.SetColumn(stackPanel5, 0);
			grid4.Children.Add(stackPanel5);
			StackPanel stackPanel6 = new StackPanel();
			stackPanel6.Children.Add(this.BuildFieldLabel("DPI"));
			Border element2;
			System.Windows.Controls.TextBox dpiField = this.BuildPremiumTextBox(initialText, "DPI", out element2);
			stackPanel6.Children.Add(element2);
			Grid.SetColumn(stackPanel6, 2);
			grid4.Children.Add(stackPanel6);
			stackPanel4.Children.Add(grid4);
			border2.Child = stackPanel4;
			stackPanel3.Children.Add(border2);
			Border border3 = this.BuildPremiumCard(13.0, 18);
			StackPanel stackPanel7 = new StackPanel();
			Grid grid5 = new Grid();
			grid5.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			grid5.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			StackPanel stackPanel8 = new StackPanel
			{
				VerticalAlignment = VerticalAlignment.Center
			};
			stackPanel8.Children.Add(new TextBlock
			{
				Text = "Memory (RAM)",
				FontSize = 12.5,
				FontWeight = FontWeights.Medium,
				Foreground = this.WhiteA(230)
			});
			stackPanel8.Children.Add(new TextBlock
			{
				Text = "Pick a preset or enter your own amount",
				FontSize = 10.5,
				Margin = new Thickness(0.0, 2.0, 0.0, 0.0),
				Foreground = this.WhiteA(120)
			});
			Grid.SetColumn(stackPanel8, 0);
			grid5.Children.Add(stackPanel8);
			System.Windows.Controls.ComboBox ramCombo = this.BuildPremiumComboBox();
			ramCombo.Width = 112.0;
			ramCombo.VerticalAlignment = VerticalAlignment.Center;
			ramCombo.Items.Add("2 GB");
			ramCombo.Items.Add("4 GB");
			ramCombo.Items.Add("8 GB");
			ramCombo.Items.Add("12 GB");
			ramCombo.Items.Add("Custom");
			Grid.SetColumn(ramCombo, 1);
			grid5.Children.Add(ramCombo);
			stackPanel7.Children.Add(grid5);
			Border element3;
			System.Windows.Controls.TextBox ramCustomBox = this.BuildPremiumTextBox("", "Custom RAM amount", "e.g. 6 GB  or  6144 MB", out element3);
			StackPanel ramCustomPanel = new StackPanel
			{
				Margin = new Thickness(0.0, 12.0, 0.0, 0.0),
				Visibility = Visibility.Collapsed
			};
			ramCustomPanel.Children.Add(this.BuildFieldLabel("CUSTOM AMOUNT"));
			ramCustomPanel.Children.Add(element3);
			long totalPhysicalMemoryMb = this.GetTotalPhysicalMemoryMb();
			ramCustomPanel.Children.Add(new TextBlock
			{
				Text = "Enter GB or MB (minimum 1 GB)" + ((totalPhysicalMemoryMb > 0L) ? (". This PC has " + this.RamMbToLabel(totalPhysicalMemoryMb.ToString()) + ".") : "."),
				FontSize = 10.0,
				Margin = new Thickness(1.0, 6.0, 0.0, 0.0),
				TextWrapping = TextWrapping.Wrap,
				Foreground = this.WhiteA(100)
			});
			stackPanel7.Children.Add(ramCustomPanel);
			ramCombo.SelectionChanged += delegate(object sender, SelectionChangedEventArgs e)
			{
				bool flag = ramCombo.SelectedItem != null && ramCombo.SelectedItem.ToString() == "Custom";
				ramCustomPanel.Visibility = (flag ? Visibility.Visible : Visibility.Collapsed);
			};
			string text2 = this.RamMbToLabel(mb);
			ramCombo.SelectedItem = text2;
			if (ramCombo.SelectedItem == null)
			{
				ramCustomBox.Text = text2;
				ramCombo.SelectedItem = "Custom";
			}
			border3.Child = stackPanel7;
			stackPanel3.Children.Add(border3);
			Border border4 = this.BuildPremiumCard(13.0, 18);
			Grid grid6 = new Grid();
			grid6.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			grid6.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			StackPanel stackPanel9 = new StackPanel
			{
				VerticalAlignment = VerticalAlignment.Center
			};
			stackPanel9.Children.Add(new TextBlock
			{
				Text = "FPS Limit",
				FontSize = 12.5,
				FontWeight = FontWeights.Medium,
				Foreground = this.WhiteA(230)
			});
			stackPanel9.Children.Add(new TextBlock
			{
				Text = "Set the maximum emulator frame rate",
				FontSize = 10.5,
				Margin = new Thickness(0.0, 2.0, 0.0, 0.0),
				Foreground = this.WhiteA(120)
			});
			Grid.SetColumn(stackPanel9, 0);
			grid6.Children.Add(stackPanel9);
			Border border5;
			System.Windows.Controls.TextBox fpsField = this.BuildPremiumTextBox(initialText2, "FPS", out border5);
			border5.Width = 104.0;
			border5.VerticalAlignment = VerticalAlignment.Center;
			Grid.SetColumn(border5, 1);
			grid6.Children.Add(border5);
			border4.Child = grid6;
			stackPanel3.Children.Add(border4);
			stackPanel2.Children.Add(stackPanel3);
			StackPanel stackPanel10 = this.BuildSection("AUDIO");
			stackPanel10.Children.Add(this.BuildOptionRow(this.BuildMusicIcon(), new Color[]
			{
				Color.FromRgb(231, 201, 150),
				Color.FromRgb(168, 128, 78)
			}, "Background Audio", "Choose a custom soundtrack file", new RoutedEventHandler(this.ChooseMusic_Click)));
			Border border6 = this.BuildPremiumCard(13.0, 18);
			StackPanel stackPanel11 = new StackPanel();
			Grid grid7 = new Grid();
			grid7.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			grid7.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			grid7.Children.Add(new TextBlock
			{
				Text = "Music Volume Level",
				FontSize = 12.5,
				FontWeight = FontWeights.Medium,
				Foreground = this.WhiteA(230),
				VerticalAlignment = VerticalAlignment.Center
			});
			double num = (this.mBgMusicPlayer != null) ? this.mBgMusicPlayer.Volume : 0.5;
			Border border7 = new Border
			{
				Background = this.GoldA(34),
				BorderBrush = this.GoldA(80),
				BorderThickness = new Thickness(1.0),
				CornerRadius = new CornerRadius(9.0),
				Padding = new Thickness(10.0, 2.0, 10.0, 2.0)
			};
			this.mVolumePercentLabel = new TextBlock
			{
				Text = string.Format("{0}%", (int)Math.Round(num * 100.0)),
				FontSize = 11.5,
				FontWeight = FontWeights.Bold,
				Foreground = this.MakeGoldBrush(0.0)
			};
			border7.Child = this.mVolumePercentLabel;
			Grid.SetColumn(border7, 1);
			grid7.Children.Add(border7);
			stackPanel11.Children.Add(grid7);
			this.mVolumeSlider = this.BuildPremiumSlider(num);
			this.mVolumeSlider.ValueChanged += this.VolumeSlider_ValueChanged;
			stackPanel11.Children.Add(this.mVolumeSlider);
			border6.Child = stackPanel11;
			stackPanel10.Children.Add(border6);
			Border border8 = this.BuildGhostButton("Reset Music", Color.FromRgb(byte.MaxValue, 110, 130), Color.FromRgb(byte.MaxValue, 130, 150));
			border8.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			border8.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				this.ResetMusic();
				e.Handled = true;
			};
			stackPanel10.Children.Add(border8);
			stackPanel2.Children.Add(stackPanel10);
			StackPanel stackPanel12 = this.BuildSection("INTEGRATIONS");
			Border border9 = this.BuildPremiumCard(13.0, 18);
			Grid grid8 = new Grid();
			grid8.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			grid8.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			StackPanel stackPanel13 = new StackPanel
			{
				VerticalAlignment = VerticalAlignment.Center
			};
			stackPanel13.Children.Add(new TextBlock
			{
				Text = "Discord Rich Presence",
				FontSize = 12.5,
				FontWeight = FontWeights.Medium,
				Foreground = this.WhiteA(230)
			});
			stackPanel13.Children.Add(new TextBlock
			{
				Text = "Show Discord activity status",
				FontSize = 10.5,
				Margin = new Thickness(0.0, 2.0, 0.0, 0.0),
				Foreground = this.WhiteA(120)
			});
			Grid.SetColumn(stackPanel13, 0);
			grid8.Children.Add(stackPanel13);
			bool discordRpcEnabled = true;
			Ellipse rpcKnob;
			Border rpcToggle = this.BuildPremiumToggle(discordRpcEnabled, out rpcKnob);
			try
			{
				Discord.Initialize("1551531261330194523");
				Discord.SetPresence("Installing", "Dev By Floatz", "https://i.pinimg.com/originals/c3/16/27/c3162775635d404d6462187ea4b5941f.gif", "Floatz", "YouTube", "https://youtube.com/@FloatzJod", "Discord", "https://dsc.gg/floatz4ever");
			}
			catch
			{
			}
			rpcToggle.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				discordRpcEnabled = !discordRpcEnabled;
				this.AnimatePremiumToggle(rpcToggle, rpcKnob, discordRpcEnabled);
				if (discordRpcEnabled)
				{
					try
					{
						Discord.Initialize("1540384184336523284");
						Discord.SetPresence("Infinity X", "Installing", "infinity-loop", "Float", "YouTube", "https://youtube.com/@FloatzJod", "Discord", "https://dsc.gg/floatz4ever");
						goto IL_7D;
					}
					catch
					{
						goto IL_7D;
					}
				}
				try
				{
					Discord.ClearPresence();
				}
				catch
				{
				}
				IL_7D:
				e.Handled = true;
			};
			Grid.SetColumn(rpcToggle, 1);
			grid8.Children.Add(rpcToggle);
			border9.Child = grid8;
			stackPanel12.Children.Add(border9);
			stackPanel2.Children.Add(stackPanel12);
			StackPanel stackPanel14 = this.BuildSection("SYSTEM");
			Border border10 = this.BuildPremiumCard(13.0, 18);
			StackPanel stackPanel15 = new StackPanel();
			stackPanel15.Children.Add(new TextBlock
			{
				Text = "Emulator Name",
				FontSize = 12.5,
				FontWeight = FontWeights.Medium,
				Foreground = this.WhiteA(230)
			});
			stackPanel15.Children.Add(new TextBlock
			{
				Text = "Shown as the display name for this instance",
				FontSize = 10.5,
				Margin = new Thickness(0.0, 2.0, 0.0, 9.0),
				Foreground = this.WhiteA(120)
			});
			Border element4;
			System.Windows.Controls.TextBox displayNameField = this.BuildPremiumTextBox(initialText3, "Emulator display name", out element4);
			stackPanel15.Children.Add(element4);
			border10.Child = stackPanel15;
			stackPanel14.Children.Add(border10);
			stackPanel2.Children.Add(stackPanel14);
			Border border11 = new Border
			{
				Margin = new Thickness(22.0, 6.0, 22.0, 16.0),
				Background = this.MakeLinear(Color.FromArgb(26, byte.MaxValue, byte.MaxValue, byte.MaxValue), Color.FromArgb(8, byte.MaxValue, byte.MaxValue, byte.MaxValue), new Point(0.0, 0.0), new Point(0.0, 1.0)),
				BorderBrush = this.GoldA(40),
				BorderThickness = new Thickness(1.0),
				CornerRadius = new CornerRadius(12.0),
				Padding = new Thickness(14.0, 8.0, 10.0, 8.0)
			};
			Grid grid9 = new Grid();
			grid9.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			grid9.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			TextBlock applyStatus = new TextBlock
			{
				FontSize = 10.5,
				FontWeight = FontWeights.SemiBold,
				VerticalAlignment = VerticalAlignment.Center,
				Foreground = this.WhiteA(130),
				TextWrapping = TextWrapping.Wrap,
				Text = "Display and system changes are saved only when you press Apply."
			};
			Grid.SetColumn(applyStatus, 0);
			grid9.Children.Add(applyStatus);
			Border border12 = this.BuildPremiumButton("APPLY CHANGES");
			border12.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			border12.Margin = new Thickness(14.0, 0.0, 0.0, 0.0);
			border12.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				e.Handled = true;
				bool flag = true;
				try
				{
					string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bluestacks.conf");
					if (!File.Exists(path))
					{
						flag = false;
					}
					else if (resolutionCombo.SelectedItem == null)
					{
						flag = false;
					}
					else
					{
						string[] array = resolutionCombo.SelectedItem.ToString().Split(new char[]
						{
							'×'
						}, StringSplitOptions.RemoveEmptyEntries);
						if (array.Length != 2)
						{
							flag = false;
						}
						else
						{
							string str3 = array[0].Trim();
							string str4 = array[1].Trim();
							string str5 = dpiField.Text.Trim();
							string str6 = fpsField.Text.Trim();
							string str7 = displayNameField.Text.Trim();
							string text3;
							if (ramCombo.SelectedItem != null && ramCombo.SelectedItem.ToString() == "Custom")
							{
								text3 = this.ParseRamInput(ramCustomBox.Text);
							}
							else
							{
								text3 = ((ramCombo.SelectedItem != null) ? this.RamLabelToMb(ramCombo.SelectedItem.ToString()) : null);
							}
							long num2 = 0L;
							long totalPhysicalMemoryMb2 = this.GetTotalPhysicalMemoryMb();
							if (text3 == null || !long.TryParse(text3, out num2))
							{
								flag = false;
							}
							else if (totalPhysicalMemoryMb2 > 0L && num2 > totalPhysicalMemoryMb2)
							{
								flag = false;
							}
							if (flag)
							{
								bool flag2 = false;
								string[] array2 = File.ReadAllLines(path);
								for (int i = 0; i < array2.Length; i++)
								{
									string text4 = array2[i].TrimStart(new char[0]);
									if (text4.StartsWith("bst.instance.Pie64.fb_width=", StringComparison.OrdinalIgnoreCase))
									{
										array2[i] = "bst.instance.Pie64.fb_width=\"" + str3 + "\"";
									}
									if (text4.StartsWith("bst.instance.Pie64.fb_height=", StringComparison.OrdinalIgnoreCase))
									{
										array2[i] = "bst.instance.Pie64.fb_height=\"" + str4 + "\"";
									}
									if (text4.StartsWith("bst.instance.Pie64.dpi=", StringComparison.OrdinalIgnoreCase))
									{
										array2[i] = "bst.instance.Pie64.dpi=\"" + str5 + "\"";
									}
									if (text4.StartsWith("bst.instance.Pie64.max_fps=", StringComparison.OrdinalIgnoreCase))
									{
										array2[i] = "bst.instance.Pie64.max_fps=\"" + str6 + "\"";
									}
									if (text4.StartsWith("bst.instance.Pie64.display_name=", StringComparison.OrdinalIgnoreCase))
									{
										array2[i] = "bst.instance.Pie64.display_name=\"" + str7 + "\"";
									}
									if (text3 != null && text4.StartsWith("bst.instance.Pie64.ram=", StringComparison.OrdinalIgnoreCase))
									{
										array2[i] = "bst.instance.Pie64.ram=\"" + text3 + "\"";
										flag2 = true;
									}
								}
								if (text3 != null && !flag2)
								{
									array2 = new List<string>(array2)
									{
										"bst.instance.Pie64.ram=\"" + text3 + "\""
									}.ToArray();
								}
								File.WriteAllLines(path, array2);
							}
						}
					}
				}
				catch
				{
					flag = false;
				}
				applyStatus.Foreground = null;
				if (flag)
				{
					applyStatus.Foreground = this.MakeGoldBrush(0.0);
				}
				else
				{
					applyStatus.Foreground = new SolidColorBrush(Color.FromRgb(248, 113, 113));
				}
				DispatcherTimer clearStatus = new DispatcherTimer
				{
					Interval = TimeSpan.FromMilliseconds(2400.0)
				};
				clearStatus.Tick += delegate(object sender, EventArgs fe)
				{
					clearStatus.Stop();
					applyStatus.Text = "Display and system changes are saved only when you press Apply.";
					applyStatus.Foreground = this.WhiteA(130);
				};
				clearStatus.Start();
			};
			Grid.SetColumn(border12, 1);
			grid9.Children.Add(border12);
			border11.Child = grid9;
			Grid.SetRow(border11, 3);
			grid2.Children.Add(border11);
			this.mSettingsPanelBorder.Child = grid2;
			this.ApplyPanelFonts(this.mSettingsPanelBorder);
			Grid grid10 = new Grid();
			grid10.Children.Add(this.mSettingsPanelBorder);
			this.mSettingsOverlay.Children.Add(grid10);
			grid.Children.Add(this.mSettingsOverlay);
		}

		// Token: 0x060001AD RID: 429 RVA: 0x00013F88 File Offset: 0x00012188
		private Border BuildPremiumCard(double cornerRadius = 13.0, byte alpha = 18)
		{
			LinearGradientBrush background = new LinearGradientBrush(new GradientStopCollection
			{
				new GradientStop(Color.FromArgb(alpha + 10, byte.MaxValue, byte.MaxValue, byte.MaxValue), 0.0),
				new GradientStop(Color.FromArgb(alpha, byte.MaxValue, byte.MaxValue, byte.MaxValue), 1.0)
			}, new Point(0.0, 0.0), new Point(0.0, 1.0));
			Border card = new Border
			{
				Background = background,
				BorderBrush = this.GoldA(34),
				BorderThickness = new Thickness(1.0),
				CornerRadius = new CornerRadius(cornerRadius),
				Padding = new Thickness(14.0, 11.0, 14.0, 11.0),
				Margin = new Thickness(0.0, 0.0, 0.0, 9.0)
			};
			card.MouseEnter += delegate(object <p0>, System.Windows.Input.MouseEventArgs <p1>)
			{
				card.BorderBrush = this.GoldA(78);
			};
			card.MouseLeave += delegate(object <p0>, System.Windows.Input.MouseEventArgs <p1>)
			{
				card.BorderBrush = this.GoldA(34);
			};
			return card;
		}

		// Token: 0x060001AE RID: 430 RVA: 0x00014104 File Offset: 0x00012304
		private TextBlock BuildFieldLabel(string text)
		{
			return new TextBlock
			{
				Text = text,
				FontSize = 9.0,
				FontWeight = FontWeights.SemiBold,
				Margin = new Thickness(1.0, 0.0, 0.0, 5.0),
				Foreground = this.WhiteA(115)
			};
		}

		// Token: 0x060001AF RID: 431 RVA: 0x00014174 File Offset: 0x00012374
		private System.Windows.Controls.ComboBox BuildPremiumComboBox()
		{
			System.Windows.Controls.ComboBox combo = new System.Windows.Controls.ComboBox
			{
				HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
				Height = 34.0,
				FontSize = 12.0,
				Foreground = this.WhiteA(235),
				Cursor = System.Windows.Input.Cursors.Hand,
				IsReadOnly = true
			};
			string xamlText = "<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'\r\n\t\t\t   xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='ComboBoxItem'>\r\n\t\t\t<Setter Property='Foreground' Value='#D8FFFFFF' />\r\n\t\t\t<Setter Property='FontSize' Value='12' />\r\n\t\t\t<Setter Property='Cursor' Value='Hand' />\r\n\t\t\t<Setter Property='SnapsToDevicePixels' Value='True' />\r\n\t\t\t<Setter Property='Template'>\r\n\t\t\t\t<Setter.Value>\r\n\t\t\t\t\t<ControlTemplate TargetType='ComboBoxItem'>\r\n\t\t\t\t\t\t<Border x:Name='Row' CornerRadius='8' Padding='11,8,11,8' Margin='0,1,0,1' Background='Transparent'>\r\n\t\t\t\t\t\t\t<Grid>\r\n\t\t\t\t\t\t\t\t<Grid.ColumnDefinitions>\r\n\t\t\t\t\t\t\t\t\t<ColumnDefinition Width='*' /><ColumnDefinition Width='Auto' />\r\n\t\t\t\t\t\t\t\t</Grid.ColumnDefinitions>\r\n\t\t\t\t\t\t\t\t<ContentPresenter VerticalAlignment='Center' TextElement.Foreground='{TemplateBinding Foreground}' />\r\n\t\t\t\t\t\t\t\t<Path x:Name='Check' Grid.Column='1' Data='M0,4.5 L3.6,8 L10,0.5' Stroke='#E0BD76' StrokeThickness='1.8'\r\n\t\t\t\t\t\t\t\t\t  StrokeStartLineCap='Round' StrokeEndLineCap='Round' StrokeLineJoin='Round'\r\n\t\t\t\t\t\t\t\t\t  VerticalAlignment='Center' Margin='14,0,0,0' Visibility='Collapsed' />\r\n\t\t\t\t\t\t\t</Grid>\r\n\t\t\t\t\t\t</Border>\r\n\t\t\t\t\t\t<ControlTemplate.Triggers>\r\n\t\t\t\t\t\t\t<Trigger Property='IsMouseOver' Value='True'>\r\n\t\t\t\t\t\t\t\t<Setter TargetName='Row' Property='Background' Value='#22E0BD76' />\r\n\t\t\t\t\t\t\t</Trigger>\r\n\t\t\t\t\t\t\t<Trigger Property='IsHighlighted' Value='True'>\r\n\t\t\t\t\t\t\t\t<Setter TargetName='Row' Property='Background' Value='#22E0BD76' />\r\n\t\t\t\t\t\t\t</Trigger>\r\n\t\t\t\t\t\t\t<Trigger Property='IsSelected' Value='True'>\r\n\t\t\t\t\t\t\t\t<Setter TargetName='Row' Property='Background' Value='#1AE0BD76' />\r\n\t\t\t\t\t\t\t\t<Setter TargetName='Check' Property='Visibility' Value='Visible' />\r\n\t\t\t\t\t\t\t\t<Setter Property='Foreground' Value='#FFFAE8BE' />\r\n\t\t\t\t\t\t\t</Trigger>\r\n\t\t\t\t\t\t</ControlTemplate.Triggers>\r\n\t\t\t\t\t</ControlTemplate>\r\n\t\t\t\t</Setter.Value>\r\n\t\t\t</Setter>\r\n\t\t</Style>";
			combo.ItemContainerStyle = (Style)XamlReader.Parse(xamlText);
			combo.Resources.Add(typeof(System.Windows.Controls.Primitives.ScrollBar), this.BuildSlimScrollBarStyle());
			string xamlText2 = "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'\r\n\t\t                 xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='ComboBox'>\r\n\t\t    <Grid>\r\n\t\t        <Border x:Name='MainBorder' CornerRadius='8' BorderThickness='1' BorderBrush='#1AFFFFFF' Background='#0DFFFFFF'>\r\n\t\t            <Grid>\r\n\t\t                <ToggleButton x:Name='ToggleButton' Background='Transparent' BorderThickness='0'\r\n\t\t                              IsChecked='{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}'\r\n\t\t                              Focusable='False' ClickMode='Press'>\r\n\t\t                    <ToggleButton.Template>\r\n\t\t                        <ControlTemplate TargetType='ToggleButton'>\r\n\t\t                            <Grid Background='Transparent'>\r\n\t\t                                <Grid.ColumnDefinitions>\r\n\t\t                                    <ColumnDefinition Width='*' /><ColumnDefinition Width='30' />\r\n\t\t                                </Grid.ColumnDefinitions>\r\n\t\t                                <Path x:Name='Arrow' Grid.Column='1' Data='M0,0 L4.5,4.5 L9,0' Stroke='#E0BD76' StrokeThickness='1.6'\r\n\t\t                                      StrokeStartLineCap='Round' StrokeEndLineCap='Round' StrokeLineJoin='Round'\r\n\t\t                                      HorizontalAlignment='Center' VerticalAlignment='Center' RenderTransformOrigin='0.5,0.5'>\r\n\t\t                                    <Path.RenderTransform><RotateTransform Angle='0' /></Path.RenderTransform>\r\n\t\t                                </Path>\r\n\t\t                            </Grid>\r\n\t\t                            <ControlTemplate.Triggers>\r\n\t\t                                <Trigger Property='IsChecked' Value='True'>\r\n\t\t                                    <Setter TargetName='Arrow' Property='RenderTransform'>\r\n\t\t                                        <Setter.Value><RotateTransform Angle='180' /></Setter.Value>\r\n\t\t                                    </Setter>\r\n\t\t                                </Trigger>\r\n\t\t                            </ControlTemplate.Triggers>\r\n\t\t                        </ControlTemplate>\r\n\t\t                    </ToggleButton.Template>\r\n\t\t                </ToggleButton>\r\n\t\t                <ContentPresenter x:Name='ContentSite' IsHitTestVisible='False'\r\n\t\t                                  Content='{TemplateBinding SelectionBoxItem}'\r\n\t\t                                  ContentTemplate='{TemplateBinding SelectionBoxItemTemplate}'\r\n\t\t                                  ContentTemplateSelector='{TemplateBinding ItemTemplateSelector}'\r\n\t\t                                  Margin='13,0,30,0' VerticalAlignment='Center' HorizontalAlignment='Left' />\r\n\t\t            </Grid>\r\n\t\t        </Border>\r\n\t\t        <Popup x:Name='Popup' Placement='Bottom' IsOpen='{TemplateBinding IsDropDownOpen}'\r\n\t\t               AllowsTransparency='True' Focusable='False' PopupAnimation='Fade'>\r\n\t\t            <Border x:Name='DropDownBorder' MinWidth='{TemplateBinding ActualWidth}' MaxHeight='230'\r\n\t\t                    Margin='0,6,0,0' CornerRadius='12' BorderThickness='1' Padding='6'>\r\n\t\t                <Border.BorderBrush>\r\n\t\t                    <LinearGradientBrush StartPoint='0,0' EndPoint='0,1'>\r\n\t\t                        <GradientStop Color='#88FAE8BE' Offset='0' /><GradientStop Color='#55E0BD76' Offset='0.5' /><GradientStop Color='#33A88244' Offset='1' />\r\n\t\t                    </LinearGradientBrush>\r\n\t\t                </Border.BorderBrush>\r\n\t\t                <Border.Background>\r\n\t\t                    <LinearGradientBrush StartPoint='0,0' EndPoint='0,1'>\r\n\t\t                        <GradientStop Color='#FA1D1A12' Offset='0' /><GradientStop Color='#FA0C0B08' Offset='1' />\r\n\t\t                    </LinearGradientBrush>\r\n\t\t                </Border.Background>\r\n\t\t                <ScrollViewer SnapsToDevicePixels='True' VerticalScrollBarVisibility='Auto' HorizontalScrollBarVisibility='Disabled'>\r\n\t\t                    <StackPanel IsItemsHost='True' KeyboardNavigation.DirectionalNavigation='Contained' />\r\n\t\t                </ScrollViewer>\r\n\t\t            </Border>\r\n\t\t        </Popup>\r\n\t\t    </Grid>\r\n\t\t    <ControlTemplate.Triggers>\r\n\t\t        <Trigger Property='IsMouseOver' Value='True'>\r\n\t\t            <Setter TargetName='MainBorder' Property='BorderBrush' Value='#3AFFFFFF' />\r\n\t\t        </Trigger>\r\n\t\t        <Trigger Property='IsDropDownOpen' Value='True'>\r\n\t\t            <Setter TargetName='MainBorder' Property='BorderBrush' Value='#D2E0BD76' />\r\n\t\t            <Setter TargetName='MainBorder' Property='Background' Value='#14E0BD76' />\r\n\t\t        </Trigger>\r\n\t\t    </ControlTemplate.Triggers>\r\n\t\t</ControlTemplate>";
			combo.Template = (ControlTemplate)XamlReader.Parse(xamlText2);
			combo.PreviewMouseWheel += delegate(object s, MouseWheelEventArgs e)
			{
				if (combo.IsDropDownOpen)
				{
					return;
				}
				e.Handled = true;
				UIElement uielement = combo.Parent as UIElement;
				if (uielement != null)
				{
					uielement.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
					{
						RoutedEvent = UIElement.MouseWheelEvent,
						Source = combo
					});
				}
			};
			return combo;
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x0000297E File Offset: 0x00000B7E
		private System.Windows.Controls.TextBox BuildPremiumTextBox(string initialText, string tooltip, out Border hostBorder)
		{
			return this.BuildPremiumTextBox(initialText, tooltip, null, out hostBorder);
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x00014250 File Offset: 0x00012450
		private Border BuildPremiumToggle(bool initialState, out Ellipse knob)
		{
			Border border = new Border();
			border.Width = 46.0;
			border.Height = 24.0;
			border.CornerRadius = new CornerRadius(12.0);
			border.Background = (initialState ? this.MakeGoldBrush(0.0) : this.WhiteA(55));
			border.Cursor = System.Windows.Input.Cursors.Hand;
			border.VerticalAlignment = VerticalAlignment.Center;
			Ellipse ellipse = new Ellipse
			{
				Width = 18.0,
				Height = 18.0,
				Fill = Brushes.White,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = (initialState ? new Thickness(25.0, 0.0, 0.0, 0.0) : new Thickness(3.0, 0.0, 0.0, 0.0))
			};
			border.Child = ellipse;
			knob = ellipse;
			return border;
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x0001436C File Offset: 0x0001256C
		private void AnimatePremiumToggle(Border toggle, Ellipse knob, bool state)
		{
			ThicknessAnimation animation = new ThicknessAnimation
			{
				To = new Thickness?(state ? new Thickness(25.0, 0.0, 0.0, 0.0) : new Thickness(3.0, 0.0, 0.0, 0.0)),
				Duration = new Duration(TimeSpan.FromMilliseconds(150.0))
			};
			knob.BeginAnimation(FrameworkElement.MarginProperty, animation);
			toggle.Background = (state ? this.MakeGoldBrush(0.0) : this.WhiteA(55));
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x00014428 File Offset: 0x00012628
		private Slider BuildPremiumSlider(double initialValue)
		{
			Slider slider = new Slider();
			slider.Minimum = 0.0;
			slider.Maximum = 1.0;
			slider.Value = initialValue;
			slider.SmallChange = 0.05;
			slider.LargeChange = 0.1;
			slider.Height = 24.0;
			slider.Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
			slider.Cursor = System.Windows.Input.Cursors.Hand;
			slider.Focusable = false;
			string xamlText = "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'\r\n\t\t                 xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='Slider'>\r\n\t\t    <Grid VerticalAlignment='Center'>\r\n\t\t        <Border x:Name='TrackBg' Height='6' CornerRadius='3' BorderThickness='1' BorderBrush='#22FFFFFF'>\r\n\t\t            <Border.Background>\r\n\t\t                <LinearGradientBrush StartPoint='0,0' EndPoint='1,0'>\r\n\t\t                    <GradientStop Color='#2AFFFFFF' Offset='0'/><GradientStop Color='#12FFFFFF' Offset='1'/>\r\n\t\t                </LinearGradientBrush>\r\n\t\t            </Border.Background>\r\n\t\t        </Border>\r\n\t\t        <Track x:Name='PART_Track'>\r\n\t\t            <Track.DecreaseRepeatButton>\r\n\t\t                <RepeatButton>\r\n\t\t                    <RepeatButton.Template>\r\n\t\t                        <ControlTemplate TargetType='RepeatButton'>\r\n\t\t                            <Border Height='6' CornerRadius='3'>\r\n\t\t                                <Border.Background>\r\n\t\t                                    <LinearGradientBrush StartPoint='0,0' EndPoint='1,0'>\r\n\t\t                                        <GradientStop Color='#FAE8C6' Offset='0'/><GradientStop Color='#E0BD76' Offset='0.55'/><GradientStop Color='#A88244' Offset='1'/>\r\n\t\t                                    </LinearGradientBrush>\r\n\t\t                                </Border.Background>\r\n\t\t                            </Border>\r\n\t\t                        </ControlTemplate>\r\n\t\t                    </RepeatButton.Template>\r\n\t\t                </RepeatButton>\r\n\t\t            </Track.DecreaseRepeatButton>\r\n\t\t            <Track.IncreaseRepeatButton>\r\n\t\t                <RepeatButton>\r\n\t\t                    <RepeatButton.Template>\r\n\t\t                        <ControlTemplate TargetType='RepeatButton'><Border Background='Transparent' Height='6' /></ControlTemplate>\r\n\t\t                    </RepeatButton.Template>\r\n\t\t                </RepeatButton>\r\n\t\t            </Track.IncreaseRepeatButton>\r\n\t\t            <Track.Thumb>\r\n\t\t                <Thumb x:Name='SliderThumb' Width='18' Height='18'>\r\n\t\t                    <Thumb.Template>\r\n\t\t                        <ControlTemplate TargetType='Thumb'>\r\n\t\t                            <Grid>\r\n\t\t                                <Ellipse Width='18' Height='18' Fill='#FFFFFFFF' Stroke='#66E0BD76' StrokeThickness='1' />\r\n\t\t                                <Ellipse Width='8' Height='8' Fill='#E0BD76' />\r\n\t\t                            </Grid>\r\n\t\t                        </ControlTemplate>\r\n\t\t                    </Thumb.Template>\r\n\t\t                </Thumb>\r\n\t\t            </Track.Thumb>\r\n\t\t        </Track>\r\n\t\t    </Grid>\r\n\t\t</ControlTemplate>";
			slider.Template = (ControlTemplate)XamlReader.Parse(xamlText);
			return slider;
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x000144E4 File Offset: 0x000126E4
		private Border BuildPremiumButton(string text)
		{
			Border button = new Border
			{
				Width = 150.0,
				Height = 36.0,
				CornerRadius = new CornerRadius(10.0),
				Cursor = System.Windows.Input.Cursors.Hand,
				Background = this.MakeGoldBrush(45.0),
				VerticalAlignment = VerticalAlignment.Center,
				RenderTransformOrigin = new Point(0.5, 0.5),
				RenderTransform = new ScaleTransform(1.0, 1.0)
			};
			button.Child = new TextBlock
			{
				Text = text,
				FontSize = 10.5,
				FontWeight = FontWeights.Bold,
				Foreground = new SolidColorBrush(Color.FromRgb(18, 14, 8)),
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			ScaleTransform scale = (ScaleTransform)button.RenderTransform;
			button.MouseEnter += delegate(object <p0>, System.Windows.Input.MouseEventArgs <p1>)
			{
				this.Animate(scale, 1.03, 120.0);
				button.Opacity = 0.93;
			};
			button.MouseLeave += delegate(object <p0>, System.Windows.Input.MouseEventArgs <p1>)
			{
				this.Animate(scale, 1.0, 140.0);
				button.Opacity = 1.0;
			};
			button.PreviewMouseDown += delegate(object <p0>, MouseButtonEventArgs <p1>)
			{
				this.Animate(scale, 0.97, 60.0);
			};
			button.PreviewMouseUp += delegate(object <p0>, MouseButtonEventArgs <p1>)
			{
				this.Animate(scale, 1.03, 80.0);
			};
			return button;
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x00014668 File Offset: 0x00012868
		private Border BuildGhostButton(string text, Color tint, Color foreground)
		{
			Border button = new Border
			{
				Background = new SolidColorBrush(Color.FromArgb(22, tint.R, tint.G, tint.B)),
				BorderBrush = new SolidColorBrush(Color.FromArgb(60, tint.R, tint.G, tint.B)),
				BorderThickness = new Thickness(1.0),
				CornerRadius = new CornerRadius(9.0),
				Padding = new Thickness(12.0, 7.0, 12.0, 7.0),
				Cursor = System.Windows.Input.Cursors.Hand
			};
			button.Child = new TextBlock
			{
				Text = text,
				FontSize = 10.5,
				FontWeight = FontWeights.SemiBold,
				Foreground = new SolidColorBrush(foreground),
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			button.MouseEnter += delegate(object <p0>, System.Windows.Input.MouseEventArgs <p1>)
			{
				button.Background = new SolidColorBrush(Color.FromArgb(55, tint.R, tint.G, tint.B));
			};
			button.MouseLeave += delegate(object <p0>, System.Windows.Input.MouseEventArgs <p1>)
			{
				button.Background = new SolidColorBrush(Color.FromArgb(22, tint.R, tint.G, tint.B));
			};
			return button;
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x000147D4 File Offset: 0x000129D4
		private void Animate(ScaleTransform t, double to, double ms)
		{
			DoubleAnimation animation = new DoubleAnimation
			{
				To = new double?(to),
				Duration = new Duration(TimeSpan.FromMilliseconds(ms))
			};
			t.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
			t.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
		}

		// Token: 0x060001BE RID: 446 RVA: 0x000029D3 File Offset: 0x00000BD3
		private SolidColorBrush WhiteA(byte alpha)
		{
			return new SolidColorBrush(Color.FromArgb(alpha, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		}

		// Token: 0x060001BF RID: 447 RVA: 0x000029EF File Offset: 0x00000BEF
		private SolidColorBrush GoldA(byte alpha)
		{
			return new SolidColorBrush(Color.FromArgb(alpha, 224, 189, 118));
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x00014A78 File Offset: 0x00012C78
		private LinearGradientBrush MakeFadeLine(bool horizontal, byte peakAlpha)
		{
			return new LinearGradientBrush(new GradientStopCollection
			{
				new GradientStop(Color.FromArgb(0, 224, 189, 118), 0.0),
				new GradientStop(Color.FromArgb(peakAlpha, 224, 189, 118), 0.5),
				new GradientStop(Color.FromArgb(0, 224, 189, 118), 1.0)
			}, new Point(0.0, 0.0), horizontal ? new Point(1.0, 0.0) : new Point(0.0, 1.0));
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x00014B4C File Offset: 0x00012D4C
		private StackPanel BuildSection(string title)
		{
			StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation = System.Windows.Controls.Orientation.Vertical;
			stackPanel.Margin = new Thickness(0.0, 0.0, 0.0, 8.0);
			Grid grid = new Grid
			{
				Margin = new Thickness(2.0, 4.0, 0.0, 10.0)
			};
			grid.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			grid.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			grid.Children.Add(new TextBlock
			{
				Text = title,
				FontSize = 10.0,
				FontWeight = FontWeights.Bold,
				Foreground = this.MakeGoldBrush(0.0),
				VerticalAlignment = VerticalAlignment.Center
			});
			Border element = new Border
			{
				Height = 1.0,
				Margin = new Thickness(12.0, 0.0, 0.0, 0.0),
				VerticalAlignment = VerticalAlignment.Center,
				Background = this.MakeLinear(Color.FromArgb(90, 224, 189, 118), Color.FromArgb(0, 224, 189, 118), new Point(0.0, 0.0), new Point(1.0, 0.0))
			};
			Grid.SetColumn(element, 1);
			grid.Children.Add(element);
			stackPanel.Children.Add(grid);
			return stackPanel;
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x00002A08 File Offset: 0x00000C08
		private Style BuildSlimScrollBarStyle()
		{
			return (Style)XamlReader.Parse("<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'\r\n\t\t\t   xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='ScrollBar'>\r\n\t\t\t<Setter Property='Background' Value='Transparent' />\r\n\t\t\t<Setter Property='Width' Value='8' />\r\n\t\t\t<Setter Property='Template'>\r\n\t\t\t\t<Setter.Value>\r\n\t\t\t\t\t<ControlTemplate TargetType='ScrollBar'>\r\n\t\t\t\t\t\t<Grid Background='Transparent'>\r\n\t\t\t\t\t\t\t<Border Width='4' CornerRadius='2' Background='#16FFFFFF' HorizontalAlignment='Center' />\r\n\t\t\t\t\t\t\t<Track x:Name='PART_Track' IsDirectionReversed='True'>\r\n\t\t\t\t\t\t\t\t<Track.DecreaseRepeatButton>\r\n\t\t\t\t\t\t\t\t\t<RepeatButton Command='ScrollBar.PageUpCommand' Focusable='False'>\r\n\t\t\t\t\t\t\t\t\t\t<RepeatButton.Template>\r\n\t\t\t\t\t\t\t\t\t\t\t<ControlTemplate TargetType='RepeatButton'><Border Background='Transparent' /></ControlTemplate>\r\n\t\t\t\t\t\t\t\t\t\t</RepeatButton.Template>\r\n\t\t\t\t\t\t\t\t\t</RepeatButton>\r\n\t\t\t\t\t\t\t\t</Track.DecreaseRepeatButton>\r\n\t\t\t\t\t\t\t\t<Track.IncreaseRepeatButton>\r\n\t\t\t\t\t\t\t\t\t<RepeatButton Command='ScrollBar.PageDownCommand' Focusable='False'>\r\n\t\t\t\t\t\t\t\t\t\t<RepeatButton.Template>\r\n\t\t\t\t\t\t\t\t\t\t\t<ControlTemplate TargetType='RepeatButton'><Border Background='Transparent' /></ControlTemplate>\r\n\t\t\t\t\t\t\t\t\t\t</RepeatButton.Template>\r\n\t\t\t\t\t\t\t\t\t</RepeatButton>\r\n\t\t\t\t\t\t\t\t</Track.IncreaseRepeatButton>\r\n\t\t\t\t\t\t\t\t<Track.Thumb>\r\n\t\t\t\t\t\t\t\t\t<Thumb>\r\n\t\t\t\t\t\t\t\t\t\t<Thumb.Template>\r\n\t\t\t\t\t\t\t\t\t\t\t<ControlTemplate TargetType='Thumb'>\r\n\t\t\t\t\t\t\t\t\t\t\t\t<Border Width='4' CornerRadius='2' HorizontalAlignment='Center' Background='#B0E0BD76' />\r\n\t\t\t\t\t\t\t\t\t\t\t</ControlTemplate>\r\n\t\t\t\t\t\t\t\t\t\t</Thumb.Template>\r\n\t\t\t\t\t\t\t\t\t</Thumb>\r\n\t\t\t\t\t\t\t\t</Track.Thumb>\r\n\t\t\t\t\t\t\t</Track>\r\n\t\t\t\t\t\t</Grid>\r\n\t\t\t\t\t</ControlTemplate>\r\n\t\t\t\t</Setter.Value>\r\n\t\t\t</Setter>\r\n\t\t</Style>");
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x00014D24 File Offset: 0x00012F24
		private string RamMbToLabel(string mb)
		{
			int num;
			if (!int.TryParse((mb ?? "").Trim(), out num) || num <= 0)
			{
				return "4 GB";
			}
			if (num % 1024 == 0)
			{
				return (num / 1024).ToString() + " GB";
			}
			return num.ToString() + " MB";
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x00014D88 File Offset: 0x00012F88
		private string RamLabelToMb(string label)
		{
			string[] array = (label ?? "").Trim().Split(new char[]
			{
				' '
			});
			int num;
			if (array.Length != 2 || !int.TryParse(array[0], out num) || num <= 0)
			{
				return null;
			}
			if (array[1].Equals("GB", StringComparison.OrdinalIgnoreCase))
			{
				num *= 1024;
			}
			return num.ToString();
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x00014DEC File Offset: 0x00012FEC
		private string ParseRamInput(string input)
		{
			if (input == null || input.Trim().Length == 0)
			{
				return null;
			}
			string text = input.Trim().ToUpperInvariant().Replace(" ", "");
			double num = 0.0;
			if (text.EndsWith("GB"))
			{
				num = 1024.0;
				text = text.Substring(0, text.Length - 2);
			}
			else if (text.EndsWith("MB"))
			{
				num = 1.0;
				text = text.Substring(0, text.Length - 2);
			}
			else if (text.EndsWith("G"))
			{
				num = 1024.0;
				text = text.Substring(0, text.Length - 1);
			}
			else if (text.EndsWith("M"))
			{
				num = 1.0;
				text = text.Substring(0, text.Length - 1);
			}
			double num2;
			if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out num2) || num2 <= 0.0)
			{
				return null;
			}
			if (num == 0.0)
			{
				num = ((num2 <= 128.0) ? 1024.0 : 1.0);
			}
			double num3 = Math.Round(num2 * num);
			if (num3 < 1024.0 || num3 > 1048576.0)
			{
				return null;
			}
			return ((long)num3).ToString();
		}

		// Token: 0x060001C6 RID: 454
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GlobalMemoryStatusEx([In] [Out] MainWindow.MemoryStatusEx buffer);

		// Token: 0x060001C7 RID: 455 RVA: 0x00014F50 File Offset: 0x00013150
		private long GetTotalPhysicalMemoryMb()
		{
			try
			{
				MainWindow.MemoryStatusEx memoryStatusEx = new MainWindow.MemoryStatusEx();
				if (MainWindow.GlobalMemoryStatusEx(memoryStatusEx))
				{
					return (long)(memoryStatusEx.ullTotalPhys / 1048576UL);
				}
			}
			catch
			{
			}
			return 0L;
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x00014F94 File Offset: 0x00013194
		private System.Windows.Controls.TextBox BuildPremiumTextBox(string initialText, string tooltip, string placeholder, out Border hostBorder)
		{
			Border host = new Border
			{
				Height = 34.0,
				CornerRadius = new CornerRadius(8.0),
				BorderThickness = new Thickness(1.0),
				BorderBrush = this.WhiteA(26),
				Background = this.WhiteA(10)
			};
			Grid grid = new Grid();
			System.Windows.Controls.TextBox box = new System.Windows.Controls.TextBox
			{
				Text = initialText,
				FontSize = 12.0,
				Foreground = this.WhiteA(235),
				Background = Brushes.Transparent,
				BorderThickness = new Thickness(0.0),
				Padding = new Thickness(12.0, 0.0, 12.0, 0.0),
				VerticalContentAlignment = VerticalAlignment.Center,
				HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
				ToolTip = tooltip,
				CaretBrush = this.MakeGoldBrush(0.0),
				SelectionBrush = this.GoldA(110)
			};
			TextBlock hint = new TextBlock
			{
				Text = placeholder,
				FontSize = 12.0,
				Foreground = this.WhiteA(70),
				IsHitTestVisible = false,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(13.0, 0.0, 12.0, 0.0),
				Visibility = ((!string.IsNullOrEmpty(placeholder) && string.IsNullOrEmpty(initialText)) ? Visibility.Visible : Visibility.Collapsed)
			};
			grid.Children.Add(box);
			grid.Children.Add(hint);
			host.Child = grid;
			box.TextChanged += delegate(object <p0>, TextChangedEventArgs <p1>)
			{
				hint.Visibility = ((!string.IsNullOrEmpty(placeholder) && box.Text.Length == 0) ? Visibility.Visible : Visibility.Collapsed);
			};
			box.GotFocus += delegate(object <p0>, RoutedEventArgs <p1>)
			{
				host.BorderBrush = this.GoldA(210);
				host.Background = this.WhiteA(14);
			};
			box.LostFocus += delegate(object <p0>, RoutedEventArgs <p1>)
			{
				host.BorderBrush = this.WhiteA(26);
				host.Background = this.WhiteA(10);
			};
			host.MouseEnter += delegate(object <p0>, System.Windows.Input.MouseEventArgs <p1>)
			{
				if (!box.IsKeyboardFocusWithin)
				{
					host.BorderBrush = this.WhiteA(58);
				}
			};
			host.MouseLeave += delegate(object <p0>, System.Windows.Input.MouseEventArgs <p1>)
			{
				if (!box.IsKeyboardFocusWithin)
				{
					host.BorderBrush = this.WhiteA(26);
				}
			};
			hostBorder = host;
			return box;
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x00015214 File Offset: 0x00013414
		private void InitPanelFonts()
		{
			Uri baseUri;
			try
			{
				string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
				baseUri = (File.Exists(System.IO.Path.Combine(baseDirectory, "Fonts", "Poppins-Regular.ttf")) ? new Uri(baseDirectory, UriKind.Absolute) : new Uri("pack://application:,,,/", UriKind.Absolute));
			}
			catch
			{
				baseUri = new Uri("pack://application:,,,/", UriKind.Absolute);
			}
			this.mFontPoppins = new FontFamily(baseUri, "./Fonts/#Google Sans");
			this.mFontPoppinsMedium = new FontFamily(baseUri, "./Fonts/#Google Sans");
			this.mFontPoppinsSemiBold = new FontFamily(baseUri, "./Fonts/#Google Sans");
		}

		// Token: 0x060001CA RID: 458 RVA: 0x00002A19 File Offset: 0x00000C19
		private void ApplyPanelFonts(DependencyObject node)
		{
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x00015308 File Offset: 0x00013508
		private Border CreateMuteButton()
		{
			if (this.mMuteButton != null)
			{
				return this.mMuteButton;
			}
			Border button = new Border
			{
				Width = 38.0,
				Height = 38.0,
				CornerRadius = new CornerRadius(19.0),
				Background = this.WhiteA(14),
				BorderBrush = this.WhiteA(38),
				BorderThickness = new Thickness(1.0),
				Cursor = System.Windows.Input.Cursors.Hand,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(0.0, 0.0, 8.0, 0.0)
			};
			TextBlock icon = new TextBlock
			{
				Text = "\ud83d\udd0a",
				FontSize = 16.0,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Foreground = Brushes.White
			};
			button.Child = icon;
			button.MouseEnter += delegate(object sender, System.Windows.Input.MouseEventArgs e)
			{
				button.Background = this.GoldA(28);
				button.BorderBrush = this.GoldA(110);
			};
			button.MouseLeave += delegate(object sender, System.Windows.Input.MouseEventArgs e)
			{
				button.Background = (this.mMusicMuted ? this.GoldA(22) : this.WhiteA(14));
				button.BorderBrush = (this.mMusicMuted ? this.GoldA(90) : this.WhiteA(38));
			};
			button.MouseLeftButtonUp += delegate(object sender, MouseButtonEventArgs e)
			{
				this.mMusicMuted = !this.mMusicMuted;
				if (this.mBgMusicPlayer != null)
				{
					if (this.mMusicMuted)
					{
						this.mBgMusicPlayer.Volume = 0.0;
						icon.Text = "\ud83d\udd07";
					}
					else
					{
						double volume = (this.mVolumeSlider != null) ? this.mVolumeSlider.Value : 0.1;
						this.mBgMusicPlayer.Volume = volume;
						icon.Text = "\ud83d\udd0a";
					}
				}
				button.Background = (this.mMusicMuted ? this.GoldA(22) : this.WhiteA(14));
				button.BorderBrush = (this.mMusicMuted ? this.GoldA(90) : this.WhiteA(38));
				e.Handled = true;
			};
			this.mMuteButton = button;
			if (this.mInstallerCloseMinimizePanel != null)
			{
				this.mInstallerCloseMinimizePanel.Children.Insert(0, button);
			}
			return button;
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x000154AC File Offset: 0x000136AC
		private static void ApplyWindowsOptimizations()
		{
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\GameDVR"))
				{
					if (registryKey != null)
					{
						registryKey.SetValue("AppCaptureEnabled", 0, RegistryValueKind.DWord);
					}
					if (registryKey != null)
					{
						registryKey.SetValue("HistoricalCaptureEnabled", 0, RegistryValueKind.DWord);
					}
				}
				using (RegistryKey registryKey2 = Registry.CurrentUser.CreateSubKey("System\\GameConfigStore"))
				{
					if (registryKey2 != null)
					{
						registryKey2.SetValue("GameDVR_Enabled", 0, RegistryValueKind.DWord);
					}
					if (registryKey2 != null)
					{
						registryKey2.SetValue("GameDVR_FSEBehaviorMode", 2, RegistryValueKind.DWord);
					}
					if (registryKey2 != null)
					{
						registryKey2.SetValue("GameDVR_HonorUserFSEBehaviorMode", 1, RegistryValueKind.DWord);
					}
					if (registryKey2 != null)
					{
						registryKey2.SetValue("GameDVR_DXGIHonorFSEWindowsCompatible", 1, RegistryValueKind.DWord);
					}
				}
				using (RegistryKey registryKey3 = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\GameDVR"))
				{
					if (registryKey3 != null)
					{
						registryKey3.SetValue("AudioCaptureEnabled", 0, RegistryValueKind.DWord);
					}
				}
				object value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\BlueStacks_msi5", "InstallDir", "");
				string text = System.IO.Path.Combine(((value != null) ? value.ToString() : null) ?? "", "HD-Player.exe");
				if (!File.Exists(text))
				{
					foreach (string text2 in new string[]
					{
						"C:\\Program Files\\BlueStacks_msi5\\HD-Player.exe"
					})
					{
						if (File.Exists(text2))
						{
							text = text2;
							break;
						}
					}
				}
				if (File.Exists(text))
				{
					using (RegistryKey registryKey4 = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\DirectX\\UserGpuPreferences"))
					{
						if (registryKey4 != null)
						{
							registryKey4.SetValue(text, "GpuPreference=2;", RegistryValueKind.String);
						}
					}
					using (RegistryKey registryKey5 = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers"))
					{
						if (registryKey5 != null)
						{
							registryKey5.SetValue(text, "~ DISABLEDXMAXIMIZEDWINDOWEDMODE", RegistryValueKind.String);
						}
					}
				}
				using (RegistryKey registryKey6 = Registry.LocalMachine.CreateSubKey("SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers"))
				{
					if (registryKey6 != null)
					{
						registryKey6.SetValue("HwSchMode", 2, RegistryValueKind.DWord);
					}
				}
				try
				{
					foreach (Process process in Process.GetProcessesByName("HD-Player"))
					{
						try
						{
							process.PriorityClass = ProcessPriorityClass.High;
						}
						catch
						{
						}
						try
						{
							process.PriorityBoostEnabled = true;
						}
						catch
						{
						}
					}
				}
				catch
				{
				}
				Logger.Info("Emulator Windows optimizations applied successfully.");
			}
			catch (Exception ex)
			{
				Logger.Warning("Emulator Windows optimization failed: " + ex.Message);
			}
		}

		// Token: 0x060001D6 RID: 470
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetProcessInformation(IntPtr hProcess, MainWindow.PROCESS_INFORMATION_CLASS processInformationClass, ref MainWindow.MEMORY_PRIORITY_INFORMATION processInformation, uint processInformationSize);

		// Token: 0x060001D7 RID: 471
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetProcessInformation(IntPtr hProcess, MainWindow.PROCESS_INFORMATION_CLASS processInformationClass, ref MainWindow.PROCESS_IO_PRIORITY_INFORMATION processInformation, uint processInformationSize);

		// Token: 0x060001D8 RID: 472
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetProcessInformation(IntPtr hProcess, MainWindow.PROCESS_INFORMATION_CLASS processInformationClass, ref MainWindow.PROCESS_POWER_THROTTLING_STATE processInformation, uint processInformationSize);

		// Token: 0x060001D9 RID: 473 RVA: 0x00015820 File Offset: 0x00013A20
		private static void Litemax()
		{
			try
			{
				foreach (string processName in new string[]
				{
					"HD-Player",
					"HD-GLCheck",
					"BstkSVC"
				})
				{
					try
					{
						foreach (Process process in Process.GetProcessesByName(processName))
						{
							try
							{
								if (!process.HasExited)
								{
									process.PriorityClass = ProcessPriorityClass.High;
									MainWindow.PROCESS_POWER_THROTTLING_STATE structure = new MainWindow.PROCESS_POWER_THROTTLING_STATE
									{
										Version = 1U,
										ControlMask = 1U,
										StateMask = 0U
									};
									MainWindow.SetProcessInformation(process.Handle, MainWindow.PROCESS_INFORMATION_CLASS.ProcessPowerThrottling, ref structure, (uint)Marshal.SizeOf<MainWindow.PROCESS_POWER_THROTTLING_STATE>(structure));
									MainWindow.MEMORY_PRIORITY_INFORMATION structure2 = new MainWindow.MEMORY_PRIORITY_INFORMATION
									{
										MemoryPriority = 5U
									};
									MainWindow.SetProcessInformation(process.Handle, MainWindow.PROCESS_INFORMATION_CLASS.ProcessMemoryPriority, ref structure2, (uint)Marshal.SizeOf<MainWindow.MEMORY_PRIORITY_INFORMATION>(structure2));
									MainWindow.PROCESS_IO_PRIORITY_INFORMATION structure3 = new MainWindow.PROCESS_IO_PRIORITY_INFORMATION
									{
										IoPriority = 3U
									};
									MainWindow.SetProcessInformation(process.Handle, MainWindow.PROCESS_INFORMATION_CLASS.ProcessIoPriority, ref structure3, (uint)Marshal.SizeOf<MainWindow.PROCESS_IO_PRIORITY_INFORMATION>(structure3));
								}
							}
							catch
							{
							}
							finally
							{
								process.Dispose();
							}
						}
					}
					catch
					{
					}
				}
				MainWindow.ApplyWindowsOptimizations();
			}
			catch
			{
			}
		}

		// Token: 0x060001DA RID: 474 RVA: 0x000159B4 File Offset: 0x00013BB4
		private Border CreateCircleButton(string text, string textColor, double fontSize)
		{
			Border border = new Border();
			border.Width = 28.0;
			border.Height = 28.0;
			border.CornerRadius = new CornerRadius(14.0);
			border.Background = new SolidColorBrush(Color.FromArgb(210, 20, 20, 20));
			border.Margin = new Thickness(5.0, 12.0, 5.0, 5.0);
			border.Cursor = System.Windows.Input.Cursors.Hand;
			TextBlock child = new TextBlock
			{
				Text = text,
				Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textColor)),
				FontSize = fontSize,
				FontWeight = FontWeights.SemiBold,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				TextAlignment = TextAlignment.Center
			};
			border.Child = child;
			return border;
		}

		// Token: 0x060001DB RID: 475 RVA: 0x00015AA0 File Offset: 0x00013CA0
		private void CreateCustomWindowButtons()
		{
			this.mMinimizeButton.Visibility = Visibility.Collapsed;
			this.mCloseButton.Visibility = Visibility.Collapsed;
			this.mInstallerCloseMinimizePanel.Children.Clear();
			this.mInstallerCloseMinimizePanel.Background = Brushes.Transparent;
			Border minimizeButton = new Border
			{
				Width = 35.0,
				Height = 35.0,
				CornerRadius = new CornerRadius(17.5),
				Background = new SolidColorBrush(Color.FromArgb(35, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				BorderBrush = new SolidColorBrush(Color.FromArgb(55, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				BorderThickness = new Thickness(1.0),
				Margin = new Thickness(5.0, 8.0, 3.0, 5.0),
				Cursor = System.Windows.Input.Cursors.Hand
			};
			System.Windows.Shapes.Path child = new System.Windows.Shapes.Path
			{
				Data = Geometry.Parse("M 3,8 L 13,8"),
				Stroke = Brushes.White,
				StrokeThickness = 1.5,
				StrokeStartLineCap = PenLineCap.Round,
				StrokeEndLineCap = PenLineCap.Round,
				Width = 16.0,
				Height = 16.0,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			minimizeButton.Child = child;
			minimizeButton.MouseEnter += delegate(object sender, System.Windows.Input.MouseEventArgs e)
			{
				minimizeButton.Background = new SolidColorBrush(Color.FromArgb(55, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				minimizeButton.BorderBrush = new SolidColorBrush(Color.FromArgb(90, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			};
			minimizeButton.MouseLeave += delegate(object sender, System.Windows.Input.MouseEventArgs e)
			{
				minimizeButton.Background = new SolidColorBrush(Color.FromArgb(35, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				minimizeButton.BorderBrush = new SolidColorBrush(Color.FromArgb(55, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			};
			minimizeButton.MouseLeftButtonUp += delegate(object sender, MouseButtonEventArgs e)
			{
				this.WindowState = WindowState.Minimized;
			};
			Border closeButton = new Border
			{
				Width = 35.0,
				Height = 35.0,
				CornerRadius = new CornerRadius(17.5),
				Background = new SolidColorBrush(Color.FromArgb(35, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				BorderBrush = new SolidColorBrush(Color.FromArgb(55, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
				BorderThickness = new Thickness(1.0),
				Margin = new Thickness(3.0, 8.0, 15.0, 5.0),
				Cursor = System.Windows.Input.Cursors.Hand
			};
			System.Windows.Shapes.Path child2 = new System.Windows.Shapes.Path
			{
				Data = Geometry.Parse("M 4,4 L 12,12 M 12,4 L 4,12"),
				Stroke = Brushes.White,
				StrokeThickness = 1.5,
				StrokeStartLineCap = PenLineCap.Round,
				StrokeEndLineCap = PenLineCap.Round,
				Width = 16.0,
				Height = 16.0,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			closeButton.Child = child2;
			closeButton.MouseEnter += delegate(object sender, System.Windows.Input.MouseEventArgs e)
			{
				closeButton.Background = new SolidColorBrush(Color.FromArgb(60, byte.MaxValue, 55, 55));
				closeButton.BorderBrush = new SolidColorBrush(Color.FromArgb(110, byte.MaxValue, 80, 80));
			};
			closeButton.MouseLeave += delegate(object sender, System.Windows.Input.MouseEventArgs e)
			{
				closeButton.Background = new SolidColorBrush(Color.FromArgb(35, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				closeButton.BorderBrush = new SolidColorBrush(Color.FromArgb(55, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			};
			closeButton.MouseLeftButtonUp += delegate(object sender, MouseButtonEventArgs e)
			{
				this.Close();
			};
			this.mInstallerCloseMinimizePanel.Children.Add(minimizeButton);
			this.mInstallerCloseMinimizePanel.Children.Add(closeButton);
		}

		// Token: 0x040000C6 RID: 198
		private BackgroundWorker mBackgroundWorker;

		// Token: 0x040000C7 RID: 199
		private System.Timers.Timer mProgressUpdateTimer;

		// Token: 0x040000C8 RID: 200
		private System.Timers.Timer mProgressTextAnimationTimer;

		// Token: 0x040000C9 RID: 201
		private System.Timers.Timer mBackgroundChangeTimer;

		// Token: 0x040000CA RID: 202
		private string[] mBackgroundImages;

		// Token: 0x040000CB RID: 203
		private int mBackgroundSelectedIndex;

		// Token: 0x040000CC RID: 204
		private bool mDontShowClosePopUp;

		// Token: 0x040000CD RID: 205
		private int mCurrentSectionMaxProgress;

		// Token: 0x040000CE RID: 206
		private int mInstallFailedErrorCode;

		// Token: 0x040000DC RID: 220
		private Grid mSettingsOverlay;

		// Token: 0x040000DD RID: 221
		private Border mSettingsPanelBorder;

		// Token: 0x040000DE RID: 222
		private Slider mVolumeSlider;

		// Token: 0x040000DF RID: 223
		private TextBlock mVolumePercentLabel;

		// Token: 0x040000E0 RID: 224
		private MediaPlayer mBgMusicPlayer;

		// Token: 0x040000E1 RID: 225
		private bool mCustomWallpaperActive;

		// Token: 0x040000E2 RID: 226
		private const string DefaultMusicFileName = "installer_music.mp3";

		// Token: 0x040000E3 RID: 227
		private Color AccentGold;

		// Token: 0x040000E4 RID: 228
		private Color AccentGoldDeep;

		// Token: 0x040000E5 RID: 229
		private Color AccentGoldPale;

		// Token: 0x040000E6 RID: 230
		private FontFamily mFontPoppins;

		// Token: 0x040000E7 RID: 231
		private FontFamily mFontPoppinsMedium;

		// Token: 0x040000E8 RID: 232
		private FontFamily mFontPoppinsSemiBold;

		// Token: 0x040000E9 RID: 233
		private MediaElement mForegroundMedia;

		// Token: 0x040000EA RID: 234
		private MediaElement mBackgroundMedia;

		// Token: 0x040000EB RID: 235
		private Border mMuteButton;

		// Token: 0x040000EC RID: 236
		private bool mMusicMuted;

		// Token: 0x040000ED RID: 237
		private const uint PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 1U;

		// Token: 0x02000025 RID: 37
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private class MemoryStatusEx
		{
			// Token: 0x060001F6 RID: 502 RVA: 0x00002B86 File Offset: 0x00000D86
			public MemoryStatusEx()
			{
				this.dwLength = (uint)Marshal.SizeOf(typeof(MainWindow.MemoryStatusEx));
			}

			// Token: 0x040000FE RID: 254
			public uint dwLength;

			// Token: 0x040000FF RID: 255
			public uint dwMemoryLoad;

			// Token: 0x04000100 RID: 256
			public ulong ullTotalPhys;

			// Token: 0x04000101 RID: 257
			public ulong ullAvailPhys;

			// Token: 0x04000102 RID: 258
			public ulong ullTotalPageFile;

			// Token: 0x04000103 RID: 259
			public ulong ullAvailPageFile;

			// Token: 0x04000104 RID: 260
			public ulong ullTotalVirtual;

			// Token: 0x04000105 RID: 261
			public ulong ullAvailVirtual;

			// Token: 0x04000106 RID: 262
			public ulong ullAvailExtendedVirtual;
		}

		// Token: 0x0200003D RID: 61
		private enum PROCESS_INFORMATION_CLASS
		{
			// Token: 0x04000176 RID: 374
			ProcessMemoryPriority,
			// Token: 0x04000177 RID: 375
			ProcessIoPriority = 2,
			// Token: 0x04000178 RID: 376
			ProcessPowerThrottling = 4
		}

		// Token: 0x0200003E RID: 62
		private struct MEMORY_PRIORITY_INFORMATION
		{
			// Token: 0x04000179 RID: 377
			public uint MemoryPriority;
		}

		// Token: 0x0200003F RID: 63
		private struct PROCESS_IO_PRIORITY_INFORMATION
		{
			// Token: 0x0400017A RID: 378
			public uint IoPriority;
		}

		// Token: 0x02000040 RID: 64
		private struct PROCESS_POWER_THROTTLING_STATE
		{
			// Token: 0x0400017B RID: 379
			public uint Version;

			// Token: 0x0400017C RID: 380
			public uint ControlMask;

			// Token: 0x0400017D RID: 381
			public uint StateMask;
		}
	}
}
