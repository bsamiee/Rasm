// --- [TYPES] ---------------------------------------------------------------------------

type Option<A> = { readonly kind: 'some'; readonly value: A } | { readonly kind: 'none' };
type Fault = { readonly kind: 'exited'; readonly subject: string; readonly code: number; readonly output: string } | { readonly kind: 'unstarted' | 'undecoded'; readonly subject: string; readonly cause: unknown };
type Result<T> = { readonly kind: 'ok'; readonly value: T } | { readonly kind: 'fault'; readonly faults: readonly [Fault, ...Fault[]] };

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CONSTRUCTORS]

const none: Option<never> = { kind: 'none' };
const some = <A>(value: A): Option<A> => ({ kind: 'some', value });
const ok = <T>(value: T): Result<T> => ({ kind: 'ok', value });
const fault = <T>(value: Fault): Result<T> => ({ kind: 'fault', faults: [value] });

// --- [COMBINATORS]

const map = <A, B>(result: Result<A>, f: (value: A) => B): Result<B> => (result.kind === 'ok' ? ok(f(result.value)) : result);
const bind = <A, R extends Result<unknown> | Promise<Result<unknown>>>(result: Result<A>, f: (value: A) => R): R | Result<never> => (result.kind === 'ok' ? f(result.value) : result);
const all = <T>(results: readonly Result<T>[]): Result<readonly T[]> => results.reduce<Result<readonly T[]>>((done, next) => (done.kind === 'ok' ? map(next, (value) => [...done.value, value]) : { kind: 'fault', faults: [...done.faults, ...(next.kind === 'fault' ? next.faults : [])] }), ok([]));

// --- [CONVERSIONS]

const decoded = <T>(subject: string, printed: Result<string>): Result<T> =>
    bind(printed, (text): Result<T> => {
        try {
            return ok(JSON.parse(text));
        } catch (cause) {
            return fault({ kind: 'undecoded', subject, cause });
        }
    });

// --- [TEXT]

const rendered = (faults: readonly Fault[]): string =>
    faults
        .map((value) => {
            const [outcome, detail] = value.kind === 'exited' ? [`exited ${value.code}`, value.output] : [{ unstarted: 'did not run', undecoded: 'printed no JSON' }[value.kind], String(value.cause)];
            return [`${value.subject} ${outcome}`, detail].filter((part) => part.length > 0).join(', ');
        })
        .join('. ');

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Option, Result };
export { all, bind, decoded, fault, map, none, ok, rendered, some };
