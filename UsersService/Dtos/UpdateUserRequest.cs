using System.ComponentModel.DataAnnotations;

public class UpdateUserRequest
{
    [EmailAddress] // формат email
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }
}
