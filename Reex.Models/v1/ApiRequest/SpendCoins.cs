using System;

namespace Reex.Models.v1.ApiRequest
{
    /// <summary>
    /// Request model to spend coins
    /// </summary>
    public class SpendCoins
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        public string ToAddress { get; set; }
        public decimal transferValue { get; set; }
    }
}
