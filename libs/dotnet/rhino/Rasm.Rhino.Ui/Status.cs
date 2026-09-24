using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.Runtime.Notifications;
using Rhino.UI;

namespace Rasm.Rhino.Ui;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StatusOp {
    public sealed record PromptMessage(LocalizeStringPair Text) : StatusOp;

    public sealed record MessagePane(Option<LocalizeStringPair> Text) : StatusOp;

    public sealed record DistancePane(double Value, LengthUnit Units) : StatusOp;

    public sealed record NumberPane(double Value) : StatusOp;

    public sealed record PointPane(Point3d Point) : StatusOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ToastPlacement {
    public sealed record Default() : ToastPlacement;

    public sealed record Scaled(int TextHeight) : ToastPlacement;

    public sealed record Located(int TextHeight, PointF Location) : ToastPlacement;
}

public sealed record ProgressMeterOptions(Limits<int> Range, LocalizeStringPair Label, bool EmbedLabel, bool ShowPercentComplete, bool WaitCursor, bool CancelOnEscape) {
    internal Fin<(int Lower, int Upper)> Bounds =>
        from lower in Range.Lower.ToFin(new Invalid(nameof(Range)))
        from upper in Range.Upper.ToFin(new Invalid(nameof(Range)))
        from inclusive in Range.Inclusive(nameof(Range))
        select (lower.Value, upper.Value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProgressMeterOwner {
    public sealed record Owned() : ProgressMeterOwner;

    public sealed record Foreign() : ProgressMeterOwner;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProgressMeterOp {
    public sealed record Absolute(int Position, Option<LocalizeStringPair> Label) : ProgressMeterOp;

    public sealed record Relative(int Delta, Option<LocalizeStringPair> Label) : ProgressMeterOp;

    public sealed record Relabel(LocalizeStringPair Label) : ProgressMeterOp;
}

public sealed record NotificationSpec(
    LocalizeStringPair Title,
    LocalizeStringPair Body,
    Option<LocalizeStringPair> Description,
    Notification.Severity Severity,
    Option<LocalizeStringPair> ConfirmButtonTitle,
    Option<LocalizeStringPair> CancelButtonTitle,
    Option<LocalizeStringPair> AlternateButtonTitle,
    Map<string, string> Metadata,
    Seq<Assembly> AllowedAssemblies);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NotificationEvent {
    public sealed record ButtonClicked(ButtonType Button) : NotificationEvent;

    public sealed record PropertyChanged(Option<string> Property) : NotificationEvent;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ProgressMeter {
    internal ProgressMeter(uint serial, ProgressMeterOwner owner, ProgressMeterOptions options, (int Lower, int Upper) bounds, CancellationToken cancel) =>
        (Serial, Owner, Options, Bounds, Cancel) = (serial, owner, options, bounds, cancel);

    public uint Serial { get; }

    public ProgressMeterOwner Owner { get; }

    public ProgressMeterOptions Options { get; }

    public (int Lower, int Upper) Bounds { get; }

    public CancellationToken Cancel { get; }

    public IProgress<double> Fraction(Action<Error> reject) =>
        new FractionProgress(this, reject);

    public IO<Option<int>> Update(ProgressMeterOp op) =>
        Owner.Map(
            owned: Applied(op).Map(static previous => Some(previous)),
            foreign: IO.pure(Option<int>.None));

    private IO<int> Applied(ProgressMeterOp op) =>
        op.Switch(
            this,
            absolute: static (meter, absolute) =>
                from position in IO.lift(() => meter.Options.Range.Check(absolute.Position, nameof(ProgressMeterOp.Absolute.Position)))
                from previous in IO.lift(() => meter.Updated(absolute.Label, position, absolute: true))
                select previous,
            relative: static (meter, relative) => IO.lift(() => meter.Updated(relative.Label, relative.Delta, absolute: false)),
            relabel: static (meter, relabel) => IO.lift(() => StatusBar.UpdateProgressMeter(meter.Serial, relabel.Label.Local, RhinoMath.UnsetIntIndex, absolute: true)));

    private int Updated(Option<LocalizeStringPair> label, int position, bool absolute) =>
        label.Match(
            Some: text => StatusBar.UpdateProgressMeter(Serial, text.Local, position, absolute),
            None: () => StatusBar.UpdateProgressMeter(Serial, position, absolute));

    private sealed class FractionProgress(ProgressMeter meter, Action<Error> reject) : IProgress<double> {
        // --- [LIMITS]
        private static readonly Fin<Limits<double>> Fraction = Limits.AtLeast(0.0).AtMost(1.0, nameof(Fraction));

        // --- [PROGRESS]
        public void Report(double value) =>
            _ = Answers.Answer(
                from position in IO.lift(() => Fraction
                    .Bind(limits => limits.Check(value, nameof(value)))
                    .Map(fraction => meter.Bounds.Lower + (int)Math.Round(fraction * (meter.Bounds.Upper - meter.Bounds.Lower), MidpointRounding.ToEven)))
                from previous in meter.Update(new ProgressMeterOp.Absolute(position, Option<LocalizeStringPair>.None))
                select previous,
                reject,
                Option<int>.None);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Status {
    // --- [WRITES]
    public static IO<Unit> Apply(RhinoDoc doc, Seq<StatusOp> ops) =>
        ops.TraverseM(op => Applied(doc, op)).As().Map(static _ => unit);

    public static IO<uint> Toast(RhinoView view, LocalizeStringPair message, ToastPlacement placement) =>
        IO.lift(() => placement.Switch(
            (View: view, Text: message.Local),
            @default: static (state, _) => state.View.ShowToast(state.Text),
            scaled: static (state, scaled) => state.View.ShowToast(state.Text, scaled.TextHeight),
            located: static (state, located) => state.View.ShowToast(state.Text, located.TextHeight, located.Location)));

    private static IO<Unit> Applied(RhinoDoc doc, StatusOp op) =>
        op.Switch(
            doc,
            promptMessage: static (_, message) => IO.lift(() => RhinoApp.SetCommandPromptMessage(message.Text.Local)),
            messagePane: static (_, pane) => IO.lift(() => pane.Text.Match(Some: static text => StatusBar.SetMessagePane(text.Local), None: StatusBar.ClearMessagePane)),
            distancePane: static (document, distance) =>
                from scale in IO.lift(() => DocumentUnits.UnitScale(distance.Units, document.ModelUnits))
                from set in IO.lift(() => StatusBar.SetDistancePane(distance.Value * scale))
                select set,
            numberPane: static (_, number) => IO.lift(() => StatusBar.SetNumberPane(number.Value)),
            pointPane: static (_, point) => IO.lift(() => StatusBar.SetPointPane(point.Point)));

    // --- [SCOPES]
    public static IO<TValue> UseProgressMeter<TValue>(RhinoDoc doc, ProgressMeterOptions options, Action<DeliveryLoss> lost, Func<ProgressMeter, IO<TValue>> body) =>
        from bounds in IO.lift(() => options.Bounds)
        from value in Disposal.Bracketed(
            IO.lift(() => Ownership(StatusBar.ShowProgressMeter(
                doc.RuntimeSerialNumber,
                bounds.Lower,
                bounds.Upper,
                options.Label.Local,
                options.EmbedLabel,
                options.ShowPercentComplete))),
            owner => IO.lift(() => owner.Switch(
                doc.RuntimeSerialNumber,
                owned: static (serial, _) => StatusBar.HideProgressMeter(serial),
                foreign: static (_, _) => { })),
            owner => Disposal.Using(
                () => options.WaitCursor ? new WaitCursor() : Thinktecture.Empty.Disposable(),
                _ => WithCancelOnEscape(options.CancelOnEscape, lost, cancel => body(new ProgressMeter(doc.RuntimeSerialNumber, owner, options, bounds, cancel)))))
        select value;

    public static IO<TValue> UseNotification<TValue>(NotificationSpec spec, Func<NotificationEvent, IO<Unit>> onEvent, Action<Error> reject, Func<Notification, IO<TValue>> body) =>
        Disposal.Bracketed(
            IO.lift(() => Notification.ExecuteAssemblyProtectedCode(() => {
                Notification notification = Configured(spec, onEvent, reject);
                _ = spec.Metadata.Iter((key, value) => notification[key] = value);
                return notification;
            })),
            static notification => IO.lift(() => Notification.ExecuteAssemblyProtectedCode(() => {
                notification.HideModal();
                _ = NotificationCenter.Notifications.Remove(notification);
            })),
            notification => Disposal.Using(
                from changes in Events.Attach<PropertyChangedEventHandler>(
                    h => notification.PropertyChanged += h,
                    h => notification.PropertyChanged -= h,
                    (_, e) => _ = Answers.Answer(onEvent(new NotificationEvent.PropertyChanged(Optional(e.PropertyName))), reject, unit))
                from queued in GeometryOps.OnFailure(
                    IO.lift(() => Notification.ExecuteAssemblyProtectedCode(() => NotificationCenter.Notifications.Add(notification))),
                    IO.lift(changes.Dispose))
                select changes,
                _ => body(notification)));

    private static IO<TValue> WithCancelOnEscape<TValue>(bool cancelOnEscape, Action<DeliveryLoss> lost, Func<CancellationToken, IO<TValue>> body) =>
        Disposal.Using(static () => new CancellationTokenSource(), source =>
            Disposal.Using(
                cancelOnEscape
                    ? Deliveries.Observe(new EventScope.Any(), Seq(EventKind.EscapeKeyPressed), new Delivery<DocEvent>.Inline(_ => IO.lift(source.Cancel)), lost)
                    : IO.pure(Thinktecture.Empty.Disposable()),
                _ => body(cancelOnEscape ? source.Token : CancellationToken.None)));

    private static Fin<ProgressMeterOwner> Ownership(int answer) =>
        answer switch {
            1 => new ProgressMeterOwner.Owned(),
            -1 => new ProgressMeterOwner.Foreign(),
            _ => new Refused(nameof(StatusBar.ShowProgressMeter)),
        };

    private static Notification Configured(NotificationSpec spec, Func<NotificationEvent, IO<Unit>> onEvent, Action<Error> reject) =>
        new(spec.AllowedAssemblies.IsEmpty ? Seq<Assembly>() : spec.AllowedAssemblies.Add(typeof(Status).Assembly)) {
            Title = spec.Title.Local,
            Message = spec.Body.Local,
            SeverityLevel = spec.Severity,
            Description = spec.Description.Map(static text => text.Local).ValueUnsafe(),
            ConfirmButtonTitle = spec.ConfirmButtonTitle.Map(static text => text.Local).ValueUnsafe(),
            CancelButtonTitle = spec.CancelButtonTitle.Map(static text => text.Local).ValueUnsafe(),
            AlternateButtonTitle = spec.AlternateButtonTitle.Map(static text => text.Local).ValueUnsafe(),
            ButtonClicked = button => _ = Answers.Answer(onEvent(new NotificationEvent.ButtonClicked(button)), reject, unit),
        };
}
