using System.Reflection;
using Foundation.CSharp.Analyzers.Contracts;
using GhPlugin = Grasshopper2.Framework.Plugin;

namespace Rasm.Grasshopper;

/// <summary>
/// Polymorphic base for Grasshopper 2 plugin manifests. Subclasses pass explicit manifest metadata
/// and bind <see cref="GhPlugin.Icon"/> in their own constructor.
/// </summary>
[BoundaryAdapter]
public abstract class Plugin : GhPlugin {
    private const BindingFlags StaticPublic = BindingFlags.Public | BindingFlags.Static;

    private readonly string pluginAuthor;
    private readonly string pluginCopyright;

    protected Plugin(
        Guid id,
        Nomen nomen,
        Version? version,
        string author,
        string copyright) : base(
            id: id,
            nomen: nomen,
            version: version) {
        pluginAuthor = author;
        pluginCopyright = copyright;

        Seq<string> faults = toSeq(GetType().Assembly.GetTypes()
            .Where(static type => typeof(IComponentSpec).IsAssignableFrom(c: type) && !type.IsAbstract && !type.IsGenericTypeDefinition && type.IsClass)
            .Distinct()
            .SelectMany(static spec => Validate(spec: spec)));
        _ = faults.IsEmpty
            ? Unit.Default
            : throw new InvalidOperationException(message: string.Join(separator: "; ", values: faults));
    }
    private static Seq<T> StaticSeq<T>(Type spec, string name) =>
        spec.GetProperty(name: name, bindingAttr: StaticPublic)?.GetValue(obj: null) is Seq<T> seq ? seq : Seq<T>();
    private static Seq<string> Validate(Type spec) {
        Seq<IPort> inputs = StaticSeq<IPort>(spec: spec, name: nameof(IComponentSpec.Inputs));
        Seq<IPort> outputs = toSeq(StaticSeq<IOutputGroup>(spec: spec, name: nameof(IComponentSpec.Outputs)).Bind(static group => group.Ports));
        return toSeq(Seq(
            inputs.IsEmpty ? Some($"{spec.FullName}: Inputs is empty") : Option<string>.None,
            outputs.IsEmpty ? Some($"{spec.FullName}: Outputs is empty") : Option<string>.None).Somes()
            .Concat(inputs.Concat(outputs)
                .GroupBy(keySelector: static port => port.Code, comparer: StringComparer.Ordinal)
                .Where(static group => group.Count() > 1)
                .Select(group => $"{spec.FullName}: duplicate port code '{group.Key}' on {string.Join(separator: ", ", values: group.Select(static port => port.Name))}")));
    }
    public override string Author => pluginAuthor;
    public override string Copyright => pluginCopyright;
}
