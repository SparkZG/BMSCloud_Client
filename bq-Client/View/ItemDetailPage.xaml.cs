using System.Windows.Controls;
using DevExpress.Xpf.WindowsUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DevExpress.Xpf.Core;
using SocketLibrary;
using System.Net;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using bq_Client.ViewModels;
using System.ComponentModel;
using System.Globalization;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using DevExpress.Xpf.Charts;

namespace bq_Client.View
{
    /// <summary>
    /// Interaction logic for ItemDetailPage.xaml
    /// </summary>
    public partial class ItemDetailPage : UserControl
    {
        ItemDetailViewModel idv = null;
        public ItemDetailPage()
        {
            InitializeComponent();
        }
        private void ItemDetail_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.FuncNavigationRoot(false);

            idv = (ItemDetailViewModel)this.DataContext;
            //委托附加,要在定时器开始之前！！
            idv.UpdateChartFun += RefreshChart;           


            idv.BatVolt = (DigitVarViewModel)this.FindResource("BatVoltDigitVar");
            idv.BatTemp = (DigitVarViewModel)this.FindResource("BatAmpDigitVar");
            idv.cellMaxVolt = (DigitVarViewModel)this.FindResource("cellMaxVoltDigit");
            idv.cellAvrVolt = (DigitVarViewModel)this.FindResource("cellAvrVoltDigit");
            idv.cellMinVolt = (DigitVarViewModel)this.FindResource("cellMinVoltDigit");
            idv.SetMainList();

            //batCellListBox、batOtherListBox、ChartComobox 、TypeComobox设定绑定源要放在最后定义
            batCellListBox.ItemsSource = idv.PackCellList;
            batOtherListBox.ItemsSource = idv.PackOtherList;
            ChartComobox.ItemsSource = idv.PackDataTypeComobox;
            ChartComobox.SelectedIndex = 1;
        }

        /// <summary>
        /// 更改实时图表的绑定源
        /// </summary>
        private void ChartComobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LineSeries2DReal.Points.BeginInit();
            LineSeries2DReal.Points.Clear();
            foreach (DataRow item in idv.dataCurrent.Rows)
            {
                LineSeries2DReal.Points.Add(new SeriesPoint(Convert.ToDateTime(item["时间"]), Convert.ToDouble(item[idv.DataType])));
            }
            LineSeries2DReal.Points.EndInit();
        }
        /// <summary>
        /// 实时刷新图表
        /// </summary>
        private void RefreshChart(object value)
        {
            LineSeries2DReal.Points.BeginInit();
            LineSeries2DReal.Points.Add(new SeriesPoint(DateTime.Now, (double)value));
            LineSeries2DReal.Points.EndInit();
            //imageV.ToolTip = "电压一级告警";
        }

        private void ReConnect_Click(object sender, RoutedEventArgs e)
        {
            idv.ReConnect();
        }

        private void Operate_Click(object sender, RoutedEventArgs e)
        {
            PassWord pw = new PassWord();
            bool? result = pw.ShowDialog();
            if (result == true)
            {
                idv.SendControlByte();
            }

        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key==Key.Escape)
            {
                BLLCommon.CloseWaitWindow(false);  
            }
        }


    }
}
