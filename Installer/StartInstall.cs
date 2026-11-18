using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using BlueStacks.Common;

namespace BlueStacks.Installer
{
	// Token: 0x0200001D RID: 29
	public class StartInstall : UserControl, IComponentConnector
	{
		// Token: 0x060000ED RID: 237 RVA: 0x0000E39C File Offset: 0x0000C59C
		public StartInstall()
		{
			this.InitializeComponent();
			ResourceDictionary resourceDictionary = new ResourceDictionary
			{
				Source = new Uri("/HD-Common;component/Resources/ButtonResource.xaml", UriKind.RelativeOrAbsolute)
			};
			Style style = resourceDictionary["MaterialDesignButton"] as Style;
			bool flag = this.mInstallNowButton.Style != style;
			if (flag)
			{
				this.mInstallNowButton.Style = style;
			}
			base.Loaded += this.StartInstallUserControl_Loaded;
		}

		// Token: 0x060000EE RID: 238 RVA: 0x0000238F File Offset: 0x0000058F
		private void StartInstallUserControl_Loaded(object sender, RoutedEventArgs e)
		{
			this.mAgreeCheckBox.IsChecked = true;
		}

		// Token: 0x060000EF RID: 239 RVA: 0x0000239F File Offset: 0x0000059F
		private void SoftwareLincenceMouseDown(object sender, MouseButtonEventArgs e)
		{
			Process.Start("http://www.bluestacks.com/terms-and-privacy.html");
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x0000E418 File Offset: 0x0000C618
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			bool contentLoaded = this._contentLoaded;
			if (!contentLoaded)
			{
				this._contentLoaded = true;
				Uri resourceLocator = new Uri("/BlueStacksInstaller;component/controls/startinstall.xaml", UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x0000E450 File Offset: 0x0000C650
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				this.mInstallNowButton = (Button)target;
				break;
			case 2:
				this.mCustomInstallLicensePanel = (StackPanel)target;
				break;
			case 3:
				this.mCustomInstallImage = (CustomPictureBox)target;
				break;
			case 4:
				this.mCustomInstallLabel = (Label)target;
				break;
			case 5:
				this.mAgreeCheckBox = (TextWrapCustomCheckBox)target;
				break;
			case 6:
				((Label)target).MouseDown += this.SoftwareLincenceMouseDown;
				break;
			case 7:
				this.mReceiveCheckBox = (TextWrapCustomCheckBox)target;
				break;
			default:
				this._contentLoaded = true;
				break;
			}
		}

		// Token: 0x04000089 RID: 137
		internal Button mInstallNowButton;

		// Token: 0x0400008A RID: 138
		internal StackPanel mCustomInstallLicensePanel;

		// Token: 0x0400008B RID: 139
		internal CustomPictureBox mCustomInstallImage;

		// Token: 0x0400008C RID: 140
		internal Label mCustomInstallLabel;

		// Token: 0x0400008D RID: 141
		internal TextWrapCustomCheckBox mAgreeCheckBox;

		// Token: 0x0400008E RID: 142
		internal TextWrapCustomCheckBox mReceiveCheckBox;

		// Token: 0x0400008F RID: 143
		private bool _contentLoaded;
	}
}
