# [BIM_IFC_WIRE_FORM]

`Rasm.Bim` owns the IFC wire-form vocabulary as the SOLE GeometryGym/IFC owner: the serialization a document is written in, the container it is wrapped by, the release span each serialization publishes across, and the byte read that recovers a release from foreign bytes before any database is constructed.

Serialization and container are SEPARATE axes whose product is sparse on both crossings — a container names the serializations it wraps, a serialization names the releases it publishes for — so a form is a generated corner of that product rather than an enumerated row, and the pair `(form, release)` a producer may seal is a matrix read both ends of the wire perform.

The peer decoder at `libs/typescript/core/.planning/interchange/frame.md` models the same two axes and admits one serialization this producer refuses. That refusal is DECLARED here as a row carrying its own diagnostic — an `ifcx` payload is identified by its own header member and refused by name — rather than expressed as a missing row a reader can only observe as a header miss.

Faults rail `Model/faults#FAULT_BAND` `BimFault` through their `Detail` row; the re-author that consumes a form is `Projection/egress#IFC_EGRESS`.

## [01]-[INDEX]

- [02]-[IFC_WIRE_FORM]: `IfcSerialization`/`IfcContainer`/`SniffExtent` the three axes, `IfcWireForm` the generated crossing carrying the descriptor derivations and the published matrix, `Sniff` the pre-construction release read, and `EmitContext` the emit-axis carrier.

## [02]-[IFC_WIRE_FORM]

- Owner: `IfcSerialization` the `[SmartEnum<string>]` naming each IFC document encoding beside its entry extension, its interop `FidelityRank`, the `SniffExtent` its header read costs, the `ReleaseVersion` span it publishes across, its optional producer `Refusal`, and its own `Probe`/`Seal`/`Admit` delegates; `IfcContainer` the wrapper axis carrying the serializations it `Wraps`, the extension that overrides the inner one, the extent it `Raises`, and the `Unwrap` that recovers the wrapped bytes; `SniffExtent` the release-read cost carrying its byte window and the forfeit it states; `IfcWireForm` the admitted crossing of the two axes; `EmitContext` the one emit-axis carrier.
- Cases: serializations `step` · `xml` · `json` · `ifcx`; containers `plain` · `zip`; extents `line` · `element` · `document`. The crossing `Forms` generates every pair the `Wraps` column admits — `step`, `step-zip`, `xml`, `json`, `ifcx` — and the four named handles read off it, so a new serialization or container yields its forms with no handle edit.
- Law: the two axes cross SPARSELY in both directions — `zip` wraps the two text serializations alone, so a zipped ifcJSON names a wrapper nothing defines, and each serialization publishes across the releases whose schema it was authored against, so `Published(form, release)` refuses a pair no schema validates. A fused row set expressed the first invariant only as an absent row and the second not at all, which let this producer seal an artifact its own peer decoder refuses.
- Law: `Ifc4X3` publishes under NO serialization — the ISO-approved 4.3 line carries the `IFC4X3_ADD2` identifier and every published artifact spells it, so the seam release stays in the roster to NAME that refusal rather than failing an unrecognised token at the peer.
- Law: PUBLICATION and ADMISSION are two verdicts. `Published` gates the producer, so nothing is sealed that a peer refuses; `Sniff` gates the reader and admits every release the frozen `Model/elements#IFC_CLASS` `ReleaseMap.Lower` carries, release candidates included, because reading a legacy document is a capability while writing one is a claim. Collapsing the two onto one predicate either forfeits the legacy read or publishes the draft.
- Entry: `IfcWireForm.Route(key)` resolves a form from its wire token and `IfcWireForm.Of(serialization, container, key)` admits a pair against the `Wraps` crossing; `form.Published(release, key)` is the emit precondition; `form.Sniff(bytes, key)` returns `Fin<GGRelease>` — the release the import rail seeds the database with, read off the bytes BEFORE construction; `form.Seal(target, entry)`/`form.Admit(bytes, release)` are the two byte directions.
- Auto: the descriptor DERIVES from two row reads — the container's extension wins where it names one, and its raised extent wins over the serialization's own, so a zipped payload is unwrapped to its whole document before the header read instead of having a text probe run over archive bytes. `Sniff` unwraps the container, slices the extent's window, runs the serialization's `Probe`, then refuses a refused row by name, parses the token onto the GeometryGym release, and gates `ReleaseMap.Lower` membership — the `IFC4X4_DRAFT` member excluded by law — so an absent header, an unparseable token, an unadmitted release, and an unproduced serialization are four verdicts and the import never guesses 4x3 over a 2x3 file [H8].
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new serialization is one `IfcSerialization` row carrying its span and its three delegates; a new container is one `IfcContainer` row naming what it wraps; a new release span is a cell on the row that publishes it; a producer refusal is the `Refusal` column, never an absent row.
- Boundary: the write container is GeometryGym's OWN entry-extension dispatch inside `WriteStream`, so the seal takes the form's extension in the entry name and no container ladder exists on that side; the read container is this page's `Unwrap`, because GeometryGym opens no archive on the read side and its `TextReader` door elects XML or STEP by peek alone — a `_` tail handing archive bytes to the text parser reads as a malformed model rather than as a container nothing opened. A `false` writer return is REFUSAL, never an empty artifact, because an empty buffer read as a written model is the forged artifact. `Option`, not `Fin`, on both delegates: the row owns the byte act while its caller owns the fault vocabulary, so a refused write lifts under the calling `Op` rather than minting a second fault family here. The `ifcx` row carries a real header `Probe` and refusing byte delegates: the probe makes the refusal NAME the version it found, and the delegates sit beneath the `Published`/`Sniff` gates as defense-in-depth, unreachable while those gates stand. The `Exchange/format#FORMAT_AXIS` catalogue-pending IFC5 row seats on this serialization when a toolkit lands; until then no `InterchangeFormat` row reaches it and the refusal is the whole capability. Release RAISING (the seam-to-GeometryGym target) is `Projection/egress#IFC_EGRESS` `ReleaseRaise` — this page reads the lowering direction alone, and the GeometryGym `ReleaseVersion` never reaches the seam `Header`.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Bim.Model;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;
using GGRelease = GeometryGym.Ifc.ReleaseVersion;
using BimRail = Rasm.Domain.HookRail<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Bim.Projection;

// --- [TYPES] ------------------------------------------------------------------------------
// What a release read COSTS, in the one unit an ingress budget spends: how much payload must arrive before the
// release is known. The forfeit is the row's own column because the three differ genuinely — a STEP header is a
// line, an XML root is one element, and a JSON member carries no ordering guarantee at all.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SniffExtent {
    public static readonly SniffExtent Line     = new("line",     Some(HeaderWindow), "release-unknown-before-header-line");
    public static readonly SniffExtent Element  = new("element",  Some(HeaderWindow), "release-unknown-before-root-element");
    public static readonly SniffExtent Document = new("document", Option<int>.None,   "release-unknown-before-whole-document");

    // A schema declaration outside the first few KiB of a line- or element-extent payload is not a header, so those
    // reads are bounded rather than materializing an arbitrary file as a string; None is the whole document.
    const int HeaderWindow = 4096;

    public Option<int> Window { get; }

    // The forfeit the row states, raised as the fault subject when the read completes without a release — so an
    // operator reads WHICH extent was consumed rather than a bare miss.
    public string Degrade { get; }

    public ReadOnlyMemory<byte> Slice(ReadOnlyMemory<byte> payload) =>
        Window.Match(Some: window => payload[..Math.Min(payload.Length, window)], None: () => payload);
}

// The wrapper axis. A container wraps whatever text a serialization wrote, so it carries no text identity of its
// own: seating a zip beside the serializations named no text at all, left a zipped ifcXML with no seat, and handed
// every reader a token it had to re-inspect the bytes to interpret.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IfcContainer {
    // Plain DERIVES its wrap set from the serialization roster rather than restating it, so a new serialization is
    // wrappable-unwrapped by construction. The delegate defers the read past both type initializers.
    public static readonly IfcContainer Plain = new("plain", Option<string>.None, Option<SniffExtent>.None, Every, Passthrough);
    public static readonly IfcContainer Zip   = new("zip", Some(".ifczip"), Some(SniffExtent.Document), Texts, Unarchived);

    // The wrapper's extension WINS over the inner one where it names one — the buildingSMART `.ifczip` container
    // extension GeometryGym's own write dispatch reads.
    public Option<string> Extension { get; }

    // Inflation is the container's whole sniff consequence, and the row names WHAT it raises to rather than a bool
    // a reader must re-map onto an extent.
    public Option<SniffExtent> Raises { get; }

    [UseDelegateFromConstructor]
    public partial Seq<IfcSerialization> Wraps();

    static Seq<IfcSerialization> Every() => IfcSerialization.Items.AsIterable().ToSeq();

    static Seq<IfcSerialization> Texts() => Seq(IfcSerialization.Step, IfcSerialization.Xml);

    // The read side is the row's own: an unwrapped payload IS its own bytes, and the archive's first entry IS the
    // model — the seal lays down exactly one. An empty archive REFUSES rather than admitting an empty database,
    // the same forged-artifact law the seal side holds.
    [UseDelegateFromConstructor]
    public partial Option<ReadOnlyMemory<byte>> Unwrap(ReadOnlyMemory<byte> payload);

    static Option<ReadOnlyMemory<byte>> Passthrough(ReadOnlyMemory<byte> bytes) => Some(bytes);

    static Option<ReadOnlyMemory<byte>> Unarchived(ReadOnlyMemory<byte> bytes) {
        using MemoryStream source = new(bytes.ToArray(), writable: false);
        using ZipArchive archive = new(source, ZipArchiveMode.Read);
        return archive.Entries is { Count: > 0 } entries ? Entry(entries[0]) : None;
    }

    static Option<ReadOnlyMemory<byte>> Entry(ZipArchiveEntry entry) {
        using MemoryStream sink = new();
        using (Stream source = entry.Open()) { source.CopyTo(sink); }
        return Some<ReadOnlyMemory<byte>>(sink.ToArray());
    }
}

// The encoding axis. Releases is the SPAN the row publishes across and it is sparse: STEP and XML carry the four
// editions that shipped an EXPRESS schema and an XSD together, ifcJSON the one release its schema was authored
// for, IFCX the IFC5 line alone. The seam Ifc4X3 appears in NO span — the ISO edition publishes as Ifc4X3Add2 — so
// a document spelling it refuses by name. GeometryGym's FormatIfcSerialization carries NO column here: the stream
// writer dispatches on the entry extension and the one enum member any body names is intrinsic to the single row
// that binds it, so a column would have been a value nothing read and a lie on the row GeometryGym cannot write.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IfcSerialization {
    public static readonly IfcSerialization Step = new("step", ".ifc", 0, SniffExtent.Line,
        Seq(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4, ReleaseVersion.Ifc4X1, ReleaseVersion.Ifc4X3Add2),
        Option<string>.None, StepToken, Streamed, Parsed);
    public static readonly IfcSerialization Xml = new("xml", ".ifcxml", 1, SniffExtent.Element,
        Seq(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4, ReleaseVersion.Ifc4X1, ReleaseVersion.Ifc4X3Add2),
        Option<string>.None, XmlnsToken, Streamed, Parsed);
    public static readonly IfcSerialization Json = new("json", ".ifcjson", 2, SniffExtent.Document,
        Seq(ReleaseVersion.Ifc4), Option<string>.None, JsonToken, Texted, Jsoned);
    // IFC5's own encoding, not a `json` release: the document carries its release at header.ifcxVersion where
    // ifcJSON reads schemaIdentifier, so folding IFC5 onto the JSON row would fork two vocabularies into one member
    // read. This producer authors no IFC5 — the frozen ReleaseMap gives the seam Ifc5 no GeometryGym image — so the
    // row DECLARES that refusal beside a real probe, and an .ifcx payload is named rather than reported as a
    // header miss by the STEP probe it would otherwise fall through to.
    public static readonly IfcSerialization Ifcx = new("ifcx", ".ifcx", 3, SniffExtent.Document,
        Seq(ReleaseVersion.Ifc5), Some("ifc-form-unproduced"), IfcxToken, Unproduced, Unproduced);

    public string Extension { get; }

    // The interop-fidelity rank, ascending — the ROW owns it because the row owns the encoding it ranks, and a
    // container ranks with the serialization it repeats, so a negotiation fold needs no second dispatch.
    public int FidelityRank { get; }

    public SniffExtent Extent { get; }

    public Seq<ReleaseVersion> Releases { get; }

    // Some names a serialization this producer identifies and refuses, carrying the row it raises. Absence of a
    // row cannot say this: it reads as an unknown encoding, which is what let a peer-admitted form diverge silently.
    public Option<string> Refusal { get; }

    // The header read, bounded by the extent the caller sliced. ReadOnlyMemory, not a span: the delegate column is
    // a generic Func and a ref struct cannot be its argument.
    [UseDelegateFromConstructor]
    public partial Option<string> Probe(ReadOnlyMemory<byte> header);

    [UseDelegateFromConstructor]
    public partial Option<ReadOnlyMemory<byte>> Seal(DatabaseIfc target, string entry);

    // `release` is the ADMITTED schema Sniff already gated against the frozen ReleaseMap, stamped after the parse
    // so the database carries the admitted release rather than GeometryGym's own header guess — which is the whole
    // point of sniffing first.
    [UseDelegateFromConstructor]
    public partial Option<DatabaseIfc> Admit(ReadOnlyMemory<byte> bytes, GGRelease release);

    // --- [HEADER_PROBES]

    static Option<string> StepToken(ReadOnlyMemory<byte> header) =>
        Delimited(Encoding.ASCII.GetString(header.Span), "FILE_SCHEMA", StringComparison.Ordinal, opening: '\'', StepTokenEnd);

    // The ifcXML xmlns schema URI — ".../ifcXML/<release>[/AddN]": the first path segment after the marker is the
    // token, so the segment starts AT the marker and the read passes no opening delimiter.
    static Option<string> XmlnsToken(ReadOnlyMemory<byte> header) =>
        Delimited(Encoding.UTF8.GetString(header.Span), "ifcXML/", StringComparison.OrdinalIgnoreCase, opening: '\0', XmlTokenEnd);

    static Option<string> JsonToken(ReadOnlyMemory<byte> header) => Member(header, "schemaIdentifier");

    static Option<string> IfcxToken(ReadOnlyMemory<byte> header) => Member(header, "header", "ifcxVersion");

    static readonly char[] XmlTokenEnd = ['/', '"', '\''];
    static readonly char[] StepTokenEnd = ['\''];

    // The ONE marker-then-delimited-slice read the two text probes take, expression-shaped and TOTAL: a missing
    // marker, a missing opening delimiter, and an unterminated token are three Nones, never an index that walks off
    // the window. '\0' as the opening delimiter means the token starts immediately after the marker.
    static Option<string> Delimited(string header, string marker, StringComparison how, char opening, char[] closing) =>
        header.IndexOf(marker, how) switch {
            < 0 => None,
            var at => (opening == '\0' ? at + marker.Length : header.IndexOf(opening, at) + 1) switch {
                <= 0 => None,
                var start => header.IndexOfAny(closing, start) switch {
                    var end when end > start => Some(header[start..end]),
                    _ => None,
                },
            },
        };

    // The ONE JSON header read both document-extent rows take: the path walks the member chain the row names; the
    // enclosing Sniff Op.Catch preserves a malformed payload's parse error before this optional member read returns.
    static Option<string> Member(ReadOnlyMemory<byte> document, params string[] path) =>
        Optional(toSeq(path).Fold(JsonNode.Parse(document.Span), static (node, step) => node?[step])?.ToString());

    // --- [BYTE_DIRECTIONS]

    // GeometryGym's WriteStream reads the ENTRY extension and opens the container itself — a ZipArchive holding one
    // `<stem>.ifc` entry for `.ifczip`, an ifcXML document for `.xml`, STEP text otherwise — so the container rides
    // the entry name and no write-side ladder exists here.
    static Option<ReadOnlyMemory<byte>> Streamed(DatabaseIfc target, string entry) {
        using MemoryStream sink = new();
        return target.WriteStream(sink, entry) ? Some<ReadOnlyMemory<byte>>(sink.ToArray()) : None;
    }

    // Only the ifcJSON row binds this: GeometryGym's stream writer carries no JSON arm, so the row spells its own
    // column rather than a ladder value.
    static Option<ReadOnlyMemory<byte>> Texted(DatabaseIfc target, string entry) =>
        Optional(target.ToString(FormatIfcSerialization.JSON))
            .Map(static text => (ReadOnlyMemory<byte>)Encoding.UTF8.GetBytes(text));

    // The two TEXT serializations share ONE body: DatabaseIfc(TextReader) peeks the first character and reads an XML
    // document on '<' or a STEP stream otherwise, so neither row needs an arm of its own and a local format switch
    // here would restate a dispatch GeometryGym already owns.
    static Option<DatabaseIfc> Parsed(ReadOnlyMemory<byte> bytes, GGRelease release) {
        string text = Encoding.UTF8.GetString(bytes.Span);
        if (text.Length == 0) { return None; }
        using StringReader source = new(text);
        return Some(new DatabaseIfc(source) { Release = release });
    }

    // The ifcJSON reader is its OWN public door — the TextReader peek elects XML or STEP and never JSON — so the row
    // constructs the database at the admitted release and reads the payload into it: the read IS the mutation, the
    // same construct-is-the-authoring-act seam the relationship side names.
    static Option<DatabaseIfc> Jsoned(ReadOnlyMemory<byte> bytes, GGRelease release) {
        if (bytes.IsEmpty) { return None; }
        using StringReader source = new(Encoding.UTF8.GetString(bytes.Span));
        DatabaseIfc target = new(release);
        target.ReadJSONFile(source);
        return Some(target);
    }

    // The refused row's byte pair, one body for both directions: the Published and Sniff gates stand above it, so
    // reaching here at all would mean a gate was bypassed and None is the honest answer either way.
    static Option<ReadOnlyMemory<byte>> Unproduced(DatabaseIfc target, string entry) => None;

    static Option<DatabaseIfc> Unproduced(ReadOnlyMemory<byte> bytes, GGRelease release) => None;
}

// --- [MODELS] -----------------------------------------------------------------------------
// The admitted crossing of the two axes. The product is GENERATED off the wraps column, so the sparse container
// crossing is enforced by construction rather than by a roster nobody can prove complete, and the four named
// handles below are reads off that crossing.
public sealed record IfcWireForm {
    IfcWireForm(IfcSerialization serialization, IfcContainer container) {
        Serialization = serialization;
        Container = container;
    }

    public IfcSerialization Serialization { get; }

    public IfcContainer Container { get; }

    public static readonly Seq<IfcWireForm> Forms = IfcContainer.Items.AsIterable().ToSeq()
        .Bind(container => container.Wraps().Map(serialization => new IfcWireForm(serialization, container)));

    // The direct ToFrozenDictionary IS the uniqueness gate — two containers claiming one key fail at type
    // initialization (the FaultBand registry law), never a GroupBy/First mask silently electing a winner.
    static readonly FrozenDictionary<string, IfcWireForm> ByKey =
        Forms.ToFrozenDictionary(static form => form.Key, StringComparer.Ordinal);

    public static readonly IfcWireForm Step = ByKey["step"];
    public static readonly IfcWireForm StepZip = ByKey["step-zip"];
    public static readonly IfcWireForm Xml = ByKey["xml"];
    public static readonly IfcWireForm Json = ByKey["json"];

    // An unwrapped form is its serialization's own token, so the four landed keys are unchanged and a wrapper
    // suffixes rather than re-spells — the token a peer's two-member form struct joins on.
    public string Key => Container == IfcContainer.Plain ? Serialization.Key : $"{Serialization.Key}-{Container.Key}";

    // Two row reads answer the whole descriptor: the wrapper's extension WINS where it names one, and its raised
    // extent wins over the inner one. Matching on the extension instead splits one derivation into two arms whose
    // header column is identical and whose extent differs only by the cell the container already carries.
    public string Extension => Container.Extension.IfNone(Serialization.Extension);

    public SniffExtent Extent => Container.Raises.IfNone(Serialization.Extent);

    public int FidelityRank => Serialization.FidelityRank;

    public static Option<IfcWireForm> Route(string key) =>
        ByKey.TryGetValue(key, out IfcWireForm? form) && form is { } resolved ? Some(resolved) : None;

    public static Fin<IfcWireForm> Of(IfcSerialization serialization, IfcContainer container, Op key) =>
        Route(container == IfcContainer.Plain ? serialization.Key : $"{serialization.Key}-{container.Key}")
            .ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "ifc-form-uncontained", container.Key, serialization.Key })));

    // The PRODUCER precondition, read off the same rows a consumer reads: a form and a release the span does not
    // carry names an artifact no schema validates, and a refused serialization names one this producer never
    // authors. Sealing first and discovering the refusal at the peer is what this gate deletes.
    public Fin<Unit> Published(ReleaseVersion release, Op key) =>
        Serialization.Refusal.Match(
            Some: detail => Fin.Fail<Unit>(new BimFault.Refused(key, BimScope.Projection, BimReason.Capability,
                string.Join(':', new object?[] { detail, Key, release.Key }))),
            None: () => Serialization.Releases.Contains(release)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "ifc-form-unpublished", Key, release.Key }))));

    // The schema sniff [H8]: unwrap the container, slice the extent's window, run the row's probe, then refuse a
    // refused row BY NAME with the version it found, parse the token onto the GeometryGym release, and gate
    // membership in the frozen ReleaseMap.Lower key set (IFC4X4_DRAFT excluded by law). The container unwrap is
    // what makes a zipped payload sniffable at all: a text probe over archive bytes reported every zipped model as
    // a header miss, the silent IFC4X3_ADD2 default having masked it before that.
    public Fin<GGRelease> Sniff(ReadOnlyMemory<byte> bytes, Op key) =>
        key.Catch(() => Container.Unwrap(bytes)
            .Bind(payload => Serialization.Probe(Extent.Slice(payload)))
            .ToFin(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "schema-header", Key, Extent.Degrade })))
            .Bind(token => Serialization.Refusal.Match(
                Some: detail => Fin.Fail<GGRelease>(new BimFault.Refused(key, BimScope.Projection, BimReason.Capability,
                    string.Join(':', new object?[] { detail, Key, token }))),
                None: () => Enum.TryParse(token, ignoreCase: true, out GGRelease sniffed) && ReleaseMap.Lower.ContainsKey(sniffed)
                    ? Fin.Succ(sniffed)
                    : Fin.Fail<GGRelease>(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "schema-header", Key, "unmapped", token }))))));

    public Option<ReadOnlyMemory<byte>> Seal(DatabaseIfc target, string entry) => Serialization.Seal(target, entry);

    public Option<DatabaseIfc> Admit(ReadOnlyMemory<byte> bytes, GGRelease release) =>
        Container.Unwrap(bytes).Bind(payload => Serialization.Admit(payload, release));
}

// The four orthogonal emit axes — the diff-prior snapshot, the partial-export scope, the declared unit regime, and
// the composition's hook registry — on ONE optional carrier, so the entrypoint never grows a parallel Option tail;
// every absent axis derives its default from the graph (no prior -> ADDED, no scope -> the whole graph, no units ->
// the Header.Units declared scheme, no rail -> the unbracketed write). The rail rides the CARRIER rather than the
// observability page's optional entry-slot idiom because this entrypoint already owns one context argument.
public sealed record EmitContext(
    Option<ElementGraph> Prior = default,
    Option<ElementQuery> Scope = default,
    Option<UnitScheme> Units = default,
    Option<BimRail> Rail = default) {
    public static readonly EmitContext Whole = new();
}
```

## [03]-[RESEARCH]

(none)
