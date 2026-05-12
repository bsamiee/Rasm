using System.Reflection;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI.Icon;
using GhPlugin = Grasshopper2.Framework.Plugin;

namespace Rasm.Grasshopper;

[BoundaryAdapter]
public abstract class Plugin : GhPlugin {
    private readonly Lazy<string> author;
    private readonly Lazy<string> copyright;
    protected Plugin(Guid id, Nomen nomen, Version? version) : base(id: id, nomen: nomen, version: version) {
        Type self = GetType();
        author = new Lazy<string>(valueFactory: () => self.GetCustomAttribute<Grasshopper2.AuthorAttribute>()?.Author ?? string.Empty);
        copyright = new Lazy<string>(valueFactory: () => self.Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty);
    }
    public override void OnLoaded() {
        base.OnLoaded();
        Seq<string> faults = toSeq(GetType().Assembly.GetTypes())
            .Filter(static type => !type.IsAbstract && !type.IsGenericTypeDefinition && type.IsClass && IsComponentSubclass(type: type))
            .Distinct()
            .Bind(static spec => Validate(spec: spec));
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
        Seq<IPort> inputs = manifest.ReadInputs(instance: probe).Map(static pair => pair.Port);
        Seq<IPort> outputs = manifest.ReadOutputs(instance: probe).Map(static pair => pair.Group).Bind(static group => group.Ports);
        Seq<string> structuralFaults = toSeq(Seq(
            inputs.IsEmpty ? Some($"{spec.FullName}: Inputs is empty") : Option<string>.None,
            outputs.IsEmpty ? Some($"{spec.FullName}: Outputs is empty") : Option<string>.None).Somes());
        Seq<string> returnTypeFaults = manifest.OutputProperties
            .Filter(static pair => !typeof(IOutputGroup).IsAssignableFrom(c: pair.Property.PropertyType))
            .Map(pair => $"{spec.FullName}: [Output] property '{pair.Property.Name}' must return IOutputGroup (found {pair.Property.PropertyType.Name})");
        Seq<string> codeDuplicates = toSeq(inputs.Concat(outputs)
            .GroupBy(keySelector: static port => port.Code, comparer: StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(group => $"{spec.FullName}: duplicate port code '{group.Key}' on {string.Join(separator: ", ", values: group.Select(static port => port.Name))}"));
        Seq<string> nameDuplicates = toSeq(inputs.Concat(outputs)
            .GroupBy(keySelector: static port => port.Name, comparer: StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(group => $"{spec.FullName}: duplicate port name '{group.Key}' on codes {string.Join(separator: ", ", values: group.Select(static port => port.Code))}"));
        return structuralFaults.Concat(returnTypeFaults).Concat(codeDuplicates).Concat(nameDuplicates).ToSeq();
    }
    public override string Author => author.Value;
    public override string Copyright => copyright.Value;
    public override IIcon Icon =>
        GetType().GetCustomAttribute<IconAttribute>() switch {
            IconAttribute attr => AbstractIcon.FromResource(name: attr.Name, type: GetType()),
            _ => base.Icon,
        };
}
