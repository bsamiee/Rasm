using Core.Domain;
using Core.Runtime;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;
namespace Analysis;

// --- [SURFACE] ---------------------------------------------------------------------------------

public static class Analyze {
    public static readonly Eff<AnalysisRuntime, AnalysisRuntime> Asks =
        Eff.runtime<AnalysisRuntime>().As();
    public static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        Run(
            query: query,
            runtime: query switch {
                Query<TGeometry, TOut> candidate => Fin.Fail<AnalysisRuntime>(candidate.Key.MissingContext()),
                _ => Fin.Fail<AnalysisRuntime>(OperationFault.MissingOperation()),
            },
            requiresContext: false,
            input: input);
    public static Scope From(RhinoDoc? doc) =>
        new(runtime: GeometryContext.FromDocument(doc: doc)
            .ToFin()
            .Map(static (GeometryContext context) => new AnalysisRuntime(Context: context)));
    public static Scope In(UnitSystem units) =>
        new(runtime: GeometryContext.CreateDefault(units: units)
            .ToFin()
            .Map(static (GeometryContext context) => new AnalysisRuntime(Context: context)));
    public static Scope In(
        double absolute,
        double relative,
        double angle,
        UnitSystem units) =>
        new(runtime: GeometryContext.FromKnownUnits(
                absoluteTolerance: absolute,
                relativeTolerance: relative,
                angleToleranceRadians: angle,
                units: units)
            .ToFin()
            .Map(static (GeometryContext context) => new AnalysisRuntime(Context: context)));
    public static Scope In(GeometryContext context) =>
        new(runtime: Optional(context)
            .ToFin(Query.ScopeKey.MissingContext())
            .Map(static (GeometryContext geometryContext) => new AnalysisRuntime(Context: geometryContext)));
    public sealed record Scope {
        public Fin<AnalysisRuntime> Runtime { get; }
        internal Scope(Fin<AnalysisRuntime> runtime) =>
            Runtime = runtime;
        public Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
            Query<TGeometry, TOut>? query,
            params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
            Analyze.Run(
                query: query,
                runtime: Runtime,
                requiresContext: true,
                input: input);
    }
    private static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        Fin<AnalysisRuntime> runtime,
        bool requiresContext,
        ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        query switch {
            Query<TGeometry, TOut> candidate => new Program<TGeometry, TOut>(
                    query: candidate,
                    runtime: runtime,
                    requiresContext: requiresContext)
                .Execute(input: input),
            _ => Fin.Fail<Seq<TOut>>(OperationFault.MissingOperation()).ToValidation(),
        };
    private sealed class Program<TGeometry, TOut>(
        Query<TGeometry, TOut> query,
        Fin<AnalysisRuntime> runtime,
        bool requiresContext) where TGeometry : notnull {
        private readonly Fin<Unit> setup = (
                query.Ready,
                (requiresContext || query.RequiresContext) switch {
                    true => runtime.Map(static (AnalysisRuntime _) => unit),
                    false => Fin.Succ(unit),
                })
            .Apply(static (Unit _, Unit _) => unit)
            .As();
        internal Validation<Error, Seq<TOut>> Execute(
            params ReadOnlySpan<TGeometry> input) =>
            setup.IsSucc switch {
                true => Execute(input: input, start: 0, length: input.Length),
                false => setup
                    .Map(static (Unit _) => Seq<TOut>())
                    .ToValidation(),
            };
        private Validation<Error, Seq<TOut>> Execute(
            ReadOnlySpan<TGeometry> input,
            int start,
            int length) =>
            length switch {
                0 => Fin.Succ(Seq<TOut>()).ToValidation(),
                1 => Apply(input: input[start]).ToValidation(),
                _ => (
                    Execute(
                        input: input,
                        start: start,
                        length: length / 2),
                    Execute(
                        input: input,
                        start: start + (length / 2),
                        length: length - (length / 2))
                ).Apply(static (Seq<TOut> left, Seq<TOut> right) => left + right)
                .As(),
            };
        private Fin<Seq<TOut>> Apply(TGeometry input) =>
            Optional(input)
                .ToFin(ValidationFault.MissingGeometry())
                .Bind(ApplyValidated);
        private Fin<Seq<TOut>> ApplyValidated(TGeometry input) =>
            (query.Requirement, input) switch {
                (GeometryRequirement requirement, GeometryBase native)
                    when requirement != GeometryRequirement.None =>
                    runtime.Bind((AnalysisRuntime rt) =>
                        rt.Context.Validate(
                                geometry: native,
                                requirement: requirement)
                            .ToFin()
                            .Bind(_ => query.ApplyDirect(
                                geometry: input,
                                context: Fin.Succ(rt.Context)))),
                _ => query.ApplyDirect(
                    geometry: input,
                    context: runtime.Map(static (AnalysisRuntime rt) => rt.Context)),
            };
    }
}
