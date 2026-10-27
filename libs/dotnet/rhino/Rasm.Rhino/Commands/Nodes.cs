using Rhino.NodeInCode;

namespace Rasm.RhinoCommon.Commands;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NodeOutput(string Name, Option<object> Value);

public sealed record NodeResult(Seq<NodeOutput> Outputs, Seq<string> Warnings);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Nodes {
    public static IO<Seq<ComponentFunctionInfo>> Functions() =>
        IO.lift(static () => toSeq(Components.NodeInCodeFunctions.GetDynamicMembers()).Strict());

    public static IO<ComponentFunctionInfo> Find(string fullName) =>
        IO.lift(() => Conversions.Found(Components.FindComponent(fullName)));

    public static IO<NodeResult> Evaluate(ComponentFunctionInfo node, Seq<Option<object>> inputs, bool keepTree) =>
        IO.lift(() => Conversions.Found(node.Evaluate(inputs.Map(static input => input.ValueUnsafe()), keepTree, out string[] warnings)).Map(values => new NodeResult(
            toSeq(node.OutputNames).Zip(toSeq(values), static (name, value) => new NodeOutput(name, Optional(value))),
            Conversions.Rows(warnings))));
}
