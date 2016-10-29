using System;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;

namespace AppServiceProvider
{
    public interface IDataReceivedAction
    {
        void OnDataReceived(string data);
    }

    public sealed class EchoService : IBackgroundTask
    {
        BackgroundTaskDeferral serviceDeferral;
        AppServiceConnection connection;

        static IDataReceivedAction s_pingBack;

        public static void SetPingBack(IDataReceivedAction pingBack)
        {
            s_pingBack = pingBack;
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Take a service deferral so the service isn't terminated
            serviceDeferral = taskInstance.GetDeferral();

            taskInstance.Canceled += OnTaskCanceled;

            var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            connection = details.AppServiceConnection;
            // Listen for incoming app service requests
            connection.RequestReceived += OnRequestReceived;
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (serviceDeferral != null)
            {
                // Complete the service deferral
                serviceDeferral.Complete();
                serviceDeferral = null;
            }
        }

        async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // Get a deferral so we can use an awaitable API to respond to the message
            var messageDeferral = args.GetDeferral();

            try
            {
                var input = args.Request.Message;

                var inputData = (string)input["input"];

                s_pingBack.OnDataReceived(inputData);

                var result = new ValueSet();
                result.Add("ack", "OK");

                // Send the response
                await args.Request.SendResponseAsync(result);

            }
            finally
            {
                // Complete the message deferral so the platform knows we're done responding
                messageDeferral.Complete();
            }
        }
    }
}
