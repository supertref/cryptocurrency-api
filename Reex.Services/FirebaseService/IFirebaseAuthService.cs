using Reex.Models.v1.ApiRequest;
using Reex.Models.v1.ApiResponse;
using System.Threading.Tasks;

namespace Reex.Services.FirebaseService
{
    public interface IFirebaseAuthService
    {
        Task<LoginStatus> Login(Login login);
        Task<LoginStatus> RegisterUser(Register register);
        Task<UserDetail> GetUser(string firebaseToken);
        Task VerifyEmail(string firebaseToken);
        Task SendPasswordResetEmail(string email);
    }
}
