# [ELEMENT_WIRE_PAYLOAD]

`WireCodec`'s node and edge ENVELOPE plane of the `rasm.element.v1` crossing: the `NodeWire` eight-payload fold minting each node's authoritative content address, the `RelationshipWire` six-arm fold, the header family (`Header`/`StepHeader` with the S-E2 four-face `UnitScheme` lowering), and the object-payload codecs (`Object` shell with its presence writes, `Placement`, `Classification`, `SchemaSpan`, `OwnerHistory`, `Appearance`, `Compose`/`Assign`/`Associate`/`Void`/`Connect`/`Generic`). Decode dispatch rides the generated `PayloadCase`/`EdgeCase` closed enums; every payload re-admits through its seam factory.

## [01]-[INDEX]

- [02]-[NODE_PAYLOAD]: `NodeWire`/`RelationshipWire` envelope folds, the header and object-payload transcriptions, and their `ToNode`/`ToEdge`/`ToHeader`/`ToObject` re-admissions.

## [02]-[NODE_PAYLOAD]

- Cases: `Node` 8 arms and `Relationship` 6 arms — census rows [01]/[02] at `Graph/wire#WIRE_CODEC`.
- Law: this page is one PARTIAL PART of the `Graph/wire#WIRE_CODEC` `[Mapper]` family — the `[Mapper]` attribute, the `[UNION_PARITY]` census, the `[KEY_CODECS]`, the shared decode gates (`Present`/`Opt`/`Row`/`Named`/`Iso`/`ToInterval`/`ToDate`/`BothOrNeither`/`OptMeasure`/`OptCurve`), the `[PRESENCE_SHELLS]` and carrier-codec laws, `ElementWire`, and the frozen-number ledger all live THERE; a member landing here lands its census/ledger row there in the same edit.
- Law: every decoded value re-crosses its OWNER's admission gate — the decoder constructs no case directly and trusts no carried invariant (the `ContentAddress.Verify` distrust posture); every optional column crosses by EXPLICIT presence, never a defaulted zero, blank, or sentinel.
- Packages: Google.Protobuf, Riok.Mapperly, NodaTime.Serialization.Protobuf, LanguageExt.Core, Thinktecture.Runtime.Extensions (the generated total `Switch` encode dispatch and `TryGet` row gates) — the manifest triad rides `Graph/wire#WIRE_CODEC`.
- Growth: a new column on a family this page owns is one append-only numbered field at the corpus proto, one ledger row at `Graph/wire#WIRE_CODEC`, and one transcription member here; a new union case also lands its `CrossingFamily` arm count and its oneof mirror in the same edit — the parity census refuses a half-landed pair.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers.Binary;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using LanguageExt;
using LanguageExt.Common;
using NodaTime.Serialization.Protobuf;
using Rasm.Domain;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Geospatial;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Riok.Mapperly.Abstractions;
using Band = Rasm.Numerics.Band;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;
using static Rasm.Element.Graph.SeamConverters;

namespace Rasm.Element.Graph;

// --- [SERVICES] ---------------------------------------------------------------------------
// One partial part of the ONE `[Mapper]` WireCodec family — the attribute, the parity census, the key codecs, and
// the shared decode gates ride `Graph/wire#WIRE_CODEC`; this part owns the node/edge envelope and header/object payload transcriptions.
internal static partial class WireCodec {
 internal static partial AppearanceWire ToWire(AppearanceSummary summary);

 [MapProperty(nameof(Relationship.Assign.Subject), nameof(AssignWire.SubjectId))]
 [MapProperty(nameof(Relationship.Assign.Definition), nameof(AssignWire.DefinitionId))]
 internal static partial AssignWire ToWire(Relationship.Assign edge);

 [MapProperty(nameof(Relationship.Associate.Subject), nameof(AssociateWire.SubjectId))]
 [MapProperty(nameof(Relationship.Associate.Resource), nameof(AssociateWire.ResourceId))]
 internal static partial AssociateWire ToWire(Relationship.Associate edge);

 [MapProperty(nameof(Relationship.Void.Host), nameof(VoidWire.HostId))]
 [MapProperty(nameof(Relationship.Void.Feature), nameof(VoidWire.FeatureId))]
 internal static partial VoidWire ToWire(Relationship.Void edge);

 // Hand-owned since the S-E2 widening: the UnitScheme lowers onto THREE wire faces (the Overrides map, the axes
 // run, the culture/format pair) and a generated member map cannot fan one source member across them.
 [UserMapping] internal static HeaderWire ToWire(Header header) {
  HeaderWire w = new() {
   Schema = header.Schema.Key, View = header.View.Key, GeoReference = ToWire(header.Reference),
   Tolerance = header.Tolerance, At = header.At.ToTimestamp(), Step = ToWire(header.Step),
   Culture = header.Units.CultureName, Format = header.Units.Format,
  };
  ToWire(header.Units, w.UnitScheme);
  w.Axes.AddRange(toSeq(header.Units.Axes).Map(static pair => new UnitAxisWire {
   Axis = pair.Key.Key, Factor = pair.Value.Factor, Offset = pair.Value.Offset, Token = pair.Value.Token,
  }));
  return w;
 }

 internal static partial StepHeaderWire ToWire(StepHeader step);

 // --- [PRESENCE_SHELLS] — the seven cases carrying proto3 optional SCALAR/STRING columns. Those columns land on
 // protoc's Has*/Clear* pattern behind a NULL-REJECTING setter, so no nullable-return carrier can leave one unset —
 // a generated assignment fails on the scalar shape and throws on the string shape — and each such column is
 // [MapperIgnoreSource]-named on its generated Shell as a HAND-CROSSED member (the roster comment discriminates it
 // from a non-crossing ignore) while the ONE wrapper owns its IfSome presence writes; [UserMapping(Default = true)]
 // keeps the wrapper the pair's selected mapping beside its Shell. Envelope owns Id (NodeWire.id), so both node
 // payload mappings exclude it; AllClassifications is the node's own COMPUTED union of the primary and secondary
 // columns, both of which already cross on their own fields, so mapping it would double-store the primary.
 [MapperIgnoreSource(nameof(Node.Object.Id))]
 [MapperIgnoreSource(nameof(Node.Object.AllClassifications))]
 [MapperIgnoreSource(nameof(Node.Object.ExternalId))]
 [MapperIgnoreSource(nameof(Node.Object.ObjectType))]
 private static partial ObjectWire Shell(Node.Object node);
 [UserMapping(Default = true)] internal static ObjectWire ToWire(Node.Object node) {
  ObjectWire w = Shell(node); node.ExternalId.IfSome(v => w.ExternalId = v); node.ObjectType.IfSome(v => w.ObjectType = v); return w;
 }

 [MapProperty(nameof(Relationship.Connect.From), nameof(ConnectWire.FromId))]
 [MapProperty(nameof(Relationship.Connect.To), nameof(ConnectWire.ToId))]
 [MapperIgnoreSource(nameof(Relationship.Connect.Realizing))]
 [MapperIgnoreSource(nameof(Relationship.Connect.Interface))]
 private static partial ConnectWire Shell(Relationship.Connect edge);
 [UserMapping(Default = true)] internal static ConnectWire ToWire(Relationship.Connect edge) {
  ConnectWire w = Shell(edge); edge.Realizing.IfSome(r => w.RealizingId = r.Value); edge.Interface.IfSome(k => w.InterfaceKey = ToWire(k)); return w;
 }

 [UserMapping] internal static void ToWire(UnitScheme scheme, [MappingTarget] MapField<string, string> wire) { foreach (var (quantity, unit) in scheme.Overrides) { wire[quantity.Value] = unit; } }

 [UserMapping] internal static ClassificationWire ToWire(Classification c) {
  ClassificationWire w = new() { System = c.System, Code = c.Code, Edition = c.Edition };
  c.Source.IfSome(s => w.Source = s); c.EditionDate.IfSome(d => w.EditionDate = NodaTime.Text.LocalDatePattern.Iso.Format(d)); c.Title.IfSome(t => w.Title = t); return w;
 }

 [UserMapping] internal static void ToWire(RepresentationContentHash representations, [MappingTarget] MapField<string, ByteString> wire) { foreach (var (id, hash) in representations.ByIdentifier) { wire[id] = ToWire(hash); } }

 [UserMapping] internal static SchemaSpanWire ToWire(SchemaSpan span) {
  SchemaSpanWire w = new() { IntroducedIn = span.IntroducedIn.Key }; span.RemovedIn.IfSome(r => w.RemovedIn = r.Key); return w;
 }

 // Every column is a plain crossing once the static temporal mappers and the Option<Instant> carrier are registered,
 // so the audit row generates whole and only its ABSENCE decision stays hand-owned.
 internal static partial OwnerHistoryWire ToWire(OwnerHistory history);

 [UserMapping] internal static OwnerHistoryWire? ToWire(Option<OwnerHistory> history) => history.Match<OwnerHistoryWire?>(static h => ToWire(h), static () => null);

 // The placement frame flattens to its nine ordered columns by AUTO-FLATTEN — LocationX binds the Location.X source
 // path with zero configuration — and the frame's ABSENCE rides the same nullable-return carrier every optional
 // message crossing takes.
 internal static partial PlacementWire ToWire(PlacementTransform placement);

 [UserMapping] internal static PlacementWire? ToWire(Option<PlacementTransform> placement) => placement.Match<PlacementWire?>(static p => ToWire(p), static () => null);

 [UserMapping] internal static ComposeWire ToWire(Relationship.Compose edge) {
  ComposeWire wire = new() { WholeId = edge.Whole.Value, PartId = edge.Part.Value, SubKind = edge.SubKind.Key };
  edge.Ordinal.IfSome(ordinal => wire.Ordinal = ordinal); return wire;
 }

 [UserMapping] internal static GenericWire ToWire(Relationship.Generic edge) {
  GenericWire wire = new() { WireName = edge.WireName.Value, RelatingId = edge.Source.Value, RelatedId = edge.Target.Value };
  ToWire(edge.Attributes, wire.Attributes);
  wire.Participants.AddRange(edge.Participants.Map(participant => {
   RelationshipParticipantWire row = new() { NodeId = participant.Node.Value, Role = participant.Role.Value };
   participant.Ordinal.IfSome(ordinal => row.Ordinal = ordinal); return row;
  }));
  return wire;
 }

 // One envelope fold per union uses generated total Switch; a new case breaks compilation.
 internal static NodeWire ToWire(Node node, double tolerance) {
  NodeWire wire = node.Switch<NodeWire>(
   @object: o => new() { Id = o.Id.Value, Object = ToWire(o) },
   material: m => new() { Id = m.Id.Value, Material = ToWire(m) },
   propertySet: p => new() { Id = p.Id.Value, PropertySet = ToWire(p.Bag) },
   quantitySet: q => new() { Id = q.Id.Value, QuantitySet = ToWire(q.Bag) },
   assessment: a => new() { Id = a.Id.Value, Assessment = ToWire(a.Payload) },
   appearance: a => new() { Id = a.Id.Value, Appearance = ToWire(a.Summary) },
   coverage: c => new() { Id = c.Id.Value, Coverage = ToWire(c.Grid) },
   observation: o => new() { Id = o.Id.Value, Observation = ToWire(o.Series) });
  wire.ContentAddress = ToWire(ContentAddress.Of(node, tolerance).Value);
  return wire;
 }

 internal static RelationshipWire ToWire(Relationship edge) => edge.Switch<RelationshipWire>(
  compose: e => new() { Compose = ToWire(e) },
  assign: e => new() { Assign = ToWire(e) },
  associate: e => new() { Associate = ToWire(e) },
  connect: e => new() { Connect = ToWire(e) },
  @void: e => new() { Void = ToWire(e) },
  generic: e => new() { Generic = ToWire(e) });

 // --- [DECODE_DISPATCH] — the generated closed PayloadCase/EdgeCase/ValueCase/UsageCase enums own decode
 // dispatch (an unset case rails the kernel representation fault, and a new oneof arm surfaces as an unhandled enum member); every
 // value re-crosses the SAME seam gates an in-process author does — admitted, never trusted raw.
 internal static Fin<Node> ToNode(NodeWire w, Op key) {
  NodeId id = NodeId.Create(w.Id);                                     // verbatim — never re-derived
  return w.PayloadCase switch {
   NodeWire.PayloadOneofCase.Object => ToObject(id, w.Object, key),
   NodeWire.PayloadOneofCase.Material => ToMaterial(id, w.Material, key),
   NodeWire.PayloadOneofCase.PropertySet => ToBag(w.PropertySet, key).Map(bag => (Node)new Node.PropertySet(id, bag)),
   NodeWire.PayloadOneofCase.QuantitySet => ToBag(w.QuantitySet, key).Map(bag => (Node)new Node.QuantitySet(id, bag)),
   NodeWire.PayloadOneofCase.Assessment => ToAssessment(w.Assessment, key).Map(payload => (Node)new Node.Assessment(id, payload)),
   NodeWire.PayloadOneofCase.Appearance => AppearanceSummary.Rehydrate(
    ToKey(w.Appearance.AppearanceKey),
    AppearanceVector.Create(w.Appearance.BaseColorR, w.Appearance.BaseColorG, w.Appearance.BaseColorB,
     w.Appearance.Metallic, w.Appearance.Roughness, w.Appearance.Opacity, w.Appearance.Transmissive), key)
    .Map(summary => (Node)new Node.Appearance(id, summary)),
   NodeWire.PayloadOneofCase.Coverage => ToCoverage(w.Coverage, key).Map(grid => (Node)new Node.Coverage(id, grid)),
   NodeWire.PayloadOneofCase.Observation => ToObservation(w.Observation, key).Map(series => (Node)new Node.Observation(id, series)),
   _ => new KernelFault.InvalidValue("element-wire.node.payload", "one payload arm is required", Some(key)),
  };
 }

 internal static Fin<Relationship> ToEdge(RelationshipWire w, Op key) => w.EdgeCase switch {
  RelationshipWire.EdgeOneofCase.Compose => key.Row<string, ComposeKind>(w.Compose.SubKind)
   .Map(k => (Relationship)new Relationship.Compose(
    NodeId.Create(w.Compose.WholeId), NodeId.Create(w.Compose.PartId), k,
    Opt(w.Compose.HasOrdinal, w.Compose.Ordinal))),
  RelationshipWire.EdgeOneofCase.Assign => key.Row<string, AssignKind>(w.Assign.SubKind)
   .Map(k => (Relationship)new Relationship.Assign(NodeId.Create(w.Assign.SubjectId), NodeId.Create(w.Assign.DefinitionId), k)),
  RelationshipWire.EdgeOneofCase.Associate => ToUsage(w.Associate.Usage, key)
   .Map(u => (Relationship)new Relationship.Associate(NodeId.Create(w.Associate.SubjectId), NodeId.Create(w.Associate.ResourceId), u)),
  RelationshipWire.EdgeOneofCase.Connect => key.Row<string, ConnectKind>(w.Connect.SubKind)
   .Map(k => (Relationship)new Relationship.Connect(NodeId.Create(w.Connect.FromId), NodeId.Create(w.Connect.ToId), k,
    Opt(w.Connect.HasRealizingId, w.Connect.RealizingId).Map(NodeId.Create),
    Opt(w.Connect.HasInterfaceKey, w.Connect.InterfaceKey).Map(ToKey))),
  RelationshipWire.EdgeOneofCase.Void => key.Row<string, VoidKind>(w.Void.SubKind)
   .Map(k => (Relationship)new Relationship.Void(NodeId.Create(w.Void.HostId), NodeId.Create(w.Void.FeatureId), k)),
  RelationshipWire.EdgeOneofCase.Generic =>
   from name in key.AcceptValidated<WireName>(w.Generic.WireName)
   from attributes in ToValueMap(w.Generic.Attributes, key)
   from participants in toSeq(w.Generic.Participants).TraverseM(participant =>
    key.AcceptValidated<RoleName>(participant.Role).Map(role => new RelationshipParticipant(
     NodeId.Create(participant.NodeId), role, Opt(participant.HasOrdinal, participant.Ordinal)))).As()
   select (Relationship)new Relationship.Generic(
    name, NodeId.Create(w.Generic.RelatingId), NodeId.Create(w.Generic.RelatedId), attributes, participants),
  _ => new KernelFault.InvalidValue("element-wire.edge", "one edge arm is required", Some(key)),
 };

 // The three required header messages are INDEPENDENT admissions gated by name — proto3 message presence is
 // nullness, so each rides Present before its read — the axes run re-crosses the DimensionAxis row gate, and the
 // unit map lands through toMap: both key spaces are ordinal and the parser already deduped, so no narrowing
 // exists to gate. Units rides the INIT property (the positional slot died at the owner), the four wire faces
 // rebuilding the one S-E2 scheme; a blank format reads as the owner's own "G" default.
 internal static Fin<Header> ToHeader(HeaderWire w, Op key) =>
  (key.Row<string, ReleaseVersion>(w.Schema).ToValidation(),
     key.Row<string, ModelView>(w.View).ToValidation(),
     Present(w.GeoReference, "header.geo_reference", key).Bind(geo => ToGeoReference(geo, key)).ToValidation(),
     Present(w.At, "header.at", key).ToValidation(),
     Present(w.Step, "header.step", key).ToValidation(),
     toSeq(w.Axes).Traverse(axis =>
      key.Row<int, DimensionAxis>(axis.Axis).ToValidation()
       .Map(r => (Axis: r, Value: new UnitAxis(axis.Factor, axis.Offset, axis.Token)))).As(),
     toSeq(w.UnitScheme).Traverse(pair => key.AcceptValidated<QuantityType>(pair.Key).ToValidation()
      .Map(type => (Type: type, pair.Value))).As(),
     In(w.Tolerance, Band.Positive, "header-tolerance", key))
    .Apply((schema, view, geo, at, step, axes, units, tolerance) => new Header(schema, view, geo, tolerance, at.ToInstant(),
     new StepHeader(toSeq(step.Descriptions), step.Name, step.TimeStamp.ToInstant(), toSeq(step.Authors),
      toSeq(step.Organizations), step.Preprocessor, step.OriginatingSystem, toSeq(step.Schema))) {
     Units = new UnitScheme(
      toMap(units.Map(static pair => (pair.Type, pair.Value))),
      toMap(axes.Map(static pair => (pair.Axis, pair.Value))),
      w.Culture, w.Format.Length == 0 ? "G" : w.Format),
    }).As();

 // --- [DECODE_PAYLOADS] — per-payload re-admission over the verified seam factories.
 static Fin<Node> ToObject(NodeId id, ObjectWire w, Op key) =>
  key.Row<string, ObjectKind>(w.Kind).Bind(kind =>
   Present(w.Classification, "object.classification", key).Bind(row => ToClassification(row, key)).Bind(primary =>
    toSeq(w.Classifications).TraverseM(c => ToClassification(c, key)).As().Bind(secondary =>
     Present(w.Span, "object.span", key).Bind(s => ToSpan(s, key)).Map(span => (Node)new Node.Object(
      id, kind, Opt(w.HasExternalId, w.ExternalId), primary, PredefinedType.Create(w.PredefinedType),
      Opt(w.HasObjectType, w.ObjectType), w.Name, w.Tag,
      new RepresentationContentHash(toMap(toSeq(w.Representations).Map(static p => (p.Key, ToKey(p.Value))))),
      Optional(w.History).Map(h => new OwnerHistory(h.OwningUser, h.OwningApplication, h.Created.ToInstant(),
       Optional(h.Modified).Map(static m => m.ToInstant()), h.ChangeAction, h.State)),
      span, secondary, ToPlacement(w.Placement))))));

 // The frame re-admits through its own kernel factory: the nine columns are free reals under no seam gate (a placement
 // carries no tolerance and no invariant — the canonical-bytes exclusion at its owner is what makes it free), so
 // message presence answers the whole decision and no rail is owed. The bare Vector3 is the enclosing namespace's
 // seam coordinate, never the System.Numerics carrier the prelude also has in scope.
 static Option<PlacementTransform> ToPlacement(PlacementWire? w) =>
  w is null
   ? None
   : Some(PlacementTransform.Create(
      new Vector3(w.LocationX, w.LocationY, w.LocationZ),
      new Vector3(w.AxisX, w.AxisY, w.AxisZ),
      new Vector3(w.RefDirectionX, w.RefDirectionY, w.RefDirectionZ)));

 static Fin<Classification> ToClassification(ClassificationWire w, Op key) =>
  ToDate(w.HasEditionDate, w.EditionDate, key).Bind(editionDate =>
   Classification.Of(w.System, w.Code, key, w.Edition,
    source: Opt(w.HasSource, w.Source), editionDate: editionDate, title: Opt(w.HasTitle, w.Title)));

 static Fin<SchemaSpan> ToSpan(SchemaSpanWire w, Op key) =>
  from introduced in key.Row<string, ReleaseVersion>(w.IntroducedIn)
  from removed in Optional(w.HasRemovedIn ? w.RemovedIn : null).Traverse(value => key.Row<string, ReleaseVersion>(value)).As()
  select new SchemaSpan(introduced, removed);
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
