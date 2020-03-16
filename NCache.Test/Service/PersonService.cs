using System;
using System.Threading.Tasks;
using NCache.Test.Model;

namespace NCache.Test.Service
{
    public class PersonService : IPersonService
    {
        public PersonService()
        {
        }

        [Cache(KeyPrefix ="Person",Expiration=600)]
        public Person GetPerson(int id)
        {
            Person person = new Person
            {
                Id=id,
                Name="liguoliang",
                Birthday=new DateTime(1992,12,11)
            };
            return person;
        }

        [Cache(KeyPrefix = "PersonAdd", Expiration  = 3600)]
        public void AddPerson(Person person)
        {

        }

        [Cache(KeyPrefix = "PersonUpdate", Expiration  = 3600)]
        public async Task<Person> UpdatePerson(int id,Person person)
        {
            await Task.Delay(10);
            return person;
        }
    }
}
