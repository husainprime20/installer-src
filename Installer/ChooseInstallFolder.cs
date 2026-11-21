using System;
using System.Windows;
using System.Windows.Controls;
using BlueStacks.Common;

namespace BlueStacks.Installer
{
	// Token: 0x02000007 RID: 7
	public partial class ChooseInstallFolder : UserControl
	{
		// Token: 0x06000016 RID: 22 RVA: 0x00004F38 File Offset: 0x00003138
		public ChooseInstallFolder()
		{
			this.InitializeComponent();
			this.mBluestacksDataLocation.Width = 300.0;
			this.mChooseFolderLabel.Margin = new Thickness(0.0, 0.0, 0.0, 1.0);
			StackPanel stackPanel = this.mChooseFolderLabel.Parent as StackPanel;
			if (stackPanel != null)
			{
				stackPanel.Margin = new Thickness(-15.0, 0.0, 0.0, 0.0);
			}
			this.mChooseFolderButton.Visibility = Visibility.Collapsed;
			Grid grid = base.Content as Grid;
			if (grid != null)
			{
				foreach (object obj in grid.Children)
				{
					StackPanel stackPanel2 = ((UIElement)obj) as StackPanel;
					if (stackPanel2 != null && Grid.GetRow(stackPanel2) == 2)
					{
						stackPanel2.Visibility = Visibility.Collapsed;
						break;
					}
				}
			}
		}

	}
}
