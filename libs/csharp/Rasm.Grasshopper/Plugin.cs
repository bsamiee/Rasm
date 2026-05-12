using System.Reflection;
using GhPlugin = Grasshopper2.Framework.Plugin;

namespace Rasm.Grasshopper;

/// <summary>
/// Polymorphic base for Grasshopper 2 plugin manifests. Subclasses pass explicit manifest metadata
/// and bind <see cref="GhPlugin.Icon"/> in their own constructor.
/// </summary>
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
            .SelectMany(static spec => {
                Seq<IPort> inputs = spec.GetProperty(name: nameof(IComponentSpec.Inputs), bindingAttr: StaticPublic)?.GetValue(obj: null) is Seq<IPort> inputPorts
                    ? inputPorts
                    : Seq<IPort>();
                Seq<IOutputGroup> groups = spec.GetProperty(name: nameof(IComponentSpec.Outputs), bindingAttr: StaticPublic)?.GetValue(obj: null) is Seq<IOutputGroup> outputGroups
                    ? outputGroups
                    : Seq<IOutputGroup>();
                Seq<IPort> outputs = toSeq(groups.Bind(static group => group.Ports));
                IEnumerable<IPort> ports = inputs.Concat(second: outputs);
                return Enumerable.Concat(
                    first: Seq(
                        inputs.IsEmpty ? Some($"{spec.FullName}: Inputs is empty") : Option<string>.None,
                        outputs.IsEmpty ? Some($"{spec.FullName}: Outputs is empty") : Option<string>.None
                    ).Somes(),
                    second: ports
                        .GroupBy(keySelector: static port => port.Code, comparer: StringComparer.Ordinal)
                        .Where(static group => group.Count() > 1)
                        .Select(group => $"{spec.FullName}: duplicate port code '{group.Key}' on {string.Join(separator: ", ", values: group.Select(static port => port.Name))}"));
            }));
        _ = faults.IsEmpty
            ? Unit.Default
            : throw new InvalidOperationException(message: string.Join(separator: "; ", values: faults));
    }
    public override string Author => pluginAuthor;
    public override string Copyright => pluginCopyright;
}
