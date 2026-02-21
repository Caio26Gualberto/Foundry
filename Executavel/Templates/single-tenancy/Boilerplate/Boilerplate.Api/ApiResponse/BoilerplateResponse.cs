namespace Boilerplate.Api.ApiResponse
{
    public class BoilerplateResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
