#include "stdafx.h"

#define BUFSIZE 4096

int main(int argc, char **argv)
{
    printf("Hi there!\n");

    char buffer[BUFSIZE];

    auto stdinHandle = GetStdHandle(STD_INPUT_HANDLE);

    while (1)
    {
        DWORD dwRead;
        BOOL bSuccess = ReadFile(stdinHandle, buffer, BUFSIZE, &dwRead, NULL);

        if (!bSuccess || dwRead == 0)
        {
            printf("end of stream! bSuccess=%d, dwRead=%d\n", bSuccess, dwRead);
            _flushall();
            break;
        }
        else
        {
            Sleep(10*1000);
            buffer[dwRead] = '\0';
            printf("echo: %s\n", buffer);
            _flushall();
        }
    }

    printf("finished, hit enter: ");
    _getch();

    return 0;
}

