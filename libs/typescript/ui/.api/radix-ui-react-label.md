# [TS_UI_API_RADIX_UI_REACT_LABEL]

`@radix-ui/react-label` mints the composition-plane `<label>` primitive — one `forwardRef` over `Primitive.label` binding visible copy to its control by native `htmlFor` — adding exactly one behavior over native `<label>`: a target-scoped multi-click `preventDefault` that drives the associated control on a double-click of the label text instead of selecting it. `react-aria` owns interaction, `intl` the string, the token plane the class; this row owns the `<label>` element and its `htmlFor` association.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@radix-ui/react-label`
- package: `@radix-ui/react-label` (MIT)
- module: single `.` barrel, dual ESM/CJS, no subpaths; `"use client"`
- runtime: `runtime:browser`, core `ui` composition plane; React peer via workspace catalog
- rail: view/compose — the `<label>` primitive binding visible copy to its control

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the label primitive and its native prop contract

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]       | [CAPABILITY]                                                          |
| :-----: | :--------------- | :------------------ | :-------------------------------------------------------------------- |
|  [01]   | `Label` / `Root` | primitive component | `view/compose` field-label row; ref forwards to `HTMLLabelElement`    |
|  [02]   | `LabelProps`     | prop contract       | native `<label>` attrs + `htmlFor` + inherited `asChild` (empty body) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: associate, slot-merge, and the caller-composed text-guard

| [INDEX] | [SURFACE]                                  | [SHAPE]    | [CAPABILITY]                                                    |
| :-----: | :----------------------------------------- | :--------- | :-------------------------------------------------------------- |
|  [01]   | `<Label htmlFor={fieldId}>{label}</Label>` | associate  | visible label binds control; `htmlFor` feeds `aria-labelledby`  |
|  [02]   | `<Label asChild>{child}</Label>`           | slot-merge | render-as-child through `createSlot('Primitive.label')`         |
|  [03]   | `<Label onMouseDown={caller}>`             | text-guard | double-click drives control; nested-control mousedown untouched |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Label === Root`: one component under two names — the namespace idiom reads `Label.Root`, the flat idiom imports `Label`; a second label component is the defect.
- `asChild` is inherited, not owned: `LabelProps` is `Primitive.label`'s props verbatim, and `Primitive.label` is `asChild ? createSlot('Primitive.label') : 'label'`, so `<Label asChild>` merges through the shared `Primitive`/`Slot` machinery, never a label-local re-clone.
- `Label` owns `onMouseDown` on `Primitive.label` and adds only the target-scoped multi-click `preventDefault`; native `<label>` semantics keep association, focus, and click-forwarding, and an interactive-target mousedown invokes neither the caller handler nor the guard.

[STACKING]:
- `@radix-ui/react-slot`(`.api/radix-ui-react-slot.md`): `asChild` routes through `createSlot('Primitive.label')`, inheriting the whole Slot merge algorithm that catalog owns; the `onMouseDown` text-guard rides its handler-chain, preserving the child's own `onMouseDown`, and `createSlot('Primitive.label')` enforces exactly one React-element child.
- `effect`(`libs/typescript/.api/effect.md`): `Schema.decodeUnknown` decodes the field struct and `Schema.standardSchemaV1` binds it to the form; the schema-owned control id flows to `Label.htmlFor`, and the control's `aria-labelledby` resolves back, so the accessible name derives from schema ids rather than a hand-wired string.
- token plane + `intl`: the label string is an `intl` catalog value keyed by the kernel `Locale` brand; `class-variance-authority` + `clsx` + `tailwind-merge` resolve `className`; `Label` owns neither text nor class, only the element and its association.
- `react-aria-components`(`.api/react-aria-components.md`): RAC ships its own `Label` reading `LabelContext`, the context an RAC field injects to push field id, `htmlFor`, and aria wiring; a radix `Label` never reads `LabelContext`, so a radix `Label` inside an RAC field is the double-primitive defect that drops the field association and the aria render override. RAC `Label` owns the label inside any RAC field; radix `Label` serves the non-aria styling plane. One label owner per field.

[LOCAL_ADMISSION]:
- core `ui` composition plane only; `scope:viewer` never imports `@radix-ui/react-label`.
- one label primitive per folder; a new label variant is a token-plane class or an `asChild` element, never a second component.
- native `htmlFor` is the association surface; hand-wiring `aria-labelledby` where `htmlFor` binds is the defect.

[RAIL_LAW]:
- Package: `@radix-ui/react-label`
- Owns: the composition-plane `<label>` primitive, its native prop contract (`htmlFor` association, `asChild` inherited from `Primitive.label`), and the target-scoped multi-click text-selection guard
- Accept: `Label`/`Root` as the one label component, `htmlFor` as the association surface feeding schema-derived form ids, `asChild` inheriting the Slot merge through `Primitive.label` → `createSlot('Primitive.label')`, `intl`-owned text, token-plane class
- Reject: a second label component, a radix `Label` inside an RAC field that labels via `LabelContext`, hand-wired `aria-labelledby` where `htmlFor` binds, label-local re-implementation of slot merge or text styling, treating the guard as caller-always-runs
