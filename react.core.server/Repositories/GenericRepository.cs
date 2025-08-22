using Microsoft.EntityFrameworkCore;
using react.core.Server.Data;
using System.Linq.Expressions;

namespace react.core.Server.Repositories
{
	public class GenericRepository<T> : IRepository<T> where T : class
	{
		private readonly AppDbContext _context;
		private readonly DbSet<T> _dbSet;

		public GenericRepository(AppDbContext context)
		{
			_context = context;
			_dbSet = context.Set<T>();
		}

		public IQueryable<T> Where(Expression<Func<T, bool>> predicate)
		{
			return _dbSet.Where(predicate);
		}

		public IQueryable<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
		{
			return _dbSet.Select(selector);
		}

		public IQueryable<T> Include(params Expression<Func<T, object>>[] includes)
		{
			IQueryable<T> query = _dbSet;
			foreach (var include in includes)
			{
				query = query.Include(include);
			}
			return query;
		}

		public T? Get(object id)
		{
			return _dbSet.Find(id);
		}

		public IQueryable<T> GetAll()
		{
			return _dbSet.AsQueryable();
		}

		public void Add(T entity)
		{
			_dbSet.Add(entity);
		}
		public void AddRange(IEnumerable<T> entities)
		{
			_dbSet.AddRange(entities);
		}
		public void Update(T entity)
		{
			_dbSet.Update(entity);
		}

		public void Delete(object id)
		{
			var entity = _dbSet.Find(id);
			if (entity != null)
			{
				_dbSet.Remove(entity);
			}
		}

		public void SaveChanges()
		{
			_context.SaveChanges();
		}
	}
}
