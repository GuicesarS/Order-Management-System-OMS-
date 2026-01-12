namespace OrderManagement.Application.Common;

public class Result<T>
{
    public bool Success { get; }
    public string ErrorMessage { get; }
    public T? Data { get; }

    public Result(bool success, string errorMessage, T? data)
    {
        Success = success;
        Data = data;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Ok(T data) => new Result<T>(true, string.Empty, data);
    public static Result<T> Failure(string errorMessage) => new Result<T>(false, errorMessage, default);
}
