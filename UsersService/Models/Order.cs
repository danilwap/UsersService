namespace UsersService.Models;

public enum StatusOrder
{
    New = 1,
    Accepted = 2,
    Processing = 3,
    Sent = 4,
    Delivered = 5,
    Canceled = 6
}

public class Order
{
    public long Id { get; set; }
    public DateTime DateOrder { get; set; }
    public Guid UserId { get; set; }
    public StatusOrder Status { get; set; }
    public float Amount { get; set; }

}

