#include "stdafx.h"

int main()
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
            "Hello Pipe\n",
            12,   // = length of string + terminating '\0' !!!
            &dwWritten,
            NULL);

        printf("written %d bytes\n", dwWritten);

        CloseHandle(hPipe);
    }
    else
    {
        printf("Error %d\n", GetLastError());
    }

    return (0);
}
