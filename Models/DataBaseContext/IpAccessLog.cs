using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// IP访问记录表
/// </summary>
public partial class IpAccessLog
{
    /// <summary>
    /// 自增主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// IP和时间戳哈希值
    /// </summary>
    public string? IpId { get; set; }

    /// <summary>
    /// 客户端IP地址
    /// </summary>
    public string IpAddress { get; set; } = null!;

    /// <summary>
    /// 客户端浏览器信息
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 请求体内容(脱敏)
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// 请求时间(毫秒)
    /// </summary>
    public DateTime RequestTime { get; set; }

    /// <summary>
    /// HTTP请求方法
    /// </summary>
    public string RequestMethod { get; set; } = null!;

    /// <summary>
    /// 完整请求路径
    /// </summary>
    public string RequestUrl { get; set; } = null!;

    /// <summary>
    /// HTTP协议版本
    /// </summary>
    public string? HttpVersion { get; set; }

    /// <summary>
    /// 响应状态码
    /// </summary>
    public int? ResponseStatus { get; set; }

    /// <summary>
    /// 处理耗时(毫秒)
    /// </summary>
    public uint? ResponseTimeMs { get; set; }

    /// <summary>
    /// 来源页面URL
    /// </summary>
    public string? Referer { get; set; }

    /// <summary>
    /// 请求头信息(JSON)
    /// </summary>
    public string? Headers { get; set; }

    /// <summary>
    /// IP地理位置(JSON)
    /// </summary>
    public string? GeoLocation { get; set; }

    /// <summary>
    /// 设备类型
    /// </summary>
    public string? DeviceType { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    public string? OsName { get; set; }

    /// <summary>
    /// 浏览器
    /// </summary>
    public string? BrowserName { get; set; }

    /// <summary>
    /// 0-正常 1-爬虫
    /// </summary>
    public sbyte IsBot { get; set; }

    /// <summary>
    /// 威胁等级0-5
    /// </summary>
    public byte ThreatLevel { get; set; }

    /// <summary>
    /// 用户会话ID
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// 关联用户ID
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// 备注信息
    /// </summary>
    public string? ExtraNotes { get; set; }

    /// <summary>
    /// 0-正常 1-删除
    /// </summary>
    public sbyte IsDeleted { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

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
