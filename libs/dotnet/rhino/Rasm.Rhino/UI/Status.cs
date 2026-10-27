using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.PlugIns;
using Rasm.UI;
using Rasm.UI.Components;
using Rhino;
using Rhino.Runtime.Notifications;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

namespace Rasm.RhinoCommon.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Notice(string Title, string Description, Option<string> Message, Notification.Severity Severity, Option<GlyphSource> Glyph, Option<View.Panel> Reveal);

public sealed record NoticeReplies(Option<string> Confirm, Option<string> Alternate, Option<string> Cancel, Func<ButtonType, IO<Unit>> Clicked);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class WorkProgress<T>(Atom<Content<T>> work, long total) : IProgress<int> where T : notnull {
    public void Report(int value) => _ = work.Swap(held => held.Requested(Progress.Validated(value, total).ToOption()));
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
    public static IO<T> Paced<T>(RhinoDoc doc, string label, int total, Func<IProgress<int>, IO<T>> work, IPlugInLifetime owner) where T : notnull =>
        (from reading in atomIO<Content<T>>(new Content<T>.Pending())
         from forked in use(work(new WorkProgress<T>(reading, total)).Fork(), static fork => fork.Cancel)
         from metered in Metered(doc, label, total, reading, owner).OnUiThread()
         from attached in EventKind.EscapeKeyPressed.Inline(_ => forked.Cancel, owner).OnUiThread()
         from value in forked.Await
         select value).Bracket();

    private static IO<Unit> Metered<T>(RhinoDoc doc, string label, int total, Atom<Content<T>> reading, IPlugInLifetime owner) where T : notnull =>
        from answer in IO.lift(() => StatusBar.ShowProgressMeter(doc.RuntimeSerialNumber, 0, total, RowText.Localize(label, owner).Local, embedLabel: true, showPercentComplete: true))
        from moved in answer switch {
            1 => Products.Released(() => Eto.Forms.Application.Instance.Invoke(() => StatusBar.HideProgressMeter(doc.RuntimeSerialNumber)))
                >> Subscriptions.Changed(reading).Inline(work => Moved(work, doc.RuntimeSerialNumber).OnUiThread(), owner),
            _ => IO.lift(() => RhinoFault.Unless(answer is -1)),
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
    public static IO<Notification> Show(Notice notice, Option<NoticeReplies> replies, IPlugInLifetime owner) =>
        use(
            IO.lift(() => Notification.ExecuteAssemblyProtectedCode(() => Raised(notice, replies, owner))),
            static raised => Notification.ExecuteAssemblyProtectedCode(() => Rhino.Runtime.Notifications.NotificationCenter.Notifications.Remove(raised)));

    public static IO<Unit> Revise(Notification raised, Notice notice) =>
        IO.lift(() => Notification.ExecuteAssemblyProtectedCode(() => NoticeMapper.Update(notice, raised)));

    private static Notification Raised(Notice notice, Option<NoticeReplies> replies, IPlugInLifetime owner) {
        Notification raised = new([typeof(Status).Assembly]);
        NoticeMapper.Update(notice, raised);
        _ = replies.Iter(reply => {
            (raised.ConfirmButtonTitle, raised.AlternateButtonTitle) = (Conversions.Unset(reply.Confirm), Conversions.Unset(reply.Alternate));
            raised.ButtonClicked = button => owner.Start(reply.Clicked(button));
            _ = reply.Cancel.Iter(title => raised.CancelButtonTitle = title);
        });
        Rhino.Runtime.Notifications.NotificationCenter.Notifications.Add(raised);
        return raised;
    }
}
