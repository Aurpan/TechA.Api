using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechA.Core.Entities;
using TechA.Core.Interfaces.Domain;

namespace TechA.Api.Controllers;

[ApiController]
[Route("api/v1/expenses")]
[Authorize]
public class ExpenseTransactionController : ControllerBase
{
    private readonly IExpenseTransactionService _expenseTransactionService;

    public ExpenseTransactionController(IExpenseTransactionService expenseTransactionService)
    {
        _expenseTransactionService = expenseTransactionService;
    }

    [HttpGet("GetAllExpenses")]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized(new { Message = "Invalid token." });

        var transactions = await _expenseTransactionService.GetAllByUserIdAsync(userId);
        return Ok(transactions);
    }

    [HttpGet("{id:guid}", Name = "GetExpenseById")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized(new { Message = "Invalid token." });

        var transaction = await _expenseTransactionService.GetByIdAsync(id, userId);

        if (transaction is null)
            return NotFound(new { Message = "Transaction not found." });

        return Ok(transaction);
    }

    [HttpPost("CreateExpense")]
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized(new { Message = "Invalid token." });

        var transaction = await _expenseTransactionService.CreateAsync(
            userId,
            request.ProductName,
            request.Category,
            request.Amount,
            request.Type,
            request.DateTime);

        return CreatedAtRoute("GetExpenseById", new { id = transaction.Id }, transaction);
    }

    [HttpPut("{id:guid}", Name = "UpdateExpense")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExpenseRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized(new { Message = "Invalid token." });

        var transaction = await _expenseTransactionService.UpdateAsync(
            id,
            userId,
            request.ProductName,
            request.Category,
            request.Amount,
            request.Type,
            request.DateTime);

        if (transaction is null)
            return NotFound(new { Message = "Transaction not found." });

        return Ok(transaction);
    }

    [HttpDelete("{id:guid}", Name = "DeleteExpense")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
            return Unauthorized(new { Message = "Invalid token." });

        var deleted = await _expenseTransactionService.DeleteAsync(id, userId);

        if (!deleted)
            return NotFound(new { Message = "Transaction not found." });

        return NoContent();
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            return Guid.Empty;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

public record CreateExpenseRequest
{
    public required string ProductName { get; init; }
    public required string Category { get; init; }
    public required decimal Amount { get; init; }
    public required ExpenseType Type { get; init; }
    public DateTime? DateTime { get; init; }
}

public record UpdateExpenseRequest
{
    public required string ProductName { get; init; }
    public required string Category { get; init; }
    public required decimal Amount { get; init; }
    public required ExpenseType Type { get; init; }
    public DateTime? DateTime { get; init; }
}
