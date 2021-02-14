using CovidDataExtractor.Entity;
using System;
using System.Threading.Tasks;

namespace CovidDataExtractor.Repositories

{
    public interface IRepository
    {
        Task Add(Data data);
        bool Exists(DateTime fromdate);
    }
}