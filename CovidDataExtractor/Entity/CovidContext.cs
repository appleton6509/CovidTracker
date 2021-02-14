using Microsoft.EntityFrameworkCore;


namespace CovidDataExtractor.Entity
{
    public class CovidContext : DbContext
    {
        public CovidContext( DbContextOptions options) : base(options) { }

        public DbSet<Data> Data { get; set; }

    }
}
