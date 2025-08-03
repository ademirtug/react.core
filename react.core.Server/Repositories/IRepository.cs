using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace duoword.admin.Server.Repositories
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> Where(Expression<Func<T, bool>> predicate);
        IQueryable<TResult> Select<TResult>(Expression<Func<T, TResult>> selector);
        public IQueryable<T> Include(params Expression<Func<T, object>>[] includes);
        T? Get(object id);
        IQueryable<T> GetAll();
        void Insert(T entity);
        void Update(T entity);
        void Delete(object id);
        void SaveChanges();
    }
}
