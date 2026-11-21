using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlueStacks.Common;

namespace BlueStacks.Installer
{
	// Token: 0x0200001D RID: 29
	public partial class StartInstall : UserControl
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

	}
}
