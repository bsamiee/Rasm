using System.Reflection;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI.Icon;
using GhPlugin = Grasshopper2.Framework.Plugin;

namespace Rasm.Grasshopper;

[BoundaryAdapter]
public abstract class Plugin : GhPlugin {
    private readonly string author;
    private readonly string copyright;
    protected Plugin(Guid id, Nomen nomen, Version? version) : base(id: id, nomen: nomen, version: version) {
        Type self = GetType();
        author = self.GetCustomAttribute<Grasshopper2.AuthorAttribute>()?.Author ?? string.Empty;
        copyright = self.Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;
    }
    public override void OnLoaded() {
        base.OnLoaded();
        Seq<string> faults = toSeq(GetType().Assembly.GetTypes())
            .Filter(static type => typeof(Component).IsAssignableFrom(c: type) && !type.IsAbstract && !type.IsGenericTypeDefinition)
            .Distinct()
            .Bind(Validate);
        _ = faults.IsEmpty ? Unit.Default : throw new InvalidOperationException(message: string.Join(separator: "; ", values: faults));
    }
    private static Seq<string> Validate(Type spec) {
        Component probe = (Component)Activator.CreateInstance(type: spec)!;
        Seq<PortSpec> inputSpecs = probe.Spec.Inputs;
        Seq<OutputSpec> outputSpecs = probe.Spec.Outputs;
        Seq<IPort> inputs = toSeq(inputSpecs.AsIterable().Select(static pair => pair.Port).Where(static port => port is not null).Select(static port => port!));
        Seq<IOutputGroup> outputGroups = toSeq(outputSpecs.AsIterable().Select(static pair => pair.Group).Where(static group => group is not null).Select(static group => group!));
        Seq<IPort> outputs = outputGroups.Bind(static group => group.Ports);
        Seq<string> structuralFaults = toSeq(Seq(
            inputs.IsEmpty ? Some($"{spec.FullName}: Inputs is empty") : Option<string>.None,
            outputs.IsEmpty ? Some($"{spec.FullName}: Outputs is empty") : Option<string>.None).Somes());
        Seq<string> nullFaults = toSeq(inputSpecs
                .AsIterable()
                .Select((pair, index) => pair.Port is null ? $"{spec.FullName}: input {index} is null" : null)
                .Where(static fault => fault is not null)
                .Select(static fault => fault!))
            .Concat(toSeq(outputSpecs
                .AsIterable()
                .Select((pair, index) => pair.Group is null ? $"{spec.FullName}: output {index} is null" : null)
                .Where(static fault => fault is not null)
                .Select(static fault => fault!)))
            .ToSeq();
        Seq<(string Side, Seq<IPort> Ports)> sides = Seq((Side: "input", Ports: inputs), (Side: "output", Ports: outputs));
        Seq<string> codeDuplicates = sides.Bind(side => toSeq(side.Ports
            .GroupBy(keySelector: static port => port.Code, comparer: StringComparer.Ordinal)
            .Where(static group => group.Skip(1).Any())
            .Select(group => $"{spec.FullName}: duplicate {side.Side} port code '{group.Key}' on {string.Join(separator: ", ", values: group.Select(static port => port.Name))}")));
        Seq<string> nameDuplicates = sides.Bind(side => toSeq(side.Ports
            .GroupBy(keySelector: static port => port.Name, comparer: StringComparer.Ordinal)
            .Where(static group => group.Skip(1).Any())
            .Select(group => $"{spec.FullName}: duplicate {side.Side} port name '{group.Key}' on codes {string.Join(separator: ", ", values: group.Select(static port => port.Code))}")));
        return structuralFaults.Concat(nullFaults).Concat(codeDuplicates).Concat(nameDuplicates).ToSeq();
    }
    public override string Author => author;
    public override string Copyright => copyright;
    public override IIcon Icon =>
        GetType().GetCustomAttribute<IconAttribute>() switch {
            IconAttribute attr => AbstractIcon.FromResource(name: attr.Name, type: GetType()),
            _ => base.Icon,
        };
}
