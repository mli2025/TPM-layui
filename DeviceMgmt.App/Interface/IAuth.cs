using DeviceMgmt.App.AuthStrategies;
using DeviceMgmt.App.Response;

namespace DeviceMgmt.App.Interface;

public interface IAuth
{
    bool CheckLogin(string token, string otherInfo = "");
    void RenewToken(string token);
    AuthStrategyContext? GetCurrentUser(string otherInfo = "");
    string GetUserName(string otherInfo = "");
    LoginResult Login(string appKey, string username, string pwd, bool needEncrypt = true, string? ip = null, string? userAgent = null);
    bool Logout(string token);
    LoginResult ChangePassword(string token, string oldPassword, string newPassword);
}
