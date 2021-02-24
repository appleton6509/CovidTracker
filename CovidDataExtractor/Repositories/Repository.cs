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

        public async Task Add(Data data)
        {
            if (Exists(data.FromDate))
                return;

            using var context = factory.CreateDbContext();
            context.Data.Add(data);
            await context.SaveChangesAsync();
            return;
        }
        public bool Exists(DateTime fromDate)
        {
            using var context = factory.CreateDbContext();
            IQueryable<Data> found = context.Data.Where(x => x.FromDate.Equals(fromDate));
            return found.Count() > 0;
        }

    }
}
