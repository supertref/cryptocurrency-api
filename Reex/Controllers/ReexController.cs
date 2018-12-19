using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public ReexController(IWalletManagementService walletManagementService, IRehiveService rehiveService)
        {
            this.walletManagementService = walletManagementService;
            this.rehiveService = rehiveService;
        }

        // GET api/v1/reex/create
        [HttpGet]
        [Route("wallet/{id}/{email}")]
        public async Task<ActionResult<Wallet>> GetWallet(Guid id, string email)
        {
            if(string.IsNullOrEmpty(email))
            {
                return NotFound(RequestResponse.NotFound());
            }

            var result = await walletManagementService.GetWallets(id, email);

            if(!result.Any())
            {
                return NotFound(RequestResponse.NotFound("Need to initialise wallet for user"));
            }

            return Ok(result.Select(x => new Wallet(x.ID, x.UserId, null, null, true, x.Label, x.Email, x.Addresses)).FirstOrDefault());
        }

        // GET api/v1/reex/create
        [HttpGet]
        [Route("getbalance/{id}/{email}")]
        public async Task<ActionResult<Balance>> GetBalance(Guid id, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return NotFound();
            }

            var result = await walletManagementService.GetBalance(id, email);
            return Ok(result);
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

                try
                {
                    var result = await walletManagementService.SpendCoins(request);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, RequestResponse.InternalServerError());
                }
            }
            catch(Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, RequestResponse.InternalServerError());
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
