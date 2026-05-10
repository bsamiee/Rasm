using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;
namespace Rasm.Analysis;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Analyze {
    public static readonly Eff<Context, Context> Asks =
        Eff.runtime<Context>().As();
    public static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        Run(
            query: query,
            scope: Option<Fin<Context>>.None,
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
                scope: Some(Context),
                input: input);
    }
    private static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        Option<Fin<Context>> scope,
        ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        RunMaterialized(query: query, scope: scope, input: input.ToArray());
    private static Validation<Error, Seq<TOut>> RunMaterialized<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        Option<Fin<Context>> scope,
        TGeometry[] input) where TGeometry : notnull =>
        Optional(query)
            .ToFin(OpFault.MissingOperation())
            .Match(
                Succ: resolved => RunQuery(query: resolved, scope: scope, input: input),
                Fail: error => Fin.Fail<Seq<TOut>>(error).ToValidation());
    private static Validation<Error, Seq<TOut>> RunQuery<TGeometry, TOut>(
        Query<TGeometry, TOut> query,
        Option<Fin<Context>> scope,
        TGeometry[] input) where TGeometry : notnull =>
        query.PreflightFault.Match(
            Some: fault => Fin.Fail<Seq<TOut>>(fault).ToValidation(),
            None: () => ResolveContext(query: query, scope: scope).Match(
                Succ: context => new Program<TGeometry, TOut>(query: query, context: context).Execute(input: input),
                Fail: error => Fin.Fail<Seq<TOut>>(error).ToValidation()));
    private static Fin<Context> ResolveContext<TGeometry, TOut>(
        Query<TGeometry, TOut> query,
        Option<Fin<Context>> scope) where TGeometry : notnull =>
        scope.Match(
            Some: provided => provided,
            None: () => query.RequiresContext switch {
                true => Fin.Fail<Context>(query.Key.MissingContext()),
                false => Context.CreateDefault(units: UnitSystem.Millimeters).ToFin(),
            });
    private sealed class Program<TGeometry, TOut>(
        Query<TGeometry, TOut> query,
        Context context) where TGeometry : notnull {
        internal Validation<Error, Seq<TOut>> Execute(TGeometry[] input) =>
            input
                .AsIterable()
                .ToSeq()
                .Traverse(Apply)
                .As()
                .Map(static chunks => chunks.Bind(static chunk => chunk))
                .ToValidation();
        internal Fin<Seq<TOut>> Apply(TGeometry input) =>
            Optional(input)
                .ToFin(ValidationFault.MissingGeometry())
                .Bind(geometry => query.Apply(geometry: geometry).Run(context));
    }
}
