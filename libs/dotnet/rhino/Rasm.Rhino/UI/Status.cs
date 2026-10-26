using Rasm.Rhino.Events;
using Rasm.UI;
using Rhino;
using Rhino.Runtime.Notifications;
using Rhino.UI;
using Riok.Mapperly.Abstractions;
using NativeNotificationCenter = Rhino.Runtime.Notifications.NotificationCenter;

namespace Rasm.Rhino.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Notice(string Title, string Description, Option<string> Message, Notification.Severity Severity, Option<GlyphSource> Glyph, Option<View.Panel> Reveal);

public sealed record NoticeReplies(Option<string> Confirm, Option<string> Alternate, Option<string> Cancel, Func<ButtonType, IO<Unit>> Clicked);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class WorkProgress<T>(Atom<Content<T>> work, long total) : IProgress<int> where T : notnull {
    public void Report(int value) => _ = work.Swap(held => held.Requested(Some(Progress.Create(value, total))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class NoticeMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(Notice.Severity), nameof(Notification.SeverityLevel))]
    [MapperIgnoreSource(nameof(Notice.Glyph), Justification = "Notifications draws glyph on system banner")]
    [MapperIgnoreSource(nameof(Notice.Reveal), Justification = "Notifications opens panel from system banner")]
    internal static partial void Update(Notice notice, Notification raised);
}

public static class Status {
    // --- [PACING]
    public static IO<T> Paced<T>(RhinoDoc doc, string label, int total, Func<IProgress<int>, IO<T>> work, IPlugInSink sink) where T : notnull =>
        DisposalOps.Within<IDisposable, T>(retain =>
            from reading in atomIO<Content<T>>(new Content<T>.Pending(None, None))
            from forked in DisposalOps.Fork(work(new WorkProgress<T>(reading, total)), retain)
            from metered in Metered(doc, label, total, reading, sink, retain).OnUiThread()
            from attached in EventKind.EscapeKeyPressed.Inline(_ => forked.Cancel, sink, retain).OnUiThread()
            from value in forked.Await
            select value, static held => DisposalOps.Release(held).OnUiThread());

    private static IO<Unit> Metered<T>(RhinoDoc doc, string label, int total, Atom<Content<T>> reading, IPlugInSink sink, Action<IDisposable> retain) where T : notnull =>
        from answer in DisposalOps.Acquire(
            IO.lift(() => StatusBar.ShowProgressMeter(doc.RuntimeSerialNumber, 0, total, RowText.Localize(label, sink).Local, embedLabel: true, showPercentComplete: true)),
            answer => when(answer is 1, IO.lift(() => StatusBar.HideProgressMeter(doc.RuntimeSerialNumber))).As(), retain)
        from moved in answer switch {
            1 => Subscriptions.Changed(reading, nameof(reading.Change)).Inline(work => Moved(work, doc.RuntimeSerialNumber).OnUiThread(), sink, retain),
            -1 => IO.pure(unit),
            _ => IO.fail<Unit>(new Refused(nameof(StatusBar.ShowProgressMeter), Refused.Declined)),
        }
        select moved;

    private static IO<Unit> Moved<T>(Content<T> work, uint serial) where T : notnull =>
        IO.lift(() => work.Switch(
            serial,
            pending: static (held, pending) => pending.Work.Iter(progress => StatusBar.UpdateProgressMeter(held, progress.Positions.Value, absolute: true)),
            ready: static (_, _) => unit,
            empty: static (_, _) => unit,
            failed: static (_, _) => unit));

    // --- [METER]
    extension(Progress progress) {
        internal (int Value, int MaxValue) Positions => progress.Total <= int.MaxValue
            ? ((int)progress.Done, (int)progress.Total)
            : ((int)((decimal)progress.Done * int.MaxValue / progress.Total), int.MaxValue);
    }

    // --- [NOTICES]
    public static IO<Disposal<Notification>> Show(Notice notice, Option<NoticeReplies> replies, IPlugInSink sink, Action<Disposal<Notification>> retain) =>
        DisposalOps.Acquire(IO.lift(() => Notification.ExecuteAssemblyProtectedCode(() => Added(notice, replies, sink))), held => Notification.ExecuteAssemblyProtectedCode(() => {
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
        raised.ButtonClicked = button => Callbacks.Started(IO.pure(button).Bind(reply.Clicked), new CallbackSite(sink, typeof(Notification), nameof(Notification.ButtonClicked)));
        _ = reply.Cancel.Iter(title => raised.CancelButtonTitle = title);
    }
}
