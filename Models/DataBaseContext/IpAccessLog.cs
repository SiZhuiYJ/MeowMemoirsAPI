using System;
using System.Collections.Generic;

namespace MeowMemoirsAPI.Models.DataBaseContext;

/// <summary>
/// IP访问记录表，用于安全审计、流量分析和行为追踪
/// </summary>
public partial class IpAccessLog
{
    /// <summary>
    /// 自增主键，唯一标识每条记录
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 通过IP和时间戳生成的哈希值，用于匿名化标识
    /// </summary>
    public string? IpId { get; set; }

    /// <summary>
    /// 客户端IP地址（支持IPv4/IPv6），示例：&quot;203.0.113.45&quot; 或 &quot;2001:db8::1&quot;
    /// </summary>
    public string IpAddress { get; set; } = null!;

    /// <summary>
    /// 客户端浏览器/设备信息，示例：&quot;Mozilla/5.0 (Windows NT 10.0; Win64; x64)...&quot;
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 请求体内容（敏感信息需脱敏），示例：`{&quot;username&quot;:&quot;test&quot;,&quot;password&quot;:&quot;***&quot;}`
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// 请求时间（精确到毫秒），示例：&quot;2023-10-25 14:30:45.123&quot;
    /// </summary>
    public DateTime RequestTime { get; set; }

    /// <summary>
    /// HTTP请求方法，示例：GET、POST、PUT、DELETE
    /// </summary>
    public string RequestMethod { get; set; } = null!;

    /// <summary>
    /// 完整请求路径（含查询参数），示例：&quot;/api/login?token=abc123&quot;
    /// </summary>
    public string RequestUrl { get; set; } = null!;

    /// <summary>
    /// HTTP协议版本，示例：&quot;HTTP/1.1&quot; 或 &quot;HTTP/2&quot;
    /// </summary>
    public string? HttpVersion { get; set; }

    /// <summary>
    /// 服务器响应状态码，示例：200（成功）、404（未找到）、500（服务器错误）
    /// </summary>
    public int? ResponseStatus { get; set; }

    /// <summary>
    /// 服务器处理请求耗时（毫秒），示例：125
    /// </summary>
    public int? ResponseTimeMs { get; set; }

    /// <summary>
    /// 来源页面URL（可选），示例：&quot;https://example.com/home&quot;
    /// </summary>
    public string? Referer { get; set; }

    /// <summary>
    /// 请求头信息（JSON格式），示例：`{&quot;Accept-Language&quot;: &quot;en-US&quot;, &quot;Cookie&quot;: &quot;...&quot;}`
    /// </summary>
    public string? Headers { get; set; }

    /// <summary>
    /// IP地理位置信息（JSON格式），示例：`{&quot;country&quot;: &quot;CN&quot;, &quot;city&quot;: &quot;Beijing&quot;}`
    /// </summary>
    public string? GeoLocation { get; set; }

    /// <summary>
    /// 设备类型（通过User-Agent解析），示例：Mobile、Desktop、Tablet
    /// </summary>
    public string? DeviceType { get; set; }

    /// <summary>
    /// 操作系统名称及版本，示例：Windows 10、iOS 16.5
    /// </summary>
    public string? OsName { get; set; }

    /// <summary>
    /// 浏览器名称及版本，示例：Chrome 118、Firefox 119
    /// </summary>
    public string? BrowserName { get; set; }

    /// <summary>
    /// 是否为爬虫/机器人请求，TRUE/FALSE
    /// </summary>
    public bool? IsBot { get; set; }

    /// <summary>
    /// 威胁等级（0-5），0=正常，3=可疑，5=攻击行为
    /// </summary>
    public sbyte? ThreatLevel { get; set; }

    /// <summary>
    /// 用户会话ID（如有），示例：&quot;sess_abc123xyz&quot;
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// 关联用户ID（如已登录），示例：&quot;usr_456&quot;
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// 备注信息（如攻击类型），示例：&quot;SQL Injection Attempt&quot;
    /// </summary>
    public string? ExtraNotes { get; set; }
}
