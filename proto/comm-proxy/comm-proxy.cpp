#include "stdafx.h"
#include "app-service-client.h"

#define BUFSIZE 4096

char buffer[BUFSIZE];
char buffer_from_admin[BUFSIZE];

wchar_t wbuffer[BUFSIZE];

uint32_t send_data_to_admin(char* request, uint32_t request_len, char* response, uint32_t max_response_len);

int main(Platform::Array<Platform::String^>^ args)
{
    auto stdinHandle = GetStdHandle(STD_INPUT_HANDLE);

    while (1)
    {
        DWORD read_from_stdin;
        BOOL bSuccess = ReadFile(stdinHandle, buffer, BUFSIZE, &read_from_stdin, NULL);

        if (!bSuccess || read_from_stdin == 0)
        {
            printf("end of stream! bSuccess=%d, dwRead=%d\n", bSuccess, read_from_stdin);
            _flushall();
            break;
        }
        else
        {
            buffer[read_from_stdin] = '\0';
            printf("received: %s\n", buffer);
            _flushall();

            uint32 read_from_admin = send_data_to_admin(buffer, read_from_stdin, buffer_from_admin, BUFSIZE);

            int chars_converted = MultiByteToWideChar(CP_ACP, 0, buffer_from_admin, read_from_admin, wbuffer, BUFSIZE);

            send_data_to_UWP_app(wbuffer, chars_converted, L"com.microsoft.echo", L"Microsoft.AppServicesProvider.EchoService_8wekyb3d8bbwe");
        }
    }
}

