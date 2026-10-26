# [REACT]

Covers TSDoc on React components and hooks.

## [01]-[COMPONENT_FUNCTION]

Components are documented apart from their props interface, `@param` names the interface:

```ts
/**
 * Summary of what the component renders
 *
 * @remarks
 * Observable behavior and integration notes
 *
 * @param props - Props declared on {@link SubmitFormProps}
 * @returns Rendered form
 */
```

Components take no `@typeParam`, the props interface declares the generic.

## [02]-[HOOKS]

Hooks from `@effect/atom-react` (`useAtomValue`, `useAtom`) return the atom value. Atoms made from an `Effect` hold `AsyncResult.AsyncResult<A, E>` from `effect/reactivity`: a union of `Initial`, `Success` (`value`), and `Failure` (`cause: Cause<E>`, `previousSuccess`) tagged by `_tag`, with a `waiting` flag on every case. `@returns` names the `A` and each `E`:

```ts
/**
 * Subscribes to active account's invoice list
 *
 * @returns `AsyncResult` of invoices, or `InvoiceFetchError` when the API rejects the account
 */
```
