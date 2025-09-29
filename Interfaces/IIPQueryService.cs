using MeowMemoirsAPI.Models.IP;

namespace MeowMemoirsAPI.Interfaces
{
    public interface IIPQueryService
    {
        IPLocation Query(string ip);
    }
}
