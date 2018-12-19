using Reex.Models.v1.Wallet;
using System.Threading.Tasks;

namespace Reex.Services.RehiveService
{
    public interface IRehiveService
    {
        Task<VerifyToken> VerifyUser(string token);
        Task<VerifyToken> VerifyTwoFactor(int code, string token);
    }
}
