# [UI_BCF]

The BCF issue-anchor render surface over the decoded `csharp:Rasm.Bim/Review/issues#TS_PROJECTION` `BcfWire` topic family — a viewpoint-and-issue cohort anchored on element GlobalIds, floating each open topic over the `render/glb.md` canvas at its viewpoint camera and highlighting the topic's selected element set. `BcfAnchor` decodes the settled `BcfTopicWire`/`BcfCommentWire`/`BcfViewpointWire` shapes the C# branch mints through `BimWireOptions.Json`, projects each viewpoint onto the `viewport` viewpoint codec and the `render/routes.md` evidence fold, and composes the `floating-anchor.md` placement keyed to the viewpoint screen position. The surface is a projection over settled owners — it decodes the BCF wire, reuses the existing `viewport`/`observation` leaves, and re-mints neither the topic vocabulary nor a second issue shape. The overlay holds no domain state; an anchored topic reads its open state through the `binding/atom.md` `AtomBinding`.

## [01]-[INDEX]

- [01]-[BCF_ANCHOR]: the `BcfStatusKey` topic-status vocabulary, the `anchorViewpoint` viewpoint-to-element-selection projection, and the floating per-topic `Atom.family` cohort over the `BcfWire` topic family.

## [02]-[BCF_ANCHOR]

- Owner: `BcfAnchor`, the issue-anchor surface over the decoded `BcfWire` topic family; `BcfStatusKey`, the `Schema.Literal` status vocabulary decoding the C# `BcfStatus` `[JsonStringEnumMemberName]` lower-kebab discriminant (`open`/`in-progress`/`resolved`/`closed`/`reopened`) verbatim; `anchorViewpoint`, the projection folding a `BcfViewpointWire` onto the element-GlobalId selection set and the camera the `viewport` leaf applies; and the per-topic `Atom.family` subscription keyed by `BcfTopicWire.guid` so each topic memoizes one cell. The cohort renders each topic as a floating surface composing the `floating-anchor.md` placement over the `render/glb.md` canvas at the viewpoint camera, the selected GlobalIds highlighting on the `viewport` element selection.
- Cases: each topic's status keys off the decoded `BcfStatusKey` string — an `open`/`in-progress` topic renders as an active anchor, a `resolved`/`closed` topic collapses to a marker, a `reopened` topic re-activates — a keyed-domain vocabulary lookup over the status string, never a `Match` chain restating one status per arm; `anchorViewpoint` reads the `BcfViewpointWire` `selectedGlobalIds` as the element set the `viewport` highlights and the `cameraPosition`/`cameraDirection`/`cameraUpVector`/`fieldOfView` triplet as the camera the viewport applies, so a TS pick round-trips the exact `csharp:Rasm.Bim/Model/elements#ELEMENT_MODEL` `GlobalId` selection the C# viewpoint carries; a topic with multiple viewpoints folds each onto one anchor keyed by the viewpoint `guid`, the comment thread rendering as the topic's evidence fold; the cohort renders over the active surface through the `floating-anchor.md` placement keyed to the projected viewpoint position.
- Entry: the overlay subscribes through the `binding/atom.md` `AtomBinding` to a `projection` cell folding the decoded `BcfWire.topics` payload — the json-stj codec row admitting `BcfTopicWire` at `interchange/Codec/codec#DECODE_RAIL` — each topic cell one `Atom.family` arg over the topic `guid`; the cohort composes the `floating-anchor.md` placement; the anchor applies a selected viewpoint onto the `viewport` camera through the one `AtomBinding`, never a viewport handle reached directly; the overlay reads and never emits a BCF mutation — a topic status change or a new comment crosses the `interchange` `CommandGateway` as one BCF intent, never a transport directly.
- Packages: `react`, `react-dom`, `@floating-ui/react`, `@effect-atom/atom`, `effect`; no BCF/IFC codec package on this card — the `.bcfzip` archive and BCF markup XML are the C# `BcfArchive` codec, host-local, never re-implemented in the browser.
- Growth: a new BCF topic field is one column on the decoded `BcfTopicWire` decoded through the same json-stj row, never a second wire vocabulary; a new viewpoint facet (a clipping plane, an orthographic camera) lands as one field on the `anchorViewpoint` projection; a new surface the cohort overlays (the `render/geo.md` map for a geo-anchored issue) composes the same `BcfAnchor`, never a parallel issue surface; a new status is one `BcfStatusKey` literal breaking the status-vocabulary lookup at compile time.
- Boundary: a branch-side BCF topic shape or a second issue vocabulary beside the C# `BcfWire` is the named cross-language drift defect — the topic, comment, and viewpoint vocabulary is minted once in C# and decoded here; a TS-side `.bcfzip` archive read or BCF markup XML parse beside the host-local `BcfArchive` codec is the named defect — the browser decodes only the host-free `BcfWire` JSON, never the archive bytes; a viewpoint anchored on a geometry handle instead of the `Model/elements#ELEMENT_MODEL` `GlobalId` is the named defect — the anchor aligns to element GlobalIds only; a hand-rolled `getBoundingClientRect` viewpoint placement beside the `floating-anchor.md` owner is the named defect; a viewpoint camera applied by reaching the `viewport` GL context directly instead of through the `AtomBinding` camera state is the named defect; a `BcfStatus` decoded by ordinal instead of the lower-kebab string is the named seam violation; a BCF mutation dialed at a transport directly instead of the `interchange` `CommandGateway` is the named defect.

```ts contract
import type { BcfTopicWire, BcfViewpointWire } from "@rasm/interchange";
import type { CameraState } from "@rasm/ts/ui/interaction/gesture";
import { Atom } from "@effect-atom/atom";
import { Option, Schema } from "effect";

const BcfStatusKey = Schema.Literal("open", "in-progress", "resolved", "closed", "reopened");
type BcfStatusKey = Schema.Schema.Type<typeof BcfStatusKey>;

const ACTIVE_OF = {
  open: true,
  "in-progress": true,
  resolved: false,
  closed: false,
  reopened: true,
} as const satisfies Record<BcfStatusKey, boolean>;

interface BcfAnchorState {
  readonly topicGuid: string;
  readonly title: string;
  readonly active: boolean;
  readonly selection: ReadonlyArray<string>;
  readonly camera: Option.Option<CameraState>;
}

const anchorViewpoint = (viewpoint: BcfViewpointWire): { readonly selection: ReadonlyArray<string>; readonly camera: CameraState } => ({
  selection: viewpoint.selectedGlobalIds,
  camera: {
    azimuth: Math.atan2(viewpoint.cameraDirection.y, viewpoint.cameraDirection.x),
    elevation: Math.asin(viewpoint.cameraDirection.z),
    distance: Math.hypot(viewpoint.cameraPosition.x, viewpoint.cameraPosition.y, viewpoint.cameraPosition.z),
    target: [
      viewpoint.cameraPosition.x + viewpoint.cameraDirection.x,
      viewpoint.cameraPosition.y + viewpoint.cameraDirection.y,
      viewpoint.cameraPosition.z + viewpoint.cameraDirection.z,
    ] as const,
  },
});

const anchorOf = (topic: BcfTopicWire): BcfAnchorState => ({
  topicGuid: topic.guid,
  title: topic.title,
  active: ACTIVE_OF[topic.status],
  selection: topic.viewpoints.flatMap((v) => v.selectedGlobalIds),
  camera: Option.map(Option.fromNullable(topic.viewpoints[0]), (v) => anchorViewpoint(v).camera),
});

const bcfAnchorFamily = (
  topicOf: (guid: string) => Option.Option<BcfTopicWire>,
): (guid: string) => Atom.Atom<Option.Option<BcfAnchorState>> =>
  Atom.family((guid: string) => Atom.make(() => Option.map(topicOf(guid), anchorOf)));
```

The decoded `BcfTopicWire`/`BcfViewpointWire` mirror the C# `csharp:Rasm.Bim/Review/issues#TS_PROJECTION` records field-for-field — `status` is the lower-kebab `BcfStatusKey` the `ACTIVE_OF` vocabulary maps to the active flag, the camera triplet crosses as the kernel `{x,y,z}` object the `cameraPosition`/`cameraDirection`/`cameraUpVector` fields carry, and `selectedGlobalIds`/`visibleGlobalIds` cross as the element-GlobalId arrays the anchor highlights on the `viewport` selection. `anchorViewpoint` derives one `CameraState` from the viewpoint camera so the BCF camera composes the same `interaction/gesture.md` `CameraState` the viewport camera fold reads, never a parallel camera shape. The BCF status vocabulary is the keyed-domain lookup the `ACTIVE_OF` `as const satisfies Record` owns, so a new status breaks the lookup at compile time rather than aliasing into a silent active default. The `Atom.family`/`Atom.make` constructors are the `atom#ATOM_BINDING` surface; the `floating#FLOATING_ANCHOR` placement keys the issue marker to the projected viewpoint position.

RESEARCH [BCF_WIRE_DECODE]: the `BcfTopicWire`/`BcfCommentWire`/`BcfViewpointWire` `Schema.Class` decode rows are owned at `interchange/Codec/codec#DECODE_RAIL` as one json-stj codec row over the `BimWireOptions.Json` camelCase Strict emission, and the BCF cluster lands as one row on `interchange/Contract/inventory#WIRE_INVENTORY` under a `csharp:Rasm.Bim/Exchange/wire#WIRE_PROJECTION` `BimWireContext` anchor — but the `wire-inventory` cluster-to-rail map carries no `Rasm.Bim` row today (the eleven anchors are AppHost/Persistence/Compute/AppUi only) and `decode-rail` registers no BCF `Schema.Class` decode beside its proto/messagepack/json-stj rows, so the `@rasm/interchange` `BcfTopicWire`/`BcfViewpointWire` import does not yet resolve and this surface stays BLOCKED on (1) a Bim wire-inventory row binding `csharp:Rasm.Bim/Review/issues#TS_PROJECTION` to the json-stj codec and (2) the matching `decode-rail` `Schema.Class` rows. The producer-side shape is settled — the `BcfStatusKey` lower-kebab literal, the `ACTIVE_OF` status vocabulary, the `anchorViewpoint` camera projection, and the `Atom.family`/`floating-anchor` composition transcribe the C# `BcfStatus` `[JsonStringEnumMemberName]` discriminant and the `BcfViewpointWire` field set verbatim, the `BcfWire`/`BcfTopicWire`/`BcfCommentWire`/`BcfViewpointWire` `[JsonSerializable]` rows confirmed on the one `BimWireContext` at `Exchange/wire#WIRE_PROJECTION` — only the consumer-side interchange landing is the open residual.
