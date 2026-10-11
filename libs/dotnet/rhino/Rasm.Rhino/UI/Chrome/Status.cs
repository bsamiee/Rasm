using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.Runtime.Notifications;
using Rhino.UI;
using Riok.Mapperly.Abstractions;
using NativeNotificationCenter = Rhino.Runtime.Notifications.NotificationCenter;

namespace Rasm.Rhino.UI.Chrome;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record WorkState {
    public sealed record Inactive : WorkState;

    public sealed record Indeterminate : WorkState;

    public sealed record Counted(long Done, long Total) : WorkState {
        public (int Done, int Total) Meter => Total <= int.MaxValue
            ? ((int)Done, (int)Total)
            : ((int)((decimal)Done * int.MaxValue / Total), int.MaxValue);
    }
}

public sealed record Notice(string Title, string Description, Option<string> Message, Notification.Severity Severity, Option<(IGlyph Glyph, int Master)> Glyph, Option<View.Panel> Reveal);

public sealed record NoticeReplies(Option<string> Confirm, Option<string> Alternate, Option<string> Cancel, Func<ButtonType, IO<Unit>> Clicked);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class WorkProgress(Atom<WorkState> work, int total) : IProgress<int>, IDisposable {
    public void Report(int value) => _ = work.Swap(_ => new WorkState.Counted(value, total));

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
    public static IO<T> Paced<T>(RhinoDoc doc, string label, int total, Func<IProgress<int>, IO<T>> work, IPlugInSink sink) =>
        DisposalOps.Within<IDisposable, T>(retain =>
            from reading in IO.lift(static () => Atom<WorkState>(new WorkState.Indeterminate()))
            from progress in DisposalOps.Acquire(IO.lift(() => new WorkProgress(reading, total)), static held => IO.lift(held.Dispose), retain)
            from forked in DisposalOps.Fork(work(progress), retain)
            from metered in Metered(doc, label, total, reading, sink, retain).Post()
            from attached in EventKind.EscapeKeyPressed.Inline(_ => forked.Cancel, sink, retain).Post()
            from value in forked.Await
            select value, static held => DisposalOps.Release(held).Post());

    private static IO<Unit> Metered(RhinoDoc doc, string label, int total, Atom<WorkState> reading, IPlugInSink sink, Action<IDisposable> retain) =>
        from window in IO.lift(() => Missing.Unless(RhinoEtoApp.MainWindowForDocument(doc), nameof(RhinoEtoApp.MainWindowForDocument)))
        from answer in DisposalOps.Acquire(
            IO.lift(() => StatusBar.ShowProgressMeter(doc.RuntimeSerialNumber, 0, total, RowText.Localize(label, table: Some<object>(sink)).Local, embedLabel: true, showPercentComplete: true)),
            answer => when(answer == 1, IO.lift(() => StatusBar.HideProgressMeter(doc.RuntimeSerialNumber))).As(),
            answer => {
                if (answer == 1) retain(new Disposal<uint>(doc.RuntimeSerialNumber, static serial => StatusBar.HideProgressMeter(serial)));
            })
        from sampled in answer switch {
            1 => FrameClocks.Sample(window, Moved(reading, doc.RuntimeSerialNumber), new CallbackSite(sink, typeof(StatusBar), nameof(StatusBar.UpdateProgressMeter)), retain),
            -1 => IO.pure(unit),
            _ => IO.fail<Unit>(new Refused(nameof(StatusBar.ShowProgressMeter))),
        }
        select sampled;

    private static IO<Unit> Moved(Atom<WorkState> reading, uint serial) =>
        IO.lift(() => reading.Value.Switch(
            serial,
            inactive: static (_, _) => unit,
            indeterminate: static (_, _) => unit,
            counted: static (held, counted) => ignore(StatusBar.UpdateProgressMeter(held, counted.Meter.Done, absolute: true))));

    // --- [NOTICES]
    public static IO<Disposal<Notification>> Show(Notice notice, Option<NoticeReplies> replies, IPlugInSink sink, Action<Disposal<Notification>> retain) =>
        DisposalOps.Acquire(
            IO.lift(() => Notification.ExecuteAssemblyProtectedCode(() => Added(notice, replies, sink))),
            static held => IO.lift(held.Dispose),
            held => Notification.ExecuteAssemblyProtectedCode(() => {
                _ = held.Held.Iter(static raised => NativeNotificationCenter.Notifications.Add(raised));
                retain(held);
                return unit;
            }));

    public static IO<Unit> Revise(Disposal<Notification> held, Notice notice) =>
        IO.lift(() => held.Held.Iter(raised => Notification.ExecuteAssemblyProtectedCode(() => NoticeMapper.Update(notice, raised))));

    private static Disposal<Notification> Added(Notice notice, Option<NoticeReplies> replies, IPlugInSink sink) {
        Notification raised = new([typeof(Status).Assembly]);
        NoticeMapper.Update(notice, raised);
        _ = replies.Iter(reply => Replied(raised, reply, sink));
        return new Disposal<Notification>(raised, static held => Notification.ExecuteAssemblyProtectedCode(() => NativeNotificationCenter.Notifications.Remove(held)));
    }

    private static void Replied(Notification raised, NoticeReplies reply, IPlugInSink sink) {
        (raised.ConfirmButtonTitle, raised.AlternateButtonTitle) = (Conversions.Unset(reply.Confirm), Conversions.Unset(reply.Alternate));
        raised.ButtonClicked = button => Callbacks.Run(IO.pure(button).Bind(reply.Clicked), new CallbackSite(sink, typeof(Notification), nameof(Notification.ButtonClicked)));
        _ = reply.Cancel.Iter(title => raised.CancelButtonTitle = title);
    }
}
