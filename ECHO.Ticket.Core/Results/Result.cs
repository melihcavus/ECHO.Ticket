namespace ECHO.Ticket.Core.Results;

public class Result
{
    public bool IsSuccess { get; }
    public string Message { get; }

    // Constructor'ları dışarıdan new'lenemesin diye protected yapıyoruz. 
    // Sadece alttaki Success ve Failure metodlarıyla üretilecekler.
    protected Result(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    // Başarılı durumlar için
    public static Result Success(string message = "")
    {
        return new Result(true, message);
    }

    // Hatalı durumlar için
    public static Result Failure(string message)
    {
        return new Result(false, message);
    }
}