using System.Collections.Generic;

namespace Rasm.Domain;

internal readonly record struct Op {
    internal Op(string name) => Name = name;
    internal string Name { get; }
}
internal static class OpFault {
    internal const int UnsupportedCode = 9104;
    internal static Error MissingOperation() => Error.New(message: "Geometry operation requires a query.");
    internal static Error MissingContext(this Op key) =>
        Error.New(message: $"Geometry operation '{key.Name}' requires a model context.");
    internal static Error InvalidInput(this Op key) =>
        Error.New(message: $"Geometry operation '{key.Name}' received invalid Rhino input.");
    internal static Error InvalidResult(this Op key) =>
        Error.New(message: $"Geometry operation '{key.Name}' produced no valid Rhino result.");
    internal static Error Cancelled() => Error.New(message: "Geometry operation was cancelled.");
    internal static Error Unsupported(this Op key, Type geometryType, Type outputType) =>
        Error.New(code: UnsupportedCode, message: $"Geometry operation '{key.Name}' does not support geometry '{geometryType.Name}' with output '{outputType.Name}'.");
    internal static Error ComputationFailed(string label) =>
        Error.New(message: $"Rhino {label} computation failed.");
    internal static Error ComputationUnsupported(string label, Type geometryType) =>
        Error.New(message: $"Rhino {label} computation does not support geometry '{geometryType.Name}'.");
    internal static Error PrimitiveNoEdges(this Op key, string primitive) =>
        Error.New(message: $"Geometry operation '{key.Name}' rejects '{primitive}' primitive: no edges.");
    internal static Error PrimitiveNoVertices(this Op key, string primitive) =>
        Error.New(message: $"Geometry operation '{key.Name}' rejects '{primitive}' primitive: no vertices.");
}
