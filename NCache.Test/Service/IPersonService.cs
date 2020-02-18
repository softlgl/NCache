using System;
using System.Threading.Tasks;
using NCache.Test.Model;

namespace NCache.Test.Service
{
    public interface IPersonService
    {
        Person GetPerson(int id);

        void AddPerson(Person person);

        Task<Person> UpdatePerson(int id,Person person);
    }
}
