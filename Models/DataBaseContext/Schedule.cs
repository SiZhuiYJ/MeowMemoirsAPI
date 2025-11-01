using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// 课表主表
/// </summary>
public partial class Schedule
{
    /// <summary>
    /// 课表ID
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    /// 课程名
    /// </summary>
    public string ScheduleName { get; set; } = null!;

    /// <summary>
    /// 开课时间
    /// </summary>
    public string StartTime { get; set; } = null!;

    /// <summary>
    /// 本学期周数
    /// </summary>
    public int WeekCount { get; set; }

    /// <summary>
    /// 作息表
    /// </summary>
    public string Timetable { get; set; } = null!;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 0-正常 1-删除
    /// </summary>
    public sbyte IsDeleted { get; set; }

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
