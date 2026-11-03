// --- [TYPES] ---------------------------------------------------------------------------

type Fault = { readonly kind: 'exited'; readonly program: string; readonly code: number; readonly output: string } | { readonly kind: 'thrown'; readonly program: string; readonly cause: unknown };
type Faults = readonly [Fault, ...Fault[]];

// --- [MODELS] --------------------------------------------------------------------------

class Option<A> {
    static readonly none: Option<never> = new Option<never>([]);
    readonly values: readonly [] | readonly [A];

    constructor(values: readonly [] | readonly [A]) {
        this.values = values;
    }

    static some<T>(value: T): Option<T> {
        return new Option<T>([value]);
    }

    static given<T>(value: T | null | undefined): Option<T> {
        return value === null || value === undefined ? Option.none : Option.some(value);
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
    readonly outcome: { readonly kind: 'ok'; readonly value: A } | { readonly kind: 'fault'; readonly faults: Faults };

    constructor(outcome: { readonly kind: 'ok'; readonly value: A } | { readonly kind: 'fault'; readonly faults: Faults }) {
        this.outcome = outcome;
    }

    static ok<T>(value: T): Result<T> {
        return new Result<T>({ kind: 'ok', value });
    }

    static fault(...faults: Faults): Result<never> {
        return new Result<never>({ kind: 'fault', faults });
    }

    static all<T>(results: readonly Result<T>[]): Result<readonly T[]> {
        const [first, ...rest] = results.flatMap(({ outcome }) => (outcome.kind === 'fault' ? outcome.faults : []));
        return first === undefined ? Result.ok(results.flatMap((result) => result.toArray())) : Result.fault(first, ...rest);
    }

    toArray(): readonly A[] {
        return this.outcome.kind === 'ok' ? [this.outcome.value] : [];
    }

    map<B>(f: (value: A) => B): Result<B> {
        return this.outcome.kind === 'ok' ? Result.ok(f(this.outcome.value)) : Result.fault(...this.outcome.faults);
    }

    bind<R extends Result<unknown> | Promise<Result<unknown>>>(f: (value: A) => R): R | Result<never> {
        return this.outcome.kind === 'ok' ? f(this.outcome.value) : Result.fault(...this.outcome.faults);
    }

    match<B>(onOk: (value: A) => B, onFault: (faults: Faults) => B): B {
        return this.outcome.kind === 'ok' ? onOk(this.outcome.value) : onFault(this.outcome.faults);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Option, Result };
