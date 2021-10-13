using System;
using Confluent.SchemaRegistry;
using Martelskiy.Net.Kafka.Serialization.Avro.Serializer.SubjectNameMapper;
using Shouldly;
using Xunit;

namespace Martelskiy.Net.Kafka.Tests.SchemaRegistry
{
    public class SchemaRegistrySubjectNameStrategyMapperTests
    {
        private readonly ISerializerSubjectNameStrategyMapper _sut;

        public SchemaRegistrySubjectNameStrategyMapperTests()
        {
            _sut = new SerializerSubjectNameStrategyMapper();
        }

        [Fact]
        public void Should_MapToConfluentEnumTopic_When_PassTopicString()
        {
            var result = _sut.Map("Topic");

            result.ShouldBe(SubjectNameStrategy.Topic);
        }

        [Fact]
        public void Should_MapToConfluentEnumRecord_When_PassRecordString()
        {
            var result = _sut.Map("Record");

            result.ShouldBe(SubjectNameStrategy.Record);
        }

        [Fact]
        public void Should_MapToConfluentEnumTopicRecord_When_PassTopicRecordString()
        {
            var result = _sut.Map("TopicRecord");

            result.ShouldBe(SubjectNameStrategy.TopicRecord);
        }

        [Fact]
        public void Should_ThrowArgumentOurOfRangeException_When_InvalidStringPassed()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => _sut.Map("InvalidStrategy"));
        }
    }
}
