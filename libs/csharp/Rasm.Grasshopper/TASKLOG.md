# [RASM_GRASSHOPPER_TASKLOG]

Grasshopper host boundary's open and closed work, distilled from ideas and design-page RESEARCH residuals.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[0002]-[QUEUED]: Host `.api` catalogs for the Rhino-side assemblies close the folder's last unverified member routes.
- Capability: folder `.api/` catalogs for `RhinoCommon`, `Rhino.UI`, `GrasshopperIO`, and `System.Drawing.Common` at the admitted-seam depth the GH2/Eto catalogs hold.
- Shape: one catalog per assembly covering the members the design pages compose — `RhinoDoc`, `Rhino.UI.Dialogs.ShowEditBox`/`ShowNumberBox`, `EtoExtensions.UseRhinoStyle`, `IReader`/`IWriter`/`IStorable`, and the `Rhino.Geometry` port carriers.
- Unlocks: every fence member on those seams verifies against a folder catalog instead of estate memory, and the README registry claim closes.
- Anchors: `tools/assay` api decompile over the installed RhinoWIP assemblies; the landed `api-gh2-*`/`api-eto-*` catalog form.
- Atomic: four catalog files.

[0001]-[QUEUED]: GH plugin-root `HybridCache` registration discharges the session-cache app-root obligations.
- Capability: one composition-root cache profile for the GH plugin — raster-byte `IHybridCacheSerializer` admission, `MaximumPayloadBytes` sized to the largest admitted canvas raster, `ReportTagMetrics` enabled with `gh-doc:{documentId:N}` as the per-document hit/miss dimension.
- Shape: one `AddHybridCache`/`IHybridCacheBuilder` registration block at the plugin root composing the `.api/api-hybrid-cache.md` `[APP_ROOT_OBLIGATIONS]` rows.
- Unlocks: L1-only residency, sized raster caching, and per-document cache observability for every `SessionCache` consumer with zero folder edits.
- Anchors: `Shell/session.md` `SlotPolicy` rows; the folder `.api/api-hybrid-cache.md` overlay; the branch `libs/csharp/.api/api-hybrid-cache.md` registration surface.
- Tension: the APP stratum owning the GH plugin root carries no landed planning folder; this card holds the obligation until that stratum lands.
- Atomic: one registration block.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
