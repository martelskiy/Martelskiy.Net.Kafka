using System;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace Martelskiy.Net.Kafka.Serialization.Avro.Serializer.SubjectNameMapper
{
    public class SerializerSubjectNameStrategyMapper : ISerializerSubjectNameStrategyMapper
    {
        public SubjectNameStrategy Map(string strategy)
        {
            switch (strategy)
            {
                case "Topic":
                    return SubjectNameStrategy.Topic;
                case "Record":
                    return SubjectNameStrategy.Record;
                case "TopicRecord":
                    return SubjectNameStrategy.TopicRecord;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(AvroSerializerConfig.SubjectNameStrategy),
                        $"Could not parse value {strategy} to nether Topic, Record nor TopicRecord");
            }
        }
    }
}