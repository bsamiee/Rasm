# [BIM_IFC_WIRE_FORM]

`Rasm.Bim` owns the IFC wire-form vocabulary as the SOLE GeometryGym/IFC owner: the serialization a document is written in, the container it is wrapped by, the release span each serialization publishes across, and the byte read that recovers a release from foreign bytes before any database is constructed.

Serialization and container are SEPARATE axes whose product is sparse on both crossings — a container names the serializations it wraps, a serialization names the releases it publishes for — so a form is a generated corner of that product rather than an enumerated row, and the pair `(form, release)` a producer may seal is a matrix read both ends of the wire perform.

The peer decoder at `libs/typescript/core/.planning/interchange/frame.md` models the same two axes and admits one serialization this producer refuses. That refusal is DECLARED here as a row carrying its own diagnostic — an `ifcx` payload is identified by its own header member and refused by name — rather than expressed as a missing row a reader can only observe as a header miss.

Faults return `Model/faults#FAULT_BAND` `BimFault` through their `Detail` row; the re-author that consumes a form is `Projection/egress#IFC_EGRESS`.

## [01]-[INDEX]

- [02]-[IFC_WIRE_FORM]: `IfcSerialization`/`IfcContainer`/`SniffExtent` the three axes, `IfcWireForm` the generated crossing carrying the descriptor derivations and the published matrix, `Sniff` the pre-construction release read, and `EmitContext` the emit-axis carrier.

## [02]-[IFC_WIRE_FORM]

- Owner: `IfcSerialization` the `[SmartEnum<string>]` naming each IFC document encoding beside its entry extension, its interop `FidelityRank`, the `SniffExtent` its header read costs, the `ReleaseVersion` span it publishes across, its optional producer `Refusal`, and its own `Probe`/`Seal`/`Admit` delegates; `IfcContainer` the wrapper axis carrying the serializations it `Wraps`, the extension that overrides the inner one, the extent it `Raises`, and the `Unwrap` that recovers the wrapped bytes; `SniffExtent` the release-read cost carrying its byte window and the forfeit it states; `IfcWireForm` the admitted crossing of the two axes; `EmitContext` the one emit-axis carrier.
- Cases: serializations `step` · `xml` · `json` · `ifcx`; containers `plain` · `zip`; extents `line` · `element` · `document`. The crossing `Forms` generates every pair the `Wraps` column admits — `step`, `step-zip`, `xml`, `json`, `ifcx` — and the four named handles read off it, so a new serialization or container yields its forms with no handle edit.
- Law: the two axes cross SPARSELY in both directions — `zip` wraps the two text serializations alone, so a zipped ifcJSON names a wrapper nothing defines, and each serialization publishes across the releases whose schema it was authored against, so `Published(form, release)` refuses a pair no schema validates. A fused row set expressed the first invariant only as an absent row and the second not at all, which let this producer seal an artifact its own peer decoder refuses.
- Law: `Ifc4X3` publishes under NO serialization — the ISO-approved 4.3 line carries the `IFC4X3_ADD2` identifier and every published artifact spells it, so the shared release stays in the roster to NAME that refusal rather than failing an unrecognised token at the peer.
- Law: PUBLICATION and ADMISSION are two verdicts. `Published` gates the producer, so nothing is sealed that a peer refuses; `Sniff` gates the reader and admits every release the frozen `Model/elements#IFC_CLASS` `ReleaseMap.Lower` carries, release candidates included, because reading a legacy document is a capability while writing one is a claim. Collapsing the two onto one predicate either forfeits the legacy read or publishes the draft.
- Entry: `IfcWireForm.Route()` resolves a form from its wire token and `IfcWireForm.Of(serialization, container)` admits a pair against the `Wraps` crossing; `form.Published(release)` is the emit precondition; `form.Sniff(bytes)` returns `Fin<GGRelease>` — the release the import path seeds the database with, read off the bytes BEFORE construction; `form.Seal(target, entry)`/`form.Admit(bytes, release)` are the two byte directions.
- Auto: the descriptor DERIVES from two row reads — the container's extension wins where it names one, and its raised extent wins over the serialization's own, so a zipped payload is unwrapped to its whole document before the header read instead of having a text probe run over archive bytes. `Sniff` unwraps the container, slices the extent's window, runs the serialization's `Probe`, then refuses a refused row by name, parses the token onto the GeometryGym release, and gates `ReleaseMap.Lower` membership — the `IFC4X4_DRAFT` member excluded by law — so an absent header, an unparseable token, an unadmitted release, and an unproduced serialization are four verdicts and the import never guesses 4x3 over a 2x3 file [H8].
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new serialization is one `IfcSerialization` row carrying its span and its three delegates; a new container is one `IfcContainer` row naming what it wraps; a new release span is a cell on the row that publishes it; a producer refusal is the `Refusal` column, never an absent row.
- Boundary: the write container is GeometryGym's OWN entry-extension dispatch inside `WriteStream`, so the seal takes the form's extension in the entry name and no container ladder exists on that side; the read container is this page's `Unwrap`, because GeometryGym opens no archive on the read side and its `TextReader` door elects XML or STEP by peek alone — a `_` tail handing archive bytes to the text parser reads as a malformed model rather than as a container nothing opened. A `false` writer return is REFUSAL, never an empty artifact, because an empty buffer read as a written model is the forged artifact. `Option`, not `Fin`, on both delegates: the row owns the byte act while its caller owns the fault vocabulary, so a refused write lifts under the calling `Op` rather than minting a second fault family here. The `ifcx` row carries a real header `Probe` and refusing byte delegates: the probe makes the refusal NAME the version it found, and the delegates sit beneath the `Published`/`Sniff` gates as defense-in-depth, unreachable while those gates stand. The `Exchange/format#FORMAT_AXIS` catalogue-pending IFC5 row seats on this serialization when a toolkit lands; until then no `InterchangeFormat` row reaches it and the refusal is the whole capability. Release RAISING (the contract-to-GeometryGym target) is `Projection/egress#IFC_EGRESS` `ReleaseRaise` — this page reads the lowering direction alone, and the GeometryGym `ReleaseVersion` never reaches the shared `Header`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;
using GGRelease = GeometryGym.Ifc.ReleaseVersion;
using BimHooks = Rasm.Domain.HookSet<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Bim.Projection;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SniffExtent {
    public static readonly SniffExtent Line     = new("line",     Some(HeaderWindow), "release-unknown-before-header-line");
    public static readonly SniffExtent Element  = new("element",  Some(HeaderWindow), "release-unknown-before-root-element");
    public static readonly SniffExtent Document = new("document", Option<int>.None,   "release-unknown-before-whole-document");

    const int HeaderWindow = 4096;

    public Option<int> Window { get; }

    public string Degrade { get; }

    public ReadOnlyMemory<byte> Slice(ReadOnlyMemory<byte> payload) =>
        Window.Match(Some: window => payload[..Math.Min(payload.Length, window)], None: () => payload);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IfcContainer {
    public static readonly IfcContainer Plain = new("plain", Option<string>.None, Option<SniffExtent>.None, Every, Passthrough);
    public static readonly IfcContainer Zip   = new("zip", Some(".ifczip"), Some(SniffExtent.Document), Texts, Unarchived);

    public Option<string> Extension { get; }

    public Option<SniffExtent> Raises { get; }

    [UseDelegateFromConstructor]
    public partial Seq<IfcSerialization> Wraps();

    static Seq<IfcSerialization> Every() => toSeq(IfcSerialization.Items);

    static Seq<IfcSerialization> Texts() => Seq(IfcSerialization.Step, IfcSerialization.Xml);

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
    public static readonly IfcSerialization Ifcx = new("ifcx", ".ifcx", 3, SniffExtent.Document,
        Seq(ReleaseVersion.Ifc5), Some("ifc-form-unproduced"), IfcxToken, Unproduced, Unproduced);

    public string Extension { get; }

    public int FidelityRank { get; }

    public SniffExtent Extent { get; }

    public Seq<ReleaseVersion> Releases { get; }

    public Option<string> Refusal { get; }

    [UseDelegateFromConstructor]
    public partial Option<string> Probe(ReadOnlyMemory<byte> header);

    [UseDelegateFromConstructor]
    public partial Option<ReadOnlyMemory<byte>> Seal(DatabaseIfc target, string entry);

    [UseDelegateFromConstructor]
    public partial Option<DatabaseIfc> Admit(ReadOnlyMemory<byte> bytes, GGRelease release);

    // --- [HEADER_PROBES]

    static Option<string> StepToken(ReadOnlyMemory<byte> header) =>
        Delimited(Encoding.ASCII.GetString(header.Span), "FILE_SCHEMA", StringComparison.Ordinal, opening: '\'', StepTokenEnd);

    static Option<string> XmlnsToken(ReadOnlyMemory<byte> header) =>
        Delimited(Encoding.UTF8.GetString(header.Span), "ifcXML/", StringComparison.OrdinalIgnoreCase, opening: '\0', XmlTokenEnd);

    static Option<string> JsonToken(ReadOnlyMemory<byte> header) => Member(header, "schemaIdentifier");

    static Option<string> IfcxToken(ReadOnlyMemory<byte> header) => Member(header, "header", "ifcxVersion");

    static readonly char[] XmlTokenEnd = ['/', '"', '\''];
    static readonly char[] StepTokenEnd = ['\''];

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

    static Option<string> Member(ReadOnlyMemory<byte> document, params string[] path) =>
        Optional(toSeq(path).Fold(JsonNode.Parse(document.Span), static (node, step) => node?[step])?.ToString());

    // --- [BYTE_DIRECTIONS]

    static Option<ReadOnlyMemory<byte>> Streamed(DatabaseIfc target, string entry) {
        using MemoryStream sink = new();
        return target.WriteStream(sink, entry) ? Some<ReadOnlyMemory<byte>>(sink.ToArray()) : None;
    }

    static Option<ReadOnlyMemory<byte>> Texted(DatabaseIfc target, string entry) =>
        Optional(target.ToString(FormatIfcSerialization.JSON))
            .Map(static text => (ReadOnlyMemory<byte>)Encoding.UTF8.GetBytes(text));

    static Option<DatabaseIfc> Parsed(ReadOnlyMemory<byte> bytes, GGRelease release) {
        string text = Encoding.UTF8.GetString(bytes.Span);
        if (text.Length == 0) { return None; }
        using StringReader source = new(text);
        return Some(new DatabaseIfc(source) { Release = release });
    }

    static Option<DatabaseIfc> Jsoned(ReadOnlyMemory<byte> bytes, GGRelease release) {
        if (bytes.IsEmpty) { return None; }
        using StringReader source = new(Encoding.UTF8.GetString(bytes.Span));
        DatabaseIfc target = new(release);
        target.ReadJSONFile(source);
        return Some(target);
    }

    static Option<ReadOnlyMemory<byte>> Unproduced(DatabaseIfc target, string entry) => None;

    static Option<DatabaseIfc> Unproduced(ReadOnlyMemory<byte> bytes, GGRelease release) => None;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record IfcWireForm {
    IfcWireForm(IfcSerialization serialization, IfcContainer container) {
        Serialization = serialization;
        Container = container;
    }

    public IfcSerialization Serialization { get; }

    public IfcContainer Container { get; }

    public static readonly Seq<IfcWireForm> Forms = toSeq(IfcContainer.Items)
        .Bind(container => container.Wraps().Map(serialization => new IfcWireForm(serialization, container)));

    static readonly FrozenDictionary<string, IfcWireForm> ByKey =
        Forms.ToFrozenDictionary(static form => form.Key, StringComparer.Ordinal);

    public static readonly IfcWireForm Step = ByKey["step"];
    public static readonly IfcWireForm StepZip = ByKey["step-zip"];
    public static readonly IfcWireForm Xml = ByKey["xml"];
    public static readonly IfcWireForm Json = ByKey["json"];

    public string Key => Container == IfcContainer.Plain ? Serialization.Key : $"{Serialization.Key}-{Container.Key}";

    public string Extension => Container.Extension.IfNone(Serialization.Extension);

    public SniffExtent Extent => Container.Raises.IfNone(Serialization.Extent);

    public int FidelityRank => Serialization.FidelityRank;

    public static Option<IfcWireForm> Route(string key) =>
        ByKey.TryGetValue(out IfcWireForm? form) && form is { } resolved ? Some(resolved) : None;

    public static Fin<IfcWireForm> Of(IfcSerialization serialization, IfcContainer container) =>
        Route(container == IfcContainer.Plain ? serialization.Key : $"{serialization.Key}-{container.Key}")
            .ToFin(new BimFault.Refused(BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "ifc-form-uncontained", container.Key, serialization.Key })));

    public Fin<Unit> Published(ReleaseVersion release) =>
        Serialization.Refusal.Match(
            Some: detail => Fin.Fail<Unit>(new BimFault.Refused(BimScope.Projection, BimReason.Capability,
                string.Join(':', new object?[] { detail, Key, release.Key }))),
            None: () => Serialization.Releases.Contains(release)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new BimFault.Refused(BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "ifc-form-unpublished", Key, release.Key }))));

    public Fin<GGRelease> Sniff(ReadOnlyMemory<byte> bytes) =>
        Try.lift(() => Container.Unwrap(bytes)
            .Bind(payload => Serialization.Probe(Extent.Slice(payload)))
            .ToFin(new BimFault.Refused(BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "schema-header", Key, Extent.Degrade })))
            .Bind(token => Serialization.Refusal.Match(
                Some: detail => Fin.Fail<GGRelease>(new BimFault.Refused(BimScope.Projection, BimReason.Capability,
                    string.Join(':', new object?[] { detail, Key, token }))),
                None: () => Enum.TryParse(token, ignoreCase: true, out GGRelease sniffed) && ReleaseMap.Lower.ContainsKey(sniffed)
                    ? Fin.Succ(sniffed)
                    : Fin.Fail<GGRelease>(new BimFault.Refused(BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "schema-header", Key, "unmapped", token })))))).Run().Bind(static inner => inner);

    public Option<ReadOnlyMemory<byte>> Seal(DatabaseIfc target, string entry) => Serialization.Seal(target, entry);

    public Option<DatabaseIfc> Admit(ReadOnlyMemory<byte> bytes, GGRelease release) =>
        Container.Unwrap(bytes).Bind(payload => Serialization.Admit(payload, release));
}

public sealed record EmitContext(
    Option<ElementGraph> Prior = default,
    Option<ElementQuery> Scope = default,
    Option<UnitScheme> Units = default,
    Option<BimHooks> Hooks = default) {
    public static readonly EmitContext Whole = new();
}
```

## [03]-[RESEARCH]

(none)
