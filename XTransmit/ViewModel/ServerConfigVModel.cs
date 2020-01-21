﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XTransmit.Utility;
using XTransmit.ViewModel.Element;

namespace XTransmit.ViewModel
{
    public class ServerConfigVModel : BaseViewModel
    {
        public ServerView ServerInfoData { get; private set; }

        public List<ItemView> ServerIPData { get; private set; }

        private bool vIsFetching = false;
        public bool IsFetching
        {
            get => vIsFetching;
            private set
            {
                vIsFetching = value;
                OnPropertyChanged(nameof(IsFetching));
            }
        }

        // language
        private static readonly string sr_title = (string)Application.Current.FindResource("dialog_server_title");
        private static readonly string sr_not_availabe = (string)Application.Current.FindResource("not_availabe");
        private static readonly string sr_invalid_ip = (string)Application.Current.FindResource("invalid_ip");
        private static readonly string sr_invalid_port = (string)Application.Current.FindResource("invalid_port");

        public ServerConfigVModel(ServerView serverInfo)
        {
            ServerInfoData = serverInfo;

            ServerIPData = UpdateInfo();
        }

        [SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
        private List<ItemView> UpdateInfo()
        {
            return new List<ItemView>()
            {
                new ItemView{Label = "Created", Text = ServerInfoData.TimeCreated ?? sr_not_availabe},
                new ItemView{Label = "Last Ping (ms)", Text = ServerInfoData.Ping.ToString()},

                new ItemView{Label = "Country", Text = ServerInfoData.vServerProfile.IPData?.Country ?? sr_not_availabe},
                new ItemView{Label = "Region", Text = ServerInfoData.vServerProfile.IPData?.Region ?? sr_not_availabe},
                new ItemView{Label = "City", Text = ServerInfoData.vServerProfile.IPData?.City ?? sr_not_availabe},
                new ItemView{Label = "Location", Text = ServerInfoData.vServerProfile.IPData?.Location ?? sr_not_availabe},
                new ItemView{Label = "Org", Text = ServerInfoData.vServerProfile.IPData?.Organization ?? sr_not_availabe},
                new ItemView{Label = "Postal", Text = ServerInfoData.vServerProfile.IPData?.Postal ?? sr_not_availabe},
                //new ItemInfo{Label = "Host Name", Text = ServerInfoData.vServerProfile.IPData?.hostname ?? sr_not_availabe},
            };
        }


        /** Commands ======================================================================================================
         */
        private bool IsNotFetching(object parameter) => !IsFetching;

        public RelayCommand CommandFetchData => new RelayCommand(FetchDataAsync, IsNotFetching);
        private async void FetchDataAsync(object parameter)
        {
            IsFetching = true;

            await Task.Run(() =>
            {
                ServerInfoData.UpdateIPInfo(true);
            }).ConfigureAwait(true);

            ServerIPData = UpdateInfo();
            OnPropertyChanged(nameof(ServerIPData));

            IsFetching = false;
            CommandManager.InvalidateRequerySuggested();
        }

        public RelayCommand CommandCloseOK => new RelayCommand(CloseOK, IsNotFetching);
        private void CloseOK(object parameter)
        {
            Window window = (Window)parameter;

            Match matchIP = Regex.Match(ServerInfoData.HostIP, RegexHelper.IPv4AddressRegex);
            if (!matchIP.Success)
            {
                new View.DialogPrompt(sr_title, sr_invalid_ip).ShowDialog();
                return;
            }
            if (ServerInfoData.HostPort < 100 || ServerInfoData.HostPort > 65535)
            {
                new View.DialogPrompt(sr_title, sr_invalid_port).ShowDialog();
                return;
            }

            window.DialogResult = true;
            window.Close();
        }

        public RelayCommand CommandCloseCancel => new RelayCommand(CloseCancel);
        private void CloseCancel(object parameter)
        {
            if (parameter is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
}
