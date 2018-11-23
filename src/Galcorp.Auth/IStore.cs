using System.Threading.Tasks;

namespace Galcorp.Auth
{
    public interface IStore
    {
        Task Store(string key, object value);
        Task<T> Read<T>(string key)
            where T: class;
    }
}