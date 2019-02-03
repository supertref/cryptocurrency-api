using Newtonsoft.Json;

namespace Reex.Models.v1.ApiResponse
{
    public class RequestResponse
    {
        protected const string SUCCESS = "success";
        protected const string ERROR = "error";

        #region constrcutors
        public RequestResponse(string status, string message)
        {
            this.Status = status;
            this.Message = message;
        }
        #endregion

        #region properties
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        #endregion

        #region methods
        public static RequestResponse Success() => new RequestResponse(SUCCESS, null);
        public static RequestResponse NotFound() => new RequestResponse(ERROR, "Data could not be found");
        public static RequestResponse NotFound(string message) => new RequestResponse(ERROR, message);
        public static RequestResponse BadRequest() => new RequestResponse(ERROR, "Something went wrong wile executing your request");
        public static RequestResponse BadRequest(string message) => new RequestResponse(ERROR, message);
        public static RequestResponse InternalServerError() => new RequestResponse(ERROR, "An error occurred while processing your request");
        public static RequestResponse InternalServerError(string message) => new RequestResponse(ERROR, message);
        #endregion
    }
}
