using DataAccess.Model;
using System.Threading.Tasks;

namespace DataAccess.DataAccess
{
    public interface IDataAccessCollection
    {
        void Add(IDataAccess dataAccess);
        Task RemoveSale(Sale sale);
        Task WriteSale(Sale sale);
    }
}