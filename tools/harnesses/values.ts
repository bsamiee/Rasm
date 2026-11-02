import { identity, Match, Option, Predicate, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SCALED = /^(?<mantissa>-?\d+)p(?<power>-?\d+)$/u;
const SPECIAL: Record.ReadonlyRecord<string, number | undefined> = { undefined, NaN: Number.NaN, Infinity: Number.POSITIVE_INFINITY, '-Infinity': Number.NEGATIVE_INFINITY, '-0': -0 };

// --- [MODELS] --------------------------------------------------------------------------

const Tagged = Schema.Struct({ $: Schema.String });
const JsonObject = Schema.Record(Schema.String, Schema.Json);

// --- [OPERATIONS] ----------------------------------------------------------------------

const tagged = (tag: string): unknown =>
    Option.getOrElse(
        Option.orElse(Record.get(SPECIAL, tag), () => Option.map(Option.fromNullishOr(SCALED.exec(tag)?.groups), ({ mantissa = '0', power = '0' }) => Number(mantissa) * 2 ** Math.ceil(Number(power) / 2) * 2 ** Math.floor(Number(power) / 2))),
        () => tag,
    );
const revive = (value: Schema.Json): unknown =>
    Match.value(value).pipe(
        Match.when(Array.isArray, (items) => items.map(revive)),
        Match.when(
            (held: Schema.Json): held is typeof Tagged.Type => Schema.is(Tagged)(held) && Object.keys(held).length === 1,
            ({ $ }) => tagged($),
        ),
        Match.when(Schema.is(JsonObject), Record.map(revive)),
        Match.orElse(identity),
    );
const bury = (value: unknown): Schema.Json =>
    Match.value(value).pipe(
        Match.when(Predicate.isUndefined, () => ({ $: 'undefined' })),
        Match.when(
            (number: unknown) => Object.is(number, -0) || (Predicate.isNumber(number) && !Number.isFinite(number)),
            (number) => ({ $: Object.is(number, -0) ? '-0' : String(number) }),
        ),
        Match.when(Array.isArray, (items: readonly unknown[]) => items.map(bury)),
        Match.when(Predicate.isObject, Record.map(bury)),
        Match.whenOr(Predicate.isString, Predicate.isNumber, Predicate.isBoolean, Predicate.isNull, identity<string | number | boolean | null>),
        Match.orElse(String),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { bury, revive };
