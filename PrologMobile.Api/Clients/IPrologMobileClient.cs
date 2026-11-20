using PrologMobile.Api.Models;

namespace PrologMobile.Api.Clients;

public interface IPrologMobileClient
{
    Task<InventoryResponse> GetInventoryAsync(
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default);
}
