using System.ComponentModel.DataAnnotations;
using UsersService.Models;

namespace UsersService.Dtos;

public class CreateOrderRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public StatusOrder Status { get; set; }

    [Required]
    public float Amount { get; set; }
}
