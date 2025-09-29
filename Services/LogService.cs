using MeowMemoirsAPI.Interfaces;
using MeowMemoirsAPI.Models.Log;
using System.Text;

namespace MeowMemoirsAPI.Services
{
    /// <summary>
    /// 日志服务
    /// </summary>
    public class LogService : ILogService
    {
        private sealed class LogFileState
        {
            public string Directory { get; }
            public string BaseFileName { get; }
            public long MaxFileSizeBytes { get; }
            public string CurrentFilePath { get; private set; }
            public DateTime CurrentFileDate { get; private set; }

            public LogFileState(string directory, string baseFileName, long maxFileSizeBytes)
            {
                Directory = directory;
                BaseFileName = baseFileName;
                MaxFileSizeBytes = maxFileSizeBytes;
                CurrentFileDate = DateTime.Today;
                CurrentFilePath = GenerateNewFilePath();
                EnsureDirectoryExists();
            }

            public void RotateIfNeeded()
            {
                if (DateTime.Today > CurrentFileDate)
                {
                    CurrentFileDate = DateTime.Today;
                    CurrentFilePath = GenerateNewFilePath();
                }

                if (File.Exists(CurrentFilePath) &&
                    new FileInfo(CurrentFilePath).Length >= MaxFileSizeBytes)
                {
                    CurrentFilePath = GenerateNewFilePath();
                }
            }

            private string GenerateNewFilePath()
            {
                var now = DateTime.Now;
                string baseName = $"{now:yyyyMMdd}_{BaseFileName}";
                int index = 1;

                string newFilePath;
                do
                {
                    newFilePath = Path.Combine(Directory, $"{baseName}_{index:D3}.log");
                    index++;
                } while (File.Exists(newFilePath));

                return newFilePath;
            }

            private void EnsureDirectoryExists()
            {
                if (!System.IO.Directory.Exists(Directory))
                {
                    System.IO.Directory.CreateDirectory(Directory);
                }
            }
        }

        private readonly LogFileState _accessLog;
        private readonly LogFileState _errorLog;
        private readonly LogFileState _loginLog;
        private readonly LogFileState _runLog;
        private readonly object _lock = new();

        /// <summary>
        /// 日志服务构造函数
        /// </summary>
        /// <param name="configuration"></param>
        public LogService(IConfiguration configuration)
        {
            long maxSize = configuration.GetValue<long>("Logging:MaxFileSizeBytes", 1048576);
            //long maxSize = long.Parse(configuration["Logging:MaxFileSizeBytes"]!);

            _accessLog = new LogFileState(
                directory: configuration["Logging:LogAccessPath"]?? "/www/wwwroot/www.meowmemoirs.cn.api/",
                baseFileName: "access",
                maxFileSizeBytes: maxSize
            );

            _errorLog = new LogFileState(
                directory: configuration["Logging:LogErrorPath"]?? "/www/wwwroot/www.meowmemoirs.cn.api/",
                baseFileName: "error",
                maxFileSizeBytes: maxSize
            );

            _loginLog = new LogFileState(
                directory: configuration["Logging:LogLogInPath"] ?? "/www/wwwroot/www.meowmemoirs.cn.api/",
                baseFileName: "login",
                maxFileSizeBytes: maxSize
            );
            _runLog = new LogFileState(
                directory: configuration["Logging:LogRunPath"]!,
                baseFileName: "run",
                maxFileSizeBytes: maxSize
            );
        }
        /// <summary>
        /// 记录访问日志
        /// </summary>
        /// <param name="logEntry"></param>
        public void LogEntry(LogEntry logEntry)
        {
            var content = new StringBuilder()
                .AppendLine($"Token: {logEntry.Token}")
                .AppendLine($"IP: {logEntry.Ip}")
                .AppendLine($"Device: {logEntry.DeviceInfo ?? "N/A"}")
                .AppendLine($"Path: {logEntry.RequestPath}")
                .AppendLine($"Time: {logEntry.AccessTime:yyyy-MM-dd HH:mm:ss zzz}")
                .AppendLine($"Result: {logEntry.Result}")
                .AppendIf(!string.IsNullOrEmpty(logEntry.RequestBody), $"Body: {logEntry.RequestBody}")
                .AppendLine(new string('-', 50))
                .ToString();

            WriteLog(_accessLog, content);
        }
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="logError"></param>
        public void LogError(LogError logError)
        {
            var content = new StringBuilder()
                .AppendLine($"Token: {logError.Token}")
                .AppendLine($"Time: {logError.DateTime:yyyy-MM-dd HH:mm:ss zzz}")
                .AppendLine($"IP: {logError.Ip}")
                .AppendLine($"Device: {logError.DeviceInfo ?? "N/A"}")
                .AppendLine($"Name: {logError.Name}")
                .AppendLine($"Message: {logError.Message}")
                .AppendIf(!string.IsNullOrEmpty(logError.RequestBody), $"Body: {logError.RequestBody}")
                .AppendLine(new string('-', 50))
                .ToString();

            WriteLog(_errorLog, content);
        }
        /// <summary>
        /// 记录登录日志
        /// </summary>
        /// <param name="logLogIn"></param>
        public void LogLogin(LogLogIn logLogIn)
        {
            var content = new StringBuilder()
                .AppendLine($"Token: {logLogIn.Token}")
                .AppendLine($"Time: {logLogIn.DateTime:yyyy-MM-dd HH:mm:ss zzz}")
                .AppendLine($"Message: {logLogIn.Message}")
                .AppendLine($"IP: {logLogIn.Ip}")
                .AppendLine($"Device: {logLogIn.DeviceInfo ?? "N/A"}")
                .AppendIf(!string.IsNullOrEmpty(logLogIn.RequestBody), $"Body: {logLogIn.RequestBody}")
                .AppendLine(new string('-', 50))
                .ToString();

            WriteLog(_loginLog, content);
        }
        /// <summary>
        /// 记录运行日志
        /// </summary>
        /// <param name="logRun"></param>
        public void LogRun(LogRun logRun)
        {
            var content = new StringBuilder()
                .AppendLine($"Content: {logRun.Content}")
                .AppendLine($"CreateTime: {logRun.CreateTime:yyyy-MM-dd HH:mm:ss zzz}")
                .AppendLine($"CreateUser: {logRun.CreateUser}")
                .AppendIf(!string.IsNullOrEmpty(logRun.Remark), $"Remark: {logRun.Remark}")
                .AppendLine(new string('-', 50))
                .ToString();

            WriteLog(_runLog, content);
        }

        private void WriteLog(LogFileState state, string content)
        {
            lock (_lock)
            {
                try
                {
                    state.RotateIfNeeded();
                    File.AppendAllText(state.CurrentFilePath, content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write log: {ex.Message}");
                }
            }
        }
    }

    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendIf(this StringBuilder sb, bool condition, string value)
        {
            return condition ? sb.AppendLine(value) : sb;
        }
    }
}