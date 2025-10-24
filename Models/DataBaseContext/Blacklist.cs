using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// 系统黑名单表
/// </summary>
public partial class Blacklist
{
    /// <summary>
    /// 黑名单ID
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 类型:token/user/ip
    /// </summary>
    public string BlacklistType { get; set; } = null!;

    /// <summary>
    /// 具体值
    /// </summary>
    public string BlacklistValue { get; set; } = null!;

    /// <summary>
    /// 加入原因
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 操作管理员ID
    /// </summary>
    public ulong? AdminId { get; set; }

    /// <summary>
    /// 0-正常 1-删除
    /// </summary>
    public sbyte IsDeleted { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 更新时间
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
}
