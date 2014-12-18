﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.Web.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace WirelessHelper
{
    public class UserManager
    {
        public string UserName;
        public string UserPassWord;
        public bool PWDSaved=false;
        TheData thedata;
        public string result;
        public string loginresponse="";
        public CancellationTokenSource cts = new CancellationTokenSource();
        public string Result
        {
            get { return result; }
            set {
                result = value;

            }
        }


      public string init()
        {
            loginresponse = "";
            Result = "";
           thedata = new TheData();
           thedata.GetTheData(this);
           return thedata.TransferData();

        }
      public async void postData()
      {
          init();
          string url = "http://w.dlut.edu.cn/cgi-bin/srun_portal";
          //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
          //request.Method = "POST";
          //request.ContentType = "application/x-www-form-urlencoded";
          Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();

          HttpBufferContent buffer = new HttpBufferContent(thedata.TempData.AsBuffer());
          Windows.Web.Http.HttpResponseMessage response = await httpClient.PostAsync(new Uri(url), buffer).AsTask(cts.Token);
          Result = await response.Content.ReadAsStringAsync().AsTask(cts.Token);

          if (Result.IndexOf("成功") >= 0)
          {
              SendNotification("登录成功");
          }
          else if (Result.IndexOf("失败") >= 0)
          {
              SendNotification("登录失败");
          }
          else
          {
              SendNotification("请检查网络");
          }
      }


       private void SendNotification(string p)
       {
           XmlDocument toastXML = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
           XmlNodeList elements = toastXML.GetElementsByTagName("text");
           foreach (IXmlNode node in elements)
           {
               node.InnerText = p;
           }
           ToastNotification notification = new ToastNotification(toastXML);
           ToastNotificationManager.CreateToastNotifier().Show(notification);
       }
    }

    public class TheData
    {
        public Dictionary<string, string> parameter=new Dictionary<string,string>();
        public byte[] TempData = null;

    public void GetTheData(UserManager UM)
     {
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
    parameter.Add("username", UM.UserName);
    parameter.Add("password", UM.UserPassWord);
    parameter.Add("save_me", "on");
     }
    public string  TransferData()
     {
         bool flag = false;
         string temp = "";
        foreach (KeyValuePair<string,string> Attr in this.parameter)
        {
            if (flag == false)
            {
                temp += Attr.Key +"="+ Attr.Value;
                flag = true;
            }
            else
            {
                temp += "&" + Attr.Key + "=" + Attr.Value;
            }
        }
        TempData = Encoding.UTF8.GetBytes(temp);
        return temp;
     }
    }


}
