namespace QREventPlatform.Advanced.DTOs;

class LoginUserDto
{
    public Guid Id { get; set; }
    public string PasswordHash { get; set; } = null!;
    public int Role { get; set; }
    public bool IsActive { get; set; }
}
