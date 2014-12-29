using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=391641 上有介绍

namespace WirelessHelper
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public UserManager usermanager=new UserManager();
        private ApplicationDataContainer _appSettings;
        public  XmlDocument toastXML = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
        public bool register = false;
        public bool flag = false;
        public BackgroundTaskRegistration task;
        public MainPage()
        {
            this.InitializeComponent();
            _appSettings = ApplicationData.Current.LocalSettings;
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            writetextbox();
            // TODO: 准备此处显示的页面。
            // TODO: 如果您的应用程序包含多个页面，请确保
            // 通过注册以下事件来处理硬件“后退”按钮:
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed 事件。
            // 如果使用由某些模板提供的 NavigationHelper，
            // 则系统会为您处理该事件。
            foreach (var tasks in BackgroundTaskRegistration.AllTasks)
            {
                if(tasks.Value.Name == "ListenForDLUT")
                {
                    register = true;
                    task = (BackgroundTaskRegistration)tasks.Value;
                    break;
                }

                 
            }
            if(register)
            {
              BackgroundSwitch.IsOn = true;
              NotificationSwitch.IsEnabled = true;
            }
            flag = true;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            if(UserNameBox.Text=="")
            {
                MessageDialog message = new MessageDialog("请输入用户名");
                await message.ShowAsync();
                return;
            }
            if(PassWordBox.Password=="")
            {
                MessageDialog message = new MessageDialog("请输入密码");
                await message.ShowAsync();
                return;
            }

            ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile == null || connectionProfile.ProfileName != "DLUT")
            {
                MessageDialog message = new MessageDialog("请连接到DLUT！");
                await message.ShowAsync();
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-wifi:"));
                return;
            }
            usermanager.UserName = UserNameBox.Text;
            usermanager.UserPassWord = PassWordBox.Password;
            usermanager.postData();
            readtextbox();
            
        }


        private void readtextbox()
        {
            _appSettings.Values["username"] = usermanager.UserName;
            _appSettings.Values["password"] = usermanager.UserPassWord;
            _appSettings.Values["save"] = usermanager.PWDSaved;
            if(NotificationSwitch.IsEnabled)
            _appSettings.Values["notification"] = NotificationSwitch.IsOn;
        }

        private void writetextbox()
        {
            if (_appSettings.Values.ContainsKey("username"))
            {
                UserNameBox.Text = _appSettings.Values["username"].ToString();
                usermanager.UserName = _appSettings.Values["username"].ToString();
            }
            if(_appSettings.Values.ContainsKey("password"))
            {
                usermanager.UserPassWord = _appSettings.Values["password"].ToString();
            }
            if (_appSettings.Values.ContainsKey("notification"))
            {
                NotificationSwitch.IsOn = (bool)_appSettings.Values["notification"];
            }
            if (_appSettings.Values.ContainsKey("save"))
            {
                SavedSwitch.IsOn = (bool)_appSettings.Values["save"];
                if ((bool)_appSettings.Values["save"])
                    usermanager.PWDSaved = true;

                if ((bool)_appSettings.Values["save"]&&_appSettings.Values.ContainsKey("password"))
                    PassWordBox.Password = _appSettings.Values["password"].ToString();
            }
        }

        

        private async void BackgroundSwitch_Toggled(object sender, RoutedEventArgs e)
        {

            if (register == false && BackgroundSwitch.IsOn == true)
            {
                if(!(_appSettings.Values.ContainsKey("username") )&&!_appSettings.Values.ContainsKey("password") &&(UserNameBox.Text==""||PassWordBox.Password=="" ))
                {
                    await new MessageDialog("请保存用户信息").ShowAsync();
                    BackgroundSwitch.IsOn = false;
                    return;
                }
                readtextbox();
                var builder = new BackgroundTaskBuilder();
                builder.Name = "ListenForDLUT";
                builder.TaskEntryPoint = "ListenForConnecting.ListenTask";
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.NetworkStateChange, false));
                builder.AddCondition(new SystemCondition(SystemConditionType.FreeNetworkAvailable));
                var access = await BackgroundExecutionManager.RequestAccessAsync();
                if (access == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity)
                {
                    task = builder.Register();
                }
                else if (access == BackgroundAccessStatus.Denied)
                {
                    await new MessageDialog("您已禁用后台任务或后台任务数量已达最大!").ShowAsync();
                    BackgroundSwitch.IsOn = false;
                    return;
                }
                NotificationSwitch.IsEnabled = true;
                register = true;
            }
            else
            {
                if (register && BackgroundSwitch.IsOn == false)
                {
                    foreach (var task in BackgroundTaskRegistration.AllTasks)
                    {
                        if (task.Value.Name == "ListenForDLUT")
                        {
                            task.Value.Unregister(true);
                            register = false;
                            NotificationSwitch.IsEnabled = false;
                            return;

                        }

                    }
                    await new MessageDialog("后台未开启!").ShowAsync();
                    NotificationSwitch.IsEnabled = false;
                    register = false;
                }
            }
        }

        private void NotificationSwitch_Toggled(object sender, RoutedEventArgs e)
        {
        if(flag)
            readtextbox();
        }

        private void UserNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UserNameBox.Text != "")
            {
                usermanager.UserName = UserNameBox.Text;
                readtextbox();
            }
        }

        private void UserNameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (UserNameBox.Text != "")
            {
                usermanager.UserName = UserNameBox.Text;
                readtextbox();
            }
        }

        private void PassWordBox_LostFocus(object sender, RoutedEventArgs e)
        {
        if(PassWordBox.Password!="")
        {
            usermanager.UserPassWord = PassWordBox.Password;
            readtextbox();
        }
        }

        private void SavedSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if(flag)
            {
                usermanager.PWDSaved = SavedSwitch.IsOn;
                readtextbox();
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile == null || connectionProfile.ProfileName != "DLUT")
            {
                MessageDialog message = new MessageDialog("未连接到DLUT！");
                await message.ShowAsync();
                return;
            }
            usermanager.LoginOut();
        }



    }
}
