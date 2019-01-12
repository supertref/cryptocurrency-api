using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Reex.Models.v1;
using Reex.Models.v1.ApiRequest;
using Reex.Models.v1.ApiResponse;
using Reex.Models.v1.Wallet;
using Reex.Services.FirebaseService;
using Reex.Services.WalletManagementService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TwoFactorAuthNet;

namespace Reex.Controllers
{
    [Authorize(AuthenticationSchemes = "TokenAuthentication")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ReexController : ControllerBase
    {
        #region fields
        private readonly IWalletManagementService walletManagementService;
        private readonly IMemoryCache memoryCache;
        private readonly MemoryCacheEntryOptions cacheEntryOptions;
        private readonly IFirebaseDbService firebaseDbService;
        private readonly TwoFactorAuth twoFactorAuth;
        private readonly string issuer;
        #endregion

        #region constructors
        public ReexController(IWalletManagementService walletManagementService, IMemoryCache memoryCache, IConfiguration configuration, IFirebaseDbService firebaseDbService)
        {
            this.walletManagementService = walletManagementService;
            this.memoryCache = memoryCache;
            this.cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(int.Parse(configuration["CacheExpiryInMinutes"] ?? "60")));
            this.firebaseDbService = firebaseDbService;
            this.issuer = configuration["MyCompany"];
            this.twoFactorAuth = new TwoFactorAuth(issuer);
        }
        #endregion

        #region actions
        // GET api/v1/reex/wallet
        [HttpGet]
        [Route("wallet/{id}/{email}")]
        public async Task<ActionResult<Wallet>> GetWallet(Guid id, string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return NotFound(RequestResponse.NotFound());
                }

                if (User.Identity.Name != email)
                {
                    return BadRequest(RequestResponse.BadRequest());
                }

                var result = await walletManagementService.GetWallet(id, email);

                if (result is null)
                {
                    return NotFound(RequestResponse.NotFound("Need to initialise wallet for user"));
                }

                return Ok(new Wallet(result.WalletId, result.UserId, null, null, true, result.Label, result.Email, result.Addresses));
            }
            catch(Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, RequestResponse.InternalServerError());
            }
        }

        // GET api/v1/reex/wallets
        [HttpGet]
        [Route("wallets/{id}/{email}")]
        public async Task<ActionResult<Wallet>> GetWallets(string id, string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(id))
                {
                    return NotFound(RequestResponse.NotFound());
                }

                if (User.Identity.Name != email)
                {
                    return BadRequest(RequestResponse.BadRequest());
                }

                var result = await walletManagementService.GetWallets(id, email);

                if (!result.Any())
                {
                    return NotFound(RequestResponse.NotFound($"Need to initialise wallets for user"));
                }

                return Ok(result.Select(x => new Wallet(x.WalletId, x.UserId, null, null, true, x.Label, x.Email, x.Addresses)).FirstOrDefault());
            }
            catch(Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, RequestResponse.InternalServerError());
            }
        }

        // GET api/v1/reex/getbalance
        [HttpGet]
        [Route("getbalance/{id}/{email}")]
        public async Task<ActionResult<Balance>> GetBalance(Guid id, string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return NotFound();
                }

                if (User.Identity.Name != email)
                {
                    return BadRequest(RequestResponse.BadRequest());
                }

                var result = await walletManagementService.GetBalance(id, email);
                return Ok(result);
            }
            catch(Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, RequestResponse.InternalServerError());
            }
        }

        // GET api/v1/reex/transactions
        [HttpGet]
        [Route("transactions/{id}/{email}/{from}/{count}")]
        public async Task<ActionResult<TransactionWrapper>> GetTransactions(Guid id, string email, int from = 0, int count = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return NotFound();
                }

                if (User.Identity.Name != email)
                {
                    return BadRequest(RequestResponse.BadRequest());
                }

                var result = await walletManagementService.GetTransactions(id, email, from, count);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, RequestResponse.InternalServerError(ex.Message));
            }
        }

        // GET api/v1/reex/getinfo
        [HttpGet]
        [Route("getInfo")]
        public async Task<ActionResult<BlockChainInfo>> GetInfo()
        {
            try
            {
                BlockChainInfo cachedInfo;
                var cacheKey = $"blockchainInfo";
                bool doesExists = memoryCache.TryGetValue(cacheKey, out cachedInfo);

                if (!doesExists)
                {
                    var result = await walletManagementService.GetInfo();
                    memoryCache.Set(cacheKey, result, cacheEntryOptions);
                    return result;
                }

                return cachedInfo;
            }
            catch(Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, RequestResponse.InternalServerError());
            }
        }

        // POST api/v1/reex/create
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<WalletCreated>> CreateWallet([FromBody] CreateWallet request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest(new ArgumentNullException(nameof(request.Email)).Message);
                }

                if (User.Identity.Name != request.Email)
                {
                    return BadRequest(RequestResponse.BadRequest());
                }

                var result = await walletManagementService.CreateWallet(request);
                return Ok(result);
            }
            catch(Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, RequestResponse.InternalServerError());
            }
        }

        // POST api/v1/reex/createAddress
        [HttpPost]
        [Route("createAddress")]
        public async Task<ActionResult<IList<Address>>> CreateAddress([FromBody] CreateWalletAddress request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new ArgumentNullException(nameof(request.Email)).Message);
                }

                if (string.IsNullOrWhiteSpace(request.Label))
                {
                    return BadRequest(new ArgumentNullException(nameof(request.Label)).Message);
                }

                if (User.Identity.Name != request.Email)
                {
                    return BadRequest(RequestResponse.BadRequest());
                }

                var result = await walletManagementService.CreateAddress(request);
                return Ok(result);
            }
            catch(Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, RequestResponse.InternalServerError());
            }
        }

        // POST api/v1/reex/spendCoins
        [HttpPost]
        [Route("spendCoins")]
        public async Task<ActionResult<CoinTransfer>> SpendCoins([FromBody] SpendCoins request)
        {
            try
            {
                if (request is null || string.IsNullOrEmpty(request.ToAddress))
                {
                    return BadRequest("Invalid request.");
                }

                if (User.Identity.Name != request.Email)
                {
                    return BadRequest(RequestResponse.BadRequest("Invalid request. Please try again."));
                }

                var result = await walletManagementService.SpendCoins(request);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, RequestResponse.InternalServerError(ex.Message));
            }
        }

        // POST/api/v1/reex/exportPrivKey
        [HttpPost]
        [Route("exportPrivKey")]
        public async Task<ActionResult<ApiResponse<ExportPrivateKey>>> ExportPrivKey([FromBody] VerifyMfa mfaEnable)
        {
            try
            {
                if (mfaEnable is null || string.IsNullOrWhiteSpace(mfaEnable.MfaCode))
                {
                    return BadRequest(RequestResponse.BadRequest("Mfa code is required in order to export your private key."));
                }

                var userName = User?.Identity?.Name;
                var userId = User?.Claims.Where(x => x.Type == CustomClaims.USER_ID).FirstOrDefault()?.Value;

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(RequestResponse.BadRequest("Something went wrong trying to validate your request."));
                }

                var userProperties = await firebaseDbService.GetUserProperties(userId);

                if (userProperties is null)
                {
                    return NotFound(RequestResponse.NotFound("Error trying to find your Mfa data. Please try again."));
                }

                if (!userProperties.IsMfaEnabled)
                {
                    return BadRequest(RequestResponse.BadRequest("You must first enable Mfa in order to use this function."));
                }

                var verified = twoFactorAuth.VerifyCode(userProperties.Secret, mfaEnable.MfaCode);

                if (!verified)
                {
                    return BadRequest(RequestResponse.BadRequest("Invalid Mfa code provided. Please try again."));
                }

                var result = await walletManagementService.GetWallets(userId, userName);

                if (!result.Any())
                {
                    return NotFound(RequestResponse.NotFound($"No wallets found."));
                }

                var wallet = result.FirstOrDefault();
                var exportPrivKey = new ExportPrivateKey(wallet.PrivateKey, wallet.Addresses.FirstOrDefault()?.MyAddress);
                return Ok(new ApiResponse<ExportPrivateKey>(exportPrivKey));
            }
            catch(Exception)
            {
                return BadRequest(RequestResponse.BadRequest("Something went wrong trying to export you private key."));
            }
        }
        #endregion
    }
}
