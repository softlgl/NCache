﻿using System;
namespace NCache.Repository
{
    public interface ICacheRepository
    {
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string get(string key);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="expireSeconds"></param>
        void set(string key,string data,int expireSeconds);

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key"></param>
        void delete(string key);
    }
}