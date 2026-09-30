using System.Drawing;
using System.Reflection;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.Runtime.Notifications;
using Rhino.UI;

namespace Rasm.Rhino.Ui;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ToastPlacement {
    public sealed record Default() : ToastPlacement;

    public sealed record Scaled(int TextHeight) : ToastPlacement;

    public sealed record Located(int TextHeight, PointF Location) : ToastPlacement;
}

[ComplexValueObject]
[ValidationError<ValidationFailure>]
public sealed partial class ProgressMeterOptions {
    public int Lower { get; }

    public int Upper { get; }

    public LocalizeStringPair Label { get; }

    public bool EmbedLabel { get; }

    public bool ShowPercentComplete { get; }

    public bool WaitCursor { get; }

    public bool CancelOnEscape { get; }

    public static Fin<ProgressMeterOptions> From(int lower, int upper, LocalizeStringPair label, bool embedLabel, bool showPercentComplete, bool waitCursor, bool cancelOnEscape) =>
        Validate(lower, upper, label, embedLabel, showPercentComplete, waitCursor, cancelOnEscape, out ProgressMeterOptions? item) is { } error ? error : item!;

    static partial void ValidateFactoryArguments(
        ref ValidationFailure? validationError,
        ref int lower,
        ref int upper,
        ref LocalizeStringPair label,
        ref bool embedLabel,
        ref bool showPercentComplete,
        ref bool waitCursor,
        ref bool cancelOnEscape) =>
        validationError = Limits.AtLeast(lower).Violated(upper, nameof(Upper));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProgressMeterOwner {
    public sealed record Owned() : ProgressMeterOwner;

    public sealed record Foreign() : ProgressMeterOwner;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ProgressMeter {
    internal ProgressMeter(uint serial, ProgressMeterOwner owner, ProgressMeterOptions options, CancellationToken cancel) =>
        (Serial, Owner, Options, Cancel) = (serial, owner, options, cancel);

    public uint Serial { get; }

    public ProgressMeterOwner Owner { get; }

    public ProgressMeterOptions Options { get; }

    public CancellationToken Cancel { get; }

    public IProgress<double> Fraction(Action<Error> reject) =>
        new FractionProgress(this, reject);

    public IO<Option<int>> Update(int position, bool absolute, Option<LocalizeStringPair> label) =>
        IO.lift(() => Owner.Switch(
            (Serial, Position: position, Absolute: absolute, Label: label),
            owned: static (state, _) => Some(state.Label.Match(
                Some: text => StatusBar.UpdateProgressMeter(state.Serial, text.Local, state.Position, state.Absolute),
                None: () => StatusBar.UpdateProgressMeter(state.Serial, state.Position, state.Absolute))),
            foreign: static (_, _) => Option<int>.None));

    private sealed class FractionProgress(ProgressMeter meter, Action<Error> reject) : IProgress<double> {
        public void Report(double value) =>
            _ = Answers.Answer(
                meter.Update(
                    meter.Options.Lower + (int)Math.Round(value * (meter.Options.Upper - meter.Options.Lower), MidpointRounding.ToEven),
                    absolute: true,
                    None),
                reject,
                Option<int>.None);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Status {
    // --- [WRITES]
    public static IO<Unit> DistancePane(RhinoDoc doc, double value, LengthUnit units) =>
        from scale in IO.lift(() =>
            Invalid.Unless(!LengthUnit.IsUnset(units) && !LengthUnit.IsNone(units) && !LengthUnit.IsUnset(doc.ModelUnits) && !LengthUnit.IsNone(doc.ModelUnits), nameof(LengthUnit.Scale))
                .Map(_ => LengthUnit.Scale(units, doc.ModelUnits)))
        from set in IO.lift(() => StatusBar.SetDistancePane(value * scale))
        select set;

    public static IO<uint> Toast(RhinoView view, LocalizeStringPair message, ToastPlacement placement) =>
        IO.lift(() => placement.Switch(
            (View: view, Text: message.Local),
            @default: static (state, _) => state.View.ShowToast(state.Text),
            scaled: static (state, scaled) => state.View.ShowToast(state.Text, scaled.TextHeight),
            located: static (state, located) => state.View.ShowToast(state.Text, located.TextHeight, located.Location)));

    // --- [SCOPES]
    public static IO<TValue> UseProgressMeter<TValue>(RhinoDoc doc, ProgressMeterOptions options, Func<ProgressMeter, IO<TValue>> body) =>
        IO.lift(() => Ownership(StatusBar.ShowProgressMeter(
                doc.RuntimeSerialNumber,
                options.Lower,
                options.Upper,
                options.Label.Local,
                options.EmbedLabel,
                options.ShowPercentComplete)))
            .Bracket(
                Use: owner => DisposalOps.Using(
                    static () => new CancellationTokenSource(),
                    source => DisposalOps.Using(
                        DisposalOps.AcquireAll(
                            Seq(
                                    Answers.Found(options.WaitCursor, IO.lift(static IDisposable () => new WaitCursor())),
                                    Answers.Found(
                                        options.CancelOnEscape,
                                        Events.Attach<EventHandler>(static h => RhinoApp.EscapeKeyPressed += h, static h => RhinoApp.EscapeKeyPressed -= h, (_, _) => source.Cancel())))
                                .Somes()),
                        _ => body(new ProgressMeter(doc.RuntimeSerialNumber, owner, options, source.Token)))),
                Fin: owner => IO.lift(() => owner.Switch(
                    doc.RuntimeSerialNumber,
                    owned: static (serial, _) => StatusBar.HideProgressMeter(serial),
                    foreign: static (_, _) => { })));

    public static IO<TValue> UseNotification<TValue>(Seq<Assembly> allowedAssemblies, Action<Notification> configure, Func<Notification, IO<TValue>> body) =>
        IO.lift(() => Notification.ExecuteAssemblyProtectedCode(() => {
            Notification notification = new(allowedAssemblies.IsEmpty ? allowedAssemblies : allowedAssemblies.Add(typeof(Status).Assembly));
            configure(notification);
            NotificationCenter.Notifications.Add(notification);
            return notification;
        })).Bracket(
            Use: body,
            Fin: static notification => IO.lift(() => Notification.ExecuteAssemblyProtectedCode(() => {
                notification.HideModal();
                _ = NotificationCenter.Notifications.Remove(notification);
            })));

    private static Fin<ProgressMeterOwner> Ownership(int answer) =>
        answer switch {
            1 => new ProgressMeterOwner.Owned(),
            -1 => new ProgressMeterOwner.Foreign(),
            _ => new Refused(nameof(StatusBar.ShowProgressMeter)),
        };
}
