using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using BlueStacks.Common;

namespace BlueStacks.Installer
{
	// Token: 0x02000007 RID: 7
	public class ChooseInstallFolder : UserControl, IComponentConnector
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

		// Token: 0x06000017 RID: 23 RVA: 0x0000505C File Offset: 0x0000325C
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			if (!this._contentLoaded)
			{
				this._contentLoaded = true;
				Uri resourceLocator = new Uri("/BlueStacksInstaller;component/controls/chooseinstallfolder.xaml", UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}
		}

		// Token: 0x06000018 RID: 24 RVA: 0x0000508C File Offset: 0x0000328C
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				this.mBluestacksDataLocation = (CustomTextBox)target;
				return;
			case 2:
				this.mChooseFolderButton = (CustomPictureBox)target;
				return;
			case 3:
				this.mChooseFolderLabel = (Label)target;
				return;
			case 4:
				this.mSpaceAvailable = (Label)target;
				return;
			case 5:
				this.mInstallNowButton = (CustomPictureBox)target;
				return;
			case 6:
				this.mInstallNowLabel = (Label)target;
				return;
			case 7:
				this.mBackButton = (CustomPictureBox)target;
				return;
			case 8:
				this.mBackButtonLabel = (Label)target;
				return;
			default:
				this._contentLoaded = true;
				return;
			}
		}

		// Token: 0x0400000F RID: 15
		internal CustomTextBox mBluestacksDataLocation;

		// Token: 0x04000010 RID: 16
		internal CustomPictureBox mChooseFolderButton;

		// Token: 0x04000011 RID: 17
		internal Label mChooseFolderLabel;

		// Token: 0x04000012 RID: 18
		internal Label mSpaceAvailable;

		// Token: 0x04000013 RID: 19
		internal CustomPictureBox mInstallNowButton;

		// Token: 0x04000014 RID: 20
		internal Label mInstallNowLabel;

		// Token: 0x04000015 RID: 21
		internal CustomPictureBox mBackButton;

		// Token: 0x04000016 RID: 22
		internal Label mBackButtonLabel;

		// Token: 0x04000017 RID: 23
		private bool _contentLoaded;
	}
}
