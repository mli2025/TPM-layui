using DeviceMgmt.App.AuthStrategies;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using Infrastructure.Cache;
using Infrastructure.DEncrypt;

namespace DeviceMgmt.App.Apps.System;

public class AuthApp : IAuth
{
    private const string TokenPrefix = "DeviceMgmt:Token:";
    private readonly IRepository<Sys_User> _userRepo;
    private readonly ModuleApp _moduleApp;
    private readonly ICacheContext _cache;

    public AuthApp(IRepository<Sys_User> userRepo, ModuleApp moduleApp, ICacheContext cache)
    {
        _userRepo = userRepo;
        _moduleApp = moduleApp;
        _cache = cache;
    }

    public bool CheckLogin(string token, string otherInfo = "")
    {
        if (string.IsNullOrEmpty(token)) return false;
        return _cache.Exists(TokenPrefix + token);
    }

    public AuthStrategyContext? GetCurrentUser(string otherInfo = "")
    {
        var token = otherInfo;
        if (string.IsNullOrEmpty(token)) return null;
        return _cache.Get<AuthStrategyContext>(TokenPrefix + token);
    }

    public string GetUserName(string otherInfo = "")
    {
        var ctx = GetCurrentUser(otherInfo);
        return ctx?.User.Account ?? string.Empty;
    }

    public LoginResult Login(string appKey, string username, string pwd, bool needEncrypt = true)
    {
        var result = new LoginResult();
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(pwd))
        {
            result.code = 400;
            result.msg = "Account or password is required.";
            return result;
        }

        var user = _userRepo.FindSingle("[Account]=@a", new { a = username });
        if (user == null)
        {
            result.code = 401;
            result.msg = "Account does not exist.";
            return result;
        }
        var hashed = needEncrypt ? DesEncrypt.Md5(pwd) : pwd;
        if (!string.Equals(user.Password, hashed, StringComparison.Ordinal)
            && !string.Equals(user.Password, pwd, StringComparison.Ordinal))
        {
            result.code = 401;
            result.msg = "Wrong password.";
            return result;
        }

        var token = Guid.NewGuid().ToString("N");
        var context = new AuthStrategyContext
        {
            User = user,
            Modules = _moduleApp.GetModulesByUser(user.Id),
            ModuleElements = _moduleApp.GetButtonsByUser(user.Id)
        };
        _cache.Set(TokenPrefix + token, context, TimeSpan.FromHours(8));
        result.Token = token;
        result.success = true;
        return result;
    }

    public bool Logout(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        _cache.Remove(TokenPrefix + token);
        return true;
    }

    public LoginResult ChangePassword(string token, string oldPassword, string newPassword)
    {
        var result = new LoginResult();
        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
        {
            result.code = 400;
            result.msg = "旧密码和新密码不能为空";
            return result;
        }
        if (newPassword.Length < 6)
        {
            result.code = 400;
            result.msg = "新密码至少 6 位";
            return result;
        }

        var ctx = GetCurrentUser(token);
        if (ctx == null)
        {
            result.code = 401;
            result.msg = "登录已过期，请重新登录";
            return result;
        }

        var user = _userRepo.FindSingle("[Id]=@id", new { id = ctx.User.Id });
        if (user == null)
        {
            result.code = 404;
            result.msg = "账号不存在";
            return result;
        }

        var oldHash = DesEncrypt.Md5(oldPassword);
        if (!string.Equals(user.Password, oldHash, StringComparison.Ordinal)
            && !string.Equals(user.Password, oldPassword, StringComparison.Ordinal))
        {
            result.code = 401;
            result.msg = "旧密码不正确";
            return result;
        }

        var newHash = DesEncrypt.Md5(newPassword);
        _userRepo.ExecuteSql("UPDATE [Sys_User] SET [Password]=@p WHERE [Id]=@id",
            new { p = newHash, id = user.Id });

        user.Password = newHash;
        ctx.User = user;
        _cache.Set(TokenPrefix + token, ctx, TimeSpan.FromHours(8));

        result.code = 200;
        result.msg = "ok";
        result.success = true;
        return result;
    }
}
