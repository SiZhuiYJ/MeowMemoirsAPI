using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// 系统黑名单表
/// </summary>
public partial class Blacklist
{
    public int BlacklistId { get; set; }

    /// <summary>
    /// 黑名单类型：token/用户/IP
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// 存储的具体值（Token字符串/用户ID/IP地址）
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// 加入黑名单的原因
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 过期时间（可选，永久黑名单可为NULL）
    /// </summary>
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 加入黑名单时间
    /// </summary>
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// 操作管理员ID
    /// </summary>
    public int? AdminId { get; set; }
}
