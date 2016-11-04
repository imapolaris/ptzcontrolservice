using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PTZControlClient
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            base.OnStartup(e);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Common.Log.Logger.Default.Error(e.ExceptionObject.ToString());
            Exception ex = (e.ExceptionObject as Exception);
            if (ex != null)
                MessageBox.Show(ex.Message);
            else
            MessageBox.Show(e.ExceptionObject.ToString());
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Common.Log.Logger.Default.Error(e.Exception);
            MessageBox.Show(e.Exception.Message);
            e.Handled = true;
        }
    }
}