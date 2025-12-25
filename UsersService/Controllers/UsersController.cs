using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsersService.Data;
using UsersService.Models;
using Npgsql;

using UsersService.Dtos;
using System.Text.Json;




namespace UsersService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db) => _db = db;

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<List<User>>> GetAll()
        => await _db.Users.AsNoTracking().ToListAsync();

    // GET: api/users/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<User>> GetById(Guid id)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return user is null ? NotFound() : Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<User>> Create(User user)
    {
        user.Id = Guid.NewGuid();
        user.CreatedAtUtc = DateTime.UtcNow;
        user.UpdatedAtUtc = DateTime.UtcNow;

        _db.Users.Add(user);

        try
        {
            // 1️. Сохраняем пользователя
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            return Conflict(new
            {
                message = "Пользователь с таким email уже существует.",
                field = "email",
                value = user.Email
            });
        }

        // 2️⃣ Пишем историю ТОЛЬКО если пользователь реально создан
        var afterJson = JsonSerializer.Serialize(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName
        });

        _db.UserChanges.Add(new UserChange
        {
            UserId = user.Id,
            ChangeType = UserChangeType.Created,
            ChangedAtUtc = DateTime.UtcNow,
            ChangedBy = "system",
            AfterJson = afterJson
        });

        // 3️⃣ Сохраняем историю
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }


    // PUT: api/users/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null) return NotFound();

        // Снимок "до"
        var beforeJson = JsonSerializer.Serialize(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName
        });

        // Применяем изменения (только если пришли)
        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email;

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName;

        user.UpdatedAtUtc = DateTime.UtcNow;

        // Снимок "после" (ещё до сохранения — это будущие значения)
        var afterJson = JsonSerializer.Serialize(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName
        });

        // Добавляем запись истории
        _db.UserChanges.Add(new UserChange
        {
            UserId = user.Id,
            ChangeType = UserChangeType.Updated,
            ChangedAtUtc = DateTime.UtcNow,
            ChangedBy = "system",
            BeforeJson = beforeJson,
            AfterJson = afterJson
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            // Дубль Email (уникальный индекс)
            return Conflict(new
            {
                message = "Пользователь с таким email уже существует.",
                field = "email",
                value = request.Email
            });
        }

        return NoContent();
    }

// DELETE: api/users/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null) return NotFound();

        // Снимок "до" (что удаляем)
        var beforeJson = JsonSerializer.Serialize(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName
        });

        // История удаления
        _db.UserChanges.Add(new UserChange
        {
            UserId = user.Id,
            ChangeType = UserChangeType.Deleted,
            ChangedAtUtc = DateTime.UtcNow,
            ChangedBy = "system",
            BeforeJson = beforeJson
        });

        _db.Users.Remove(user);

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
