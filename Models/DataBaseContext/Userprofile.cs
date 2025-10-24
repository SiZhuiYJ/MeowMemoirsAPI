using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// 用户资料表
/// </summary>
public partial class UserProfile
{
    /// <summary>
    /// 用户资料ID
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateOnly? Birthday { get; set; }

    /// <summary>
    /// 0-女 1-男 2-未知
    /// </summary>
    public sbyte Gender { get; set; }

    /// <summary>
    /// 用户经度
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// 用户纬度
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// 用户地址
    /// </summary>
    public string? Address { get; set; }

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

    public virtual User User { get; set; } = null!;
}
