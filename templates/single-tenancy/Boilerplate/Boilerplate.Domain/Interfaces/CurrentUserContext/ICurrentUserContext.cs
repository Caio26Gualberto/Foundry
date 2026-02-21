namespace Boilerplate.Application.Interfaces.ICurrentUserContext
{
    public interface ICurrentUserContext
    {
        int UserId { get; }
        bool IsAuthenticated { get; }
        string Email { get; }
    }
}
