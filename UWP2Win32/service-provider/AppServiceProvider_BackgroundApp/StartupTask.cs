using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.ApplicationModel;

namespace AppServiceProvider_BackgroundApp
{
    public sealed class NumerAdderTask : IBackgroundTask
    {
        BackgroundTaskDeferral serviceDeferral;
        AppServiceConnection connection;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var pkgFamilyName = Package.Current.Id.FamilyName;
            System.Diagnostics.Debug.WriteLine(pkgFamilyName);

            //Take a service deferral so the service isn't terminated
            serviceDeferral = taskInstance.GetDeferral();

            taskInstance.Canceled += OnTaskCanceled;

            if (taskInstance.TriggerDetails != null)
            {

                var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
                connection = details.AppServiceConnection;

                //Listen for incoming app service requests
                connection.RequestReceived += OnRequestReceived;
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (serviceDeferral != null)
            {
                //Complete the service deferral
                serviceDeferral.Complete();
                serviceDeferral = null;
            }
        }

        async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            //Get a deferral so we can use an awaitable API to respond to the message
            var messageDeferral = args.GetDeferral();

            try
            {
                var input = args.Request.Message;
                int minValue = (int)input["value1"];
                int maxValue = (int)input["value2"];

                //Create the response
                var result = new ValueSet();
                result.Add("sum", 1000 + minValue + maxValue);

                //Send the response
                await args.Request.SendResponseAsync(result);

            }
            finally
            {
                //Complete the message deferral so the platform knows we're done responding
                messageDeferral.Complete();
            }
        }
    }
}
