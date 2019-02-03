using Newtonsoft.Json;

namespace Reex.Models.v1.ApiResponse
{
    public class ApiResponse<T>
    {
        protected const string SUCCESS = "success";
        protected const string ERROR = "error";

        #region constructors
        public ApiResponse() { }

        public ApiResponse(string message)
        {
            this.Message = message;
        }

        public ApiResponse(T data, string message = "", string status = SUCCESS)
        {
            this.Status = status;
            this.Message = message;
            this.Data = data;
        }
        #endregion

        #region properties
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public T Data { get; set; }
        #endregion
    }
}
