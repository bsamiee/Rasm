using Core.Domain;
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
            runtime: Option<AnalysisRuntime>.None,
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
        public Scope WithIndex(int index) =>
            new(runtime: Runtime.Map((AnalysisRuntime rt) => rt with {
                Index = IndexHint.Create(value: index).Match(
                    Succ: static (IndexHint hint) => Some(hint),
                    Fail: static (Error _) => Option<IndexHint>.None),
            }));
        public Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
            Query<TGeometry, TOut>? query,
            params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
            Analyze.Run(
                query: query,
                runtime: Runtime.ToOption(),
                input: input);
    }
    private static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        Option<AnalysisRuntime> runtime,
        ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        query switch {
            Query<TGeometry, TOut> candidate => new Program<TGeometry, TOut>(
                    query: candidate,
                    runtime: runtime)
                .Execute(input: input),
            _ => Fin.Fail<Seq<TOut>>(OperationFault.MissingOperation()).ToValidation(),
        };
    private sealed class Program<TGeometry, TOut>(
        Query<TGeometry, TOut> query,
        Option<AnalysisRuntime> runtime) where TGeometry : notnull {
        internal Validation<Error, Seq<TOut>> Execute(
            params ReadOnlySpan<TGeometry> input) =>
            input.ToArray()
                .AsIterable()
                .ToSeq()
                .Traverse(Apply)
                .As()
                .Map(static (Seq<Seq<TOut>> chunks) => chunks.Bind(static (Seq<TOut> chunk) => chunk))
                .ToValidation();
        internal Fin<Seq<TOut>> Apply(TGeometry input) =>
            Optional(input)
                .ToFin(ValidationFault.MissingGeometry())
                .Bind((TGeometry geometry) => runtime.Match(
                    Some: (AnalysisRuntime rt) => query.Apply(geometry: geometry).Run(rt),
                    None: () => Fin.Fail<Seq<TOut>>(query.Key.MissingContext())));
    }
}
