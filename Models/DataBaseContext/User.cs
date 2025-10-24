using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// 用户表
/// </summary>
public partial class User
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// RainbowID
    /// </summary>
    public string RainbowId { get; set; } = null!;

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 密码(加密存储)
    /// </summary>
    public string UserPassword { get; set; } = null!;

    /// <summary>
    /// 手机号
    /// </summary>
    public string? UserPhone { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string UserEmail { get; set; } = null!;

    /// <summary>
    /// 头像
    /// </summary>
    public string? UserImg { get; set; }

    /// <summary>
    /// 权限等级: v1-普通用户 admin-管理员
    /// </summary>
    public string PermissionLevel { get; set; } = null!;

    /// <summary>
    /// 密保问题
    /// </summary>
    public string? SecurityQuestion { get; set; }

    /// <summary>
    /// 密保答案
    /// </summary>
    public string? SecurityAnswer { get; set; }

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

    public virtual ICollection<BlogTag> BlogTags { get; set; } = new List<BlogTag>();

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<LoginSession> LoginSessions { get; set; } = new List<LoginSession>();

    public virtual UserProfile? UserProfile { get; set; }
}
