using Confluent.SchemaRegistry;

namespace Martelskiy.Net.Kafka.Serialization.Avro.Serializer.SubjectNameMapper
{
    public interface ISerializerSubjectNameStrategyMapper
    {
        SubjectNameStrategy Map(string strategy);
    }
}
