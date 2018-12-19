namespace Reex.Models.v1.Wallet
{
    public class VerifyToken
    {
        #region properties
        public string Status { get; set; }
        public string Message { get; set; }
        public VerifyTokenData Data { get; set; }
        #endregion
    }
}
