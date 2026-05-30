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
    private readonly IRepository<Sys_LoginLog> _loginLogRepo;
    private readonly IRepository<Sys_AccountLock> _lockRepo;
    private readonly IRepository<Sys_Setting> _settingRepo;

    // 设置内存缓存（避免每请求查库）
    private DateTime _settingLoadedAt = DateTime.MinValue;
    private int _idleMinutes = 60;
    private int _failThreshold = 5;

    public AuthApp(IRepository<Sys_User> userRepo, ModuleApp moduleApp, ICacheContext cache,
        IRepository<Sys_LoginLog> loginLogRepo, IRepository<Sys_AccountLock> lockRepo, IRepository<Sys_Setting> settingRepo)
    {
        _userRepo = userRepo;
        _moduleApp = moduleApp;
        _cache = cache;
        _loginLogRepo = loginLogRepo;
        _lockRepo = lockRepo;
        _settingRepo = settingRepo;
    }

    /// <summary>读取安全设置（会话超时分钟 / 失败锁定阈值），内存缓存 60s</summary>
    private void EnsureSettings()
    {
        if ((DateTime.Now - _settingLoadedAt).TotalSeconds < 60) return;
        try
        {
            var idle = _settingRepo.FindSingle("[Key]=@k", new { k = "Security.SessionIdleMinutes" });
            if (idle != null && int.TryParse(idle.Value, out var m) && m > 0) _idleMinutes = m;
            var th = _settingRepo.FindSingle("[Key]=@k", new { k = "Security.LoginFailThreshold" });
            if (th != null && int.TryParse(th.Value, out var t) && t > 0) _failThreshold = t;
        }
        catch { /* 设置表缺失则用默认值 */ }
        _settingLoadedAt = DateTime.Now;
    }

    private TimeSpan TokenTtl { get { EnsureSettings(); return TimeSpan.FromMinutes(_idleMinutes); } }

    public bool CheckLogin(string token, string otherInfo = "")
    {
        if (string.IsNullOrEmpty(token)) return false;
        return _cache.Exists(TokenPrefix + token);
    }

    /// <summary>每次受保护请求续期，实现空闲会话超时（滑动过期）</summary>
    public void RenewToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return;
        var ctx = _cache.Get<AuthStrategyContext>(TokenPrefix + token);
        if (ctx != null) _cache.Set(TokenPrefix + token, ctx, TokenTtl);
    }

    private void WriteLoginLog(long? userId, string account, bool success, string? failReason, string? ip, string? ua)
    {
        try
        {
            _loginLogRepo.Insert(new Sys_LoginLog
            {
                UserId = userId,
                Account = account,
                LoginTime = DateTime.Now,
                IpAddress = ip,
                UserAgent = ua,
                Success = success,
                FailReason = failReason
            });
        }
        catch { /* 日志表缺失不阻断登录 */ }
    }

    /// <summary>返回 true 表示账户当前处于锁定状态</summary>
    private bool IsLocked(long userId)
    {
        try
        {
            var row = _lockRepo.FindSingle("[UserId]=@u", new { u = userId });
            return row != null && row.IsLocked;
        }
        catch { return false; }
    }

    private void RegisterFail(long userId, string account)
    {
        EnsureSettings();
        try
        {
            var row = _lockRepo.FindSingle("[UserId]=@u", new { u = userId });
            if (row == null)
            {
                _lockRepo.Insert(new Sys_AccountLock { UserId = userId, Account = account, FailCount = 1, IsLocked = false });
            }
            else
            {
                var count = row.FailCount + 1;
                var locked = count >= _failThreshold;
                _lockRepo.ExecuteSql(
                    "UPDATE [Sys_AccountLock] SET [FailCount]=@c,[IsLocked]=@l,[LockedAt]=CASE WHEN @l=1 THEN getdate() ELSE [LockedAt] END WHERE [Id]=@id",
                    new { c = count, l = locked, id = row.Id });
            }
        }
        catch { /* 锁定表缺失则不计数 */ }
    }

    private void ResetFail(long userId)
    {
        try { _lockRepo.ExecuteSql("UPDATE [Sys_AccountLock] SET [FailCount]=0,[IsLocked]=0 WHERE [UserId]=@u", new { u = userId }); }
        catch { }
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

    public LoginResult Login(string appKey, string username, string pwd, bool needEncrypt = true, string? ip = null, string? userAgent = null)
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
            WriteLoginLog(null, username, false, "account not exist", ip, userAgent);
            result.code = 401;
            result.msg = "Account does not exist.";
            return result;
        }

        // 账户锁定校验（URS 406）
        if (IsLocked(user.Id))
        {
            WriteLoginLog(user.Id, username, false, "account locked", ip, userAgent);
            result.code = 423;
            result.msg = "Account is locked. Please contact administrator.";
            return result;
        }

        var hashed = needEncrypt ? DesEncrypt.Md5(pwd) : pwd;
        if (!string.Equals(user.Password, hashed, StringComparison.Ordinal)
            && !string.Equals(user.Password, pwd, StringComparison.Ordinal))
        {
            RegisterFail(user.Id, username);
            WriteLoginLog(user.Id, username, false, "wrong password", ip, userAgent);
            result.code = 401;
            result.msg = "Wrong password.";
            return result;
        }

        ResetFail(user.Id);
        WriteLoginLog(user.Id, username, true, null, ip, userAgent);

        var token = Guid.NewGuid().ToString("N");
        var context = new AuthStrategyContext
        {
            User = user,
            Modules = _moduleApp.GetModulesByUser(user.Id),
            ModuleElements = _moduleApp.GetButtonsByUser(user.Id)
        };
        _cache.Set(TokenPrefix + token, context, TokenTtl);
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
        _cache.Set(TokenPrefix + token, ctx, TokenTtl);

        result.code = 200;
        result.msg = "ok";
        result.success = true;
        return result;
    }
}
