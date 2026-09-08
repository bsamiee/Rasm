// Store keys and the refinements that read a stored value, one key per row under a namespace because the store has no compare-and-set

// --- [TYPES] ---------------------------------------------------------------------------

type Namespace = (typeof NAMESPACES)[number];

type Guard<T> = (value: unknown) => value is T;

// A record of field refinements and the record type it reads
type Shape = Readonly<Record<string, Guard<unknown>>>;

type Struct<S extends Shape> = { readonly [K in keyof S]: S[K] extends Guard<infer T> ? T : never };

// The value of secrets, one secret value per id
type StringRecord = Readonly<Record<string, string>>;

// The redacted text of the session's last prompt under prompt/<session>
interface Prompt {
    readonly text: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const NAMESPACES = ['secrets', 'injected', 'loaded', 'snapshot', 'dns', 'prompt'] as const;

// --- [KEYS] ----------------------------------------------------------------------------

const key = (namespace: Namespace, ...parts: readonly string[]): string => [namespace, ...parts].join('/');

// The keys under a namespace and its parts in a key list
const keys =
    (namespace: Namespace, ...parts: readonly string[]): ((all: readonly string[]) => readonly string[]) =>
    (all: readonly string[]): readonly string[] =>
        all.filter((candidate) => candidate.startsWith(`${key(namespace, ...parts)}/`));

// The id part of a key after the namespace and the parts
const suffix =
    (namespace: Namespace, ...parts: readonly string[]): ((storeKey: string) => string) =>
    (storeKey: string): string =>
        storeKey.slice(key(namespace, ...parts).length + 1);

// The ids under a namespace and its parts in a key list
const ids =
    (namespace: Namespace, ...parts: readonly string[]): ((all: readonly string[]) => readonly string[]) =>
    (all: readonly string[]): readonly string[] =>
        keys(namespace, ...parts)(all).map(suffix(namespace, ...parts));

// --- [REFINEMENTS] ---------------------------------------------------------------------

const isString: Guard<string> = (value): value is string => typeof value === 'string';

const isRecord: Guard<Readonly<Record<string, unknown>>> = (value): value is Readonly<Record<string, unknown>> =>
    typeof value === 'object' && value !== null && !Array.isArray(value);

// One refinement over unknown from a shape of field refinements, holds on a record whose every named field passes its own
const struct =
    <S extends Shape>(shape: S): Guard<Struct<S>> =>
    (value): value is Struct<S> =>
        isRecord(value) && Object.entries(shape).every(([field, guard]) => guard(value[field]));

const isStringRecord: Guard<StringRecord> = (value): value is StringRecord => isRecord(value) && Object.values(value).every(isString);

const isPrompt: Guard<Prompt> = struct({ text: isString });

// --- [DECODERS] ------------------------------------------------------------------------

// The value under a refinement, undefined for a value outside it
const decode =
    <T>(guard: Guard<T>): ((value: unknown) => T | undefined) =>
    (value: unknown): T | undefined =>
        guard(value) ? value : undefined;

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Guard, Namespace, Prompt, StringRecord };
export { decode, ids, isPrompt, isRecord, isString, isStringRecord, key, keys, NAMESPACES, struct, suffix };
