namespace MeowMemoirsAPI.Models.Http
{        /// <summary>
         /// HTTP数据传输对象
         /// </summary>
    public class HttpData
    {
        /// <summary>
        /// 通知编号
        /// </summary>
        public required int Code { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public Object? Data { get; set; }

        /// <summary>
        /// 返回通知
        /// </summary>
        public required string Message { get; set; }

    }
}
