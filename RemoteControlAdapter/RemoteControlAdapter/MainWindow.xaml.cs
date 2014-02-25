using RemoteControlAdapter.Model;
using RemoteControlAdapter.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Speech.Synthesis;
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

namespace RemoteControlAdapter
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel _viewModel;
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
                        _viewModel.PowerCommand.Execute(null);
                        break;
                    case ControlType.VolueUp:
                        _viewModel.VolumeUpCommand.Execute(null);
                        break;
                    case ControlType.VolumeDown:
                        _viewModel.VolumeDownCommand.Execute(null);
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

            _viewModel.OnSuggest += () =>
            {
                SpeechSynthesizer ss = new SpeechSynthesizer();
                var info = ss.GetInstalledVoices().Where(q => q.VoiceInfo.Culture.LCID == 1033).Select(q => q).Single();
                ss.SelectVoice(info.VoiceInfo.Name);
                ss.Rate = -2;
                ss.SpeakAsync("Privilege Suggest you Television, NHK");
                circleSelector.IsSuggest = true;
            };
            //COMポートリストを再取得
            _viewModel.ResetPortListCommand.Execute(null);
            
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

            
            _viewModel.ResetPortListCommand.Execute(null);
            
        }

        private void btnConnectArduino_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ConnectArduino.Execute(comboPort.SelectedItem.ToString());
        }

        private void btnCall_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CallMobileCommand.Execute(null);
        }

        private void check_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void check_Click(object sender, RoutedEventArgs e)
        {
            if (check.IsChecked.Value == true)
            {
                gridMenu.Visibility = Visibility.Visible;
            }
            else
            {
                gridMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void btnclose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        

        
    }
}
