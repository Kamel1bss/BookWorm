using BookWorm.DataAccess.Data;
using BookWorm.DataAccess.IRepository;
using BookWorm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWorm.DataAccess.Repository;

public class CompanyRepository(ApplicationDbContext db) : Repository<Company>(db), ICompanyRepository
{
    private readonly ApplicationDbContext _db = db;
    public void Update(Company entity)
    {
        _db.Companies.Update(entity);
    }
}
