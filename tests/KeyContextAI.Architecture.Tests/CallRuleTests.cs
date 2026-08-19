using System.Reflection;
using System.Runtime.CompilerServices;
using KeyContextAI.Core.Engines;

namespace KeyContextAI.Architecture.Tests;

/// <summary>
/// Enforces the strict IDesign call rules bound at the architecture lens. These are law in this
/// codebase, not convention, which is why they are a test rather than a document.
/// </summary>
/// <remarks>
/// Implemented with plain reflection rather than an architecture-testing package: the dependency
/// policy is "earned dependencies only", and no package is needed to answer these questions.
///
/// The rules:
/// <list type="bullet">
///   <item>Accessors decouple the system from the outside world and call nothing inside it.</item>
///   <item>Engines own algorithms. They call no engine, no manager, and no accessor.</item>
///   <item>Managers own flow. They call no manager, but may call engines and accessors.</item>
/// </list>
/// </remarks>
public sealed class CallRuleTests
{
    private static readonly Assembly CoreAssembly = typeof(MappingEngine).Assembly;

    /// <summary>
    /// The real component types in a namespace. Compiler-generated closures and iterator state
    /// machines are excluded — they are an implementation detail of lambdas, not components, and
    /// counting them would make the rules fail for reasons no author could act on.
    /// </summary>
    private static IEnumerable<Type> TypesIn(Assembly assembly, string namespaceSuffix) =>
        assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => t.Namespace?.EndsWith(namespaceSuffix, StringComparison.Ordinal) == true)
            .Where(t => !t.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
            .Where(t => !t.Name.StartsWith('<'));

    /// <summary>
    /// The types a type depends on: its constructor parameters, its fields, and its method
    /// parameters and returns. That is enough to catch a component reaching for a collaborator it
    /// is not allowed to know about.
    /// </summary>
    private static IEnumerable<Type> DependenciesOf(Type type)
    {
        foreach (var ctor in type.GetConstructors())
        {
            foreach (var parameter in ctor.GetParameters())
            {
                yield return parameter.ParameterType;
            }
        }

        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            yield return field.FieldType;
        }

        foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
        {
            yield return method.ReturnType;
            foreach (var parameter in method.GetParameters())
            {
                yield return parameter.ParameterType;
            }
        }
    }

    /// <summary>Unwraps generics so <c>IReadOnlyList&lt;IFooAccessor&gt;</c> is seen as the accessor.</summary>
    private static IEnumerable<Type> Flatten(Type type)
    {
        yield return type;

        if (!type.IsGenericType)
        {
            yield break;
        }

        foreach (var argument in type.GetGenericArguments())
        {
            foreach (var inner in Flatten(argument))
            {
                yield return inner;
            }
        }
    }

    private static bool IsRole(Type type, string roleSuffix) =>
        type.Name.EndsWith(roleSuffix, StringComparison.Ordinal)
        || (type.Name.StartsWith('I') && type.Name.EndsWith(roleSuffix, StringComparison.Ordinal));

    [Fact]
    public void Engines_DoNotDependOnAccessors()
    {
        // The stricter-than-classic-IDesign rule agreed at the component lens: managers hand data
        // in, so engines stay unit-testable with zero mocks.
        var violations = TypesIn(CoreAssembly, ".Engines")
            .SelectMany(engine => DependenciesOf(engine)
                .SelectMany(Flatten)
                .Where(dep => IsRole(dep, "Accessor"))
                .Select(dep => $"{engine.Name} -> {dep.Name}"))
            .Distinct()
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Engines_DoNotDependOnManagers()
    {
        var violations = TypesIn(CoreAssembly, ".Engines")
            .SelectMany(engine => DependenciesOf(engine)
                .SelectMany(Flatten)
                .Where(dep => IsRole(dep, "Manager"))
                .Select(dep => $"{engine.Name} -> {dep.Name}"))
            .Distinct()
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Engines_DoNotDependOnOtherEngines()
    {
        var violations = TypesIn(CoreAssembly, ".Engines")
            .SelectMany(engine => DependenciesOf(engine)
                .SelectMany(Flatten)
                .Where(dep => IsRole(dep, "Engine") && dep != engine)
                .Select(dep => $"{engine.Name} -> {dep.Name}"))
            .Distinct()
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Managers_DoNotDependOnOtherManagers()
    {
        var managers = TypesIn(CoreAssembly, ".Managers").ToList();

        var violations = managers
            .SelectMany(manager => DependenciesOf(manager)
                .SelectMany(Flatten)
                .Where(dep => IsRole(dep, "Manager") && dep != manager)
                .Select(dep => $"{manager.Name} -> {dep.Name}"))
            .Distinct()
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Core_DoesNotReferenceThePlatformAssembly()
    {
        // Core holds every manager and engine and must stay free of Win32, which is what lets the
        // whole correction algorithm be tested without a desktop — and what makes the recorded
        // native-hook swap a single-project change.
        var referenced = CoreAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Where(name => name?.Contains("Platform", StringComparison.Ordinal) == true)
            .ToList();

        Assert.Empty(referenced);
    }

    [Fact]
    public void EveryEngine_ImplementsAContract()
    {
        // A component outside its interface cannot be substituted in a test or swapped by DI.
        var withoutContract = TypesIn(CoreAssembly, ".Engines")
            .Where(engine => !engine.GetInterfaces().Any(i =>
                i.Namespace?.EndsWith(".Contracts", StringComparison.Ordinal) == true))
            .Select(engine => engine.Name)
            .ToList();

        Assert.Empty(withoutContract);
    }
}
