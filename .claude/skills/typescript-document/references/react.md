# [REACT]

Covers component and hook documentation: props interfaces, the component function, and the `Result` a hook returns.

## [01]-[PROPS_INTERFACES]

Document the props declared on the interface with a one-line doc comment per member, and leave inherited members to the base interface, except a member redeclared to narrow its type, which takes its own comment for the narrowed shape.

## [02]-[COMPONENT_FUNCTION]

Document the component apart from its props interface, the `@param` names the interface in place of repeating each prop:

```ts
/**
 * Summary of what the component renders
 *
 * @remarks
 * Observable behavior and integration notes
 *
 * @param props - Props declared on {@link SubmitFormProps}
 * @returns The rendered form
 */
```

The component takes no `@typeParam`, the props interface declares the generic.

## [03]-[HOOKS]

Hooks from `@effect-atom/atom-react` (`useAtomValue`, `useAtom`) return the atom value, and an atom made from an `Effect` holds `Result.Result<A, E>` from `@effect-atom/atom`: a union of `Initial`, `Success` (`value`), and `Failure` (`cause: Cause<E>`, `previousSuccess`) tagged by `_tag`, with a `waiting` flag on every case. `@returns` names the `A` and each `E`, the `Result` type documents the union once:

```ts
/**
 * Subscribes to the invoice list of the active account
 *
 * @returns `Result` of the invoices, or `InvoiceFetchError` when the API rejects the account
 */
```
