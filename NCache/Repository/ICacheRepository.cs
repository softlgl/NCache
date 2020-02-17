using System;
namespace NCache.Repository
{
    public interface ICacheRepository
    {
        string get(string key);
        void set(string key,string data,int expireSeconds);
        void delete(string key);
    }
}
