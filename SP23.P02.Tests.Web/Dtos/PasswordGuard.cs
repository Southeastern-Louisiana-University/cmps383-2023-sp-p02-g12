namespace SP23.P02.Tests.Web.Dtos;

internal class PasswordGuard
{
    public string? Password
    {
        get => null;
        set => throw new Exception("You returned a password, don't do this!");
    }

    public string? PasswordHash
    {
        get => null;
        set => throw new Exception("You returned a password, don't do this!");
    }
}