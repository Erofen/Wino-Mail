﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Wino.Helpers;
using Wino.MenuFlyouts;
using Wino.Domain.Models.Menus;
using Wino.Domain.Enums;


#if NET8_0
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif
namespace Wino.Controls
{
    public class RendererCommandBar : CommandBar, IDisposable
    {
        public static readonly DependencyProperty MenuItemsProperty = DependencyProperty.Register(nameof(MenuItemsProperty), typeof(ObservableCollection<MailOperationMenuItem>), typeof(RendererCommandBar), new PropertyMetadata(null, OnMenuItemsChanged));
        public static readonly DependencyProperty OperationClickedCommandProperty = DependencyProperty.Register(nameof(OperationClickedCommand), typeof(ICommand), typeof(RendererCommandBar), new PropertyMetadata(null));

        public ICommand OperationClickedCommand
        {
            get { return (ICommand)GetValue(OperationClickedCommandProperty); }
            set { SetValue(OperationClickedCommandProperty, value); }
        }

        public ObservableCollection<MailOperationMenuItem> MenuItems
        {
            get { return (ObservableCollection<MailOperationMenuItem>)GetValue(MenuItemsProperty); }
            set { SetValue(MenuItemsProperty, value); }
        }

        public RendererCommandBar()
        {
            this.DefaultStyleKey = typeof(CommandBar);
        }

        private static void OnMenuItemsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is RendererCommandBar commandBar)
            {
                if (args.OldValue != null)
                    commandBar.UnregisterButtonCollection(args.NewValue as ObservableCollection<MailOperationMenuItem>);

                if (args.NewValue != null)
                    commandBar.RegisterButtonCollection(args.NewValue as ObservableCollection<MailOperationMenuItem>);
            }
        }

        private void RegisterButtonCollection(ObservableCollection<MailOperationMenuItem> collection)
        {
            collection.CollectionChanged -= ButtonCollectionChanged;
            collection.CollectionChanged += ButtonCollectionChanged;

            InitItems(collection);
        }

        private void UnregisterButtonCollection(ObservableCollection<MailOperationMenuItem> collection)
        {
            collection.CollectionChanged -= ButtonCollectionChanged;
        }

        // One time initializer on registration.
        private void InitItems(IEnumerable<MailOperationMenuItem> items)
        {
            foreach (var item in items)
            {
                var operationText = XamlHelpers.GetOperationString(item.Operation);

                var operationAppBarItem = new AppBarButton()
                {
                    Label = operationText
                };

                ToolTipService.SetToolTip(operationAppBarItem, operationText);

                PrimaryCommands.Add(operationAppBarItem);
            }
        }

        private void ButtonCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                PrimaryCommands.Clear();
                SecondaryCommands.Clear();
            }

            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    if (newItem is MailOperationMenuItem item)
                    {
                        var button = new RendererCommandBarItem(item.Operation, Clicked)
                        {
                            IsEnabled = item.IsEnabled
                        };

                        if (!item.IsSecondaryMenuPreferred)
                            PrimaryCommands.Add(button);
                        else
                            SecondaryCommands.Add(button);
                    }
                }
            }
        }

        private void Clicked(MailOperation operation)
        {
            OperationClickedCommand?.Execute(operation);
        }

        private void DisposeMenuItems()
        {
            foreach (var item in this.PrimaryCommands)
            {
                if (item is RendererCommandBarItem rendererCommandBarItem)
                {
                    rendererCommandBarItem.Dispose();
                }
            }

            foreach (var item in this.SecondaryCommands)
            {
                if (item is RendererCommandBarItem rendererCommandBarItem)
                {
                    rendererCommandBarItem.Dispose();
                }
            }
        }

        public void Dispose()
        {
            DisposeMenuItems();
        }
    }
}
