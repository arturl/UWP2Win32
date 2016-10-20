#include "stdafx.h"

using namespace concurrency;
using namespace Windows::ApplicationModel::AppService;

wchar_t* AppServiceResponseStatusString(AppServiceResponseStatus status)
{
    switch (status)
    {
    case AppServiceResponseStatus::Failure:
        return L"Failure: The service failed to acknowledge the message we sent it. It may have been terminated because the client was suspended.";

    case AppServiceResponseStatus::ResourceLimitsExceeded:
        return L"ResourceLimitsExceeded: The service exceeded the resources allocated to it and had to be terminated.";

    case AppServiceResponseStatus::Unknown:
    default:
        return L"Unknown: An unkown error occurred while we were trying to send a message to the service.";
    }
}

wchar_t* AppServiceConnectionStatusString(AppServiceConnectionStatus status)
{
    switch (status)
    {
    case AppServiceConnectionStatus::Success:
        return L"Success";
    case AppServiceConnectionStatus::AppNotInstalled:
        return L"AppNotInstalled";
    case AppServiceConnectionStatus::AppUnavailable:
        return L"AppUnavailable";
    case AppServiceConnectionStatus::AppServiceUnavailable:
        return L"AppServiceUnavailable";
    case AppServiceConnectionStatus::Unknown:
        return L"Unknown";
    case AppServiceConnectionStatus::RemoteSystemUnavailable:
        return L"RemoteSystemUnavailable";
    case AppServiceConnectionStatus::RemoteSystemNotSupportedByApp:
        return L"RemoteSystemNotSupportedByApp";
    case AppServiceConnectionStatus::NotAuthorized:
        return L"NotAuthorized";
    default:
        return L"??";
    }
}
void sleep_visually(int seconds)
{
    for (int i = 0; i < seconds; i++)
    {
        if(i % 10 == 0)
            wprintf(L"*");
        else
            wprintf(L".");
        Sleep(1000);
    }
    wprintf(L"\n");
}

const int sleep_after_connection = 30; // seconds
const int sleep_after_completion = 1; // seconds

int main(Platform::Array<Platform::String^>^ args)
{
    std::random_device r;

    while (1)
    {
        Windows::ApplicationModel::AppService::AppServiceConnection^ newConnection = ref new Windows::ApplicationModel::AppService::AppServiceConnection();
#if true
        newConnection->AppServiceName = L"com.microsoft.numbercruncher2";
        newConnection->PackageFamilyName = L"AppServiceProviderBackgroundApp4-uwp_55b230cc6y9ay";
#else
        newConnection->AppServiceName = L"com.microsoft.numbercruncher";
        newConnection->PackageFamilyName = L"Microsoft.AppServicesProvider.NumberCruncher_9nthh9tntkkay";
#endif

        create_task(newConnection->OpenAsync()).then([&](task<Windows::ApplicationModel::AppService::AppServiceConnectionStatus> statusTask) {
            auto status = statusTask.get();
            if (status != Windows::ApplicationModel::AppService::AppServiceConnectionStatus::Success)
            {
                wprintf(L"Error '%s'\n", AppServiceConnectionStatusString(status));
                return task_from_result();
            }

            sleep_visually(sleep_after_connection);

            auto inputs = ref new Windows::Foundation::Collections::ValueSet();
            int min = 0;
            int max = 100;

            std::default_random_engine e1(r());
            std::uniform_int_distribution<int> uniform_dist(min, max);
            int val1 = uniform_dist(e1);
            int val2 = uniform_dist(e1);

            wprintf(L"%d + %d = ", val1, val2);

            inputs->Insert(L"value1", val1);
            inputs->Insert(L"value2", val2);

            return create_task(newConnection->SendMessageAsync(inputs)).then([](task<Windows::ApplicationModel::AppService::AppServiceResponse^> responseTask) {
                try
                {
                    auto response = responseTask.get();
                    auto status = response->Status;
                    if (status == Windows::ApplicationModel::AppService::AppServiceResponseStatus::Success)
                    {
                        auto resultText = response->Message->Lookup(L"sum");
                        wprintf(L"%s\n", resultText->ToString()->Data());
                    }
                    else
                    {
                        wprintf(L"Error: '%s'\n", AppServiceResponseStatusString(status));
                    }
                }
                catch (...)
                {
                    wprintf(L"Error: responseTask threw an exception\n");
                }
            });
        }).wait();

        sleep_visually(sleep_after_completion);
    }

    return 0;
}

