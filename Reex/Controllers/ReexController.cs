using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Reex.Models.v1.ApiRequest;
using Reex.Models.v1.ApiResponse;
using Reex.Models.v1.Wallet;
using Reex.Services.RehiveService;
using Reex.Services.WalletManagementService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Reex.Controllers
{
    [Authorize(AuthenticationSchemes = "TokenAuthentication")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ReexController : ControllerBase
    {
        private readonly IWalletManagementService walletManagementService;
        private readonly IRehiveService rehiveService;
        private readonly IMemoryCache memoryCache;
        private readonly MemoryCacheEntryOptions cacheEntryOptions;

        public ReexController(IWalletManagementService walletManagementService, IRehiveService rehiveService, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this.walletManagementService = walletManagementService;
            this.rehiveService = rehiveService;
            this.memoryCache = memoryCache;
            this.cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(int.Parse(configuration["CacheExpiryInMinutes"] ?? "60")));
        }

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

                var identity = User.Identity as ClaimsIdentity;
                if (identity is null || identity.Claims.Where(x => x.Type == "ID" && x.Value.ToLower() == id.ToString().ToLower()).FirstOrDefault() is null)
                {
                    return BadRequest(RequestResponse.BadRequest());
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
        public async Task<ActionResult<Wallet>> GetWallets(Guid id, string email)
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
            catch(Exception ex)
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
            catch(Exception ex)
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
            catch(Exception ex)
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
            catch(Exception ex)
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
                    return BadRequest();
                }

                if (User.Identity.Name != request.Email)
                {
                    return BadRequest(RequestResponse.BadRequest());
                }

                var result = await walletManagementService.SpendCoins(request);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, RequestResponse.InternalServerError(ex.Message));
            }
        }

        // POST api/v1/reex/auth/verify/mfa/{code}
        [AllowAnonymous]
        [HttpPost]
        [Route("auth/verify/mfa/{code}")]
        public async Task<ActionResult> VerifyTwoFactorAuth(int code)
        {
            try
            {
                var header = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);

                if (header is null)
                {
                    return BadRequest("Invalid mfa Code");
                }

                var result = await rehiveService.VerifyTwoFactor(code, header.Parameter);

                return Ok(result.Status);
            }
            catch(Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, RequestResponse.InternalServerError());
            }
        }
    }
}
