# [TS_UI_API_RADIX_UI_REACT_LABEL]

`@radix-ui/react-label` is the composition-plane label primitive: one `forwardRef` component rendering `@radix-ui/react-primitive`'s `Primitive.label`, carrying exactly one behavior beyond native `<label>` passthrough — an `onMouseDown` wrapper that early-returns when the mousedown target is an interactive control (`button, input, select, textarea`), else runs the caller's `onMouseDown` first and then `preventDefault`s a multi-click (`event.detail > 1`) unless the caller already prevented it, so a double-click on the label *copy* drives the associated control instead of selecting text. It is deliberately thin: `react-aria`/`react-aria-components` own interaction, focus, and field association; `intl` owns the label string; the token plane (`class-variance-authority`/`clsx`/`tailwind-merge`) owns the class; this row owns the DOM `<label>` semantics, the `htmlFor` association feeding `Schema→aria` `FormBinding`, and the `asChild` slot merge inherited whole from `@radix-ui/react-slot`. Core `ui` only (`runtime:browser`), never `scope:viewer`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@radix-ui/react-label`
- package: `@radix-ui/react-label`
- license: `MIT`
- react-peer: `react catalog` (React 19 spine; `@types/react` `*`); depends on `@radix-ui/react-primitive@catalog` → `@radix-ui/react-slot@catalog` (`.api/radix-ui-react-slot.md`)
- catalog-verdict: KEEP
- runtime: `runtime:browser`, core `ui` — composition plane; not `scope:viewer`. `"use client"`.
- modules: single `.` barrel — `Label` / `Root` component, `LabelProps` type (verified exports: `Label`, `LabelProps`, `Root`)

```ts contract
// Verified dist/index.d.ts — LabelProps adds NO own prop; it is Primitive.label's props verbatim (asChild rides in from Primitive).
import { Primitive } from '@radix-ui/react-primitive'
interface LabelProps extends React.ComponentPropsWithoutRef<typeof Primitive.label> {}
declare const Label: React.ForwardRefExoticComponent<LabelProps & React.RefAttributes<HTMLLabelElement>>
declare const Root: typeof Label   // Root === Label, one component under two names
export { Label, type LabelProps, Root }

// Verified dist/index.mjs — the single augmented behavior: a target-scoped, caller-composed, multi-click text-guard.
onMouseDown: (event) => {
  if ((event.target as HTMLElement).closest('button, input, select, textarea')) return  // interactive target: invoke nothing
  props.onMouseDown?.(event)                                                             // caller's handler runs first (may preventDefault to opt out)
  if (!event.defaultPrevented && event.detail > 1) event.preventDefault()                // guard: kill the double-click text-selection
}
```

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: label primitive + prop contract
- rail: view/compose
- `Label` and `Root` are the same `forwardRef` component under two names (`Root` is the namespace-import idiom `Label.Root`). `LabelProps extends React.ComponentPropsWithoutRef<typeof Primitive.label>` with an EMPTY body — every native `<label>` attribute (`htmlFor`, `id`, `className`, `children`, `onMouseDown`, …) plus the `asChild?: boolean` knob, all inherited from `Primitive.label` (`PrimitivePropsWithRef<E> = ComponentPropsWithRef<E> & { asChild?: boolean }`). There is no additional own-prop; the association surface is native `htmlFor`, and the only augmented behavior is the double-click text-guard.

| [INDEX] | [SYMBOL]                                                                                     | [TYPE_FAMILY]       | [CONSUMER_BOUNDARY]                                                                   |
| :-----: | :------------------------------------------------------------------------------------------- | :------------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `Label` / `Root` (`ForwardRefExoticComponent<LabelProps & RefAttributes<HTMLLabelElement>>`) | primitive component | `view/compose` field-label row; ref forwards to the `<label>` element                 |
|  [02]   | `LabelProps` (`= ComponentPropsWithoutRef<typeof Primitive.label>`)                          | prop contract       | native `<label>` attrs + `htmlFor` association + inherited `asChild` (empty own body) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: render + associate + slot-merge
- rail: view/compose
- One component, three composition modes: bare `<Label htmlFor={id}>` associates by native `htmlFor`; `<Label asChild>` merges label behavior onto a caller element through `Primitive.label` → the `createSlot('Primitive.label')` instance; the `onMouseDown` guard is target-scoped (interactive controls opt out entirely) and caller-composed (caller runs first, guard adds `preventDefault` on multi-click).

| [INDEX] | [SURFACE]                                                                                                                 | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                                            |
| :-----: | :------------------------------------------------------------------------------------------------------------------------ | :------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `<Label htmlFor={fieldId}>{label}</Label>`                                                                                | associate      | visible label ↔ control; `htmlFor` id feeds `aria-labelledby`                                  |
|  [02]   | `<Label asChild><Slot-mergeable element/></Label>`                                                                        | slot-merge     | render-as-child via `Primitive.label`→`createSlot('Primitive.label')`; `view/compose` slot row |
|  [03]   | `<Label onMouseDown={caller}>` — guard skips interactive targets, else caller-first then `preventDefault` on `detail > 1` | text-guard     | double-click on label copy drives the control; a mousedown on a nested control is untouched    |

## [04]-[IMPLEMENTATION_LAW]

[PRIMITIVE_TOPOLOGY]:
- one component, two names: `Label === Root`; the namespace idiom (`import * as Label`) reads `Label.Root`, the flat idiom imports `Label`. A second label component family is the named defect.
- `asChild` is inherited, not owned: `LabelProps` is `Primitive.label`'s props verbatim, and `Primitive.label` is `asChild ? createSlot('Primitive.label') : 'label'` (verified `react-primitive` runtime) — so `<Label asChild>` merges through the shared `Primitive`/`Slot` machinery under the owner name `Primitive.label`, never a label-local re-clone.
- the only augmented behavior is the target-scoped multi-click `preventDefault`; label-for association, focus, and click-forwarding are native `<label>` semantics the browser already owns. The wrapper OWNS `onMouseDown` on `Primitive.label`, so an interactive-target mousedown invokes neither the caller handler nor the guard.

[INTEGRATION_LAW]:
- Stack with `@radix-ui/react-slot` (`.api/radix-ui-react-slot.md` [02]): `asChild` inherits the EXACT Slot prop/ref merge — event handlers chain (child's own first, then the label's, both fire), `style` shallow-merges (child wins), `className` concatenates (label then child), every other prop is child-wins (`{ ...labelProps, ...childProps }`), and `ref` composes via the internalized `@radix-ui/react-compose-refs`. The `onMouseDown` text-guard therefore rides that handler-chain when `asChild`, preserving the child's own `onMouseDown` — the label never re-implements reconciliation. Exactly one React-element child, enforced by the `createSlot('Primitive.label')` invariant.
- Stack with `Schema→aria` `FormBinding` (universal `Schema.standardSchemaV1(schema)`, `.api/effect.md`): a `FormBinding` derives field ids from a decoded `Schema` struct; `Label.htmlFor` binds the visible label to the control id, and the control's `aria-labelledby` resolves to the label — the accessible name flows from schema-owned ids, never a hand-wired string.
- Stack with `intl` message rows and the token plane: the label string is an `intl` catalog value keyed by the kernel `Locale` brand; `class-variance-authority`+`clsx`+`tailwind-merge` (`.api/*`) resolve `className`. Label owns neither text nor class — only the `<label>` element and its association.
- Boundary vs `react-aria-components` (`.api/react-aria-components.md`): RAC ships its OWN `Label` (fields family) that reads `LabelContext` — the context an RAC `TextField`/`Form`/field injects to push the field id, `htmlFor`, and aria wiring into the label. A radix `Label` does NOT read `LabelContext`, so a radix `Label` inside an RAC field is the double-primitive defect: it drops the field's association and the aria spine's `render` element-override. Radix `Label` serves the NON-aria styling plane (a native `<label>` + `htmlFor` off the react-aria state machine); RAC `Label` owns the label inside any RAC field. One label owner per field.

[LOCAL_ADMISSION]:
- core `ui` composition plane only; `scope:viewer` never imports it.
- one label primitive across the folder; a new label variant is a token-plane class or an `asChild` element, never a second component.
- association is native `htmlFor`; hand-wiring `aria-labelledby` when `htmlFor` suffices is the defect.
- radix `Label` off the react-aria state machine only; inside an RAC field the RAC `Label` (via `LabelContext`) is the owner — never a radix `Label` there.

[RAIL_LAW]:
- Package: `@radix-ui/react-label`
- Owns: the composition-plane `<label>` primitive, its native prop contract (`htmlFor` association + `asChild` inherited from `Primitive.label`), and the target-scoped multi-click text-selection guard
- Accept: `Label`/`Root` as the one label component, `htmlFor` as the association surface feeding `Schema→aria` `FormBinding`, `asChild` inheriting the Slot merge through `Primitive.label`→`createSlot('Primitive.label')`, `intl`-owned text, token-plane class
- Reject: a second label component family, `Label` inside an RAC field that already labels via `LabelContext`, hand-wired `aria-labelledby` where `htmlFor` binds, label-local re-implementation of slot merging or text styling, treating the guard as caller-always-runs (an interactive-target mousedown invokes neither caller nor guard)
