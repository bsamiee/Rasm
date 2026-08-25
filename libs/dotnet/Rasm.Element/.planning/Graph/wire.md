# [ELEMENT_WIRE]

`ElementWire` lowers one admitted native `Node` onto generated `NodeWire` for the persistence edit seam. `NodeWire` is support closure beneath the manifest-root `EntityEditWire`; it is not a public graph snapshot, graph delta, or relationship protocol. Native `ElementGraph`, `GraphDelta`, and `Relationship` remain branch-interior owners.

The mapping carries `NodeId` and `content_address` as the kernel's canonical 16-byte projections. Every nested value reuses its domain admission on decode, while Celly evaluates the generated corpus constraints before projection.

## [01]-[INDEX]

- [02]-[NODE_CODEC]: generated node-support transcription, union parity, shared semantic converters, and the `ElementWire.Encode`/`Decode` boundary used by persistence member patches.
- [03]-[IMPLEMENTATION_LAW]: support-closure ownership, address, admission, and evolution rules.

## [02]-[NODE_CODEC]

- Cases: `NodeWire.payload` mirrors the eight `Node` cases. Nested generated oneofs mirror `PropertyValue`, `TemporalValue`, `MaterialComposition`, and `MaterialPropertySet` only because a node payload can reach them. `CoverageSample` stays branch-interior because no node seats it.
- Law: `NodeWire` exists to make `Persistence/Version/merge#STRUCTURAL_DIFF` field-mask edits schema-aware. It does not make the enclosing graph, delta algebra, relationship algebra, headers, redaction policy, or event framing a cross-language contract.
- Law: `WireCodec` is one `[Mapper]` partial family split by generated message family. `SeamConverters` is the public identity and semantic-value converter set composed by sibling packages; no protobuf-shaped DTO or alias is added.
- Entry: `Encode(node, tolerance, key)` mints `content_address` under the caller's active graph tolerance and validates the generated result. `Decode(wire, key)` validates and re-admits every nested value, but does not claim address verification because tolerance belongs to the graph context at the persistence caller.
- Output: the caller retains producer-carried `content_address` as the held-node OCC base. It never derives that value from ProtoJSON or treats `NodeWire` as a manifest actor.
- Packages: Celly.Protovalidate validates corpus rules; Google.Protobuf owns generated messages and descriptors; Rasm owns `ContentHash.Wire`/`Admit`; Mapperly owns field transcription; Thinktecture owns total union dispatch; LanguageExt owns `Fin` and presence; NodaTime.Serialization.Protobuf owns temporal projections.
- Growth: a new seated `Node` case lands one corpus arm and one total mapping. A graph-local feature stays native unless a real manifest actor requires it; code generation never justifies widening the public contract surface.

| [INDEX] | [FAMILY]              | [WIRE_ONEOF]                           | [ARMS] | [CANON_ORDINALS] | [PROTO_FIELDS] |
| :-----: | :-------------------- | :------------------------------------- | -----: | :--------------- | :------------- |
|  [01]   | `Node`                | `NodeWire.payload`                     |      8 | `0..7`           | `2..9`         |
|  [02]   | `PropertyValue`       | `PropertyValueWire.value`              |     14 | `0..13`          | `1..14`        |
|  [03]   | `TemporalValue`       | `TemporalWire.value`                   |      5 | `0..4`           | `1..5`         |
|  [04]   | `MaterialComposition` | `MaterialCompositionWire.composition`  |      4 | `0..3`           | `1..4`         |
|  [05]   | `MaterialPropertySet` | `MaterialPropertySetWire.property_set` |     12 | `0..11`          | `2..13`        |

The corpus owns every field number. The only envelope bracket this projection must know is `NodeWire`: `id = 1`, payload arms `2..9`, and `content_address = 10`. Nested-family number law stays with the corresponding corpus file; this page carries only the arm census consumed by the codec's parity fold.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Numerics;
using System.Globalization;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using LanguageExt;
using LanguageExt.Common;
using NodaTime.Serialization.Protobuf;
// Contracts are retired from this logic.
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Properties;
using Riok.Mapperly.Abstractions;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;
using static Rasm.Element.Graph.SeamConverters;

namespace Rasm.Element.Graph;


// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CrossingFamily {
 public static readonly CrossingFamily Node = new("node", 8, static () => Arms<NodeWire.PayloadOneofCase>());
 public static readonly CrossingFamily PropertyValue = new("property-value", 14, static () => Arms<PropertyValueWire.ValueOneofCase>());
 public static readonly CrossingFamily TemporalValue = new("temporal-value", 5, static () => Arms<TemporalWire.ValueOneofCase>());
 public static readonly CrossingFamily MaterialComposition = new("material-composition", 4, static () => Arms<MaterialCompositionWire.CompositionOneofCase>());
 public static readonly CrossingFamily MaterialPropertySet = new("material-property-set", 12, static () => Arms<MaterialPropertySetWire.PropertySetOneofCase>());

 public int Arms { get; }

 [UseDelegateFromConstructor]
 public partial int WireArms();

 static int Arms<T>() where T : struct, Enum => Enum.GetValues<T>().Length - 1;
}

// --- [SERVICES] ------------------------------------------------------------------------
// --- [SERVICES] ------------------------------------------------------------------------
public static partial class SeamConverters {
 // --- [KEY_CODECS]
 [UserMapping] public static ByteString ToWire(NodeId id) =>
  ContentHash.Wire(UInt128.Parse(id.Value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture));
 [UserMapping] public static string ToWire(MaterialId id) => id.Value;
 [UserMapping] public static ByteString ToWire(UInt128 key) => ContentHash.Wire(key);

 public static Fin<UInt128> ToKey(ByteString wire, Op key) => ContentHash.Admit(wire.Span, key);

 public static Fin<NodeId> ToNodeId(ByteString wire, Op key) =>
  ContentHash.Admit(wire.Span, key)
   .Map(static value => NodeId.Create(value.ToString("X32", CultureInfo.InvariantCulture)));

 // --- [CARRIER_CODECS]
 [UserMapping] public static MeasureValueWire? ToWire(Option<MeasureValue> value) => value.Match<MeasureValueWire?>(static m => ToWire(m), static () => null);
 [UserMapping] public static SampledCurveWire? ToWire(Option<SampledCurve> curve) => curve.Match<SampledCurveWire?>(static c => WireCodec.ToWire(c), static () => null);
 [UserMapping] public static Timestamp? ToWire(Option<NodaTime.Instant> at) => at.Match<Timestamp?>(static i => i.ToTimestamp(), static () => null);

 [UserMapping] public static MeasureValueWire ToWire(MeasureValue m) {
  MeasureValueWire w = new() {
   Dimension = new DimensionWire {
    QuantityType = m.Type.Value,
    Length = m.Dimension.Length, Mass = m.Dimension.Mass, Time = m.Dimension.Time,
    Current = m.Dimension.Current, Temperature = m.Dimension.Temperature,
    Amount = m.Dimension.Amount, LuminousIntensity = m.Dimension.LuminousIntensity,
   },
   Si = m.Si,
  };
  m.Uncertainty.IfSome(b => w.Uncertainty = ToWire(b));
  return w;
 }

 public static Fin<MeasureValue> ToMeasure(MeasureValueWire? w, Op key) =>
  w is null
   ? new KernelFault.InvalidValue("element-wire.measure", "required message is absent", Some(key))
   : w.Dimension is null
    ? new KernelFault.InvalidValue("element-wire.measure.dimension", "required message is absent", Some(key))
   : from type in key.AcceptValidated<QuantityType>(w.Dimension.QuantityType)
     from measure in MeasureValue.OfSi(
      type,
      Dimension.Create(
       w.Dimension.Length, w.Dimension.Mass, w.Dimension.Time, w.Dimension.Current,
       w.Dimension.Temperature, w.Dimension.Amount, w.Dimension.LuminousIntensity),
      w.Si,
      key: key)
     from admitted in w.Uncertainty is null
      ? Fin.Succ(measure)
      : ToMeasureBand(w.Uncertainty, key).Bind(band => measure.WithUncertainty(band, key))
     select admitted;

 [UserMapping] public static MeasureBandWire ToWire(MeasureBand band) {
  MeasureBandWire w = new() {
   Kind = ToWire(band.Kind),
   LowerSi = band.LowerSi,
   UpperSi = band.UpperSi,
  };
  band.StandardDeviationSi.IfSome(sd => w.StandardDeviationSi = sd); band.CoverageFactor.IfSome(k => w.CoverageFactor = k); return w;
 }

 static Rasm.Contracts.Element.UncertaintyKind ToWire(UncertaintyKind value) => value == UncertaintyKind.Exact
  ? Rasm.Contracts.Element.UncertaintyKind.Exact
  : value == UncertaintyKind.Absolute
   ? Rasm.Contracts.Element.UncertaintyKind.Absolute
   : value == UncertaintyKind.Relative
    ? Rasm.Contracts.Element.UncertaintyKind.Relative
    : value == UncertaintyKind.Interval
     ? Rasm.Contracts.Element.UncertaintyKind.Interval
     : value == UncertaintyKind.Normal
      ? Rasm.Contracts.Element.UncertaintyKind.Normal
      : throw new UnreachableException();

 public static Fin<MeasureBand> ToMeasureBand(MeasureBandWire? w, Op key) =>
  w is null
   ? new KernelFault.InvalidValue("element-wire.measure-band", "required message is absent", Some(key))
   : (w.Kind switch {
       Rasm.Contracts.Element.UncertaintyKind.Exact => Fin.Succ(UncertaintyKind.Exact),
       Rasm.Contracts.Element.UncertaintyKind.Absolute => Fin.Succ(UncertaintyKind.Absolute),
       Rasm.Contracts.Element.UncertaintyKind.Relative => Fin.Succ(UncertaintyKind.Relative),
       Rasm.Contracts.Element.UncertaintyKind.Interval => Fin.Succ(UncertaintyKind.Interval),
       Rasm.Contracts.Element.UncertaintyKind.Normal => Fin.Succ(UncertaintyKind.Normal),
       _ => Fin.Fail<UncertaintyKind>(new KernelFault.InvalidValue(
        "element-wire.measure-band.kind", "name a defined non-default kind", Some(key))),
      }).Bind(kind => MeasureBand.Admit(
       kind, w.LowerSi, w.UpperSi,
       w.HasStandardDeviationSi ? Some(w.StandardDeviationSi) : None,
       w.HasCoverageFactor ? Some(w.CoverageFactor) : None, key));

 [UserMapping] public static ClassificationWire ToWire(Classification value) {
  ClassificationWire wire = new() { System = value.System, Code = value.Code, Edition = value.Edition };
  value.Source.IfSome(source => wire.Source = source);
  value.EditionDate.IfSome(date => wire.EditionDate = date.ToDate());
  value.Title.IfSome(title => wire.Title = title);
  return wire;
 }

 public static Fin<Classification> ToClassification(ClassificationWire? wire, Op key) =>
  wire is null
   ? new KernelFault.InvalidValue("element-wire.classification", "required message is absent", Some(key))
   : from editionDate in Optional(wire.EditionDate)
      .Traverse(date => key.Catch(() => date.ToLocalDate()))
      .As()
     from admitted in Classification.Of(
      wire.System, wire.Code, key, wire.Edition,
      source: wire.HasSource ? Some(wire.Source) : None, editionDate: editionDate,
      title: wire.HasTitle ? Some(wire.Title) : None)
     select admitted;

 [UserMapping] public static PlacementWire ToWire(PlacementTransform value) => new() {
  Location = ToWire(value.Location),
  Axis = ToWire(value.Axis),
  RefDirection = ToWire(value.RefDirection),
 };

 [UserMapping] public static PlacementWire? ToWire(Option<PlacementTransform> value) =>
  value.Match<PlacementWire?>(static placement => ToWire(placement), static () => null);

 public static Fin<PlacementTransform> ToPlacement(PlacementWire? wire, Op key) =>
  wire is null
   ? new KernelFault.InvalidValue("element-wire.placement", "required message is absent", Some(key))
   : wire.Location is null || wire.Axis is null || wire.RefDirection is null
    ? new KernelFault.InvalidValue("element-wire.placement", "carry location, axis, and ref_direction", Some(key))
    : Fin.Succ(PlacementTransform.Create(
       ToVector(wire.Location), ToVector(wire.Axis), ToVector(wire.RefDirection)));

 static VectorWire ToWire(Vector3 value) => new() { X = value.X, Y = value.Y, Z = value.Z };
 static Vector3 ToVector(VectorWire value) => new(value.X, value.Y, value.Z);
}

[Mapper(
 EnabledConversions = MappingConversionType.Constructor | MappingConversionType.ImplicitCast |
  MappingConversionType.Enumerable | MappingConversionType.Dictionary,
 RequiredMappingStrategy = RequiredMappingStrategy.Both)]
[UseStaticMapper(typeof(SeamConverters))]
[UseStaticMapper(typeof(NodaTime.Serialization.Protobuf.NodaExtensions))]
[UseStaticMapper(typeof(NodaTime.Serialization.Protobuf.ProtobufExtensions))]
internal static partial class WireCodec {
 // --- [UNION_PARITY]
 static WireCodec() {
  foreach (CrossingFamily family in CrossingFamily.Items) {
   if (family.Arms != family.WireArms()) {
    throw new InvalidOperationException($"<wire-union-parity:{family.Key}:{family.Arms}:{family.WireArms()}>");
   }
  }
 }

 static Fin<T> Iso<T>(NodaTime.Text.IPattern<T> pattern, string token, Op key) =>
  pattern.Parse(token) is { Success: true } parsed
   ? Fin.Succ(parsed.Value)
   : new KernelFault.InvalidValue("element-wire.temporal", $"parse {token}", Some(key));

 static Fin<Unit> BothOrNeither(bool left, bool right, string column, Op key) =>
  left == right ? Fin.Succ(unit) : new KernelFault.InvalidValue($"element-wire.{column}", "carry both presence columns or neither", Some(key));

 static Fin<Option<NodaTime.LocalDate>> ToDate(bool present, string iso, Op key) =>
  Opt(present, iso).Traverse(token => Iso(NodaTime.Text.LocalDatePattern.Iso, token, key)).As();

 static Fin<T> Present<T>(T? w, string column, Op key) where T : class =>
  w is not null ? Fin.Succ(w) : new KernelFault.InvalidValue($"element-wire.{column}", "required message is absent", Some(key));

 static Fin<NodaTime.Interval> ToInterval(
  Google.Protobuf.WellKnownTypes.Timestamp? start, Google.Protobuf.WellKnownTypes.Timestamp? end, string column, Op key) =>
  from opened in Present(start, $"{column}.start", key)
  from closed in Present(end, $"{column}.end", key)
  from window in opened.ToInstant() <= closed.ToInstant()
   ? Fin.Succ(new NodaTime.Interval(opened.ToInstant(), closed.ToInstant()))
   : new KernelFault.InvalidValue($"element-wire.{column}", "window start must not follow its end", Some(key))
  select window;

 static Fin<Option<MeasureValue>> OptMeasure(MeasureValueWire? w, Op key) =>
  Optional(w).Traverse(m => ToMeasure(m, key)).As();

 static Fin<Option<SampledCurve>> OptCurve(SampledCurveWire? w, Op key) =>
  Optional(w).Traverse(c => SampledCurve.Of(
   c.Points.Select(static point => point.At).ToArray(),
   c.Points.Select(static point => point.Value).ToArray(), key)).As();

 static Option<T> Opt<T>(bool present, T value) => present ? Some(value) : None;

 static Fin<Map<PropertyName, T>> Named<T>(Seq<(PropertyName Name, T Value)> pairs, Op key) =>
  pairs.Fold(Fin.Succ(Map<PropertyName, T>()), (acc, pair) => acc.Bind(m => m.ContainsKey(pair.Name)
   ? new KernelFault.InvalidValue(
      "element-wire.property-name", $"remain unique after ordinal-ignore-case admission; duplicate {pair.Name.Value}", Some(key))
   : Fin.Succ(m.Add(pair.Name, pair.Value))));

}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ElementWire {
 public static Fin<NodeWire> Encode(Node node, double tolerance, Op key) =>
  WireCodec.ToWire(node, tolerance, key).Bind(wire => WireCodec.Validate(wire, key));

 public static Fin<Node> Decode(NodeWire wire, Op key) =>
  WireCodec.Validate(wire, key).Bind(valid => key.Catch(() => WireCodec.ToNode(valid, key)));
}
```

## [03]-[IMPLEMENTATION_LAW]

- [KEY_VERBATIM_LAW]: `NodeId` and every `UInt128` key cross through `ContentHash.Wire` and re-admit through `ContentHash.Admit`; this seam owns no width or byte-order twin.
- [NODE_OCC_ADDRESS]: `content_address` mints under the caller-supplied active tolerance and is retained by the EntityEdit consumer as its OCC coordinate.
- [CODEC_DIVISION]: generated messages own structure, Celly owns corpus-authored validation, Mapperly owns field transcription, and Thinktecture/generated case enums own exhaustive dispatch.
- [BOUNDARY_SCOPE]: native graphs, deltas, and relationships have no peer decoder, so no protobuf root, registry row, event announcement, redaction wrapper, or local replacement codec exists.
- [WIRE_BYTES_LAW]: `NodeWire` bytes are transport spelling, never graph identity. Content identity remains `ContentAddress.Of(node, tolerance)`.
- [WIRE_EVOLUTION]: the corpus proto is the only wire declaration; compatible node payload growth is append-only and regenerates all bindings before consumers compile.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
