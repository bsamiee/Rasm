import { DateTime, identity, Match, Option, Predicate, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Kind = typeof Kinds.Type;
interface Reference {
    readonly kind: Kind;
    readonly path: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const KINDS = ['enum', 'object', 'before', 'read'] as const;
const NUMBERS: Record.ReadonlyRecord<string, number | undefined> = { undefined, NaN: Number.NaN, Infinity: Number.POSITIVE_INFINITY, '-Infinity': Number.NEGATIVE_INFINITY, '-0': -0 };
const SCALED = /^(?<mantissa>-?\d+)p(?<power>-?\d+)$/u;
const DATED = /^date:(?<instant>.+)$/u;
const REFERENCE = new RegExp(`^(?<kind>${KINDS.join('|')}):(?<path>.+)$`, 'u');

// --- [MODELS] --------------------------------------------------------------------------

const Kinds = Schema.Literals(KINDS);
const Tagged = Schema.Struct({ $: Schema.String });
const JsonObject = Schema.Record(Schema.String, Schema.Json);

// --- [OPERATIONS] ----------------------------------------------------------------------

const tagged = (tag: string, located: (reference: Reference) => unknown): unknown =>
    Option.getOrElse(
        Option.firstSomeOf<unknown>([
            Record.get(NUMBERS, tag),
            Option.map(Option.fromNullishOr(SCALED.exec(tag)?.groups), ({ mantissa = '0', power = '0' }) => Number(mantissa) * 2 ** Math.ceil(Number(power) / 2) * 2 ** Math.floor(Number(power) / 2)),
            Option.flatMap(Option.fromNullishOr(DATED.exec(tag)?.groups?.instant), (instant) => Option.map(DateTime.make(instant), DateTime.toDateUtc)),
            Option.flatMap(Option.fromNullishOr(REFERENCE.exec(tag)?.groups), ({ kind, path = '' }) => Option.map(Schema.decodeUnknownOption(Kinds)(kind), (held) => located({ kind: held, path }))),
        ]),
        () => tag,
    );
const revive = (value: Schema.Json, located: (reference: Reference) => unknown = ({ kind, path }) => ({ $: `${kind}:${path}` })): unknown =>
    Match.value(value).pipe(
        Match.when(Array.isArray, (items) => items.map((item) => revive(item, located))),
        Match.when(
            (held: Schema.Json): held is typeof Tagged.Type => Schema.is(Tagged)(held) && Object.keys(held).length === 1,
            ({ $ }) => tagged($, located),
        ),
        Match.when(
            Schema.is(JsonObject),
            Record.map((item: Schema.Json) => revive(item, located)),
        ),
        Match.orElse(identity),
    );
const bury = (value: unknown, named: (held: object) => Option.Option<string> = Option.none): Schema.Json =>
    Match.value(value).pipe(
        Match.when(
            (held: unknown) => Predicate.isUndefined(held) || (Predicate.isNumber(held) && (!Number.isFinite(held) || Object.is(held, -0))),
            (held) => ({
                $: Option.getOrElse(
                    Record.findFirst(NUMBERS, (number) => Object.is(number, held)),
                    () => ['undefined'] as const,
                )[0],
            }),
        ),
        Match.when(Array.isArray, (items: readonly unknown[]) => items.map((item) => bury(item, named))),
        Match.when(Predicate.isFunction, () => '<function>'),
        Match.when(Predicate.isDate, (held) => ({ $: `date:${Number.isNaN(held.getTime()) ? 'NaN' : held.toISOString()}` })),
        Match.when(
            (held: unknown): held is Record.ReadonlyRecord<string, unknown> => Predicate.isObject(held) && Object.getPrototypeOf(held) === Object.prototype,
            Record.map((item: unknown) => bury(item, named)),
        ),
        Match.whenOr(Predicate.isString, Predicate.isNumber, Predicate.isBoolean, Predicate.isNull, identity<string | number | boolean | null>),
        Match.when(Predicate.isObject, (held) => Option.match(named(held), { onSome: ($) => ({ $ }), onNone: () => String(held) })),
        Match.orElse(String),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { bury, JsonObject, type Reference, revive, Tagged };
