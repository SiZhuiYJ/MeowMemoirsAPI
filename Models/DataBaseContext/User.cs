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
    public int UserId { get; set; }

    /// <summary>
    /// RainbowID
    /// </summary>
    public string RainbowId { get; set; } = null!;

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 密码
    /// </summary>
    public string UserPwd { get; set; } = null!;

    /// <summary>
    /// 电话
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
    /// 权限等级
    /// </summary>
    public string Permissions { get; set; } = null!;

    /// <summary>
    /// 密保问题
    /// </summary>
    public string? Question { get; set; }

    /// <summary>
    /// 密保答案
    /// </summary>
    public string? SecPwd { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    public virtual ICollection<Blogtag> Blogtags { get; set; } = new List<Blogtag>();

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Loginsession> Loginsessions { get; set; } = new List<Loginsession>();

    public virtual ICollection<Userprofile> Userprofiles { get; set; } = new List<Userprofile>();
}
