using TechA.Core.Entities;

namespace TechA.Core.Interfaces.Persistence;

public interface ILlmResponseRepository
{
    Task AddAsync(LlmResponse response);
}
