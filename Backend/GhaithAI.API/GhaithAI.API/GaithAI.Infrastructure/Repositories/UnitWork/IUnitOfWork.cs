using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.API.Models;
using GhaithAI.API.Repositories.Interfaces;

namespace GhaithAI.API.Repositories.UnitWork
{
    public interface IUnitOfWork
    {
        IChatRepository ChatMessages { get; }
        IRiskRepository RiskEvents { get; }
        Task<int> CompleteAsync();

    }
}
