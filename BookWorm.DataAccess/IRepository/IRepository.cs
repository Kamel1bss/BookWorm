using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookWorm.DataAccess.IRepository;

public interface IRepository<T> where T : class
{
    IEnumerable<T> GetAll();
    void Add(T entity);
    T Get(Expression<Func<T, bool>> filter);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
