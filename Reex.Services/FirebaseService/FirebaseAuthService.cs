using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Reex.Models.v1.ApiRequest;
using Reex.Models.v1.ApiResponse;

namespace Reex.Services.FirebaseService
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        #region fields
        private readonly string firebaseApiKey;
        private readonly string firebaseDatabase;
        private readonly IFirebaseAuthProvider authProvider;
        private readonly IMemoryCache memoryCache;
        private readonly MemoryCacheEntryOptions cacheEntryOptions;
        #endregion

        #region properties
        #endregion

        #region constructors
        public FirebaseAuthService(IConfiguration configuration, IMemoryCache memoryCache)
        {
            this.firebaseApiKey = configuration["FirebaseApiKey"];
            this.firebaseDatabase = configuration["FirebaseDatabase"];
            this.authProvider = new FirebaseAuthProvider(new FirebaseConfig(this.firebaseApiKey));
            this.memoryCache = memoryCache;
            this.cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(int.Parse(configuration["CacheExpiryInMinutes"] ?? "60")));
        }
        #endregion

        #region public methods
        public async Task<LoginStatus> Login(Login login)
        {
            var auth = await authProvider.SignInWithEmailAndPasswordAsync(login.Email, login.Password);

            if(auth is null || auth.User is null)
            {
                throw new FirebaseAuthException(string.Empty, string.Empty, string.Empty, new InvalidOperationException());
            }

            return new LoginStatus(auth.User.LocalId, auth.User.Email, auth.User.IsEmailVerified, auth.ExpiresIn, auth.FirebaseToken, auth.RefreshToken);
        }

        public async Task<UserDetail> GetUser(string firebaseToken)
        {
            User cachedUser;
            var cacheKey = firebaseToken;
            bool doesExists = memoryCache.TryGetValue(cacheKey, out cachedUser);

            if(!doesExists)
            {
                var user = await authProvider.GetUserAsync(firebaseToken);

                if (user is null)
                {
                    throw new FirebaseAuthException(string.Empty, string.Empty, string.Empty, new InvalidOperationException());
                }

                if(user.IsEmailVerified)
                {
                    memoryCache.Set(cacheKey, user, cacheEntryOptions);
                }

                return new UserDetail(user.LocalId, user.Email, user.IsEmailVerified, false);
            }

            return new UserDetail(cachedUser.LocalId, cachedUser.Email, cachedUser.IsEmailVerified, false);
        }

        public async Task<LoginStatus> RegisterUser(Register register)
        {
            var result = await authProvider.CreateUserWithEmailAndPasswordAsync(register.Email, register.Password, string.Empty, true);

            if (result is null || result.User is null)
            {
                throw new FirebaseAuthException(string.Empty, string.Empty, string.Empty, new InvalidOperationException());
            }

            return new LoginStatus(result.User.LocalId, result.User.Email, result.User.IsEmailVerified, result.ExpiresIn, result.FirebaseToken, result.RefreshToken);
        }

        public async Task VerifyEmail(string firebaseToken)
        {
            await authProvider.SendEmailVerificationAsync(firebaseToken);
        }

        public async Task SendPasswordResetEmail(string email)
        {
            await authProvider.SendPasswordResetEmailAsync(email);
        }
        #endregion
    }
}
