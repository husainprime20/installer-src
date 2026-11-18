using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using BlueStacks.Common;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace BlueStacks.Installer
{
	// Token: 0x02000003 RID: 3
	public static class ConfManager
	{
		// Token: 0x06000004 RID: 4 RVA: 0x0000319C File Offset: 0x0000139C
		private static string GenerateAndroidId()
		{
			byte[] array = new byte[8];
			Random random = new Random();
			random.NextBytes(array);
			return BitConverter.ToUInt64(array, 0).ToString("x16");
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000031D8 File Offset: 0x000013D8
		public static int CreateProperties(string confPath)
		{
			int num = 0;
			Logger.Info("In ConfManager.CreateProperties");
			try
			{
				string locale = LocaleStrings.Locale;
				string value;
				string text;
				string value2;
				string text2;
				string text3;
				bool flag;
				string text4;
				string text5;
				string text6;
				string value3;
				Utils.GetCodesAndCountryInfo(out value, out text, out value2, out text2, out text3, out flag, out text4, out text5, out text6, out value3, locale, "", "", InstallerProperties.CampaignMD5, InstallerProperties.GUID, InstallerProperties.DefaultVmName, InstallerProperties.CloudHost);
				bool value4 = Utils.CheckSsse3Info(Path.Combine(InstallerProperties.InstallerExtractedDir, "HD-CheckCpu.exe"));
				text4 = (string.IsNullOrEmpty(text4) ? "8.8.8.8" : text4);
				text5 = (string.IsNullOrEmpty(text5) ? "10.0.2.3" : text5);
				BridgeApis.ConfSetString("bst.bluestacks_cloud_url", InstallerProperties.CloudHost);
				BridgeApis.ConfSetString("bst.bluestacks_cloud_url2", string.Equals(InstallerProperties.CloudHost, "https://cloud.bluestacks.com") ? "https://delegate.bluestacks.com" : "https://cloud.bluestacks.com");
				BridgeApis.ConfSetString("bst.bluestacks_cdn_url", "https://cdn3.bluestacks.com");
				BridgeApis.ConfSetString("bst.bluestacks_eb_url", "https://eb.bluestacks.com");
				BridgeApis.ConfSetString("bst.dns_server", text4);
				BridgeApis.ConfSetString("bst.dns_server2", text5);
				bool flag2 = !string.IsNullOrEmpty(InstallerProperties.CampaignName);
				if (flag2)
				{
					BridgeApis.ConfSetString("bst.campaign_name", InstallerProperties.CampaignName);
				}
				bool flag3 = !string.IsNullOrEmpty(InstallerProperties.CampaignMD5);
				if (flag3)
				{
					BridgeApis.ConfSetString("bst.campaign_hash", InstallerProperties.CampaignMD5);
				}
				BridgeApis.ConfSetString("bst.install_id", InstallerProperties.InstallId);
				BridgeApis.ConfSetString("bst.version_machine_id", GuidUtils.BlueStacksVersionId);
				BridgeApis.ConfSetBool("bst.status.sse4_available", InstallerProperties.IsSSE4Available == 1);
				bool flag4 = InstallerProperties.AppToLaunch == "bsx";
				if (flag4)
				{
					BridgeApis.ConfSetBool("bst.enable_bsx_app_shortcuts", true);
				}
				BridgeApis.ConfSetString("bst.host_user_agent", string.Concat(new string[]
				{
					"Windows NT ",
					Environment.OSVersion.Version.Major.ToString(),
					".",
					Environment.OSVersion.Version.Minor.ToString(),
					SystemUtils.IsOs64Bit() ? "; Win64; x64" : ""
				}));
				bool isFreshInstall = InstallerProperties.IsFreshInstall;
				if (isFreshInstall)
				{
					BridgeApis.ConfSetString("bst.status.hypervisor", InstallerProperties.CurrentHypervisor);
					BridgeApis.ConfSetString("bst.guid", InstallerProperties.GUID);
					BridgeApis.ConfSetString("bst.machine_id", GuidUtils.BlueStacksMachineId);
					BridgeApis.ConfSetString("bst.locale", Customizations.Instance.Locale);
					BridgeApis.ConfSetString("bst.country", InstallerProperties.Country);
					BridgeApis.ConfSetString("bst.install_date", DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
					BridgeApis.ConfSetBool("bst.prefer_dedicated_gpu", InstallerProperties.PreferDedicatedGPU);
					BridgeApis.ConfSetMultiString("bst.installed_images", new List<string>
					{
						InstallerProperties.VmToLaunch
					});
					BridgeApis.ConfSetBool("bst.status.ssse3_available", value4);
					BridgeApis.ConfSetBool("bst.enable_sigin_gamelaunch", Customizations.Instance.EnableGoogleSignInOnGameLaunch);
					ConfManager.SetRamAndCpus();
					BridgeApis.ConfSetString("bst.qt_renderer", SystemUtils.IsOSWin7() ? "Software" : "Auto");
					ConfManager.SetHighPerformanceMode();
					int value5;
					int value6;
					Utils.GetDefaultGuestResolution(out value5, out value6);
					BridgeApis.ConfSetI32("bst.instance." + InstallerProperties.VmToLaunch + ".fb_width", value5);
					BridgeApis.ConfSetI32("bst.instance." + InstallerProperties.VmToLaunch + ".fb_height", value6);
					BridgeApis.ConfSetString("bst.instance." + InstallerProperties.VmToLaunch + ".device_carrier_code", value2);
					BridgeApis.ConfSetString("bst.instance." + InstallerProperties.VmToLaunch + ".device_country_code", value);
					BridgeApis.ConfSetString("bst.instance." + InstallerProperties.VmToLaunch + ".device_profile_code", string.IsNullOrEmpty(App.sOpt.deviceProfile) ? text : App.sOpt.deviceProfile);
					BridgeApis.ConfSetString("bst.instance." + InstallerProperties.VmToLaunch + ".graphics_engine", string.IsNullOrEmpty(App.sOpt.graphicEngine) ? ((InstallerProperties.GLMode == 1) ? "pga" : "aga") : App.sOpt.graphicEngine);
					BridgeApis.ConfSetString("bst.instance." + InstallerProperties.VmToLaunch + ".graphics_renderer", string.IsNullOrEmpty(App.sOpt.graphicRenderer) ? ((InstallerProperties.GLRenderMode == 1) ? "gl" : "dx") : App.sOpt.graphicRenderer);
					BridgeApis.ConfSetString("bst.instance." + InstallerProperties.VmToLaunch + ".libc_mem_allocator", value3);
					BridgeApis.ConfSetString("bst.instance." + InstallerProperties.VmToLaunch + ".android_id", ConfManager.GenerateAndroidId());
					bool flag5 = string.IsNullOrEmpty(BridgeApis.ConfGetString("bst.instance." + InstallerProperties.VmToLaunch + ".libc_mem_allocator"));
					if (flag5)
					{
						BridgeApis.ConfSetString("bst.instance." + InstallerProperties.VmToLaunch + ".libc_mem_allocator", "jem");
					}
					bool addAppPlayerToDisplayName = Customizations.Instance.AddAppPlayerToDisplayName;
					if (addAppPlayerToDisplayName)
					{
						BridgeApis.ConfSetString("bst.instance." + InstallerProperties.VmToLaunch + ".display_name", "MSI App Player Engine App Player");
					}
					bool flag6 = App.sOpt.skipShortcuts || !Customizations.Instance.EnableAppDesktopShortcuts;
					if (flag6)
					{
						BridgeApis.ConfSetBool("bst.create_desktop_shortcuts", false);
					}
					bool enableQuickLaunch = Customizations.Instance.EnableQuickLaunch;
					if (enableQuickLaunch)
					{
						BridgeApis.ConfSetBool("bst.feature.quicklaunch", true);
					}
					bool flag7 = !Customizations.Instance.BlueStacksXSupported;
					if (flag7)
					{
						BridgeApis.ConfSetBool("bst.feature.bluestacksX", false);
					}
					bool flag8 = !Customizations.Instance.EnableDiscordIntegration;
					if (flag8)
					{
						BridgeApis.ConfSetBool("bst.enable_discord_integration", false);
					}
					bool flag9 = !Customizations.Instance.EnableDesktopNotifications;
					if (flag9)
					{
						BridgeApis.ConfSetBool("bst.instance." + InstallerProperties.VmToLaunch + ".enable_notifications", false);
					}
					bool flag10 = !string.IsNullOrEmpty(text6);
					if (flag10)
					{
						int num2;
						bool flag11 = int.TryParse(text6, out num2);
						if (flag11)
						{
							AbiValues abiValues = (AbiValues)num2;
							bool flag12 = !ImageTraits.Get(InstallerProperties.VmToLaunch).Is64Bit;
							if (flag12)
							{
								abiValues &= ~AbiValues.arm64;
								abiValues &= ~AbiValues.x64;
							}
							List<string> value7 = new List<string>(from _ in abiValues.ToString().Split(new char[]
							{
								','
							}, StringSplitOptions.RemoveEmptyEntries)
							select _.Trim());
							BridgeApis.ConfSetMultiString("bst.instance." + InstallerProperties.VmToLaunch + ".abi_list", value7);
						}
					}
				}
				else
				{
					System.Version v = new System.Version(InstallerProperties.OldVersion);
					System.Version v2 = new System.Version("5.1.100");
					bool flag13 = v < v2;
					if (flag13)
					{
						foreach (string str in InstallerProperties.VmList)
						{
							string a = BridgeApis.ConfGetString("bst.instance." + str + ".astc_decoding_mode");
							bool flag14 = string.Equals(a, "disabled", StringComparison.InvariantCultureIgnoreCase);
							if (flag14)
							{
								BridgeApis.ConfSetString("bst.instance." + str + ".astc_decoding_mode", "software");
							}
						}
						ConfManager.SetHighPerformanceMode();
					}
					System.Version v3 = new System.Version("5.6.0.0");
					bool flag15 = v < v3;
					if (flag15)
					{
						BridgeApis.ConfSetBool("bst.custom_cursor_enabled", true);
					}
					System.Version v4 = new System.Version("5.6.0.0");
					bool flag16 = v < v4;
					if (flag16)
					{
						BridgeApis.ConfSetBool("bst.enable_sigin_gamelaunch", Customizations.Instance.EnableGoogleSignInOnGameLaunch);
					}
					System.Version v5 = new System.Version("5.9.0.0");
					bool flag17 = v < v5 && !Customizations.Instance.EnableDesktopNotifications;
					if (flag17)
					{
						foreach (string str2 in InstallerProperties.VmList)
						{
							BridgeApis.ConfSetBool("bst.instance." + str2 + ".enable_notifications", Customizations.Instance.EnableDesktopNotifications);
						}
					}
					ConfManager.SetCustomResolutionValue();
					bool flag18 = string.IsNullOrEmpty(BridgeApis.ConfGetString("bst.install_date"));
					if (flag18)
					{
						BridgeApis.ConfSetString("bst.install_date", DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
					}
					foreach (string str3 in InstallerProperties.VmList)
					{
						BridgeApis.ConfSetBool("bst.instance." + str3 + ".first_boot", true);
					}
				}
				bool showCloudInstance = Customizations.Instance.ShowCloudInstance;
				if (showCloudInstance)
				{
					BridgeApis.ConfSetBool("bst.feature.show_cloud_instance", true);
				}
				ConfManager.GetInCompatibleCpuInfoFromCloud();
				ConfManager.GetConfFromCloud();
				Logger.Info("Bluestacks.conf writing to path: {0}", new object[]
				{
					confPath
				});
				num = BridgeApis.ConfCommitToFile(confPath);
				bool flag19 = InstallerProperties.IsFreshInstall || num != 0;
				if (flag19)
				{
					return num;
				}
				bool isUpgrade = InstallerProperties.IsUpgrade;
				if (isUpgrade)
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					string text7 = Path.Combine(InstallerProperties.PDDir, "bluestacks.conf");
					bool flag20 = !File.Exists(text7);
					if (flag20)
					{
						Logger.Warning("Conf file does not exist at {0}", new object[]
						{
							text7
						});
					}
					Logger.Info("Reading {0}", new object[]
					{
						Path.GetFileName(text7)
					});
					string[] array = File.ReadAllLines(text7);
					foreach (string text8 in array)
					{
						bool flag21 = text8.Contains("bst.shortcut.");
						if (flag21)
						{
							dictionary.Add(text8.Split(new char[]
							{
								'='
							})[0], text8.Split(new char[]
							{
								'='
							})[1]);
						}
					}
					bool flag22 = !File.Exists(confPath);
					if (flag22)
					{
						Logger.Warning("Conf file does not exist at {0}", new object[]
						{
							confPath
						});
					}
					Logger.Info("Reading {0}", new object[]
					{
						Path.GetFileName(confPath)
					});
					array = File.ReadAllLines(confPath);
					bool flag23 = false;
					foreach (string text9 in array)
					{
						bool flag24 = text9.Contains("bst.shortcut.");
						if (flag24)
						{
							bool flag25 = !dictionary.ContainsKey(text9.Split(new char[]
							{
								'='
							})[0]);
							if (flag25)
							{
								bool flag26 = dictionary.Values.Contains(text9.Split(new char[]
								{
									'='
								})[1]);
								if (flag26)
								{
									BridgeApis.ConfSetString(text9.Split(new char[]
									{
										'='
									})[0], "");
									flag23 = true;
								}
							}
						}
					}
					bool flag27 = flag23;
					if (flag27)
					{
						return BridgeApis.ConfCommitToFile(Path.Combine(InstallerProperties.PDDirNew, "bluestacks.conf"));
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Some error occured while creating props file " + ex.ToString());
				return -63;
			}
			return num;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00003D64 File Offset: 0x00001F64
		private static void GetConfFromCloud()
		{
			try
			{
				string url = InstallerProperties.CloudHost + "/app_player/get_conf_updates";
				NameValueCollection headerCollection = new NameValueCollection
				{
					{
						"vmname",
						InstallerProperties.VmToLaunch
					},
					{
						"x_android_id",
						BridgeApis.ConfGetString("bst.instance." + InstallerProperties.VmToLaunch + ".android_id")
					},
					{
						"x_android_image",
						InstallerProperties.VmToLaunch
					},
					{
						"x_email",
						BridgeApis.ConfGetString("bst.bluestacks_account_id")
					},
					{
						"x_google_aid",
						BridgeApis.ConfGetString("bst.instance." + InstallerProperties.VmToLaunch + ".android_google_ad_id")
					},
					{
						"x_machine_id",
						GuidUtils.BlueStacksMachineId
					},
					{
						"x_oem",
						"msi5"
					},
					{
						"x_session_id",
						BridgeApis.ConfGetI64("bst.instance." + InstallerProperties.VmToLaunch + ".status.session_id").ToString()
					},
					{
						"x_version_machine_id",
						GuidUtils.BlueStacksVersionId
					}
				};
				Dictionary<string, string> data = new Dictionary<string, string>
				{
					{
						"android_id",
						BridgeApis.ConfGetString("bst.instance." + InstallerProperties.VmToLaunch + ".android_id")
					},
					{
						"android_image",
						InstallerProperties.VmToLaunch
					},
					{
						"bluestacks_account_id",
						BridgeApis.ConfGetString("bst.bluestacks_account_id")
					},
					{
						"campaign_hash",
						BridgeApis.ConfGetString("bst.campaign_hash")
					},
					{
						"country",
						BridgeApis.ConfGetString("bst.country")
					},
					{
						"guid",
						BridgeApis.ConfGetString("bst.guid")
					},
					{
						"hypervisor",
						BridgeApis.ConfGetString("bst.status.hypervisor")
					},
					{
						"install_id",
						BridgeApis.ConfGetString("bst.install_id")
					},
					{
						"instance",
						InstallerProperties.VmToLaunch
					},
					{
						"locale",
						BridgeApis.ConfGetString("bst.locale")
					},
					{
						"machine_id",
						BridgeApis.ConfGetString("bst.machine_id")
					},
					{
						"oem",
						"msi5"
					},
					{
						"player_version",
						"5.12.120.6303"
					},
					{
						"session_id",
						BridgeApis.ConfGetI64("bst.instance." + InstallerProperties.VmToLaunch + ".status.session_id").ToString()
					},
					{
						"version_machine_id",
						BridgeApis.ConfGetString("bst.version_machine_id")
					}
				};
				Logger.Info("Fetching cloud conf details details");
				string json = HTTP.PostWrapper(url, data, null, false, 3, 1000, 5000, headerCollection, "");
				JObject jobject = JObject.Parse(json);
				JObject jobject2 = jobject["properties"] as JObject;
				foreach (KeyValuePair<string, JToken> keyValuePair in jobject2)
				{
					BridgeApis.ConfSetStringUnsafe(keyValuePair.Key.ToString(), keyValuePair.Value.ToString());
				}
				Logger.Info("conf successfully updated form cloud");
			}
			catch (Exception ex)
			{
				Logger.Error("Couldn't update conf from cloud. Ex: {0}", new object[]
				{
					ex
				});
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000040BC File Offset: 0x000022BC
		private static void SetRamAndCpus()
		{
			Logger.Info("Attempting to set RAM and CPU");
			bool flag = false;
			int num = -1;
			int num2 = -1;
			int.TryParse(App.sOpt.ram, out num);
			int.TryParse(App.sOpt.cpu, out num2);
			try
			{
				ulong num3 = 1048576UL;
				double num4 = Profile.TotalPhysicalMemory / num3;
				bool flag2 = num4 >= 7782.4 && Profile.NumberOfLogicalProcessors >= 8;
				if (flag2)
				{
					Logger.Info("Settings high memory/cpu configuration");
					num = ((num == -1) ? 4096 : Math.Min(num, 4096));
					num2 = ((num2 == -1) ? 4 : Math.Min(num2, 4));
					BridgeApis.ConfSetI32("bst.instance." + InstallerProperties.VmToLaunch + ".ram", num);
					BridgeApis.ConfSetI32("bst.instance." + InstallerProperties.VmToLaunch + ".cpus", num2);
					flag = true;
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Could not set machine configuration for high-end. Will use standard formula. Ex: {0}", new object[]
				{
					ex
				});
			}
			bool flag3 = !flag;
			if (flag3)
			{
				int androidVMMemory = Utils.GetAndroidVMMemory(true);
				int recommendedVCPUCount = Utils.GetRecommendedVCPUCount(true);
				num = ((num == -1) ? androidVMMemory : Math.Min(num, 4096));
				num2 = ((num2 == -1) ? recommendedVCPUCount : Math.Min(num2, 4));
				BridgeApis.ConfSetI32("bst.instance." + InstallerProperties.VmToLaunch + ".ram", num);
				BridgeApis.ConfSetI32("bst.instance." + InstallerProperties.VmToLaunch + ".cpus", num2);
			}
			Logger.Info("Setting RAM: {0}, CPU: {1}", new object[]
			{
				num,
				num2
			});
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00004268 File Offset: 0x00002468
		private static void SetHighPerformanceMode()
		{
			int num = int.Parse(Profile.Ram);
			bool flag = num > 4096;
			if (flag)
			{
				BridgeApis.ConfSetBool("bst.mem_swap_enabled", false);
				BridgeApis.ConfSetBool("bst.mem_pcd_enabled", false);
				BridgeApis.ConfSetBool("bst.mem_pcr_enabled", false);
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000042B4 File Offset: 0x000024B4
		private static void SetCustomResolutionValue()
		{
			List<string> source = new List<string>
			{
				"1280 x 720",
				"960 x 540",
				"1920 x 1080",
				"1600 x 900",
				"2560 x 1440",
				"3840 x 2160",
				"720 x 1280",
				"540 x 960",
				"1080 x 1920",
				"900 x 1600",
				"1440 x 2560",
				"2160 x 3840",
				"1680 x 720",
				"2560 x 1080",
				"3440 x 1440",
				"5120 x 2160"
			};
			List<string> list = new List<string>();
			foreach (string text in InstallerProperties.VmList)
			{
				string resValue = ConfReader.Guest(text).FbWidth.ToString() + " x " + ConfReader.Guest(text).FbHeight.ToString();
				string text2 = source.FirstOrDefault((string x) => x.Contains(resValue));
				bool flag = text2 == null;
				if (flag)
				{
					BridgeApis.ConfSetBool("bst.instance." + text + ".custom_resolution_selected", true);
					bool flag2 = !list.Contains(resValue);
					if (flag2)
					{
						list.Add(resValue);
					}
				}
			}
			bool flag3 = list.Count<string>() > 0;
			if (flag3)
			{
				BridgeApis.ConfSetMultiString("bst.custom_resolutions", list);
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00004490 File Offset: 0x00002690
		private static void GetInCompatibleCpuInfoFromCloud()
		{
			try
			{
				Logger.Info("Fetching cpu info list for showing virtualization popup");
				string url = InstallerProperties.CloudHost + "/app_player/get_rawmode_incompatible_cpu_list";
				Dictionary<string, string> data = new Dictionary<string, string>
				{
					{
						"android_image",
						InstallerProperties.VmToLaunch
					},
					{
						"guid",
						BridgeApis.ConfGetString("bst.guid")
					},
					{
						"install_id",
						BridgeApis.ConfGetString("bst.install_id")
					},
					{
						"locale",
						BridgeApis.ConfGetString("bst.locale")
					},
					{
						"machine_id",
						BridgeApis.ConfGetString("bst.machine_id")
					},
					{
						"oem",
						"msi5"
					},
					{
						"player_version",
						"5.12.120.6303"
					},
					{
						"version_machine_id",
						BridgeApis.ConfGetString("bst.version_machine_id")
					}
				};
				JObject jobject = JObject.Parse(HTTP.PostWrapper(url, data, null, false, 1, 0, 5000, InstallerStats.GetEarlyHeaders(InstallerProperties.DefaultVmName), ""));
				bool value = false;
				string text = "";
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0\\"))
				{
					bool flag = registryKey != null;
					if (flag)
					{
						text = ((string)registryKey.GetValue("ProcessorNameString", "")).Trim();
					}
				}
				Logger.Info("CPU version: {0}", new object[]
				{
					text
				});
				foreach (JToken jtoken in ((IEnumerable<JToken>)jobject["cpu_list"]))
				{
					string text2 = jtoken.ToString().Trim();
					bool flag2 = text2.EndsWith("*", StringComparison.InvariantCulture);
					if (flag2)
					{
						bool flag3 = text.StartsWith(text2.Substring(0, text2.Length - 2), StringComparison.InvariantCultureIgnoreCase);
						if (flag3)
						{
							value = true;
							Logger.Info("Incompatible cpu matched: cpu: {0}, rule: {1}", new object[]
							{
								text,
								text2
							});
							break;
						}
					}
					else
					{
						bool flag4 = text.Equals(text2, StringComparison.InvariantCultureIgnoreCase);
						if (flag4)
						{
							value = true;
							Logger.Info("Incompatible cpu matched: cpu: {0}, rule: {1}", new object[]
							{
								text,
								text2
							});
							break;
						}
					}
				}
				BridgeApis.ConfSetBool("bst.raw_incompatible_cpu", value);
			}
			catch (Exception ex)
			{
				Logger.Error("Error in fetching incompatible cpu list from cloud, Error: {0}", new object[]
				{
					ex
				});
			}
		}
	}
}
