namespace YouthRetreatRegistration.Services;

public interface IAdminAuthService
{
    bool IsAuthenticated { get; }
    bool Login(string username, string password);
    void Logout();
}
