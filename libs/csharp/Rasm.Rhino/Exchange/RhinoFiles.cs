using Rasm.Rhino.Commands;
using Rasm.Rhino.UI;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record IoScheduler {
    private IoScheduler() { }
    public sealed record Sequential : IoScheduler;
    public sealed record Parallel(int? MaxDegree = null) : IoScheduler;

    internal Fin<Seq<TOut>> Filter<TIn, TOut>(IEnumerable<TIn> source, Func<TIn, bool> predicate, Func<TIn, TOut> map) =>
        Switch(
            (Source: source, Predicate: predicate, Map: map),
            sequential: static (ctx, _) => Fin.Succ(toSeq(ctx.Source.Where(ctx.Predicate).Select(ctx.Map))),
            // BOUNDARY ADAPTER — PLINQ AsParallel projects the LanguageExt fold onto the BCL parallel partitioner; toSeq materializes before crossing back.
            parallel: static (ctx, par) => par.MaxDegree switch {
                int max when max <= 0 => Fin.Fail<Seq<TOut>>(error: Op.Of(name: nameof(IoScheduler)).InvalidInput()),
                int max => Fin.Succ(toSeq(ctx.Source.AsParallel().WithDegreeOfParallelism(max).Where(ctx.Predicate).Select(ctx.Map))),
                _ => Fin.Succ(toSeq(ctx.Source.AsParallel().Where(ctx.Predicate).Select(ctx.Map))),
            });
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct FileRuntime {
    internal FileRuntime(Option<RhinoDoc> document, RunMode mode, Option<Context> domain, Option<DocumentEdit> edit, Option<RhinoUi> ui, IoScheduler scheduler) =>
        (Document, Mode, Domain, Edit, Ui, Scheduler) = (document, mode, domain, edit, ui, scheduler);

    internal Option<RhinoDoc> Document { get; }

    internal RunMode Mode { get; }

    internal Option<Context> Domain { get; }

    internal Option<DocumentEdit> Edit { get; }

    internal Option<RhinoUi> Ui { get; }

    internal IoScheduler Scheduler { get; }
}

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class RhinoFiles {
    private readonly Option<RhinoDoc> document;
    private readonly RunMode mode;
    private readonly IoScheduler scheduler;

    private RhinoFiles(Option<RhinoDoc> document, RunMode mode, IoScheduler scheduler) =>
        (this.document, this.mode, this.scheduler) = (document, mode, scheduler);

    public static RhinoFiles Live(RhinoDoc document, RunMode mode = RunMode.Interactive, IoScheduler? scheduler = null) =>
        new(document: Optional(document), mode: mode, scheduler: scheduler ?? new IoScheduler.Parallel());

    public static RhinoFiles Offline(RunMode mode = RunMode.Scripted, IoScheduler? scheduler = null) =>
        new(document: Option<RhinoDoc>.None, mode: mode, scheduler: scheduler ?? new IoScheduler.Parallel());

    public Fin<T> Run<T>(Eff<FileRuntime, T> operation) =>
        from valid in Optional(operation).ToFin(Fail: Op.Of().InvalidInput())
        from runtime in document.Case switch {
            RhinoDoc { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false } active =>
                Context.Of(doc: active).ToFin().Map(domain => new FileRuntime(
                    document: Some(active),
                    mode: mode,
                    domain: Some(domain),
                    edit: Some(new DocumentEdit(document: active, domain: domain)),
                    ui: Some(new RhinoUi(document: active, mode: mode)),
                    scheduler: scheduler)),
            RhinoDoc => Fin.Fail<FileRuntime>(error: Op.Of(name: nameof(RhinoFiles)).MissingContext()),
            _ => Fin.Succ(value: new FileRuntime(
                document: Option<RhinoDoc>.None,
                mode: mode,
                domain: Option<Context>.None,
                edit: Option<DocumentEdit>.None,
                ui: Option<RhinoUi>.None,
                scheduler: scheduler)),
        }
        from result in valid.Run(runtime)
        select result;
}
