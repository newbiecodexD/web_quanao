using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using web_quanao.Models;

namespace web_quanao.Infrastructure.Repositories
{
    public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly ClothingStoreDBEntities _db;
        private readonly DbSet<TEntity> _set;

        public EfRepository(ClothingStoreDBEntities db)
        {
            _db = db;
            _set = _db.Set<TEntity>();
        }

        public TEntity Get(object id) => _set.Find(id);
        public IEnumerable<TEntity> GetAll() => _set.ToList();
        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate) => _set.Where(predicate).ToList();
        public TEntity Add(TEntity entity) => _set.Add(entity);
        public void AddRange(IEnumerable<TEntity> entities) => _set.AddRange(entities);
        public void Remove(TEntity entity) => _set.Remove(entity);
        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            foreach (var e in entities) _set.Remove(e);
        }
        public IQueryable<TEntity> Query() => _set.AsQueryable();
    }
}
