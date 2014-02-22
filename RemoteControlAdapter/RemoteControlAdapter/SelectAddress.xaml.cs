using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RemoteControlAdapter
{
    /// <summary>
    /// SelectAddress.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectAddress : Window
    {
        public event Action<string> OnSelectAddress;
        public SelectAddress(List<string> ipList)
        {
            InitializeComponent();

            this.ipList.ItemsSource = ipList;
            OnSelectAddress += (e) => { };
        }

        private void ipList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ipList.SelectedIndex != -1)
            {
                OnSelectAddress(ipList.SelectedItem.ToString());
                Close();
            }
        }
    }
}
