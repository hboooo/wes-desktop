using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections;
using System.Configuration;
using Wes.Wrapper;
using Wes.Utilities;
using Wes.Desktop.Windows.ViewModel;
using Wes.Utilities.Xml;

namespace Wes.Desktop.Windows.Controls
{
    /// <summary>
    /// ChatControl.xaml 的交互逻辑
    /// </summary>
    public partial class ChatControl : UserControl
    {

        public ObservableCollection<ChatMessage> MessageItems
        {
            get { return (ObservableCollection<ChatMessage>)GetValue(MessageItemsProperty); }
            set { SetValue(MessageItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MessageItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageItemsProperty =
            DependencyProperty.Register("MessageItems", typeof(ObservableCollection<ChatMessage>), typeof(ChatControl), new PropertyMetadata(null));


        public Visibility TooltipVisibility
        {
            get { return (Visibility)GetValue(TooltipVisibilityProperty); }
            set { SetValue(TooltipVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TootipVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TooltipVisibilityProperty =
            DependencyProperty.Register("TooltipVisibility", typeof(Visibility), typeof(ChatControl), new PropertyMetadata(Visibility.Collapsed));


        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(ChatControl), new PropertyMetadata(null));


        private BitmapImage _robotHeader = null;
        private BitmapImage _userHeader = null;
        private Brush _left = new SolidColorBrush(Color.FromRgb(152, 225, 101));
        private Brush _right = new SolidColorBrush(Color.FromRgb(245, 245, 245));

        public static readonly String _appKey = ConfigurationManager.AppSettings["RobotAppKey"];

        private static readonly string _cache = System.IO.Path.Combine(AppPath.DataPath, "chat.db");
        private int _maxCahceItem = 1000; //緩存記錄個數

        public ChatControl()
        {
            InitializeComponent();
            this.Loaded += ChatControl_Loaded;
            Init();
            tbMessage.Focus();
        }

        private void ChatControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= ChatControl_Loaded;
        }

        private void Init()
        {
            _robotHeader = new BitmapImage(new Uri("pack://application:,,,/Resources/robot.png", UriKind.RelativeOrAbsolute));
            _userHeader = new BitmapImage(new Uri("pack://application:,,,/Resources/user.png", UriKind.RelativeOrAbsolute));

            if (MessageItems == null)
            {
                MessageItems = new ObservableCollection<ChatMessage>();
            }

            List<XmlDataViewModel> msgItems = EnvironmetService.GetEntityValues("chat", _cache);
            if (msgItems == null || msgItems.Count == 0)
            {
                Hello();
            }
            else
            {
                foreach (var item in msgItems)
                {
                    AddMessageItem(item.Value, (FlowDirection)(Convert.ToInt32(item.FlowID)), 1);
                }
            }
        }

        public void Hello()
        {
            AddMessageItem("很高兴为您服务，我上知天文下知地理", FlowDirection.LeftToRight);
        }

        private void AddMessageItem(string message, FlowDirection direction, int type = 0)
        {
            ChatMessage chatMessage = new ChatMessage();
            chatMessage.FlowDir = direction;
            if (direction == FlowDirection.LeftToRight)
            {
                chatMessage.HeadImage = _robotHeader;
                chatMessage.FlowDir = FlowDirection.LeftToRight;
                chatMessage.BackColor = _left;
            }
            else if (direction == FlowDirection.RightToLeft)
            {
                chatMessage.HeadImage = _userHeader;
                chatMessage.FlowDir = FlowDirection.RightToLeft;
                chatMessage.BackColor = _right;
            }
            chatMessage.Message = message;
            MessageItems.Add(chatMessage);

            if (type == 0)
                SaveChatData(message, direction);
        }

        private void SaveChatData(string message, FlowDirection direction)
        {
            try
            {
                List<string> keyIns = EnvironmetService.GetValues("chat", _cache);
                if (keyIns != null && keyIns.Count >= _maxCahceItem)
                {
                    EnvironmetService.RemoveFirstValue("chat", "items", _cache);
                }

                XmlDataViewModel xmlDataViewModel = new XmlDataViewModel();
                xmlDataViewModel.FlowID = ((int)direction).ToString();
                xmlDataViewModel.Value = message;
                EnvironmetService.AppendValue("chat", "items", xmlDataViewModel, _cache);
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        public void Clean()
        {
            if (MessageItems != null)
            {
                MessageItems.Clear();
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Message) || string.IsNullOrEmpty(Message.Trim()))
            {
                TooltipVisibility = Visibility.Visible;
                return;
            }
            TooltipVisibility = Visibility.Collapsed;
            AddMessageItem(Message, FlowDirection.RightToLeft);
            PostChat(Message);
            Message = "";
        }

        private void PostChat(string message)
        {
            var postData = new
            {
                perception = new
                {
                    inputText = new
                    {
                        text = message
                    }
                },
                userInfo = new
                {
                    apiKey = _appKey,
                    userId = WesDesktop.Instance.User.Code,
                }
            };

            RestApi.NewInstance(Method.POST)
            .SetUrl("http://openapi.tuling123.com/openapi/api/v2")
            .AddJsonBody(postData)
            .ExecuteAsync((res, ex, restApi) =>
            {
                if (restApi != null)
                {
                    dynamic resData = restApi.To<object>();
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (resData != null && resData.results != null)
                            {
                                foreach (var item in resData.results)
                                {
                                    if (item.resultType == "text")
                                    {
                                        AddMessageItem(item.values.text.ToString(), FlowDirection.LeftToRight);
                                    }
                                }
                            }
                            else
                            {
                                LoggingService.Debug(DynamicJson.SerializeObject(resData));
                            }
                        }
                        catch { }
                    }));
                }
            });
        }

        private void btnClean_Click(object sender, RoutedEventArgs e)
        {
            EnvironmetService.DeleteElement("chat", _cache);
            this.MessageItems?.Clear();
        }
    }
}
