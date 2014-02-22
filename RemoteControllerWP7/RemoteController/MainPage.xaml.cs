using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using RemoteControlAdapter.Model;
using Newtonsoft.Json;

namespace RemoteController
{
    public partial class MainPage : PhoneApplicationPage
    {
        MainViewModel _viewModel;
        // コンストラクター
        public MainPage()
        {
            InitializeComponent();

            // ListBox コントロールのデータ コンテキストをサンプル データに設定します
            _viewModel = new MainViewModel();
            
            
            DataContext = _viewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            this.circleSelector.OnCenterButtonPressed += (s, e) =>
            {
                RemoteData d = new RemoteData()
                {
                    ControlData = (int)e,
                    Name = "garicchi"
                };
                _viewModel.SendData(d);
            };
            
        }

        // ViewModel Items のデータを読み込みます
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Connect(textIp.Text,5000);
            _viewModel.Client.OnConnected += () =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Connected!");
                    _viewModel.ConnectStatus = "Connected!";
                });
            };

            
        }

        private void btnCommand_Click(object sender, RoutedEventArgs e)
        {
           /* _viewModel.BeginReceiveData();

            _viewModel.Client.ReceiveCompleted += (str) =>
            {
                var data = JsonConvert.DeserializeObject<MobileRemoteData>(str);
                switch (data.ControlType)
                {
                    case MobileControlType.Call:
                        MessageBox.Show("Calling Now");
                        break;
                    case MobileControlType.Suggest:
                        break;
                }

                _viewModel.Client.ReceiveTextAsync();
            };*/
            
             
        }
    }
}