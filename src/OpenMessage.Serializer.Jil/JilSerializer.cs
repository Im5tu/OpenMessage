using Jil;
using System;
using System.IO;
using System.Text;
using JilOptions = Jil.Options;

namespace OpenMessage.Serializer.Jil
{
    public class JilSerializer : ISerializer
    {
        private readonly JilOptions _settings;

        public string TypeName => "application/json";

        public JilSerializer(JilOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _settings = options;
        }

        public T Deserialize<T>(Stream entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var streamReader = new StreamReader(entity))
                return JSON.Deserialize<T>(streamReader.ReadToEnd(), _settings);
        }

        public Stream Serialize<T>(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return new MemoryStream(Encoding.UTF8.GetBytes(JSON.Serialize(entity, _settings)));
        }
    }
}
