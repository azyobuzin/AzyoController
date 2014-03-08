﻿using System.Windows;
using Livet;
using RemoteControlAdapter.Model;
using RemoteControlAdapter.Model.Tweets;

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
            ReceivedUserTweets.LoadFromDatabase();
            TweetReceiver.Initialize();
            ChannelSuggesting.Initialize();
            UsualSuggesting.Initialize();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            UsualSuggesting.EndWatching();
            Settings.Instance.Save();
        }
    }
}
