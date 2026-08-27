using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OneBotSdk.Net.V10.Client;
using OneBotSdk.Net.V10.Transports;
using OneBotSdk.Net.V11.Client;
using OneBotSdk.Net.V11.Transports;
using OneBotSdk.Net.V12.Client;
using OneBotSdk.Net.V12.Transports;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class SynchronousActionApiTests
{
    public static IEnumerable<object[]> ClientCases()
    {
        yield return new object[] { typeof(OneBot10Client), 46 };
        yield return new object[] { typeof(OneBot11Client), 45 };
        yield return new object[] { typeof(OneBot12Client), 45 };
    }

    [Theory]
    [MemberData(nameof(ClientCases))]
    public void EveryAsyncActionOverload_HasAnEquivalentSynchronousOverload(
        Type clientType,
        int expectedOverloadCount)
    {
        var publicMethods = clientType.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var asynchronousMethods = publicMethods
            .Where(IsAsynchronousActionMethod)
            .ToArray();
        var synchronousMethods = publicMethods
            .Where(method => !method.IsSpecialName && !method.Name.EndsWith("Async", StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(expectedOverloadCount, asynchronousMethods.Length);
        Assert.Equal(expectedOverloadCount, synchronousMethods.Length);

        foreach (var asynchronousMethod in asynchronousMethods)
        {
            var expectedName = asynchronousMethod.Name.Substring(
                0,
                asynchronousMethod.Name.Length - "Async".Length);
            var synchronousMethod = Assert.Single(synchronousMethods, candidate =>
                candidate.Name == expectedName &&
                candidate.GetGenericArguments().Length == asynchronousMethod.GetGenericArguments().Length &&
                HaveEquivalentParameterTypes(asynchronousMethod, candidate));

            Assert.True(
                HaveEquivalentTypeShape(
                    asynchronousMethod.ReturnType.GetGenericArguments()[0],
                    synchronousMethod.ReturnType),
                $"{clientType.Name}.{synchronousMethod.Name} has an unexpected return type.");
            AssertEquivalentGenericParameters(asynchronousMethod, synchronousMethod);
            AssertEquivalentParameters(asynchronousMethod, synchronousMethod);
        }

        foreach (var synchronousMethod in synchronousMethods)
        {
            Assert.Single(asynchronousMethods, candidate =>
                candidate.Name == synchronousMethod.Name + "Async" &&
                candidate.GetGenericArguments().Length == synchronousMethod.GetGenericArguments().Length &&
                HaveEquivalentParameterTypes(candidate, synchronousMethod));
        }
    }

    [Fact]
    public void SynchronousActions_UseTheAsyncTransportPathAndReturnParsedResponses()
    {
        var oneBot10Transport = new OneBot10RecordingTransport();
        var oneBot11Transport = new OneBot11RecordingTransport();
        var oneBot12Transport = new OneBot12RecordingTransport();

        var oneBot10Response = new OneBot10Client(oneBot10Transport)
            .CallAction<int?>("sync_v10", ParseValue);
        var oneBot11Response = new OneBot11Client(oneBot11Transport)
            .CallAction<int?>("sync_v11", ParseValue);
        var oneBot12Response = new OneBot12Client(oneBot12Transport)
            .CallAction<int?>("sync_v12", ParseValue);

        Assert.Equal(10, oneBot10Response.Data);
        Assert.Equal(11, oneBot11Response.Data);
        Assert.Equal(12, oneBot12Response.Data);
        Assert.Equal("sync_v10", oneBot10Transport.Action);
        Assert.Equal("sync_v11", oneBot11Transport.Action);
        Assert.Equal("sync_v12", oneBot12Transport.Action);
        Assert.Equal(1, oneBot10Transport.CallCount);
        Assert.Equal(1, oneBot11Transport.CallCount);
        Assert.Equal(1, oneBot12Transport.CallCount);
    }

    [Fact]
    public void SynchronousAction_RethrowsTheOriginalAsyncExceptionWithoutAggregateException()
    {
        var expected = new InvalidOperationException("transport failure");
        var client = new OneBot11Client(new OneBot11FaultingTransport(expected));

        var actual = Assert.Throws<InvalidOperationException>(() => client.CallAction("failure"));

        Assert.Same(expected, actual);
    }

    private static int? ParseValue(JsonNode? data)
    {
        return data?["value"]?.GetValue<int>();
    }

    private static bool IsAsynchronousActionMethod(MethodInfo method)
    {
        return !method.IsSpecialName &&
               method.Name.EndsWith("Async", StringComparison.Ordinal) &&
               method.ReturnType.IsGenericType &&
               method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
    }

    private static bool HaveEquivalentParameterTypes(MethodInfo asynchronousMethod, MethodInfo synchronousMethod)
    {
        var asynchronousParameters = asynchronousMethod.GetParameters();
        var synchronousParameters = synchronousMethod.GetParameters();
        return asynchronousParameters.Length == synchronousParameters.Length &&
               asynchronousParameters
                   .Zip(synchronousParameters, (left, right) => HaveEquivalentTypeShape(left.ParameterType, right.ParameterType))
                   .All(value => value);
    }

    private static void AssertEquivalentParameters(MethodInfo asynchronousMethod, MethodInfo synchronousMethod)
    {
        var asynchronousParameters = asynchronousMethod.GetParameters();
        var synchronousParameters = synchronousMethod.GetParameters();

        for (var index = 0; index < asynchronousParameters.Length; index++)
        {
            var asynchronousParameter = asynchronousParameters[index];
            var synchronousParameter = synchronousParameters[index];
            Assert.Equal(asynchronousParameter.Name, synchronousParameter.Name);
            Assert.Equal(asynchronousParameter.IsOptional, synchronousParameter.IsOptional);
            Assert.Equal(asynchronousParameter.HasDefaultValue, synchronousParameter.HasDefaultValue);
            if (asynchronousParameter.HasDefaultValue)
            {
                Assert.Equal(asynchronousParameter.DefaultValue, synchronousParameter.DefaultValue);
            }
        }
    }

    private static void AssertEquivalentGenericParameters(MethodInfo asynchronousMethod, MethodInfo synchronousMethod)
    {
        var asynchronousArguments = asynchronousMethod.GetGenericArguments();
        var synchronousArguments = synchronousMethod.GetGenericArguments();

        for (var index = 0; index < asynchronousArguments.Length; index++)
        {
            var asynchronousArgument = asynchronousArguments[index];
            var synchronousArgument = synchronousArguments[index];
            Assert.Equal(asynchronousArgument.GenericParameterAttributes, synchronousArgument.GenericParameterAttributes);

            var asynchronousConstraints = asynchronousArgument.GetGenericParameterConstraints();
            var synchronousConstraints = synchronousArgument.GetGenericParameterConstraints();
            Assert.Equal(asynchronousConstraints.Length, synchronousConstraints.Length);
            Assert.All(asynchronousConstraints, asynchronousConstraint =>
                Assert.Contains(
                    synchronousConstraints,
                    synchronousConstraint => HaveEquivalentTypeShape(asynchronousConstraint, synchronousConstraint)));
        }
    }

    private static bool HaveEquivalentTypeShape(Type left, Type right)
    {
        if (left.IsGenericParameter || right.IsGenericParameter)
        {
            return left.IsGenericParameter &&
                   right.IsGenericParameter &&
                   left.GenericParameterPosition == right.GenericParameterPosition &&
                   (left.DeclaringMethod == null) == (right.DeclaringMethod == null);
        }

        if (left.IsByRef || right.IsByRef)
        {
            return left.IsByRef &&
                   right.IsByRef &&
                   HaveEquivalentTypeShape(left.GetElementType()!, right.GetElementType()!);
        }

        if (left.IsPointer || right.IsPointer)
        {
            return left.IsPointer &&
                   right.IsPointer &&
                   HaveEquivalentTypeShape(left.GetElementType()!, right.GetElementType()!);
        }

        if (left.IsArray || right.IsArray)
        {
            return left.IsArray &&
                   right.IsArray &&
                   left.GetArrayRank() == right.GetArrayRank() &&
                   HaveEquivalentTypeShape(left.GetElementType()!, right.GetElementType()!);
        }

        if (left.IsGenericType || right.IsGenericType)
        {
            return left.IsGenericType &&
                   right.IsGenericType &&
                   left.GetGenericTypeDefinition() == right.GetGenericTypeDefinition() &&
                   left.GetGenericArguments()
                       .Zip(right.GetGenericArguments(), HaveEquivalentTypeShape)
                       .All(value => value);
        }

        return left == right;
    }

    private sealed class OneBot10RecordingTransport : IOneBot10ActionTransport
    {
        public int CallCount { get; private set; }
        public string? Action { get; private set; }

        public Task<OneBot10ActionTransportResult> SendAsync(
            string action,
            JsonObject? parameters,
            JsonNode? echo,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Action = action;
            var response = CreateResponse(10);
            return Task.FromResult(new OneBot10ActionTransportResult(
                action,
                parameters ?? new JsonObject(),
                echo,
                "{}",
                response,
                response.ToJsonString()));
        }
    }

    private sealed class OneBot11RecordingTransport : IOneBot11ActionTransport
    {
        public int CallCount { get; private set; }
        public string? Action { get; private set; }

        public Task<OneBot11ActionTransportResult> SendAsync(
            string action,
            JsonObject? parameters,
            JsonNode? echo,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Action = action;
            var response = CreateResponse(11);
            return Task.FromResult(new OneBot11ActionTransportResult(
                action,
                parameters ?? new JsonObject(),
                echo,
                "{}",
                response,
                response.ToJsonString()));
        }
    }

    private sealed class OneBot12RecordingTransport : IOneBot12ActionTransport
    {
        public int CallCount { get; private set; }
        public string? Action { get; private set; }

        public Task<OneBot12ActionTransportResult> SendAsync(
            string action,
            JsonObject? parameters,
            string? echo,
            OneBotSdk.Net.V12.OneBot12Self? self,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Action = action;
            var response = CreateResponse(12);
            return Task.FromResult(new OneBot12ActionTransportResult(
                action,
                parameters ?? new JsonObject(),
                echo,
                self,
                "{}",
                response,
                response.ToJsonString()));
        }
    }

    private sealed class OneBot11FaultingTransport : IOneBot11ActionTransport
    {
        private readonly Exception _exception;

        public OneBot11FaultingTransport(Exception exception)
        {
            _exception = exception;
        }

        public Task<OneBot11ActionTransportResult> SendAsync(
            string action,
            JsonObject? parameters,
            JsonNode? echo,
            CancellationToken cancellationToken)
        {
            return Task.FromException<OneBot11ActionTransportResult>(_exception);
        }
    }

    private static JsonObject CreateResponse(int value)
    {
        return new JsonObject
        {
            ["status"] = "ok",
            ["retcode"] = 0,
            ["data"] = new JsonObject { ["value"] = value },
            ["message"] = string.Empty
        };
    }
}
