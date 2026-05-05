using System.Diagnostics;
using Core.Domain;
using LanguageExt.Common;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Core.Composition;

// --- [COMPOSITION] -----------------------------------------------------------------------------

public static class Registration {
    private static readonly ActivitySource Source = new(name: "Rasm.Core");

    public static IServiceCollection AddCore(this IServiceCollection services) =>
        services
            .AddSingleton(Source)
            .AddSingleton<OperationFactory>(static (IServiceProvider _) =>
                new CoreFactory())
            .Decorate<OperationFactory>(static (
                    OperationFactory inner,
                    IServiceProvider provider) =>
                new ObservedFactory(
                    inner: inner,
                    source: provider.GetRequiredService<ActivitySource>()));

    private sealed class CoreFactory : OperationFactory {
        public override Fin<Operation<TIn, TOut>> Create<TIn, TOut>(
            Func<TIn, Fin<Seq<TOut>>> operation) =>
            Operation.Create(operation: operation);
    }

    private sealed class ObservedFactory(
        OperationFactory inner,
        ActivitySource source) : OperationFactory {
        public override Fin<Operation<TIn, TOut>> Create<TIn, TOut>(
            Func<TIn, Fin<Seq<TOut>>> operation) =>
            inner.Create(operation: operation)
                .Map((Operation<TIn, TOut> candidate) =>
                    (Operation<TIn, TOut>)new Observed<TIn, TOut>(
                        inner: candidate,
                        source: source));
    }

    private sealed class Observed<TIn, TOut>(
        Operation<TIn, TOut> inner,
        ActivitySource source) : Operation<TIn, TOut> where TIn : notnull {
        private const string ActivityName = "rasm.operation";

        internal override Fin<Seq<TOut>> Apply(TIn input) =>
            inner.Apply(input: input);

        public override Validation<Error, Seq<TOut>> Execute(
            Operation.Mode executionMode,
            params ReadOnlySpan<TIn> input) =>
            source.HasListeners() switch {
                false => inner.Execute(
                    executionMode: executionMode,
                    input: input),
                true => Trace(
                    executionMode: executionMode,
                    input: input),
            };

        private Validation<Error, Seq<TOut>> Trace(
            Operation.Mode executionMode,
            ReadOnlySpan<TIn> input) {
            using Activity? activity = source.StartActivity(name: ActivityName);
            Validation<Error, Seq<TOut>> result = inner.Execute(
                executionMode: executionMode,
                input: input);
            Fin<Seq<TOut>> final = result.ToFin();
            _ = activity?.SetTag(key: "rasm.operation", value: ActivityName);
            _ = activity?.SetTag(key: "rasm.input.type", value: typeof(TIn).FullName);
            _ = activity?.SetTag(key: "rasm.output.type", value: typeof(TOut).FullName);
            _ = activity?.SetTag(key: "rasm.input.count", value: input.Length);
            _ = activity?.SetTag(key: "rasm.execution", value: executionMode.ToString());
            _ = activity?.SetTag(key: "rasm.success", value: final.IsSucc);
            _ = result.Match(
                Succ: static (Seq<TOut> _) => unit,
                Fail: (Error error) => {
                    _ = activity?.SetTag(key: "rasm.error.code", value: error.Code);
                    _ = activity?.SetTag(key: "rasm.error.message", value: error.Message);
                    _ = activity?.SetStatus(code: ActivityStatusCode.Error, description: error.Message);
                    return unit;
                });
            return result;
        }
    }
}
