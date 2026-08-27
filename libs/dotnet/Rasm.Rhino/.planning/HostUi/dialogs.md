# [RASM_RHINO_HOSTUI_DIALOGS]

`Inquiries` owns the interrogations only `Rhino.UI` can present, and `HostAssets` owns the productions only `RhinoDoc` and `Rhino.UI.DrawingUtilities` can make. Every prompt the toolkit also presents — a message, a text edit, a bounded number, a colour, a face, a file or folder — is the kernel `PickerSpec` family and reaches the operator through this page's document frame rather than through a second request vocabulary. Every asset origin, extent, filter, and raster product is the kernel `Interaction/asset` family; what stays here is the SVG rasterizer with its own polarity and ground, the document-bound mesh preview, the linetype stroke geometry, and the named-colour roster.

## [01]-[INDEX]

- [02]-[INQUIRIES]: `Inquiry`, `InquiryAnswer`, and the two `Inquiries.Ask` arities own the document dialog frame — the Rhino-only interrogations under their own fold, and the kernel picker demands under the same session grant.
- [03]-[HOST_ASSETS]: `HostAsset`, `HostProduct`, and `HostAssets.Render` own the four productions the kernel asset family cannot make.

## [02]-[INQUIRIES]

- Owner: `Inquiry` is the request vocabulary for the twelve interrogations `Rhino.UI` publishes and the toolkit does not; `InquiryAnswer` detaches every host result into an admitted value; `InquiryRow` owns the keyed choice, check, and property payloads; `ChoiceMultiplicity` owns single-versus-many admission and result projection; `VerdictPolicy` owns the message posture whose button roster the toolkit cannot present.
- Entry: `Ask` under two arities the REQUEST discriminates — a local `Inquiry` folds here, a kernel `PickerDemand<TResult>` presents through the same frame — because both need the one thing this boundary alone supplies: the `SessionNeed.Dialog` grant and the document's own main window as the anchor.
- Auto: the caption-to-row projection is ONE ordered admission, so the duplicate-key refusal, the duplicate-caption refusal, and the host's answer-by-caption lookup are one construction instead of two `Distinct` counts and a linear rescan per arm.
- Auto: the layer, linetype, and multi-layer asks read their host tables through ONE roster gate — a live count, an admitted ordinal set, and an optional by-layer sentinel — so no arm re-derives the range comparison the gate already made.
- Law: the message prompt seats at the kernel and only its UNPRESENTABLE roster stays. `PickerSpec.Ask` carries the toolkit button sets; Rhino's abort/retry/ignore and retry/cancel rosters have no `MessageBoxButtons` member, which is the carve the kernel prompt owner states, so `Inquiry.Verdict` exists for those two rosters alone and its traits, delivery, and modality are the KERNEL vocabularies projected onto the host flag word at the arm. NAMED LOSS: none — every other message posture is one `PickerSpec.Ask` value.
- Law: `Inquiry.Shade` stays because the Rhino colour dialog is a different host surface, not a narrower one: it presents a `NamedColorList` palette and raises a live per-change callback, and the toolkit dialog `PickerSpec.Shade` presents through publishes neither. A preview refusal accumulates on a BOUNDED ring and rides beside the accepted colour, because a transient preview throw never invalidates the colour the operator then accepted; a dismissal answers `UiFault.Dismissed` and carries nothing.
- Law: the context menu is a PROJECTION of the kernel node tree, never a second authoring vocabulary. `MenuForge.Flatten` answers the flat roster `Dialogs.ShowContextMenu` consumes — the text, the `MenuMode` ordinal array, and the verb each slot names — and `MenuForge.Choose` resolves the returned ordinal back to its `IntentKey`, so no caller re-derives the mapping from its own copy of the roster it passed in and a header or divider ordinal refuses typed.
- Law: the print-width ask composes the folder's own plot-weight owner. `PrintPen` already names the three states a host double smuggles — the application default at `0.0`, the no-plot posture at `-1.0`, and a positive millimetre snapped onto the ISO 128-24 ladder — so this ask declares no width type and the dialog's cancel, which answers an unset double, is read BEFORE the pen ingress rather than surfacing there as an out-of-range width.
- Law: every host cancellation is `UiFault.Dismissed` and every out-of-range result is `InvalidResult`; admission rejects and execution never repairs, so no consumer observes a partial answer.
- Law: the answer names the operator's own axis, never the request's. A layer ask offers the set-current button under a three-corner request row and the operator answers a two-state fact, so `LayerCurrency` is the answer's vocabulary and the request row is not reused as a result.
- Law: a host table ordinal is admitted against a LIVE count at the boundary and nowhere else — no value object can carry an invariant whose bound is read per call — so the roster gate is the one admission and every arm downstream reads an admitted ordinal.
- Boundary: the kernel picker marshals itself, so the demand arity nests two crossings in a stated order — the Rhino command frame outside, the toolkit marshal inside — because the anchor and the document grant are resolved before any toolkit dialog exists.
- Boundary: native `ref`/`out` calls stay statement-shaped inside the terminal fold, and the host verdict is read through the settle gate so the host member itself runs under the operation's catch.
- Output: `InquiryAnswer` for the local family and `Option<TResult>` for a kernel demand, dismissal riding absence there and its own refusal case here — both settled values holding no live host dialog.
- Packages: `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`Dialogs` message, list, check, property, context-menu, layer, linetype, print-width, sun, and colour members; `NamedColorList`; `RhinoEtoApp.MainWindowForDocument`); `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui-controls.md` (`RangeDialog`); `libs/dotnet/Rasm.Rhino/.api/api-eto-forms.md` (`Control` as the anchor type); LanguageExt.Core (`Fin`, `Option`, `Seq`, `guard`, `TraverseM`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum]`, `[ComplexValueObject]`, `[ValueObject]`, `[UseDelegateFromConstructor]`); `Rasm/Interaction` (`PickerDemand`, `UiFault`, `MenuNode`, `MenuForge`, `MenuSlot`, `IntentTable`, `IntentKey`, `AskTrait`, `AskDelivery`, `AskModality`, `AlphaMode`); `Rasm/Domain` (`Op`, `Ring<Error>`, `ICapability`, `CapabilitySet`, `CapabilityLaw`); `Rasm/Numerics` (`PerceptualColor`, `Dimension`); `Rasm.Rhino/Document` (`DocumentSession`, `SessionNeed`, `PrintPen`).
- Growth: a new Rhino-only interrogation is one `Inquiry` case, one answer case, and one arm; a new toolkit prompt is one `PickerSpec` case at the kernel and no edit here; a message roster the toolkit gains moves OUT of `VerdictRoster` and into the kernel policy.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Interaction;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rhino.Runtime;
using Rhino.UI.Controls;
using DrawingPoint = System.Drawing.Point;
using DrawingSize = System.Drawing.Size;
using GdiBitmap = System.Drawing.Bitmap;

namespace Rasm.Rhino.HostUi;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<ShowMessageButton>]
public sealed partial class VerdictRoster {
    public static readonly VerdictRoster AbortRetryIgnore = new(
        key: ShowMessageButton.AbortRetryIgnore,
        capacity: 3,
        answers: static () => CapabilitySet<HostVerdict>.Of(HostVerdict.Abort, HostVerdict.Retry, HostVerdict.Ignore));
    public static readonly VerdictRoster RetryCancel = new(
        key: ShowMessageButton.RetryCancel,
        capacity: 2,
        answers: static () => CapabilitySet<HostVerdict>.Of(HostVerdict.Retry, HostVerdict.Cancel));

    internal int Capacity { get; }

    internal CapabilitySet<HostVerdict> Answers => Admitted();

    [UseDelegateFromConstructor]
    private partial CapabilitySet<HostVerdict> Admitted();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HostVerdict : ICapability<HostVerdict> {
    public static readonly HostVerdict Abort = new(key: "abort", host: ShowMessageResult.Abort);
    public static readonly HostVerdict Retry = new(key: "retry", host: ShowMessageResult.Retry);
    public static readonly HostVerdict Ignore = new(key: "ignore", host: ShowMessageResult.Ignore);
    public static readonly HostVerdict Cancel = new(key: "cancel", host: ShowMessageResult.Cancel);

    internal ShowMessageResult Host { get; }

    internal static Fin<HostVerdict> OfHost(ShowMessageResult host, VerdictRoster roster, Op key) =>
        key.Row<ShowMessageResult, HostVerdict>(candidate: host, column: static row => row.Host)
            .Bind(row => roster.Answers.Admits(row)
                ? Fin.Succ(value: row)
                : Fin.Fail<HostVerdict>(error: key.InvalidResult(detail: row.Key)));
}

[SmartEnum<ShowMessageIcon>]
public sealed partial class MessageIcon {
    public static readonly MessageIcon None = new(key: ShowMessageIcon.None);
    public static readonly MessageIcon Error = new(key: ShowMessageIcon.Error);
    public static readonly MessageIcon Question = new(key: ShowMessageIcon.Question);
    public static readonly MessageIcon Warning = new(key: ShowMessageIcon.Warning);
    public static readonly MessageIcon Information = new(key: ShowMessageIcon.Information);
}

[SmartEnum<ShowMessageDefaultButton>]
public sealed partial class MessageDefault {
    public static readonly MessageDefault First = new(key: ShowMessageDefaultButton.Button1, ordinal: 1);
    public static readonly MessageDefault Second = new(key: ShowMessageDefaultButton.Button2, ordinal: 2);
    public static readonly MessageDefault Third = new(key: ShowMessageDefaultButton.Button3, ordinal: 3);

    internal int Ordinal { get; }
}

[SmartEnum<bool>]
public sealed partial class LayerCreation {
    public static readonly LayerCreation Hidden = new(false);
    public static readonly LayerCreation Available = new(true);
}

[SmartEnum<bool>]
public sealed partial class LinetypeByLayer {
    public static readonly LinetypeByLayer Hidden = new(false, sentinel: static () => Option<int>.None);
    public static readonly LinetypeByLayer Available = new(true, sentinel: static () => Some(-1));

    [UseDelegateFromConstructor]
    internal partial Option<int> Sentinel();
}

[SmartEnum<bool>]
public sealed partial class LayerCurrency {
    public static readonly LayerCurrency Unchanged = new(false);
    public static readonly LayerCurrency MadeCurrent = new(true);
}

[SmartEnum<bool>]
public sealed partial class RangeEdge {
    public static readonly RangeEdge Fixed = new(false);
    public static readonly RangeEdge Adjustable = new(true);
}

[SmartEnum]
public sealed partial class CurrentLayerChoice {
    public static readonly CurrentLayerChoice Hidden = new(show: false, initial: false);
    public static readonly CurrentLayerChoice Offered = new(show: true, initial: false);
    public static readonly CurrentLayerChoice Selected = new(show: true, initial: true);

    internal bool Show { get; }
    internal bool Initial { get; }
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct InquiryKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "Inquiry key is empty.");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerInquiry {
    private LayerInquiry() { }
    public sealed record One(CurrentLayerChoice Current) : LayerInquiry;
    public sealed record Many : LayerInquiry;
    public sealed record Material : LayerInquiry;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinetypeInquiry {
    private LinetypeInquiry() { }
    public sealed record ById(HostText Title, HostText Prompt, Option<Guid> Selected = default) : LinetypeInquiry;
    public sealed record ByIndex(int Selected, LinetypeByLayer ByLayer) : LinetypeInquiry;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InquiryRow {
    private InquiryRow() { }
    public sealed record Choice(InquiryKey Key, HostText Caption, bool Selected) : InquiryRow;
    public sealed record Check(InquiryKey Key, HostText Caption, bool Checked) : InquiryRow;
    public sealed record Property(InquiryKey Key, HostText Caption, string Value) : InquiryRow;

    internal InquiryKey Identity => Switch(
        choice: static row => row.Key,
        check: static row => row.Key,
        property: static row => row.Key);

    internal HostText Label => Switch(
        choice: static row => row.Caption,
        check: static row => row.Caption,
        property: static row => row.Caption);
}

[SmartEnum]
public sealed partial class ChoiceMultiplicity {
    public static readonly ChoiceMultiplicity One = new(pick: SelectScalar);
    public static readonly ChoiceMultiplicity Many = new(pick: SelectSet);

    [UseDelegateFromConstructor]
    internal partial Fin<InquiryAnswer> Pick(
        HostText title, HostText prompt, Seq<(string Caption, InquiryRow.Choice Row)> rows, Op op);

    private static Fin<InquiryAnswer> SelectScalar(
        HostText title, HostText prompt, Seq<(string Caption, InquiryRow.Choice Row)> rows, Op op) =>
        from _ in guard(
                flag: rows.Count(static pair => pair.Row.Selected) <= 1,
                False: op.InvalidInput())
            .ToFin()
        let captions = rows.Map(static pair => pair.Caption).ToArray()
        let seeded = rows.Find(static pair => pair.Row.Selected).Map(static pair => pair.Caption)
        let picked = seeded.Match(
            Some: caption => Dialogs.ShowListBox(
                title: title.Resolve(), message: prompt.Resolve(), items: captions, selectedItem: caption),
            None: () => Dialogs.ShowListBox(
                title: title.Resolve(), message: prompt.Resolve(), items: captions)) as string
        from caption in Optional(picked).ToFin(Fail: new UiFault.Dismissed(Key: op))
        from row in Resolved(rows: rows, caption: caption, op: op)
        select (InquiryAnswer)new InquiryAnswer.Choice(Key: row.Key);

    private static Fin<InquiryAnswer> SelectSet(
        HostText title, HostText prompt, Seq<(string Caption, InquiryRow.Choice Row)> rows, Op op) =>
        from selected in Optional(Dialogs.ShowMultiListBox(
                title: title.Resolve(),
                message: prompt.Resolve(),
                items: rows.Map(static pair => pair.Caption).ToArray(),
                defaults: rows.Filter(static pair => pair.Row.Selected).Map(static pair => pair.Caption).ToArray()))
            .ToFin(Fail: new UiFault.Dismissed(Key: op))
        let chosen = toSeq(selected).Strict()
        from _ in guard(flag: chosen.Distinct().Count == chosen.Count, False: op.InvalidResult())
        from matched in chosen.TraverseM(caption => Resolved(rows: rows, caption: caption, op: op)).As()
        select (InquiryAnswer)new InquiryAnswer.Choices(Keys: matched.Map(static row => row.Key).Strict());

    private static Fin<InquiryRow.Choice> Resolved(
        Seq<(string Caption, InquiryRow.Choice Row)> rows, string caption, Op op) =>
        rows.Find(pair => string.Equals(pair.Caption, caption, StringComparison.Ordinal))
            .Map(static pair => pair.Row)
            .ToFin(Fail: op.InvalidResult(detail: caption));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Inquiry {
    private Inquiry() { }
    public sealed record Verdict(HostText Body, HostText Title, VerdictPolicy Policy) : Inquiry;
    public sealed record Transcript(HostText Body, HostText Title) : Inquiry;
    public sealed record Pick(HostText Title, HostText Prompt, Seq<InquiryRow.Choice> Rows, ChoiceMultiplicity Multiplicity) : Inquiry;
    public sealed record Check(HostText Title, HostText Prompt, Seq<InquiryRow.Check> Rows) : Inquiry;
    public sealed record Properties(HostText Title, HostText Prompt, Seq<InquiryRow.Property> Rows) : Inquiry;
    public sealed record Menu(Seq<MenuNode> Nodes, IntentTable Table, DrawingPoint ScreenPoint) : Inquiry;
    public sealed record Range(RangeInquiry Request) : Inquiry;
    public sealed record Layers(HostText Title, LayerInquiry Scope, Seq<int> Selected, LayerCreation Creation) : Inquiry;
    public sealed record Linetype(LinetypeInquiry Request) : Inquiry;
    public sealed record PrintWidth(HostText Title, HostText Prompt, Option<PrintPen> Selected = default) : Inquiry;
    public sealed record Sun : Inquiry;
    public sealed record Shade(
        PerceptualColor Seed,
        AlphaMode Alpha,
        Option<NamedColorList> Palette = default,
        Option<Func<PerceptualColor, Fin<Unit>>> Preview = default) : Inquiry;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InquiryAnswer {
    private InquiryAnswer() { }
    public sealed record Verdict(HostVerdict Value) : InquiryAnswer;
    public sealed record Transcript : InquiryAnswer;
    public sealed record Choice(InquiryKey Key) : InquiryAnswer;
    public sealed record Choices(Seq<InquiryKey> Keys) : InquiryAnswer;
    public sealed record Checks(Seq<(InquiryKey Key, bool Checked)> Rows) : InquiryAnswer;
    public sealed record Properties(Seq<(InquiryKey Key, string Value)> Rows) : InquiryAnswer;
    public sealed record Menu(IntentKey Verb) : InquiryAnswer;
    public sealed record Range(double Minimum, double Maximum) : InquiryAnswer;
    public sealed record Layer(int Index, LayerCurrency Currency) : InquiryAnswer;
    public sealed record Layers(Seq<int> Indices) : InquiryAnswer;
    public sealed record LayerMaterial(Seq<int> Indices) : InquiryAnswer;
    public sealed record LinetypeId(Guid Value) : InquiryAnswer;
    public sealed record LinetypeIndex(int Value) : InquiryAnswer;
    public sealed record PrintWidth(PrintPen Value) : InquiryAnswer;
    public sealed record SunChanged : InquiryAnswer;
    public sealed record Shade(PerceptualColor Value, Seq<Error> PreviewFaults) : InquiryAnswer;
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class VerdictPolicy {
    public VerdictRoster Roster { get; }
    public MessageIcon Icon { get; }
    public MessageDefault Default { get; }
    public CapabilitySet<AskTrait> Traits { get; }
    public AskDelivery Delivery { get; }
    public AskModality Modality { get; }

    public static CapabilityLaw<AskTrait> Law => CapabilityLaw<AskTrait>.Open;

    internal ShowMessageOptions HostOptions =>
        toSeq(Traits.Held).Fold(HostDelivery, static (all, trait) => all | HostTrait(trait));

    internal ShowMessageMode HostMode => Modality == AskModality.System
        ? ShowMessageMode.SystemModal
        : Modality == AskModality.Task
            ? ShowMessageMode.TaskModal
            : ShowMessageMode.ApplicationModal;

    private ShowMessageOptions HostDelivery => Delivery == AskDelivery.Desktop
        ? ShowMessageOptions.DefaultDesktopOnly
        : Delivery == AskDelivery.Service
            ? ShowMessageOptions.ServiceNotification
            : ShowMessageOptions.None;

    private static ShowMessageOptions HostTrait(AskTrait trait) =>
        trait == AskTrait.Topmost ? ShowMessageOptions.TopMost
        : trait == AskTrait.RightAligned ? ShowMessageOptions.RightAlign
        : trait == AskTrait.RightToLeft ? ShowMessageOptions.RtlReading
        : ShowMessageOptions.SetForeground;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref VerdictRoster roster,
        ref MessageIcon icon,
        ref MessageDefault @default,
        ref CapabilitySet<AskTrait> traits,
        ref AskDelivery delivery,
        ref AskModality modality) =>
        validationError = @default.Ordinal > roster.Capacity
            ? new ValidationError(message: $"Message default button {@default.Ordinal} exceeds the {roster.Key} roster.")
            : null;
}

[ComplexValueObject]
public sealed partial class RangeInquiry {
    public double Minimum { get; }
    public double Maximum { get; }
    public Rasm.Numerics.Dimension Decimals { get; }
    public Rasm.Numerics.Dimension Increment { get; }
    public RangeEdge MinimumEdge { get; }
    public RangeEdge MaximumEdge { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double minimum,
        ref double maximum,
        ref Rasm.Numerics.Dimension decimals,
        ref Rasm.Numerics.Dimension increment,
        ref RangeEdge minimumEdge,
        ref RangeEdge maximumEdge) {
        Seq<string> violated = Seq(
                (Holds: double.IsFinite(minimum) && double.IsFinite(maximum) && minimum <= maximum,
                    Clause: "an ordered finite bound pair"),
                (Holds: increment.Value > 0, Clause: "a positive step"))
            .Filter(static row => !row.Holds)
            .Map(static row => row.Clause)
            .Strict();
        validationError = violated.IsEmpty
            ? null
            : new ValidationError(message: $"Range inquiry requires {string.Join(" and ", violated)}.");
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Inquiries {
    private static readonly Rasm.Numerics.Dimension PreviewCap = Rasm.Numerics.Dimension.Create(value: 64);

    public static Fin<InquiryAnswer> Ask(DocumentSession session, Inquiry request, Op? key = null) {
        Op op = key.OrDefault();
        return op.Accept<object>(session, request).Bind(_ => Framed<InquiryAnswer>(
            session: session,
            body: (frame, parent) => request.Switch(
                (Frame: frame, Parent: parent, Op: op),
                verdict: static (held, ask) =>
                    from body in held.Op.AcceptText(value: ask.Body.Resolve())
                    from settled in HostVerdict.OfHost(
                        host: Dialogs.ShowMessage(
                            parent: held.Parent,
                            message: body,
                            title: ask.Title.Resolve(),
                            buttons: ask.Policy.Roster.Key,
                            icon: ask.Policy.Icon.Key,
                            defaultButton: ask.Policy.Default.Key,
                            options: ask.Policy.HostOptions,
                            mode: ask.Policy.HostMode),
                        roster: ask.Policy.Roster,
                        key: held.Op)
                    select (InquiryAnswer)new InquiryAnswer.Verdict(Value: settled),
                transcript: static (held, ask) => held.Op.Catch(() => {
                    Dialogs.ShowTextDialog(message: ask.Body.Resolve(), title: ask.Title.Resolve());
                    return Fin.Succ<InquiryAnswer>(value: new InquiryAnswer.Transcript());
                }),
                pick: static (held, ask) => Keyed(rows: ask.Rows, op: held.Op).Bind(rows => ask.Multiplicity.Pick(
                    title: ask.Title, prompt: ask.Prompt, rows: rows, op: held.Op)),
                check: static (held, ask) =>
                    from rows in Keyed(rows: ask.Rows, op: held.Op)
                    from states in Optional(Dialogs.ShowCheckListBox(
                            title: ask.Title.Resolve(),
                            message: ask.Prompt.Resolve(),
                            items: rows.Map(static pair => pair.Caption).ToArray(),
                            checkState: rows.Map(static pair => pair.Row.Checked).ToArray()))
                        .ToFin(Fail: new UiFault.Dismissed(Key: held.Op))
                    from answer in Zipped(rows: rows, values: states, op: held.Op)
                    select (InquiryAnswer)new InquiryAnswer.Checks(Rows: answer),
                properties: static (held, ask) =>
                    from rows in Keyed(rows: ask.Rows, op: held.Op)
                    from values in Optional(Dialogs.ShowPropertyListBox(
                            title: ask.Title.Resolve(),
                            message: ask.Prompt.Resolve(),
                            items: rows.Map(static pair => pair.Caption).ToArray(),
                            values: rows.Map(static pair => pair.Row.Value).ToArray()))
                        .ToFin(Fail: new UiFault.Dismissed(Key: held.Op))
                    from answer in Zipped(rows: rows, values: values, op: held.Op)
                    select (InquiryAnswer)new InquiryAnswer.Properties(Rows: answer),
                menu: static (held, ask) =>
                    from slots in MenuForge.Flatten(nodes: ask.Nodes, table: ask.Table, key: held.Op)
                    let index = Dialogs.ShowContextMenu(
                        items: slots.Map(static slot => slot.Text).AsIterable(),
                        screenPoint: ask.ScreenPoint,
                        modes: slots.Map(static slot => slot.Mode.Key).AsIterable())
                    from chosen in MenuForge.Choose(slots: slots, index: index, key: held.Op)
                    from verb in chosen.ToFin(Fail: new UiFault.Dismissed(Key: held.Op))
                    select (InquiryAnswer)new InquiryAnswer.Menu(Verb: verb),
                range: static (held, ask) => held.Op.Catch(() => {
                    using RangeDialog dialog = new(
                        min: ask.Request.Minimum,
                        max: ask.Request.Maximum,
                        decimals: ask.Request.Decimals.Value,
                        increment: ask.Request.Increment.Value,
                        min_range: ask.Request.MinimumEdge.Key,
                        max_range: ask.Request.MaximumEdge.Key);
                    return Settled(
                        probe: () => ShellWindows.Present(
                            dialog: dialog,
                            session: held.Frame.Session,
                            parent: Some<Control>(held.Parent),
                            key: held.Op),
                        op: held.Op,
                        answer: () => Ranged(request: ask.Request, minimum: dialog.Min, maximum: dialog.Max, op: held.Op));
                }),
                layers: static (held, ask) => ask.Scope.Switch(
                    (Request: ask, held.Frame, held.Op),
                    one: static (frame, scope) =>
                        from seeds in Roster(
                            values: frame.Request.Selected,
                            count: frame.Frame.Model.Layers.Count,
                            sentinel: None,
                            failure: frame.Op.InvalidInput())
                        from _ in guard(flag: seeds.Count <= 1, False: frame.Op.InvalidInput())
                        from answer in SelectLayer(
                            request: frame.Request, model: frame.Frame.Model, scope: scope,
                            seed: seeds.Head.IfNone(-1), op: frame.Op)
                        select answer,
                    many: static (frame, _) => Picked(request: frame.Request, model: frame.Frame.Model, op: frame.Op)
                        .Map<InquiryAnswer>(values => new InquiryAnswer.Layers(Indices: values)),
                    material: static (frame, _) => Picked(request: frame.Request, model: frame.Frame.Model, op: frame.Op)
                        .Bind(values => Settled(
                            probe: () => Fin.Succ(value: Dialogs.ShowLayerMaterialDialog(
                                frame.Frame.Model, values.AsIterable())),
                            op: frame.Op,
                            answer: () => Fin.Succ<InquiryAnswer>(value: new InquiryAnswer.LayerMaterial(Indices: values))))),
                linetype: static (held, ask) => ask.Request.Switch(
                    held,
                    byId: static (held, pick) => Dialogs.ShowLineTypes(
                        title: pick.Title.Resolve(),
                        message: pick.Prompt.Resolve(),
                        doc: held.Frame.Model,
                        selectedLineTypeId: pick.Selected.IfNone(Guid.Empty)) is Guid id && id != Guid.Empty
                        ? held.Frame.Model.Linetypes.Find(id: id, ignoreDeletedLinetypes: true) >= 0
                            ? Fin.Succ<InquiryAnswer>(value: new InquiryAnswer.LinetypeId(Value: id))
                            : Fin.Fail<InquiryAnswer>(error: held.Op.InvalidResult())
                        : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op)),
                    byIndex: static (held, pick) => SelectLinetype(
                        pick: pick, count: held.Frame.Model.Linetypes.Count, op: held.Op)),
                printWidth: static (held, ask) => {
                    double width = ask.Selected.Match(
                        Some: pen => Dialogs.ShowPrintWidths(
                            title: ask.Title.Resolve(), message: ask.Prompt.Resolve(), selectedWidth: pen.ToHost()),
                        None: () => Dialogs.ShowPrintWidths(title: ask.Title.Resolve(), message: ask.Prompt.Resolve()));
                    return Settled(
                        probe: () => Fin.Succ(value: RhinoMath.IsValidDouble(width)),
                        op: held.Op,
                        answer: () => PrintPen.OfHost(weight: width, key: held.Op)
                            .Map<InquiryAnswer>(static pen => new InquiryAnswer.PrintWidth(Value: pen)));
                },
                sun: static (held, _) => Settled(
                    probe: () => Fin.Succ(value: Dialogs.ShowSunDialog(sun: held.Frame.Model.Lights.Sun)),
                    op: held.Op,
                    answer: static () => Fin.Succ<InquiryAnswer>(value: new InquiryAnswer.SunChanged())),
                shade: static (held, ask) => Shaded(ask: ask, parent: held.Parent, op: held.Op)),
            op: op));
    }

    public static Fin<Option<TResult>> Ask<TResult>(DocumentSession session, PickerDemand<TResult> demand, Op? key = null) {
        Op op = key.OrDefault();
        return op.Accept<object>(session, demand).Bind(_ => Framed<Option<TResult>>(
            session: session,
            body: (_, parent) => demand.Present(anchor: Some<Control>(parent), key: op),
            op: op));
    }

    private static Fin<T> Framed<T>(
        DocumentSession session,
        Func<(RhinoDoc Model, DocumentSession Session), Control, Fin<T>> body,
        Op op) =>
        HostThread.Run(
            work: new HostWork<T>.Session(
                Document: session,
                Needs: [SessionNeed.Dialog],
                Body: model => Optional(RhinoEtoApp.MainWindowForDocument(model))
                    .ToFin(Fail: op.MissingContext())
                    .Bind(parent => body((Model: model, Session: session), parent))),
            key: op);

    private static Fin<InquiryAnswer> Settled(Func<Fin<bool>> probe, Op op, Func<Fin<InquiryAnswer>> answer) =>
        op.Catch(probe).Bind(accepted => accepted
            ? op.Catch(answer)
            : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: op)));

    private static Fin<Seq<(string Caption, TRow Row)>> Keyed<TRow>(Seq<TRow> rows, Op op) where TRow : InquiryRow =>
        from _ in guard(flag: !rows.IsEmpty, False: op.InvalidInput()).ToFin()
        from keys in rows.TraverseM(row => op.AcceptValidated<InquiryKey>(row.Identity.ToValue())).As()
        from __ in guard(flag: keys.Distinct().Count == keys.Count, False: op.InvalidInput())
        from captions in rows.TraverseM(row => op.AcceptText(value: row.Label.Resolve())).As()
        from ___ in guard(flag: captions.Distinct().Count == captions.Count, False: op.InvalidInput())
        select captions.Zip(rows).Strict();

    private static Fin<Seq<(InquiryKey Key, TValue Value)>> Zipped<TRow, TValue>(
        Seq<(string Caption, TRow Row)> rows, IEnumerable<TValue> values, Op op) where TRow : InquiryRow =>
        toSeq(values).Strict() is var settled && settled.Count == rows.Count
            ? Fin.Succ(value: rows.Map(static pair => pair.Row.Identity).Zip(settled).Strict())
            : Fin.Fail<Seq<(InquiryKey Key, TValue Value)>>(error: op.InvalidResult());

    private static Fin<Seq<int>> Roster(Seq<int> values, int count, Option<int> sentinel, Error failure) =>
        count >= 0
            && values.Distinct().Count == values.Count
            && values.ForAll(index => (index >= 0 && index < count) || sentinel.Exists(allowed => index == allowed))
            ? Fin.Succ(value: values)
            : Fin.Fail<Seq<int>>(error: failure);

    private static Fin<InquiryAnswer> SelectLayer(
        Inquiry.Layers request, RhinoDoc model, LayerInquiry.One scope, int seed, Op op) {
        bool setCurrent = scope.Current.Initial;
        int index = seed;
        bool accepted = Dialogs.ShowSelectLayerDialog(
            layerIndex: ref index,
            dialogTitle: request.Title.Resolve(),
            showNewLayerButton: request.Creation.Key,
            showSetCurrentButton: scope.Current.Show,
            initialSetCurrentState: ref setCurrent);
        (int Index, bool Current) answered = (Index: index, Current: setCurrent);
        return Settled(
            probe: () => Fin.Succ(value: accepted),
            op: op,
            answer: () =>
                from admitted in Roster(
                    values: Seq(answered.Index), count: model.Layers.Count, sentinel: None, failure: op.InvalidResult())
                from currency in op.Row<bool, LayerCurrency>(candidate: answered.Current)
                select (InquiryAnswer)new InquiryAnswer.Layer(Index: admitted.Head, Currency: currency));
    }

    private static Fin<InquiryAnswer> SelectLinetype(LinetypeInquiry.ByIndex pick, int count, Op op) =>
        Roster(values: Seq(pick.Selected), count: count, sentinel: pick.ByLayer.Sentinel(), failure: op.InvalidInput())
            .Bind(seeds => {
                int index = seeds.Head;
                bool accepted = Dialogs.ShowSelectLinetypeDialog(
                    linetypeIndex: ref index, displayByLayer: pick.ByLayer.Key);
                int answered = index;
                return Settled(
                    probe: () => Fin.Succ(value: accepted),
                    op: op,
                    answer: () => Roster(
                            values: Seq(answered), count: count, sentinel: pick.ByLayer.Sentinel(),
                            failure: op.InvalidResult())
                        .Map<InquiryAnswer>(admitted => new InquiryAnswer.LinetypeIndex(Value: admitted.Head)));
            });

    private static Fin<Seq<int>> Picked(Inquiry.Layers request, RhinoDoc model, Op op) =>
        from _ in Roster(values: request.Selected, count: model.Layers.Count, sentinel: None, failure: op.InvalidInput())
        from picked in Dialogs.ShowSelectMultipleLayersDialog(
            defaultLayerIndices: request.Selected.AsIterable(),
            dialogTitle: request.Title.Resolve(),
            showNewLayerButton: request.Creation.Key,
            layerIndices: out int[] indices)
            ? Fin.Succ(value: toSeq(indices).Strict())
            : Fin.Fail<Seq<int>>(error: new UiFault.Dismissed(Key: op))
        from __ in guard(flag: !picked.IsEmpty, False: op.InvalidResult())
        from admitted in Roster(values: picked, count: model.Layers.Count, sentinel: None, failure: op.InvalidResult())
        select admitted;

    private static Fin<InquiryAnswer> Shaded(Inquiry.Shade ask, Control parent, Op op) {
        Ring<Error> previews = new(cap: PreviewCap);
        return ask.Seed.ToColor4f(key: op).Bind(seed => {
            Color4f colour = seed;
            bool accepted = Dialogs.ShowColorDialog(
                parent: parent,
                color: ref colour,
                allowAlpha: ask.Alpha == AlphaMode.Alpha,
                namedColorList: Op.ToHostSlot(ask.Palette),
                colorCallback: Op.ToHostSlot(ask.Preview.Map(preview => new Dialogs.OnColorChangedEvent(
                    live => ignore(PerceptualColor.OfHost(host: live, key: op)
                        .Bind(preview)
                        .IfFail(fault => ignore(previews.Park(item: fault))))))));
            Color4f answered = colour;
            return Settled(
                probe: () => Fin.Succ(value: accepted),
                op: op,
                answer: () => PerceptualColor.OfHost(host: answered, key: op).Map<InquiryAnswer>(
                    value => new InquiryAnswer.Shade(Value: value, PreviewFaults: previews.Parked)));
        });
    }

    private static Fin<InquiryAnswer> Ranged(RangeInquiry request, double minimum, double maximum, Op op) =>
        from _ in guard(
                flag: double.IsFinite(minimum) && double.IsFinite(maximum) && minimum <= maximum
                    && (request.MinimumEdge == RangeEdge.Adjustable || minimum >= request.Minimum)
                    && (request.MaximumEdge == RangeEdge.Adjustable || maximum <= request.Maximum),
                False: op.InvalidResult())
            .ToFin()
        select (InquiryAnswer)new InquiryAnswer.Range(Minimum: minimum, Maximum: maximum);
}
```

## [03]-[HOST_ASSETS]

- Owner: `HostAsset` is the closed request family for the four productions no kernel asset origin can make; `HostProduct` carries their three answer shapes; `PreviewInk` closes the mesh-preview colour cardinality; `PatternPass` carries the linetype pattern pair whose presence selects the host overload; `PixelGround` carries the composite the pixel rasterizer alone publishes; `PreviewChannel` keys the host stroke channel; `PaletteEntry` is one admitted named colour.
- Entry: `HostAssets.Render` admits the whole request, then routes on the document the request declares — only the mesh preview names a `DocumentSession` and demands `SessionNeed.Read`, and every other production runs in a plain command frame.
- Auto: `HostAssets.Polarity` reads the host dark-mode probe once and answers a kernel `ThemeVariant`, so a caller following the host and a caller pinning a variant hand this owner ONE column and no tri-state optional bool exists.
- Law: a resource, file, stream, scale-indexed set, or draw-program origin resolves at the KERNEL and never here — `AssetOrigin.Resolve(extent, stack, key)` already answers the raster in the asked product shape, so the three resource loaders, the scale-down selector, and the two output-typed cases this page carried are the deleted form. NAMED LOSS: the host's reduced-variant picker, which selected a smaller EMBEDDED image rather than resampling; recovered because a multi-variant asset is `AssetOrigin.Raster`, whose selection reads the asked extent as data.
- Law: the SVG production stays because the rasterizer is Rhino's own — it adjusts for dark mode and composites onto a ground the toolkit decoder has no parameter for — so this owner takes the kernel origin family as its INPUT and answers the kernel raster carrier as its OUTPUT, and only the rasterization between them is host work.
- Law: an origin the host rasterizer cannot read refuses TYPED naming the case. The rasterizer takes SVG document TEXT, which is exactly `AssetOrigin.Source`; every other origin case names a byte source the kernel already resolves, so routing one here would be this page decoding what the kernel owns.
- Law: the ground rides its PRESENCE and selects the host member. `PixelsFromSvg` composites onto a ground under a declared coverage carriage and `BitmapFromSvg` publishes neither parameter, so a stated ground is a pixel ask and an absent one a bitmap ask — the same presence law the kernel number prompt states, and it forecloses the corner where a declared ground is silently dropped.
- Law: the mesh preview's colour cardinality is a CASE admitted once at the request. Zero colours derive the document display colour, one broadcasts, and one per mesh pairs; the count comparison happens at admission and never again inside a production that already holds an admitted ink.
- Law: text metrics are not an asset. Shaping and measurement are the kernel paint owner's `GlyphBlock`, whose `Measure` already crosses the toolkit marshal and answers a result, so a caller measures directly and this page opens no second crossing — which is what retired the blocking wait this production once made on a marshalled task.
- Law: the named-colour roster leaves as ADMITTED colour, never host bytes. It is a colour resource and seats with colour, so each entry carries a `PerceptualColor` and no consumer of this page holds a host colour value.
- Law: a raster plane's rows mint through `AssetRaster.OfPixels` alone, so the buffer is proved against the extent and the coverage carriage it declares before any consumer reads past its end.
- Output: `HostProduct` — a kernel raster, immutable stroke runs, or admitted palette entries; every disposable host asset rides inside the kernel raster's own `Lease`, so a bare bitmap crossing this boundary is the deleted shape.
- Packages: `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui.md` (`DrawingUtilities.BitmapFromSvg`/`PixelsFromSvg`/`CreateMeshPreviewImage`/`CreateCurvePreviewGeometry`/`CreateLinetypePreviewGeometryEx`; `NamedColorList.Default`; the stroke-channel semantics); `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-runtime.md` (`HostUtils.RunningInDarkMode`); `libs/dotnet/.api/api-system-drawing-common.md` (the GDI bitmap the raster carrier leases); LanguageExt.Core (`Fin`, `Option`, `Seq`, `Arr`, `TraverseM`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum]`); `Rasm/Domain` (`Lease<T>`); `Rasm/Interaction` (`AssetOrigin`, `AssetExtent`, `AssetRaster`, `AlphaLayout`, `ThemeVariant`, `UiFault`); `Rasm/Numerics` (`PerceptualColor`, `Dimension`, `PositiveMagnitude`); `Rasm.Rhino/Document` (`DocumentSession`, `SessionNeed`).
- Growth: a new host production is one `HostAsset` case, one arm, and one `HostProduct` shape only if no existing shape carries it; a new stroke channel is one `PreviewChannel` row; a new coverage carriage is one kernel `AlphaLayout` row and no edit here.
- Boundary: the host image cache, the `DisplayBitmap` table, and the plug-in icon registry keep their own custody — this owner mints, answers, and never retains.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PreviewChannel {
    public static readonly PreviewChannel Dashes = new(key: 0);
    public static readonly PreviewChannel Shapes = new(key: 1);
    public static readonly PreviewChannel Glyphs = new(key: 2);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PreviewInk {
    private PreviewInk() { }
    public sealed record Document : PreviewInk;
    public sealed record Uniform(PerceptualColor Value) : PreviewInk;
    public sealed record PerMesh(Seq<PerceptualColor> Values) : PreviewInk;

    internal Fin<Unit> Admit(Rasm.Numerics.Dimension meshes, Op op) => Switch(
        (Meshes: meshes, Op: op),
        document: static (_, _) => Fin.Succ(value: unit),
        uniform: static (_, _) => Fin.Succ(value: unit),
        perMesh: static (held, row) => guard(
            flag: row.Values.Count == held.Meshes.Value, False: held.Op.InvalidInput()).ToFin());

    internal Seq<PerceptualColor> Spread(PerceptualColor fallback, Rasm.Numerics.Dimension meshes) => Switch(
        (Fallback: fallback, Meshes: meshes),
        document: static (held, _) => LanguageExt.Seq.generate(held.Meshes.Value, _ => held.Fallback).Strict(),
        uniform: static (held, row) => LanguageExt.Seq.generate(held.Meshes.Value, _ => row.Value).Strict(),
        perMesh: static (_, row) => row.Values);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct PatternPass(PositiveMagnitude Scale, PreviewChannel Channel);

public readonly record struct PixelGround(PerceptualColor Ground, AlphaLayout Layout);

public readonly record struct PaletteEntry(string Name, PerceptualColor Value);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HostAsset {
    private HostAsset() { }
    public sealed record Vector(
        AssetOrigin Origin, AssetExtent Extent, ThemeVariant Polarity, Option<PixelGround> Ground = default) : HostAsset;
    public sealed record MeshPreview(
        DocumentSession Session, Seq<Mesh> Meshes, PreviewInk Ink, AssetExtent Extent) : HostAsset;
    public sealed record Strokes(
        Curve Curve, Linetype Linetype, AssetExtent Extent, Option<PatternPass> Pattern = default) : HostAsset;
    public sealed record Palette(Option<NamedColorList> Source = default) : HostAsset;

    internal Option<DocumentSession> Document => Switch(
        vector: static _ => Option<DocumentSession>.None,
        meshPreview: static request => Some(request.Session),
        strokes: static _ => Option<DocumentSession>.None,
        palette: static _ => Option<DocumentSession>.None);

    internal Fin<HostAsset> Admit(Op op) => Switch(
        state: op,
        vector: static (op, ask) => op.Accept<object>(ask.Origin, ask.Extent, ask.Polarity).Map(static _ => unit),
        meshPreview: static (op, ask) =>
            from _ in op.Accept<object>(ask.Session, ask.Ink, ask.Extent)
            from meshes in op.AcceptValidated<Rasm.Numerics.Dimension>(ask.Meshes.Count)
            from __ in guard(flag: meshes.Value > 0, False: op.InvalidInput())
            from ___ in ask.Ink.Admit(meshes: meshes, op: op)
            select unit,
        strokes: static (op, ask) => op.Accept<object>(ask.Curve, ask.Linetype, ask.Extent).Map(static _ => unit),
        palette: static (_, _) => Fin.Succ(value: unit))
        .Map(_ => this);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HostProduct {
    private HostProduct() { }
    public sealed record Raster(AssetRaster Value) : HostProduct;
    public sealed record Strokes(Seq<Seq<Point2f>> Runs) : HostProduct;
    public sealed record Palette(Seq<PaletteEntry> Entries) : HostProduct;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostAssets {
    public static ThemeVariant Polarity => HostUtils.RunningInDarkMode ? ThemeVariant.Dark : ThemeVariant.Light;

    public static Fin<HostProduct> Render(HostAsset request, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(request)
            .Bind(admitted => admitted.Admit(op))
            .Bind(admitted => admitted.Document.Match(
                Some: session => HostThread.Run(
                    work: new HostWork<HostProduct>.Session(
                        Document: session,
                        Needs: [SessionNeed.Read],
                        Body: model => Produce(model: Some(model), request: admitted, op: op)),
                    key: op),
                None: () => HostThread.Run(
                    work: new HostWork<HostProduct>.Execute(Body: () => Produce(model: None, request: admitted, op: op)),
                    key: op)));
    }

    private static Fin<HostProduct> Produce(Option<RhinoDoc> model, HostAsset request, Op op) =>
        request.Switch(
            (Model: model, Op: op),
            vector: static (held, ask) => Source(origin: ask.Origin, op: held.Op).Bind(text => ask.Ground.Match(
                Some: ground => Pixels(text: text, ask: ask, ground: ground, op: held.Op),
                None: () => Bitmap(text: text, ask: ask, op: held.Op))),
            meshPreview: static (held, ask) =>
                from model in held.Model.ToFin(Fail: held.Op.MissingContext())
                from fallback in held.Op.Catch(() => Fin.Succ(value: model.CreateDefaultAttributes().DrawColor(model)))
                from neutral in PerceptualColor.OfHost(host: fallback, key: held.Op)
                let meshes = Rasm.Numerics.Dimension.Create(value: ask.Meshes.Count)
                from hosted in ask.Ink.Spread(fallback: neutral, meshes: meshes)
                    .TraverseM(ink => ink.ToDrawing(key: held.Op))
                    .As()
                from bitmap in held.Op.Catch(() => Optional(DrawingUtilities.CreateMeshPreviewImage(
                        doc: model,
                        meshes: ask.Meshes,
                        colors: hosted,
                        size: Extent(ask.Extent)))
                    .ToFin(Fail: held.Op.InvalidResult()))
                select (HostProduct)new HostProduct.Raster(Value: new AssetRaster.Gdi(
                    Scale: ask.Extent.Scale, Bitmap: new Lease<GdiBitmap>.Owned(Value: bitmap))),
            strokes: static (held, ask) => held.Op.Catch(() => Optional(ask.Pattern.Match(
                    Some: pattern => DrawingUtilities.CreateLinetypePreviewGeometryEx(
                        ask.Curve,
                        ask.Linetype,
                        ask.Extent.PixelWidth,
                        ask.Extent.PixelHeight,
                        pattern.Scale.Value,
                        pattern.Channel.Key),
                    None: () => DrawingUtilities.CreateCurvePreviewGeometry(
                        curve: ask.Curve,
                        linetype: ask.Linetype,
                        width: ask.Extent.PixelWidth,
                        height: ask.Extent.PixelHeight)))
                .ToFin(Fail: held.Op.InvalidResult())
                .Map(static runs => (HostProduct)new HostProduct.Strokes(
                    Runs: toSeq(runs).Map(static run => toSeq(run).Strict()).Strict()))),
            palette: static (held, ask) => toSeq(ask.Source.IfNone(() => NamedColorList.Default))
                .TraverseM(named => PerceptualColor.OfHost(host: named.Color, key: held.Op)
                    .Map(value => new PaletteEntry(Name: named.Name, Value: value)))
                .As()
                .Map(static entries => (HostProduct)new HostProduct.Palette(Entries: entries.Strict())));

    private static Fin<string> Source(AssetOrigin origin, Op op) => origin.Switch(
        state: op,
        resource: static (op, _) => Refused(nameof(AssetOrigin.Resource), op),
        file: static (op, _) => Refused(nameof(AssetOrigin.File), op),
        stream: static (op, _) => Refused(nameof(AssetOrigin.Stream), op),
        raster: static (op, _) => Refused(nameof(AssetOrigin.Raster), op),
        vector: static (op, _) => Refused(nameof(AssetOrigin.Vector), op),
        source: static (op, row) => op.AcceptText(value: row.Text),
        render: static (op, _) => Refused(nameof(AssetOrigin.Render), op));

    private static Fin<string> Refused(string origin, Op op) =>
        Fin.Fail<string>(error: new UiFault.HostRejected(
            Key: op,
            Detail: $"{nameof(DrawingUtilities.BitmapFromSvg)} reads source text; {origin} resolves at the kernel"));

    private static Fin<HostProduct> Bitmap(string text, HostAsset.Vector ask, Op op) =>
        op.Catch(() => Optional(DrawingUtilities.BitmapFromSvg(
                svg: text,
                width: ask.Extent.PixelWidth,
                height: ask.Extent.PixelHeight,
                adjustForDarkMode: ask.Polarity != ThemeVariant.Light))
            .ToFin(Fail: op.InvalidResult())
            .Map(bitmap => (HostProduct)new HostProduct.Raster(Value: new AssetRaster.Gdi(
                Scale: ask.Extent.Scale, Bitmap: new Lease<GdiBitmap>.Owned(Value: bitmap)))));

    private static Fin<HostProduct> Pixels(string text, HostAsset.Vector ask, PixelGround ground, Op op) =>
        from backdrop in ground.Ground.ToDrawing(key: op)
        from rows in op.Catch(() => Optional(DrawingUtilities.PixelsFromSvg(
                svg: text,
                width: ask.Extent.PixelWidth,
                height: ask.Extent.PixelHeight,
                premultiplyAlpha: ground.Layout == AlphaLayout.Premultiplied,
                backgroundColor: backdrop,
                adjustForDarkMode: ask.Polarity != ThemeVariant.Light))
            .ToFin(Fail: op.InvalidResult()))
        from raster in AssetRaster.OfPixels(
            scale: ask.Extent.Scale, extent: ask.Extent, layout: ground.Layout, rows: toArray(rows), key: op)
        select (HostProduct)new HostProduct.Raster(Value: raster);

    private static DrawingSize Extent(AssetExtent extent) =>
        new(width: extent.PixelWidth, height: extent.PixelHeight);
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
