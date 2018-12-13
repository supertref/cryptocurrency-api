using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using Reecore.Reex.Models;
using System;
using System.Collections.Generic;

namespace Reecore.Reex.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ReexController : ControllerBase
    {
        private readonly Network network;
        private static Wallet wallet;

        public ReexController(NBitcoin.Altcoins.Reex instance)
        {
            network = instance.Mainnet;
        }

        // GET api/v1/reex/create
        [HttpGet]
        [Route("wallets/{email}")]
        public ActionResult<IList<Address>> GetWallets(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return NotFound();
            }

            return Ok(wallet.Addresses);
        }

        // POST api/v1/reex/create
        [HttpPost]
        [Route("create/{email}")]
        public ActionResult<WalletCreated> CreateWallet(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return NotFound();
            }

            var walletId = Guid.NewGuid();
            var privateKey = new Key();
            var reexPrivateKey = privateKey.GetWif(network);
            var reexPublicKey = privateKey.PubKey.GetAddress(network);
            var walletLabel = "Main Wallet";
            var addressLabel = "Main Address";

            var addresses = new List<Address>()
            {
                new Address(Guid.NewGuid(), walletId, reexPublicKey.ToString(), addressLabel)
            };

            wallet = new Wallet(walletId, reexPrivateKey.ToString(), true, walletLabel, email, addresses);

            return Ok(new WalletCreated(wallet.ID, reexPublicKey.ToString(), addressLabel));
        }

        // POST api/v1/reex/create/address
        [HttpPost]
        [Route("create/{email}/address/{label}")]
        public ActionResult<IList<Address>> CreateAddress(string email, string label)
        {
            if(string.IsNullOrWhiteSpace(email))
            {
                return NotFound();
            }

            if(string.IsNullOrWhiteSpace(label))
            {
                return BadRequest(new ArgumentNullException(nameof(label)).Message);
            }

            var privateKey = new Key();
            var newAddress = new Address(Guid.NewGuid(), wallet.ID, string.Empty, label);
            wallet.Addresses.Add(newAddress);

            return Ok(newAddress);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
