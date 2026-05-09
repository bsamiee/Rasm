using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;
namespace Analysis;

// --- [SURFACE] ---------------------------------------------------------------------------------

public static class Analyze {
    public static readonly Eff<GeometryContext, GeometryContext> Asks =
        Eff.runtime<GeometryContext>().As();
    public static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        Run(
            query: query,
            runtime: Option<GeometryContext>.None,
            input: input);
    public static Scope From(RhinoDoc? doc) =>
        new(context: GeometryContext.FromDocument(doc: doc).ToFin(), index: None);
    public static Scope In(UnitSystem units) =>
        new(context: GeometryContext.CreateDefault(units: units).ToFin(), index: None);
    public static Scope In(
        double absolute,
        double relative,
        double angle,
        UnitSystem units) =>
        new(
            context: GeometryContext.FromKnownUnits(
                    absoluteTolerance: absolute,
                    relativeTolerance: relative,
                    angleToleranceRadians: angle,
                    units: units)
                .ToFin(),
            index: None);
    public static Scope In(GeometryContext context) =>
        new(
            context: Optional(context).ToFin(Query.ScopeKey.MissingContext()),
            index: None);
    public sealed record Scope {
        public Fin<GeometryContext> Context { get; init; }
        public Option<IndexHint> Index { get; init; }
        internal Scope(Fin<GeometryContext> context, Option<IndexHint> index) {
            Context = context;
            Index = index;
        }
        public Scope WithIndex(int index) =>
            this with {
                Index = IndexHint.Create(value: index).Match(
                    Succ: static (IndexHint hint) => Some(hint),
                    Fail: static (Error _) => Option<IndexHint>.None),
            };
        public Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
            Query<TGeometry, TOut>? query,
            params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
            Analyze.Run(
                query: query,
                runtime: Context.ToOption(),
                input: input);
    }
    private static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        Option<GeometryContext> runtime,
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
        Option<GeometryContext> runtime) where TGeometry : notnull {
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
                    Some: (GeometryContext context) => query.Apply(geometry: geometry).Run(context),
                    None: () => Fin.Fail<Seq<TOut>>(query.Key.MissingContext())));
    }
}
