using aspnetauthentication.Models.Requests;
using aspnetauthentication.Models.Responses;
using System.Collections.Generic;

namespace aspnetauthentication.Models
{
    public class BadRequestApiResponse:ApiResponse<object>
    {
        public BadRequestApiResponse(string message, string code = "400.1") : base(message, null)
        {
            Code = code;
        }

        public IEnumerable<BadRequestModel> Errors { get; set; }
    }
}
