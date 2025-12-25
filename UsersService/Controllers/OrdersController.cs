using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;
using UsersService.Data;
using UsersService.Dtos;
using UsersService.Models;


namespace UsersService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(AppDbContext db, ILogger<OrdersController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: api: orders
    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAll()
        => await _db.Orders.AsNoTracking().ToListAsync();


    // GET: api/orders/{id} - Получение заказов по Id, Добавить пагинацию
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Order>> GetById(Guid id)
    {
        var orders_by_id = await _db.Orders
            .AsNoTracking()
            .Where(x => x.UserId == id)
            .OrderBy(x => x.DateOrder)
            .Select(x => new Order
            {
                Id = x.Id,
                UserId = x.UserId,
                DateOrder = x.DateOrder,
                Status = x.Status,
                Amount = x.Amount
        }
        ).ToListAsync();

        return Ok(orders_by_id);

    }


    // POST: api/orders
    [HttpPost]
    public async Task<ActionResult<Order>> Create(CreateOrderRequest request)
    {

        _logger.LogInformation("Создан новый заказ User_Id - {UserId}, Сумма - {Amount}", request.UserId, request.Amount);

        _db.Orders.Add(new Order
    {
        UserId = request.UserId,
        DateOrder = DateTime.UtcNow,
        Status = request.Status,
        Amount = request.Amount
    });


        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = request.UserId }, request);
    }



    // PUT: api/orders/{id} - обновление по id заказа, добавить отображение информации о том, что статус может быть от 1 до 6
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, UpdateOrderRequest request)
    {
        _logger.LogInformation("Updating order {Order_id}", id);

        var order = await _db.Orders.FirstOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound();


        if (request.Amount is not null)
            order.Amount = request.Amount.Value;

        if (request.Status is not null && Enum.IsDefined(typeof(StatusOrder), request.Status.Value))
            order.Status = request.Status.Value;

        await _db.SaveChangesAsync();
        return NoContent();


    }



    // DELETE: api/orders/{id} - Удаление заказа по ID заказа
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        _logger.LogInformation("Deleting order {order_id}", id);

        var order = await _db.Orders.FirstOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound();

        _db.Orders.Remove(order);

        await _db.SaveChangesAsync();
        return NoContent();
    }

}