namespace SP23.P02.Tests.Web.Dtos;

internal class UserDto : PasswordGuard
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string[]? Roles { get; set; }
}