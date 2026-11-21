using System;
using System.Windows;
using System.Windows.Controls;
using BlueStacks.Common;

namespace BlueStacks.Installer
{
	// Token: 0x02000010 RID: 16
	public partial class InstallFinish : UserControl
	{
		// Token: 0x0600004B RID: 75 RVA: 0x00006BF8 File Offset: 0x00004DF8
		public InstallFinish()
		{
			this.InitializeComponent();
			ResourceDictionary resourceDictionary = new ResourceDictionary
			{
				Source = new Uri("/HD-Common;component/Resources/ButtonResource.xaml", UriKind.RelativeOrAbsolute)
			};
			Style style = resourceDictionary["MaterialDesignButton"] as Style;
			bool flag = this.mLaunchBlueStacks.Style != style;
			if (flag)
			{
				this.mLaunchBlueStacks.Style = style;
			}
		}

	}
}
