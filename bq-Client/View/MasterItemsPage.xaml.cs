using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows;
using bq_Client.ViewModels;
using System;
using System.Threading;
using SocketLibrary;
using bq_Client.DataModel;

namespace bq_Client.View
{
    /// <summary>
    /// Interaction logic for MasterItemsPage.xaml
    /// </summary>
    public partial class MasterItemsPage : UserControl
    {
        MasterItemsViewModel giv = null;
        public MasterItemsPage()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.FlC.AddHandler(Button.ClickEvent, new RoutedEventHandler(Button_Click));
            giv = (MasterItemsViewModel)this.DataContext;
            custList.ItemsSource = ViewModel.CustListSource;
        }
        private void ReConnect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            giv.ReConnect();
        }

        private void Operate_Click(object sender, RoutedEventArgs e)
        {
            PassWord pw = new PassWord();
            bool? result = pw.ShowDialog();
            if (result == true)
            {
                giv.SendControlByte();
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (giv.IsFirst)
            {
                SampleDataItem sd = (SampleDataItem)(e.OriginalSource as Button).DataContext;
                giv.NavigateToMaster.Execute(sd.ItemId);
            }            
        }
    }
}
