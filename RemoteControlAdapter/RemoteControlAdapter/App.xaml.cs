﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Livet;
using RemoteControlAdapter.Model;

namespace RemoteControlAdapter
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = this.Dispatcher;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.Instance.Save();
        }
    }
}
