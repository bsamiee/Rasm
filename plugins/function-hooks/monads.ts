// --- [MODELS] --------------------------------------------------------------------------

class Maybe<A> {
    static readonly none: Maybe<never> = new Maybe<never>([]);
    static readonly some = <T>(value: T): Maybe<T> => new Maybe<T>([value]);
    static readonly fromNullable = <T>(value: T | null | undefined): Maybe<T> => (value === null || value === undefined ? Maybe.none : Maybe.some(value));
    readonly values: readonly [] | readonly [A];

    constructor(values: Maybe<A>['values']) {
        this.values = values;
    }

    map<B>(f: (value: A) => B): Maybe<B> {
        const { values } = this;
        return values.length === 1 ? Maybe.some(f(values[0])) : Maybe.none;
    }

    match<B>(onSome: (value: A) => B, onNone: () => B): B {
        const { values } = this;
        return values.length === 1 ? onSome(values[0]) : onNone();
    }
}

class Outcome<A> {
    static readonly ok = <T>(value: T): Outcome<T> => new Outcome<T>({ kind: 'ok', value });
    static readonly fault = (...faults: Faults): Outcome<never> => new Outcome<never>({ kind: 'fault', faults });
    static readonly all = <T>(results: readonly Outcome<T>[]): Outcome<readonly T[]> => {
        const [first, ...rest] = results.flatMap(({ outcome }) => (outcome.kind === 'fault' ? outcome.faults : []));
        return first === undefined ? Outcome.ok(results.flatMap((result) => result.values)) : Outcome.fault(first, ...rest);
    };
    readonly outcome: { readonly kind: 'ok'; readonly value: A } | { readonly kind: 'fault'; readonly faults: Faults };

    constructor(outcome: Outcome<A>['outcome']) {
        this.outcome = outcome;
    }

    get values(): readonly [] | readonly [A] {
        return this.outcome.kind === 'ok' ? [this.outcome.value] : [];
    }

    map<B>(f: (value: A) => B): Outcome<B> {
        const { outcome } = this;
        return outcome.kind === 'ok' ? Outcome.ok(f(outcome.value)) : Outcome.fault(...outcome.faults);
    }

    bind<R extends Outcome<unknown> | Promise<Outcome<unknown>>>(f: (value: A) => R): R | Outcome<never> {
        return this.match<R | Outcome<never>>(f, (faults) => Outcome.fault(...faults));
    }

    match<B>(onOk: (value: A) => B, onFault: (faults: Faults) => B): B {
        return this.outcome.kind === 'ok' ? onOk(this.outcome.value) : onFault(this.outcome.faults);
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

type Fault = { readonly kind: 'exited'; readonly program: string; readonly code: number; readonly output: string } | { readonly kind: 'thrown'; readonly program: string; readonly cause: unknown } | { readonly kind: 'unparsed'; readonly text: string };
type Faults = readonly [Fault, ...Fault[]];

// --- [OPERATIONS] ----------------------------------------------------------------------

const faultLine = (fault: Fault): string => {
    if (fault.kind === 'unparsed') {
        return `Bash grammar rejects \`${fault.text}\`, rewrite that span`;
    }
    return fault.kind === 'exited' ? [`${fault.program} exited ${fault.code}`, ...(fault.output === '' ? [] : [fault.output])].join('. ') : `${fault.program} failed. ${String(fault.cause)}`;
};
const faultText = (faults: Faults): string => faults.map(faultLine).join('. ');

// --- [EXPORTS] -------------------------------------------------------------------------

export { faultText, Maybe, Outcome };
