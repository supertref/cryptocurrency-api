using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Reex.Models.v1;
using Reex.Models.v1.ApiRequest;
using Reex.Models.v1.ApiResponse;
using Reex.Services.FirebaseService;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TwoFactorAuthNet;

namespace Reex.Controllers
{
    [Authorize(AuthenticationSchemes = "TokenAuthentication")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        #region constants
        private const string MFA_CACHE_KEY = "mfa-userId-cache-key-";
        #endregion

        #region fields
        private readonly IFirebaseAuthService firebaseAuthService;
        private readonly IFirebaseDbService firebaseDbService;
        private readonly IMemoryCache memoryCache;
        private readonly MemoryCacheEntryOptions cacheEntryOptions;
        private readonly TwoFactorAuth twoFactorAuth;
        #endregion

        #region constructors
        public AccountController(IFirebaseAuthService firebaseAuthService, IFirebaseDbService firebaseDbService, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this.firebaseAuthService = firebaseAuthService;
            this.firebaseDbService = firebaseDbService;
            this.memoryCache = memoryCache;
            this.cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(int.Parse(configuration["CacheExpiryInMinutes"] ?? "60")));
            this.twoFactorAuth = new TwoFactorAuth(configuration["MyCompany"]);
        }
        #endregion

        #region actions
        // POST api/v1/account/authenticate
        [AllowAnonymous]
        [HttpPost]
        [Route("authenticate")]
        public async Task<ActionResult<ApiResponse<LoginStatus>>> Login([FromBody] Login request)
        {
            try
            {
                var result = await firebaseAuthService.Login(request);

                return Ok(new ApiResponse<LoginStatus>(result));
            }
            catch(Exception)
            {
                return BadRequest(RequestResponse.BadRequest("Something went wrong trying to login."));
            }
        }

        // POST api/v1/account/register
        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<ApiResponse<LoginStatus>>> RegisterUser([FromBody] Register register)
        {
            try
            {
                if (register is null || string.IsNullOrWhiteSpace(register.Email) || string.IsNullOrWhiteSpace(register.Password))
                {
                    return BadRequest(RequestResponse.BadRequest("Please check your input."));
                }

                var result = await firebaseAuthService.RegisterUser(register);

                return Ok(new ApiResponse<LoginStatus>(result));
            }
            catch (Exception)
            {
                return BadRequest(RequestResponse.BadRequest("Something went wrong during the registration."));
            }
        }

        // POST api/v1/account/resetPassword
        [AllowAnonymous]
        [HttpPost]
        [Route("resetPassword")]
        public async Task<ActionResult> ResetPassword([FromBody] PasswordReset passwordReset)
        {
            try
            {
                if(passwordReset is null || string.IsNullOrWhiteSpace(passwordReset.Email))
                {
                    return BadRequest(RequestResponse.BadRequest("Please check your input."));
                }

                await firebaseAuthService.SendPasswordResetEmail(passwordReset.Email);

                return Ok();
            }
            catch(Exception)
            {
                return BadRequest(RequestResponse.BadRequest("Something went wrong trying to send reset email."));
            }
        }

        // POST api/v1/account/verifyEmail
        [HttpPost]
        [Route("verifyEmail")]
        public async Task<ActionResult> VerifyEmail()
        {
            try
            {
                AuthenticationHeaderValue authHeader;
                AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out authHeader);

                if (authHeader is null)
                {
                    return NotFound(RequestResponse.NotFound("Invalid request."));
                }

                await firebaseAuthService.SendPasswordResetEmail(authHeader.Parameter);

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(RequestResponse.BadRequest("Something went wrong trying to send verify email."));
            }
        }

        // POST api/v1/account/mfa/enable
        [HttpPost]
        [Route("mfa/enable")]
        public ActionResult<ApiResponse<UserProperties>> EnableMfa()
        {
            try
            {
                var userName = User?.Identity?.Name;
                var userId = User?.Claims.Where(x => x.Type == CustomClaims.USER_ID).FirstOrDefault()?.Value;

                if(string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(RequestResponse.BadRequest("Something went wrong trying to validate your request."));
                }

                string secret;
                var cacheKey = $"{MFA_CACHE_KEY}{userId}";
                bool doesExists = memoryCache.TryGetValue(cacheKey, out secret);

                if(!doesExists)
                {
                    secret = twoFactorAuth.CreateSecret(160);
                    memoryCache.Set(cacheKey, secret, cacheEntryOptions);
                    var result = new UserProperties(userId, secret);
                    return Ok(new ApiResponse<UserProperties>(result));
                }

                var cacheResult = new UserProperties(userId, secret);
                return Ok(new ApiResponse<UserProperties>(cacheResult));
            }
            catch(Exception)
            {
                return BadRequest(RequestResponse.BadRequest("Something went wrong trying to enable Mfa."));
            }
        }

        // POST api/v1/account/mfa/verify
        [HttpPost]
        [Route("mfa/verify")]
        public async Task<ActionResult<ApiResponse<UserProperties>>> VerifyMfaEnable([FromBody] VerifyMfa mfaEnable)
        {
            try
            {
                if(mfaEnable is null || string.IsNullOrWhiteSpace(mfaEnable.MfaCode))
                {
                    return BadRequest(RequestResponse.BadRequest("Mfa code is required for verification."));
                }

                var userName = User?.Identity?.Name;
                var userId = User?.Claims.Where(x => x.Type == CustomClaims.USER_ID).FirstOrDefault()?.Value;

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(RequestResponse.BadRequest("Something went wrong trying to validate your request."));
                }

                string secret;
                var cacheKey = $"{MFA_CACHE_KEY}{userId}";
                bool doesExists = memoryCache.TryGetValue(cacheKey, out secret);

                if (!doesExists)
                {
                    return BadRequest(RequestResponse.BadRequest("Something went wrong. Please try restart the Mfa process."));
                }

                var verified = twoFactorAuth.VerifyCode(secret, mfaEnable.MfaCode);

                if(!verified)
                {
                    return BadRequest(RequestResponse.BadRequest("Invalid Mfa code provided. Please try again."));
                }

                var cacheResult = new UserProperties(userId, secret, true);
                var currentPropertiesKey = await firebaseDbService.GetUserPropertiesKey(userId);
                if (!string.IsNullOrWhiteSpace(currentPropertiesKey))
                {
                    await firebaseDbService.UpdateUserProperties(currentPropertiesKey, cacheResult);
                }
                else
                {
                    await firebaseDbService.CreateUserProperties(cacheResult);
                }

                return Ok(new ApiResponse<UserProperties>(cacheResult));
            }
            catch (Exception)
            {
                return BadRequest(RequestResponse.BadRequest("Something went wrong trying to enable Mfa."));
            }
        }

        // POST api/v1/account/mfa/disable
        [HttpPost]
        [Route("mfa/disable")]
        public async Task<ActionResult<ApiResponse<UserProperties>>> VerifyMfaDisable([FromBody] VerifyMfa mfaEnable)
        {
            try
            {
                if (mfaEnable is null || string.IsNullOrWhiteSpace(mfaEnable.MfaCode))
                {
                    return BadRequest(RequestResponse.BadRequest("Mfa code is required for verification."));
                }

                var userName = User?.Identity?.Name;
                var userId = User?.Claims.Where(x => x.Type == CustomClaims.USER_ID).FirstOrDefault()?.Value;

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(RequestResponse.BadRequest("Something went wrong trying to validate your request."));
                }

                string secret;
                var cacheKey = $"{MFA_CACHE_KEY}{userId}";
                bool doesExists = memoryCache.TryGetValue(cacheKey, out secret);

                if (!doesExists)
                {
                    return BadRequest(RequestResponse.BadRequest("Something went wrong. Please try restart the Mfa process."));
                }

                var verified = twoFactorAuth.VerifyCode(secret, mfaEnable.MfaCode);

                if (!verified)
                {
                    return BadRequest(RequestResponse.BadRequest("Invalid Mfa code provided. Please try again."));
                }

                var currentPropertiesKey = await firebaseDbService.GetUserPropertiesKey(userId);

                if(!string.IsNullOrWhiteSpace(currentPropertiesKey))
                {
                    return BadRequest(RequestResponse.BadRequest("Mfa record not found. Please enable Mfa."));
                }

                var cacheResult = new UserProperties(userId, secret, false);
                await firebaseDbService.UpdateUserProperties(currentPropertiesKey, cacheResult);
                return Ok(new ApiResponse<UserProperties>(cacheResult));
            }
            catch (Exception)
            {
                return BadRequest(RequestResponse.BadRequest("Something went wrong trying to enable Mfa."));
            }
        }

        // POST api/v1/account/mfa/auth
        [HttpPost]
        [Route("mfa/auth")]
        public async Task<ActionResult<RequestResponse>> MfaAuth([FromBody] VerifyMfa mfaEnable)
        {
            try
            {
                if (mfaEnable is null || string.IsNullOrWhiteSpace(mfaEnable.MfaCode))
                {
                    return BadRequest(RequestResponse.BadRequest("Mfa code is required for verification."));
                }

                var userName = User?.Identity?.Name;
                var userId = User?.Claims.Where(x => x.Type == CustomClaims.USER_ID).FirstOrDefault()?.Value;

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(RequestResponse.BadRequest("Something went wrong trying to validate your request."));
                }

                var userProperties = await firebaseDbService.GetUserProperties(userId);

                if(userProperties is null)
                {
                    return NotFound(RequestResponse.NotFound("Error finding the data you are looking for."));
                }

                if(!userProperties.IsMfaEnabled)
                {
                    return BadRequest(RequestResponse.BadRequest("Mfa not enabled for this user."));
                }

                var verified = twoFactorAuth.VerifyCode(userProperties.Secret, mfaEnable.MfaCode);

                if (!verified)
                {
                    return BadRequest(RequestResponse.BadRequest("Invalid Mfa code provided. Please try again."));
                }

                return Ok(RequestResponse.Success());
            }
            catch (Exception)
            {
                return BadRequest(RequestResponse.BadRequest("Something went wrong trying to enable Mfa."));
            }
        }

        // GET api/v1/account/getUser/{token}
        [HttpGet]
        [Route("getUser")]
        public async Task<ActionResult<ApiResponse<UserDetail>>> GetUserDetail()
        {
            try
            {
                AuthenticationHeaderValue authHeader;
                AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out authHeader);

                if(authHeader is null)
                {
                    return NotFound(RequestResponse.NotFound("Invalid request."));
                }

                var result = await firebaseAuthService.GetUser(authHeader.Parameter);
                var userPropertiesResult = await firebaseDbService.GetUserProperties(result.UserId);
                result.IsMfaEnabled = userPropertiesResult.IsMfaEnabled;

                return Ok(new ApiResponse<UserDetail>(result));
            }
            catch (Exception)
            {
                return BadRequest(RequestResponse.BadRequest("An error occured while trying to get your details."));
            }
        }
        #endregion
    }
}