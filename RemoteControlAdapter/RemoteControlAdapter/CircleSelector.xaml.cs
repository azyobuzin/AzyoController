using RemoteControlAdapter.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RemoteControlAdapter
{
    /// <summary>
    /// 青リングのUIユーザーコントロール
    /// </summary>
    public partial class CircleSelector : UserControl
    {
        public event Action<object,ControlType> OnCenterButtonPressed;
        CircleSelectorViewModel _viewModel;

        private bool isSuggest;

        public bool IsSuggest
        {
            get
            { 
                return isSuggest;
            }
            set
            {
                isSuggest = value;
                this.Dispatcher.InvokeAsync(() =>
                {
                    if (value)
                    {
                        VisualStateManager.GoToState(this, "SuggestMode", true);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, "NormalMode", true);
                    }
                });
            }
        }

        public CircleSelector()
        {
            InitializeComponent();
            _viewModel = new CircleSelectorViewModel();
            isSuggest = false;
            DataContext = _viewModel;

            OnCenterButtonPressed += (s,e) =>
            {
            };
            VisualStateManager.GoToState(this, "NormalMode", true);
        }

        


        public double GetPointLnegth(Point p)
        {
            return Math.Sqrt(p.X*p.X+p.Y*p.Y);
        }

        

        private void gridSelector_TouchMove(object sender, TouchEventArgs e)
        {
            InputProcess(e.GetTouchPoint(gridRoot).Position);
            
        }

        private void InputProcess(Point p)
        {
            
            Point rootPoint = new Point(gridRoot.ActualWidth / 2, gridRoot.ActualHeight / 2);

            Point touchPoint = new Point(p.X - rootPoint.X, p.Y - rootPoint.Y);

            Point firstPoint = new Point(gridRoot.ActualWidth / 2 - rootPoint.X, 0 - rootPoint.Y);

            double naiseki = firstPoint.X * touchPoint.X + firstPoint.Y * touchPoint.Y;
            double firstLength = GetPointLnegth(firstPoint);
            double touchLength = GetPointLnegth(touchPoint);
            double deg = Math.Acos((naiseki / (firstLength * touchLength))) * 180 / Math.PI;
            if (p.X < rootPoint.X)
            {
                deg = 360 - deg;
            }
           
            _viewModel.PointDegree = deg;
            if (deg >= 0 && deg < 40)
            {
                _viewModel.CenterBrush = Resources["Brush1"] as SolidColorBrush;
                _viewModel.CenterText = "P";
                _viewModel.State = (int)ControlType.Power;
            }
            else if (deg >= 40 && deg < 80)
            {
                _viewModel.CenterBrush = Resources["Brush2"] as SolidColorBrush;
                _viewModel.CenterText = "↑";
                _viewModel.State = (int)ControlType.VolueUp;
            }
            else if (deg >= 80 && deg < 120)
            {
                _viewModel.CenterBrush = Resources["Brush3"] as SolidColorBrush;
                _viewModel.CenterText = "↓";
                _viewModel.State = (int)ControlType.VolumeDown;
            }
            else if (deg >= 120 && deg < 160)
            {
                _viewModel.CenterBrush = Resources["Brush4"] as SolidColorBrush;
                _viewModel.CenterText = "Ch.1";
                _viewModel.State = (int)ControlType.Chanel1;
            }
            else if (deg >= 160 && deg < 200)
            {
                _viewModel.CenterBrush = Resources["Brush5"] as SolidColorBrush;
                _viewModel.CenterText = "Ch.4";
                _viewModel.State = (int)ControlType.Chanel4;
            }
            else if (deg >= 200 && deg < 240)
            {
                _viewModel.CenterBrush = Resources["Brush6"] as SolidColorBrush;
                _viewModel.CenterText = "Ch.5";
                _viewModel.State = (int)ControlType.Chanel5;
            }
            else if (deg >= 240 && deg < 280)
            {
                _viewModel.CenterBrush = Resources["Brush7"] as SolidColorBrush;
                _viewModel.CenterText = "Ch.6";
                _viewModel.State = (int)ControlType.Chanel6;
            }
            else if (deg >= 280 && deg < 320)
            {
                _viewModel.CenterBrush = Resources["Brush8"] as SolidColorBrush;
                _viewModel.CenterText = "Ch.8";
                _viewModel.State = (int)ControlType.Chanel8;
            }
            else if (deg >= 320 && deg < 360)
            {
                _viewModel.CenterBrush = Resources["Brush9"] as SolidColorBrush;
                _viewModel.CenterText = "Ch.10";
                _viewModel.State = (int)ControlType.Chanel10;
            }
            else
                Debug.WriteLine(deg);
        }

        private void gridSelector_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton==MouseButtonState.Pressed)
            InputProcess(e.GetPosition(gridRoot));
            
        }

        

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var story = Resources["archStoryboard"] as Storyboard;
            story.Begin();
        }

        private void TextBlock_TouchUp(object sender, TouchEventArgs e)
        {
            OnCenterButtonPressed(this,(ControlType)_viewModel.State);
        }

        private void btnCenter_TouchUp(object sender, TouchEventArgs e)
        {
            OnCenterButtonPressed(this, (ControlType)_viewModel.State);
        }

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OnCenterButtonPressed(this, (ControlType)_viewModel.State);
        }

        private void btnCenter_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OnCenterButtonPressed(this, (ControlType)_viewModel.State);
        }

        public void PlaySuggestAnimation()
        {
            var story = Resources["suggestStoryboard"] as Storyboard;
            story.Begin();
        }
    }

    public class CircleSelectorViewModel:INotifyPropertyChanged
    {
        private int _state;

        public int State
        {
            get { return _state; }
            set { _state = value; ModelPropertyChanged("State"); }
        }
        private double _pointDegree;

        public double PointDegree
        {
            get { return _pointDegree; }
            set { _pointDegree = value; ModelPropertyChanged("PointDegree"); }
        }

        private SolidColorBrush _CenterBrush;

        public SolidColorBrush CenterBrush
        {
            get { return _CenterBrush; }
            set { _CenterBrush = value; ModelPropertyChanged("CenterBrush"); }
        }

        private string _centerText;

        public string CenterText
        {
            get { return _centerText; }
            set { _centerText = value; ModelPropertyChanged("CenterText"); }
        }
        public CircleSelectorViewModel()
        {
            _pointDegree = 0;
            CenterBrush = new SolidColorBrush(Colors.DeepSkyBlue);
            CenterText = "";
            State = 0;
        }

        


        public event PropertyChangedEventHandler PropertyChanged;

        public void ModelPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}