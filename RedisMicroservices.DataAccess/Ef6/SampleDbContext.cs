using System.Data.Entity;
using RedisMicroservices.Domain.Entity;

namespace RedisMicroservices.DataAccess.Ef6
{
    public class SampleDbContext:DbContext
    {
        public SampleDbContext() : base("SampleConnection")
        {
            
        }

        public DbSet<Sample> Samples { get; set; } 
    }
}