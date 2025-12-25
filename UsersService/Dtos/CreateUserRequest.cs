using System.ComponentModel.DataAnnotations;

namespace UsersService.Dtos;

public class CreateUserRequest
{
    [Required]
    [EmailAddress] // 👈 проверка формата email
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
}
