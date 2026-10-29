using System.Globalization;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.Runtime.Notifications;
using Rhino.UI;

namespace Rasm.Rhino.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Progress {
    public long Done { get; }
    public long Total { get; }

    public double Fraction => (double)Done / Total;

    public (int Value, int MaxValue) Positions =>
        Total <= int.MaxValue ? ((int)Done, (int)Total) : ((int)((decimal)Done * int.MaxValue / Total), int.MaxValue);

    public static Option<Progress> Of(long done, long total) => Conversions.Present(total > 0 && done >= 0 && done <= total, new Progress(done, total));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Loading<T> where T : notnull {
    public Option<T> Held => Switch(pending: static _ => Option<T>.None, ready: static ready => Some(ready.Value), failed: static _ => Option<T>.None);

    public sealed record Pending(Option<Progress> Work) : Loading<T> {
        public Option<string> Stage { get; init; }
    }

    public sealed record Ready(T Value) : Loading<T>;

    public sealed record Failed(Error Error) : Loading<T>;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class WorkProgress<T>(Atom<Loading<T>> work, long total) : IProgress<int>, IProgress<(int Done, string Stage)> where T : notnull {
    public void Report(int value) => _ = work.Swap(_ => new Loading<T>.Pending(Progress.Of(value, total)));

    public void Report((int Done, string Stage) value) => _ = work.Swap(_ => new Loading<T>.Pending(Progress.Of(value.Done, total)) { Stage = Some(value.Stage) });
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Notices {
    // --- [PROGRESS]
    public static IO<T> Paced<T>(RhinoDoc doc, string label, int total, Func<WorkProgress<T>, IO<T>> work) where T : notnull =>
        (from reading in atomIO<Loading<T>>(new Loading<T>.Pending(None))
         from forked in use(work(new WorkProgress<T>(reading, total)).Fork(), static fork => fork.Cancel)
         from metered in Metered(doc, label, total, reading).OnUiThread()
         from taskbar in Subscriptions.Changed(reading).Attach(Taskbar).OnUiThread()
         from escaped in Subscriptions.On<EventHandler, EventArgs>(static h => RhinoApp.EscapeKeyPressed += h, static h => RhinoApp.EscapeKeyPressed -= h, static handler => handler.Invoke).Attach(_ => forked.Cancel).OnUiThread()
         from value in forked.Await
         select value).Bracket();

    private static IO<Unit> Metered<T>(RhinoDoc doc, string label, int total, Atom<Loading<T>> reading) where T : notnull =>
        from answer in IO.lift(() => StatusBar.ShowProgressMeter(doc.RuntimeSerialNumber, 0, total, LocalText.Of(label).Local, embedLabel: true, showPercentComplete: true))
        from shown in IO.lift(() => RhinoFault.Unless(answer is 1, RhinoReason.Declined, answer))
        from hidden in use(IO.pure(doc.RuntimeSerialNumber), static serial => IO.lift(() => StatusBar.HideProgressMeter(serial)).OnUiThread().Map(unit))
        from moved in Subscriptions.Changed(reading).Attach(work => Moved(work, doc.RuntimeSerialNumber).OnUiThread())
        select unit;

    private static IO<Unit> Moved<T>(Loading<T> work, uint serial) where T : notnull =>
        IO.lift(() => work.Switch(
            serial,
            pending: static (held, pending) => pending.Work.Iter(progress => _ = pending.Stage.Match(
                Some: stage => StatusBar.UpdateProgressMeter(held, LocalText.Of(stage).Local, progress.Positions.Value, absolute: true),
                None: () => StatusBar.UpdateProgressMeter(held, progress.Positions.Value, absolute: true))),
            ready: static (_, _) => unit,
            failed: static (_, _) => unit));

    public static IO<Unit> Taskbar<T>(Loading<T> work) where T : notnull =>
        work.Switch(
            pending: static pending => pending.Work.Match(
                Some: static progress => (State: Eto.Forms.TaskbarProgressState.Progress, Progress: (float)progress.Fraction),
                None: static () => (State: Eto.Forms.TaskbarProgressState.Indeterminate, Progress: 0f)),
            ready: static _ => (State: Eto.Forms.TaskbarProgressState.None, Progress: 0f),
            failed: static _ => (State: Eto.Forms.TaskbarProgressState.Error, Progress: 0f)) switch {
                var shown => IO.lift(() => Eto.Forms.Taskbar.SetProgress(shown.State, shown.Progress)).OnUiThread(),
            };

    // --- [PANES]
    public static IO<Unit> Message(string text) =>
        use(IO.lift(() => StatusBar.SetMessagePane(LocalText.Of(text).Local)), static _ => IO.lift(StatusBar.ClearMessagePane)).Map(unit);

    public static IO<Unit> Distance(RhinoDoc doc, Length distance) =>
        IO.lift(() => StatusBar.SetDistancePane(Quantities.As(distance, doc.ModelUnits)));

    public static IO<Unit> Badge(string label) =>
        use(IO.lift(() => { Eto.Forms.Application.Instance.BadgeLabel = label; }), static _ => IO.lift(static () => { Eto.Forms.Application.Instance.BadgeLabel = ""; }).OnUiThread()).OnUiThread().Map(unit);

    // --- [TOASTS]
    public static IO<uint> Toast(RhinoDoc doc, string message, Option<int> textHeight = default) =>
        from view in Products.Of(() => doc.Views.ActiveView)
        from serial in IO.lift(() => textHeight.Match(
            Some: height => view.ShowToast(LocalText.Of(message).Local, height),
            None: () => view.ShowToast(LocalText.Of(message).Local)))
        select serial;

    // --- [NOTICES]
    public static IO<Notification> Post(Action<Notification> write, Option<IconResource> icon = default, Option<View.Panel> reveal = default) =>
        from raised in use(
            IO.lift(() => Notification.ExecuteAssemblyProtectedCode(() => {
                Notification created = new([typeof(Notices).Assembly]);
                write(created);
                global::Rhino.Runtime.Notifications.NotificationCenter.Notifications.Add(created);
                return created;
            })),
            static held => IO.lift(() => Notification.ExecuteAssemblyProtectedCode(() => global::Rhino.Runtime.Notifications.NotificationCenter.Notifications.Remove(held))).Map(unit))
        from banner in IO.lift(static () => Eto.Forms.Application.Instance.IsActive).Bind(active => unless(active, Banner(raised, icon, reveal)).As())
        select raised;

    public static IO<Unit> Revise(Notification raised, Action<Notification> write) =>
        IO.lift(() => Notification.ExecuteAssemblyProtectedCode(() => write(raised)));

    private static IO<Unit> Banner(Notification raised, Option<IconResource> icon, Option<View.Panel> reveal) =>
        icon.Traverse(static art => Icons.Frames(art, IconSlot.Banner)).As().Bind(image => use(static () => new Eto.Forms.Notification()).Bind(banner => IO.lift(() => {
            (banner.Title, banner.Message, banner.UserData) = (raised.Title, raised.Description, Conversions.Unset(reveal.Map(static panel => panel.Identity.GUID.ToString("D", CultureInfo.InvariantCulture))));
            banner.ContentImage = image.ValueUnsafe();
            banner.Show();
        })).Bracket());

    internal static IO<Unit> Revealing() =>
        Subscriptions.On<Eto.Forms.NotificationEventArgs>(static h => Eto.Forms.Application.Instance.NotificationActivated += h, static h => Eto.Forms.Application.Instance.NotificationActivated -= h)
            .Attach(static args => IO.lift(() => Conversions.Present(Guid.TryParse(args.UserData, CultureInfo.InvariantCulture, out Guid id), id).Bind(ViewCatalog.Current.Find<View.Panel>))
                .Bind(static found => found.TraverseM(HostPanels.Open).As()).Map(static _ => unit));

    // --- [TRAY]
    public static IO<Unit> Tray(Command<Invoke>.Menu menu, RowScope scope) =>
        from tray in use(static () => new Eto.Forms.TrayIndicator())
        from themed in menu.Face.Icon.Traverse(icon => Icons.Themed(icon, IconSlot.Small, frames => tray.Image = frames)).As()
        from popup in CommandRows.Menu(menu.Groups, scope)
        from assigned in IO.lift(() => tray.Menu = popup.Menu)
        from activated in menu.Primary.Traverse(primary => CommandRows.Press(primary, scope).Bind(press =>
            Subscriptions.On<EventArgs>(h => tray.Activated += h, h => tray.Activated -= h).Attach(_ => press))).As()
        from shown in IO.lift(tray.Show)
        from titled in IO.lift(() => tray.Title = LocalText.Of(menu.Face.Caption).Local)
        select unit;
}
