// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.ArchitectureTests;

using System.Linq;
using System.Reflection;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using AwesomeAssertions;
using NetArchTest.Rules;
using Xunit;

[Trait("Category", "Architecture")]
public sealed class ArchitectureRulesTests
{
    private static readonly Assembly EventsAssembly = typeof(IEvent).Assembly;
    private static readonly Assembly SerializationAssembly = typeof(EricksonLopez.Events.Serialization.SystemTextJson.EventsJsonSerializerOptionsExtensions).Assembly;
    private static readonly Assembly OpenTelemetryAssembly = typeof(EricksonLopez.Events.OpenTelemetry.EventsOpenTelemetryExtensions).Assembly;
    private static readonly Assembly GeneratorsAssembly = typeof(EricksonLopez.Events.Generators.EventIncrementalGenerator).Assembly;
    private static readonly Assembly CloudEventsAssembly = typeof(EricksonLopez.Events.CloudEvents.CloudEvent<>).Assembly;
    private static readonly Assembly TestingAssembly = typeof(EricksonLopez.Events.Testing.FakeEventPublisher).Assembly;

    [Fact]
    public void EventsCore_WithForbiddenDependencies_ShouldNotDependOnInfrastructureOrBrokers()
    {
        var forbiddenDependencies = new[]
        {
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "Dapper",
            "MassTransit",
            "RabbitMQ.Client",
            "Confluent.Kafka",
            "Azure.Messaging.ServiceBus",
            "Amazon.SQS",
            "System.Text.Json",
            "Newtonsoft.Json"
        };

        var result = Types.InAssembly(EventsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDependencies)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Core package must not have any dependencies on infrastructure, brokers, or serializes.");
    }

    [Fact]
    public void Serialization_WithForbiddenDependencies_ShouldNotDependOnInfrastructureOrBrokers()
    {
        var forbiddenDependencies = new[]
        {
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "Dapper",
            "MassTransit",
            "RabbitMQ.Client",
            "Confluent.Kafka",
            "Azure.Messaging.ServiceBus",
            "Amazon.SQS",
            "Newtonsoft.Json"
        };

        var result = Types.InAssembly(SerializationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDependencies)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Serialization package must not have any dependencies on infrastructure or brokers.");
    }

    [Fact]
    public void OpenTelemetry_WithForbiddenDependencies_ShouldNotDependOnInfrastructureOrBrokers()
    {
        var forbiddenDependencies = new[]
        {
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "Dapper",
            "MassTransit",
            "RabbitMQ.Client",
            "Confluent.Kafka",
            "Azure.Messaging.ServiceBus",
            "Amazon.SQS",
            "System.Text.Json",
            "Newtonsoft.Json"
        };

        var result = Types.InAssembly(OpenTelemetryAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDependencies)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("OpenTelemetry package must not have any dependencies on infrastructure, brokers, or JSON serializers.");
    }

    [Fact]
    public void Generators_WithForbiddenDependencies_ShouldNotDependOnInfrastructureOrBrokers()
    {
        var forbiddenDependencies = new[]
        {
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "Dapper",
            "MassTransit",
            "RabbitMQ.Client",
            "Confluent.Kafka",
            "Azure.Messaging.ServiceBus",
            "Amazon.SQS",
            "System.Text.Json",
            "Newtonsoft.Json"
        };

        var result = Types.InAssembly(GeneratorsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDependencies)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Generators package must not have any dependencies on infrastructure, brokers, or runtime serializers.");
    }


    [Fact]
    public void CloudEvents_WithForbiddenDependencies_ShouldNotDependOnInfrastructureOrBrokers()
    {
        var forbiddenDependencies = new[]
        {
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "Dapper",
            "MassTransit",
            "RabbitMQ.Client",
            "Confluent.Kafka",
            "Azure.Messaging.ServiceBus",
            "Amazon.SQS",
            "Newtonsoft.Json"
        };

        var result = Types.InAssembly(CloudEventsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDependencies)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("CloudEvents package must not have direct dependencies on message broker SDKs or ORMs.");
    }

    [Fact]
    public void Testing_WithForbiddenDependencies_ShouldNotDependOnInfrastructureOrBrokers()
    {
        var forbiddenDependencies = new[]
        {
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "Dapper",
            "MassTransit",
            "RabbitMQ.Client",
            "Confluent.Kafka",
            "Azure.Messaging.ServiceBus",
            "Amazon.SQS",
            "Newtonsoft.Json"
        };

        var result = Types.InAssembly(TestingAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDependencies)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Testing utilities package must not have direct dependencies on message broker SDKs or ORMs.");
    }

    [Fact]
    public void Identifiers_WhenInNamespace_ShouldBeValueTypesAndImmutable()
    {
        var identifierTypes = EventsAssembly.GetExportedTypes()
            .Where(t => t.Namespace == typeof(EventId).Namespace)
            .ToList();

        identifierTypes.Should().NotBeEmpty("Identifier types must exist in the domain.");

        foreach (var type in identifierTypes)
        {
            type.IsValueType.Should().BeTrue($"{type.FullName} must be a value type (readonly struct/record struct) for zero heap allocation.");

            // Verify all instance fields are readonly / init-only
            var instanceFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in instanceFields)
            {
                field.IsInitOnly.Should().BeTrue($"Field {field.Name} in {type.Name} must be readonly/init-only to guarantee immutability.");
            }
        }
    }

    [Fact]
    public void EventContracts_WhenDefined_ShouldInheritFromIEvent()
    {
        typeof(IDomainEvent).Should().Implement<IEvent>();
        typeof(IIntegrationEvent).Should().Implement<IEvent>();
    }

    [Fact]
    public void CoreTypes_WhenInAssembly_ShouldResideInEricksonLopezEventsNamespace()
    {
        var result = Types.InAssembly(EventsAssembly)
            .That()
            .ArePublic()
            .Should()
            .ResideInNamespaceMatching(@"^EricksonLopez\.Events.*")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void SerializationTypes_WhenInAssembly_ShouldResideInEricksonLopezEventsSerializationNamespace()
    {
        var result = Types.InAssembly(SerializationAssembly)
            .That()
            .ArePublic()
            .Should()
            .ResideInNamespaceMatching(@"^EricksonLopez\.Events\.Serialization.*")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void OpenTelemetryTypes_WhenInAssembly_ShouldResideInEricksonLopezEventsOpenTelemetryNamespace()
    {
        var result = Types.InAssembly(OpenTelemetryAssembly)
            .That()
            .ArePublic()
            .Should()
            .ResideInNamespaceMatching(@"^EricksonLopez\.Events\.OpenTelemetry.*")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void GeneratorsTypes_WhenInAssembly_ShouldResideInEricksonLopezEventsGeneratorsNamespace()
    {
        var result = Types.InAssembly(GeneratorsAssembly)
            .That()
            .ArePublic()
            .Should()
            .ResideInNamespaceMatching(@"^EricksonLopez\.Events\.Generators.*")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }


    [Fact]
    public void CloudEventsTypes_WhenInAssembly_ShouldResideInEricksonLopezEventsCloudEventsNamespace()
    {
        var result = Types.InAssembly(CloudEventsAssembly)
            .That()
            .ArePublic()
            .Should()
            .ResideInNamespaceMatching(@"^EricksonLopez\.Events\.CloudEvents.*")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void TestingTypes_WhenInAssembly_ShouldResideInEricksonLopezEventsTestingNamespace()
    {
        var result = Types.InAssembly(TestingAssembly)
            .That()
            .ArePublic()
            .Should()
            .ResideInNamespaceMatching(@"^EricksonLopez\.Events\.Testing.*")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}





