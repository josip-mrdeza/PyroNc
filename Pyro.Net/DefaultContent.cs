using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pyro.Net
{
    public class DefaultContent : ISizeableContent
    {
        public string SenderId { get; set; }
        public string JsonData { get; set; }
        public string FullTypeName { get; set; }

        public object GetInstance()
        {
            return _instance ??= JsonSerializer.Deserialize(JsonData, Type.GetType(FullTypeName)!, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true    
            });
        }

        private object _instance;

        public virtual object Convert()
        {
            var instance = JsonSerializer.Deserialize(JsonData, Type.GetType(FullTypeName)!, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true    
            });

            return instance;
        }
        
        public virtual T Convert<T>()
        {
            var instance = JsonSerializer.Deserialize<T>(JsonData, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true    
            });

            return instance;
        }
    }

    public class DefaultContent<T> : DefaultContent
    {
        public DefaultContent(T obj)
        {
            JsonData = JsonSerializer.Serialize(obj, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true    
            });
            FullTypeName = typeof(T).AssemblyQualifiedName;
        }
    }
}