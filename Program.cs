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

//builder.Services.AddMemoryCache();// �����ڴ滺��

#region ���ݿ�����
// Զ���������ݿ⣺scaffold-dbcontext 'Server=8.137.127.7;Database=database;charset=utf8;uid=root;pwd=a743ac967ce1cda0;port=3306;' Pomelo.EntityFrameworkCore.MySql -OutputDir Models/DataBaseContext -context MyRainbowContext -Force
//scaffold-dbcontext 'Server=localhost;Database=MyDatabase;charset=utf8;uid=root;pwd=0129Hxxx;port=3306;' Pomelo.EntityFrameworkCore.MySql -OutputDir Models/DataBaseContext -context MyRainbowContext -Force
//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//    => optionsBuilder.UseMySql("server=localhost;database=MyDatabase;charset=utf8;uid=root;pwd=0129Hxxx;port=3306", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.3.0-mysql"));

builder.Services.AddDbContext<MyRainbowContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("MyDatabaseConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MyDatabaseConnection"))));
#endregion

#region ����ע��
// ע������ṩ����֧�� GBK
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endregion

#region ����ע��
//�ڴ��������£����ݿ���������ʺ�ʹ�� AddScoped ����ע�ᣬ���� ASP.NET Core Ӧ������Ϊ������������Ӧ���ÿ�� HTTP ���󶼻ᴴ��һ���µķ����������������������ʹ����ͬ�����ݿ�������ʵ���ܱ�֤���ݲ�����һ���Ժ������ԡ�
//����ԭ��
//״̬һ���ԣ�ͬһ�������ڣ�ʹ����ͬ�����ݿ�������ʵ�����Ա�֤����״̬��һ���ԡ������ڴ���һ��ҵ���߼�ʱ�����ܻ��漰������ݿ��ѯ�͸��²�����ʹ��ͬһ��������ʵ������ȷ����Щ����������ͬ�����ݿ��ա�
//���������ݿ�����ͨ����Ҫ��ͬһ��������ʵ������ɡ�ʹ�� AddScoped ���Ա�֤��һ������Ĵ�������У����е����ݿ��������ͬһ�������н��У�����������ݲ�һ�µ����⡣

//AddSingleton��������Ӧ�ó�������������ֻ����һ��ʵ������������״̬����ȫ�ֹ���ķ���
#region ����AddSingleton
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // ��� IHttpContextAccessor ������ע������ ���� HttpContext
builder.Services.AddSingleton<ILogService, LogService>();// ��� LogService ������ע������ ��־����
builder.Services.AddSingleton<IIPQueryService,IPQueryService>();// ��� IPQueryService ������ע������ IP��ѯ����
#endregion

//AddTransient��ÿ�����󶼻ᴴ����ʵ������������״̬����
#region ����AddTransient
builder.Services.AddTransient<IAuthService, AuthService>();// ��� AuthService ������ע������ �����֤����
builder.Services.AddTransient<IFileService, FileService>(); // ��� FileService ������ע������ �ļ�����
#endregion

//AddScoped����ͬһ�������������ڷ�����ͬʵ������������������������豣��״̬һ�µķ���
#region ����AddScoped
builder.Services.AddScoped<IJwtService, JwtService>(); // ��� JwtService ������ע������ JWT ����

#endregion

#endregion

#region JWT��֤
// ��� JWT ����
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));
// ��ȡappsettings.json��JwtConfig����
var jwtConfig = builder.Configuration.GetSection("JwtConfig").Get<JwtConfig>();
var jwtConfigSection = builder.Configuration.GetSection("JwtConfig:");
// ��ȡ�����ļ��е� JWT ����
builder.Configuration.Bind("JwtConfig", jwtConfig);
builder.Services
    .AddAuthentication(option =>
    {
        //��֤middleware����
        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // ָ�������������ͣ��ؼ����ã�
            NameClaimType = "RainbowId", // ����������������޸�

            // ������֤����
            ValidateIssuer = true,
            ValidateAudience = true,
            //Token�䷢����
            ValidIssuer = jwtConfig.Issuer,
            //�䷢��˭
            ValidAudience = jwtConfig.Audience,
            //�����keyҪ���м���
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey)),
            //�Ƿ���֤Token��Ч�ڣ�ʹ�õ�ǰʱ����Token��Claims�е�NotBefore��Expires�Ա�
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // �ϸ�ʱ����֤
        };
    });
#endregion

#region Swagger����
builder.Services.AddSwaggerGen(opt =>
{
    #region ���ýӿ�ע��

    string xmlPath = Path.Combine(AppContext.BaseDirectory, "NET+MySQL.xml");// xml ����һ�����Ŀ����һ�¼���
    opt.IncludeXmlComments(xmlPath);

    #endregion

    #region ���ýӿڷ����汾

    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "���汾 API",
        Description = "���汾��API",
    });
    opt.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2",
        Title = "�ڶ��汾 API",
        Description = "�ڶ��汾��API",
    });
    opt.SwaggerDoc("v3", new OpenApiInfo
    {
        Version = "v3",
        Title = "�����汾 API",
        Description = "�����汾��API",
    });

    #endregion

    #region ���ýӿ�token��֤
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Description = "���¿�����������ͷ����Ҫ���Jwt��ȨToken��Bearer Token",
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

#region ��������
builder.Services.AddCors(o => o.AddPolicy("MyCors", b => b.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod()));
#endregion

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



#region ����CORS
app.UseCors("MyCors");
#endregion

#region ����HTTP����ܵ�
if (app.Environment.IsDevelopment())
{
    #region ����Swagger
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
#region ����м��
app.UseMiddleware<RealIpMiddleware>();// ��ӻ�ȡ��ʵIP���м��
app.UseMiddleware<LoggingMiddleware>();// �����־�м��
#endregion
app.Run();
