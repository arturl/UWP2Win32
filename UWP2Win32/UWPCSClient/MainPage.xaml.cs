using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPCSClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            ConsumeService();
        }

        const int sleep_after_connection = 45; // seconds
        const int sleep_after_completion = 1; // seconds

        private async Task ConsumeService()
        {
            while (true)
            {
                Windows.ApplicationModel.AppService.AppServiceConnection newConnection = new Windows.ApplicationModel.AppService.AppServiceConnection();
#if false
                newConnection.AppServiceName = "com.microsoft.numbercruncher2";
                newConnection.PackageFamilyName = "AppServiceProviderBackgroundApp4-uwp_55b230cc6y9ay";
#else
                newConnection.AppServiceName = "com.microsoft.numbercruncher";
                newConnection.PackageFamilyName = "Microsoft.AppServicesProvider.NumberCruncher_9nthh9tntkkay";
#endif
                var status = await newConnection.OpenAsync();

                if (status != Windows.ApplicationModel.AppService.AppServiceConnectionStatus.Success)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Error {0}", status));
                    return;
                }

                await Task.Delay(sleep_after_connection*1000);

                var inputs = new Windows.Foundation.Collections.ValueSet();
                int min = 0;
                int max = 100;

                Random random = new Random();
                int val1 = random.Next(min, max);
                int val2 = random.Next(min, max);

                System.Diagnostics.Debug.Write(string.Format("{0} + {1} = ", val1, val2));

                inputs.Add("value1", val1);
                inputs.Add("value2", val2);

                var response = await newConnection.SendMessageAsync(inputs);

                if (response.Status == Windows.ApplicationModel.AppService.AppServiceResponseStatus.Success)
                {
                    var resultText = response.Message["sum"];
                    System.Diagnostics.Debug.WriteLine(string.Format("{0}", resultText));

                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                        () =>
                        {
                            textBlock.Text = string.Format("[{3}]: {0} + {1} = {2}", val1, val2, resultText, DateTime.Now.ToString("HH:mm:ss"));
                        });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Error {0}", response.Status));
                }

                await Task.Delay(sleep_after_completion * 1000);
            }
        }
    }
}
