using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Rasm.Grasshopper;

// --- [PLUGIN_REGISTRY] -------------------------------------------------------------------
public static class PluginRegistry {
    private const BindingFlags StaticPublic = BindingFlags.Public | BindingFlags.Static;
    private const string SkipEnvVar = "RASM_SKIP_SPEC_VALIDATION";
    private static int subscribed;

    [SuppressMessage("Usage", "CA2255",
        Justification = "Foundation drift defense: subscribe once to AssemblyLoad so downstream plugin assemblies are validated before GH2's plugin loader reflects on their types. No work occurs until a downstream assembly loads.")]
    [ModuleInitializer]
    internal static void Subscribe() {
        if (Interlocked.Exchange(location1: ref subscribed, value: 1) is not 0) {
            return;
        }
        if (Environment.GetEnvironmentVariable(variable: SkipEnvVar) is { Length: > 0 }) {
            return;
        }
        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
    }
    public static Fin<Unit> Validate(Assembly assembly) {
        ArgumentNullException.ThrowIfNull(argument: assembly);
        Seq<string> faults = Discover(assembly: assembly).Bind(Inspect);
        return faults.IsEmpty
            ? Fin.Succ(Unit.Default)
            : Fin.Fail<Unit>(Error.New(message: string.Join(separator: "; ", values: faults)));
    }
    private static void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args) {
        Assembly loaded = args.LoadedAssembly;
        Assembly self = typeof(PluginRegistry).Assembly;
        if (loaded == self || !References(loaded: loaded, target: self.GetName().Name ?? string.Empty)) {
            return;
        }
        _ = Validate(assembly: loaded).IfFail(error => throw new PluginSpecException(message: $"GH2 plugin spec drift in {loaded.GetName().Name}: {error.Message}"));
    }
    private static bool References(Assembly loaded, string target) {
        try {
            return loaded.GetReferencedAssemblies().Any(reference => string.Equals(a: reference.Name, b: target, comparisonType: StringComparison.Ordinal));
        } catch (FileNotFoundException) {
            return false;
        } catch (NotSupportedException) {
            return false;
        }
    }
    private static Seq<Type> Discover(Assembly assembly) {
        ServiceCollection services = new();
        _ = services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(action: filter => filter.AssignableTo(type: typeof(IComponentSpec)).Where(static type => !type.IsAbstract && !type.IsGenericTypeDefinition && type.IsClass))
            .UsingRegistrationStrategy(registrationStrategy: RegistrationStrategy.Skip)
            .AsSelf()
            .WithSingletonLifetime());
        return toSeq(services.Select(static descriptor => descriptor.ServiceType).Distinct());
    }
    private static Seq<string> Inspect(Type spec) {
        Seq<IPort> inputs = Read<IPort>(spec: spec, member: nameof(IComponentSpec.Inputs));
        Seq<IPort> outputs = toSeq(Read<IOutputGroup>(spec: spec, member: nameof(IComponentSpec.Outputs)).Bind(static group => group.Ports));
        Seq<string> faults = Seq(
            inputs.IsEmpty ? Some($"{spec.FullName}: Inputs is empty") : Option<string>.None,
            outputs.IsEmpty ? Some($"{spec.FullName}: Outputs is empty") : Option<string>.None
        ).Somes();
        return toSeq(faults.Concat(second: Duplicates(spec: spec, ports: inputs.Concat(second: outputs))));
    }
    private static Seq<T> Read<T>(Type spec, string member) =>
        spec.GetProperty(name: member, bindingAttr: StaticPublic)?.GetValue(obj: null) is Seq<T> seq ? seq : Seq<T>();
    private static IEnumerable<string> Duplicates(Type spec, IEnumerable<IPort> ports) =>
        ports.GroupBy(keySelector: static port => port.Code, comparer: StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(group => $"{spec.FullName}: duplicate port code '{group.Key}' on {string.Join(separator: ", ", values: group.Select(static port => port.Name))}");
}

// --- [PLUGIN_SPEC_EXCEPTION] -------------------------------------------------------------
public sealed class PluginSpecException : InvalidOperationException {
    public PluginSpecException() { }
    public PluginSpecException(string message) : base(message: message) { }
    public PluginSpecException(string message, Exception innerException) : base(message: message, innerException: innerException) { }
}
