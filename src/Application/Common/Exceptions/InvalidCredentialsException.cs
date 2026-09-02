namespace Application.Common.Exceptions;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Email or password is incorrect.")
    {
    }
}
