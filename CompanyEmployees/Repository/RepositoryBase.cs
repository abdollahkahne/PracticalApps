using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Contracts;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    // we do not used SaveChanges Method of DbContext Here so the change do not enforce/apply to database itself
    // we solve this in our RepositoryManager. 
    // Here we do the following steps
    // 1- Create IRepositoryBase<T> which is interface of generic Repository
    // 2- Create RepositoryBase<T> which is an abstract class of Generic Repository (Shared methods of all user repositories)
    // 3- Create ICompanyRepository and IEmployeeRepository which they are interfaces for specific methods
    // 4- Create CompanyRepository which inherit from abstract class RepositoryBase<Company> and ICompanyRepository to cover all methods (only implemented interface and of course its constructor )
    // we repeate step 4 for all entities (Here only Employee)
    // 5- Create Repository Manager class responsible for generating user repository instances and injecting them to service collection and importantly to apply save changes method to them
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected RepositoryContext _db;

        protected RepositoryBase(RepositoryContext db)
        {
            _db = db;
        }

        public void Create(T entity)
        {
            _db.Set<T>().Add(entity);
        }

        public void Delete(T entity)
        {
            _db.Set<T>().Remove(entity);
        }

        public IQueryable<T> FindAll(bool trackChanges)
        {
            if (trackChanges)
            {
                return _db.Set<T>();
            }
            else
            {
                return _db.Set<T>().AsNoTracking();
            }

        }

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges)
        {
            if (trackChanges)
            {
                return _db.Set<T>().Where(expression);
            }
            else
            {
                return _db.Set<T>().Where(expression).AsNoTracking();
            }
        }

        public void Update(T entity)
        {
            _db.Set<T>().Update(entity);
        }
    }
}