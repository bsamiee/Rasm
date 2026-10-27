using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.Runtime.Notifications;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Progress {
    public long Done { get; }
    public long Total { get; }

    public double Fraction => (double)Done / Total;

    public (int Value, int MaxValue) Positions =>
        Total <= int.MaxValue ? ((int)Done, (int)Total) : ((int)((decimal)Done * int.MaxValue / Total), int.MaxValue);

    public static Option<Progress> Of(long done, long total) => Conversions.Found(total > 0 && done >= 0 && done <= total, new Progress(done, total));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Loading<T> where T : notnull {
    public Option<T> Held => Switch(pending: static _ => Option<T>.None, ready: static ready => Some(ready.Value), failed: static _ => Option<T>.None);

    public Loading<T> Requested(Option<Progress> work) => new Pending(work);

    public sealed record Pending(Option<Progress> Work) : Loading<T>;

    public sealed record Ready(T Value) : Loading<T>;

    public sealed record Failed(Error Error) : Loading<T>;
}

public sealed record Notice(string Title, string Description, Option<string> Message, Notification.Severity Severity, Option<IconResource> Icon, Option<View.Panel> Reveal);

public sealed record NoticeReplies(Option<string> Confirm, Option<string> Alternate, Option<string> Cancel, Func<ButtonType, IO<Unit>> Clicked);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class WorkProgress<T>(Atom<Loading<T>> work, long total) : IProgress<int> where T : notnull {
    public void Report(int value) => _ = work.Swap(held => held.Requested(Progress.Of(value, total)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class NoticeMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(Notice.Severity), nameof(Notification.SeverityLevel))]
    [MapperIgnoreSource(nameof(Notice.Icon), Justification = "Notifications draws the icon on the system banner")]
    [MapperIgnoreSource(nameof(Notice.Reveal), Justification = "Notifications opens the panel from the system banner")]
    internal static partial void Update(Notice notice, Notification raised);
}

public static class Status {
    // --- [PACING]
    public static IO<T> Paced<T>(RhinoDoc doc, string label, int total, Func<IProgress<int>, IO<T>> work, IPlugInLifetime owner) where T : notnull =>
        (from reading in atomIO<Loading<T>>(new Loading<T>.Pending(None))
         from forked in use(work(new WorkProgress<T>(reading, total)).Fork(), static fork => fork.Cancel)
         from metered in Metered(doc, label, total, reading, owner).OnUiThread()
         from attached in EventKind.EscapeKeyPressed.Inline(_ => forked.Cancel, owner).OnUiThread()
         from value in forked.Await
         select value).Bracket();

    private static IO<Unit> Metered<T>(RhinoDoc doc, string label, int total, Atom<Loading<T>> reading, IPlugInLifetime owner) where T : notnull =>
        from answer in IO.lift(() => StatusBar.ShowProgressMeter(doc.RuntimeSerialNumber, 0, total, RowText.Localize(label, owner).Local, embedLabel: true, showPercentComplete: true))
        from moved in answer switch {
            1 => use(IO.pure(doc.RuntimeSerialNumber), static serial => IO.lift(() => StatusBar.HideProgressMeter(serial)).OnUiThread().Map(unit))
                .Bind(_ => Subscriptions.Changed(reading).Inline(work => Moved(work, doc.RuntimeSerialNumber).OnUiThread(), owner)),
            _ => IO.lift(() => RhinoFault.Unless(answer is -1)),
        }
        select moved;

    private static IO<Unit> Moved<T>(Loading<T> work, uint serial) where T : notnull =>
        IO.lift(() => work.Switch(
            serial,
            pending: static (held, pending) => pending.Work.Iter(progress => StatusBar.UpdateProgressMeter(held, progress.Positions.Value, absolute: true)),
            ready: static (_, _) => unit,
            failed: static (_, _) => unit));

    // --- [NOTICES]
    public static IO<Notification> Show(Notice notice, Option<NoticeReplies> replies, IPlugInLifetime owner) =>
        use(
            IO.lift(() => Notification.ExecuteAssemblyProtectedCode(() => Raised(notice, replies, owner))),
            static raised => Notification.ExecuteAssemblyProtectedCode(() => global::Rhino.Runtime.Notifications.NotificationCenter.Notifications.Remove(raised)));

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
        global::Rhino.Runtime.Notifications.NotificationCenter.Notifications.Add(raised);
        return raised;
    }
}
