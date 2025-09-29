using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// 用户登录会话表
/// </summary>
public partial class Loginsession
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public int SessionId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Token到期时间
    /// </summary>
    public DateTime ExpireTime { get; set; }

    /// <summary>
    /// 长Token（刷新Token）
    /// </summary>
    public string RefreshToken { get; set; } = null!;

    /// <summary>
    /// 会话创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTime? LastActivity { get; set; }

    /// <summary>
    /// 设备信息
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// 登录IP地址
    /// </summary>
    public string? Ip { get; set; }

    public virtual User User { get; set; } = null!;
}
