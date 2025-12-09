namespace Application.Features.Tenancy
{
    public interface ItenantService
    {
        Task<string> CreateTenantAsync(CreateTenantRequest createTenant, CancellationToken ct); 
        Task<string> ActivateAsync(string id);
        Task<string> DeactivateAsync(string id);
        Task<string> UpdateSubscriptionAsync(string id, DateTime newExpiryDate);
    }
}
