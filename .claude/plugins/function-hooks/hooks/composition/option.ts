// Option as a case record, fromPredicate holds the one two-way branch of the plugin

// --- [TYPES] ---------------------------------------------------------------------------

interface OptionCases<A, B> {
    readonly some: (value: A) => B;
    readonly none: () => B;
}

interface Option<A> {
    readonly match: <B>(cases: OptionCases<A, B>) => B;
}

// A record of field refinements, the shape struct reads
type Shape = Readonly<Record<string, (value: unknown) => boolean>>;

// The record type a shape decodes, each field the type its refinement narrows to
type Struct<S extends Shape> = { readonly [K in keyof S]: S[K] extends (value: unknown) => value is infer T ? T : never };

// --- [CONSTRUCTORS] --------------------------------------------------------------------

const some = <A>(value: A): Option<A> => ({ match: (cases) => cases.some(value) });

const none = <A>(): Option<A> => ({ match: (cases) => cases.none() });

// --- [REFINEMENTS] ---------------------------------------------------------------------

// A plain object, the record a shape reads its fields from
const isRecord = (value: unknown): value is Readonly<Record<string, unknown>> => typeof value === 'object' && value !== null && !Array.isArray(value);

// The refinement widened to admit an absent value, the form of an optional field in a shape
const optional =
    <T>(refinement: (value: unknown) => value is T): ((value: unknown) => value is T | undefined) =>
    (value: unknown): value is T | undefined =>
        value === undefined || refinement(value);

// One refinement over unknown from a shape of field refinements, holds on a record whose every named field passes its own
const struct =
    <S extends Shape>(shape: S): ((value: unknown) => value is Struct<S>) =>
    (value: unknown): value is Struct<S> =>
        isRecord(value) && Object.entries(shape).every(([field, refinement]) => refinement(value[field]));

// --- [OPERATIONS] ----------------------------------------------------------------------

const fromPredicate =
    <A, B extends A>(refinement: (value: A) => value is B): ((value: A) => Option<B>) =>
    (value: A): Option<B> => {
        if (refinement(value)) {
            return some(value);
        }
        return none();
    };

const fromNullable = <A>(value: A | null | undefined): Option<A> =>
    fromPredicate((candidate: A | null | undefined): candidate is A => candidate !== null && candidate !== undefined)(value);

// Boolean as an Option, the branch every file dispatches through
const fromBoolean: (condition: boolean) => Option<true> = fromPredicate((condition: boolean): condition is true => condition);

// The value of a some, else the fallback, the inner function infers the option's type and the union keeps it beside a literal fallback
const getOrElse =
    <B>(fallback: () => B): (<A>(option: Option<A>) => A | B) =>
    <A>(option: Option<A>): A | B =>
        option.match<A | B>({ some: (value) => value, none: fallback });

// The function applied under the some, a none stays a none
const map =
    <A, B>(f: (value: A) => B): ((option: Option<A>) => Option<B>) =>
    (option: Option<A>): Option<B> =>
        option.match<Option<B>>({ some: (value) => some(f(value)), none });

// The Option-returning function applied under the some, the dependent step of an Option chain
const flatMap =
    <A, B>(f: (value: A) => Option<B>): ((option: Option<A>) => Option<B>) =>
    (option: Option<A>): Option<B> =>
        option.match<Option<B>>({ some: f, none });

// The value as a some when the predicate holds and none otherwise, one call for a value present under a condition
const liftPredicate =
    <A>(predicate: (value: A) => boolean): ((value: A) => Option<A>) =>
    (value: A): Option<A> =>
        map(() => value)(fromBoolean(predicate(value)));

// One element for a some and none for a none, the shape flatMap over a list consumes
const toArray = <A>(option: Option<A>): readonly A[] => option.match<readonly A[]>({ some: (value) => [value], none: () => [] });

// The somes of an asynchronous Option-returning function over a list in list order with the nones dropped, one Promise.all over the independent calls
const traverse =
    <A, B>(f: (value: A) => Promise<Option<B>>): ((values: readonly A[]) => Promise<readonly B[]>) =>
    async (values: readonly A[]): Promise<readonly B[]> =>
        (await Promise.all(values.map(f))).flatMap(toArray);

// The asynchronous function run under the some with its value kept as a some, a none resolves as a none without a call
const forEach =
    <A, B>(f: (value: A) => Promise<B>): ((option: Option<A>) => Promise<Option<B>>) =>
    (option: Option<A>): Promise<Option<B>> =>
        option.match<Promise<Option<B>>>({ some: (value) => f(value).then(some), none: () => Promise.resolve(none()) });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Option, Struct };
export {
    flatMap,
    forEach,
    fromBoolean,
    fromNullable,
    fromPredicate,
    getOrElse,
    isRecord,
    liftPredicate,
    map,
    none,
    optional,
    some,
    struct,
    toArray,
    traverse,
};
