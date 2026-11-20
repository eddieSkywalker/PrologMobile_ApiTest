# PrologMobile_ApiTest

## Setup instructions

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
2. Create `PrologMobile.Api/appsettings.Development.json` with your Inventory API settings:

   ```json
   {
     "ApiCredentials": {
       "BaseUrl": "https://your-api-host",
       "ApiKey": "your-api-key"
     },
     "InventoryPagination": {
       "Page": 1,
       "PageSize": 20
     }
   }
   ```

   Replace the placeholders with the API's host URL and your key. The client requests `/api/records` from that host. `PageSize` supports values from 1 to 25 according to the API documentation.

   The tracked `appsettings.json` contains shared or non-sensitive settings. Environment-specific settings override matching values in that file; the included launch profiles select `Development`. Environment-specific appsettings files are Git-ignored. Keep real API keys out of source control, including the tracked `appsettings.json`.

3. From the repository root, run:

   ```powershell
   dotnet run
   ```

   At startup, the app requests one page and logs the number of records received. If the request fails, it logs the error and remains running.

## API authentication

The API key is sent in the `X-API-Key` request header.
