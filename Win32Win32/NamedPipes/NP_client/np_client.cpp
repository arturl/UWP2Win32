#include "stdafx.h"

int main(int argc, char** argv)
{
    char* text;
    if (argc != 2)
    {
        text = "<data>";
    }
    else
    {
        text = argv[1];
    }
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
            text,
            strlen(text),
            &dwWritten,
            NULL);

        printf("written %d bytes\n", dwWritten);

        char buffer[1024];
        DWORD dwRead;
        while (ReadFile(hPipe, buffer, sizeof(buffer) - 1, &dwRead, NULL) != FALSE)
        {
            /* add terminating zero */
            buffer[dwRead] = '\0';

            /* do something with data in buffer */
            printf("received response '%s'\n", buffer);
        }

        CloseHandle(hPipe);
    }
    else
    {
        printf("Error %d\n", GetLastError());
    }

    return (0);
}
