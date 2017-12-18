using System.Windows;
using DevExpress.Xpf.Core;
using System;
using System.Net;
using SocketLibrary;
using System.Windows.Controls;
using DevExpress.Xpf.WindowsUI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using bq_Client.ViewModels;
using System.ComponentModel;
using System.Globalization;
using DevExpress.Xpf.Grid;
using System.Collections.ObjectModel;

namespace bq_Client
{
    /// <summary>
    /// 定义一个全局委托，用来更新界面元素
    /// </summary>
    /// <param name="Object">介入的object</param>
    public delegate void DeleFunc1(object Object);


    /// <summary>
    /// 定义一个ViewModel全局委托，用来更新界面元素
    /// </summary>
    /// <param name="Object">介入的object</param>
    public delegate void DeleFunc2(object Object, byte eventType = 0xff);


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DXWindow
    {

        MainViewModel mvm = new MainViewModel();
        public static DeleFunc1 FuncNavigationRoot = null;

        public MainWindow()
        {
            InitializeComponent();
            mvm = (MainViewModel)this.DataContext;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FuncNavigationRoot = IsRootMode =>
            {
                if ((bool)IsRootMode)
                {
                    NavigationRoot.BackNavigationMode = BackNavigationMode.Root;
                }
                else
                {
                    NavigationRoot.BackNavigationMode = BackNavigationMode.PreviousScreen;
                }
            };            

            mvm.StartConnent();
        }      

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (ViewModel.socketClient != null)
            {
                ViewModel.socketClient.Stop();
                ViewModel.socketClient = null;
            }
        }
    }
}

