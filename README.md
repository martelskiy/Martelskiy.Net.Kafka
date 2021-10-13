Package is a wrapper to Confluent NuGet [package](https://github.com/confluentinc/confluent-kafka-dotnet/). The purpose of package is to be able to easily add Kafka producer/consumer.

## Serialization

Default location in appsettings: `Kafka:AvroSerializerConfiguration`. Currently we use Avro serializer/deserializer. For *consumers*(deserialization) there are no extra configuration you can set. However for *producers*(serializartion) you can set
following settings:

`ValueSubjectNameStrategy`(optional. Default value is `Topic`),
`KeySubjectNameStrategy`(optional. Default value is `Topic`). 

Configuration example:

```
"Kafka":{
  "AvroSerializerConfiguration": {
      "ValueSubjectNameStrategy": "TopicRecord",
      "KeySubjectNameStrategy" :  "TopicRecord"
  } 
}

```        
## Schema registry

Default location in appsettings: `Kafka:SchemaRegistryConfiguration`
There are three possible configurations supported by schema registry client: 
`Url`(required)

Configuration example:
```
"Kafka":{
  "SchemaRegistryConfiguration": {
        "Url": "localhost:8081"
  }
}

```

## Kafka Consumer

Default location in appsettings: `Kafka:ConsumerConfiguration`
Kafka consumers implemented as .NET Core `BackgroundService`. Package supports having multiple Kafka consumers in one application.

So possible scenarios:

1. **I have an application and I want to have *one* Kafka consumer.**
Implementation:

 a) It needs to inherit from generic `KafkaDaemonBase<TKey,TValue>`. `TKey` is the key type you expect, `TValue`(it can be set to `Null` or `Ignore`) is the value type of expect to receive from Kafka(`ISpecificRecord`, `GenericRecord`, `string` etc):


```
    public class KafkaConsumerDaemon1 : KafkaDaemonBase<Ignore, string>
    {
        public KafkaConsumerDaemon1(IKafkaConsumer<Ignore, string> kafkaConsumer, ILogger<KafkaDaemonBase<Ignore, string>> logger, IHostApplicationLifetime host) 
            : base(kafkaConsumer, logger, host)
        {
        }
    }
```

 b) Then need to inherit from `KafkaConsumerBase<TKey, TValue>`. **NOTE**: `TKey` and `TValue` types should match types in your `KafkaDaemonBase<TKey,TValue>`:

```
    public class KafkaConsumer1 : KafkaConsumerBase<Ignore, string>
    {
        public KafkaConsumer1(
            ILogger<KafkaConsumer1> logger,
            IConsumer<Ignore, string> consumer,
            TopicConfiguration topicConfiguration,
            CultureInfo culture)
            : base(logger, consumer, topicConfiguration, culture)
        {
            
        }

        protected override Task Handle(ConsumeResult<Ignore, string> record, CancellationToken cancellationToken)
        {
            //Record is the kafka message you receive
        }
    }
```
 c) Add this to `Startup.cs`(or where dependencies are being registered)(services is type of `IServiceCollection`):

```
services.ConfigureBackgroundConsumer<KafkaConsumer1, KafkaConsumerDaemon1>(hostContext.Configuration); //multiple overloads available
```

**NOTE**: You can inject whatever you need into `KafkaConsumer1` just register it in DI. Also, don't forget that consumer is a *singleton*, so retrieve all your transient service through usage of `IServiceScopeFactory`

 d) By default(if you don't use method overload when register daemon and consumer) all settings we be read from `Kafka` section in your `appsettings.json`. So this is sample configuratin:
```
 "Kafka": {
      "enabled": true, //default is true if not set
      "topic": "your_topic_name",

      "ConsumerConfiguration": {
        "security.protocol": "plaintext",
        "group.id": "consumer-group-id",
        "bootstrap.servers": "kafka:9092"
      },

      "SchemaRegistryConfiguration": {
        "Url": "localhost:8081"
      }
    },
```

Also you can set reading from *specifc* Kafka offset:

```
 "Kafka": {
      "enabled": true,
      "topic": "your_topic_name",
      "partitionoffset": [
        {
          "partition": 0,
          "offset": 0
        }
      ],
      "ConsumerConfiguration": {
        "security.protocol": "plaintext",
        "group.id": "consumer-group-id",
        "bootstrap.servers": "kafka:9092"
      },

      "SchemaRegistryConfiguration": {
        "Url": "localhost:8081"
      }
    },
```

2. **I want to consume from *multiple* topics within my application. From all topics I expect message of with the same type of `Key` and `Value`.**
Implementation:
 a) You can reuse same Consumer class but you **cannot** reuse the same Daemon. 
 So you will need to add your consumer and daemons same as decribed above. The difference will be in configuration and registration is `Startup.cs`. 

```
 services.ConfigureBackgroundConsumer<SharedKafkaConsumer, KafkaConsumerDaemon1>(hostContext.Configuration, "Kafka:Consumer1");
 services.ConfigureBackgroundConsumer<SharedKafkaConsumer, KafkaConsumerDaemon2>(hostContext.Configuration, "Kafka:Consumer2");
```

 b)And you `appsettings.json` would look like this:

```
"Kafka": {
    "Consumer1": {
      "topic": "consumer-topic-1"
    },

    "Consumer2": {
      "topic": "consumer-topic-2"
    },

    "ConsumerConfiguration": {
        "security.protocol": "plaintext",
        "group.id": "group",
        "bootstrap.servers": "kafka:9092",
    },

    "SchemaRegistryConfiguration": {
        "Url": "localhost:8081"
    }
```

3. **I want to consume from multiple topics and all messages would be have different types.**

a) It needs to add separate Daemons and Consumers per topic. So both `KafkaConsumer1` and `KafkaConsumer2` would have to inherit from `KafkaConsumerBase<,>`.
This is how registrations would look like:

```
 services.ConfigureBackgroundConsumer<KafkaConsumer1, KafkaConsumerDaemon1>(hostContext.Configuration, "Kafka:Consumer1");
 services.ConfigureBackgroundConsumer<KafkaConsumer2, KafkaConsumerDaemon2>(hostContext.Configuration, "Kafka:Consumer2");
```

## Kafka Producer

Default location in appsettings: `Kafka:ProducerConfiguration`
Possible scenarios:

1. **I want to have an instance within my application that can produce to the Kafka topic. I want to be able to inject it with DI.**

 a) It needs to inherit from `KafkaProducerBase<,>` class:

```
 public class KafkaProducer : KafkaProducerBase<Null, byte[]>
    {
        public KafkaProducer(
            IProducer<Null, byte[]> kafkaProducer, 
            TopicConfiguration topicConfiguration,
            ILogger<KafkaProducerBase<Null, byte[]>> logger) 
            : base(kafkaProducer, topicConfiguration, logger)
        {
        }
    }
```
 b) Add this to `Startup.cs`(or where dependencies are being registered)(`services` is type of `IServiceCollection`):

```
 services.ConfigureKafkaProducer<KafkaProducer, Null, byte[]>(hostContext.Configuration);
```

 c) Add producer configuration to your `appsettings.json`:

```
 "Kafka": {
      "topic": "producer-topic",

      "ProducerConfiguration": {
        "bootstrap.servers": "kafka:9092"
      },

      "SchemaRegistryConfiguration": {
        "Url": "localhost:8081"
      }
    },
```

 d) Inject your Producer:

```
 var producer = services.GetRequiredService<IKafkaProducer<Null, byte[]>>();

 await producer.Produce(new byte[]{});
 ```

2. **I want to have *two* different kafka producers within my application and being able to inject them with DI.**

 a) Add two producers:

 ```
    public class KafkaProducer1 : KafkaProducerBase<Null, byte[]>
    {
        public KafkaProducer1(
            IProducer<Null, byte[]> kafkaProducer, 
            TopicConfiguration topicConfiguration,
            ILogger<KafkaProducerBase<Null, byte[]>> logger) 
            : base(kafkaProducer, topicConfiguration, logger)
        {
        }
    }

    public class KafkaProducer2 : KafkaProducerBase<string, string>
    {
        public KafkaProducer2(
            IProducer<string, string> kafkaProducer, 
            TopicConfiguration topicConfiguration,
            ILogger<KafkaProducerBase<string, string>> logger) 
            : base(kafkaProducer, topicConfiguration, logger)
        {
        }
    }
```
 b) Configure those in your `Startup.cs` and set the path to configuration for specific producer(otherwise it will read settings from default "Kafka"):

```
services.ConfigureKafkaProducer<KafkaProducer1, Null, byte[]>(hostContext.Configuration, "Kafka:Producer1");
services.ConfigureKafkaProducer<KafkaProducer2, string, string>(hostContext.Configuration, "Kafka:Producer2");
```

 c) `appsettings.json` would look like this:

```
 "Kafka": {
    "Producer1": {
      "topic": "producer-topic-1"
    },
    "Producer2": {
      "topic": "producer-topic-2"
    },

    "ProducerConfiguration": {
        "bootstrap.servers": "kafka:9092"
      },

    "SchemaRegistryConfiguration": {
        "Url": "localhost:8081"
    }
 }
 ```

 d) Usage:

```
 var producer1 = services.GetRequiredService<IKafkaProducer<Null, byte[]>>();
 var producer2 = services.GetRequiredService<IKafkaProducer<string, string>>();
```