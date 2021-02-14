using CovidDataExtractor.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CovidDataExtractor.Repositories
{
    public class Repository : IRepository
    {
        private readonly IDbContextFactory<CovidContext> factory;
        public Repository(IDbContextFactory<CovidContext> factory)
        {
            this.factory = factory;
        }

        public Task Add(Data data)
        {
            using var context = factory.CreateDbContext();
            context.Data.Add(data);
            return context.SaveChangesAsync();
        }
        public bool Exists(DateTime fromdate)
        {
            using var context = factory.CreateDbContext();
            IQueryable<Data> found = context.Data.Where(x => x.FromDate.Equals(fromdate));
            return found.Count() > 0;
        }

    }
}
