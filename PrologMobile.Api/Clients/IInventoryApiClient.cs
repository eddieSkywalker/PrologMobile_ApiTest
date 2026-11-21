using PrologMobile.Api.Models;

namespace PrologMobile.Api.Clients;

public interface IInventoryApiClient
{
    Task<InventoryResponse> GetInventoryAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
