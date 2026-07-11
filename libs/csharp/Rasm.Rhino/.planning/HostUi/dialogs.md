# [RASM_RHINO_HOSTUI_DIALOGS]

The native dialog intent rail of `Rasm.Rhino.HostUi` — ONE closed `Inquiry` discriminant over the full `Rhino.UI.Dialogs` roster (message, text transcript, list, multi-list, check-list, property-list, context menu, edit box, number box, the layer family, both linetype pickers, print widths, sun, live color) plus the file prompts (Rhino save/open dialogs, the Eto folder chooser), folded by one `Inquiries.Ask` dispatch into a typed `InquiryAnswer` — and the `DrawingUtilities` resource and preview families: icon/bitmap/image resource loading with scale-down rows, SVG rasterization under the host dark-mode read, mesh preview images, and linetype/curve preview geometry. The census `Intent.cs` carried one factory per dialog modality and imported `Rasm.Rhino.Exchange` for its file prompts; both die here — every modality is a case whose payload is the request, every result is a kernel-neutral answer case (paths cross as plain admitted strings, never an exchange endpoint), and cancellation is one `UiFault.Dismissed` on every arm. Interrogation is capability-gated: `Ask` demands `SessionNeed.Dialog` on the `DocumentSession`, so a scripted or headless lane refuses a modal before any host window exists. Typed Eto interrogation stays the Eto chrome `Prompt<TResult>`; document-anchored presentation of such a dialog rides the shell `SemiModal` seam — this page owns only the host-native roster and its resources.

## [01]-[INDEX]

- [02]-[REQUEST_VOCABULARY]: `MessageShape` + `MenuEntry`/`MenuMode` + `LayerScope` + `LinetypeAsk` + `ColorAsk` + `FileFrame`/`FileAsk` — the request payload records and sub-families the inquiry cases carry.
- [03]-[INQUIRY_RAIL]: `Inquiry` + `InquiryAnswer` + `Inquiries.Ask` — the one native-intent discriminant, its mirrored answer family, and the single capability-gated, marshalled, dismissal-railed dispatch.
- [04]-[RESOURCES]: `ResourceKind` + `HostResources` — the (output × scale-down) resource-loader rows over the `DrawingUtilities` icon/bitmap/image family, SVG rasterization with the dark-mode read, and the named-color roster.
- [05]-[PREVIEWS]: `PreviewOp` + `PreviewYield` + `Previews` — mesh preview images and curve/linetype preview geometry as one request family over a read-gated document session.

## [02]-[REQUEST_VOCABULARY]

- Owner: the request payloads the `[03]` cases carry. `MessageShape` bundles the five host message selectors (`ShowMessageButton`/`ShowMessageIcon`/`ShowMessageDefaultButton`/`ShowMessageOptions`/`ShowMessageMode`) as one policy value with `Plain` the canonical row — the foreign enums live only on this seam record, never in a consumer signature. `MenuEntry` pairs a caption with a `MenuMode` row whose key IS the host mode ordinal (`Active` 0, `Muted` 1, `Divider` 2), so the context-menu mode array derives from rows and the census default-mode trap (an unset mode rendering every item unclickable) is unrepresentable. `LayerScope` closes the three layer-dialog shapes — single pick with set-current wiring, multi pick, multi pick with the material dialog chained — as one sub-family on one `Layer` case. `LinetypeAsk` closes the two host linetype pickers on their genuinely distinct identity regimes (table `Guid` versus table index). `ColorAsk` carries the live-preview color request — initial `Color4f`, alpha admission, optional named-color list, optional `Dialogs.OnColorChangedEvent` live callback. `FileFrame` is the shared file-prompt frame (title, filter, seed name, directory, default extension) and `FileAsk` the four-prompt family: save, open-one, open-many, folder.
- Law: sub-family payloads carry only what their host call consumes — a knob that re-describes the case is deleted, and the case IS the modality, so no request record grows a mode flag beside its discriminant.
- Law: file answers are kernel-neutral — full path strings on the answer case, admitted downstream by whichever exchange or document owner consumes them; a file-endpoint or capture contract on this page's surface is the census breach this vocabulary forecloses.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Rhino.UI (the message/color/named-color seam types), Eto.Forms (`SelectFolderDialog` seam).
- Growth (HOST): a new host dialog knob (a message option, a file-prompt flag) is one field on the owning payload record consumed in its `[03]` arm.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Reflection;
using Eto.Forms;
using Rasm.Csp;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rasm.Rhino.Eto;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Runtime;
using Rhino.UI;
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerScope {
    private LayerScope() { }
    public sealed record One(bool ShowSetCurrent = false, bool InitialSetCurrent = false) : LayerScope;
    public sealed record Many : LayerScope;
    public sealed record Material : LayerScope;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinetypeAsk {
    private LinetypeAsk() { }
    public sealed record ById(string Title, string Message, Option<Guid> Selected = default) : LinetypeAsk;
    public sealed record ByIndex(int Seed = -1, bool DisplayByLayer = false) : LinetypeAsk;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FileAsk {
    private FileAsk() { }
    public sealed record Save(FileFrame Frame) : FileAsk;
    public sealed record OpenOne(FileFrame Frame) : FileAsk;
    public sealed record OpenMany(FileFrame Frame) : FileAsk;
    public sealed record Folder(string Title, Option<string> Directory = default) : FileAsk;
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct MessageShape(
    ShowMessageButton Buttons = default,
    ShowMessageIcon Icon = default,
    ShowMessageDefaultButton Default = default,
    ShowMessageOptions Options = default,
    ShowMessageMode Mode = default) {
    public static readonly MessageShape Plain = new();
}

public readonly record struct MenuEntry(string Caption, MenuMode Mode);

public readonly record struct ColorAsk(
    Color4f Initial,
    bool AllowAlpha = false,
    Option<NamedColorList> Named = default,
    Option<Dialogs.OnColorChangedEvent> Live = default);

public sealed record FileFrame(
    string Title,
    string Filter,
    Option<string> Seed = default,
    Option<string> Directory = default,
    Option<string> Extension = default);
```

## [03]-[INQUIRY_RAIL]

- Owner: `Inquiry` — the one closed native-intent discriminant; every case is a complete request — and `InquiryAnswer`, the mirrored typed result family. `Inquiries.Ask(DocumentSession, Inquiry, Op?)` is the sole dispatch: it demands `SessionNeed.Dialog` (interactive lane, live document, no active point acquisition) through `HostThread.OnSession` — the demand marshals and the answer rides the crossing seam — resolves the parent once as the document main window, and runs one total generated `Switch` whose every arm converts the host outcome — a `false`, a null, a `Guid.Empty`, a negative width, a `-1` menu index — into `UiFault.Dismissed`. One dismissal fault on every arm is the uniform-cancellation acceptance the census factories scattered.
- Law: the answer discriminant is recoverable from the request case — `Pick` answers `Chosen`, `Layer` answers `Layers`, `Files` answers `Files` — so a consumer switches the answer only at the arity its request already fixed, and a wrong-shaped answer marks a dispatch defect, never a consumer branch.
- Law: seed clamping is request admission — the bounded number box clamps its seed into the window before the host call because the host bounds constrain the spinner, not the initial display; an inverted window is a typed rejection before any dialog exists.
- Law: the layer-material chain is one arm — multi-layer pick then `ShowLayerMaterialDialog` — so partial completion (layers picked, material dismissed) folds to `Dismissed` and never leaks a half-answered composite.
- Law: typed Eto interrogation and document-anchored semi-modal presentation compose the Eto chrome `Prompt` and the shell `SemiModal` seam; this rail carries only dialogs the host itself draws.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm.Domain (`Op`), Document sub-domain (`DocumentSession`, `SessionNeed.Dialog`), Eto sub-domain (`UiFault`), shell page (`HostThread.OnSession`), Rhino.UI (`Dialogs.ShowMessage`/`ShowTextDialog`/`ShowListBox`/`ShowMultiListBox`/`ShowCheckListBox`/`ShowPropertyListBox`/`ShowContextMenu`/`ShowEditBox`/`ShowNumberBox`/`ShowSelectLayerDialog`/`ShowSelectMultipleLayersDialog`/`ShowLayerMaterialDialog`/`ShowLineTypes`/`ShowSelectLinetypeDialog`/`ShowPrintWidths`/`ShowSunDialog`/`ShowColorDialog`, `RhinoSaveDialog.ShowSaveDialog`, `RhinoOpenDialog.ShowOpenDialog`), Eto.Forms (`SelectFolderDialog.ShowDialog`), RhinoCommon (`RhinoDoc.Lights.Sun`).
- Growth (DOMAIN): a new host dialog is one `Inquiry` case plus one `InquiryAnswer` case breaking `Ask` loudly at compile time; a factory beside the union is the census regression.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Inquiry {
    private Inquiry() { }
    public sealed record Message(string Body, string Title, MessageShape Shape) : Inquiry;
    public sealed record Transcript(string Body, string Title) : Inquiry;
    public sealed record Pick(string Title, string Prompt, Seq<string> Entries, Option<string> Selected = default) : Inquiry;
    public sealed record PickMany(string Title, string Prompt, Seq<string> Entries, Seq<string> Preselect = default) : Inquiry;
    public sealed record CheckList(string Title, string Prompt, Seq<(string Entry, bool Checked)> Rows) : Inquiry;
    public sealed record PropertyList(string Title, string Prompt, Seq<(string Name, string Value)> Rows) : Inquiry;
    public sealed record Menu(Seq<MenuEntry> Entries, DrawingPoint ScreenPoint) : Inquiry;
    public sealed record Edit(string Title, string Prompt, string Seed = "", bool Multiline = false) : Inquiry;
    public sealed record Number(string Title, string Prompt, double Seed = 0.0, Option<(double Lower, double Upper)> Window = default) : Inquiry;
    public sealed record Layer(string Title, LayerScope Scope, Seq<int> Preselect = default, bool ShowNewLayer = false) : Inquiry;
    public sealed record Linetype(LinetypeAsk Ask) : Inquiry;
    public sealed record PrintWidth(string Title, string Prompt, Option<double> Selected = default) : Inquiry;
    public sealed record Sun : Inquiry;
    public sealed record Color(ColorAsk Ask) : Inquiry;
    public sealed record Files(FileAsk Ask) : Inquiry;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InquiryAnswer {
    private InquiryAnswer() { }
    public sealed record Acknowledged(ShowMessageResult Verdict) : InquiryAnswer;
    public sealed record Done : InquiryAnswer;
    public sealed record Chosen(string Entry) : InquiryAnswer;
    public sealed record ChosenMany(Seq<string> Entries) : InquiryAnswer;
    public sealed record Checked(Seq<bool> States) : InquiryAnswer;
    public sealed record Valued(Seq<(string Name, string Value)> Rows) : InquiryAnswer;
    public sealed record MenuIndex(int Index) : InquiryAnswer;
    public sealed record Edited(string Text) : InquiryAnswer;
    public sealed record Measured(double Value) : InquiryAnswer;
    public sealed record Layers(Seq<int> Indices, bool SetCurrent, bool MaterialAccepted) : InquiryAnswer;
    public sealed record LinetypeId(Guid Id) : InquiryAnswer;
    public sealed record LinetypeIndex(int Index) : InquiryAnswer;
    public sealed record Pigment(Color4f Value) : InquiryAnswer;
    public sealed record Files(Seq<string> Paths) : InquiryAnswer;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Inquiries {
    public static Fin<InquiryAnswer> Ask(DocumentSession session, Inquiry ask, Op? key = null) {
        Op op = key.OrDefault();
        return HostThread.OnSession(
            session: session,
            body: document => {
                Window? parent = RhinoEtoApp.MainWindowForDocument(document);
                return ask.Switch(
                    state: (Document: document, Parent: parent, Op: op),
                    message: static (held, ask) =>
                        from body in held.Op.AcceptText(value: ask.Body)
                        select (InquiryAnswer)new InquiryAnswer.Acknowledged(Verdict: Dialogs.ShowMessage(
                            parent: held.Parent, message: body, title: ask.Title, buttons: ask.Shape.Buttons,
                            icon: ask.Shape.Icon, defaultButton: ask.Shape.Default, options: ask.Shape.Options, mode: ask.Shape.Mode)),
                    transcript: static (held, ask) =>
                        from body in held.Op.AcceptText(value: ask.Body)
                        from _ in held.Op.Catch(() => { Dialogs.ShowTextDialog(message: body, title: ask.Title); return Fin.Succ(value: unit); })
                        select (InquiryAnswer)new InquiryAnswer.Done(),
                    pick: static (held, ask) =>
                        from entries in Roster(entries: ask.Entries, op: held.Op)
                        from chosen in Settled(
                            picked: ask.Selected.Match(
                                Some: current => Dialogs.ShowListBox(title: ask.Title, message: ask.Prompt, items: entries, selectedItem: current),
                                None: () => Dialogs.ShowListBox(title: ask.Title, message: ask.Prompt, items: entries)) as string,
                            op: held.Op)
                        select (InquiryAnswer)new InquiryAnswer.Chosen(Entry: chosen),
                    pickMany: static (held, ask) =>
                        from _ in guard(flag: !ask.Entries.IsEmpty, False: held.Op.InvalidInput()).ToFin()
                        from chosen in Settled(
                            picked: Dialogs.ShowMultiListBox(title: ask.Title, message: ask.Prompt, items: [.. ask.Entries], defaults: [.. ask.Preselect]),
                            op: held.Op)
                        select (InquiryAnswer)new InquiryAnswer.ChosenMany(Entries: toSeq(chosen).Strict()),
                    checkList: static (held, ask) =>
                        from entries in Roster(entries: ask.Rows.Map(static row => row.Entry), op: held.Op)
                        from states in Settled(
                            picked: Dialogs.ShowCheckListBox(
                                title: ask.Title, message: ask.Prompt,
                                items: entries,
                                checkState: [.. ask.Rows.Map(static row => row.Checked)]),
                            op: held.Op)
                        select (InquiryAnswer)new InquiryAnswer.Checked(States: toSeq(states).Strict()),
                    propertyList: static (held, ask) =>
                        from names in Roster(entries: ask.Rows.Map(static row => row.Name), op: held.Op)
                        from updated in Settled(
                            picked: Dialogs.ShowPropertyListBox(
                                title: ask.Title, message: ask.Prompt,
                                items: names,
                                values: [.. ask.Rows.Map(static row => row.Value)]),
                            op: held.Op)
                        from rows in toSeq(updated).Count == ask.Rows.Count
                            ? Fin.Succ(value: ask.Rows.Map(static row => row.Name).Zip(toSeq(updated)).Strict())
                            : Fin.Fail<Seq<(string, string)>>(error: held.Op.InvalidResult())
                        select (InquiryAnswer)new InquiryAnswer.Valued(Rows: rows),
                    menu: static (held, ask) =>
                        from _ in guard(flag: !ask.Entries.IsEmpty, False: held.Op.InvalidInput()).ToFin()
                        from index in Dialogs.ShowContextMenu(
                            items: ask.Entries.Map(static entry => entry.Caption).AsIterable(),
                            screenPoint: ask.ScreenPoint,
                            modes: ask.Entries.Map(static entry => entry.Mode.Key).AsIterable()) switch {
                                int at when at >= 0 => Fin.Succ(value: at),
                                _ => Fin.Fail<int>(error: new UiFault.Dismissed(Key: held.Op)),
                            }
                        select (InquiryAnswer)new InquiryAnswer.MenuIndex(Index: index),
                    edit: static (held, ask) =>
                        Dialogs.ShowEditBox(title: ask.Title, message: ask.Prompt, defaultText: ask.Seed, multiline: ask.Multiline, text: out string text)
                            ? Fin.Succ(value: (InquiryAnswer)new InquiryAnswer.Edited(Text: text))
                            : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op)),
                    number: static (held, ask) => Numbered(held: held, ask: ask),
                    layer: static (held, ask) => Layered(held: held, ask: ask),
                    linetype: static (held, ask) => ask.Ask.Switch(
                        state: held,
                        byId: static (held, pick) =>
                            Dialogs.ShowLineTypes(title: pick.Title, message: pick.Message, doc: held.Document, selectedLineTypeId: pick.Selected.IfNone(Guid.Empty)) switch {
                                Guid id when id != Guid.Empty => Fin.Succ(value: (InquiryAnswer)new InquiryAnswer.LinetypeId(Id: id)),
                                _ => Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op)),
                            },
                        byIndex: static (held, pick) => {
                            int index = pick.Seed;
                            return Dialogs.ShowSelectLinetypeDialog(linetypeIndex: ref index, displayByLayer: pick.DisplayByLayer)
                                ? Fin.Succ(value: (InquiryAnswer)new InquiryAnswer.LinetypeIndex(Index: index))
                                : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op));
                        }),
                    printWidth: static (held, ask) =>
                        (ask.Selected.Match(
                            Some: seed => Dialogs.ShowPrintWidths(title: ask.Title, message: ask.Prompt, selectedWidth: seed),
                            None: () => Dialogs.ShowPrintWidths(title: ask.Title, message: ask.Prompt))) switch {
                                double width when width >= 0d => Fin.Succ(value: (InquiryAnswer)new InquiryAnswer.Measured(Value: width)),
                                _ => Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op)),
                            },
                    sun: static (held, _) =>
                        Dialogs.ShowSunDialog(sun: held.Document.Lights.Sun)
                            ? Fin.Succ(value: (InquiryAnswer)new InquiryAnswer.Done())
                            : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op)),
                    color: static (held, ask) => {
                        Color4f color = ask.Ask.Initial;
                        return Dialogs.ShowColorDialog(
                            parent: held.Parent,
                            color: ref color,
                            allowAlpha: ask.Ask.AllowAlpha,
                            namedColorList: ask.Ask.Named.IfNoneUnsafe((NamedColorList?)null),
                            colorCallback: ask.Ask.Live.IfNoneUnsafe((Dialogs.OnColorChangedEvent?)null))
                                ? Fin.Succ(value: (InquiryAnswer)new InquiryAnswer.Pigment(Value: color))
                                : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op));
                    },
                    files: static (held, ask) => Prompted(held: held, ask: ask.Ask));
            },
            op: op,
            SessionNeed.Dialog);
    }

    private static Fin<InquiryAnswer> Numbered((RhinoDoc Document, Window? Parent, Op Op) held, Inquiry.Number ask) =>
        ask.Window.Match(
            Some: window => window.Lower <= window.Upper
                ? Bounded(ask: ask, window: window, op: held.Op)
                : Fin.Fail<InquiryAnswer>(error: held.Op.InvalidInput()),
            None: () => {
                double value = ask.Seed;
                return Dialogs.ShowNumberBox(title: ask.Title, message: ask.Prompt, number: ref value)
                    ? Fin.Succ(value: (InquiryAnswer)new InquiryAnswer.Measured(Value: value))
                    : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op));
            });

    private static Fin<InquiryAnswer> Bounded(Inquiry.Number ask, (double Lower, double Upper) window, Op op) {
        double value = Math.Clamp(value: ask.Seed, min: window.Lower, max: window.Upper);
        return Dialogs.ShowNumberBox(title: ask.Title, message: ask.Prompt, number: ref value, minimum: window.Lower, maximum: window.Upper)
            ? Fin.Succ(value: (InquiryAnswer)new InquiryAnswer.Measured(Value: value))
            : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: op));
    }

    private static Fin<InquiryAnswer> Layered((RhinoDoc Document, Window? Parent, Op Op) held, Inquiry.Layer ask) =>
        ask.Scope.Switch(
            state: (held.Document, held.Op, Request: ask),
            one: static (frame, scope) => {
                bool setCurrent = scope.InitialSetCurrent;
                int index = frame.Request.Preselect.IsEmpty ? -1 : frame.Request.Preselect[0];
                return frame.Request.Preselect.Count switch {
                    > 1 => Fin.Fail<InquiryAnswer>(error: frame.Op.InvalidInput()),
                    _ => Dialogs.ShowSelectLayerDialog(
                            layerIndex: ref index, dialogTitle: frame.Request.Title,
                            showNewLayerButton: frame.Request.ShowNewLayer,
                            showSetCurrentButton: scope.ShowSetCurrent,
                            initialSetCurrentState: ref setCurrent)
                        ? Fin.Succ(value: (InquiryAnswer)new InquiryAnswer.Layers(Indices: Seq1(index), SetCurrent: setCurrent, MaterialAccepted: false))
                        : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: frame.Op)),
                };
            },
            many: static (frame, _) =>
                ManyLayers(request: frame.Request, op: frame.Op)
                    .Map(indices => (InquiryAnswer)new InquiryAnswer.Layers(Indices: indices, SetCurrent: false, MaterialAccepted: false)),
            material: static (frame, _) =>
                ManyLayers(request: frame.Request, op: frame.Op).Bind(indices =>
                    Dialogs.ShowLayerMaterialDialog(doc: frame.Document, layerIndices: indices.AsIterable())
                        ? Fin.Succ(value: (InquiryAnswer)new InquiryAnswer.Layers(Indices: indices, SetCurrent: false, MaterialAccepted: true))
                        : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: frame.Op))));

    private static Fin<InquiryAnswer> Prompted((RhinoDoc Document, Window? Parent, Op Op) held, FileAsk ask) =>
        ask.Switch(
            state: held,
            save: static (held, prompt) => new RhinoSaveDialog {
                    Title = prompt.Frame.Title, Filter = prompt.Frame.Filter,
                    FileName = prompt.Frame.Seed.IfNone(string.Empty),
                    InitialDirectory = prompt.Frame.Directory.IfNone(string.Empty),
                    DefaultExt = prompt.Frame.Extension.IfNone(string.Empty),
                } switch {
                    RhinoSaveDialog dialog => dialog.ShowSaveDialog()
                        ? Fin.Succ(value: (InquiryAnswer)new InquiryAnswer.Files(Paths: Seq1(dialog.FileName)))
                        : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op)),
                },
            openOne: static (held, prompt) => Opened(frame: prompt.Frame, multi: false, op: held.Op),
            openMany: static (held, prompt) => Opened(frame: prompt.Frame, multi: true, op: held.Op),
            folder: static (held, prompt) => new SelectFolderDialog { Title = prompt.Title, Directory = prompt.Directory.IfNone(string.Empty) } switch {
                SelectFolderDialog dialog => dialog.ShowDialog(parent: held.Parent) == DialogResult.Ok
                    ? Fin.Succ(value: (InquiryAnswer)new InquiryAnswer.Files(Paths: Seq1(dialog.Directory)))
                    : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: held.Op)),
            });

    private static Fin<InquiryAnswer> Opened(FileFrame frame, bool multi, Op op) =>
        new RhinoOpenDialog {
            Title = frame.Title, Filter = frame.Filter,
            FileName = frame.Seed.IfNone(string.Empty),
            InitialDirectory = frame.Directory.IfNone(string.Empty),
            DefaultExt = frame.Extension.IfNone(string.Empty),
            MultiSelect = multi,
        } switch {
            RhinoOpenDialog dialog => dialog.ShowOpenDialog()
                ? Fin.Succ(value: (InquiryAnswer)new InquiryAnswer.Files(Paths: multi ? toSeq(dialog.FileNames).Strict() : Seq1(dialog.FileName)))
                : Fin.Fail<InquiryAnswer>(error: new UiFault.Dismissed(Key: op)),
        };

    private static Fin<Seq<int>> ManyLayers(Inquiry.Layer request, Op op) =>
        Dialogs.ShowSelectMultipleLayersDialog(
            defaultLayerIndices: request.Preselect.AsIterable(),
            dialogTitle: request.Title,
            showNewLayerButton: request.ShowNewLayer,
            layerIndices: out int[] indices) switch {
                true => toSeq(indices).Filter(static index => index >= 0).Distinct() switch {
                    Seq<int> picked when !picked.IsEmpty => Fin.Succ(value: picked.Strict()),
                    _ => Fin.Fail<Seq<int>>(error: op.InvalidResult()),
                },
                false => Fin.Fail<Seq<int>>(error: new UiFault.Dismissed(Key: op)),
            };

    private static Fin<List<string>> Roster(Seq<string> entries, Op op) =>
        entries.IsEmpty ? Fin.Fail<List<string>>(error: op.InvalidInput()) : Fin.Succ(value: (List<string>)[.. entries]);

    private static Fin<T> Settled<T>(T? picked, Op op) where T : class =>
        Optional(picked).ToFin(Fail: new UiFault.Dismissed(Key: op));
}
```

## [04]-[RESOURCES]

- Owner: `ResourceKind` — the (output type × scale-down) loader vocabulary over the `DrawingUtilities` resource family: `Bitmap`/`BitmapScaled`/`Icon`/`IconScaled`/`Image` rows each carrying its `Output` type column and its `Mint` loader column, so the census frozen-dictionary-plus-typeof-switch collapses into rows and a kind/type mismatch is a typed rejection, never a cast surprise — and `HostResources`, the railed load surface: `Load<TOut>` gates name, size, and output agreement before the host call; `Vector` rasterizes SVG source through `BitmapFromSvg` with the dark-mode flag defaulting to the live `HostUtils.RunningInDarkMode` read; `Pixels` yields the raw RGBA byte plane through `PixelsFromSvg` with its premultiply and background-color axes for consumers compositing into their own buffers; `NamedColors` projects the host named-color roster from `NamedColorList.Default` or a supplied list.
- Law: dark-mode adaptation is a host fact read at load time, never a cached boolean — a pinned override enters as the explicit option, and the default re-reads the host on every rasterization so a theme flip mid-session cannot serve stale-polarity glyphs.
- Law: the `Image` row is scale-invariant by host contract — both scale polarities resolve `ImageFromResource` — and the row table states it once where the census restated it as duplicate dictionary entries.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm.Domain (`Op`), Eto sub-domain (`UiFault`), Rhino.UI (`DrawingUtilities.BitmapFromIconResource`/`LoadBitmapWithScaleDown`/`IconFromResource`/`LoadIconWithScaleDown`/`ImageFromResource`/`BitmapFromSvg`/`PixelsFromSvg`, `NamedColorList.Default`), RhinoCommon (`HostUtils.RunningInDarkMode`).
- Growth (HOST): a new host loader (a vector-tinted raster row, a cursor resource) is one `ResourceKind` row with its output column; a second loader table is the census regression.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ResourceKind {
    public static readonly ResourceKind Bitmap = new(key: 0, output: typeof(DrawingBitmap),
        mint: static (name, size, assembly) => DrawingUtilities.BitmapFromIconResource(name, size, assembly));
    public static readonly ResourceKind BitmapScaled = new(key: 1, output: typeof(DrawingBitmap),
        mint: static (name, size, assembly) => DrawingUtilities.LoadBitmapWithScaleDown(iconName: name, sizeDesired: size.Width, assembly: assembly));
    public static readonly ResourceKind Icon = new(key: 2, output: typeof(DrawingIcon),
        mint: static (name, size, assembly) => DrawingUtilities.IconFromResource(name, size, assembly));
    public static readonly ResourceKind IconScaled = new(key: 3, output: typeof(DrawingIcon),
        mint: static (name, size, assembly) => DrawingUtilities.LoadIconWithScaleDown(iconName: name, sizeDesired: size.Width, assembly: assembly));
    public static readonly ResourceKind Image = new(key: 4, output: typeof(DrawingImage),
        mint: static (name, _, assembly) => DrawingUtilities.ImageFromResource(name, assembly));

    public Type Output { get; }

    [UseDelegateFromConstructor]
    internal partial object? Mint(string name, DrawingSize size, Assembly assembly);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class HostResources {
    public static Fin<TOut> Load<TOut>(ResourceKind kind, string resourceName, DrawingSize size, Assembly assembly, Op? key = null) where TOut : class {
        Op op = key.OrDefault();
        return from name in op.AcceptText(value: resourceName)
               from _ in guard(flag: size.Width > 0 && size.Height > 0, False: op.InvalidInput()).ToFin()
               from __ in guard(flag: kind.Output.IsAssignableTo(targetType: typeof(TOut)), False: new UiFault.Rejected(
                   Key: op, Field: nameof(kind.Output), Reason: $"row '{kind}' yields {kind.Output.Name}, not {typeof(TOut).Name}")).ToFin()
               from loaded in op.Catch(() => Optional(kind.Mint(name: name, size: size, assembly: assembly) as TOut)
                   .ToFin(Fail: op.InvalidResult(detail: name)))
               select loaded;
    }

    public static Fin<DrawingBitmap> Vector(string svg, int width, int height, Option<bool> darkMode = default, Op? key = null) {
        Op op = key.OrDefault();
        return from source in op.AcceptText(value: svg)
               from _ in guard(flag: width > 0 && height > 0, False: op.InvalidInput()).ToFin()
               from bitmap in op.Catch(() => Optional(DrawingUtilities.BitmapFromSvg(
                       svg: source, width: width, height: height,
                       adjustForDarkMode: darkMode.IfNone(() => HostUtils.RunningInDarkMode)))
                   .ToFin(Fail: op.InvalidResult()))
               select bitmap;
    }

    public static Fin<byte[]> Pixels(string svg, int width, int height, DrawingColor background, bool premultiply = true, Option<bool> darkMode = default, Op? key = null) {
        Op op = key.OrDefault();
        return from source in op.AcceptText(value: svg)
               from _ in guard(flag: width > 0 && height > 0, False: op.InvalidInput()).ToFin()
               from plane in op.Catch(() => Optional(DrawingUtilities.PixelsFromSvg(
                       svg: source, width: width, height: height, premultiplyAlpha: premultiply,
                       backgroundColor: background, adjustForDarkMode: darkMode.IfNone(() => HostUtils.RunningInDarkMode)))
                   .ToFin(Fail: op.InvalidResult()))
               select plane;
    }

    public static Seq<NamedColor> NamedColors(Option<NamedColorList> source = default) =>
        toSeq(source.IfNone(() => NamedColorList.Default)).Strict();
}
```

## [05]-[PREVIEWS]

- Owner: `PreviewOp` — the closed preview-request family over the `DrawingUtilities` preview surface: `MeshImage` renders a mesh set into a preview bitmap with a 0/1/N color fold (absent colors derive from the document's default draw color, one color broadcasts, N colors pair positionally), `CurveStrokes` projects a curve under a linetype into stroke polylines, `LinetypeStrokes` projects the channel-selected linetype preview geometry — and `PreviewYield`, the typed result family (`Raster`, `Strokes`). `PreviewChannel` closes the host preview-channel ordinal the extended producer renders by, mirroring the viewport renderer: `Dashes` yields dash silhouette polygons meant to be filled, `Shapes` yields curve shape outlines meant to be stroked, `Glyphs` yields text and surface shape outlines meant for even-odd fill. `Previews.Render(DocumentSession, PreviewOp, Op?)` folds every case under `SessionNeed.Read`, because preview generation reads document display state without mutating it.
- Law: the color fold is admission — a color count that is neither zero, one, nor the mesh count is a typed rejection before the host call, so a silently mispaired preview is unrepresentable.
- Law: both stroke producers share one yield case — `CreateCurvePreviewGeometry` and `CreateLinetypePreviewGeometryEx` each return `List<Point2f[]>` polyline strokes, so `PreviewYield.Strokes` carries either producer and a consumer renders strokes without knowing which host member minted them; the channel row, not the producer, decides the paint rule.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm.Domain (`Op`), Document sub-domain (`DocumentSession`, `SessionNeed.Read`), shell page (`HostThread.OnSession`), Rhino.UI (`DrawingUtilities.CreateMeshPreviewImage`/`CreateCurvePreviewGeometry`/`CreateLinetypePreviewGeometryEx`), RhinoCommon (`RhinoDoc.CreateDefaultAttributes`).
- Growth (CONSUMER): a preview modality future panels demand (a hatch swatch, a material ball) is one `PreviewOp` case plus one `PreviewYield` case breaking `Render` at compile time.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PreviewChannel {
    public static readonly PreviewChannel Dashes = new(key: 0);
    public static readonly PreviewChannel Shapes = new(key: 1);
    public static readonly PreviewChannel Glyphs = new(key: 2);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PreviewOp {
    private PreviewOp() { }
    public sealed record MeshImage(Seq<Mesh> Meshes, Seq<DrawingColor> Colors, DrawingSize Size) : PreviewOp;
    public sealed record CurveStrokes(Curve Curve, Linetype Linetype, DrawingSize Size) : PreviewOp;
    public sealed record LinetypeStrokes(Curve Curve, Linetype Linetype, DrawingSize Size, double PatternScale, PreviewChannel Channel) : PreviewOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PreviewYield {
    private PreviewYield() { }
    public sealed record Raster(DrawingBitmap Image) : PreviewYield;
    public sealed record Strokes(Seq<Point2f[]> Polylines) : PreviewYield;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Previews {
    public static Fin<PreviewYield> Render(DocumentSession session, PreviewOp request, Op? key = null) {
        Op op = key.OrDefault();
        return HostThread.OnSession(
            session: session,
            body: document => request.Switch(
                state: (Document: document, Op: op),
                meshImage: static (held, ask) =>
                    from _ in guard(flag: !ask.Meshes.IsEmpty && ask.Size.Width > 0 && ask.Size.Height > 0, False: held.Op.InvalidInput()).ToFin()
                    from colors in ask.Colors.Count switch {
                        0 => held.Op.Catch(() => {
                            DrawingColor fallback = held.Document.CreateDefaultAttributes().DrawColor(held.Document);
                            return Fin.Succ(value: ask.Meshes.Map(_ => fallback).Strict());
                        }),
                        1 => Fin.Succ(value: ask.Meshes.Map(_ => ask.Colors[0]).Strict()),
                        int count when count == ask.Meshes.Count => Fin.Succ(value: ask.Colors),
                        _ => Fin.Fail<Seq<DrawingColor>>(error: held.Op.InvalidInput()),
                    }
                    from bitmap in held.Op.Catch(() => Optional(DrawingUtilities.CreateMeshPreviewImage(
                            doc: held.Document, meshes: ask.Meshes, colors: colors, size: ask.Size))
                        .ToFin(Fail: held.Op.InvalidResult()))
                    select (PreviewYield)new PreviewYield.Raster(Image: bitmap),
                curveStrokes: static (held, ask) =>
                    from _ in guard(flag: ask.Size.Width > 0 && ask.Size.Height > 0, False: held.Op.InvalidInput()).ToFin()
                    from strokes in held.Op.Catch(() => Optional(DrawingUtilities.CreateCurvePreviewGeometry(
                            curve: ask.Curve, linetype: ask.Linetype, width: ask.Size.Width, height: ask.Size.Height))
                        .ToFin(Fail: held.Op.InvalidResult()))
                    select (PreviewYield)new PreviewYield.Strokes(Polylines: toSeq(strokes).Strict()),
                linetypeStrokes: static (held, ask) =>
                    from _ in guard(flag: ask.Size.Width > 0 && ask.Size.Height > 0 && ask.PatternScale > 0d, False: held.Op.InvalidInput()).ToFin()
                    from strokes in held.Op.Catch(() => Optional(DrawingUtilities.CreateLinetypePreviewGeometryEx(
                            ask.Curve, ask.Linetype, ask.Size.Width, ask.Size.Height, ask.PatternScale, ask.Channel.Key))
                        .ToFin(Fail: held.Op.InvalidResult()))
                    select (PreviewYield)new PreviewYield.Strokes(Polylines: toSeq(strokes).Strict())),
            op: op,
            SessionNeed.Read);
    }
}
```
