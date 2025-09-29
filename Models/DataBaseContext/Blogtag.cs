using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// 博客标签表
/// </summary>
public partial class Blogtag
{
    /// <summary>
    /// 标签ID
    /// </summary>
    public int TagId { get; set; }

    /// <summary>
    /// 创建者ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 标签名称
    /// </summary>
    public string TagName { get; set; } = null!;

    /// <summary>
    /// 标签颜色
    /// </summary>
    public string TagColor { get; set; } = null!;

    /// <summary>
    /// 标签图标
    /// </summary>
    public string TagIcon { get; set; } = null!;

    /// <summary>
    /// 标签描述
    /// </summary>
    public string TagDescription { get; set; } = null!;

    /// <summary>
    /// 标签状态
    /// </summary>
    public int TagStatus { get; set; }

    /// <summary>
    /// 标签创建时间
    /// </summary>
    public DateTime TagCreateTime { get; set; }

    public virtual User User { get; set; } = null!;
}
