using System;

namespace Reex.Models.v1.ApiRequest
{
    /// <summary>
    /// Request model to create a new wallet address
    /// </summary>
    public class CreateWalletAddress
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Label { get; set; }
    }
}
