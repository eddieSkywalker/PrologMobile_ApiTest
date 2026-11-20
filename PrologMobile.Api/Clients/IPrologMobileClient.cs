using PrologMobile.Api.Models;

namespace PrologMobile.Api.Clients;

public interface IPrologMobileClient
{
    Task<InventoryResponse> GetInventoryAsync(CancellationToken cancellationToken = default);
}
