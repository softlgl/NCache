using System;
using NCache.Test.Model;

namespace NCache.Test.Service
{
    public interface IPersonService
    {
        Person GetPerson(int id);

        void AddPerson(Person person);
    }
}
