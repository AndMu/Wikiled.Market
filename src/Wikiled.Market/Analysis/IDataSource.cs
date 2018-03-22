using System;
using System.Threading.Tasks;

namespace Wikiled.Market.Analysis
{
    public interface IDataSource
    {
        Task<DataPackage> GetData(string stock, DateTime? from, DateTime? to);
    }
}