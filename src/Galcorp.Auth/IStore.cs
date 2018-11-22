using System.Threading.Tasks;

namespace Galcorp.Auth
{
    public interface IStore
    {
        Task Store(string key, object value);
    }
}