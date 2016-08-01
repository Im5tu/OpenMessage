using System.IO;

namespace OpenMessage
{
    public interface ISerializer
    {
        string TypeName { get; }

        Stream Serialize<T>(T entity);
        T Deserialize<T>(Stream entity);
    }
}
