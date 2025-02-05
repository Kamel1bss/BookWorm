using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWorm.DataAccess.IRepository;

public interface IUnitOfWork
{
    ICategoryRepository _categoryRepo { get; }
    void Save();
}
