﻿using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Wino.Domain.Exceptions;
using Wino.Messaging.Client.Mails;
using Wino.Domain.Exceptions;
using Wino.Domain.Models.AutoDiscovery;
using Wino.Domain.Entities;
using Wino.Domain.Interfaces;






#if NET8_0
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
#endif
namespace Wino.Views.ImapSetup
{
    public sealed partial class TestingImapConnectionPage : Page
    {
        private IImapTestService _imapTestService = App.Current.Services.GetService<IImapTestService>();

        public TestingImapConnectionPage()
        {
            InitializeComponent();
        }

        private async Task TryTestConnectionAsync(CustomServerInformation serverInformation)
        {
            await Task.Delay(1000);

            await _imapTestService.TestImapConnectionAsync(serverInformation);

            // All success. Finish setup with validated server information.

            WeakReferenceMessenger.Default.Send(new ImapSetupDismissRequested(serverInformation));
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // We can only go back to this page from failed connection page.
            // We must go back once again in that case to actual setup dialog.
            if (e.NavigationMode == NavigationMode.Back)
            {
                WeakReferenceMessenger.Default.Send(new ImapSetupBackNavigationRequested());
            }
            else
            {
                // Test connection

                CustomServerInformation serverInformationToTest = null;
                AutoDiscoverySettings autoDiscoverySettings = null;

                // Discovery settings are passed.
                // Create server information out of the discovery settings.
                if (e.Parameter is AutoDiscoverySettings parameterAutoDiscoverySettings)
                {
                    autoDiscoverySettings = parameterAutoDiscoverySettings;
                    serverInformationToTest = autoDiscoverySettings.ToServerInformation();
                }
                else if (e.Parameter is CustomServerInformation customServerInformation)
                {
                    // Only server information is passed.
                    serverInformationToTest = customServerInformation;
                }

                try
                {
                    await TryTestConnectionAsync(serverInformationToTest);
                }
                catch (Exception ex)
                {
                    string protocolLog = ex is ImapClientPoolException clientPoolException ? clientPoolException.ProtocolLog : string.Empty;

                    var failurePackage = new ImapConnectionFailedPackage(ex, protocolLog, autoDiscoverySettings);

                    WeakReferenceMessenger.Default.Send(new ImapSetupBackNavigationRequested(typeof(ImapConnectionFailedPage), failurePackage));
                }
            }
        }
    }
}
