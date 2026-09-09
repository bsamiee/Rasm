# [REACT]

Component and hook documentation: props interfaces, the component function, and the `Result` a hook returns.

## [01]-[PROPS_INTERFACES]

Each member declared on the props interface takes a one-line doc comment. An inherited member stays documented on the base interface. A member redeclared to narrow its type takes its own comment for the narrowed shape.

## [02]-[COMPONENT_FUNCTION]

The component is documented apart from its props interface, the `@param` names the interface:

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

Hooks from `@effect-atom/atom-react` (`useAtomValue`, `useAtom`) return the atom value. An atom made from an `Effect` holds `Result.Result<A, E>` from `@effect-atom/atom`: a union of `Initial`, `Success` (`value`), and `Failure` (`cause: Cause<E>`, `previousSuccess`) tagged by `_tag`, with a `waiting` flag on every case. `@returns` names the `A` and each `E`:

```ts
/**
 * Subscribes to the invoice list of the active account
 *
 * @returns `Result` of the invoices, or `InvoiceFetchError` when the API rejects the account
 */
```
