namespace SnapLink.api.Crosscutting
{
    public class Result<T>
    {
        public T Data { get;  set; }
        public bool Success { get;  set; }
        public string Message { get; set; }

        public Result(T data, bool success, string message)
        {
            Data = data;
            Success = success;
            Message = message;
        }

        public static Result<T> Ok(T data, string message = null)
            => new Result<T>(data, true, message ?? "Operation completed successfully.");

        public static Result<T> Ok(string message = null)
            => new Result<T>(default, true, message ?? "Operation completed successfully.");

        public static Result<T> Fail(string message)
            => new Result<T>(default, false, message);

        public static Result<T> Fail(T data, string message)
            => new Result<T>(data, false, message);
    }
}
