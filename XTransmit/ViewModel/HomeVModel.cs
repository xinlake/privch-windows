﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using XTransmit.ViewModel.Control;

namespace XTransmit.ViewModel
{
    /**
     * Updated: 2019-10-04
     */
    public class HomeVModel : BaseViewModel
    {
        public bool IsTransmitEnabled
        {
            get => App.GlobalConfig.IsTransmitEnabled;
            set
            {
                App.EnableTransmit(value);
            }
        }

        public string TransmitStatus
        {
            get
            {
                return App.GlobalConfig.RemoteServer != null ?
                    App.GlobalConfig.RemoteServer.FriendlyName : sr_server_not_set;
            }
        }

        public bool IsTransmitControllable { get; private set; }

        // progress
        public ProgressInfo Progress { get; private set; }

        // table
        public UserControl ContentDisplay { get; private set; }
        public List<ContentTable> ContentList { get; private set; }

        private static readonly string sr_server_not_set = (string)Application.Current.FindResource("server_not_set");

        public HomeVModel()
        {
            // init progress
            Progress = new ProgressInfo(0, false);

            // init content list and display
            ContentList = new List<ContentTable>
            {
                new ContentTable("Server", new View.ContentServer()),
                new ContentTable("Netwrok", new View.ContentNetwork()),
            };

            // TODO - Dragable table
            ContentTable contentTable = ContentList.FirstOrDefault(predicate: x => x.Title == App.GlobalPreference.ContentDisplay);
            if (contentTable == null)
            {
                contentTable = ContentList[0];
            }

            contentTable.IsChecked = true;
            ContentDisplay = contentTable.Content;

            // transmit control. Trigge the set
            IsTransmitEnabled = App.GlobalConfig.IsTransmitEnabled;
            IsTransmitControllable = true;

            // save data on closing
            Application.Current.MainWindow.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // case "hide" 
            if (Application.Current.MainWindow.IsVisible)
            {
                return;
            }

            // save preference
            ContentTable contentTable = ContentList.FirstOrDefault(predicate: x => x.IsChecked);
            App.GlobalPreference.ContentDisplay = contentTable.Title;
        }

        /** actoins ====================================================================================================== 
         */
        public void UpdateTransmitStatus()
        {
            OnPropertyChanged("IsTransmitEnabled");
            OnPropertyChanged("TransmitStatus");
        }

        public void LockTransmitControl(bool enable)
        {
            IsTransmitControllable = !enable;
            OnPropertyChanged("IsTransmitControllable");
        }

        public void AddServerByScanQRCode()
        {
            // TODO - Take care of the ContentTables list order
            ContentServerVModel serverViewModel = (ContentServerVModel)ContentList[0].Content.DataContext;
            serverViewModel.CommandAddServerQRCode.Execute(null);
        }

        // Progress is indeterminated, This mothod increase/decrease the progress value.
        // TODO Next - Progress list
        public void UpdateProgress(int progress)
        {
            Progress.Value += progress;
            if (Progress.Value < 0) Progress.Value = 0;

            if (Progress.Value == 0) Progress.IsIndeterminate = false;
            else Progress.IsIndeterminate = true;

            OnPropertyChanged("Progress");
        }


        /** Commands ======================================================================================================
         */
        public RelayCommand CommandSwitchContent => new RelayCommand(SwitchContent);
        private void SwitchContent(object newTitle)
        {
            if (newTitle is string title)
            {
                UserControl content = ContentList.FirstOrDefault(x => x.Title == title).Content;
                if (ContentDisplay != content)
                {
                    ContentDisplay = content;
                    OnPropertyChanged("ContentDisplay");
                }
            }
        }

        // open curl
        public RelayCommand CommandShowCurl => new RelayCommand(ShowCurl);
        private void ShowCurl(object parameter)
        {
            new View.WindowCurl().Show();
        }

        // show setting
        public RelayCommand CommandShowSetting => new RelayCommand(ShowSetting);
        private void ShowSetting(object parameter)
        {
            new View.DialogSetting().ShowDialog();
        }

        // show about
        public RelayCommand CommandShowAbout => new RelayCommand(ShowAbout);
        private void ShowAbout(object parameter)
        {
            new View.DialogAbout().ShowDialog();
        }

        // exit
        public RelayCommand CommandExit => new RelayCommand(ExitApp);
        private void ExitApp(object parameter)
        {
            App.CloseMainWindow();
        }
    }
}
