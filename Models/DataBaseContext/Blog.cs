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
    public int Id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 博客标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 博客封面内容（100字以内）
    /// </summary>
    public string? CoverContent { get; set; }

    /// <summary>
    /// 博客内容
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 博客标签（多个标签用逗号分隔，最多10个）
    /// </summary>
    public string? Tags { get; set; }

    public virtual User User { get; set; } = null!;
}
