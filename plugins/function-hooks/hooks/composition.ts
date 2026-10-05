// --- [TYPES] ---------------------------------------------------------------------------

type Option<A> = { readonly kind: 'some'; readonly value: A } | { readonly kind: 'none' };
type Fault =
    | { readonly kind: 'exited'; readonly subject: string; readonly code: number; readonly stderr: string }
    | { readonly kind: 'unstarted' | 'unread' | 'unwritten' | 'undecoded' | 'invalid'; readonly subject: string; readonly cause: unknown };
type Result<T> = { readonly kind: 'ok'; readonly value: T } | { readonly kind: 'fault'; readonly fault: Fault };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _LATER_LINES = /\n.*/su;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CONSTRUCTORS]

const none: Option<never> = { kind: 'none' };
const some = <A>(value: A): Option<A> => ({ kind: 'some', value });
const fromUndefined = <A>(value: A | undefined): Option<A> => (value === undefined ? none : some(value));
const ok = <T>(value: T): Result<T> => ({ kind: 'ok', value });
const fault = <T>(value: Fault): Result<T> => ({ kind: 'fault', fault: value });

// --- [COMBINATORS]

const map = <A, B>(result: Result<A>, f: (value: A) => B): Result<B> => (result.kind === 'ok' ? ok(f(result.value)) : result);
const bind = <A, R extends Result<unknown> | Promise<Result<unknown>>>(result: Result<A>, f: (value: A) => R): R | Result<never> => (result.kind === 'ok' ? f(result.value) : result);
const all = <T>(results: readonly Result<T>[]): Result<readonly T[]> => results.reduce<Result<readonly T[]>>((done, next) => bind(done, (values) => map(next, (value) => [...values, value])), ok([]));

// --- [CONVERSIONS]

const decoded = <T>(subject: string, printed: Result<string>, reviver?: (key: string, value: unknown) => unknown): Result<T> =>
    bind(printed, (text): Result<T> => {
        try {
            return ok(JSON.parse(text, reviver));
        } catch (cause) {
            return fault({ kind: 'undecoded', subject, cause });
        }
    });

const rendered = (value: Fault): string => {
    const outcomes = { unstarted: 'did not run', unread: 'not read', unwritten: 'not written', undecoded: 'output does not decode as JSON', invalid: 'does not hold its declared form' } as const;
    const [outcome, detail] = value.kind === 'exited' ? [`exited ${value.code}`, value.stderr] : [outcomes[value.kind], String(value.cause)];
    return `${value.subject} ${outcome}, ${detail.replace(_LATER_LINES, '')}`;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Option, Result };
export { all, bind, decoded, fault, fromUndefined, map, none, ok, rendered, some };
