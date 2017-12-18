using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.ComponentModel;
using System.Globalization;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows.Threading;
using System.Windows.Media;
using bq_Client.ViewModels;

namespace bq_Client.Converts
{
    #region  取反转换器 ReverseConverter
    [ValueConversion(typeof(Boolean), typeof(Boolean))]
    public class ReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Boolean state = (Boolean)value;
            return !state;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Boolean state = (Boolean)value;
            return !state;
        }
    }
    #endregion

    #region  值转换器 StateTextConverter
    [ValueConversion(typeof(Boolean), typeof(string))]
    public class StateTextConverter : IValueConverter
    {
        public string TextOnTrue { get; set; }
        public string TextOnFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Boolean state = (Boolean)value;
            if (state)
                return TextOnTrue;

            return TextOnFalse;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string state = (string)value;

            if (state == TextOnTrue)
                return true;

            return false;
        }
    }
    #endregion

    #region  计数值转换器  LargeValueTextConverter
    [ValueConversion(typeof(int), typeof(string))]
    public class LargeValueTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result;

            int countVal = (int)value;
            if (countVal < 1000)
            {
                result = string.Format("{0} ", countVal);
            }
            else if (countVal < 1000000)
            {
                result = string.Format("{0}K", countVal / 1000);
            }
            else
            {
                result = string.Format("{0}M", countVal / 1000000);
            }
            return result;

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    #region  可变精度转换器 DigitViewConverter
    [ValueConversion(typeof(float[]), typeof(String))]
    public class DigitViewConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string result;

            if ((string)parameter == "Decimal")
            {
                Decimal val = (Decimal)values[0];
                Decimal scale = (Decimal)values[1];

                if (scale == 0.001M)
                    result = String.Format("{0:D3}", val);
                else if (scale == 0.01M)
                    result = String.Format("{0:D2}", val);
                else if (scale == 0.1M)
                    result = String.Format("{0:D1}", val);
                else
                    result = String.Format("{0:D0}", val);
            }
            else
            {
                double val = (double)values[0];
                double scale = (double)values[1];

                if (scale == 0.001)
                    result = String.Format("{0:F3}", val);
                else if (scale == 0.01)
                    result = String.Format("{0:F2}", val);
                else if (scale == 0.1)
                    result = String.Format("{0:F1}", val);
                else
                    result = String.Format("{0:F0}", val);
            }

            if (values.Length == 3)
            {
                result += ' ' + (string)values[2];
            }

            return result;

        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    #endregion


    #region  状态颜色转换器 StatusColorConverter
    [ValueConversion(typeof(string), typeof(Brush))]
    public class StatusColorConverter : IValueConverter
    {
        public Brush ProtectOnBrush { get; set; }
        public Brush WarnOnBrush { get; set; }
        public Brush NormalOnBrush { get; set; }
        public Brush OverOnBrush { get; set; }
        public Brush UnderOnBrush { get; set; }
        public Brush OtherOnBrush { get; set; }
        public Brush OffBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = (string)value;
            Brush brush;

            switch (status)
            {
                case "ProtectOn":
                    brush = ProtectOnBrush; break;

                case "WarnOn":
                    brush = WarnOnBrush; break;

                case "NormalOn":
                    brush = NormalOnBrush; break;

                case "OverOn":
                    brush = OverOnBrush; break;

                case "UnderOn":
                    brush = UnderOnBrush; break;

                case "OtherOn":
                    brush = OtherOnBrush; break;

                default:
                    brush = OffBrush; break;
            }

            return brush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    #endregion


    #region  报警文本转换器 WarnTextConverter
    [ValueConversion(typeof(String[]), typeof(String))]
    public class WarnTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string index = (string)values[0];
            string warn = (string)values[1];
            return string.Format("第{0}条记录：{1}", index, warn);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    #region  报警文本块转换器 WarnTextBlockConverter
    [ValueConversion(typeof(Object[]), typeof(TextBlock))]
    public class WarnTextBlockConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int index = (int)values[0];
            DateTime dateTime = (DateTime)values[1];
            string mode = (string)values[2];
            string warn = (string)values[3];

            TextBlock textBlock = new TextBlock();
            textBlock.Padding = new Thickness(5);
            textBlock.LineHeight = 18;

            textBlock.Text = string.Format("序号 : {0}\n时间 : {1}\n模式 : {2}\n报警 : {3}",
                index, dateTime, mode, warn);
            return textBlock;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    #region 重新连接按钮可见性转换
    public class ReverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter.ToString() == "Reconnect")
            {
                if ((bool)value)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            else
            {
                if (!(bool)value)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region ToolTip显示转换
    public class ReverseToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string toolTip = "";
            if (parameter.ToString() == "RefreshCheck")
            {
                if ((bool)value)
                {
                    toolTip = "关闭自动刷新";
                }
                else
                {
                    toolTip = "开启自动刷新";
                }
            }
            return toolTip;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region ToolTip显示转换
    public class ReverseDataRangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value == 0)
            {
                return "全部";
            }
            else
            {
                return value;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int range = 0;
            if (value != null)
            {
                if (value.ToString() != "全部")
                {
                    try
                    {
                        range = (int)value;
                    }
                    catch (Exception)
                    {

                        return 0;
                    }
                    
                }
            }
            return range;
        }
    }
    #endregion

    #region AlarmRank转换
    public class ReverseAlarmRankConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter.ToString() == "Back")
            {
                if ((byte)value == 0x03)
                {
                    return new SolidColorBrush(Colors.OrangeRed);
                }
                else if ((byte)value == 0x02)
                {
                    return new SolidColorBrush(Colors.Chocolate);
                }
                else if ((byte)value == 0x01)
                {
                    return new SolidColorBrush(Colors.Gold);
                }
                else if ((byte)value == 0x00)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF54AF0E"));
                }
                else
                {
                    return new SolidColorBrush(Colors.Gray);
                }
            }
            else if (parameter.ToString() == "Fore")
            {
                if ((byte)value == 0x00 || (byte)value == 0xff)
                {
                    return new SolidColorBrush(Colors.White);
                }
                else
                {
                    return new SolidColorBrush(Colors.Black);
                }
            }
            else if (parameter.ToString() == "Status")
            {
                if ((byte)value == 0x03)
                {
                    return ViewModel.GetImage("Assets/red2.png");
                }
                else if ((byte)value == 0x02)
                {
                    return ViewModel.GetImage("Assets/orange2.png");
                }
                else if ((byte)value == 0x01)
                {
                    return ViewModel.GetImage("Assets/yellow2.png");
                }
                else
                {
                    return ViewModel.GetImage("Assets/green2.png");
                }
            }
            else
            {
                if ((byte)value == 0x03)
                {
                    return "一级故障";
                }
                else if ((byte)value == 0x02)
                {
                    return "二级故障";
                }
                else if ((byte)value == 0x01)
                {
                    return "三级故障";
                }
                else if ((byte)value == 0x00)                
                {
                    return "正常";
                }
                else
                {
                    return "离线";
                }
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region DataType转换
    public class DataTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //如果未选中任何行则默认未SOC
            if (value == null)
            {
                return "SOC(%)";
            }
            SummaryVarViewModel sv = (SummaryVarViewModel)value;
            return sv.VarNameUnit;
        }
    }
    #endregion


}
