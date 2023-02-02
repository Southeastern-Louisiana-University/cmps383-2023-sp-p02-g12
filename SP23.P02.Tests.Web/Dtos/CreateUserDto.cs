namespace SP23.P02.Tests.Web.Dtos;

internal class CreateUserDto
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string[]? Roles { get; set; }
}
