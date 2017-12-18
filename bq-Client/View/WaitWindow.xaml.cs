using System;
using System.Windows;
using DevExpress.Xpf.Core;
using System.Windows.Input;

namespace bq_Client.View
{
    public partial class WaitWindow : Window, ISplashScreen {
        public WaitWindow() {
            InitializeComponent();
        }
        public void Progress(double value) { }
        public void CloseSplashScreen() { Close(); }
        public void SetProgressState(bool isIndeterminate) { }

        private void splashWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                BLLCommon.CloseWaitWindow(false);  
            }
        }
    }
}
