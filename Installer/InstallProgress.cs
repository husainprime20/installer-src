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
	// Token: 0x02000011 RID: 17
	public class InstallProgress : UserControl, IComponentConnector
	{
		// Token: 0x0600004E RID: 78 RVA: 0x000021DE File Offset: 0x000003DE
		public InstallProgress()
		{
			this.InitializeComponent();
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00006CF8 File Offset: 0x00004EF8
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			bool contentLoaded = this._contentLoaded;
			if (!contentLoaded)
			{
				this._contentLoaded = true;
				Uri resourceLocator = new Uri("/BlueStacksInstaller;component/controls/installprogress.xaml", UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00006D30 File Offset: 0x00004F30
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				this.mProgressGrid = (Grid)target;
				break;
			case 2:
				this.mInstallProgressStatus = (Label)target;
				break;
			case 3:
				this.mInstallProgressPercentage = (Label)target;
				break;
			case 4:
				this.mInstallProgressBar = (ProgressBar)target;
				break;
			case 5:
				this.mTipsGrid = (Grid)target;
				break;
			case 6:
				this.mInstallTips = (CustomTextBlock)target;
				break;
			default:
				this._contentLoaded = true;
				break;
			}
		}

		// Token: 0x0400004E RID: 78
		internal Grid mProgressGrid;

		// Token: 0x0400004F RID: 79
		internal Label mInstallProgressStatus;

		// Token: 0x04000050 RID: 80
		internal Label mInstallProgressPercentage;

		// Token: 0x04000051 RID: 81
		internal ProgressBar mInstallProgressBar;

		// Token: 0x04000052 RID: 82
		internal Grid mTipsGrid;

		// Token: 0x04000053 RID: 83
		internal CustomTextBlock mInstallTips;

		// Token: 0x04000054 RID: 84
		private bool _contentLoaded;
	}
}
