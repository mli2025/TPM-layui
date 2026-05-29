using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.System;

/// <summary>
/// 通知中心：站内消息查询 + 已读标记。发送/分发由通知引擎 + n8n 负责。
/// </summary>
public class NotifyApp : BaseApp<Sys_NotifyRecord>
{
    public NotifyApp(IUnitWork unitWork, IRepository<Sys_NotifyRecord> repository)
        : base(unitWork, repository)
    {
    }

    public int UnreadCount(long userId)
        => Repository.Count("[ReceiverId]=@u AND [IsRead]=0", new { u = userId });

    public void MarkRead(long id)
        => Repository.ExecuteSql(
            "UPDATE [Sys_NotifyRecord] SET [IsRead]=1,[ReadTime]=getdate() WHERE [Id]=@id AND [IsRead]=0",
            new { id });

    public void MarkAllRead(long userId)
        => Repository.ExecuteSql(
            "UPDATE [Sys_NotifyRecord] SET [IsRead]=1,[ReadTime]=getdate() WHERE [ReceiverId]=@u AND [IsRead]=0",
            new { u = userId });
}
