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

    public virtual DbSet<BlogTag> BlogTags { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<IpAccessLog> IpAccessLogs { get; set; }

    public virtual DbSet<LoginSession> LoginSessions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Blacklist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("blacklist", tb => tb.HasComment("系统黑名单表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.AdminId, "idx_admin_id");

            entity.HasIndex(e => new { e.IsDeleted, e.BlacklistType, e.ExpireTime }, "idx_blacklist_management");

            entity.HasIndex(e => e.ExpireTime, "idx_expire_time");

            entity.HasIndex(e => new { e.BlacklistType, e.BlacklistValue }, "udx_type_value").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("黑名单ID")
                .HasColumnName("id");
            entity.Property(e => e.AdminId)
                .HasComment("操作管理员ID")
                .HasColumnName("admin_id");
            entity.Property(e => e.BlacklistType)
                .HasComment("类型:token/user/ip")
                .HasColumnType("enum('token','user','ip')")
                .HasColumnName("blacklist_type");
            entity.Property(e => e.BlacklistValue)
                .HasComment("具体值")
                .HasColumnName("blacklist_value");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("create_time");
            entity.Property(e => e.ExpireTime)
                .HasComment("过期时间")
                .HasColumnType("datetime")
                .HasColumnName("expire_time");
            entity.Property(e => e.ExtAttr1)
                .HasMaxLength(255)
                .HasComment("扩展字段1")
                .HasColumnName("ext_attr1");
            entity.Property(e => e.ExtAttr2)
                .HasMaxLength(255)
                .HasComment("扩展字段2")
                .HasColumnName("ext_attr2");
            entity.Property(e => e.ExtAttr3)
                .HasMaxLength(255)
                .HasComment("扩展字段3")
                .HasColumnName("ext_attr3");
            entity.Property(e => e.IsDeleted)
                .HasComment("0-正常 1-删除")
                .HasColumnName("is_deleted");
            entity.Property(e => e.Reason)
                .HasMaxLength(200)
                .HasComment("加入原因")
                .HasColumnName("reason");
            entity.Property(e => e.UpdateTime)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("更新时间")
                .HasColumnType("datetime")
                .HasColumnName("update_time");
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("blogs", tb => tb.HasComment("博客表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.Title, e.Content }, "ft_title_content").HasAnnotation("MySql:FullTextIndex", true);

            entity.HasIndex(e => new { e.IsDeleted, e.CreateTime }, "idx_blog_status");

            entity.HasIndex(e => e.CreateTime, "idx_create_time");

            entity.HasIndex(e => e.UserId, "idx_user_id");

            entity.Property(e => e.Id)
                .HasComment("博客ID")
                .HasColumnName("id");
            entity.Property(e => e.Content)
                .HasComment("博客内容")
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("create_time");
            entity.Property(e => e.ExtAttr1)
                .HasMaxLength(255)
                .HasComment("扩展字段1")
                .HasColumnName("ext_attr1");
            entity.Property(e => e.ExtAttr2)
                .HasMaxLength(255)
                .HasComment("扩展字段2")
                .HasColumnName("ext_attr2");
            entity.Property(e => e.ExtAttr3)
                .HasMaxLength(255)
                .HasComment("扩展字段3")
                .HasColumnName("ext_attr3");
            entity.Property(e => e.IsDeleted)
                .HasComment("0-正常 1-删除")
                .HasColumnName("is_deleted");
            entity.Property(e => e.LikeCount)
                .HasComment("点赞数")
                .HasColumnName("like_count");
            entity.Property(e => e.Summary)
                .HasMaxLength(100)
                .HasComment("博客摘要内容")
                .HasColumnName("summary");
            entity.Property(e => e.Tags)
                .HasComment("博客标签(JSON格式)")
                .HasColumnType("json")
                .HasColumnName("tags");
            entity.Property(e => e.Title)
                .HasComment("博客标题")
                .HasColumnName("title");
            entity.Property(e => e.UpdateTime)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("更新时间")
                .HasColumnType("datetime")
                .HasColumnName("update_time");
            entity.Property(e => e.UserId)
                .HasComment("用户ID")
                .HasColumnName("user_id");
            entity.Property(e => e.ViewCount)
                .HasComment("浏览量")
                .HasColumnName("view_count");

            entity.HasOne(d => d.User).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_blog_user");
        });

        modelBuilder.Entity<BlogTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("blog_tag", tb => tb.HasComment("博客标签表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.IsDeleted, e.TagStatus }, "idx_tag_global_status");

            entity.HasIndex(e => e.TagStatus, "idx_tag_status");

            entity.HasIndex(e => e.UserId, "idx_user_id");

            entity.HasIndex(e => e.TagName, "udx_tag_name").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("标签ID")
                .HasColumnName("id");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("create_time");
            entity.Property(e => e.ExtAttr1)
                .HasMaxLength(255)
                .HasComment("扩展字段1")
                .HasColumnName("ext_attr1");
            entity.Property(e => e.ExtAttr2)
                .HasMaxLength(255)
                .HasComment("扩展字段2")
                .HasColumnName("ext_attr2");
            entity.Property(e => e.ExtAttr3)
                .HasMaxLength(255)
                .HasComment("扩展字段3")
                .HasColumnName("ext_attr3");
            entity.Property(e => e.IsDeleted)
                .HasComment("0-正常 1-删除")
                .HasColumnName("is_deleted");
            entity.Property(e => e.TagColor)
                .HasMaxLength(7)
                .HasComment("标签颜色")
                .HasColumnName("tag_color");
            entity.Property(e => e.TagDescription)
                .HasMaxLength(200)
                .HasComment("标签描述")
                .HasColumnName("tag_description");
            entity.Property(e => e.TagIcon)
                .HasMaxLength(100)
                .HasComment("标签图标")
                .HasColumnName("tag_icon");
            entity.Property(e => e.TagName)
                .HasMaxLength(20)
                .HasComment("标签名称")
                .HasColumnName("tag_name");
            entity.Property(e => e.TagStatus)
                .HasDefaultValueSql("'1'")
                .HasComment("0-禁用 1-启用")
                .HasColumnName("tag_status");
            entity.Property(e => e.UpdateTime)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("更新时间")
                .HasColumnType("datetime")
                .HasColumnName("update_time");
            entity.Property(e => e.UserId)
                .HasComment("创建者ID")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.BlogTags)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_blog_tag_user");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("class", tb => tb.HasComment("课程表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.IsDeleted, e.DayOfWeek }, "idx_class_status");

            entity.HasIndex(e => e.DayOfWeek, "idx_day_of_week");

            entity.HasIndex(e => e.UserId, "idx_user_id");

            entity.Property(e => e.Id)
                .HasComment("课程ID")
                .HasColumnName("id");
            entity.Property(e => e.ClassName)
                .HasMaxLength(255)
                .HasComment("课程名")
                .HasColumnName("class_name");
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .HasDefaultValueSql("'#1890ff'")
                .HasComment("颜色")
                .HasColumnName("color");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("create_time");
            entity.Property(e => e.DayOfWeek)
                .HasComment("周几(1-7)")
                .HasColumnName("day_of_week");
            entity.Property(e => e.ExtAttr1)
                .HasMaxLength(255)
                .HasComment("扩展字段1")
                .HasColumnName("ext_attr1");
            entity.Property(e => e.ExtAttr2)
                .HasMaxLength(255)
                .HasComment("扩展字段2")
                .HasColumnName("ext_attr2");
            entity.Property(e => e.ExtAttr3)
                .HasMaxLength(255)
                .HasComment("扩展字段3")
                .HasColumnName("ext_attr3");
            entity.Property(e => e.IsDeleted)
                .HasComment("0-正常 1-删除")
                .HasColumnName("is_deleted");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasComment("地点")
                .HasColumnName("location");
            entity.Property(e => e.Remark)
                .HasComment("备注")
                .HasColumnType("text")
                .HasColumnName("remark");
            entity.Property(e => e.SessionList)
                .HasComment("节次(JSON数组)")
                .HasColumnType("json")
                .HasColumnName("session_list");
            entity.Property(e => e.Teacher)
                .HasMaxLength(255)
                .HasComment("教师")
                .HasColumnName("teacher");
            entity.Property(e => e.UpdateTime)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("更新时间")
                .HasColumnType("datetime")
                .HasColumnName("update_time");
            entity.Property(e => e.UserId)
                .HasComment("用户ID")
                .HasColumnName("user_id");
            entity.Property(e => e.WeekList)
                .HasComment("周数(JSON数组)")
                .HasColumnType("json")
                .HasColumnName("week_list");

            entity.HasOne(d => d.User).WithMany(p => p.Classes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_class_user");
        });

        modelBuilder.Entity<IpAccessLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("ip_access_log", tb => tb.HasComment("IP访问记录表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.IpAddress, e.RequestTime, e.ResponseStatus }, "idx_access_analysis");

            entity.HasIndex(e => e.IpAddress, "idx_ip_address");

            entity.HasIndex(e => new { e.IsDeleted, e.ThreatLevel }, "idx_log_status");

            entity.HasIndex(e => e.RequestTime, "idx_request_time");

            entity.HasIndex(e => e.ResponseStatus, "idx_response_status");

            entity.HasIndex(e => e.ThreatLevel, "idx_threat_level");

            entity.Property(e => e.Id)
                .HasComment("自增主键")
                .HasColumnName("id");
            entity.Property(e => e.BrowserName)
                .HasMaxLength(50)
                .HasComment("浏览器")
                .HasColumnName("browser_name");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("create_time");
            entity.Property(e => e.DeviceType)
                .HasMaxLength(20)
                .HasComment("设备类型")
                .HasColumnName("device_type");
            entity.Property(e => e.ExtAttr1)
                .HasMaxLength(255)
                .HasComment("扩展字段1")
                .HasColumnName("ext_attr1");
            entity.Property(e => e.ExtAttr2)
                .HasMaxLength(255)
                .HasComment("扩展字段2")
                .HasColumnName("ext_attr2");
            entity.Property(e => e.ExtAttr3)
                .HasMaxLength(255)
                .HasComment("扩展字段3")
                .HasColumnName("ext_attr3");
            entity.Property(e => e.ExtraNotes)
                .HasComment("备注信息")
                .HasColumnType("text")
                .HasColumnName("extra_notes");
            entity.Property(e => e.GeoLocation)
                .HasComment("IP地理位置(JSON)")
                .HasColumnType("json")
                .HasColumnName("geo_location");
            entity.Property(e => e.Headers)
                .HasComment("请求头信息(JSON)")
                .HasColumnType("json")
                .HasColumnName("headers");
            entity.Property(e => e.HttpVersion)
                .HasMaxLength(20)
                .HasComment("HTTP协议版本")
                .HasColumnName("http_version");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasComment("客户端IP地址")
                .HasColumnName("ip_address");
            entity.Property(e => e.IpId)
                .HasMaxLength(64)
                .HasComputedColumnSql("sha2(concat(`ip_address`,`request_time`),256)", true)
                .HasComment("IP和时间戳哈希值")
                .HasColumnName("ip_id");
            entity.Property(e => e.IsBot)
                .HasComment("0-正常 1-爬虫")
                .HasColumnName("is_bot");
            entity.Property(e => e.IsDeleted)
                .HasComment("0-正常 1-删除")
                .HasColumnName("is_deleted");
            entity.Property(e => e.OsName)
                .HasMaxLength(50)
                .HasComment("操作系统")
                .HasColumnName("os_name");
            entity.Property(e => e.Referer)
                .HasMaxLength(2048)
                .HasComment("来源页面URL")
                .HasColumnName("referer");
            entity.Property(e => e.RequestBody)
                .HasComment("请求体内容(脱敏)")
                .HasColumnType("text")
                .HasColumnName("request_body");
            entity.Property(e => e.RequestMethod)
                .HasMaxLength(10)
                .HasComment("HTTP请求方法")
                .HasColumnName("request_method");
            entity.Property(e => e.RequestTime)
                .HasComment("请求时间(毫秒)")
                .HasColumnType("datetime(3)")
                .HasColumnName("request_time");
            entity.Property(e => e.RequestUrl)
                .HasMaxLength(2048)
                .HasComment("完整请求路径")
                .HasColumnName("request_url");
            entity.Property(e => e.ResponseStatus)
                .HasComment("响应状态码")
                .HasColumnName("response_status");
            entity.Property(e => e.ResponseTimeMs)
                .HasComment("处理耗时(毫秒)")
                .HasColumnName("response_time_ms");
            entity.Property(e => e.SessionId)
                .HasMaxLength(128)
                .HasComment("用户会话ID")
                .HasColumnName("session_id");
            entity.Property(e => e.ThreatLevel)
                .HasComment("威胁等级0-5")
                .HasColumnName("threat_level");
            entity.Property(e => e.UserAgent)
                .HasComment("客户端浏览器信息")
                .HasColumnType("text")
                .HasColumnName("user_agent");
            entity.Property(e => e.UserId)
                .HasMaxLength(64)
                .HasComment("关联用户ID")
                .HasColumnName("user_id");
        });

        modelBuilder.Entity<LoginSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("login_session", tb => tb.HasComment("用户登录会话表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.AccessToken, "idx_access_token").HasAnnotation("MySql:IndexPrefixLength", new[] { 255 });

            entity.HasIndex(e => e.ExpireTime, "idx_expire_time");

            entity.HasIndex(e => new { e.IsDeleted, e.ExpireTime }, "idx_session_cleanup");

            entity.HasIndex(e => e.UserId, "idx_user_id");

            entity.HasIndex(e => e.RefreshToken, "udx_refresh_token")
                .IsUnique()
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 255 });

            entity.Property(e => e.Id)
                .HasComment("会话ID")
                .HasColumnName("id");
            entity.Property(e => e.AccessToken)
                .HasMaxLength(500)
                .HasComment("访问令牌")
                .HasColumnName("access_token");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("create_time");
            entity.Property(e => e.DeviceInfo)
                .HasMaxLength(200)
                .HasComment("设备信息")
                .HasColumnName("device_info");
            entity.Property(e => e.ExpireTime)
                .HasComment("令牌到期时间")
                .HasColumnType("datetime")
                .HasColumnName("expire_time");
            entity.Property(e => e.ExtAttr1)
                .HasMaxLength(255)
                .HasComment("扩展字段1")
                .HasColumnName("ext_attr1");
            entity.Property(e => e.ExtAttr2)
                .HasMaxLength(255)
                .HasComment("扩展字段2")
                .HasColumnName("ext_attr2");
            entity.Property(e => e.ExtAttr3)
                .HasMaxLength(255)
                .HasComment("扩展字段3")
                .HasColumnName("ext_attr3");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .HasComment("登录IP地址")
                .HasColumnName("ip_address");
            entity.Property(e => e.IsDeleted)
                .HasComment("0-正常 1-删除")
                .HasColumnName("is_deleted");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(500)
                .HasComment("刷新令牌")
                .HasColumnName("refresh_token");
            entity.Property(e => e.UpdateTime)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("最后活动时间")
                .HasColumnType("datetime")
                .HasColumnName("update_time");
            entity.Property(e => e.UserId)
                .HasComment("用户ID")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.LoginSessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_login_session_user");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user", tb => tb.HasComment("用户表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.PermissionLevel, "idx_permission_level");

            entity.HasIndex(e => new { e.IsDeleted, e.PermissionLevel }, "idx_user_status");

            entity.HasIndex(e => e.RainbowId, "udx_rainbow_id").IsUnique();

            entity.HasIndex(e => e.UserEmail, "udx_user_email").IsUnique();

            entity.HasIndex(e => e.UserName, "udx_user_name").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("用户ID")
                .HasColumnName("id");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("create_time");
            entity.Property(e => e.ExtAttr1)
                .HasMaxLength(255)
                .HasComment("扩展字段1")
                .HasColumnName("ext_attr1");
            entity.Property(e => e.ExtAttr2)
                .HasMaxLength(255)
                .HasComment("扩展字段2")
                .HasColumnName("ext_attr2");
            entity.Property(e => e.ExtAttr3)
                .HasMaxLength(255)
                .HasComment("扩展字段3")
                .HasColumnName("ext_attr3");
            entity.Property(e => e.IsDeleted)
                .HasComment("0-正常 1-删除")
                .HasColumnName("is_deleted");
            entity.Property(e => e.PermissionLevel)
                .HasMaxLength(10)
                .HasDefaultValueSql("'v1'")
                .HasComment("权限等级: v1-普通用户 admin-管理员")
                .HasColumnName("permission_level");
            entity.Property(e => e.RainbowId)
                .HasMaxLength(20)
                .HasDefaultValueSql("'rainbow_001'")
                .HasComment("RainbowID")
                .HasColumnName("rainbow_id");
            entity.Property(e => e.SecurityAnswer)
                .HasMaxLength(200)
                .HasComment("密保答案")
                .HasColumnName("security_answer");
            entity.Property(e => e.SecurityQuestion)
                .HasMaxLength(200)
                .HasComment("密保问题")
                .HasColumnName("security_question");
            entity.Property(e => e.UpdateTime)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("更新时间")
                .HasColumnType("datetime")
                .HasColumnName("update_time");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(30)
                .HasComment("邮箱")
                .HasColumnName("user_email");
            entity.Property(e => e.UserImg)
                .HasMaxLength(100)
                .HasDefaultValueSql("'default_avatar.jpg'")
                .HasComment("头像")
                .HasColumnName("user_img");
            entity.Property(e => e.UserName)
                .HasMaxLength(20)
                .HasDefaultValueSql("'rainbow_user'")
                .HasComment("用户名")
                .HasColumnName("user_name");
            entity.Property(e => e.UserPassword)
                .HasMaxLength(255)
                .HasComment("密码(加密存储)")
                .HasColumnName("user_password");
            entity.Property(e => e.UserPhone)
                .HasMaxLength(11)
                .IsFixedLength()
                .HasComment("手机号")
                .HasColumnName("user_phone");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user_profile", tb => tb.HasComment("用户资料表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Gender, "idx_gender");

            entity.HasIndex(e => new { e.IsDeleted, e.Gender }, "idx_profile_status");

            entity.HasIndex(e => e.UserId, "udx_user_id").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("用户资料ID")
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasComment("用户地址")
                .HasColumnName("address");
            entity.Property(e => e.Birthday)
                .HasDefaultValueSql("'2024-04-30'")
                .HasComment("生日")
                .HasColumnName("birthday");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("create_time");
            entity.Property(e => e.ExtAttr1)
                .HasMaxLength(255)
                .HasComment("扩展字段1")
                .HasColumnName("ext_attr1");
            entity.Property(e => e.ExtAttr2)
                .HasMaxLength(255)
                .HasComment("扩展字段2")
                .HasColumnName("ext_attr2");
            entity.Property(e => e.ExtAttr3)
                .HasMaxLength(255)
                .HasComment("扩展字段3")
                .HasColumnName("ext_attr3");
            entity.Property(e => e.Gender)
                .HasDefaultValueSql("'2'")
                .HasComment("0-女 1-男 2-未知")
                .HasColumnName("gender");
            entity.Property(e => e.IsDeleted)
                .HasComment("0-正常 1-删除")
                .HasColumnName("is_deleted");
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 7)
                .HasComment("用户纬度")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(10, 7)
                .HasComment("用户经度")
                .HasColumnName("longitude");
            entity.Property(e => e.UpdateTime)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("更新时间")
                .HasColumnType("datetime")
                .HasColumnName("update_time");
            entity.Property(e => e.UserId)
                .HasComment("用户ID")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.UserProfile)
                .HasForeignKey<UserProfile>(d => d.UserId)
                .HasConstraintName("fk_user_profile_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
