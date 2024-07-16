﻿using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;

namespace Wino.Server
{
    public class ServerContext
    {
        private AppServiceConnection connection = null;

        public ServerContext()
        {
            InitializeAppServiceConnection();
        }

        /// <summary>
        /// Open connection to UWP app service
        /// </summary>
        public async void InitializeAppServiceConnection()
        {
            connection = new AppServiceConnection
            {
                AppServiceName = "WinoInteropService",
                PackageFamilyName = Package.Current.Id.FamilyName
            };

            connection.RequestReceived += OnWinRTMessageReceived;
            connection.ServiceClosed += OnConnectionClosed;

            AppServiceConnectionStatus status = await connection.OpenAsync();

            if (status != AppServiceConnectionStatus.Success)
            {
                // TODO: Handle connection error
            }
        }

        private void OnConnectionClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            // TODO: Handle connection closed.

            // UWP app might've been terminated.
        }

        private void OnWinRTMessageReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // TODO: Handle incoming messages from UWP/WINUI Application.
        }
    }
}
