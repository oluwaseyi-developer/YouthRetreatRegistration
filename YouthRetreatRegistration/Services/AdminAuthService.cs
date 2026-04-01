namespace YouthRetreatRegistration.Services;

public sealed class AdminAuthService : IAdminAuthService
{
    private const string ValidUsername = "YouthAdmin";
    private const string ValidPassword = "Ikotunegbe";

    public bool IsAuthenticated { get; private set; }

    public bool Login(string username, string password)
    {
        if (string.Equals(username, ValidUsername, StringComparison.Ordinal)
            && string.Equals(password, ValidPassword, StringComparison.Ordinal))
        {
            IsAuthenticated = true;
            return true;
        }

        return false;
    }

    public void Logout()
    {
        IsAuthenticated = false;
    }
}
