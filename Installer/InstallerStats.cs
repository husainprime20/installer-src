using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using BlueStacks.Common;
using Newtonsoft.Json.Linq;

namespace BlueStacks.Installer
{
	// Token: 0x0200000C RID: 12
	public class InstallerStats
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600003B RID: 59 RVA: 0x0000215E File Offset: 0x0000035E
		public static string InstallerStatsUrl
		{
			get
			{
				return string.Format("{0}{1}", InstallerProperties.CloudHost, "/bs3/stats/unified_install_stats");
			}
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000065B8 File Offset: 0x000047B8
		public static Dictionary<string, string> GetCommonData(string eventName)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>
			{
				{
					"event",
					eventName
				},
				{
					"install_id",
					InstallerProperties.InstallId
				},
				{
					"engine_version",
					"5.12.120.6303"
				},
				{
					"client_version",
					"5.12.120.6303"
				},
				{
					"os",
					Profile.Os
				},
				{
					"installer_arch",
					InstallerProperties.InstallerArchitecture
				},
				{
					"guid",
					InstallerProperties.GUID
				},
				{
					"oem",
					"msi5"
				},
				{
					"campaign_hash",
					InstallerProperties.CampaignMD5
				},
				{
					"campaign_name",
					InstallerProperties.CampaignName
				},
				{
					"locale",
					InstallerProperties.CurrentLocale
				},
				{
					"time_since_launch",
					(App.sClock.ElapsedMilliseconds / 1000L).ToString()
				},
				{
					"comment",
					InstallerStats.InstallerComment.ToString()
				},
				{
					"hypervisor",
					InstallerProperties.CurrentHypervisor
				}
			};
			bool isUpgrade = InstallerProperties.IsUpgrade;
			if (isUpgrade)
			{
				dictionary.Add("old_guid", InstallerProperties.OldGUID);
				dictionary.Add("old_product_version", InstallerProperties.OldVersion);
				dictionary.Add("old_install_id", InstallerProperties.OldInstallId);
			}
			return dictionary;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00006718 File Offset: 0x00004918
		internal static void SendInstallFailedEvent(string errorCode)
		{
			InstallerUtils.AttemptToClearSomeSpaceIfShort();
			string installEvent = InstallerProperties.IsUpgrade ? "upgrade_failed" : "install_failed";
			string text = InstallerUtils.ZipLogsAndRegistry(InstallerProperties.OldLogsDir, InstallerProperties.PDDir, true);
			bool flag = text != null && File.Exists(text);
			if (flag)
			{
				InstallerStats.ReportInstallFailedStats(installEvent, errorCode, text, "application/x-7z-compressed");
				File.Delete(text);
			}
			else
			{
				InstallerStats.ReportInstallFailedStats(installEvent, errorCode, InstallerProperties.InstallerLogFilePath, "text/plain");
			}
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00006790 File Offset: 0x00004990
		internal static NameValueCollection GetEarlyHeaders(string imageName)
		{
			return new NameValueCollection
			{
				{
					"x_oem",
					"msi5"
				},
				{
					"x_machine_id",
					GuidUtils.BlueStacksMachineId
				},
				{
					"x_version_machine_id",
					GuidUtils.BlueStacksVersionId
				},
				{
					"x_android_image",
					Strings.ImageName
				},
				{
					"vmname",
					imageName
				}
			};
		}

		// Token: 0x0600003F RID: 63 RVA: 0x000067F8 File Offset: 0x000049F8
		public static void Enqueue(string installEvent, bool isEarlyCall = false, string arg1 = "", string arg2 = "")
		{
			Logger.Info("Enqueue {0}", new object[]
			{
				installEvent
			});
			InstallerStats.sStatsQueue.Enqueue(delegate
			{
				InstallerStats.SendStat(installEvent, isEarlyCall, arg1, arg2);
			});
		}

		// Token: 0x06000040 RID: 64 RVA: 0x0000685C File Offset: 0x00004A5C
		public static string SendStat(string installEvent, bool isEarlyCall = false, string arg1 = "", string arg2 = "")
		{
			string text = "";
			try
			{
				Dictionary<string, string> commonData = InstallerStats.GetCommonData(installEvent);
				bool flag = !string.IsNullOrEmpty(arg1);
				if (flag)
				{
					commonData.Add("arg1", arg1);
				}
				bool flag2 = !string.IsNullOrEmpty(arg2);
				if (flag2)
				{
					commonData.Add("arg2", arg2);
				}
				string defaultVmName = InstallerProperties.DefaultVmName;
				Logger.Info("Sending {0}. IsEarlyCall: {1}", new object[]
				{
					installEvent,
					isEarlyCall
				});
				if (isEarlyCall)
				{
					text = HTTP.PostWrapper(InstallerStats.InstallerStatsUrl, commonData, null, false, 3, 0, 5000, InstallerStats.GetEarlyHeaders(defaultVmName), Utils.GetUserAgent("msi5", InstallerProperties.GUID));
				}
				else
				{
					text = BstHttpClient.Post(InstallerStats.InstallerStatsUrl, commonData, defaultVmName, null, false, 0, 3, 0, false, "msi5", InstallerProperties.GUID);
				}
				Logger.Info("Response for event {0}: {1}", new object[]
				{
					installEvent,
					text
				});
			}
			catch (Exception ex)
			{
				Logger.Warning("Failed to send stats for event: {0}. Ex: {1}", new object[]
				{
					installEvent,
					ex
				});
			}
			return text;
		}

		// Token: 0x06000041 RID: 65 RVA: 0x0000697C File Offset: 0x00004B7C
		public static void ReportInstallFailedStats(string installEvent, string errorCode, string filePath, string contentType)
		{
			Dictionary<string, string> commonData = InstallerStats.GetCommonData(installEvent);
			commonData.Add("failure_reason", errorCode);
			IFormatProvider currentCulture = CultureInfo.CurrentCulture;
			string format = InstallerStats.InstallerStatsUrl + "?{0}";
			object[] array = new object[1];
			array[0] = string.Join("&", (from kvp in commonData
			select string.Format(CultureInfo.CurrentCulture, "{0}={1}", new object[]
			{
				kvp.Key,
				kvp.Value
			})).ToArray<string>());
			string url = string.Format(currentCulture, format, array);
			try
			{
				string str = BstHttpClient.HTTPGaeFileUploader(url, commonData, null, filePath, contentType, false, null, InstallerProperties.GUID, 1, 0);
				Logger.Debug("Response for install failed logs upload : " + str);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to upload install failed logs.");
				Logger.Error(ex.ToString());
			}
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00006A4C File Offset: 0x00004C4C
		public static void SendStartEventAfterShowingMainWindowAsync()
		{
			Thread thread = new Thread(delegate()
			{
				bool isUpgrade = InstallerProperties.IsUpgrade;
				string text;
				if (isUpgrade)
				{
					text = InstallerStats.SendStat("upgrade_launched", true, "", "");
				}
				else
				{
					text = InstallerStats.SendStat("install_launched", true, "", "");
				}
				Logger.Info("The json response for campaign {0} is {1}", new object[]
				{
					InstallerProperties.CampaignMD5,
					text
				});
				try
				{
					JObject jobject = JObject.Parse(text);
					try
					{
						bool flag = jobject.ContainsKey("fle_obj");
						if (flag)
						{
							string campaignName = jobject["fle_obj"]["campaign_name"].ToString();
							InstallerProperties.CampaignName = campaignName;
							InstallerProperties.CampaignJson = jobject["fle_obj"].ToString();
						}
						else
						{
							Logger.Warning("No FLE obj response for campaign MD5");
						}
					}
					catch (Exception ex)
					{
						Logger.Error("Failed to parse response string for campaign name");
						Logger.Error(ex.ToString());
					}
					bool flag2 = jobject.ContainsKey("gl_settings_order");
					if (flag2)
					{
						JArray jarray = jobject["gl_settings_order"].Value<JArray>();
						InstallerProperties.GlCheckOrderList = jarray.ToObject<List<GlProperty>>();
						InstallerProperties.IsGlCheckOrderResponseCame = true;
					}
				}
				catch (Exception ex2)
				{
					Logger.Error("Failed to parse response string for gl order list");
					Logger.Error(ex2.ToString());
				}
			})
			{
				IsBackground = true
			};
			thread.Start();
		}

		// Token: 0x0400002D RID: 45
		public static InstallerComments InstallerComment = InstallerComments.NONE;

		// Token: 0x0400002E RID: 46
		internal static SerialWorkQueue sStatsQueue = new SerialWorkQueue();
	}
}
