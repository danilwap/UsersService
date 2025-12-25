using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;


using UsersService.Data;
using UsersService.Models;
using UsersService.Dtos;



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
    public async Task<ActionResult<User>> Create(CreateUserRequest request)
    {
        // быстрая проверка уникальности (для понятного ответа)
        var exists = await _db.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
        {
            return Conflict(new
            {
                message = "Пользователь с таким email уже существует.",
                field = "email",
                value = request.Email
            });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.Users.Add(user);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            // защита от race condition
            return Conflict(new
            {
                message = "Пользователь с таким email уже существует.",
                field = "email",
                value = request.Email
            });
        }

        // история создания
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

    // GET: api/users/{id}/history?take=50&skip=0
    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<List<UserChangeDto>>> GetHistory(
        Guid id,
        [FromQuery] int take = 50,
        [FromQuery] int skip = 0)
    {
        // защита от странных значений
        if (take <= 0) take = 50;
        if (take > 200) take = 200;
        if (skip < 0) skip = 0;

        var history = await _db.UserChanges
            .AsNoTracking()
            .Where(x => x.UserId == id)
            .OrderByDescending(x => x.ChangedAtUtc)
            .Skip(skip)
            .Take(take)
            .Select(x => new UserChangeDto
            {
                Id = x.Id,
                UserId = x.UserId,
                ChangedAtUtc = x.ChangedAtUtc,
                ChangeType = x.ChangeType.ToString(),
                ChangedBy = x.ChangedBy,
                BeforeJson = x.BeforeJson,
                AfterJson = x.AfterJson
            })
            .ToListAsync();

        return Ok(history);
    }



}
