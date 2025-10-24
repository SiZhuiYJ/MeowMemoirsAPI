using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// 博客标签表
/// </summary>
public partial class BlogTag
{
    /// <summary>
    /// 标签ID
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 创建者ID
    /// </summary>
    public ulong UserId { get; set; }

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
    /// 0-禁用 1-启用
    /// </summary>
    public sbyte TagStatus { get; set; }

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
