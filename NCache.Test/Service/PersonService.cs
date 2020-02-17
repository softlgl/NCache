using System;
using NCache.Test.Model;

namespace NCache.Test.Service
{
    public class PersonService : IPersonService
    {
        public PersonService()
        {
        }

        [Cache(KeyPrefix ="Person",ExpirationSeconds =3600)]
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

        public void AddPerson(Person person)
        {

        }
    }
}
