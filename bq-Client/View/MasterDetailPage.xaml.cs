using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using bq_Client.ViewModels;
using DevExpress.Xpf.Charts;
using System.Data;
using bq_Client.DataModel;

namespace bq_Client.View
{
    /// <summary>
    /// MasterDetailPage.xaml 的交互逻辑
    /// </summary>
    public partial class MasterDetailPage : UserControl
    {
        MasterDetailViewModel pgv = null;
        public MasterDetailPage()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.FuncNavigationRoot(false);

            this.FlCPack.AddHandler(Button.ClickEvent, new RoutedEventHandler(Button_Click));

            pgv = (MasterDetailViewModel)this.DataContext;
            //委托附加,要在定时器开始之前！！
            pgv.UpdateChartFun += RefreshChart;
            pgv.LoadDataList();
            PackGrid.ItemsSource = pgv.MasterDataList;
            warnStateListBox.ItemsSource = pgv.stateList;                   
        }

        private void ChartComobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LineSeries2D.Points.BeginInit();
            LineSeries2D.Points.Clear();
            foreach (DataRow item in pgv.dataCurrent.Rows)
            {
                LineSeries2D.Points.Add(new SeriesPoint(Convert.ToDateTime(item["时间"]), Convert.ToDouble(item[pgv.DataType])));
            }
            LineSeries2D.Points.EndInit();
        }

        /// <summary>
        /// 实时刷新图表
        /// </summary>
        private void RefreshChart(object value)
        {
            LineSeries2D.Points.BeginInit();
            LineSeries2D.Points.Add(new SeriesPoint(DateTime.Now, (double)value));
            LineSeries2D.Points.EndInit();
        }

        private void PackGrid_SelectedItemChanged(object sender, DevExpress.Xpf.Grid.SelectedItemChangedEventArgs e)
        {
            LineSeries2D.Points.BeginInit();
            LineSeries2D.Points.Clear();
            foreach (DataRow item in pgv.dataCurrent.Rows)
            {
                LineSeries2D.Points.Add(new SeriesPoint(Convert.ToDateTime(item["时间"]), Convert.ToDouble(item[pgv.DataType])));
            }
            LineSeries2D.Points.EndInit();
        }

        private void ReConnect_Click(object sender, RoutedEventArgs e)
        {
            pgv.ReConnect();
        }

        private void Operate_Click(object sender, RoutedEventArgs e)
        {
            PassWord pw = new PassWord();
            bool? result = pw.ShowDialog();
            if (result == true)
            {
                pgv.SendControlByte();
            }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (pgv.IsFirst)
            {
                SampleDataItem sd = (SampleDataItem)(e.OriginalSource as Button).DataContext;
                pgv.NavigateToPack.Execute(sd.ItemId);
            }            
        }
    }
}
