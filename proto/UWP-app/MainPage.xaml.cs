using System;
using System.Collections.Generic;
using System.IO;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace proto
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        IAsyncOperation<ProcessLauncherResult> processLauncherResult;
        ProcessLauncherOptions processLauncherOptions;

        InMemoryRandomAccessStream standardInput;
        InMemoryRandomAccessStream standardOutput;

        private async Task SendDataToProxy(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            IBuffer ibuffer = buffer.AsBuffer();
            await standardInput.WriteAsync(ibuffer);
        }

        ulong lastByteRead = 0;

        private async void button_Click_Send(object sender, RoutedEventArgs e)
        {
            SendDataToProxy(this.textBox.Text);
        }

        private async void button_Click_Receive(object sender, RoutedEventArgs e)
        {
#if false
            var s = standardOutput.AsStreamForRead();
            byte[] chars = new byte[256];
            int read = s.Read(chars, 0, 256);
            System.Diagnostics.Debug.WriteLine(read);
#endif

            using (var outStreamRedirect = standardOutput.GetInputStreamAt(lastByteRead))
            {
                var size = standardOutput.Size;
                using (var dataReader = new DataReader(outStreamRedirect))
                {
#if false
                    var buff = dataReader.ReadBuffer((uint)size);
                    byte[] chars = new byte[size];
                    var bytesLoaded = await buff.AsStream().ReadAsync(chars, 0, (int)size);
                    string stringRead = Encoding.ASCII.GetString(chars);
#else
                    var bytesLoaded = await dataReader.LoadAsync((uint)size);
                    var stringRead = dataReader.ReadString(bytesLoaded);
#endif
                    if (stringRead == "") stringRead = "<empty>";
                    System.Diagnostics.Debug.WriteLine(stringRead);
                    lastByteRead += (ulong)bytesLoaded;
                    textBlockReceived.Text = stringRead;
                }
            }
        }

        private async Task StartProxyProcess()
        {
            processLauncherOptions = new ProcessLauncherOptions();
            standardInput = new InMemoryRandomAccessStream();
            standardOutput = new InMemoryRandomAccessStream();
            var standardError = new InMemoryRandomAccessStream();

            processLauncherOptions.StandardOutput = standardOutput;
            processLauncherOptions.StandardError = standardError;
            processLauncherOptions.StandardInput = standardInput.GetInputStreamAt(0);
/*
            byte[] buffer = Encoding.ASCII.GetBytes("first string before starting the process");
            IBuffer ibuffer = buffer.AsBuffer();
            await standardInput.WriteAsync(ibuffer);
*/
            processLauncherResult = ProcessLauncher.RunToCompletionAsync("comm-proxy.exe", "", processLauncherOptions);

#if false
            using (var outStreamRedirect = standardOutput.GetInputStreamAt(0))
            {
                var size = standardOutput.Size;
                using (var dataReader = new DataReader(outStreamRedirect))
                {
                    var bytesLoaded = await dataReader.LoadAsync((uint)size);
                    var stringRead = dataReader.ReadString(bytesLoaded);
                    System.Diagnostics.Debug.WriteLine(stringRead);
                }
            }
#endif

        }

        private async void button_Click_Start(object sender, RoutedEventArgs e)
        {
            StartProxyProcess();
        }

        private async void button_Click_Close(object sender, RoutedEventArgs e)
        {
            processLauncherOptions.StandardInput.Dispose();
        }
    }
}
