using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// 用户登录会话表
/// </summary>
public partial class LoginSession
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; set; } = null!;

    /// <summary>
    /// 令牌到期时间
    /// </summary>
    public DateTime ExpireTime { get; set; }

    /// <summary>
    /// 设备信息
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// 登录IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 0-正常 1-删除
    /// </summary>
    public sbyte IsDeleted { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// 扩展字段1
    /// </summary>
    public string? ExtAttr1 { get; set; }

    /// <summary>
    /// 扩展字段2
    /// </summary>
    public string? ExtAttr2 { get; set; }

    /// <summary>
    /// 扩展字段3
    /// </summary>
    public string? ExtAttr3 { get; set; }

    public virtual User User { get; set; } = null!;
}
