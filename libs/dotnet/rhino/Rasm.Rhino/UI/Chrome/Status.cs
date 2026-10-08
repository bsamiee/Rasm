using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.Runtime.Notifications;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.UI.Chrome;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct WorkCount {
    public int Done { get; }
    public int Total { get; }

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int done, ref int total) =>
        validationError = total >= 1 && done >= 0 && done <= total ? null : new InvalidRhinoValue();
}

[Union]
public abstract partial record WorkState {
    public sealed record Inactive : WorkState;

    public sealed record Indeterminate : WorkState;

    public sealed record Counted(WorkCount Work) : WorkState;
}

public sealed record Notice(string Title, string Description, Option<string> Message, Notification.Severity Severity, Option<IGlyph> Glyph, Option<View.Panel> Reveal);

public sealed record NoticeReplies(Option<string> Confirm, Option<string> Alternate, Option<string> Cancel, Func<ButtonType, IO<Unit>> Clicked);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class WorkProgress(Atom<WorkState> work, int total, CallbackSite site) : IProgress<int>, IDisposable {
    public void Report(int value) =>
        _ = Callbacks.Answer(
            IO.lift(() => (WorkCount.Validate(value, total, out WorkCount count) is { } error ? error : (Fin<WorkCount>)count).Map(counted => ignore(work.Swap(_ => counted)))),
            static () => unit,
            site);

    public void Dispose() => _ = work.Swap(static _ => new WorkState.Inactive());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class NoticeMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(Notice.Severity), nameof(Notification.SeverityLevel))]
    [MapperIgnoreSource(nameof(Notice.Glyph), Justification = "Presence draws it on the system banner")]
    [MapperIgnoreSource(nameof(Notice.Reveal), Justification = "Presence opens it from the system banner")]
    internal static partial void Update(Notice notice, Notification raised);
}

public static class Status {
    // --- [PACING]
    public static IO<A> Paced<A>(RhinoDoc doc, string label, int total, Func<IProgress<int>, IO<A>> work, IPlugInSink sink) =>
        from reading in IO.lift(static () => Atom<WorkState>(new WorkState.Indeterminate()))
        let progress = new WorkProgress(reading, total, new CallbackSite(sink, typeof(WorkProgress), nameof(WorkProgress.Report)))
        from forked in work(progress).Fork()
        from value in DisposalOps.AcquireAll(
                Seq(
                    IO.pure<IDisposable>(new Disposal<ForkIO<A>>(forked, static held => held.Cancel.Run())),
                    IO.pure<IDisposable>(progress),
                    Metered(doc, label, total, reading, sink),
                    EventKind.EscapeKeyPressed.Inline(_ => forked.Cancel, sink)),
                DisposalOps.Release)
            .Post()
            .Bracket(Use: _ => forked.Await, Fin: static held => DisposalOps.Release(held).Post())
        select value;

    private static IO<IDisposable> Metered(RhinoDoc doc, string label, int total, Atom<WorkState> reading, IPlugInSink sink) =>
        from window in IO.lift(() => Missing.Unless(RhinoEtoApp.MainWindowForDocument(doc), nameof(RhinoEtoApp.MainWindowForDocument)))
        from acquire in IO.lift(() => Owned(
            StatusBar.ShowProgressMeter(doc.RuntimeSerialNumber, 0, total, RowText.Localize(label, table: Some<object>(sink)).Local, embedLabel: true, showPercentComplete: true),
            Seq(
                IO.pure<IDisposable>(new Disposal<uint>(doc.RuntimeSerialNumber, static serial => StatusBar.HideProgressMeter(serial))),
                FrameClocks.Sample(window, Moved(reading, doc.RuntimeSerialNumber), new CallbackSite(sink, typeof(StatusBar), nameof(StatusBar.UpdateProgressMeter))))))
        from held in DisposalOps.AcquireAll(acquire, DisposalOps.Release)
        select DisposalOps.Composite(held, new CallbackSite(sink, typeof(StatusBar), nameof(StatusBar.HideProgressMeter)));

    private static Fin<Seq<IO<IDisposable>>> Owned(int answer, Seq<IO<IDisposable>> acquire) =>
        answer switch {
            1 => acquire,
            -1 => Seq<IO<IDisposable>>(),
            _ => new Refused(nameof(StatusBar.ShowProgressMeter)),
        };

    private static IO<Unit> Moved(Atom<WorkState> reading, uint serial) =>
        IO.lift(() => reading.Value.Switch(
            serial,
            inactive: static (_, _) => unit,
            indeterminate: static (_, _) => unit,
            counted: static (held, counted) => ignore(StatusBar.UpdateProgressMeter(held, counted.Work.Done, absolute: true))));

    // --- [NOTICES]
    public static IO<Disposal<Notification>> Raise(Notice notice, Option<NoticeReplies> replies, IPlugInSink sink) =>
        IO.lift(() => Notification.ExecuteAssemblyProtectedCode(() => Added(notice, replies, sink)));

    public static IO<Unit> Revise(Disposal<Notification> held, Notice notice) =>
        IO.lift(() => held.Held.Iter(raised => Notification.ExecuteAssemblyProtectedCode(() => NoticeMapper.Update(notice, raised))));

    private static Disposal<Notification> Added(Notice notice, Option<NoticeReplies> replies, IPlugInSink sink) {
        Notification raised = new([typeof(Status).Assembly]);
        NoticeMapper.Update(notice, raised);
        replies.Iter(reply => Replied(raised, reply, sink));
        NotificationCenter.Notifications.Add(raised);
        return new Disposal<Notification>(raised, static held => _ = NotificationCenter.Notifications.Remove(held));
    }

    private static void Replied(Notification raised, NoticeReplies reply, IPlugInSink sink) {
        (raised.ConfirmButtonTitle, raised.AlternateButtonTitle) = (Conversions.Unset(reply.Confirm), Conversions.Unset(reply.Alternate));
        raised.ButtonClicked = button => _ = Callbacks.Answer(button, reply.Clicked, static () => unit, new CallbackSite(sink, typeof(Notification), nameof(Notification.ButtonClicked)));
        reply.Cancel.Iter(title => raised.CancelButtonTitle = title);
    }
}
