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
	// Token: 0x02000010 RID: 16
	public class InstallFinish : UserControl, IComponentConnector
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

		// Token: 0x0600004C RID: 76 RVA: 0x00006C64 File Offset: 0x00004E64
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			bool contentLoaded = this._contentLoaded;
			if (!contentLoaded)
			{
				this._contentLoaded = true;
				Uri resourceLocator = new Uri("/BlueStacksInstaller;component/controls/installfinish.xaml", UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00006C9C File Offset: 0x00004E9C
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				this.mLaunchBlueStacks = (Button)target;
				break;
			case 2:
				this.mLaunchBSCheckBox = (TextWrapCustomCheckBox)target;
				break;
			case 3:
				this.mInstallDoneText = (Label)target;
				break;
			default:
				this._contentLoaded = true;
				break;
			}
		}

		// Token: 0x0400004A RID: 74
		internal Button mLaunchBlueStacks;

		// Token: 0x0400004B RID: 75
		internal TextWrapCustomCheckBox mLaunchBSCheckBox;

		// Token: 0x0400004C RID: 76
		internal Label mInstallDoneText;

		// Token: 0x0400004D RID: 77
		private bool _contentLoaded;
	}
}
