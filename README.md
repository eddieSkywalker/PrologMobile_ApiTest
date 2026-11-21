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
   dotnet run --project .\PrologMobile.Api
   ```

   At startup, the app runs and calls API to retrieve first page of orders as well as grabs metadata to determine total orders and pages needed to loop through. It logs each successful JSON response payload and the category summary in an indented format. If retrieval fails or the responses are inconsistent, it logs the error and remains running.

## API authentication

The API key is sent in the `X-API-Key` request header.

## Rate limiting

When the Inventory API returns HTTP 429, the client retries that page up to four times. It waits for the `Retry-After` header when present; otherwise, it waits 1, 2, 3, and 4 seconds between retries. If the retries are exhausted, it logs that it could not retrieve and summarize all records, and the app continues starting without a summary.

## Architecture and assumptions

- `InventoryApiClient` requests one page at a time; `InventoryService` retrieves and checks the pages; `InventorySummarizer` calculates the results without making HTTP calls.
- The API describes a static dataset. Page metadata must agree across responses, and the final unique record count must match the reported total. Identical repeated records are counted once; conflicting records with the same ID cause retrieval to fail. These checks cannot detect records omitted consistently from all reported totals.
- Records with zero quantity are excluded. Category names are case-sensitive. If records share the latest update time, the record with the lowest ID is selected.
- The summary includes the full latest record and full contributing records, including their category.

## Known limitations

- The required HTTP endpoint for returning the summary has not been implemented. Results are currently logged at startup.
- The repository does not yet include unit tests.
