namespace ECHO.Ticket.Core.Results;

// T, geriye döneceğimiz verinin tipini (Örn: Event, User, List<Event>) temsil eder.
public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool isSuccess, string message, T? data) : base(isSuccess, message)
    {
        Data = data;
    }

    // Hem başarılı hem de data dönen durum
    public static Result<T> Success(T data, string message = "")
    {
        return new Result<T>(true, message, data);
    }

    // Hatalı durum (Data null döner)
    public static new Result<T> Failure(string message)
    {
        return new Result<T>(false, message, default);
    }
}