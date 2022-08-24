using System.IO;
using System.Threading.Tasks;

namespace Pyro.Net
{
    public interface ISizeableContent
    {
        public string JsonData { get; set; }

        public object Convert();
        public T Convert<T>();
    }
}