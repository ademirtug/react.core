using System.Linq.Expressions;

namespace react.core.Server.Repositories
{
	public interface IRepository<T> where T : class
	{
		IQueryable<T> Where(Expression<Func<T, bool>> predicate);
		IQueryable<TResult> Select<TResult>(Expression<Func<T, TResult>> selector);
		public IQueryable<T> Include(params Expression<Func<T, object>>[] includes);
		T? Get(object id);
		IQueryable<T> GetAll();
		void Add(T entity);
		void AddRange(IEnumerable<T> entities);
		void Update(T entity);
		void Delete(object id);
		void SaveChanges();
	}
}
