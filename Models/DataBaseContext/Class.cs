using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// 课程表
/// </summary>
public partial class Class
{
    /// <summary>
    /// 课程ID
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    /// 课程名
    /// </summary>
    public string ClassName { get; set; } = null!;

    /// <summary>
    /// 地点
    /// </summary>
    public string Location { get; set; } = null!;

    /// <summary>
    /// 周几(1-7)
    /// </summary>
    public byte DayOfWeek { get; set; }

    /// <summary>
    /// 周数(JSON数组)
    /// </summary>
    public string WeekList { get; set; } = null!;

    /// <summary>
    /// 节次(JSON数组)
    /// </summary>
    public string SessionList { get; set; } = null!;

    /// <summary>
    /// 教师
    /// </summary>
    public string Teacher { get; set; } = null!;

    /// <summary>
    /// 颜色
    /// </summary>
    public string Color { get; set; } = null!;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

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
