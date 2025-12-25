namespace UsersService.Dtos;

public class UserChangeDto
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime ChangedAtUtc { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public string? ChangedBy { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
}
