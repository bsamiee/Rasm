// --- [TYPES] ---------------------------------------------------------------------------

type Option<A> = { readonly kind: 'some'; readonly value: A } | { readonly kind: 'none' };

// --- [CONSTRUCTORS] --------------------------------------------------------------------

const none: Option<never> = { kind: 'none' };

const some = <A>(value: A): Option<A> => ({ kind: 'some', value });

const fromNullable = <A>(value: A | undefined): Option<A> => (value === undefined ? none : some(value));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Option };
export { fromNullable, none, some };
