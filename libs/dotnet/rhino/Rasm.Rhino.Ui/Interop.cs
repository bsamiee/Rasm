using Rasm.Rhino.Document;
using Rhino.DocObjects;
using Rhino.NodeInCode;
using Rhino.Runtime;

namespace Rasm.Rhino.Ui;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScriptSource {
    public sealed record Script(string Text) : ScriptSource;

    public sealed record File(string Path) : ScriptSource;

    public sealed record FileInScope(string Path) : ScriptSource;

    public sealed record Compiled(PythonCompiledCode CompiledCode) : ScriptSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NamedValue {
    public abstract IO<Unit> Write(NamedParametersEventArgs e, string name);

    public sealed record String(string Value) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetString(name, out string value), new String(value));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Value));
    }

    public sealed record Strings(Seq<string> Values) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetStrings(name, out string[] values), new Strings(toSeq(values)));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Values));
    }

    public sealed record Bool(bool Value) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetBool(name, out bool value), new Bool(value));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Value));
    }

    public sealed record Int(int Value) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetInt(name, out int value), new Int(value));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Value));
    }

    public sealed record UnsignedInt(uint Value) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetUnsignedInt(name, out uint value), new UnsignedInt(value));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Value));
    }

    public sealed record UnsignedInts(Seq<uint> Values) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetUints(name, out uint[] values), new UnsignedInts(toSeq(values)));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Values));
    }

    public sealed record Double(double Value) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetDouble(name, out double value), new Double(value));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Value));
    }

    public sealed record Id(Guid Value) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetGuid(name, out Guid value), new Id(value));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Value));
    }

    public sealed record Ids(Seq<Guid> Values) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetGuids(name, out Guid[] values), new Ids(toSeq(values)));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Values));
    }

    public sealed record Color(System.Drawing.Color Value) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetColor(name, out System.Drawing.Color value), new Color(value));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Value));
    }

    public sealed record Point2i(System.Drawing.Point Value) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetPoint2i(name, out System.Drawing.Point value), new Point2i(value));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Value));
    }

    public sealed record Point(Point3d Value) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetPoint(name, out Point3d value), new Point(value));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Value));
    }

    public sealed record Vector(Vector3d Value) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetVector(name, out Vector3d value), new Vector(value));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Value));
    }

    public sealed record Line(global::Rhino.Geometry.Line Value) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetLine(name, out global::Rhino.Geometry.Line value), new Line(value));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Value));
    }

    public sealed record Arc(global::Rhino.Geometry.Arc Value) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetArc(name, out global::Rhino.Geometry.Arc value), new Arc(value));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Value));
    }

    public sealed record Plane(global::Rhino.Geometry.Plane Value) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetPlane(name, out global::Rhino.Geometry.Plane value), new Plane(value));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Value));
    }

    public sealed record Points(Seq<Point3d> Values) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetPoints(name, out Point3d[] values), new Points(toSeq(values)));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Values.ToArray()));
    }

    public sealed record GeometryBases(Seq<GeometryBase> Values) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetGeometry(name, out GeometryBase[] values), new GeometryBases(toSeq(values)));

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Values));
    }

    public sealed record MeshParameters(MeshingParameters Value) : NamedValue {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) {
            bool found = e.TryGetMeshParameters(name, out MeshingParameters value);
            if (!found)
                value.Dispose();
            return Answers.Found<NamedReading>(found, new MeshParameters(value));
        }

        public override IO<Unit> Write(NamedParametersEventArgs e, string name) => IO.lift(() => e.Set(name, Value));
    }
}

[Union]
public abstract partial record NamedReading {
    public sealed record Value(NamedValue Named) : NamedReading;

    public sealed record Viewport(ViewportInfo Info) : NamedReading {
        public static Option<NamedReading> Read(NamedParametersEventArgs e, string name) =>
            Answers.Found<NamedReading>(e.TryGetViewport(name, out ViewportInfo info), new Viewport(info));
    }
}

public sealed record NamedParameter(string Name, Func<NamedParametersEventArgs, string, Option<NamedReading>> Read, bool Required);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostInterop {
    // --- [PYTHON]
    public static IO<PythonScript> Engine() =>
        IO.lift(static () => Missing.Unless(PythonScript.Create(), nameof(PythonScript.Create)));

    public static IO<Unit> Run(PythonScript engine, ScriptSource source, Seq<(string Name, object Value)> bindings) =>
        from bound in IO.lift(() => Bound(engine, bindings))
        from ran in source.Switch(
            engine,
            script: static (scope, script) => IO.lift(() => Refused.Unless(scope.ExecuteScript(script.Text), nameof(PythonScript.ExecuteScript))),
            file: static (scope, file) => IO.lift(() => Refused.Unless(scope.ExecuteFile(file.Path), nameof(PythonScript.ExecuteFile))),
            fileInScope: static (scope, file) => IO.lift(() => Refused.Unless(scope.ExecuteFileInScope(file.Path), nameof(PythonScript.ExecuteFileInScope))),
            compiled: static (scope, compiled) => IO.lift(() => compiled.CompiledCode.Execute(scope)))
        select ran;

    public static IO<PythonCompiledCode> Compile(PythonScript engine, string script) =>
        IO.lift(() => Missing.Unless(engine.Compile(script), nameof(PythonScript.Compile)));

    public static IO<Option<object>> EvaluateExpression(PythonScript engine, string statements, string expression, Seq<(string Name, object Value)> bindings) =>
        from bound in IO.lift(() => Bound(engine, bindings))
        from value in IO.lift(() => Optional(engine.EvaluateExpression(statements, expression)))
        select value;

    private static Unit Bound(PythonScript engine, Seq<(string Name, object Value)> bindings) =>
        bindings.Iter(binding => engine.SetVariable(binding.Name, binding.Value));

    // --- [COMPONENTS]
    public static IO<Option<ComponentFunctionInfo>> FindComponent(string fullName) =>
        IO.lift(() => Optional(Components.FindComponent(fullName)));

    public static IO<(Seq<object> Values, Seq<string> Warnings)> Evaluate(ComponentFunctionInfo function, Seq<object> args, bool keepTree) =>
        IO.lift(() => (Values: toSeq(function.Evaluate(args, keepTree, out string[] warnings)), Warnings: toSeq(warnings)));

    // --- [CALLBACKS]
    private static readonly AtomHashMap<string, Guid> Names = AtomHashMap<string, Guid>();

    public static IO<IDisposable> Register(string name, Seq<NamedParameter> request, Func<Map<string, NamedReading>, Fin<Map<string, NamedValue>>> body, Action<Error> reject) =>
        from token in IO.lift(static () => Guid.NewGuid())
        from attached in Events.AttachAll(Seq(
            IO.lift(() => NameTaken.Unless(Names.FindOrAdd(name, token) == token, name)).Map<IDisposable>(_ => new Disposal(() => Names.Remove(name))),
            Events.Attach(
                h => HostUtils.RegisterNamedCallback(name, h),
                _ => HostUtils.RemoveNamedCallback(name),
                Answers.Handler<NamedParametersEventArgs>(e => Reply(e, request, body, reject), reject))))
        select attached;

    public static IO<Option<Map<string, NamedReading>>> Execute(string name, Map<string, NamedValue> args, Seq<NamedParameter> response) =>
        Disposal.Using(static () => new NamedParametersEventArgs(), e =>
            from written in args.ToSeq().TraverseM(row => row.Value.Write(e, row.Key)).As()
            from answered in IO.lift(() => HostUtils.ExecuteNamedCallback(name, e))
            from reply in answered ? Decode(e, response).Map(static found => Some(found)) : IO.pure(Option<Map<string, NamedReading>>.None)
            select reply);

    private static IO<Unit> Reply(NamedParametersEventArgs e, Seq<NamedParameter> request, Func<Map<string, NamedReading>, Fin<Map<string, NamedValue>>> body, Action<Error> reject) =>
        from args in Decode(e, request)
        from reply in IO.lift(() => body(args))
        from fails in reply.ToSeq().Map<K<IO, Unit>>(row => row.Value.Write(e, row.Key)).Fails().As()
        from reported in IO.lift(() => fails.Iter(reject))
        select unit;

    private static IO<Map<string, NamedReading>> Decode(NamedParametersEventArgs e, Seq<NamedParameter> parameters) =>
        parameters.TraverseM(parameter =>
                from value in IO.lift(() => parameter.Read(e, parameter.Name))
                from present in IO.lift(() => ParameterMissing.Unless(value.IsSome || !parameter.Required, parameter.Name))
                select value.Map(found => (parameter.Name, found)))
            .As()
            .Map(static rows => toMap(rows.Somes()));
}
