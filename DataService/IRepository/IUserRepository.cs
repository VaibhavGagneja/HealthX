using Entities.DbSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataService.IRepository
{
    public interface IUserRepository : IGenericRepository<User>
    {
       
    }
}