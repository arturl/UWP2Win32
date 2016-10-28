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

#pragma warning disable 4014

namespace proto
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        IAsyncOperation<ProcessLauncherResult> processLauncherResult;
        ProcessLauncherOptions processLauncherOptions;

        IRandomAccessStream standardInput;
        IRandomAccessStream standardOutput;

        System.Threading.EventWaitHandle outputRead;

        public MainPage()
        {
            this.InitializeComponent();
            //outputRead = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset, "iot-core-comm-proxy-output-ready");
        }

        private async Task SendDataToProxy(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            IBuffer ibuffer = buffer.AsBuffer();
            await standardInput.WriteAsync(ibuffer);
        }

        ulong lastByteRead = 0;

        private void button_Click_Send(object sender, RoutedEventArgs e)
        {
            SendDataToProxy(this.textBox.Text);
        }

        System.Threading.AutoResetEvent gotData = new System.Threading.AutoResetEvent(false);

        private void StartReceiver()
        {
            Windows.System.Threading.ThreadPool.RunAsync(async (op) =>  {

                while (true)
                {
                    gotData.WaitOne();

                    using (var outStreamRedirect = standardOutput.GetInputStreamAt(lastByteRead))
                    {
                        var size = standardOutput.Size;
                        using (var dataReader = new DataReader(outStreamRedirect))
                        {
                            var bytesLoaded = await dataReader.LoadAsync((uint)size);
                            if (bytesLoaded > 0)
                            {
                                var stringRead = dataReader.ReadString(bytesLoaded);
                                if (stringRead == "") stringRead = "<empty>";
                                Debug.WriteLine(stringRead);
                                lastByteRead += bytesLoaded;

                                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    textBlockReceived.Text = stringRead;
                                });
                            }
                        }
                    }
                }
            });
        }

        private async Task StartProxyProcess()
        {
            processLauncherOptions = new ProcessLauncherOptions();
            standardInput = new InMemoryRandomAccessStream();
            standardOutput = new InMemoryRandomAccessStream();
            var standardError = new InMemoryRandomAccessStream();

            processLauncherOptions.StandardOutput = standardOutput.GetOutputStreamAt(0);
            processLauncherOptions.StandardError = standardError;
            processLauncherOptions.StandardInput = standardInput.GetInputStreamAt(0);

            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            var syncFolder = await storageFolder.CreateFolderAsync("sync", CreationCollisionOption.OpenIfExists);
            QueryOptions queryOptions = new QueryOptions(Windows.Storage.Search.CommonFileQuery.DefaultQuery, new string[] { ".dat" });
            var queryResult = syncFolder.CreateFileQueryWithOptions(queryOptions);
            var fileList = await queryResult.GetFilesAsync();

            queryResult.ContentsChanged += QueryResult_ContentsChanged;

#if false
            byte[] buffer = Encoding.ASCII.GetBytes("first string before starting the process");
            IBuffer ibuffer = buffer.AsBuffer();
            await standardInput.WriteAsync(ibuffer);
#endif
            processLauncherResult = ProcessLauncher.RunToCompletionAsync("comm-proxy.exe", syncFolder.Path, processLauncherOptions);
        }

        private async void QueryResult_ContentsChanged(IStorageQueryResultBase sender, object args)
        {
            Debug.WriteLine("Event fired");
            // Cleanup all files in this directory:
            QueryOptions queryOptions = new QueryOptions(Windows.Storage.Search.CommonFileQuery.DefaultQuery, new string[] { ".dat" });
            var queryResult = sender.Folder.CreateFileQueryWithOptions(queryOptions);
            var fileList = await queryResult.GetFilesAsync();

            foreach (StorageFile file in fileList)
            {
                await file.DeleteAsync();
            }

            gotData.Set();
        }

        private async void button_Click_Start(object sender, RoutedEventArgs e)
        {
            StartProxyProcess();
            StartReceiver();
        }
    }
}
