﻿using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.Avnet.Action;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Common;
using Wes.Flow;
using Wes.Utilities;

namespace Wes.Customer.Avnet.ViewModel
{
    public class ShippingPallteViewModel : ScanViewModelBase<WesFlowID, ShippingPallteAction>, IViewModel
    {
        public ICommand CheckBoxClickCommand
        {
            get
            {
                return new RelayCommand<RoutedEventArgs>((obj) =>
                {
                    if (!MasterAuthorService.Authorization())
                    {
                        CheckBox checkBox = obj.Source as CheckBox;
                        checkBox.IsChecked = !checkBox.IsChecked;
                        this.SelfInfo.IsOnePalletPN = checkBox.IsChecked;
                    }
                });
            }
        }

        public ICommand NextFocusCommand
        {
            get
            {
                return new RelayCommand<KeyEventArgs>((e) =>
                {
                    try
                    {
                        if (e.Key == Key.Enter)
                        {
                            TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                            UIElement focusElement = Keyboard.FocusedElement as UIElement;
                            if (focusElement != null)
                            {
                                focusElement.MoveFocus(request);
                            }
                            e.Handled = true;
                            if (e.Source is TextBox)
                            {
                                TextBox tb = e.Source as TextBox;
                                if (tb.Name == "txtGW")
                                {
                                    var grid = VisualTreeHelper.GetParentObject<Grid>(tb, "inputGrid");
                                    if (grid != null)
                                    {
                                        UIElement uiElement = null;
                                        Control control = VisualTreeHelper.GetChildObject<Control>(grid, null) as Control;
                                        if (control != null)
                                            uiElement = control.FindName("TextScan") as UIElement;

                                        if (uiElement != null)
                                        {
                                            this.ScanValue = "PALLETEND";
                                            if (uiElement.GetType().Name.Contains("AutoCompleteBox"))
                                            {
                                                Type type = uiElement.GetType();
                                                MethodInfo methodInfo = type.GetMethod("Focus");
                                                if (methodInfo != null)
                                                {
                                                    methodInfo.Invoke(uiElement, null);
                                                }
                                            }
                                            else
                                            {
                                                uiElement.Focus();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Error(ex);
                    }
                });
            }
        }

        public ICommand RePrintCommand { get; private set; }

        public ICommand UndoPackageGridLoadingRow { get; private set; }

        protected override ShippingPallteAction GetAction()
        {
            var shippingPallteAction = ViewModelFactory.CreateActoin<ShippingPallteAction>() as ShippingPallteAction;
            if (RePrintCommand == null)
            {
                RePrintCommand = new RelayCommand<object>(shippingPallteAction.RePrint);
            }
            if (UndoPackageGridLoadingRow == null)
            {
                UndoPackageGridLoadingRow = new RelayCommand<DataGridRowEventArgs>(shippingPallteAction.DataGridLoadingRow);
            }
            return shippingPallteAction;
        }

        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_TRUCK_NO;
        }

        protected override void InitNextTargets(Dictionary<WesFlowID, dynamic> nextTargets)
        {
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_TRUCK_NO, new { tooltip = "Truck Order" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID, new { tooltip = "Package ID" });
        }

        public override void GetExceptionIgnoreProperties(HashSet<string> exceptionIgnoreProperties)
        {
            exceptionIgnoreProperties.Add("undoPackages");
            exceptionIgnoreProperties.Add("donePackages");
            exceptionIgnoreProperties.Add("errorPackages");
            exceptionIgnoreProperties.Add("imageViewList");
            exceptionIgnoreProperties.Add("doingPackageSelected");
            exceptionIgnoreProperties.Add("donePackageSelected");
        }

        protected override void TeamSupport()
        {
            var teamSupport = new WesTeamSupport(WesFlowID.FLOW_OUT_BOARDING);
            teamSupport.ShowDialog();
        }

        protected override WesFlowID ScanInterceptor(string scanVal, WesFlowID cst)
        {
            return cst;
        }

        public override void ResetUiStatus()
        {
            base.ResetUiStatus();
            if (this.ScanAction != null)
            {
                this.ScanAction.Command = WesScanCommand.NONE;
            }
            this.ScanAction.PackagePartNo = "";
            this.ScanAction.PartNo = new HashSet<string>();
        }
    }
}
