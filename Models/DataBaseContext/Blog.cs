using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// 博客表
/// </summary>
public partial class Blog
{
    /// <summary>
    /// 博客ID
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    /// 博客标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 博客摘要内容
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 博客内容
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// 博客标签(JSON格式)
    /// </summary>
    public string Tags { get; set; } = null!;

    /// <summary>
    /// 浏览量
    /// </summary>
    public uint ViewCount { get; set; }

    /// <summary>
    /// 点赞数
    /// </summary>
    public uint LikeCount { get; set; }

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
