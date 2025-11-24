using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace web_quanao.Infrastructure.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity Get(object id);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        TEntity Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);

        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);

        IQueryable<TEntity> Query();
    }
}
