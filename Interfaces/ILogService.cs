using MeowMemoirsAPI.Models.Log;

namespace MeowMemoirsAPI.Interfaces
{
    /// <summary>
    /// 日志服务接口
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// 日志记录
        /// </summary>
        /// <param name="logEntry"></param>
        void LogEntry(LogEntry logEntry);
        /// <summary>
        /// 日志错误记录
        /// </summary>
        /// <param name="logError"></param>
        void LogError(LogError logError);
        /// <summary>
        /// 用户登录日志记录
        /// </summary>
        /// <param name="logLogin"></param>
        void LogLogin(LogLogIn logLogin);

        /// <summary>
        /// 记录运行日志
        /// </summary>
        /// <param name="logRun"></param>
        void LogRun(LogRun logRun);
    }
}
