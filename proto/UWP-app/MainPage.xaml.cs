using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Streams;
using System.Text;
using Windows.UI.Core;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;

#pragma warning disable 4014

namespace proto
{
    public sealed partial class MainPage : Page
    {
        IAsyncOperation<ProcessLauncherResult> processLauncherResult;
        ProcessLauncherOptions processLauncherOptions;
        IRandomAccessStream standardInput;

        public MainPage()
        {
            this.InitializeComponent();

            var pkgFamilyName = Package.Current.Id.FamilyName;
            System.Diagnostics.Debug.WriteLine("Package Family Name:");
            System.Diagnostics.Debug.WriteLine(pkgFamilyName);

            App.OnDataReceived = (str) =>
            {
                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.textBlockReceived.Text = str;
                });
            };
        }

        private async Task SendDataToProxy(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            IBuffer ibuffer = buffer.AsBuffer();
            await standardInput.WriteAsync(ibuffer);
        }

        private void button_Click_Send(object sender, RoutedEventArgs e)
        {
            SendDataToProxy(this.textBox.Text);
        }

        System.Threading.AutoResetEvent gotData = new System.Threading.AutoResetEvent(false);

        private void StartProxyProcess()
        {
            processLauncherOptions = new ProcessLauncherOptions();
            standardInput = new InMemoryRandomAccessStream();

            processLauncherOptions.StandardOutput = null;
            processLauncherOptions.StandardError = null;
            processLauncherOptions.StandardInput = standardInput.GetInputStreamAt(0);

            processLauncherResult = ProcessLauncher.RunToCompletionAsync(
                "comm-proxy.exe", 
                "com.microsoft.echo" + " " + Package.Current.Id.FamilyName, 
                processLauncherOptions);
        }

        private void button_Click_Start(object sender, RoutedEventArgs e)
        {
            StartProxyProcess();
            this.buttonStart.IsEnabled = false;
        }
    }
}
