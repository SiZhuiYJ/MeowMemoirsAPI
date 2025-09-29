namespace MeowMemoirsAPI.Models.IP
{
    public class IPLocation
    {
        public string IP { get; set; }
        public string Country { get; set; }
        public string Area { get; set; }
        public string ISP { get; set; }
        public string Source { get; set; }

        public IPLocation()
        {
            Country = "未知";
            Area = "未知";
            ISP = "";
            Source = "未知";
        }
    }
}
