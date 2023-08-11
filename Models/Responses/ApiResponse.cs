namespace aspnetauthentication.Models.Responses
{
    public class ApiResponse<T> 
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public T Data { get; set; }
        public ApiResponse(string message, T data)
        {
            Message = message;
            Data = data;
        }

        public ApiResponse()
        {
        }       
    }
}
