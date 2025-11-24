using System;
using web_quanao.Models;

namespace web_quanao.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        public ClothingStoreDBEntities Context { get; }
        public UnitOfWork(ClothingStoreDBEntities context)
        {
            Context = context;
        }
        public int Complete() => Context.SaveChanges();
        public void Dispose() => Context.Dispose();
    }
}
