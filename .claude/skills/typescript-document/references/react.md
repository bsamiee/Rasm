# [REACT]

Covers component and hook documentation: props interfaces, the component function, and the discriminated union return and product context a hook needs.

## [01]-[PROPS_INTERFACES]

Document only the props declared on this interface. The members inherited from a base interface are already documented on the base; do not redeclare or re-document them.

The one common exception is narrowing: a component may re-declare a member with its own shape. When a member is re-declared to narrow its type, give it a `/** description */` describing that component's shape.

Use a one-line `/** description */` on every own member. Use `interface` over `type = { ... }` — TypeDoc renders interfaces with full property tables, and IDE hover tooltips only show per-property docs for interfaces.

## [02]-[COMPONENT_FUNCTION]

Document the component itself separately from its props interface. The dominant convention delegates the param to the interface rather than repeating each prop:

```ts
/**
 * Summary of what the component does.
 *
 * @remarks
 * <optional prose — observable behavior, integration notes>
 *
 * @param props - See {@link SubmitFormProps}.
 * @returns The rendered <thing>.
 */
```

The component takes no `@typeParam` — the generic is pinned in the props interface, not re-exposed on the component.

## [03]-[HOOKS]

Hooks document their discriminated union return and the product context their remarks need.

### [03.1]-[DISCRIMINATED_UNION]

Every data-fetching hook returns a discriminated union. Document both branches in `@returns`:

```ts
@returns A {@link LoadingResult} while loading, or a {@link ReadyResult} once ready.
```

### [03.2]-[PRODUCT_CONTEXT]

These cannot be inferred from code alone and need human context before writing `@remarks`:
- Business rules behind field validation (why a rule exists, not just that it does)
- Error code meanings to the end user
- What the hook enables (the feature, not just the API shape)
- When fields are conditionally visible and why

If none are available, write the structural parts (`@param`, `@returns`, field list) and add a comment:

```ts
// TODO: add @remarks with product context — see [ticket/page]
```
