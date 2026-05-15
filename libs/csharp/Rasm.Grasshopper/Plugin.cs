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
            .Filter(static type => typeof(IRasmComponent).IsAssignableFrom(c: type) && !type.IsAbstract && !type.IsGenericTypeDefinition)
            .Distinct()
            .Bind(Validate);
        _ = faults.IsEmpty ? Unit.Default : throw new InvalidOperationException(message: string.Join(separator: "; ", values: faults));
    }
    private static Seq<string> Validate(Type spec) {
        IRasmComponent probe = (IRasmComponent)Activator.CreateInstance(type: spec)!;
        Seq<IPort> inputs = probe.Spec.Inputs.Choose(static pair => Optional(pair.Port));
        Seq<IPort> outputs = probe.Spec.Outputs.Choose(static pair => Optional(pair.Group)).Bind(static group => group.Ports);
        Seq<(string Side, Seq<IPort> Ports)> sides = Seq((Side: "input", Ports: inputs), (Side: "output", Ports: outputs));
        Seq<string> structural = toSeq(Seq(
            ClosedComponentSelf(type: spec) ? null : $"{spec.FullName}: Component<TSelf> does not match concrete component type",
            inputs.IsEmpty ? $"{spec.FullName}: Inputs is empty" : null,
            outputs.IsEmpty ? $"{spec.FullName}: Outputs is empty" : null).Choose(Optional));
        Seq<string> sourceFaults = probe.Spec.Outputs.Choose(pair => Optional(pair.Group)
            .Bind(group => inputs.Exists(input => ReferenceEquals(objA: input, objB: group.Input))
                ? Option<string>.None
                : Some($"{spec.FullName}: output group input '{group.Input.Name}' is not a declared input port instance")));
        Seq<string> nullFaults = NullsAt(spec: spec, side: "input", count: probe.Spec.Inputs.Count, missing: i => probe.Spec.Inputs[i].Port is null)
            .Concat(NullsAt(spec: spec, side: "output", count: probe.Spec.Outputs.Count, missing: i => probe.Spec.Outputs[i].Group is null));
        Seq<string> duplicates = sides.Bind(side =>
            Duplicates(spec: spec, side: side.Side, ports: side.Ports, key: "code", project: static port => port.Code, label: static port => port.Name)
                .Concat(Duplicates(spec: spec, side: side.Side, ports: side.Ports, key: "name", project: static port => port.Name, label: static port => port.Code)));
        Seq<string> portCount = outputs.Count > 24 ? Seq($"{spec.FullName}: output port count {outputs.Count} exceeds 24") : Seq<string>();
        Seq<string> codeLengths = sides.Bind(side => side.Ports.Choose(port =>
            port.Code.Length is > 0 and <= 2 ? Option<string>.None : Some($"{spec.FullName}: {side.Side} port '{port.Name}' code '{port.Code}' must be 1-2 characters")));
        return structural.Concat(sourceFaults).Concat(nullFaults).Concat(duplicates).Concat(portCount).Concat(codeLengths).ToSeq();
    }
    private static bool ClosedComponentSelf(Type type) =>
        type.BaseType switch {
            Type parent when parent.IsGenericType && parent.GetGenericTypeDefinition() == typeof(Component<>) => parent.GetGenericArguments()[0] == type,
            Type parent => ClosedComponentSelf(type: parent),
            _ => false,
        };
    private static Seq<string> NullsAt(Type spec, string side, int count, Func<int, bool> missing) =>
        toSeq(Enumerable.Range(start: 0, count: count).Where(predicate: missing.Invoke).Select(index => $"{spec.FullName}: {side} {index} is null"));
    private static Seq<string> Duplicates(Type spec, string side, Seq<IPort> ports, string key, Func<IPort, string> project, Func<IPort, string> label) =>
        toSeq(ports.GroupBy(keySelector: project, comparer: StringComparer.Ordinal)
            .Where(static group => group.Skip(1).Any())
            .Select(group => $"{spec.FullName}: duplicate {side} port {key} '{group.Key}' on {string.Join(separator: ", ", values: group.Select(label))}"));
    public override string Author => author;
    public override string Copyright => copyright;
    public override IIcon Icon => IconAttribute.Resolve(owner: GetType(), fallback: base.Icon);
}
