// --- [MODELS] --------------------------------------------------------------------------

class Option<A> {
    static readonly none: Option<never> = new Option<never>([]);
    static readonly some = <T>(value: T): Option<T> => new Option<T>([value]);
    static readonly fromNullable = <T>(value: T | null | undefined): Option<T> => (value === null || value === undefined ? Option.none : Option.some(value));
    readonly values: readonly [] | readonly [A];

    constructor(values: Option<A>['values']) {
        this.values = values;
    }

    toArray(): readonly A[] {
        return this.values;
    }

    map<B>(f: (value: A) => B): Option<B> {
        const { values } = this;
        return values.length === 1 ? Option.some(f(values[0])) : Option.none;
    }

    match<B>(onSome: (value: A) => B, onNone: () => B): B {
        const { values } = this;
        return values.length === 1 ? onSome(values[0]) : onNone();
    }
}

class Result<A> {
    static readonly ok = <T>(value: T): Result<T> => new Result<T>({ kind: 'ok', value });
    static readonly fault = (...faults: Faults): Result<never> => new Result<never>({ kind: 'fault', faults });
    static readonly all = <T>(results: readonly Result<T>[]): Result<readonly T[]> => {
        const [first, ...rest] = results.flatMap(({ outcome }) => (outcome.kind === 'fault' ? outcome.faults : []));
        return first === undefined ? Result.ok(results.flatMap((result) => result.toArray())) : Result.fault(first, ...rest);
    };
    readonly outcome: { readonly kind: 'ok'; readonly value: A } | { readonly kind: 'fault'; readonly faults: Faults };

    constructor(outcome: Result<A>['outcome']) {
        this.outcome = outcome;
    }

    toArray(): readonly A[] {
        return this.outcome.kind === 'ok' ? [this.outcome.value] : [];
    }

    map<B>(f: (value: A) => B): Result<B> {
        const { outcome } = this;
        return outcome.kind === 'ok' ? Result.ok(f(outcome.value)) : Result.fault(...outcome.faults);
    }

    bind<R extends Result<unknown> | Promise<Result<unknown>>>(f: (value: A) => R): R | Result<never> {
        return this.match<R | Result<never>>(f, (faults) => Result.fault(...faults));
    }

    match<B>(onOk: (value: A) => B, onFault: (faults: Faults) => B): B {
        return this.outcome.kind === 'ok' ? onOk(this.outcome.value) : onFault(this.outcome.faults);
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

type Fault = { readonly kind: 'exited'; readonly program: string; readonly code: number; readonly output: string } | { readonly kind: 'thrown'; readonly program: string; readonly cause: unknown };
type Faults = readonly [Fault, ...Fault[]];

// --- [EXPORTS] -------------------------------------------------------------------------

export { Option, Result };
