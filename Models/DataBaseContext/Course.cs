using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// 课程详情表
/// </summary>
public partial class Course
{
    /// <summary>
    /// 课程表ID
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 课表ID
    /// </summary>
    public ulong ScheduleId { get; set; }

    /// <summary>
    /// 课程名
    /// </summary>
    public string CourseName { get; set; } = null!;

    /// <summary>
    /// 课程颜色
    /// </summary>
    public string Color { get; set; } = null!;

    /// <summary>
    /// 课程时间段
    /// </summary>
    public string TimeSlots { get; set; } = null!;

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
