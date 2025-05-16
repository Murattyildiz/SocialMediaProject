using System.Threading.Tasks;
using Entities.Concrete;

namespace Business.Abstract
{
    public interface IChatBotService
    {
        Task<string> GetWeatherInfo(string city);
        Task<string> GetLatestNews();
        Task<string> GetUniversityInfo(string query);
        Task<string> ProcessMessage(string message);
    }
} 