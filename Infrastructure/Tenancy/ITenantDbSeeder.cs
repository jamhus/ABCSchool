namespace Infrastructure.Tenancy
{
    internal interface ITenantDbSeeder
    {
        Task InitializeDatabaseAsync(CancellationToken ct);
    }
}
