using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;
namespace Analysis;

// --- [SURFACE] ---------------------------------------------------------------------------------

public static class Analyze {
    public static readonly Eff<Context, Context> Asks =
        Eff.runtime<Context>().As();
    public static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        Run(
            query: query,
            runtime: Option<Context>.None,
            input: input);
    public static Scope From(RhinoDoc? doc) =>
        new(context: Context.FromDocument(doc: doc).ToFin());
    public static Scope In(UnitSystem units) =>
        new(context: Context.CreateDefault(units: units).ToFin());
    public static Scope In(
        double absolute,
        double relative,
        double angle,
        UnitSystem units) =>
        new(
            context: Context.FromKnownUnits(
                    absoluteTolerance: absolute,
                    relativeTolerance: relative,
                    angleToleranceRadians: angle,
                    units: units)
                .ToFin());
    public static Scope In(Context context) =>
        new(context: Optional(context).ToFin(Query.ScopeKey.MissingContext()));
    public sealed record Scope {
        public Fin<Context> Context { get; }
        internal Scope(Fin<Context> context) =>
            Context = context;
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
        Option<Context> runtime,
        ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        query switch {
            Query<TGeometry, TOut> candidate => new Program<TGeometry, TOut>(
                    query: candidate,
                    runtime: runtime)
                .Execute(input: input),
            _ => Fin.Fail<Seq<TOut>>(OpFault.MissingOperation()).ToValidation(),
        };
    private sealed class Program<TGeometry, TOut>(
        Query<TGeometry, TOut> query,
        Option<Context> runtime) where TGeometry : notnull {
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
                    Some: (Context context) => query.Apply(geometry: geometry).Run(context),
                    None: () => Fin.Fail<Seq<TOut>>(query.Key.MissingContext())));
    }
}
