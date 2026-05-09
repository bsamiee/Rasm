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
        internal Validation<Error, Seq<TOut>> Execute(
            params ReadOnlySpan<TGeometry> input) =>
            Execute(input: input, start: 0, length: input.Length);
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
            RuntimeOrSentinel().Bind((AnalysisRuntime rt) =>
                query.Apply(geometry: input).Run(rt));
        private Fin<AnalysisRuntime> RuntimeOrSentinel() =>
            (runtime.IsSucc, requiresContext) switch {
                (true, _) => runtime,
                (false, true) => runtime,
                // BOUNDARY ADAPTER — sentinel runtime for context-free queries: query.Apply returns Eff<RT,A>,
                // which requires an RT to evaluate; a context-free evaluator never reads rt.Context, so we
                // synthesize an uninitialized runtime to satisfy the Eff dispatch without invoking native
                // Rhino tolerance constructors (unavailable in test harnesses).
                (false, false) => Fin.Succ(new AnalysisRuntime(
                    Context: (GeometryContext)System.Runtime.CompilerServices.RuntimeHelpers
                        .GetUninitializedObject(type: typeof(GeometryContext)))),
            };
    }
}
