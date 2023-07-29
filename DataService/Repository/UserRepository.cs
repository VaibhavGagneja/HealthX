using DataService.Data;
using DataService.IRepository;
using Entities.DbSet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context , ILogger logger) : base(context,logger)
        {

        }
        public override async Task<IEnumerable<User>> All()
        {
            try
            {
                return await _dbSet.Where(x=>x.Status==1)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex,"{Repo} All Method Has Generated An Error" ,typeof(UserRepository));
                return new List<User>();
            }
        }

        
    }
}
