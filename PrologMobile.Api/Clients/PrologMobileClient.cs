using PrologMobile.Api.Models;

namespace PrologMobile.Api.Clients;

/// <summary>
/// Client for interacting with the Prolog Mobile API.
/// </summary>
public class PrologMobileClient : IPrologMobileClient
{
    private readonly HttpClient _httpClient;

    public Task<InventoryResponse> GetInventoryAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}