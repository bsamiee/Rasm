# [RASM_RHINO_HOSTUI_DIALOGS]

`Inquiries` owns host-native interrogation, and `HostAssets` owns dialog resources and previews.

## [01]-[INDEX]

- [02]-[INQUIRIES]: `Inquiry`, `InquiryAnswer`, and `Inquiries.Ask` own native interrogation through one typed request/result fold.
- [03]-[HOST_ASSETS]: `AssetRequest`, `AssetAnswer`, and `HostAssets.Render` own resource loading, text metrics, raster projection, and preview geometry.

## [02]-[INQUIRIES]

- Owner: `Inquiry` is the complete request vocabulary; each case carries the exact host inputs for one result regime, including bounded scalar and adjustable-range entry.
- Owner: `InquiryAnswer` detaches host results into immutable values and preserves only the selected `Font` and `ShowMessageResult` provider identities required by downstream host UI.
- Owner: `InquiryRow` owns keyed choice, check, property, and menu payloads; `ChoiceMultiplicity` owns single-versus-many admission and result projection.
- Entry: `Inquiries.Ask` admits the complete request before `HostWork<T>.Session`, demands `SessionNeed.Dialog`, and resolves the document parent once.
- Law: every host cancellation becomes `UiFault.Dismissed`; invalid rosters, bounds, out-of-range seeds, and result cardinalities fail before a consumer observes a partial answer — admission rejects, execution never repairs.
- Law: `FileInquiry` carries filter text, every selected path crosses as `FileLocation`, and no exchange package enters the Host UI surface.
- Law: `MessagePolicy` is the only foreign message-enum seam, and `MenuMode` keys derive the host context-menu mode array.
- Law: `MessagePolicy` admits only an existing default button and one mutually exclusive delivery target; decorative flags remain an independent frozen set.
- Boundary: native `ref`/`out` calls remain statement-shaped inside the terminal fold; color-preview faults accumulate and replace any nominal dialog result with typed failure.
- Seam: typed Eto dialogs use `ShellWindows.Present`; this owner retains host-native and platform chooser intents only.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using System.Reflection;
using Eto.Forms;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rasm.Rhino.Eto;
using Rhino;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Runtime;
using Rhino.UI;
using Rhino.UI.Controls;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingColor = System.Drawing.Color;
using DrawingIcon = System.Drawing.Icon;
using DrawingImage = System.Drawing.Image;
using DrawingPoint = System.Drawing.Point;
using DrawingSize = System.Drawing.Size;
using RhinoOpenDialog = Rhino.UI.OpenFileDialog;
using RhinoSaveDialog = Rhino.UI.SaveFileDialog;

namespace Rasm.Rhino.HostUi;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class MenuMode {
    public static readonly MenuMode Active = new(key: 0);
    public static readonly MenuMode Muted = new(key: 1);
    public static readonly MenuMode Divider = new(key: 2);
}

[SmartEnum<ShowMessageButton>]
public sealed partial class MessageButtons {
    public static readonly MessageButtons Accept = new(key: ShowMessageButton.OK, capacity: 1);
    public static readonly MessageButtons AcceptCancel = new(key: ShowMessageButton.OKCancel, capacity: 2);
    public static readonly MessageButtons AbortRetryIgnore = new(key: ShowMessageButton.AbortRetryIgnore, capacity: 3);
    public static readonly MessageButtons YesNoCancel = new(key: ShowMessageButton.YesNoCancel, capacity: 3);
    public static readonly MessageButtons YesNo = new(key: ShowMessageButton.YesNo, capacity: 2);
    public static readonly MessageButtons RetryCancel = new(key: ShowMessageButton.RetryCancel, capacity: 2);

    internal int Capacity { get; }
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

[SmartEnum<ShowMessageOptions>]
public sealed partial class MessageOption {
    public static readonly MessageOption Foreground = new(key: ShowMessageOptions.SetForeground);
    public static readonly MessageOption TopMost = new(key: ShowMessageOptions.TopMost);
    public static readonly MessageOption RightAligned = new(key: ShowMessageOptions.RightAlign);
    public static readonly MessageOption RightToLeft = new(key: ShowMessageOptions.RtlReading);
}

[SmartEnum<ShowMessageOptions>]
public sealed partial class MessageDelivery {
    public static readonly MessageDelivery Application = new(key: ShowMessageOptions.None);
    public static readonly MessageDelivery Desktop = new(key: ShowMessageOptions.DefaultDesktopOnly);
    public static readonly MessageDelivery Service = new(key: ShowMessageOptions.ServiceNotification);
}

[SmartEnum<ShowMessageMode>]
public sealed partial class MessageMode {
    public static readonly MessageMode Application = new(key: ShowMessageMode.ApplicationModal);
    public static readonly MessageMode System = new(key: ShowMessageMode.SystemModal);
    public static readonly MessageMode Task = new(key: ShowMessageMode.TaskModal);
}

[SmartEnum]
public sealed partial class CurrentLayerChoice {
    public static readonly CurrentLayerChoice Hidden = new(show: false, initial: false);
    public static readonly CurrentLayerChoice Offered = new(show: true, initial: false);
    public static readonly CurrentLayerChoice Selected = new(show: true, initial: true);

    internal bool Show { get; }
    internal bool Initial { get; }
}

[SmartEnum<bool>]
public sealed partial class LayerCreation {
    public static readonly LayerCreation Hidden = new(false);
    public static readonly LayerCreation Available = new(true);
}

[SmartEnum<bool>]
public sealed partial class LinetypeByLayer {
    public static readonly LinetypeByLayer Hidden = new(false);
    public static readonly LinetypeByLayer Available = new(true);
}

[SmartEnum<bool>]
public sealed partial class EditLayout {
    public static readonly EditLayout SingleLine = new(false);
    public static readonly EditLayout MultipleLines = new(true);
}

[SmartEnum<bool>]
public sealed partial class AlphaChoice {
    public static readonly AlphaChoice Fixed = new(false);
    public static readonly AlphaChoice Editable = new(true);
}

[SmartEnum<bool>]
public sealed partial class FileMultiplicity {
    public static readonly FileMultiplicity One = new(false);
    public static readonly FileMultiplicity Many = new(true);
}

[SmartEnum<bool>]
public sealed partial class PixelAlpha {
    public static readonly PixelAlpha Straight = new(false);
    public static readonly PixelAlpha Premultiplied = new(true);
}

[ValueObject<double>]
public readonly partial struct PrintWidthSeed {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = !double.IsFinite(value) || value < 0d
            ? new ValidationError(message: "Print width is invalid.")
            : null;
}

[ValueObject<string>]
public readonly partial struct FileLocation {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value)
            ? new ValidationError(message: "File location is empty.")
            : null;
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

[ComplexValueObject]
public sealed partial class NumberInquiry {
    public HostText Title { get; }
    public HostText Prompt { get; }
    public double Seed { get; }
    public Option<(double Lower, double Upper)> Bounds { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref HostText title,
        ref HostText prompt,
        ref double seed,
        ref Option<(double Lower, double Upper)> bounds) =>
        validationError = title is null || prompt is null || !double.IsFinite(seed)
            || bounds.Case is (double Lower, double Upper) range
                && (!double.IsFinite(range.Lower) || !double.IsFinite(range.Upper) || range.Lower > range.Upper
                    || seed < range.Lower || seed > range.Upper)
            ? new ValidationError(message: "Number inquiry seed or bounds are invalid.")
            : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FileInquiry {
    private FileInquiry() { }
    public sealed record Save(FileFrame Frame) : FileInquiry;
    public sealed record Open(FileFrame Frame, FileMultiplicity Multiplicity) : FileInquiry;
    public sealed record Folder(HostText Title, Option<FileLocation> Directory = default) : FileInquiry;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FontSeed {
    private FontSeed() { }
    public sealed record Unspecified : FontSeed;
    public sealed record Explicit(global::Eto.Drawing.Font Value) : FontSeed;
    public sealed record System(TypeRole Role, Option<float> Size = default) : FontSeed;

    internal global::Eto.Drawing.Font Resolve() => Switch(
        unspecified: static _ => TypeRole.Body.Resolve(),
        @explicit: static font => font.Value,
        system: static seed => seed.Role.Resolve(size: seed.Size.ToNullable()));
}

[ValueObject<string>]
public readonly partial struct InquiryKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value)
            ? new ValidationError(message: "Inquiry key is empty.")
            : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InquiryRow {
    private InquiryRow() { }
    public sealed record Choice(InquiryKey Key, HostText Caption, bool Selected) : InquiryRow;
    public sealed record Check(InquiryKey Key, HostText Caption, bool Checked) : InquiryRow;
    public sealed record Property(InquiryKey Key, HostText Caption, string Value) : InquiryRow;
    public sealed record Menu(InquiryKey Key, HostText Caption, MenuMode Mode) : InquiryRow;
}

[SmartEnum]
public sealed partial class ChoiceMultiplicity {
    public static readonly ChoiceMultiplicity One = new(pick: SelectScalar);
    public static readonly ChoiceMultiplicity Many = new(pick: SelectSet);

    [UseDelegateFromConstructor]
    internal partial Fin<InquiryAnswer> Pick(HostText title, HostText prompt, Seq<InquiryRow.Choice> rows, Op op);

    private static bool Distinct(Seq<InquiryRow.Choice> rows) =>
        rows.Map(static row => row.Key).Distinct().Count == rows.Count
            && rows.Map(static row => row.Caption.Resolve()).Distinct().Count == rows.Count;

    private static Fin<InquiryAnswer> SelectScalar(HostText title, HostText prompt, Seq<InquiryRow.Choice> rows, Op op) =>
        from _ in guard(
                flag: !rows.IsEmpty && rows.Filter(static row => row.Selected).Count <= 1 && Distinct(rows),
                False: op.InvalidInput())
            .ToFin()
        let selected = rows.Filter(static row => row.Selected).Head
        let picked = selected.Match(
            Some: row => Dialogs.ShowListBox(
                title: title.Resolve(),
                message: prompt.Resolve(),
                items: [.. rows.Map(static row => row.Caption.Resolve())],
                selectedItem: row.Caption.Resolve()),
            None: () => Dialogs.ShowListBox(
                title: title.Resolve(),
                message: prompt.Resolve(),
                items: [.. rows.Map(static row => row.Caption.Resolve())])) as string
        from caption in Optional(picked).ToFin(Fail: new UiFault.Dismissed(Key: op))
        from row in rows.Filter(row => row.Caption.Resolve() == caption).Head.ToFin(Fail: op.InvalidResult())
        select (InquiryAnswer)new InquiryAnswer.Choice(Key: row.Key);

    private static Fin<InquiryAnswer> SelectSet(HostText title, HostText prompt, Seq<InquiryRow.Choice> rows, Op op) =>
        from _ in guard(flag: !rows.IsEmpty && Distinct(rows), False: op.InvalidInput()).ToFin()
        from selected in Optional(Dialogs.ShowMultiListBox(
                title: title.Resolve(),
                message: prompt.Resolve(),
                items: [.. rows.Map(static row => row.Caption.Resolve())],
                defaults: [.. rows.Filter(static row => row.Selected).Map(static row => row.Caption.Resolve())]))
            .ToFin(Fail: new UiFault.Dismissed(Key: op))
        let chosen = toSeq(selected).Strict()
        from _ in guard(flag: chosen.Distinct().Count == chosen.Count, False: op.InvalidResult()).ToFin()
        from matched in chosen.TraverseM(caption => rows
                .Filter(row => row.Caption.Resolve() == caption)
                .Head
                .ToFin(Fail: op.InvalidResult()))
            .As()
        select (InquiryAnswer)new InquiryAnswer.Choices(Keys: matched.Map(static row => row.Key).Strict());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Inquiry {
    private Inquiry() { }
    public sealed record Message(HostText Body, HostText Title, MessagePolicy Policy) : Inquiry;
    public sealed record Transcript(HostText Body, HostText Title) : Inquiry;
    public sealed record Pick(HostText Title, HostText Prompt, Seq<InquiryRow.Choice> Rows, ChoiceMultiplicity Multiplicity) : Inquiry;
    public sealed record Check(HostText Title, HostText Prompt, Seq<InquiryRow.Check> Rows) : Inquiry;
    public sealed record Properties(HostText Title, HostText Prompt, Seq<InquiryRow.Property> Rows) : Inquiry;
    public sealed record Menu(Seq<InquiryRow.Menu> Rows, DrawingPoint ScreenPoint) : Inquiry;
    public sealed record Edit(HostText Title, HostText Prompt, string Seed, EditLayout Layout) : Inquiry;
    public sealed record Number(NumberInquiry Request) : Inquiry;
    public sealed record Range(RangeInquiry Request) : Inquiry;
    public sealed record Layers(HostText Title, LayerInquiry Scope, Seq<int> Selected, LayerCreation Creation) : Inquiry;
    public sealed record Linetype(LinetypeInquiry Request) : Inquiry;
    public sealed record PrintWidth(HostText Title, HostText Prompt, Option<PrintWidthSeed> Selected = default) : Inquiry;
    public sealed record Sun : Inquiry;
    public sealed record Color(ColorInquiry Request) : Inquiry;
    public sealed record Font(FontSeed Seed) : Inquiry;
    public sealed record Files(FileInquiry Request) : Inquiry;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InquiryAnswer {
    private InquiryAnswer() { }
    public sealed record Message(ShowMessageResult Result) : InquiryAnswer;
    public sealed record Transcript : InquiryAnswer;
    public sealed record Choice(InquiryKey Key) : InquiryAnswer;
    public sealed record Choices(Seq<InquiryKey> Keys) : InquiryAnswer;
    public sealed record Checks(Seq<(InquiryKey Key, bool Checked)> Rows) : InquiryAnswer;
    public sealed record Properties(Seq<(InquiryKey Key, string Value)> Rows) : InquiryAnswer;
    public sealed record Menu(InquiryKey Key) : InquiryAnswer;
    public sealed record Edited(string Value) : InquiryAnswer;
    public sealed record Number(double Value) : InquiryAnswer;
    public sealed record Range(double Minimum, double Maximum) : InquiryAnswer;
    public sealed record Layer(int Index, bool SetCurrent) : InquiryAnswer;
    public sealed record Layers(Seq<int> Indices) : InquiryAnswer;
    public sealed record LayerMaterial(Seq<int> Indices) : InquiryAnswer;
    public sealed record LinetypeId(Guid Value) : InquiryAnswer;
    public sealed record LinetypeIndex(int Value) : InquiryAnswer;
    public sealed record PrintWidth(double Value) : InquiryAnswer;
    public sealed record SunChanged : InquiryAnswer;
    public sealed record Color(Color4f Value) : InquiryAnswer;
    public sealed record Font(global::Eto.Drawing.Font Value) : InquiryAnswer;
    public sealed record Paths(Seq<FileLocation> Values) : InquiryAnswer;
}

// --- [MODELS] -------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class MessagePolicy {
    public MessageButtons Buttons { get; }
    public MessageIcon Icon { get; }
    public MessageDefault Default { get; }
    public FrozenSet<MessageOption> Options { get; }
    public MessageDelivery Delivery { get; }
    public MessageMode Mode { get; }

    internal ShowMessageOptions HostOptions => toSeq(Options).Fold(Delivery.Key, static (all, next) => all | next.Key);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref MessageButtons buttons,
        ref MessageIcon icon,
        ref MessageDefault @default,
        ref FrozenSet<MessageOption> options,
        ref MessageDelivery delivery,
        ref MessageMode mode) =>
        validationError = buttons is null || icon is null || @default is null || options is null
            || options.Any(static option => option is null) || delivery is null || mode is null
            || @default.Ordinal > buttons.Capacity
            ? new ValidationError(message: "Message default exceeds the button roster.")
            : null;
}

public readonly record struct ColorInquiry(
    Color4f Initial,
    AlphaChoice Alpha,
    Option<NamedColorList> Palette = default,
    Option<Func<Color4f, Fin<Unit>>> Preview = default);

[SmartEnum<bool>]
public sealed partial class RangeEdge {
    public static readonly RangeEdge Fixed = new(false);
    public static readonly RangeEdge Adjustable = new(true);
}

[ComplexValueObject]
public sealed partial class RangeInquiry {
    public double Minimum { get; }
    public double Maximum { get; }
    public int Decimals { get; }
    public int Increment { get; }
    public RangeEdge MinimumEdge { get; }
    public RangeEdge MaximumEdge { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double minimum,
        ref double maximum,
        ref int decimals,
        ref int increment,
        ref RangeEdge minimumEdge,
        ref RangeEdge maximumEdge) =>
        validationError = !double.IsFinite(minimum) || !double.IsFinite(maximum) || minimum > maximum
            || decimals < 0 || increment <= 0 || minimumEdge is null || maximumEdge is null
            ? new ValidationError(message: "Range inquiry is invalid.")
            : null;
}

[ComplexValueObject]
public sealed partial class FileFrame {
    public HostText Title { get; }
    public string Filter { get; }
    public Option<FileLocation> Seed { get; }
    public Option<FileLocation> Directory { get; }
    public Option<string> Extension { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref HostText title,
        ref string filter,
        ref Option<FileLocation> seed,
        ref Option<FileLocation> directory,
        ref Option<string> extension) =>
        validationError = title is null || string.IsNullOrWhiteSpace(filter)
            || extension.Case is string extensionValue && string.IsNullOrWhiteSpace(extensionValue)
            ? new ValidationError(message: "File inquiry text is empty.")
            : null;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Inquiries {
    public static Fin<InquiryAnswer> Ask(DocumentSession session, Inquiry request, Op? key = null) {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(request);
        Op op = key.OrDefault();
        return Admit(request: request, op: op).Bind(admitted => HostThread.Run(
            work: new HostWork<InquiryAnswer>.Session(
                Document: session,
                Needs: [SessionNeed.Dialog],
                Body: document => Optional(RhinoEtoApp.MainWindowForDocument(document))
                    .ToFin(Fail: op.MissingContext())
                    .Bind(parent => admitted.Switch(
                        (Session: session, Document: document, Parent: parent, Op: op),
                        message: static (held, ask) =>
                            from body in held.Op.AcceptText(value: ask.Body.Resolve())
                            select (InquiryAnswer)new InquiryAnswer.Message(Result: Dialogs.ShowMessage(
                                parent: held.Parent,
                                message: body,
                                title: ask.Title.Resolve(),
                                buttons: ask.Policy.Buttons.Key,
                                icon: ask.Policy.Icon.Key,
                                defaultButton: ask.Policy.Default.Key,
                                options: ask.Policy.HostOptions,
                                mode: ask.Policy.Mode.Key)),
                        transcript: static (held, ask) => held.Op.Catch(() => {
                            Dialogs.ShowTextDialog(message: ask.Body.Resolve(), title: ask.Title.Resolve());
                            return Fin.Succ<InquiryAnswer>(value: new InquiryAnswer.Transcript());
                        }),
                        pick: static (held, ask) => ask.Multiplicity.Pick(
                            title: ask.Title,
                            prompt: ask.Prompt,
                            rows: ask.Rows,
                            op: held.Op),
                        check: static (held, ask) =>
                            from _ in Keyed(keys: ask.Rows.Map(static row => row.Key), op: held.Op)
                            from states in Optional(Dialogs.ShowCheckListBox(
                                    title: ask.Title.Resolve(),
                                    message: ask.Prompt.Resolve(),
                                    items: [.. ask.Rows.Map(static row => row.Caption.Resolve())],
                                    checkState: [.. ask.Rows.Map(static row => row.Checked)]))
                                .ToFin(Fail: new UiFault.Dismissed(Key: held.Op))
                            from answer in Zipped(keys: ask.Rows.Map(static row => row.Key), values: states, op: held.Op)
                            select (InquiryAnswer)new InquiryAnswer.Checks(Rows: answer),
                        properties: static (held, ask) =>
                            from _ in Keyed(keys: ask.Rows.Map(static row => row.Key), op: held.Op)
                            from values in Optional(Dialogs.ShowPropertyListBox(
                                    title: ask.Title.Resolve(),
                                    message: ask.Prompt.Resolve(),
                                    items: [.. ask.Rows.Map(static row => row.Caption.Resolve())],
                                    values: [.. ask.Rows.Map(static row => row.Value)]))
                                .ToFin(Fail: new UiFault.Dismissed(Key: held.Op))
                            from answer in Zipped(keys: ask.Rows.Map(static row => row.Key), values: values, op: held.Op)
                            select (InquiryAnswer)new InquiryAnswer.Properties(Rows: answer),
                        menu: static (held, ask) =>
                            from _ in Keyed(keys: ask.Rows.Map(static row => row.Key), op: held.Op)
                            let index = Dialogs.ShowContextMenu(
                                items: ask.Rows.Map(static row => row.Caption.Resolve()).AsIterable(),
                                screenPoint: ask.ScreenPoint,
                                modes: ask.Rows.Map(static row => row.Mode.Key).AsIterable())
                            from accepted in index switch {
                                < 0 => Fin.Fail<int>(error: new UiFault.Dismissed(Key: held.Op)),
                                var at when at >= ask.Rows.Count => Fin.Fail<int>(error: held.Op.InvalidResult()),
                                var at => Fin.Succ(value: at),
                            }
                            select (InquiryAnswer)new InquiryAnswer.Menu(Key: ask.Rows[accepted].Key),
                        edit: static (held, ask) => Dialogs.ShowEditBox(
                            title: ask.Title.Resolve(),
                            message: ask.Prompt.Resolve(),
                            defaultText: ask.Seed,
                            multiline: ask.Layout.Key,
                            text: out string text)
                            ? Fin.Succ<InquiryAnswer>(value: new InquiryAnswer.Edited(Value: text))
                            : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op)),
                        number: static (held, ask) => {
                            double value = ask.Request.Seed;
                            bool accepted = ask.Request.Bounds.Case is (double Lower, double Upper) bounds
                                ? Dialogs.ShowNumberBox(
                                    title: ask.Request.Title.Resolve(),
                                    message: ask.Request.Prompt.Resolve(),
                                    number: ref value,
                                    minimum: bounds.Lower,
                                    maximum: bounds.Upper)
                                : Dialogs.ShowNumberBox(
                                    title: ask.Request.Title.Resolve(),
                                    message: ask.Request.Prompt.Resolve(),
                                    number: ref value);
                            return Accepted(
                                accepted: accepted,
                                op: held.Op,
                                answer: () => Number(request: ask.Request, value: value, op: held.Op));
                        },
                        range: static (held, ask) => held.Op.Catch(() => {
                            using RangeDialog dialog = new(
                                min: ask.Request.Minimum,
                                max: ask.Request.Maximum,
                                decimals: ask.Request.Decimals,
                                increment: ask.Request.Increment,
                                min_range: ask.Request.MinimumEdge.Key,
                                max_range: ask.Request.MaximumEdge.Key);
                            return ShellWindows.Present(
                                    dialog: dialog,
                                    session: held.Session,
                                    parent: Some<Control>(held.Parent),
                                    key: held.Op)
                                .Bind(accepted => accepted
                                    ? Range(
                                        request: ask.Request,
                                        minimum: dialog.Min,
                                        maximum: dialog.Max,
                                        op: held.Op)
                                    : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op)));
                        }),
                        layers: static (held, ask) => ask.Scope.Switch(
                            (Request: ask, held.Document, held.Op),
                            one: static (frame, scope) => {
                                return from _ in Roster(
                                           values: frame.Request.Selected,
                                           count: frame.Document.Layers.Count,
                                           requireValue: false,
                                           failure: frame.Op.InvalidInput())
                                       from __ in guard(frame.Request.Selected.Count <= 1, frame.Op.InvalidInput()).ToFin()
                                       let seed = frame.Request.Selected.IsEmpty ? -1 : frame.Request.Selected[0]
                                       from answer in SelectLayer(frame, scope, seed)
                                       select answer;
                            },
                            many: static (frame, _) => Picked(request: frame.Request, document: frame.Document, op: frame.Op)
                                .Map<InquiryAnswer>(values => new InquiryAnswer.Layers(Indices: values)),
                            material: static (frame, _) => Picked(request: frame.Request, document: frame.Document, op: frame.Op)
                                .Bind(values => Accepted(
                                    accepted: Dialogs.ShowLayerMaterialDialog(frame.Document, values.AsIterable()),
                                    op: frame.Op,
                                    answer: () => Fin.Succ<InquiryAnswer>(
                                        value: new InquiryAnswer.LayerMaterial(Indices: values))))),
                        linetype: static (held, ask) => ask.Request.Switch(
                            held,
                            byId: static (held, pick) => Dialogs.ShowLineTypes(
                                title: pick.Title.Resolve(),
                                message: pick.Prompt.Resolve(),
                                doc: held.Document,
                                selectedLineTypeId: pick.Selected.IfNone(Guid.Empty)) is Guid id && id != Guid.Empty
                                ? held.Document.Linetypes.Find(id: id, ignoreDeletedLinetypes: true) >= 0
                                    ? Fin.Succ<InquiryAnswer>(value: new InquiryAnswer.LinetypeId(Value: id))
                                    : Fin.Fail<InquiryAnswer>(error: held.Op.InvalidResult())
                                : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op)),
                            byIndex: static (held, pick) => {
                                return Roster(
                                    index: pick.Selected,
                                    count: held.Document.Linetypes.Count,
                                    sentinel: pick.ByLayer.Key ? Some(-1) : None,
                                    failure: held.Op.InvalidInput()).Bind(seed => {
                                        int index = seed;
                                        bool accepted = Dialogs.ShowSelectLinetypeDialog(
                                            linetypeIndex: ref index,
                                            displayByLayer: pick.ByLayer.Key);
                                        return accepted
                                            ? Roster(
                                                index: index,
                                                count: held.Document.Linetypes.Count,
                                                sentinel: pick.ByLayer.Key ? Some(-1) : None,
                                                failure: held.Op.InvalidResult())
                                                .Map<InquiryAnswer>(value => new InquiryAnswer.LinetypeIndex(Value: value))
                                            : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op));
                                    });
                            }),
                        printWidth: static (held, ask) => {
                            double width = ask.Selected.Match(
                                Some: selected => Dialogs.ShowPrintWidths(
                                    title: ask.Title.Resolve(),
                                    message: ask.Prompt.Resolve(),
                                    selectedWidth: selected.ToValue()),
                                None: () => Dialogs.ShowPrintWidths(
                                    title: ask.Title.Resolve(),
                                    message: ask.Prompt.Resolve()));
                            return Accepted(
                                accepted: double.IsFinite(width) && width >= 0d,
                                op: held.Op,
                                answer: () => Fin.Succ<InquiryAnswer>(value: new InquiryAnswer.PrintWidth(Value: width)));
                        },
                        sun: static (held, _) => Dialogs.ShowSunDialog(sun: held.Document.Lights.Sun)
                            ? Fin.Succ<InquiryAnswer>(value: new InquiryAnswer.SunChanged())
                            : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op)),
                        color: static (held, ask) => {
                            Atom<Seq<Error>> previewFaults = Atom(Seq<Error>());
                            Color4f color = ask.Request.Initial;
                            bool accepted = Dialogs.ShowColorDialog(
                                parent: held.Parent,
                                color: ref color,
                                allowAlpha: ask.Request.Alpha.Key,
                                namedColorList: ask.Request.Palette.IfNoneUnsafe((NamedColorList?)null),
                                colorCallback: ask.Request.Preview
                                    .Map(preview => new Dialogs.OnColorChangedEvent(
                                        color => ignore(held.Op.Catch(() => preview(color)).IfFail(fault => {
                                            _ = previewFaults.Swap(rows => rows.Add(fault));
                                            return unit;
                                        }))))
                                    .IfNoneUnsafe((Dialogs.OnColorChangedEvent?)null));
                            Fin<InquiryAnswer> answer = Accepted(
                                accepted: accepted,
                                op: held.Op,
                                answer: () => Fin.Succ<InquiryAnswer>(value: new InquiryAnswer.Color(Value: color)));
                            return previewFaults.Value.Head.Match(
                                Some: first => Fin.Fail<InquiryAnswer>(error: previewFaults.Value.Tail.Fold(first, static (all, next) => all + next)),
                                None: () => answer);
                        },
                        font: static (held, ask) => {
                            using FontDialog dialog = new() { Font = ask.Seed.Resolve() };
                            return Accepted(
                                accepted: dialog.ShowDialog(parent: held.Parent) == DialogResult.Ok,
                                op: held.Op,
                                answer: () => Fin.Succ<InquiryAnswer>(value: new InquiryAnswer.Font(Value: dialog.Font)));
                        },
                        files: static (held, ask) => ask.Request.Switch(
                            held,
                            save: static (held, prompt) => {
                                RhinoSaveDialog dialog = new() {
                                    Title = prompt.Frame.Title.Resolve(),
                                    Filter = prompt.Frame.Filter,
                                    FileName = prompt.Frame.Seed.Map(static value => value.ToValue()).IfNone(string.Empty),
                                    InitialDirectory = prompt.Frame.Directory.Map(static value => value.ToValue()).IfNone(string.Empty),
                                    DefaultExt = prompt.Frame.Extension.IfNone(string.Empty),
                                };
                                return dialog.ShowSaveDialog()
                                    ? Paths(raw: Seq(dialog.FileName), op: held.Op)
                                    : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op));
                            },
                            open: static (held, prompt) => {
                                RhinoOpenDialog dialog = new() {
                                    Title = prompt.Frame.Title.Resolve(),
                                    Filter = prompt.Frame.Filter,
                                    FileName = prompt.Frame.Seed.Map(static value => value.ToValue()).IfNone(string.Empty),
                                    InitialDirectory = prompt.Frame.Directory.Map(static value => value.ToValue()).IfNone(string.Empty),
                                    DefaultExt = prompt.Frame.Extension.IfNone(string.Empty),
                                    MultiSelect = prompt.Multiplicity.Key,
                                };
                                if (!dialog.ShowOpenDialog()) return Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op));
                                Seq<string> paths = prompt.Multiplicity.Key
                                    ? toSeq(dialog.FileNames).Strict()
                                    : Seq(dialog.FileName);
                                return Paths(raw: paths, op: held.Op);
                            },
                            folder: static (held, prompt) => {
                                using SelectFolderDialog dialog = new() {
                                    Title = prompt.Title.Resolve(),
                                    Directory = prompt.Directory.Map(static value => value.ToValue()).IfNone(string.Empty),
                                };
                                return dialog.ShowDialog(parent: held.Parent) == DialogResult.Ok
                                    ? Paths(raw: Seq(dialog.Directory), op: held.Op)
                                    : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op));
                            }))),
            key: op));
    }

    private static Fin<Inquiry> Admit(Inquiry request, Op op) =>
        request.Switch(
                state: op,
                message: static (op, ask) => Present(op, ask.Body, ask.Title, ask.Policy),
                transcript: static (op, ask) => Present(op, ask.Body, ask.Title),
                pick: static (op, ask) =>
                    from _ in Present(op, ask.Title, ask.Prompt, ask.Multiplicity)
                    from __ in Rows(
                        rows: ask.Rows,
                        key: static row => row.Key,
                        caption: static row => row.Caption,
                        valid: static _ => true,
                        op: op)
                    select unit,
                check: static (op, ask) =>
                    from _ in Present(op, ask.Title, ask.Prompt)
                    from __ in Rows(
                        rows: ask.Rows,
                        key: static row => row.Key,
                        caption: static row => row.Caption,
                        valid: static _ => true,
                        op: op)
                    select unit,
                properties: static (op, ask) =>
                    from _ in Present(op, ask.Title, ask.Prompt)
                    from __ in Rows(
                        rows: ask.Rows,
                        key: static row => row.Key,
                        caption: static row => row.Caption,
                        valid: static row => row.Value is not null,
                        op: op)
                    select unit,
                menu: static (op, ask) => Rows(
                    rows: ask.Rows,
                    key: static row => row.Key,
                    caption: static row => row.Caption,
                    valid: static row => row.Mode is not null,
                    op: op),
                edit: static (op, ask) => Present(op, ask.Title, ask.Prompt, ask.Seed, ask.Layout),
                number: static (op, ask) => Present(op, ask.Request),
                range: static (op, ask) => Present(op, ask.Request),
                layers: static (op, ask) =>
                    from _ in Present(op, ask.Title, ask.Scope, ask.Creation)
                    from __ in LayerScope(ask.Scope, op)
                    select unit,
                linetype: static (op, ask) => Linetype(ask.Request, op),
                printWidth: static (op, ask) =>
                    from _ in Present(op, ask.Title, ask.Prompt)
                    from __ in ask.Selected.Match(
                        Some: selected => op.AcceptValidated<PrintWidthSeed>(selected.ToValue()).As(),
                        None: () => Fin.Succ(unit))
                    select unit,
                sun: static (_, _) => Fin.Succ(unit),
                color: static (op, ask) => Present(op, ask.Request.Alpha),
                font: static (op, ask) => Font(ask.Seed, op),
                files: static (op, ask) => Files(ask.Request, op))
            .Map(_ => request);

    private static Fin<Unit> Rows<TRow>(
        Seq<TRow> rows,
        Func<TRow, InquiryKey> key,
        Func<TRow, HostText> caption,
        Func<TRow, bool> valid,
        Op op) where TRow : InquiryRow =>
        from _ in guard(
                flag: !rows.IsEmpty && rows.ForAll(row => row is not null && caption(row) is not null && valid(row)),
                False: op.InvalidInput())
            .ToFin()
        from __ in rows.TraverseM(row => op.AcceptValidated<InquiryKey>(key(row).ToValue())).As()
        select unit;

    private static Fin<Unit> LayerScope(LayerInquiry scope, Op op) =>
        Optional(scope).ToFin(Fail: op.InvalidInput()).Bind(admitted => admitted.Switch(
            state: op,
            one: static (op, row) => Present(op, row.Current),
            many: static (_, _) => Fin.Succ(unit),
            material: static (_, _) => Fin.Succ(unit)));

    private static Fin<Unit> Linetype(LinetypeInquiry request, Op op) =>
        Optional(request).ToFin(Fail: op.InvalidInput()).Bind(admitted => admitted.Switch(
            state: op,
            byId: static (op, row) => Present(op, row.Title, row.Prompt),
            byIndex: static (op, row) => Present(op, row.ByLayer)));

    private static Fin<Unit> Font(FontSeed seed, Op op) =>
        Optional(seed).ToFin(Fail: op.InvalidInput()).Bind(admitted => admitted.Switch(
            state: op,
            unspecified: static (_, _) => Fin.Succ(unit),
            @explicit: static (op, row) => Present(op, row.Value),
            system: static (op, row) => guard(
                    flag: row.Role is not null && row.Size.ForAll(static size => float.IsFinite(size) && size > 0f),
                    False: op.InvalidInput())
                .ToFin()));

    private static Fin<Unit> Files(FileInquiry request, Op op) =>
        Optional(request).ToFin(Fail: op.InvalidInput()).Bind(admitted => admitted.Switch(
            state: op,
            save: static (op, row) => FileFrame(row.Frame, op),
            open: static (op, row) =>
                from _ in Present(op, row.Multiplicity)
                from __ in FileFrame(row.Frame, op)
                select unit,
            folder: static (op, row) =>
                from _ in Present(op, row.Title)
                from __ in FileLocation(row.Directory, op)
                select unit));

    private static Fin<Unit> FileFrame(FileFrame frame, Op op) =>
        Optional(frame).ToFin(Fail: op.InvalidInput()).Bind(admitted =>
            from _ in FileLocation(admitted.Seed, op)
            from __ in FileLocation(admitted.Directory, op)
            select unit);

    private static Fin<Unit> FileLocation(Option<FileLocation> location, Op op) =>
        location.Match(
            Some: value => op.AcceptValidated<FileLocation>(value.ToValue()).As(),
            None: () => Fin.Succ(unit));

    private static Fin<Unit> Present(Op op, params object?[] values) =>
        guard(flag: toSeq(values).ForAll(static value => value is not null), False: op.InvalidInput()).ToFin();

    private static Fin<InquiryAnswer> Accepted(bool accepted, Op op, Func<Fin<InquiryAnswer>> answer) =>
        accepted ? op.Catch(answer) : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: op));

    private static Fin<InquiryAnswer> Number(NumberInquiry request, double value, Op op) =>
        from _ in guard(
                flag: double.IsFinite(value)
                    && request.Bounds.ForAll(range => value >= range.Lower && value <= range.Upper),
                False: op.InvalidResult())
            .ToFin()
        select (InquiryAnswer)new InquiryAnswer.Number(Value: value);

    private static Fin<InquiryAnswer> Range(RangeInquiry request, double minimum, double maximum, Op op) =>
        from _ in guard(
                flag: double.IsFinite(minimum) && double.IsFinite(maximum) && minimum <= maximum
                    && (request.MinimumEdge == RangeEdge.Adjustable || minimum >= request.Minimum)
                    && (request.MaximumEdge == RangeEdge.Adjustable || maximum <= request.Maximum),
                False: op.InvalidResult())
            .ToFin()
        select (InquiryAnswer)new InquiryAnswer.Range(Minimum: minimum, Maximum: maximum);

    private static Fin<Unit> Keyed(Seq<InquiryKey> keys, Op op) =>
        guard(flag: !keys.IsEmpty && keys.Distinct().Count == keys.Count, False: op.InvalidInput()).ToFin();

    private static Fin<Seq<(InquiryKey Key, TValue Value)>> Zipped<TValue>(Seq<InquiryKey> keys, IEnumerable<TValue> values, Op op) =>
        toSeq(values).Strict() is var settled && settled.Count == keys.Count
            ? Fin.Succ(value: keys.Zip(settled).Strict())
            : Fin.Fail<Seq<(InquiryKey Key, TValue Value)>>(error: op.InvalidResult());

    private static Fin<InquiryAnswer> SelectLayer(
        (Inquiry.Layers Request, RhinoDoc Document, Op Op) frame,
        LayerInquiry.One scope,
        int seed) {
        bool setCurrent = scope.Current.Initial;
        int index = seed;
        bool accepted = Dialogs.ShowSelectLayerDialog(
            layerIndex: ref index,
            dialogTitle: frame.Request.Title.Resolve(),
            showNewLayerButton: frame.Request.Creation.Key,
            showSetCurrentButton: scope.Current.Show,
            initialSetCurrentState: ref setCurrent);
        return accepted
            ? Roster(index: index, count: frame.Document.Layers.Count, sentinel: None, failure: frame.Op.InvalidResult())
                .Map<InquiryAnswer>(value => new InquiryAnswer.Layer(Index: value, SetCurrent: setCurrent))
            : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: frame.Op));
    }

    private static Fin<Seq<int>> Picked(Inquiry.Layers request, RhinoDoc document, Op op) =>
        from _ in Roster(values: request.Selected, count: document.Layers.Count, requireValue: false, failure: op.InvalidInput())
        from picked in Dialogs.ShowSelectMultipleLayersDialog(
            defaultLayerIndices: request.Selected.AsIterable(),
            dialogTitle: request.Title.Resolve(),
            showNewLayerButton: request.Creation.Key,
            layerIndices: out int[] indices)
            ? Fin.Succ(value: toSeq(indices).Strict())
            : Fin.Fail<Seq<int>>(error: new UiFault.Dismissed(Key: op))
        from admitted in Roster(values: picked, count: document.Layers.Count, requireValue: true, failure: op.InvalidResult())
        select admitted;

    private static Fin<Seq<int>> Roster(Seq<int> values, int count, bool requireValue, Error failure) =>
        count >= 0
            && (!requireValue || !values.IsEmpty)
            && values.Distinct().Count == values.Count
            && values.ForAll(index => index >= 0 && index < count)
            ? Fin.Succ(value: values)
            : Fin.Fail<Seq<int>>(error: failure);

    private static Fin<int> Roster(int index, int count, Option<int> sentinel, Error failure) =>
        count >= 0 && (index >= 0 && index < count || sentinel.Case is int allowed && index == allowed)
            ? Fin.Succ(value: index)
            : Fin.Fail<int>(error: failure);

    private static Fin<InquiryAnswer> Paths(Seq<string> raw, Op op) =>
        from _ in guard(flag: !raw.IsEmpty, False: op.InvalidResult()).ToFin()
        from values in raw.TraverseM(path => op.AcceptValidated<FileLocation>(path)).As()
        select (InquiryAnswer)new InquiryAnswer.Paths(Values: values.Strict());
}
```

## [03]-[HOST_ASSETS]

- Owner: `AssetRequest` combines resource and preview production because every case consumes host display policy and yields a disposable native asset, a detached metric, a pixel plane, or immutable stroke geometry.
- Entry: `HostAssets.Render` admits the complete request before document projection; only `MeshPreview` carries a `DocumentSession` and demands `SessionNeed.Read`.
- Seam: text shaping and metrics are the Eto `GlyphBlock` owner — `TextMeasure` carries a block and answers its memoized `Measure`, `FontSeed.System` carries the Eto `TypeRole` with an optional point size, and no `FormattedText` or font-role table is minted on this surface.
- Law: `MeshPreview` accepts zero colors to derive the document display color, one color to broadcast, or one color per mesh; every other cardinality is invalid.
- Law: `LinetypePreview` carries `PreviewChannel` and positive pattern scale, so host paint semantics remain data rather than an unbounded integer.
- Law: `AssetSource`, `AssetSize`, and `PreviewScale` reject invalid ingress once; `AssetSize` admits both a maximum dimension and an overflow-safe pixel budget before provider allocation.
- Law: `PreviewPolarity.Host` reads dark mode at execution time, while `Light` and `Dark` are explicit policy rows rather than a tri-state optional boolean.
- Law: pixel planes and preview strokes materialize into strict `Seq` values inside `Op.Catch` before leaving the host fold.
- Boundary: callers own disposal of returned `Bitmap`, `Icon`, and `Image` answers.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PreviewChannel {
    public static readonly PreviewChannel Dashes = new(key: 0);
    public static readonly PreviewChannel Shapes = new(key: 1);
    public static readonly PreviewChannel Glyphs = new(key: 2);
}

[SmartEnum]
public sealed partial class ResourceScale {
    public static readonly ResourceScale Native = new();
    public static readonly ResourceScale Down = new();
}

[SmartEnum]
public sealed partial class PreviewPolarity {
    public static readonly PreviewPolarity Host = new(resolve: static () => HostUtils.RunningInDarkMode);
    public static readonly PreviewPolarity Light = new(resolve: static () => false);
    public static readonly PreviewPolarity Dark = new(resolve: static () => true);

    [UseDelegateFromConstructor]
    internal partial bool Resolve();
}

[ValueObject<string>]
public readonly partial struct AssetSource {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value)
            ? new ValidationError(message: "Asset source is empty.")
            : null;
}

[ComplexValueObject]
public sealed partial class AssetSize {
    public DrawingSize Value { get; }
    public int MaximumDimension { get; }
    public long PixelBudget { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref DrawingSize value,
        ref int maximumDimension,
        ref long pixelBudget) =>
        validationError = value.Width <= 0 || value.Height <= 0 || maximumDimension <= 0 || pixelBudget <= 0L
            || value.Width > maximumDimension || value.Height > maximumDimension
            || (long)value.Width * value.Height > pixelBudget
            ? new ValidationError(message: "Asset size is invalid.")
            : null;
}

[ValueObject<double>]
public readonly partial struct PreviewScale {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = !double.IsFinite(value) || value <= 0d
            ? new ValidationError(message: "Preview scale is invalid.")
            : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssetRequest {
    private AssetRequest() { }
    public sealed record ResourceBitmap(AssetSource Name, AssetSize Size, Assembly Assembly, ResourceScale Scale) : AssetRequest;
    public sealed record ResourceIcon(AssetSource Name, AssetSize Size, Assembly Assembly, ResourceScale Scale) : AssetRequest;
    public sealed record ResourceImage(AssetSource Name, Assembly Assembly) : AssetRequest;
    public sealed record NamedColors(Option<NamedColorList> Source = default) : AssetRequest;
    public sealed record TextMeasure(GlyphBlock Block) : AssetRequest;
    public sealed record SvgBitmap(AssetSource Source, AssetSize Size, PreviewPolarity Polarity) : AssetRequest;
    public sealed record SvgPixels(AssetSource Source, AssetSize Size, DrawingColor Background, PixelAlpha Alpha, PreviewPolarity Polarity) : AssetRequest;
    public sealed record MeshPreview(DocumentSession Session, Seq<Mesh> Meshes, Seq<DrawingColor> Colors, AssetSize Size) : AssetRequest;
    public sealed record CurvePreview(Curve Curve, Linetype Linetype, AssetSize Size) : AssetRequest;
    public sealed record LinetypePreview(Curve Curve, Linetype Linetype, AssetSize Size, PreviewScale PatternScale, PreviewChannel Channel) : AssetRequest;

    internal Option<DocumentSession> Document => Switch(
        resourceBitmap: static _ => None,
        resourceIcon: static _ => None,
        resourceImage: static _ => None,
        namedColors: static _ => None,
        textMeasure: static _ => None,
        svgBitmap: static _ => None,
        svgPixels: static _ => None,
        meshPreview: static request => Some(request.Session),
        curvePreview: static _ => None,
        linetypePreview: static _ => None);

    internal Fin<AssetRequest> Admit(Op op) =>
        Switch(
                state: op,
                resourceBitmap: static (op, ask) => Resource(ask.Name, ask.Size, ask.Assembly, ask.Scale, op),
                resourceIcon: static (op, ask) => Resource(ask.Name, ask.Size, ask.Assembly, ask.Scale, op),
                resourceImage: static (op, ask) =>
                    from _ in Present(op, ask.Assembly)
                    from __ in Source(ask.Name, op)
                    select unit,
                namedColors: static (_, _) => Fin.Succ(unit),
                textMeasure: static (op, ask) => Present(op, ask.Block),
                svgBitmap: static (op, ask) =>
                    from _ in Present(op, ask.Size, ask.Polarity)
                    from __ in Source(ask.Source, op)
                    select unit,
                svgPixels: static (op, ask) =>
                    from _ in Present(op, ask.Size, ask.Alpha, ask.Polarity)
                    from __ in Source(ask.Source, op)
                    select unit,
                meshPreview: static (op, ask) => guard(
                        flag: ask.Session is not null && ask.Size is not null && !ask.Meshes.IsEmpty
                            && ask.Meshes.ForAll(static mesh => mesh is not null)
                            && (ask.Colors.Count is 0 or 1 || ask.Colors.Count == ask.Meshes.Count),
                        False: op.InvalidInput())
                    .ToFin(),
                curvePreview: static (op, ask) => Present(op, ask.Curve, ask.Linetype, ask.Size),
                linetypePreview: static (op, ask) =>
                    from _ in Present(op, ask.Curve, ask.Linetype, ask.Size, ask.Channel)
                    from __ in op.AcceptValidated<PreviewScale>(ask.PatternScale.ToValue()).As()
                    select unit)
            .Map(_ => this);

    private static Fin<Unit> Resource(AssetSource source, AssetSize size, Assembly assembly, ResourceScale scale, Op op) =>
        from _ in Present(op, size, assembly, scale)
        from __ in Source(source, op)
        select unit;

    private static Fin<Unit> Source(AssetSource source, Op op) =>
        op.AcceptValidated<AssetSource>(source.ToValue()).As();

    private static Fin<Unit> Present(Op op, params object?[] values) =>
        guard(flag: toSeq(values).ForAll(static value => value is not null), False: op.InvalidInput()).ToFin();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssetAnswer {
    private AssetAnswer() { }
    public sealed record Bitmap(DrawingBitmap Value) : AssetAnswer;
    public sealed record Icon(DrawingIcon Value) : AssetAnswer;
    public sealed record Image(DrawingImage Value) : AssetAnswer;
    public sealed record NamedColors(Seq<NamedColor> Values) : AssetAnswer;
    public sealed record TextMetrics(global::Eto.Drawing.SizeF Value) : AssetAnswer;
    public sealed record Pixels(Seq<byte> Value) : AssetAnswer;
    public sealed record Strokes(Seq<Seq<Point2f>> Value) : AssetAnswer;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class HostAssets {
    public static Fin<AssetAnswer> Render(AssetRequest request, Op? key = null) {
        ArgumentNullException.ThrowIfNull(request);
        Op op = key.OrDefault();
        return request.Admit(op).Bind(admitted => admitted.Document.Match(
            Some: session => HostThread.Run(
                work: new HostWork<AssetAnswer>.Session(
                    Document: session,
                    Needs: [SessionNeed.Read],
                    Body: document => Produce(document: Some(document), request: admitted, op: op)),
                key: op),
            None: () => HostThread.Run(
                work: new HostWork<AssetAnswer>.Execute(Body: () => Produce(document: None, request: admitted, op: op)),
                key: op)));
    }

    private static Fin<AssetAnswer> Produce(Option<RhinoDoc> document, AssetRequest request, Op op) =>
        request.Switch(
                    (Document: document, Op: op),
                    resourceBitmap: static (held, ask) => Loaded(
                        name: ask.Name, size: ask.Size, assembly: ask.Assembly, scale: ask.Scale, op: held.Op,
                        native: DrawingUtilities.BitmapFromIconResource,
                        down: DrawingUtilities.LoadBitmapWithScaleDown,
                        answer: static bitmap => new AssetAnswer.Bitmap(Value: bitmap)),
                    resourceIcon: static (held, ask) => Loaded(
                        name: ask.Name, size: ask.Size, assembly: ask.Assembly, scale: ask.Scale, op: held.Op,
                        native: DrawingUtilities.IconFromResource,
                        down: DrawingUtilities.LoadIconWithScaleDown,
                        answer: static icon => new AssetAnswer.Icon(Value: icon)),
                    resourceImage: static (held, ask) =>
                        from image in held.Op.Catch(() => Optional(DrawingUtilities.ImageFromResource(
                                resourceName: ask.Name.ToValue(),
                                assembly: ask.Assembly))
                            .ToFin(Fail: held.Op.InvalidResult()))
                        select (AssetAnswer)new AssetAnswer.Image(Value: image),
                    namedColors: static (_, ask) => Fin.Succ<AssetAnswer>(value: new AssetAnswer.NamedColors(
                        Values: toSeq(ask.Source.IfNone(() => NamedColorList.Default)).Strict())),
                    textMeasure: static (held, ask) => held.Op.Catch(() =>
                        Fin.Succ<AssetAnswer>(value: new AssetAnswer.TextMetrics(Value: ask.Block.Measure()))),
                    svgBitmap: static (held, ask) => held.Op.Catch(() => Optional(DrawingUtilities.BitmapFromSvg(
                            svg: ask.Source.ToValue(),
                            width: ask.Size.Value.Width,
                            height: ask.Size.Value.Height,
                            adjustForDarkMode: ask.Polarity.Resolve()))
                        .ToFin(Fail: held.Op.InvalidResult())
                        .Map(static bitmap => (AssetAnswer)new AssetAnswer.Bitmap(Value: bitmap))),
                    svgPixels: static (held, ask) => held.Op.Catch(() => Optional(DrawingUtilities.PixelsFromSvg(
                            svg: ask.Source.ToValue(),
                            width: ask.Size.Value.Width,
                            height: ask.Size.Value.Height,
                            premultiplyAlpha: ask.Alpha.Key,
                            backgroundColor: ask.Background,
                            adjustForDarkMode: ask.Polarity.Resolve()))
                        .ToFin(Fail: held.Op.InvalidResult())
                        .Map(static pixels => (AssetAnswer)new AssetAnswer.Pixels(Value: toSeq(pixels).Strict()))),
                    meshPreview: static (held, ask) =>
                        from _ in guard(flag: !ask.Meshes.IsEmpty, False: held.Op.InvalidInput()).ToFin()
                        from model in held.Document.ToFin(Fail: held.Op.MissingContext())
                        let size = ask.Size.Value
                        from colors in ask.Colors.Count switch {
                            0 => held.Op.Catch(() => {
                                DrawingColor color = model.CreateDefaultAttributes().DrawColor(model);
                                return Fin.Succ(value: ask.Meshes.Map(_ => color).Strict());
                            }),
                            1 => Fin.Succ(value: ask.Meshes.Map(_ => ask.Colors[0]).Strict()),
                            int count when count == ask.Meshes.Count => Fin.Succ(value: ask.Colors),
                            _ => Fin.Fail<Seq<DrawingColor>>(error: held.Op.InvalidInput()),
                        }
                        from bitmap in held.Op.Catch(() => Optional(DrawingUtilities.CreateMeshPreviewImage(
                                doc: model,
                                meshes: ask.Meshes,
                                colors: colors,
                                size: size))
                            .ToFin(Fail: held.Op.InvalidResult()))
                        select (AssetAnswer)new AssetAnswer.Bitmap(Value: bitmap),
                    curvePreview: static (held, ask) => held.Op.Catch(() => Optional(DrawingUtilities.CreateCurvePreviewGeometry(
                            curve: ask.Curve,
                            linetype: ask.Linetype,
                            width: ask.Size.Value.Width,
                            height: ask.Size.Value.Height))
                        .ToFin(Fail: held.Op.InvalidResult())
                        .Map(strokes => Stroked(strokes))),
                    linetypePreview: static (held, ask) => held.Op.Catch(() => Optional(DrawingUtilities.CreateLinetypePreviewGeometryEx(
                            ask.Curve,
                            ask.Linetype,
                            ask.Size.Value.Width,
                            ask.Size.Value.Height,
                            ask.PatternScale.ToValue(),
                            ask.Channel.Key))
                        .ToFin(Fail: held.Op.InvalidResult())
                        .Map(strokes => Stroked(strokes))));

    private static Fin<AssetAnswer> Loaded<TAsset>(
        AssetSource name,
        AssetSize size,
        Assembly assembly,
        ResourceScale scale,
        Op op,
        Func<string, DrawingSize, Assembly, TAsset?> native,
        Func<string, int, Assembly, TAsset?> down,
        Func<TAsset, AssetAnswer> answer) where TAsset : class =>
        op.Catch(() => Optional(scale.Switch(
                (Name: name.ToValue(), Size: size.Value, Assembly: assembly, Native: native, Down: down),
                native: static held => held.Native(held.Name, held.Size, held.Assembly),
                down: static held => held.Down(held.Name, held.Size.Width, held.Assembly)))
            .ToFin(Fail: op.InvalidResult()))
            .Map(answer);

    private static AssetAnswer Stroked(IEnumerable<IEnumerable<Point2f>> strokes) =>
        new AssetAnswer.Strokes(Value: toSeq(strokes).Map(static stroke => toSeq(stroke).Strict()).Strict());
}
```
