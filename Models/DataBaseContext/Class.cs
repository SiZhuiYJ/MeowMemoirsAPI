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
    public int Id { get; set; }

    public int UserId { get; set; }

    /// <summary>
    /// 课程名
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 地点
    /// </summary>
    public string Location { get; set; } = null!;

    /// <summary>
    /// 周几（0-6）
    /// </summary>
    public sbyte DayOfWeek { get; set; }

    /// <summary>
    /// 周数（JSON数组）
    /// </summary>
    public string Week { get; set; } = null!;

    /// <summary>
    /// 节次（JSON数组）
    /// </summary>
    public string Number { get; set; } = null!;

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

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
