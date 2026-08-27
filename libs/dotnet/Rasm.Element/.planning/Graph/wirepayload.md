# [ELEMENT_WIRE_PAYLOAD]

`WireCodec` transcribes the generated `NodeWire` and its payload closure without a hand-owned DTO or graph snapshot format. Closed generated enums drive every decode arm; Celly evaluates the embedded corpus rules before projection, and every value then re-enters its domain owner's admission.

## [01]-[INDEX]

- [02]-[NODE_PAYLOAD]: exact generated-message transcriptions for nodes and object payloads.

## [02]-[NODE_PAYLOAD]

- Owner: one partial part of `Graph/wire#NODE_CODEC`; the generated `GraphReflection` descriptor owns `NodeWire` validation and this page owns only generated-message to domain projection.
- Cases: eight `NodeWire.PayloadCase` arms, eleven `RepresentationKind` rows, and the closed object vocabularies.
- Law: node ids and content keys cross as sixteen bytes through `ContentHash`; graph topology remains native and has no generated relationship mirror.
- Law: enum correspondence is explicit or numeric where both owners publish the same closed ordinal. No `Enum.ToString`, case folding, token parsing, or duplicate registry sits between the generated vocabulary and the domain rows.
- Law: Celly owns corpus-authored required, enum, scalar, and CEL rules. Local uniqueness gates exist only after domain admission narrows a key space, such as case-insensitive property names.
- Growth: a corpus enum or oneof change breaks an exhaustive conversion here; a domain-only invariant remains at its owner's factory, never copied into a protobuf-shaped validator.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Celly.Protovalidate;
using Google.Protobuf;
using LanguageExt;
using LanguageExt.Common;
using NodaTime.Serialization.Protobuf;
// Contracts are retired from this logic.
using Rasm.Domain;
using Rasm.Element.Properties;
// Contracts are retired from this logic.
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;
using static Rasm.Element.Graph.BoundaryConverters;

namespace Rasm.Element.Graph;

internal static partial class WireCodec {
 static readonly Validator GraphRules = new([GraphReflection.Descriptor]);

 internal static Fin<T> Validate<T>(T wire) where T : class, IMessage =>
  Try.lift(() => GraphRules.Validate(wire) switch {
   [] => Fin.Succ(wire),
   _ => Fin.Fail<T>(new KernelFault.InvalidInput(Axis: Some(wire.Descriptor.FullName))),
  }).Run().Bind(static inner => inner);

 internal static ObjectWire ToWire(Node.Object node) {
  ObjectWire wire = new() {
   Kind = ToWire(node.Kind),
   Classification = ToWire(node.Classification),
   PredefinedType = node.PredefinedType.ToValue(),
   Name = node.Name,
   Tag = node.Tag,
   Span = ToWire(node.Span),
  };
  node.ExternalId.IfSome(value => wire.ExternalId = value);
  node.ObjectType.IfSome(value => wire.ObjectType = value);
  node.History.IfSome(value => wire.History = ToWire(value));
  node.Placement.IfSome(value => wire.Placement = ToWire(value));
  wire.Classifications.AddRange(node.Classifications.Map(ToWire));
  wire.Representations.AddRange(node.Representations.ByIdentifier.Map(static pair => new RepresentationWire {
   Kind = ToWire(pair.Key),
   Key = ToWire(pair.Value),
  }));
  return wire;
 }

 static AppearanceWire ToWire(AppearanceSummary summary) => new() {
  AppearanceKey = ToWire(summary.AppearanceKey),
  BaseColorR = summary.BaseColorR,
  BaseColorG = summary.BaseColorG,
  BaseColorB = summary.BaseColorB,
  Metallic = summary.Metallic,
  Roughness = summary.Roughness,
  Opacity = summary.Opacity,
  Transmissive = summary.Transmissive,
 };

 static SchemaSpanWire ToWire(SchemaSpan span) {
  SchemaSpanWire wire = new() { IntroducedIn = ToWire(span.IntroducedIn) };
  span.RemovedIn.IfSome(value => wire.RemovedIn = ToWire(value));
  return wire;
 }

 static OwnerHistoryWire ToWire(OwnerHistory history) {
  OwnerHistoryWire wire = new() {
   OwningUser = history.OwningUser,
   OwningApplication = history.OwningApplication,
   Created = history.Created.ToTimestamp(),
   ChangeAction = ToChangeAction(history.ChangeAction),
   State = ToObjectState(history.State),
  };
  history.Modified.IfSome(value => wire.Modified = value.ToTimestamp());
  return wire;
 }

 internal static Fin<NodeWire> ToWire(Node node, double tolerance) =>
  Try.lift(() => node.Switch<Fin<NodeWire>>(
   @object: value => Fin.Succ(new() { Id = ToWire(value.Id), Object = ToWire(value) }),
   material: value => ToWire(value).Map(payload => new NodeWire { Id = ToWire(value.Id), Material = payload }),
   propertySet: value => Fin.Succ(new() { Id = ToWire(value.Id), PropertySet = ToWire(value.Bag) }),
   quantitySet: value => Fin.Succ(new() { Id = ToWire(value.Id), QuantitySet = ToWire(value.Bag) }),
   assessment: value => Fin.Succ(new() { Id = ToWire(value.Id), Assessment = ToWire(value.Payload) }),
   appearance: value => Fin.Succ(new() { Id = ToWire(value.Id), Appearance = ToWire(value.Summary) }),
   coverage: value => Fin.Succ(new() { Id = ToWire(value.Id), Coverage = ToWire(value.Grid) }),
   observation: value => Fin.Succ(new() { Id = ToWire(value.Id), Observation = ToWire(value.Series) }))
  .Map(wire => {
   wire.ContentAddress = ToWire(ContentAddress.Of(node, tolerance).ToValue());
   return wire;
  })).Run().Bind(static inner => inner);

 internal static Fin<Node> ToNode(NodeWire wire) =>
  ToNodeId(wire.Id).Bind(id => wire.PayloadCase switch {
   NodeWire.PayloadOneofCase.Object => ToObject(id, wire.Object),
   NodeWire.PayloadOneofCase.Material => ToMaterial(id, wire.Material),
   NodeWire.PayloadOneofCase.PropertySet => ToBag(wire.PropertySet).Map(bag => (Node)new Node.PropertySet(id, bag)),
   NodeWire.PayloadOneofCase.QuantitySet => ToBag(wire.QuantitySet).Map(bag => (Node)new Node.QuantitySet(id, bag)),
   NodeWire.PayloadOneofCase.Assessment => ToAssessment(wire.Assessment).Map(payload => (Node)new Node.Assessment(id, payload)),
   NodeWire.PayloadOneofCase.Appearance => ToAppearance(id, wire.Appearance),
   NodeWire.PayloadOneofCase.Coverage => ToCoverage(wire.Coverage).Map(grid => (Node)new Node.Coverage(id, grid)),
   NodeWire.PayloadOneofCase.Observation => ToObservation(wire.Observation).Map(series => (Node)new Node.Observation(id, series)),
   _ => new KernelFault.InvalidValue("element-wire.node.payload", "one payload arm is required"),
  });

 static Fin<Node> ToAppearance(NodeId id, AppearanceWire wire) =>
  from appearanceKey in ToKey(wire.AppearanceKey)
  from summary in AppearanceSummary.Rehydrate(appearanceKey, AppearanceVector.Create(
   wire.BaseColorR, wire.BaseColorG, wire.BaseColorB, wire.Metallic, wire.Roughness,
   wire.Opacity, wire.Transmissive))
  select (Node)new Node.Appearance(id, summary);

 static Fin<Node> ToObject(NodeId id, ObjectWire wire) =>
  from kind in ToObjectKind(wire.Kind)
  from classificationWire in Present(wire.Classification, "object.classification")
  from classification in ToClassification(classificationWire)
  from classifications in toSeq(wire.Classifications).TraverseM(row => ToClassification(row)).As()
  from spanWire in Present(wire.Span, "object.span")
  from span in ToSpan(spanWire)
  from representations in toSeq(wire.Representations).TraverseM(row =>
   from slot in ToRepresentationSlot(row.Kind)
   from hash in ToKey()
   select (Key: slot, Value: hash)).As()
  from representationMap in UniqueMap(representations, "object.representations")
  from history in Optional(wire.History).Traverse(value => ToHistory(value)).As()
  from placement in Optional(wire.Placement).Traverse(value => ToPlacement(value)).As()
  select (Node)new Node.Object(
   id, kind, Opt(wire.HasExternalId, wire.ExternalId), classification,
   PredefinedType.Create(wire.PredefinedType), Opt(wire.HasObjectType, wire.ObjectType),
   wire.Name, wire.Tag, new RepresentationContentHash(representationMap), history, span,
   classifications, placement);

 static Fin<SchemaSpan> ToSpan(SchemaSpanWire wire) =>
  from introduced in ToReleaseVersion(wire.IntroducedIn)
  from removed in Opt(wire.HasRemovedIn, wire.RemovedIn).Traverse(value => ToReleaseVersion(value)).As()
  select new SchemaSpan(introduced, removed);

 static Fin<OwnerHistory> ToHistory(OwnerHistoryWire wire) =>
  from created in Present(wire.Created, "owner-history.created")
  from modified in Optional(wire.Modified).Traverse(value => Fin.Succ(value.ToInstant())).As()
  from action in ToChangeAction(wire.ChangeAction)
  from state in ToObjectState(wire.State)
  select new OwnerHistory(wire.OwningUser, wire.OwningApplication, created.ToInstant(), modified, action, state);

 static Fin<Map<TKey, TValue>> UniqueMap<TKey, TValue>(Seq<(TKey Key, TValue Value)> rows, string column)
  where TKey : notnull => rows.Fold(
   Fin.Succ(Map<TKey, TValue>()),
   (state, row) => state.Bind(map => map.ContainsKey(row.Key)
    ? Fin.Fail<Map<TKey, TValue>>(new KernelFault.InvalidValue(
     $"element-wire.{column}", "keys remain unique after domain admission"))
    : Fin.Succ(map.Add(row.Key, row.Value))));

 static WireObjectKind ToWire(ObjectKind value) => value.Switch(
  occurrence: static () => WireObjectKind.Occurrence,
  type: static () => WireObjectKind.Type);

 static Fin<ObjectKind> ToObjectKind(WireObjectKind value) => value switch {
  WireObjectKind.Occurrence => Fin.Succ(ObjectKind.Occurrence),
  WireObjectKind.Type => Fin.Succ(ObjectKind.Type),
  _ => Fin.Fail<ObjectKind>(new KernelFault.InvalidInput(Axis: Some(nameof(ObjectWire.Kind)))),
 };

 static WireReleaseVersion ToWire(ReleaseVersion value) => value.Switch(
  ifc2X3: static () => WireReleaseVersion.Ifc2X3,
  ifc4: static () => WireReleaseVersion.Ifc4,
  ifc4X1: static () => WireReleaseVersion.Ifc4X1,
  ifc4X3: static () => WireReleaseVersion.Ifc4X3,
  ifc4X3Add2: static () => WireReleaseVersion.Ifc4X3Add2,
  ifc5: static () => WireReleaseVersion.Ifc5);

 static Fin<ReleaseVersion> ToReleaseVersion(WireReleaseVersion value) => value switch {
  WireReleaseVersion.Ifc2X3 => Fin.Succ(ReleaseVersion.Ifc2X3),
  WireReleaseVersion.Ifc4 => Fin.Succ(ReleaseVersion.Ifc4),
  WireReleaseVersion.Ifc4X1 => Fin.Succ(ReleaseVersion.Ifc4X1),
  WireReleaseVersion.Ifc4X3 => Fin.Succ(ReleaseVersion.Ifc4X3),
  WireReleaseVersion.Ifc4X3Add2 => Fin.Succ(ReleaseVersion.Ifc4X3Add2),
  WireReleaseVersion.Ifc5 => Fin.Succ(ReleaseVersion.Ifc5),
  _ => Fin.Fail<ReleaseVersion>(new KernelFault.InvalidInput(Axis: Some(nameof(SchemaSpanWire.IntroducedIn)))),
 };

 static WireRepresentationKind ToWire(RepresentationSlot value) => value.Switch(
  body: static () => WireRepresentationKind.Body,
  axis: static () => WireRepresentationKind.Axis,
  footPrint: static () => WireRepresentationKind.FootPrint,
  box: static () => WireRepresentationKind.Box,
  annotation: static () => WireRepresentationKind.Annotation,
  surface: static () => WireRepresentationKind.Surface,
  profile: static () => WireRepresentationKind.Profile,
  clearance: static () => WireRepresentationKind.Clearance,
  cog: static () => WireRepresentationKind.Cog,
  lighting: static () => WireRepresentationKind.Lighting,
  reference: static () => WireRepresentationKind.Reference);

 static Fin<RepresentationSlot> ToRepresentationSlot(WireRepresentationKind value) => value switch {
  WireRepresentationKind.Body => Fin.Succ(RepresentationSlot.Body),
  WireRepresentationKind.Axis => Fin.Succ(RepresentationSlot.Axis),
  WireRepresentationKind.FootPrint => Fin.Succ(RepresentationSlot.FootPrint),
  WireRepresentationKind.Box => Fin.Succ(RepresentationSlot.Box),
  WireRepresentationKind.Annotation => Fin.Succ(RepresentationSlot.Annotation),
  WireRepresentationKind.Surface => Fin.Succ(RepresentationSlot.Surface),
  WireRepresentationKind.Profile => Fin.Succ(RepresentationSlot.Profile),
  WireRepresentationKind.Clearance => Fin.Succ(RepresentationSlot.Clearance),
  WireRepresentationKind.Cog => Fin.Succ(RepresentationSlot.Cog),
  WireRepresentationKind.Lighting => Fin.Succ(RepresentationSlot.Lighting),
  WireRepresentationKind.Reference => Fin.Succ(RepresentationSlot.Reference),
  _ => Fin.Fail<RepresentationSlot>(new KernelFault.InvalidInput(Axis: Some(nameof(RepresentationWire.Kind)))),
 };

 static WireChangeAction ToChangeAction(string value) => value switch {
  "NOCHANGE" => WireChangeAction.Nochange,
  "MODIFIED" => WireChangeAction.Modified,
  "ADDED" => WireChangeAction.Added,
  "DELETED" => WireChangeAction.Deleted,
  "NOTDEFINED" => WireChangeAction.Notdefined,
  _ => throw new InvalidOperationException($"<owner-history-change-action:{value}>"),
 };

 static Fin<string> ToChangeAction(WireChangeAction value) => value switch {
  WireChangeAction.Nochange => Fin.Succ("NOCHANGE"),
  WireChangeAction.Modified => Fin.Succ("MODIFIED"),
  WireChangeAction.Added => Fin.Succ("ADDED"),
  WireChangeAction.Deleted => Fin.Succ("DELETED"),
  WireChangeAction.Notdefined => Fin.Succ("NOTDEFINED"),
  _ => Fin.Fail<string>(new KernelFault.InvalidInput(Axis: Some(nameof(OwnerHistoryWire.ChangeAction)))),
 };

 static WireObjectState ToObjectState(string value) => value switch {
  "READWRITE" => WireObjectState.Readwrite,
  "READONLY" => WireObjectState.Readonly,
  "LOCKED" => WireObjectState.Locked,
  "READWRITELOCKED" => WireObjectState.Readwritelocked,
  "READONLYLOCKED" => WireObjectState.Readonlylocked,
  _ => throw new InvalidOperationException($"<owner-history-state:{value}>"),
 };

 static Fin<string> ToObjectState(WireObjectState value) => value switch {
  WireObjectState.Readwrite => Fin.Succ("READWRITE"),
  WireObjectState.Readonly => Fin.Succ("READONLY"),
  WireObjectState.Locked => Fin.Succ("LOCKED"),
  WireObjectState.Readwritelocked => Fin.Succ("READWRITELOCKED"),
  WireObjectState.Readonlylocked => Fin.Succ("READONLYLOCKED"),
  _ => Fin.Fail<string>(new KernelFault.InvalidInput(Axis: Some(nameof(OwnerHistoryWire.State)))),
 };
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
