using System;
using Reservasi.DAC.Cache;


namespace Reservasi.CM.Common
{
    public class RedisBiz
    {
        private string _url;
        private int _lifetime;
        private string _redisOn;

        public RedisBiz(string url, string redisOn)
        {
            _url = url;
            _lifetime = -1;
            _redisOn = redisOn;
        }
        public RedisBiz(string url, int lifetime, string redisOn)
        {
            _url = url;
            _lifetime = lifetime;
            _redisOn = redisOn;
        }

        public bool SetRedis(string keyString, string objString)
        {
            if (_redisOn != "true") return true;

            try
            {
                var objRedis = new RedisCache(_url);
                var key = keyString;
                var objectString = objString;
                if(_lifetime == -1)
                    return objRedis.SetData(key, objectString);
                else
                    return objRedis.SetData(_lifetime, key, objectString);
            }
            catch(Exception ex)
            {
                return false;
            }
            
        }

        public string GetRedis(string keyString)
        {
            try
            {
                if (_redisOn != "true") return null;

                var objRedis = new RedisCache(_url);
                var key = keyString;
                return objRedis.GetData(key);
            }
            catch(Exception ex)
            {
                return null;
            }
            
        }
    }
}
