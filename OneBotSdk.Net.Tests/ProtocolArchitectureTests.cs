using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OneBotSdk.Net.V10;
using OneBotSdk.Net.V11;
using OneBotSdk.Net.V12;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class ProtocolArchitectureTests
{
    private static readonly string[] VersionNamespaces =
    {
        "OneBotSdk.Net.V10",
        "OneBotSdk.Net.V11",
        "OneBotSdk.Net.V12"
    };

    [Fact]
    public void OneAssembly_ContainsAllVersionedProtocolRoots()
    {
        var v10Assembly = typeof(OneBot10Protocol).Assembly;
        var v11Assembly = typeof(OneBot11Protocol).Assembly;
        var v12Assembly = typeof(OneBot12Protocol).Assembly;

        Assert.Same(v10Assembly, v11Assembly);
        Assert.Same(v11Assembly, v12Assembly);
        Assert.Equal("OneBotSdk.Net", v10Assembly.GetName().Name);

        var exportedNamespaces = v10Assembly
            .GetExportedTypes()
            .Select(type => type.Namespace)
            .Where(value => value != null)
            .ToArray();

        foreach (var versionNamespace in VersionNamespaces)
        {
            Assert.Contains(exportedNamespaces, value =>
                value!.Equals(versionNamespace, StringComparison.Ordinal) ||
                value.StartsWith(versionNamespace + ".", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void PublicProtocolApi_DoesNotLeakAnotherVersionDto()
    {
        var assembly = typeof(OneBot10Protocol).Assembly;
        foreach (var declaringType in assembly.GetExportedTypes())
        {
            var declaringVersion = GetVersionNamespace(declaringType);
            if (declaringVersion == null)
            {
                continue;
            }

            foreach (var constructor in declaringType.GetConstructors())
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    AssertDoesNotLeakVersion(declaringType, declaringVersion, parameter.ParameterType, constructor.ToString());
                }
            }

            foreach (var method in declaringType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                AssertDoesNotLeakVersion(declaringType, declaringVersion, method.ReturnType, method.ToString());
                foreach (var parameter in method.GetParameters())
                {
                    AssertDoesNotLeakVersion(declaringType, declaringVersion, parameter.ParameterType, method.ToString());
                }
            }

            foreach (var property in declaringType.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                AssertDoesNotLeakVersion(declaringType, declaringVersion, property.PropertyType, property.ToString());
            }

            foreach (var eventInfo in declaringType.GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                AssertDoesNotLeakVersion(declaringType, declaringVersion, eventInfo.EventHandlerType, eventInfo.ToString());
            }
        }
    }

    [Fact]
    public void ProductionAssembly_DoesNotReferenceNewtonsoftJson()
    {
        var references = typeof(OneBot10Protocol).Assembly.GetReferencedAssemblies();

        Assert.DoesNotContain(references, reference =>
            string.Equals(reference.Name, "Newtonsoft.Json", StringComparison.OrdinalIgnoreCase));
    }

    private static string? GetVersionNamespace(Type type)
    {
        var typeNamespace = type.Namespace;
        if (typeNamespace == null)
        {
            return null;
        }

        return VersionNamespaces.FirstOrDefault(version =>
            typeNamespace.Equals(version, StringComparison.Ordinal) ||
            typeNamespace.StartsWith(version + ".", StringComparison.Ordinal));
    }

    private static void AssertDoesNotLeakVersion(
        Type declaringType,
        string declaringVersion,
        Type? referencedType,
        string? member)
    {
        foreach (var candidate in FlattenType(referencedType))
        {
            var referencedVersion = GetVersionNamespace(candidate);
            if (referencedVersion == null || referencedVersion == declaringVersion)
            {
                continue;
            }

            Assert.Fail(
                declaringType.FullName + "." + member +
                " exposes " + candidate.FullName +
                " from another protocol version.");
        }
    }

    private static IEnumerable<Type> FlattenType(Type? type)
    {
        if (type == null)
        {
            yield break;
        }

        // Inspect arrays, by-ref values, pointers, and generic arguments recursively.
        // 递归检查数组、按引用值、指针以及泛型参数。
        if (type.HasElementType)
        {
            foreach (var element in FlattenType(type.GetElementType()))
            {
                yield return element;
            }

            yield break;
        }

        yield return type;
        if (!type.IsGenericType)
        {
            yield break;
        }

        foreach (var argument in type.GetGenericArguments())
        {
            foreach (var nested in FlattenType(argument))
            {
                yield return nested;
            }
        }
    }
}
