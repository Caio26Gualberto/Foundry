namespace Boilerplate.Application.Common.Results
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T Data { get; }
        public Error Error { get; }
        public bool IsFailure => !IsSuccess;

        public Result(T data)
        {
            IsSuccess = true;
            Data = data;
            Error = null!;
        }

        public Result(Error error)
        {
            IsSuccess = false;
            Error = error;
            Data = default!;
        }

        public static Result<T> Ok(T data) => new(data);
        public static Result<T> Fail(Error error) => new(error);
    }
}
