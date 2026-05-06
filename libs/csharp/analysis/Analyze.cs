using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Analysis;

// --- [SURFACE] ---------------------------------------------------------------------------------

public static class Analyze {
    public static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        Run(
            query: query,
            context: query switch {
                Query<TGeometry, TOut> candidate => Fin.Fail<GeometryContext>(candidate.Key.MissingContext()),
                _ => Fin.Fail<GeometryContext>(OperationFault.MissingOperation()),
            },
            requiresContext: false,
            input: input);

    public static Scope From(RhinoDoc? doc) =>
        new(context: GeometryContext.FromDocument(doc: doc).ToFin());

    public static Scope In(UnitSystem units) =>
        new(context: GeometryContext.CreateDefault(units: units).ToFin());

    public static Scope In(GeometryContext context) =>
        new(context: Optional(context)
            .ToFin(Query.ScopeKey.MissingContext()));

    public sealed class Scope(Fin<GeometryContext> context) {
        private readonly Fin<GeometryContext> context = context;

        public Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
            Query<TGeometry, TOut>? query,
            params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
            Analyze.Run(
                query: query,
                context: context,
                requiresContext: true,
                input: input);
    }

    private static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(
        Query<TGeometry, TOut>? query,
        Fin<GeometryContext> context,
        bool requiresContext,
        ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        query switch {
            Query<TGeometry, TOut> candidate => new Program<TGeometry, TOut>(
                    query: candidate,
                    context: context,
                    requiresContext: requiresContext)
                .Execute(input: input),
            _ => Fin.Fail<Seq<TOut>>(OperationFault.MissingOperation()).ToValidation(),
        };

    private sealed class Program<TGeometry, TOut>(
        Query<TGeometry, TOut> query,
        Fin<GeometryContext> context,
        bool requiresContext) : Operation<TGeometry, TOut> where TGeometry : notnull {
        private readonly Fin<Unit> setup = (
                query.Ready,
                (requiresContext || query.RequiresContext) switch {
                    true => context.Map(static (GeometryContext _) => unit),
                    false => Fin.Succ(unit),
                })
            .Apply(static (Unit _, Unit _) => unit)
            .As();

        internal override Validation<Error, Seq<TOut>> Execute(
            params ReadOnlySpan<TGeometry> input) =>
            setup.IsSucc switch {
                true => base.Execute(input: input),
                false => setup
                    .Map(static (Unit _) => Seq<TOut>())
                    .ToValidation(),
            };

        internal override Fin<Seq<TOut>> Apply(TGeometry input) =>
            (query.Requirement, input, context) switch {
                (GeometryRequirement requirement, GeometryBase native, Fin<GeometryContext> rail)
                    when requirement != GeometryRequirement.None =>
                    (Fin.Succ((
                            Query: query,
                            Input: input,
                            Native: native,
                            Requirement: requirement)),
                            rail)
                        .Apply(static (
                                (Query<TGeometry, TOut> Query, TGeometry Input, GeometryBase Native, GeometryRequirement Requirement) state,
                                GeometryContext geometryContext) => (
                                state.Query,
                                state.Input,
                                Context: geometryContext,
                                state.Native,
                                state.Requirement))
                        .As()
                        .Bind(static ((Query<TGeometry, TOut> Query, TGeometry Input, GeometryContext Context, GeometryBase Native, GeometryRequirement Requirement) state) =>
                            state.Context.Validate(
                                    geometry: state.Native,
                                    requirement: state.Requirement)
                                .ToFin()
                                .Bind(_ => state.Query.Apply(
                                    geometry: state.Input,
                                    context: Fin.Succ(state.Context)))),
                _ => query.Apply(
                    geometry: input,
                    context: context),
            };
    }
}
