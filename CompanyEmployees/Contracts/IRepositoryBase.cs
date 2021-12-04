using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Contracts
{
    // we create a generic repository once and derive all repositories from it
    public interface IRepositoryBase<T>
    {
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
        IQueryable<T> FindAll(bool trackChanges); // we need this trackChange Parameter for performance
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges);

    }
}