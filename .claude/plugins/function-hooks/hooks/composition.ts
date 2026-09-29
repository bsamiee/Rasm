// --- [TYPES] ---------------------------------------------------------------------------

type Option<A> = { readonly kind: 'some'; readonly value: A } | { readonly kind: 'none' };
type Fault =
    | { readonly kind: 'exited'; readonly subject: string; readonly code: number; readonly stderr: string }
    | { readonly kind: 'unstarted' | 'unwritten' | 'undecoded'; readonly subject: string; readonly cause: unknown };
type Result<T> = { readonly kind: 'ok'; readonly value: T } | { readonly kind: 'fault'; readonly fault: Fault };

// --- [CONSTRUCTORS] --------------------------------------------------------------------

const none: Option<never> = { kind: 'none' };

const some = <A>(value: A): Option<A> => ({ kind: 'some', value });
const fromUndefined = <A>(value: A | undefined): Option<A> => (value === undefined ? none : some(value));
const ok = <T>(value: T): Result<T> => ({ kind: 'ok', value });
const fault = <T>(value: Fault): Result<T> => ({ kind: 'fault', fault: value });

// --- [OPERATIONS] ----------------------------------------------------------------------

const map = <A, B>(result: Result<A>, f: (value: A) => B): Result<B> => (result.kind === 'ok' ? ok(f(result.value)) : result);
const bind = <A, R extends Result<unknown> | Promise<Result<unknown>>>(result: Result<A>, f: (value: A) => R): R | Result<never> => (result.kind === 'ok' ? f(result.value) : result);
const all = <T>(results: readonly Result<T>[]): Result<readonly T[]> => results.reduce<Result<readonly T[]>>((done, next) => bind(done, (values) => map(next, (value) => [...values, value])), ok([]));

const decoded = <T>(subject: string, printed: Result<string>): Result<T> =>
    bind(printed, (text): Result<T> => {
        try {
            return ok(JSON.parse(text));
        } catch (cause) {
            return fault({ kind: 'undecoded', subject, cause });
        }
    });

const rendered = (value: Fault): string => {
    const outcomes = { unstarted: 'did not run', unwritten: 'not written', undecoded: 'output does not decode as JSON' } as const;
    return value.kind === 'exited' ? `${value.subject} exited ${value.code}, ${value.stderr}` : `${value.subject} ${outcomes[value.kind]}, ${String(value.cause)}`;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Option, Result };
export { all, bind, decoded, fault, fromUndefined, map, none, ok, rendered, some };
