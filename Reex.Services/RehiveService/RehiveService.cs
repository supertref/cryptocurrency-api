using Newtonsoft.Json;
using Reex.Models.v1.Wallet;
using RestSharp;
using System.Threading.Tasks;

namespace Reex.Services.RehiveService
{
    public class RehiveService : IRehiveService
    {
        private const string BASE_URL = "https://api.rehive.com/3/";
        private const string AUTHENTICATION_SCHEME = "Token";
        private readonly RestClient client;

        public RehiveService()
        {
            this.client = new RestClient(BASE_URL);
        }

        #region methods
        public async Task<VerifyToken> VerifyTwoFactor(int code, string token)
        {
            var request = new RestRequest("auth/mfa/verify/", Method.POST);
            request.AddHeader("Authorization", $"{AUTHENTICATION_SCHEME} {token}");
            request.AddParameter("token", code);

            var result = await client.ExecuteTaskAsync(request);

            var response = JsonConvert.DeserializeObject<VerifyToken>(result.Content);

            return response;
        }

        public async Task<VerifyToken> VerifyUser(string token)
        {
            var request = new RestRequest("auth/tokens/verify/", Method.POST);
            request.AddHeader("Authorization", $"{AUTHENTICATION_SCHEME} {token}");
            request.AddParameter("token", token);

            var result = await client.ExecuteTaskAsync(request);

            var response = JsonConvert.DeserializeObject<VerifyToken>(result.Content);

            return response;
        }
        #endregion
    }
}
