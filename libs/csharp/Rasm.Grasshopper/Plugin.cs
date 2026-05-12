using System.Reflection;
using Foundation.CSharp.Analyzers.Contracts;
using GhPlugin = Grasshopper2.Framework.Plugin;

namespace Rasm.Grasshopper;

/// <summary>
/// Polymorphic base for Grasshopper 2 plugin manifests. Subclasses pass explicit manifest metadata
/// and bind <see cref="GhPlugin.Icon"/> in their own constructor. Author and Copyright resolve
/// declaratively from <see cref="Grasshopper2.AuthorAttribute"/> on the subclass and
/// <see cref="AssemblyCopyrightAttribute"/> on the owning assembly. The constructor scans the owning
/// assembly for every concrete <see cref="Component{TSelf}"/> subclass and validates each via
/// <see cref="ComponentManifest"/>; faults are aggregated and thrown together at plugin load.
/// </summary>
[BoundaryAdapter]
public abstract class Plugin : GhPlugin {
    private readonly Lazy<string> author;
    private readonly Lazy<string> copyright;
    protected Plugin(
        Guid id,
        Nomen nomen,
        Version? version) : base(
            id: id,
            nomen: nomen,
            version: version) {
        Type self = GetType();
        author = new Lazy<string>(valueFactory: () => self.GetCustomAttribute<Grasshopper2.AuthorAttribute>()?.Author ?? string.Empty);
        copyright = new Lazy<string>(valueFactory: () => self.Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty);
        Seq<string> faults = toSeq(self.Assembly.GetTypes()
            .Where(static type => !type.IsAbstract && !type.IsGenericTypeDefinition && type.IsClass && IsComponentSubclass(type: type))
            .Distinct()
            .SelectMany(static spec => Validate(spec: spec)));
        _ = faults.IsEmpty
            ? Unit.Default
            : throw new InvalidOperationException(message: string.Join(separator: "; ", values: faults));
    }
    private static bool IsComponentSubclass(Type type) =>
        type.BaseType is { } parent && (
            (parent.IsGenericType && parent.GetGenericTypeDefinition() == typeof(Component<>)) || IsComponentSubclass(type: parent));
    private static Seq<string> Validate(Type spec) {
        ComponentManifest manifest = ComponentManifest.For(type: spec);
        object probe = Activator.CreateInstance(type: spec)!;
        Seq<IPort> inputs = manifest.ReadInputs(instance: probe);
        Seq<IPort> outputs = manifest.ReadOutputs(instance: probe).Bind(static group => group.Ports);
        Seq<string> structuralFaults = toSeq(Seq(
            inputs.IsEmpty ? Some($"{spec.FullName}: Inputs is empty") : Option<string>.None,
            outputs.IsEmpty ? Some($"{spec.FullName}: Outputs is empty") : Option<string>.None).Somes());
        Seq<string> returnTypeFaults = manifest.OutputProperties
            .Filter(static property => !typeof(IOutputGroup).IsAssignableFrom(c: property.PropertyType))
            .Map(property => $"{spec.FullName}: [Output] property '{property.Name}' must return IOutputGroup (found {property.PropertyType.Name})");
        Seq<string> inputOrderFaults = DuplicateOrders(manifest.InputFields.Map(static field => (Member: field.Name, field.GetCustomAttribute<InputAttribute>()!.Order)), kind: "Input", owner: spec.FullName);
        Seq<string> outputOrderFaults = DuplicateOrders(manifest.OutputProperties.Map(static property => (Member: property.Name, property.GetCustomAttribute<OutputAttribute>()!.Order)), kind: "Output", owner: spec.FullName);
        Seq<string> duplicates = toSeq(inputs.Concat(outputs)
            .GroupBy(keySelector: static port => port.Code, comparer: StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(group => $"{spec.FullName}: duplicate port code '{group.Key}' on {string.Join(separator: ", ", values: group.Select(static port => port.Name))}"));
        Seq<string> nameDuplicates = toSeq(inputs.Concat(outputs)
            .GroupBy(keySelector: static port => port.Name, comparer: StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(group => $"{spec.FullName}: duplicate port name '{group.Key}' on codes {string.Join(separator: ", ", values: group.Select(static port => port.Code))}"));
        return structuralFaults.Concat(returnTypeFaults).Concat(inputOrderFaults).Concat(outputOrderFaults).Concat(duplicates).Concat(nameDuplicates).ToSeq();
    }
    private static Seq<string> DuplicateOrders(Seq<(string Member, int Order)> members, string kind, string? owner) =>
        toSeq(members
            .GroupBy(keySelector: static pair => pair.Order)
            .Where(static group => group.Count() > 1)
            .Select(group => $"{owner}: duplicate {kind} order {group.Key} on members {string.Join(separator: ", ", values: group.Select(static pair => pair.Member))}"));
    public override string Author => author.Value;
    public override string Copyright => copyright.Value;
}
