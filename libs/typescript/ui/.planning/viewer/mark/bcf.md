# [UI_BCF]

`viewer/mark/bcf.ts` renders the issue-collaboration plane: `Bcf.Topic` and `Bcf.Viewpoint` arrive DECODED through `wire/vocab` (BM:64, BM:99 — guid, closed lifecycle vocabularies, camera block, `GlobalId` selection anchors), and this module projects them — topic pins anchored in the viewport through the pure projection seam, viewpoint restore as one fold minting a `Camera.Intent` plus a `Selection.Op` from the viewpoint's carriage, and the topic board as RAC list rows whose status/priority styling is vocabulary-keyed. A missing anchor renders as evidence (the wire law: a viewpoint referencing an absent element is a re-location proof that partially fails, never a fault), and `ui` authors no BCF value — comment/status writes are app egress through `wire` encode.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                  |
| :-----: | :------------------ | :------------------------------------------------------------------------ |
|   [1]   | `ANCHOR_PINS`       | topic pins — world anchors projected to screen, DOM-anchored rendering     |
|   [2]   | `VIEWPOINT_RESTORE` | the viewpoint → camera-intent + selection-op fold with anchor evidence     |
|   [3]   | `TOPIC_BOARD`       | lifecycle-vocabulary rendering rows and the write-egress boundary          |

## [2]-[ANCHOR_PINS]

- Owner: `BcfMark.pins` — the pin projection: each open topic's primary viewpoint yields a world anchor (the viewpoint camera target, or the first resolvable selection element's centroid), projected per camera change through `Camera.anchor` (the pure `WebMercatorViewport` math on geo surfaces) or the live `map.project` seam; pins render as DOM anchors — a maplibre `Marker` on map surfaces, a floating-ui `VirtualElement` whose `getBoundingClientRect` wraps the projected point on scene surfaces — one pin mechanism per surface class, chosen by the surface row, never stacked.
- Packages: `@rasm/ts/wire/vocab` (`Bcf`), `maplibre-gl` (`Marker`), `@floating-ui/react` (`VirtualElement` anchoring for scene overlays), `viewer/geo/project` (`Camera.anchor`).
- Law: pins are projections of decoded topics — pin identity is the topic guid; a pin's screen position derives per frame/camera-settle from the anchor, and no pin holds its own position state.
- Law: pin glyph and tone key off the lifecycle vocabulary — the status→tone table below is the single styling source; a status conditional in a pin row marks the table unused.
- Law: `<model-viewer>` surfaces anchor through the element's own ray — `positionAndNormalFromPoint` mints new anchors on authoring gestures, and `updateHotspot`/`queryHotspot` carry pins as element hotspots — the embed's adapter row, same vocabulary.
- Boundary: pin interaction (press opens the topic) rides `act/gesture` discrete rows; the pin's HTML content sanitizes through `view/compose`'s gate when topic text renders rich.

```typescript
import type { Bcf } from "@rasm/ts/wire/vocab"
import { Option, type Schema } from "effect"

type _Topic = Schema.Schema.Type<typeof Bcf.Topic>
type _Viewpoint = Schema.Schema.Type<typeof Bcf.Viewpoint>
type _GlobalId = _Viewpoint["selection"][number]

declare namespace BcfMark {
  type Pin = {
    readonly guid: string
    readonly anchor: readonly [number, number]
    readonly status: _Topic["status"]
  }
}

const _pin = (
  topic: _Topic,
  viewpoint: Option.Option<_Viewpoint>,
  project: (world: readonly [number, number, number]) => readonly [number, number],
): Option.Option<BcfMark.Pin> =>
  Option.map(viewpoint, (held) => ({
    guid: topic.guid,
    anchor: project([held.camera.position[0], held.camera.position[1], held.camera.position[2]]),
    status: topic.status,
  }))
```

## [3]-[VIEWPOINT_RESTORE]

- Owner: `BcfMark.restore(viewpoint)` — one fold, two outputs and one receipt: the camera block (position/direction/up/fieldOfView — consume-only carriage per the wire law) mints one `Camera.Intent.LookAt` — eye from the position rows, target from position plus direction — that every surface class dispatches through `Camera.drive` (the map grounds it, the scene backends consume it natively); the `selection` GlobalId array mints `Selection.Op.Replace`; and the anchor receipt reports which selection ids resolved against the live model — the partial-failure evidence the operator sees.
- Law: restore never re-derives — TS computes no view geometry beyond coordinate adaptation; the viewpoint IS the proof, and a restore that "corrects" the camera is the drift defect.
- Law: the receipt is data — `{ requested, resolved, missing }` counts plus the missing id list; it renders as an evidence row (`intl/message` plural forms), never throws, and a fully-missing selection still restores the camera.
- Boundary: which elements exist is the residency ledger's fact (`viewer/scene/glb`); the intent dispatch is `viewer/geo/project`'s; the selection fold is `viewer/mark/selection`'s.

```typescript
import { Array, HashSet } from "effect"

declare namespace Restore {
  type Receipt = {
    readonly requested: number
    readonly resolved: number
    readonly missing: ReadonlyArray<_GlobalId>
  }
}

const _restore = (
  viewpoint: _Viewpoint,
  resident: HashSet.HashSet<_GlobalId>,
): { readonly select: ReadonlyArray<_GlobalId>; readonly receipt: Restore.Receipt } => {
  const [missing, resolved] = Array.partition(viewpoint.selection, (id) => HashSet.has(resident, id))
  return {
    select: resolved,
    receipt: { requested: viewpoint.selection.length, resolved: resolved.length, missing },
  }
}
```

## [4]-[TOPIC_BOARD]

- Owner: `BcfMark.tone` — the lifecycle styling vocabulary: one `as const` table keyed by the wire's closed `status` axis (`open`/`in-progress`/`resolved`/`closed`) carrying tone + glyph rows (a `LucideIcon` per status — icon-as-identity), with `priority` (`low`/`normal`/`high`/`critical`) as the second axis feeding a recipe variant; the board renders topics as RAC `GridList` rows (selection follows the collection engine), comment threads at full depth from the decoded topic, and stamps through `intl/format`'s `Format.instant` + `useDateFormatter`.
- Law: the vocabularies are closed AT THE WIRE — the tables here key off `Bcf.Topic["status"]`/`["priority"]` so a wire vocabulary change breaks these rows loudly at compile time; a locally-widened status is the named defect.
- Law: writes are egress, not state — a comment draft or status change is an app action encoded at `wire`; this module renders decoded truth and exposes intent callbacks, holding no authored BCF value.
- Growth: a new lifecycle presentation is one tone-table row; a new topic facet (labels, due dates) is a wire mirror change plus one row here.

```typescript
import type { LucideIcon } from "lucide-react"
import { CircleAlert, CircleCheck, CircleDot, CircleSlash } from "lucide-react"

const _tone = {
  open: { icon: CircleDot, tone: "accent" },
  "in-progress": { icon: CircleAlert, tone: "accent" },
  resolved: { icon: CircleCheck, tone: "success" },
  closed: { icon: CircleSlash, tone: "neutral" },
} as const satisfies Record<_Topic["status"], { readonly icon: LucideIcon; readonly tone: string }>

const BcfMark: {
  readonly pin: typeof _pin
  readonly restore: typeof _restore
  readonly tone: typeof _tone
} = {
  pin: _pin,
  restore: _restore,
  tone: _tone,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { BcfMark }
```
