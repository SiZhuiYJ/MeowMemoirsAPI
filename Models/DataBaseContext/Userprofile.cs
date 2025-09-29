using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// 用户资料表
/// </summary>
public partial class Userprofile
{
    /// <summary>
    /// 用户资料ID
    /// </summary>
    public int ProfileId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateOnly? Birthday { get; set; }

    /// <summary>
    /// 性别：0-女，1-男，2-未知
    /// </summary>
    public int? Sex { get; set; }

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
    /// 创建时间
    /// </summary>
    public DateTime? DateTime { get; set; }

    public virtual User User { get; set; } = null!;
}
