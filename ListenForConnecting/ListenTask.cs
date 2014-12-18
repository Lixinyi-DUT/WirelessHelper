using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.Web.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
namespace ListenForConnecting
{
    public sealed class ListenTask:IBackgroundTask
    {
      private string loginresponse = "";
      private CancellationTokenSource cts = new CancellationTokenSource();
      private byte[] TempData { get; set; }
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile!=null && connectionProfile.ProfileName == "DLUT")
            {
                ApplicationDataContainer _appSettings = ApplicationData.Current.LocalSettings;
                if (_appSettings.Values.ContainsKey("username"))
                {
                    string name= _appSettings.Values["username"].ToString();
                    string password = _appSettings.Values["password"].ToString();
                    if (_appSettings.Values.ContainsKey("notification") && (bool)_appSettings.Values["notification"])
                        SendNotification("DLUT已连接，用户名："+connectionProfile.GetNetworkConnectivityLevel().ToString());
                    PostTheData(name, password);

                }
                else
                {
                 SendNotification( "请完善用户资料以方便连接，否则关闭后台");
                }
            }
            deferral.Complete();
        }

        private void PostTheData(string name, string password)
        {
            TempData = getdata(name, password);
            postData(TempData);
        }

        private async void postData(byte[] TempData)
        {
            string url = "http://w.dlut.edu.cn/cgi-bin/srun_portal";
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();
            HttpBufferContent buffer = new HttpBufferContent(TempData.AsBuffer());
            Windows.Web.Http.HttpResponseMessage response = await httpClient.PostAsync(new Uri(url), buffer).AsTask(cts.Token);
            this.loginresponse= await response.Content.ReadAsStringAsync().AsTask(cts.Token);
       }

        private byte[] getdata(string name, string password)
        {
            byte[] data;
            Dictionary<string, string> parameter = new Dictionary<string, string>();
             parameter.Add("action", "login");
    parameter.Add("uid", "1");
    parameter.Add("is_pad", "1");
    parameter.Add("force", "0");
    parameter.Add("type", "1");
    parameter.Add("ac_id", "1");
    parameter.Add("pop", "0");
    parameter.Add("ac_type", "h3c");
    parameter.Add("rad_type", "");
    parameter.Add("geteway_auth", "0");
    parameter.Add("local_auth", "1");
    parameter.Add("is_debug", "1");
    parameter.Add("is_ldap", "0");
    parameter.Add("user_ip", "");
    parameter.Add("mac", "");
    parameter.Add("nas_ip", "");
    parameter.Add("ssid", "");
    parameter.Add("vlan", "");
    parameter.Add("wlanacname", "");
    parameter.Add("wbaredirect", "");
    parameter.Add("page_succeed", "");
    parameter.Add("page_error", "");
    parameter.Add("username", name);
    parameter.Add("password", password);
    parameter.Add("save_me", "on");
    bool flag = false;
    string temp = "";
    foreach (KeyValuePair<string, string> Attr in parameter)
    {
        if (flag == false)
        {
            temp += Attr.Key + "=" + Attr.Value;
            flag = true;
        }
        else
        {
            temp += "&" + Attr.Key + "=" + Attr.Value;
        }
    }
  data = Encoding.UTF8.GetBytes(temp);
  return data;
        }

        private void SendNotification(string p)
        {
            XmlDocument toastXML = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            XmlNodeList elements = toastXML.GetElementsByTagName("text");
            foreach(IXmlNode node in elements)
            {
                node.InnerText = p;
            }
            ToastNotification notification = new ToastNotification(toastXML);
            ToastNotificationManager.CreateToastNotifier().Show(notification);
        }
    }

}
