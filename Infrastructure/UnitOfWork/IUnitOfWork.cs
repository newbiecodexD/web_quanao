using System;
using web_quanao.Models;

namespace web_quanao.Infrastructure.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        ClothingStoreDBEntities Context { get; }
        int Complete();
    }
}
