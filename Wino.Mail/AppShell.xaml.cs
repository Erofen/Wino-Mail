﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Windows.Foundation;

using Wino.Controls;
using Wino.Domain;
using Wino.Domain.Models.Navigation;
using Wino.Extensions;
using Wino.Mail.ViewModels.Data;
using Wino.MenuFlyouts;
using Wino.MenuFlyouts.Context;
using Wino.Views.Abstract;
using Microsoft.UI.Xaml.Controls;
using Wino.Messaging.Client.Shell;
using Wino.Messaging.Client.Mails;
using Wino.Messaging.Client.Accounts;
using Wino.Domain.Entities;
using Wino.Domain.Models.Folders;
using Wino.Domain.Interfaces;
using Wino.Domain.Models.MailItem;





#if NET8_0
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Wino.Views
{
    public sealed partial class AppShell : AppShellAbstract,
        IRecipient<AccountMenuItemExtended>,
        IRecipient<NavigateMailFolderEvent>,
        IRecipient<CreateNewMailWithMultipleAccountsRequested>,
        IRecipient<InfoBarMessageRequested>,
        IRecipient<ApplicationThemeChanged>
    {
        public AppShell() : base()
        {
            InitializeComponent();
#if NET8_0
            // BackdropMaterial is not available in WinUI 3.0.
            // We manually apply it for UWP version only.
            SetupMica();

            //var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            //coreTitleBar.LayoutMetricsChanged += TitleBarLayoutUpdated;
#endif
        }

        // private void TitleBarLayoutUpdated(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarLayout(sender);

        //private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar) => RealAppBar.SystemReserved = coreTitleBar.SystemOverlayRightInset;

        private async void ItemDroppedOnFolder(object sender, DragEventArgs e)
        {
            // Validate package content.
            if (sender is WinoNavigationViewItem droppedContainer)
            {
                droppedContainer.IsDraggingItemOver = false;

                if (CanContinueDragDrop(droppedContainer, e))
                {
                    if (droppedContainer.DataContext is IBaseFolderMenuItem draggingFolder)
                    {
                        var mailCopies = new List<MailCopy>();

                        var dragPackage = e.DataView.Properties[nameof(MailDragPackage)] as MailDragPackage;
                        e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;

                        // Extract mail copies from IMailItem.
                        // ThreadViewModels will be divided into pieces.

                        foreach (var item in dragPackage.DraggingMails)
                        {
                            if (item is MailItemViewModel singleMailItemViewModel)
                            {
                                mailCopies.Add(singleMailItemViewModel.MailCopy);
                            }
                            else if (item is ThreadMailItemViewModel threadViewModel)
                            {
                                mailCopies.AddRange(threadViewModel.GetMailCopies());
                            }
                        }

                        await ViewModel.PerformMoveOperationAsync(mailCopies, draggingFolder);
                    }
                }
            }
        }

        private void ItemDragLeaveFromFolder(object sender, DragEventArgs e)
        {
            if (sender is WinoNavigationViewItem leavingContainer)
            {
                leavingContainer.IsDraggingItemOver = false;
            }
        }

        private bool CanContinueDragDrop(WinoNavigationViewItem interactingContainer, DragEventArgs args)
        {
            // TODO: Maybe override caption with some information why the validation failed?
            // Note: Caption has a max length. It may be trimmed in some languages.

            if (interactingContainer == null || !args.DataView.Properties.ContainsKey(nameof(MailDragPackage))) return false;

            var dragPackage = args.DataView.Properties[nameof(MailDragPackage)] as MailDragPackage;

            // Invalid package.
            if (!dragPackage.DraggingMails.Any()) return false;

            // Check whether source and target folder are the same.
            if (interactingContainer.IsSelected) return false;

            // Check if the interacting container is a folder.
            if (!(interactingContainer.DataContext is IBaseFolderMenuItem folderMenuItem)) return false;

            // Check if the folder is a move target.
            if (!folderMenuItem.IsMoveTarget) return false;

            // Check whether the moving item's account has at least one same as the target folder's account.
            var draggedAccountIds = folderMenuItem.HandlingFolders.Select(a => a.MailAccountId);

            if (!dragPackage.DraggingMails.Any(a => draggedAccountIds.Contains(a.AssignedAccount.Id))) return false;

            return true;
        }

        private void ItemDragEnterOnFolder(object sender, DragEventArgs e)
        {
            // Validate package content.
            if (sender is WinoNavigationViewItem droppedContainer && CanContinueDragDrop(droppedContainer, e))
            {
                droppedContainer.IsDraggingItemOver = true;

                var draggingFolder = droppedContainer.DataContext as IBaseFolderMenuItem;

                e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
                e.DragUIOverride.Caption = string.Format(Translator.DragMoveToFolderCaption, draggingFolder.FolderName);
            }
        }

        public async void Receive(AccountMenuItemExtended message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                if (message.FolderId == default) return;

                if (ViewModel.MenuItems.TryGetFolderMenuItem(message.FolderId, out IBaseFolderMenuItem foundMenuItem))
                {
                    if (foundMenuItem == null) return;

                    foundMenuItem.Expand();

                    await ViewModel.NavigateFolderAsync(foundMenuItem);

                    navigationView.SelectedItem = foundMenuItem;

                    if (message.NavigateMailItem == null) return;

                    // At this point folder is navigated and items are loaded.
                    WeakReferenceMessenger.Default.Send(new MailItemNavigationRequested(message.NavigateMailItem.UniqueId));
                }
            });
        }

        private async void MenuSelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is IMenuItem invokedMenuItem)
            {
                await ViewModel.MenuItemInvokedOrSelectedAsync(invokedMenuItem);
            }
        }

        private async void NavigationViewItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            // SelectsOnInvoked is handled in MenuSelectionChanged.
            // This part is only for the items that are not selectable.
            if (args.InvokedItemContainer is WinoNavigationViewItem winoNavigationViewItem)
            {
                if (winoNavigationViewItem.SelectsOnInvoked) return;

                await ViewModel.MenuItemInvokedOrSelectedAsync(winoNavigationViewItem.DataContext as IMenuItem);
            }
        }

        public void Receive(NavigateMailFolderEvent message)
        {
            if (message.BaseFolderMenuItem == null) return;

            if (navigationView.SelectedItem != message.BaseFolderMenuItem)
            {
                var navigateFolderArgs = new NavigateMailFolderEventArgs(message.BaseFolderMenuItem, message.FolderInitLoadAwaitTask);

                ViewModel.NavigationService.NavigateFolder(navigateFolderArgs);

                // Prevent double navigation.
                navigationView.SelectionChanged -= MenuSelectionChanged;
                navigationView.SelectedItem = message.BaseFolderMenuItem;
                navigationView.SelectionChanged += MenuSelectionChanged;
            }
            else
            {
                // Complete the init task since we are already on the right page.
                message.FolderInitLoadAwaitTask?.TrySetResult(true);
            }
        }

        private void ShellFrameContentNavigated(object sender, NavigationEventArgs e)
            => RealAppBar.ShellFrameContent = (e.Content as BasePage).ShellContent;

        private void BackButtonClicked(Controls.Advanced.WinoAppTitleBar sender, RoutedEventArgs args)
        {
            WeakReferenceMessenger.Default.Send(new ClearMailSelectionsRequested());
            WeakReferenceMessenger.Default.Send(new DisposeRenderingFrameRequested());
            WeakReferenceMessenger.Default.Send(new ShellStateUpdated());

        }

        private async void MenuItemContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            // Delegate this request to ViewModel.
            // VM will prepare available actions for this folder and show Menu Flyout.

            if (sender is WinoNavigationViewItem menuItem &&
                menuItem.DataContext is IBaseFolderMenuItem baseFolderMenuItem &&
                baseFolderMenuItem.IsMoveTarget &&
                args.TryGetPosition(sender, out Point p))
            {
                args.Handled = true;

                var source = new TaskCompletionSource<FolderOperationMenuItem>();

                var actions = ViewModel.GetFolderContextMenuActions(baseFolderMenuItem);
                var flyout = new FolderOperationFlyout(actions, source);

                flyout.ShowAt(menuItem, new FlyoutShowOptions()
                {
                    ShowMode = FlyoutShowMode.Standard,
                    Position = new Point(p.X + 30, p.Y - 20)
                });

                var operation = await source.Task;

                flyout.Dispose();

                // No action selected.
                if (operation == null) return;

                await ViewModel.PerformFolderOperationAsync(operation.Operation, baseFolderMenuItem);
            }
        }

        public void Receive(CreateNewMailWithMultipleAccountsRequested message)
        {
            // Find the NewMail menu item container.

            var container = navigationView.ContainerFromMenuItem(ViewModel.CreateMailMenuItem);

            var flyout = new AccountSelectorFlyout(message.AllAccounts, ViewModel.CreateNewMailForAsync);

            flyout.ShowAt(container, new FlyoutShowOptions()
            {
                ShowMode = FlyoutShowMode.Auto,
                Placement = FlyoutPlacementMode.Right
            });
        }

        private void NavigationPaneOpening(Microsoft.UI.Xaml.Controls.NavigationView sender, object args)
        {
            // It's annoying that NavigationView doesn't respect expansion state of the items in Minimal display mode.
            // Expanded items are collaped, and users need to expand them again.
            // Regardless of the reason, we will expand the selected item if it's a folder with parent account for visibility.

            if (sender.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal && sender.SelectedItem is IFolderMenuItem selectedFolderMenuItem)
            {
                selectedFolderMenuItem.Expand();
            }
        }

        /// <summary>
        /// InfoBar message is requested.
        /// </summary>
        public async void Receive(InfoBarMessageRequested message)
        {
            await ViewModel.ExecuteUIThread(() =>
            {
                if (string.IsNullOrEmpty(message.ActionButtonTitle) || message.Action == null)
                {
                    ShellInfoBar.ActionButton = null;
                }
                else
                {
                    ShellInfoBar.ActionButton = new Button()
                    {
                        Content = message.ActionButtonTitle,
                        Command = new RelayCommand(message.Action)
                    };
                }

                ShellInfoBar.Message = message.Message;
                ShellInfoBar.Title = message.Title;
                ShellInfoBar.Severity = message.Severity.AsMUXCInfoBarSeverity();
                ShellInfoBar.IsOpen = true;
            });
        }


        private void SetupMica()
        {
#if !NET8_0
            var resourceManager = App.Current.Services.GetService<IApplicationResourceManager<ResourceDictionary>>();

            if (resourceManager.ContainsResourceKey("UseMica"))
            {
                bool isMicaEnabled = resourceManager.GetResource<bool>("UseMica");

                BackdropMaterial.SetApplyToRootOrPageBackground(this, isMicaEnabled);
            }
#endif
        }

        public void Receive(ApplicationThemeChanged message) => SetupMica();
    }
}
