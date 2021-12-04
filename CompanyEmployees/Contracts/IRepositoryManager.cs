using System.Threading.Tasks;

namespace Contracts
{
    // we should define the interface here instead of contract to prevent loop between assembly
    // If we define type of Company as ICompanyRepository, we loose intellisense
    // If we do not give the generic CRUD Operations to developer we can use them too whih we selected here
    public interface IRepositoryManager
    {
        ICompanyRepository Company { get; }
        IEmployeeRepository Employee { get; }
        void Save();
        Task SaveAsync();
    }
}