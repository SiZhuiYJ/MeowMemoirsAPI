using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace MeowMemoirsAPI.Models.DataBaseContext;

public partial class MyRainbowContext : DbContext
{
    public MyRainbowContext()
    {
    }

    public MyRainbowContext(DbContextOptions<MyRainbowContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blacklist> Blacklists { get; set; }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<Blogtag> Blogtags { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<IpAccessLog> IpAccessLogs { get; set; }

    public virtual DbSet<Loginsession> Loginsessions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Userprofile> Userprofiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Blacklist>(entity =>
        {
            entity.HasKey(e => e.BlacklistId).HasName("PRIMARY");

            entity
                .ToTable("blacklist", tb => tb.HasComment("系统黑名单表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.ExpireTime, "idx_expiretime");

            entity.HasIndex(e => new { e.Type, e.Value }, "unique_type_value").IsUnique();

            entity.Property(e => e.BlacklistId).HasColumnName("BlacklistID");
            entity.Property(e => e.AdminId)
                .HasComment("操作管理员ID")
                .HasColumnName("AdminID");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("加入黑名单时间")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpireTime)
                .HasComment("过期时间（可选，永久黑名单可为NULL）")
                .HasColumnType("datetime");
            entity.Property(e => e.Reason)
                .HasMaxLength(200)
                .HasComment("加入黑名单的原因")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Type)
                .HasComment("黑名单类型：token/用户/IP")
                .HasColumnType("enum('token','user','ip')");
            entity.Property(e => e.Value)
                .HasComment("存储的具体值（Token字符串/用户ID/IP地址）")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("blogs", tb => tb.HasComment("博客表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.UserId, "UserID");

            entity.Property(e => e.Id)
                .HasComment("博客ID")
                .HasColumnName("id");
            entity.Property(e => e.Content)
                .HasComment("博客内容")
                .HasColumnType("text")
                .HasColumnName("content")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.CoverContent)
                .HasMaxLength(100)
                .HasComment("博客封面内容（100字以内）")
                .HasColumnName("cover_content")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Tags)
                .HasMaxLength(100)
                .HasComment("博客标签（多个标签用逗号分隔，最多10个）")
                .HasColumnName("tags")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasComment("博客标题")
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("更新时间")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasComment("用户ID")
                .HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("blogs_ibfk_1");
        });

        modelBuilder.Entity<Blogtag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PRIMARY");

            entity
                .ToTable("blogtags", tb => tb.HasComment("博客标签表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.UserId, "UserID");

            entity.Property(e => e.TagId)
                .HasComment("标签ID")
                .HasColumnName("TagID");
            entity.Property(e => e.TagColor)
                .HasMaxLength(7)
                .HasComment("标签颜色")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.TagCreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("标签创建时间")
                .HasColumnType("datetime");
            entity.Property(e => e.TagDescription)
                .HasMaxLength(200)
                .HasComment("标签描述")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.TagIcon)
                .HasMaxLength(100)
                .HasComment("标签图标")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.TagName)
                .HasMaxLength(20)
                .HasComment("标签名称")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.TagStatus).HasComment("标签状态");
            entity.Property(e => e.UserId)
                .HasComment("创建者ID")
                .HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Blogtags)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("blogtags_ibfk_1");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("classes", tb => tb.HasComment("课程表"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UserId, "UserID");

            entity.Property(e => e.Id)
                .HasComment("课程ID")
                .HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .HasDefaultValueSql("'#1890ff'")
                .HasComment("颜色")
                .HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DayOfWeek)
                .HasComment("周几（0-6）")
                .HasColumnName("dayOfWeek");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasComment("地点")
                .HasColumnName("location");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasComment("课程名")
                .HasColumnName("name");
            entity.Property(e => e.Number)
                .HasComment("节次（JSON数组）")
                .HasColumnType("json")
                .HasColumnName("number");
            entity.Property(e => e.Remark)
                .HasComment("备注")
                .HasColumnType("text")
                .HasColumnName("remark");
            entity.Property(e => e.Teacher)
                .HasMaxLength(255)
                .HasComment("教师")
                .HasColumnName("teacher");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Week)
                .HasComment("周数（JSON数组）")
                .HasColumnType("json")
                .HasColumnName("week");

            entity.HasOne(d => d.User).WithMany(p => p.Classes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("classes_ibfk_1");
        });

        modelBuilder.Entity<IpAccessLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ip_access_log", tb => tb.HasComment("IP访问记录表，用于安全审计、流量分析和行为追踪"));

            entity.HasIndex(e => e.IpAddress, "idx_ip");

            entity.HasIndex(e => e.RequestTime, "idx_time");

            entity.Property(e => e.Id)
                .HasComment("自增主键，唯一标识每条记录")
                .HasColumnName("id");
            entity.Property(e => e.BrowserName)
                .HasMaxLength(50)
                .HasComment("浏览器名称及版本，示例：Chrome 118、Firefox 119")
                .HasColumnName("browser_name");
            entity.Property(e => e.DeviceType)
                .HasMaxLength(20)
                .HasComment("设备类型（通过User-Agent解析），示例：Mobile、Desktop、Tablet")
                .HasColumnName("device_type");
            entity.Property(e => e.ExtraNotes)
                .HasComment("备注信息（如攻击类型），示例：\"SQL Injection Attempt\"")
                .HasColumnType("text")
                .HasColumnName("extra_notes");
            entity.Property(e => e.GeoLocation)
                .HasComment("IP地理位置信息（JSON格式），示例：`{\"country\": \"CN\", \"city\": \"Beijing\"}`")
                .HasColumnType("json")
                .HasColumnName("geo_location");
            entity.Property(e => e.Headers)
                .HasComment("请求头信息（JSON格式），示例：`{\"Accept-Language\": \"en-US\", \"Cookie\": \"...\"}`")
                .HasColumnType("json")
                .HasColumnName("headers");
            entity.Property(e => e.HttpVersion)
                .HasMaxLength(10)
                .HasComment("HTTP协议版本，示例：\"HTTP/1.1\" 或 \"HTTP/2\"")
                .HasColumnName("http_version");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasComment("客户端IP地址（支持IPv4/IPv6），示例：\"203.0.113.45\" 或 \"2001:db8::1\"")
                .HasColumnName("ip_address");
            entity.Property(e => e.IpId)
                .HasMaxLength(64)
                .HasComputedColumnSql("sha2(concat(`ip_address`,`request_time`),256)", true)
                .HasComment("通过IP和时间戳生成的哈希值，用于匿名化标识")
                .HasColumnName("ip_id");
            entity.Property(e => e.IsBot)
                .HasDefaultValueSql("'0'")
                .HasComment("是否为爬虫/机器人请求，TRUE/FALSE")
                .HasColumnName("is_bot");
            entity.Property(e => e.OsName)
                .HasMaxLength(50)
                .HasComment("操作系统名称及版本，示例：Windows 10、iOS 16.5")
                .HasColumnName("os_name");
            entity.Property(e => e.Referer)
                .HasMaxLength(2048)
                .HasComment("来源页面URL（可选），示例：\"https://example.com/home\"")
                .HasColumnName("referer");
            entity.Property(e => e.RequestBody)
                .HasComment("请求体内容（敏感信息需脱敏），示例：`{\"username\":\"test\",\"password\":\"***\"}`")
                .HasColumnType("text")
                .HasColumnName("request_body");
            entity.Property(e => e.RequestMethod)
                .HasMaxLength(10)
                .HasComment("HTTP请求方法，示例：GET、POST、PUT、DELETE")
                .HasColumnName("request_method");
            entity.Property(e => e.RequestTime)
                .HasComment("请求时间（精确到毫秒），示例：\"2023-10-25 14:30:45.123\"")
                .HasColumnType("datetime(3)")
                .HasColumnName("request_time");
            entity.Property(e => e.RequestUrl)
                .HasMaxLength(2048)
                .HasComment("完整请求路径（含查询参数），示例：\"/api/login?token=abc123\"")
                .HasColumnName("request_url");
            entity.Property(e => e.ResponseStatus)
                .HasComment("服务器响应状态码，示例：200（成功）、404（未找到）、500（服务器错误）")
                .HasColumnName("response_status");
            entity.Property(e => e.ResponseTimeMs)
                .HasComment("服务器处理请求耗时（毫秒），示例：125")
                .HasColumnName("response_time_ms");
            entity.Property(e => e.SessionId)
                .HasMaxLength(128)
                .HasComment("用户会话ID（如有），示例：\"sess_abc123xyz\"")
                .HasColumnName("session_id");
            entity.Property(e => e.ThreatLevel)
                .HasDefaultValueSql("'0'")
                .HasComment("威胁等级（0-5），0=正常，3=可疑，5=攻击行为")
                .HasColumnName("threat_level");
            entity.Property(e => e.UserAgent)
                .HasComment("客户端浏览器/设备信息，示例：\"Mozilla/5.0 (Windows NT 10.0; Win64; x64)...\"")
                .HasColumnType("text")
                .HasColumnName("user_agent");
            entity.Property(e => e.UserId)
                .HasMaxLength(64)
                .HasComment("关联用户ID（如已登录），示例：\"usr_456\"")
                .HasColumnName("user_id");
        });

        modelBuilder.Entity<Loginsession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PRIMARY");

            entity
                .ToTable("loginsession", tb => tb.HasComment("用户登录会话表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.ExpireTime, "idx_expiretime");

            entity.HasIndex(e => e.RefreshToken, "idx_refreshtoken").IsUnique();

            entity.HasIndex(e => e.UserId, "idx_userid");

            entity.Property(e => e.SessionId)
                .HasComment("会话ID")
                .HasColumnName("SessionID");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("会话创建时间")
                .HasColumnType("datetime");
            entity.Property(e => e.DeviceInfo)
                .HasMaxLength(200)
                .HasComment("设备信息")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ExpireTime)
                .HasComment("Token到期时间")
                .HasColumnType("datetime");
            entity.Property(e => e.Ip)
                .HasMaxLength(50)
                .HasComment("登录IP地址")
                .HasColumnName("IP")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.LastActivity)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("最后活动时间")
                .HasColumnType("datetime");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(400)
                .HasComment("长Token（刷新Token）")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.UserId)
                .HasComment("用户ID")
                .HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Loginsessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("loginsession_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity
                .ToTable("user", tb => tb.HasComment("用户表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.RainbowId, "RainbowID").IsUnique();

            entity.HasIndex(e => e.UserEmail, "UserEmail").IsUnique();

            entity.HasIndex(e => e.UserName, "UserName").IsUnique();

            entity.Property(e => e.UserId)
                .HasComment("用户ID")
                .HasColumnName("UserID");
            entity.Property(e => e.Permissions)
                .HasMaxLength(5)
                .HasDefaultValueSql("'v1'")
                .HasComment("权限等级")
                .HasColumnName("permissions")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Question)
                .HasMaxLength(200)
                .HasComment("密保问题")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.RainbowId)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Rainbow_ID'")
                .HasComment("RainbowID")
                .HasColumnName("RainbowID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.SecPwd)
                .HasMaxLength(200)
                .HasComment("密保答案")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(30)
                .HasComment("邮箱")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.UserImg)
                .HasMaxLength(30)
                .HasDefaultValueSql("'_1.jpg'")
                .HasComment("头像")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.UserName)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Rainbow_name'")
                .HasComment("用户名")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.UserPhone)
                .HasMaxLength(11)
                .HasComment("电话")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.UserPwd)
                .HasMaxLength(20)
                .HasComment("密码")
                .HasColumnName("UserPWD")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Userprofile>(entity =>
        {
            entity.HasKey(e => e.ProfileId).HasName("PRIMARY");

            entity
                .ToTable("userprofile", tb => tb.HasComment("用户资料表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.UserId, "UserID");

            entity.Property(e => e.ProfileId)
                .HasComment("用户资料ID")
                .HasColumnName("ProfileID");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasComment("用户地址")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Birthday)
                .HasDefaultValueSql("'2024-04-30'")
                .HasComment("生日")
                .HasColumnName("birthday");
            entity.Property(e => e.DateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime");
            entity.Property(e => e.Latitude)
                .HasPrecision(18, 15)
                .HasComment("用户纬度");
            entity.Property(e => e.Longitude)
                .HasPrecision(18, 15)
                .HasComment("用户经度");
            entity.Property(e => e.Sex)
                .HasDefaultValueSql("'2'")
                .HasComment("性别：0-女，1-男，2-未知")
                .HasColumnName("sex");
            entity.Property(e => e.UserId)
                .HasComment("用户ID")
                .HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Userprofiles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("userprofile_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
