# Rasm.Grasshopper Agent Instructions

## Purpose

`Rasm.Grasshopper` is the canonical Grasshopper 2 boundary over `Rasm`. It is closer to `Rasm.Rhino` than to `Rasm`: it captures host API capability, preserves native semantics, and exposes a smaller, stronger boundary for plugin and UI code.

Build GH2-native component, data, and UI rails that let downstream apps stay thin. App code should declare intent, ports, outputs, component specs, and UI requests without hand-rolling GH2 lifecycle, data access, tree paths, conversion, disposal, undo, repaint, or UI-thread sequencing.

## Design Contract

- Build host boundaries, not wrapper facades. Keep GH2 APIs internal unless exposing them carries real semantic value.
- Preserve full native capability without caller ceremony. Internalize `IDataAccess`, `Pear`, `Twig`, `Tree`, `Site`, `Garden`, parameter adders, `ModularComponent`, canvas/document/editor methods, events, undo, repaint, and diagnostics.
- Keep one public route per concern. Use `Component<TSelf>` / `ComponentSpec`, `Port`, `Output`, `Shape`, `GhUi`, and typed UI operation algebras as the access rails.
- Add value above GH2. Encode lifecycle safety, typed port catalogs, side-aware registration, unit scaling, tree/path preservation, transfer/disposal evidence, undo grouping, repaint policy, read-only snapshots, and UI-thread protection.
- Keep internals functional. Use `Fin<T>`, `Validation`, `Option`, `Seq`, `TraverseM`, folds, discriminants, typed requests, and projection carriers.

## Folder Ownership

- `Components/` owns plugin/component infrastructure, static component definitions, component UI callbacks, port catalogs, port policies, input/output access, `Shape` conversion, runtime context, progress, output binding, GH2 diagnostics, and ownership transfer.
- `UI/` owns GH2 UI intents, scope resolution, canvas operations, document mutations, editor operations, input/dialog/menu/toolbar requests, native events, layout, paint overlays, wire queries, wire mutations, undo grouping, and repaint policy.
- Future folders should follow the same pattern: one GH2 concern, one public owner, typed operation data, and a single dispatch rail that protects native semantics.

## Component Rules

- Keep `IComponentDefinition<TSelf>.Definition` type-owned. GH2 calls `AddInputs` and `AddOutputs` before derived constructors complete, so component specs must not depend on mutable instance construction.
- Keep `ComponentSpec.Of(...)` as the component declaration rail. Fold explicit inputs and output-owned inputs there; dedupe by port instance identity.
- Keep `PortKind`, `Port`, `Port<TVal>`, and `PortPolicy` as the port ownership rail. Do not scatter GH2 adder logic, enum wire encoding, side support, or hidden/modular behavior into callers.
- Keep `OutputBinding` opaque. Output execution, `Flow<T>`, diagnostics, unsupported/empty handling, transfer evidence, and `Shape.DisposeUnlessTransferred` belong behind `Output`.
- Keep `Shape` as the geometry boundary token. Accept through `Rasm.Domain.Kind`, GH2 brokers, and `ConversionServer`; preserve converted geometry ownership until transfer is proven.

## UI Rules

- Keep `GhUi` as the UI intent constructor and `GrasshopperUi.Use` as the executor. Do not reintroduce `CanvasUi`, `ComponentUi`, wrapper services, or parallel UI frameworks.
- Extend existing request algebras: `CanvasOp`, `DocumentOp`, `EditorOp`, `InputRequest<T>`, `UiEvent`, `LayoutOp`, `PaintRequest<T>`, and `WireOp`.
- Preserve read-only semantics. Snapshot/query operations should not open editors, mutate documents, or repaint unless the request policy says so.
- Preserve mutation semantics. Document, layout, and wire changes must flow through the existing mutation rail with undo, repaint, action commit, and snapshot behavior owned once.
- Repaint is policy-driven: `RepaintRequest |` absorbs at `GrasshopperUi.Use` exit (`None` identity; `Canvas` beats `Scheduled`; `Object`+same-id idempotent). `CanvasOp.Invalidate` records policy only — never calls native invalidate directly.
- Subscription teardown has three tagged semantics via `SubscriptionTeardown`: `RunAlways` (default `Atom`, LIFO, repeat detach), `DetachOnce` (`detachOnce: true`, pacer/timer guards), `TokenGated` (`OwnedSubscription` token match). Compose with `Subscription.PaintPacer` / `Subscription.DisposeOnce` instead of raw `|` when mixing paint hooks and chrome subscriptions.
- `WireDrawnCache` validates a composite `WireDrawnStamp` (document hash, modifications, projection, draw inner frame). Pure viewport drift without `AfterWires` capture fails `Read` by design; subscribe `WirePaintObserve` or accept live `CaptureDrawn` fallback. Bridge scenarios need an open GH document, `EditorOp.EnsureVisible`, `WirePaintObserve` (or `OverlayPen`), and a completed `AfterWires` paint — `EnsureVisible` alone does not populate the cache in the same script turn.
- Motion has two paths: canvas paint loop (`Animated<T>` + optional `Pacer`/`MotionClock`) vs CoreAnimation cosmetics (`CosmeticIntent` on `NSView.Layer`). Both honour `MotionAccessibility.ShouldReduceMotion`; spring integrator prefers `Pacer.LastTargetTimestamp` delta with `TimeProvider` fallback on first frame / MessageLoop. Cosmetics map through content→control locus before CALayer attach; decorative motion skips under reduce-motion or differentiate-without-color.
- Use GH2/Eto/RhinoCommon only. Do not introduce WinForms, WPF, polling replacements for native events, or an Eto abstraction layer beside the current rail.
- Use `docs/system-api-map` for GH2/Rhino/System API boundary decisions, especially host-provided assemblies versus NuGet packages. Use `docs/external-libs/mathnet` only where numerical or symbolic computation adds value; preserve GH2 tree, path, coverage, and ownership semantics.

## Rasm Integration Rules

- Treat `Rasm` as the computation kernel. Consume `IAspect`, `Operation<TGeometry,TOut>`, `Analyze.Scope`, `Context`, `Requirement`, `TopologyProjection`, `ClosestHit`, and domain faults directly.
- Do not create a second local operation model for analysis. GH2 components should bridge to `Rasm` operations instead of duplicating analysis, validation, stats, coercion, or geometry ownership.
- Keep `Radyab` and other apps as thin callers. App code demonstrates usage but does not set the ceiling for the library boundary.
- Keep variable-driven policies. Defaults may exist for GH2-native behavior, but callers must be able to provide meaningful values when a workflow needs them.

## Surface Rules

- Do not add wrapper-only methods that rename GH2 calls.
- Do not expose every native knob as public parameters. Model semantic operations and hide native detail behind typed policies or intent records.
- Do not split ownership across managers, helpers, compatibility shims, or app-specific convenience APIs.
- Do not duplicate host access paths. Components own component/data flow; UI owns UI/document/canvas/wire flow.
- Do not preserve transitional names or files. Delete obsolete rails once the unified boundary owns the capability.
