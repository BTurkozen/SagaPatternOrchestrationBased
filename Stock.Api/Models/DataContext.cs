using Microsoft.EntityFrameworkCore;

namespace Stock.Api.Models
{
    public class DataContext:DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<Stock> Stocks { get; set; }
    }
}
