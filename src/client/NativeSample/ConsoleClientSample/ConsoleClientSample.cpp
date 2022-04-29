
#include <stdlib.h>
#include <stdio.h>

extern char* GetAccessToken();

int main()
{
    printf("Acquiring Access Token...\n");

    char* accessToken = GetAccessToken();

    printf("Access Token: %s\n", accessToken);
}
