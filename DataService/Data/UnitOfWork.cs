using DataService.IConfiguration;
using DataService.IRepository;
using DataService.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Data
{
    public class UnitOfWork : IUnitOfWork,IDisposable
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger _logger; 
        public IUserRepository Users { get; private set; }

        public UnitOfWork(AppDbContext appDbContext, ILoggerFactory logger)
        {
            _appDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
            _logger = logger.CreateLogger("db_logs") ?? throw new ArgumentNullException(nameof(logger));
            Users = new UserRepository(_appDbContext,_logger);
        }

        public async Task CompletedAsync()
        {
            await _appDbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _appDbContext.Dispose();
        }
    }
}
