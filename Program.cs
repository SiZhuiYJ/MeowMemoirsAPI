using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Middleware.address;
using MeowMemoirsAPI.Middleware.log;
using MeowMemoirsAPI.Models.DataBaseContext;
using MeowMemoirsAPI.Models.JWT;
using MeowMemoirsAPI.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddMemoryCache();// 开启内存缓存

#region 数据库连接
// 远程连接数据库：scaffold-dbcontext 'Server=8.137.127.7;Database=database;charset=utf8;uid=root;pwd=a743ac967ce1cda0;port=3306;' Pomelo.EntityFrameworkCore.MySql -OutputDir Models/DataBaseContext -context MyRainbowContext -Force
//scaffold-dbcontext 'Server=localhost;Database=MyDatabase;charset=utf8;uid=root;pwd=0129Hxxx;port=3306;' Pomelo.EntityFrameworkCore.MySql -OutputDir Models/DataBaseContext -context MyRainbowContext -Force
//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//    => optionsBuilder.UseMySql("server=localhost;database=MyDatabase;charset=utf8;uid=root;pwd=0129Hxxx;port=3306", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.3.0-mysql"));

builder.Services.AddDbContext<MyRainbowContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("MyDatabaseConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MyDatabaseConnection"))));
#endregion

#region 编码注册
// 注册编码提供程序，支持 GBK
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endregion

#region 服务注册
//在大多数情况下，数据库操作服务适合使用 AddScoped 进行注册，这在 ASP.NET Core 应用中尤为常见。在这类应用里，每个 HTTP 请求都会创建一个新的服务作用域，在这个作用域内使用相同的数据库上下文实例能保证数据操作的一致性和事务性。
//适用原因
//状态一致性：同一个请求内，使用相同的数据库上下文实例可以保证数据状态的一致性。比如在处理一个业务逻辑时，可能会涉及多次数据库查询和更新操作，使用同一个上下文实例可以确保这些操作基于相同的数据快照。
//事务处理：数据库事务通常需要在同一个上下文实例中完成。使用 AddScoped 可以保证在一个请求的处理过程中，所有的数据库操作都在同一个事务中进行，避免出现数据不一致的问题。

//AddSingleton：在整个应用程序生命周期内只创建一个实例，适用于无状态且需全局共享的服务
#region 服务AddSingleton
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // 添加 IHttpContextAccessor 到依赖注入容器 访问 HttpContext
builder.Services.AddSingleton<ILogService, LogService>();// 添加 LogService 到依赖注入容器 日志服务
builder.Services.AddSingleton<IIPQueryService,IPQueryService>();// 添加 IPQueryService 到依赖注入容器 IP查询服务
#endregion

//AddTransient：每次请求都会创建新实例，适用于无状态服务。
#region 服务AddTransient
builder.Services.AddTransient<IAuthService, AuthService>();// 添加 AuthService 到依赖注入容器 身份验证服务
builder.Services.AddTransient<IFileService, FileService>(); // 添加 FileService 到依赖注入容器 文件服务
#endregion

//AddScoped：在同一个服务作用域内返回相同实例，适用于在请求处理过程中需保持状态一致的服务。
#region 服务AddScoped
builder.Services.AddScoped<IJwtService, JwtService>(); // 添加 JwtService 到依赖注入容器 JWT 服务

#endregion

#endregion

#region JWT验证
// 添加 JWT 配置
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));
// 读取appsettings.json的JwtConfig配置
var jwtConfig = builder.Configuration.GetSection("JwtConfig").Get<JwtConfig>();
var jwtConfigSection = builder.Configuration.GetSection("JwtConfig:");
// 读取配置文件中的 JWT 配置
builder.Configuration.Bind("JwtConfig", jwtConfig);
builder.Services
    .AddAuthentication(option =>
    {
        //认证middleware配置
        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 指定名称声明类型（关键配置）
            NameClaimType = "RainbowId", // 根据你的声明名称修改

            // 其他验证参数
            ValidateIssuer = true,
            ValidateAudience = true,
            //Token颁发机构
            ValidIssuer = jwtConfig.Issuer,
            //颁发给谁
            ValidAudience = jwtConfig.Audience,
            //这里的key要进行加密
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey)),
            //是否验证Token有效期，使用当前时间与Token的Claims中的NotBefore和Expires对比
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // 严格时间验证
        };
    });
#endregion

#region Swagger配置
builder.Services.AddSwaggerGen(opt =>
{
    #region 配置接口注释

    string xmlPath = Path.Combine(AppContext.BaseDirectory, "NET+MySQL.xml");// xml 名称一般和项目名称一致即可
    opt.IncludeXmlComments(xmlPath);

    #endregion

    #region 配置接口分组或版本

    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "初版本 API",
        Description = "初版本的API",
    });
    opt.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2",
        Title = "第二版本 API",
        Description = "第二版本的API",
    });
    opt.SwaggerDoc("v3", new OpenApiInfo
    {
        Version = "v3",
        Title = "第三版本 API",
        Description = "第三版本的API",
    });

    #endregion

    #region 配置接口token验证
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme{
                                Reference = new OpenApiReference {
                                            Type = ReferenceType.SecurityScheme,
                                            Id = "Bearer"
                                }
                           },Array.Empty<string>()
                        }
    });
    #endregion
});
#endregion

#region 跨域配置
builder.Services.AddCors(o => o.AddPolicy("MyCors", b => b.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod()));
#endregion

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



#region 配置CORS
app.UseCors("MyCors");
#endregion

#region 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    #region 配置Swagger
    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint($"/swagger/v1/swagger.json", "v1");
        opt.SwaggerEndpoint($"/swagger/v2/swagger.json", "v2");
        opt.SwaggerEndpoint($"/swagger/v3/swagger.json", "v3");
    });
    #endregion
}
#endregion
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
#region 添加中间件
app.UseMiddleware<RealIpMiddleware>();// 添加获取真实IP的中间件
app.UseMiddleware<LoggingMiddleware>();// 添加日志中间件
#endregion
app.Run();
