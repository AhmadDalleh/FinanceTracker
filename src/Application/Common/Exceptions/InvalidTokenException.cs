namespace Application.Common.Exceptions;

public class InvalidTokenException : Exception
{
    public InvalidTokenException() : base("The reset token is invalid or has expired.")
    {
    }
}
