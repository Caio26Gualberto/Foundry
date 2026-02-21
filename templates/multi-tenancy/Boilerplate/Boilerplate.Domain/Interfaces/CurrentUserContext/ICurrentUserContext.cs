namespace Boilerplate.Application.Interfaces.ICurrentUserContext
{
    public interface ICurrentUserContext
    {
        int UserId { get; }
        int? TenantId { get; }
        bool IsAuthenticated { get; }
        string Email { get; }
        bool IsImpersonating { get; }
        int? ImpersonatedBy { get; }
    }
}
