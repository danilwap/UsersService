using UsersService.Models;

namespace UsersService.Dtos
{
    public class UpdateOrderRequest
    {
        public StatusOrder? Status { get; set; }
        public float? Amount { get; set; }

    }
}
