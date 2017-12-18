using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;


namespace bq_Client
{
    /// <summary>
    /// Interaction logic for PassWord.xaml
    /// </summary>
    public partial class PassWord : DXWindow
    {
        public PassWord()
        {
            InitializeComponent();
            passwordBox.PasswordChar = (char)9679;
            passwordBox.Focus();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            else if (e.Key == Key.Enter)
            {
                string pass = passwordBox.Password.ToLower();
                if (pass == "888888" || pass == "mjq263")
                {
                    this.DialogResult = true;
                }
                else
                {
                    promptTexBlcok.Text = "密码错误，重新输入或者按【ESC】退出";
                    promptTexBlcok.Visibility = System.Windows.Visibility.Visible;
                    passwordBox.Focus();
                }
            }
            else if (promptTexBlcok.Visibility == System.Windows.Visibility.Visible)
            {
                promptTexBlcok.Visibility = System.Windows.Visibility.Collapsed;
                passwordBox.Password = "";
            }
        }
    }
}
