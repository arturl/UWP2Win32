#include "stdafx.h"

#define BUFSIZE 4096

char buffer[BUFSIZE];

void signal_data_ready(char* directory)
{
    if (directory == nullptr) return;
    char fullfile[MAX_PATH] = { 0 };
    strcat_s(fullfile, directory);
    strcat_s(fullfile, "\\sync.dat");
    FILE *fp = fopen(fullfile, "w+");
    fclose(fp);
}

int main(int argc, char **argv)
{
    auto stdinHandle = GetStdHandle(STD_INPUT_HANDLE);

    char* signal_directory = nullptr;

    if (argc == 2)
    {
        signal_directory = argv[1];
    }

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
            // Sleep(10*1000);
            buffer[dwRead] = '\0';
            printf("echo: %s\n", buffer);
            _flushall();
            signal_data_ready(signal_directory);
        }
    }

    printf("finished, hit enter: ");
    _getch();

    return 0;
}

