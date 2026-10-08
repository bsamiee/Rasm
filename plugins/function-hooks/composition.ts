import type { Option } from './hooks/state.d.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Fault =
    | { readonly kind: 'exited'; readonly subject: string; readonly code: number; readonly stderr: string }
    | { readonly kind: 'refused'; readonly subject: string; readonly text: string }
    | { readonly kind: 'unstarted' | 'unread' | 'unwritten' | 'undecoded' | 'invalid'; readonly subject: string; readonly cause: unknown };
type Result<T> = { readonly kind: 'ok'; readonly value: T } | { readonly kind: 'fault'; readonly faults: readonly [Fault, ...Fault[]] };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _LATER_LINES = /\n.*/su;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CONSTRUCTORS]

const none: Option<never> = { kind: 'none' };
const some = <A>(value: A): Option<A> => ({ kind: 'some', value });
const fromUndefined = <A>(value: A | undefined): Option<A> => (value === undefined ? none : some(value));
const ok = <T>(value: T): Result<T> => ({ kind: 'ok', value });
const fault = <T>(value: Fault): Result<T> => ({ kind: 'fault', faults: [value] });

// --- [COMBINATORS]

const map = <A, B>(result: Result<A>, f: (value: A) => B): Result<B> => (result.kind === 'ok' ? ok(f(result.value)) : result);
const bind = <A, R extends Result<unknown> | Promise<Result<unknown>>>(result: Result<A>, f: (value: A) => R): R | Result<never> => (result.kind === 'ok' ? f(result.value) : result);
const both = <A, B>(left: Result<A>, right: Result<B>): Result<readonly [A, B]> => (left.kind === 'ok' ? map(right, (value) => [left.value, value] as const) : { kind: 'fault', faults: [...left.faults, ...(right.kind === 'fault' ? right.faults : [])] });
const all = <T>(results: readonly Result<T>[]): Result<readonly T[]> => results.reduce<Result<readonly T[]>>((done, next) => map(both(done, next), ([values, value]) => [...values, value]), ok([]));

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

const counted = (count: number, noun: string, plural: string): string => `${count} ${count === 1 ? noun : plural}`;

const _described = (value: Fault): readonly [outcome: string, detail: string] => {
    switch (value.kind) {
        case 'exited':
            return [`exited ${value.code}`, value.stderr];
        case 'refused':
            return ['returned an error', value.text];
        default:
            return [{ unstarted: 'did not run', unread: 'not read', unwritten: 'not written', undecoded: 'output does not decode as JSON', invalid: 'does not hold its declared form' }[value.kind], String(value.cause)];
    }
};

const rendered = (faults: readonly Fault[]): string =>
    faults
        .map((value) => {
            const [outcome, detail] = _described(value);
            return [`${value.subject} ${outcome}`, detail.replace(_LATER_LINES, '')].filter((part) => part.length > 0).join(', ');
        })
        .join('. ');

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Option } from './hooks/state.d.ts';
export type { Fault, Result };
export { all, bind, both, counted, decoded, fault, fromUndefined, map, none, ok, rendered, some };
