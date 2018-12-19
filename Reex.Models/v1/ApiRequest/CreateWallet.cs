using System;

namespace Reex.Models.v1.ApiRequest
{
    public class CreateWallet
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
    }
}
