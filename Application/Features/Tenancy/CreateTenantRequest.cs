namespace Application.Features.Tenancy
{
    public class CreateTenantRequest
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsActive { get; set; }
    }
}
