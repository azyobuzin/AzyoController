using RemoteControlAdapter.Model;
using RemoteControlAdapter.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Livet.Messaging;
using MahApps.Metro.Controls.Dialogs;

namespace RemoteControlAdapter
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        MainViewModel _viewModel;
        MessageListener messageListener;
        public MainWindow()
        {
            InitializeComponent();
            this._viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            //青いリングUIの真ん中のボタンがプッシュされたとき
            //どのコマンドかがControlType型で送られてくる
            circleSelector.OnCenterButtonPressed += (s, e) =>
            {
                switch (e)
                {
                    case ControlType.Power:
                        _viewModel.PowerCommand.Execute();
                        break;
                    case ControlType.VolueUp:
                        _viewModel.VolumeUpCommand.Execute();
                        break;
                    case ControlType.VolumeDown:
                        _viewModel.VolumeDownCommand.Execute();
                        break;

                    case ControlType.Chanel1:
                        _viewModel.ChangeChannelCommand.Execute(ControlType.Chanel1);
                        break;
                    case ControlType.Chanel4:
                        _viewModel.ChangeChannelCommand.Execute(ControlType.Chanel4);
                        break;
                    case ControlType.Chanel5:
                        _viewModel.ChangeChannelCommand.Execute(ControlType.Chanel5);
                        break;
                    case ControlType.Chanel6:
                        _viewModel.ChangeChannelCommand.Execute(ControlType.Chanel6);
                        break;
                    case ControlType.Chanel8:
                        _viewModel.ChangeChannelCommand.Execute(ControlType.Chanel8);
                        break;
                    case ControlType.Chanel10:
                        _viewModel.ChangeChannelCommand.Execute(ControlType.Chanel10);
                        break;
                }
            };
            //COMポートリストを再取得
            _viewModel.ResetPortListCommand.Execute();

            messageListener = new MessageListener(_viewModel.Messenger)
            {
                {
                    "WaitingForGettingTokens",
                    m => (m as ResponsiveInteractionMessage<Task<ProgressDialogController>>).Response =
                        this.ShowProgressAsync("アカウント追加", "認証中...")
                },
                {
                    "InputPin",
                    m =>
                    {
                        var grm = m as GenericResponsiveInteractionMessage<string, Task<string>>;
                        Process.Start(grm.Value);
                        grm.Response = this.ShowInputAsync("アカウント追加", "Twitter にログインし、ブラウザに表示された番号を入力してください。");
                    }
                },
                {
                    "AuthorizationError",
                    m => this.ShowMessageAsync("アカウント追加", "Twitter にログインできませんでした。")
                }
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var address = Dns.GetHostAddresses(Dns.GetHostName());
            SelectAddress window = new SelectAddress(address.Select(q=>q.ToString()).ToList());
            window.Show();
            window.OnSelectAddress += (add) =>
            {
                _viewModel.BeginListenCommand.Execute(add);
            
            };
            
        }

        private void btnPortUpdate_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ResetPortListCommand.Execute();
        }

        private void btnConnectArduino_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ConnectArduino.Execute(comboPort.SelectedItem.ToString());
        }

        private void btnCall_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CallMobileCommand.Execute();
        }

        private void btnUsers_Click(object sender, RoutedEventArgs e)
        {
            flyoutUsers.IsOpen = true;
        }

    }
}
