using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BlueStacks.Installer
{
	// Token: 0x0200000A RID: 10
	public class InstallError : UserControl, IComponentConnector
	{
		// Token: 0x06000028 RID: 40 RVA: 0x00002115 File Offset: 0x00000315
		public InstallError()
		{
			this.InitializeComponent();
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00005944 File Offset: 0x00003B44
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			bool contentLoaded = this._contentLoaded;
			if (!contentLoaded)
			{
				this._contentLoaded = true;
				Uri resourceLocator = new Uri("/BlueStacksInstaller;component/controls/installerror.xaml", UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}
		}

		// Token: 0x0600002A RID: 42 RVA: 0x0000597C File Offset: 0x00003B7C
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				this.mInstallErrorGrid = (StackPanel)target;
				break;
			case 2:
				this.mInstallErrorCode = (Label)target;
				break;
			case 3:
				this.mPreInstallErrorGrid = (StackPanel)target;
				break;
			case 4:
				this.mSystemRequirementNotMetLabel = (Label)target;
				break;
			case 5:
				this.mPreInstallReason1 = (TextBlock)target;
				break;
			case 6:
				this.mPreInstallReason2 = (TextBlock)target;
				break;
			case 7:
				this.mRestartNowButton = (Button)target;
				break;
			default:
				this._contentLoaded = true;
				break;
			}
		}

		// Token: 0x04000020 RID: 32
		internal StackPanel mInstallErrorGrid;

		// Token: 0x04000021 RID: 33
		internal Label mInstallErrorCode;

		// Token: 0x04000022 RID: 34
		internal StackPanel mPreInstallErrorGrid;

		// Token: 0x04000023 RID: 35
		internal Label mSystemRequirementNotMetLabel;

		// Token: 0x04000024 RID: 36
		internal TextBlock mPreInstallReason1;

		// Token: 0x04000025 RID: 37
		internal TextBlock mPreInstallReason2;

		// Token: 0x04000026 RID: 38
		internal Button mRestartNowButton;

		// Token: 0x04000027 RID: 39
		private bool _contentLoaded;
	}
}
