#include "stdafx.h"

uint32_t send_data_to_admin(char* request, uint32_t request_len, char* response, uint32_t max_response_len)
{
    HANDLE hPipe;
    DWORD dwWritten;

    hPipe = CreateFile(TEXT("\\\\.\\pipe\\ztp-client-pipe"),
        GENERIC_READ | GENERIC_WRITE,
        0,
        NULL,
        OPEN_EXISTING,
        0,
        NULL);
    if (hPipe != INVALID_HANDLE_VALUE)
    {
        WriteFile(hPipe,
            request,
            request_len,
            &dwWritten,
            NULL);

        printf("written %d bytes\n", dwWritten);

        DWORD dwRead = 0;
        if(ReadFile(hPipe, response, max_response_len, &dwRead, NULL) != FALSE)
        {
            printf("received response %d bytes\n", dwRead);
        }

        CloseHandle(hPipe);

        return dwRead;
    }
    else
    {
        printf("Error %d\n", GetLastError());
    }

    return (0);
}
