using System.Runtime.InteropServices;
using LanguageExt.Common;
using Rhino.Geometry;

namespace Core.Domain;

// --- [SERVICES] --------------------------------------------------------------------------------

public static class Operation {
    public static Fin<Operation<TIn, TOut>> Create<TIn, TOut>(
        Func<TIn, Fin<Seq<TOut>>>? operation) where TIn : notnull =>
            Optional(operation)
                .ToFin(OperationFault.MissingOperation())
                .Map(static (Func<TIn, Fin<Seq<TOut>>> candidate) =>
                (Operation<TIn, TOut>)new Callable<TIn, TOut>(operation: candidate));

    public static Fin<Operation<TGeometry, TOut>> WithValidation<TGeometry, TOut>(
        this Operation<TGeometry, TOut>? program,
        GeometryContext? context) where TGeometry : GeometryBase =>
        (
            Optional(program).ToFin(OperationFault.MissingOperation()).ToValidation(),
            Optional(context).ToFin(OperationFault.MissingContext()).ToValidation()
        ).Apply(static (
                Operation<TGeometry, TOut> candidate,
                GeometryContext geometryContext) =>
            (Operation<TGeometry, TOut>)new Validated<TGeometry, TOut>(
                program: candidate,
                context: geometryContext))
        .As()
        .ToFin();

    private sealed class Callable<TIn, TOut>(
        Func<TIn, Fin<Seq<TOut>>> operation) : Operation<TIn, TOut> where TIn : notnull {
        internal override Fin<Seq<TOut>> Apply(TIn input) =>
            operation(arg: input);
    }

    private sealed class Validated<TGeometry, TOut>(
        Operation<TGeometry, TOut> program,
        GeometryContext context) : Operation<TGeometry, TOut> where TGeometry : GeometryBase {
        internal override Fin<Seq<TOut>> Apply(TGeometry input) =>
            context.Validate(geometry: input)
                .ToFin()
                .Bind(program.Apply);
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly record struct Mode {
        private const byte FailFastMask = 0;
        private const byte AccumulateMask = 1;

        private Mode(byte mask) =>
            Mask = mask;

        private byte Mask { get; }
        public static Mode FailFast => new(mask: FailFastMask);
        public static Mode Accumulate => new(mask: AccumulateMask);
        internal string Name =>
            Mask switch {
                FailFastMask => nameof(FailFast),
                AccumulateMask => nameof(Accumulate),
                _ => nameof(FailFast),
            };
        internal bool Accumulates =>
            Mask == AccumulateMask;
    }

}

public abstract class Operation<TIn, TOut> where TIn : notnull {
    internal abstract Fin<Seq<TOut>> Apply(TIn input);

    public virtual Validation<Error, Seq<TOut>> Execute(
        Operation.Mode executionMode,
        params ReadOnlySpan<TIn> input) {
        TIn[] inputs = input.ToArray();
        return executionMode.Accumulates switch {
            true => inputs.Aggregate(
                seed: (
                    this,
                    Fin.Succ(LanguageExt.Prelude.Seq<TOut>()).ToValidation()),
                func: static (
                    (Operation<TIn, TOut>, Validation<Error, Seq<TOut>>) current,
                    TIn item) => (
                    current.Item1,
                    (current.Item2, current.Item1.Apply(input: item).ToValidation())
                        .Apply(static (Seq<TOut> previous, Seq<TOut> next) => previous + next)
                        .As())).Item2,
            false => inputs.Aggregate(
                seed: (
                    this,
                    Fin.Succ(LanguageExt.Prelude.Seq<TOut>())),
                func: static (
                    (Operation<TIn, TOut>, Fin<Seq<TOut>>) current,
                    TIn item) => current.Item2.IsSucc switch {
                        true => (
                            current.Item1,
                            (current.Item2, current.Item1.Apply(input: item))
                                .Apply(static (Seq<TOut> previous, Seq<TOut> next) => previous + next)
                                .As()),
                        false => current,
                    }).Item2.ToValidation(),
        };
    }

}

public abstract class OperationFactory {
    public abstract Fin<Operation<TIn, TOut>> Create<TIn, TOut>(
        Func<TIn, Fin<Seq<TOut>>> operation) where TIn : notnull;
}

// --- [ERRORS] ----------------------------------------------------------------------------------

internal static class OperationFault {
    internal static Error MissingOperation() =>
        Error.New(message: "Operation program requires a callable operation.");

    internal static Error MissingContext() =>
        Error.New(message: "Operation program requires a geometry context.");

}
