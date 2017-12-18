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
using DevExpress.Xpf.Core;
using System.Data;
using Microsoft.Win32;
using DevExpress.Xpf.Charts;

namespace bq_Client.View
{
    /// <summary>
    /// HistoryDataPage.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryDataPage : UserControl
    {
        HistoryViewModel hvm = new HistoryViewModel();
        public HistoryDataPage()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            MainWindow.FuncNavigationRoot(true);

            hvm = (HistoryViewModel)this.DataContext;
            hvm.CreateChartFun += CreateChart;
            hvm.AfterSelectPackFun += Null =>
            {
                hvm.DataHistory = hvm.dataHistoryPack.Clone();
                HistotyGrid.ItemsSource = hvm.DataHistory;
                CreateChart(null);
            };
            DataRange.ItemsSource = BLLCommon.dataRange;


            string MaxDate = DateTime.Now.ToShortDateString();
            MaxDate += " 23:59:59";

            bStartDate.MaxValue = Convert.ToDateTime(MaxDate);
            bEndDate.MaxValue = Convert.ToDateTime(MaxDate);


            //主机como源
            List<int> MasterComoSource = new List<int> { };
            for (int i = 1; i <= Properties.Settings.Default.MasterNum; i++)
            {
                MasterComoSource.Add(i);
            }
            MasterComo.ItemsSource = MasterComoSource;
            if (Properties.Settings.Default.HistoryMasterIndex > MasterComoSource.Count - 1)
                MasterComo.SelectedIndex = 0;
            else
                MasterComo.SelectedIndex = Properties.Settings.Default.HistoryMasterIndex;

            HistoryTypeComo.ItemsSource = BLLCommon.historyType;

            //pack-como源
            List<int> PackComoSource = new List<int> { };
            for (int i = 1; i <= Properties.Settings.Default.PackNum; i++)
            {
                PackComoSource.Add(i);
            }
            PackComo.ItemsSource = PackComoSource;
            if (Properties.Settings.Default.HistoryPackIndex == -1)
            {
                HistoryTypeComo.SelectedIndex = 0;
                PackComo.SelectedIndex = 0;
            }
            else
            {
                if (Properties.Settings.Default.HistoryPackIndex > PackComoSource.Count - 1)
                    PackComo.SelectedIndex = 0;
                else
                    PackComo.SelectedIndex = Properties.Settings.Default.HistoryPackIndex;
                HistoryTypeComo.SelectedIndex = 1;
            }


        }
        private void CreateChart(object o)
        {
            LineSeries2DAnalysis.Points.BeginInit();
            LineSeries2DAnalysis.Points.Clear();
            foreach (DataRow item in hvm.DataHistory.Rows)
            {
                DateTime drTime = Convert.ToDateTime(item["时间"]);
                if (drTime > hvm.StartTime && drTime < hvm.EndTime)
                {
                    try
                    {
                        LineSeries2DAnalysis.Points.Add(new SeriesPoint(drTime, Convert.ToDouble(item[hvm.DataTypeChart])));
                    }
                    catch (Exception)
                    {
                        DXMessageBox.Show("数据类型选择不正确，无法生成图表，请重新选择！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
                    if (hvm.DataRange != 0 && LineSeries2DAnalysis.Points.Count > hvm.DataRange)
                    {
                        LineSeries2DAnalysis.Points.RemoveAt(0);
                    }
                }
            }
            LineSeries2DAnalysis.Points.EndInit();
        }

        #region Command-读取历史信息
        /// <summary>
        /// 语言选择命令
        /// </summary>
        private static RoutedUICommand readHistoryData = new RoutedUICommand("ReadHistoryData", "ReadHistoryData", typeof(ItemDetailPage));
        public static RoutedUICommand ReadHistoryData
        {
            get { return readHistoryData; }
        }


        private void ReadHistoryData_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void ReadHistoryData_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (hvm.StartTime > hvm.EndTime)
            {
                DXMessageBox.Show("开始时间不能大于结束时间！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            hvm.SendReadHistoryCommand(HistoryTypeComo.SelectedIndex);
        }
        #endregion


        #region Command-导出CSV文件
        /// <summary>
        /// 导出csv文件命令
        /// </summary>
        private static RoutedUICommand exportData = new RoutedUICommand("ExportData", "ExportData", typeof(ItemDetailPage));
        public static RoutedUICommand ExportData
        {
            get { return exportData; }
        }


        private void ExportData_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void ExportData_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            saveFileDialog.AddExtension = true;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.DefaultExt = "*.CSV";
            saveFileDialog.Filter = "CSV files|*.CSV";
            saveFileDialog.FileName = "HistoryData" + "(" + hvm.StartTime + "--" + hvm.EndTime + ")";
            bool? result = saveFileDialog.ShowDialog();
            if (result == true && saveFileDialog.FileName != null) //打开保存文件对话框
            {
                if (hvm.ExportCSV(saveFileDialog.FileName))
                {
                    DXMessageBox.Show("导出成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    DXMessageBox.Show("导出失败！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        #endregion

        private void ReConnect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            hvm.ReConnect();
        }

        private void Operate_Click(object sender, RoutedEventArgs e)
        {
            PassWord pw = new PassWord();
            bool? result = pw.ShowDialog();
            if (result == true)
            {
                hvm.SendControlByte();
            }

        }
        private void TypeComo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryTypeComo.SelectedIndex == 0)
            {
                PackComo.Visibility = Visibility.Collapsed;
                hvm.DataHistory = ViewModel.dataCurrentMaster.Clone();
                HistotyGrid.ColumnsSource = hvm.MasterDataColumn;
                //TypeComobox设定绑定源要放在最后定义
                TypeComobox.ItemsSource = ViewModel.MasterListComobox;
                HistotyGrid.ItemsSource = hvm.DataHistory;
                CreateChart(null);
            }
            else
            {
                hvm.SendReadPackStatus();
                if (!hvm.OffConnect)
                {
                    PackComo.Visibility = Visibility.Visible;
                }                
                HistotyGrid.ColumnsSource = hvm.PackListColumn;
                //TypeComobox设定绑定源要放在最后定义
                TypeComobox.ItemsSource = hvm.PackDataTypeComobox;
            }                        
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                BLLCommon.CloseWaitWindow(false);  
            }
        }

        private void MasterOrPack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            hvm.DataHistory.Rows.Clear();
            CreateChart(null);
        }
    }
}
