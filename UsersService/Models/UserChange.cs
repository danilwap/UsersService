namespace UsersService.Models;

public enum UserChangeType
{
    Created = 1,
    Updated = 2,
    Deleted = 3
}

public class UserChange
{
    public long Id { get; set; }              // удобно авто-инкрементом
    public Guid UserId { get; set; }
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    public UserChangeType ChangeType { get; set; }

    // кто изменил (пока строкой; позже можно заменить на пользователя/токен)
    public string? ChangedBy { get; set; }

    // что было/стало (храним JSON строкой — просто и гибко)
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }

}
