import indesign, { app, type Enumerator } from 'adobe:indesign';
import { bury, type JsonObject, revive } from '@rasm/harnesses/cases/values';
import type { Encoded, fields, HostCase } from '@rasm/harnesses/hosts/adobe/indesign/contract';
import { Match, Option, Predicate, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Members {
    [key: string]: unknown;
}
type Item = typeof HostCase.Type;
type Scripted = Exclude<Item, { readonly _tag: 'Panel' }>;
type Batch = typeof Encoded.Type;
type Records = { -readonly [K in keyof typeof fields]: (typeof fields)[K]['Type'][number][] };
interface Codec {
    readonly encoded: (value: unknown) => Schema.Json;
    readonly record: (value: Record.ReadonlyRecord<string, unknown>) => typeof JsonObject.Type;
    readonly revived: (value: Schema.Json) => unknown;
}
interface Session<I extends Item = Item> {
    readonly item: I;
    readonly batch: Batch;
    readonly codec: Codec;
    readonly documents: unknown[];
    readonly confirms: boolean[];
    readonly prompts: (string | null)[];
    readonly files: (string | null)[];
    readonly actions: Members[];
    readonly dialogs: NonNullable<Item['dialogs']>[number][];
    readonly record: Records;
    value: Option.Option<unknown>;
}

// --- [MODELS] --------------------------------------------------------------------------

const Strings = Schema.Array(Schema.String);

// --- [OPERATIONS] ----------------------------------------------------------------------

const names = Object.getOwnPropertyNames(indesign).filter((name) => name !== 'app');
const field = (target: unknown, key: PropertyKey): unknown => Reflect.get(new Object(target), key);
const message = (error: unknown) => String(field(error, 'message') ?? error);
const invoked = (target: unknown, key: string, args: readonly unknown[]): unknown => {
    const method = field(target, key);
    if (!Predicate.isFunction(method)) {
        throw new TypeError(`${key} is not a function`);
    }
    return Reflect.apply(method, target, args);
};
const enumerator = (value: unknown): value is Enumerator => value instanceof indesign.Enumerator;
const equal = (left: unknown, right: unknown) => enumerator(left) && enumerator(right) && left.equals(right);
const Codec = (enumerations: Batch['enumerations']): Codec => {
    const owners = new Map<string, readonly string[]>();
    Object.entries(enumerations).forEach(([enumeration, listed]) => listed.forEach((member) => owners.set(member, [...(owners.get(member) ?? []), enumeration])));
    const qualified = (held: Enumerator) => {
        const name = held.toString();
        const holding = (owners.get(name) ?? []).filter((enumeration) => equal(field(field(indesign, enumeration), name), held));
        return Option.match(
            Option.filter(Option.fromUndefinedOr(holding[0]), () => holding.length === 1),
            { onNone: () => `enum:${name}`, onSome: (owner) => `enum:${owner}.${name}` },
        );
    };
    const named = (held: object): Option.Option<string> =>
        Match.value(held).pipe(
            Match.when(enumerator, qualified),
            Match.when(
                (value: object) => Predicate.isFunction(field(value, 'toSpecifier')),
                (value) => `object:${String(invoked(value, 'toSpecifier', []))}`,
            ),
            Match.option,
        );
    const resolved = (path: string): unknown => {
        const dot = path.indexOf('.');
        const candidates = dot < 0 ? (owners.get(path) ?? []).map((enumeration) => field(field(indesign, enumeration), path)).filter(enumerator) : [field(field(indesign, path.slice(0, dot)), path.slice(dot + 1))].filter(enumerator);
        const [first] = candidates;
        return Option.getOrThrowWith(
            Option.filter(Option.fromUndefinedOr(first), (held) => candidates.every((other) => equal(other, held))),
            () => new Error(`enum:${path} names ${first === undefined ? 'no enumerator' : `enumerators of ${(owners.get(path) ?? []).join(', ')} with different codes`}`),
        );
    };
    const located = { enum: resolved, object: (path: string): unknown => app.doScript('resolve(arguments[0])', indesign.ScriptLanguage.JAVASCRIPT, [path]) };
    const encoded = (value: unknown) => bury(value, named);
    return { encoded, record: Record.map(encoded), revived: (value) => revive(value, located) };
};
const compiled = (parameters: readonly string[], code: string) =>
    // biome-ignore lint/nursery/noImpliedEval: case data and originals compile inside the host
    new globalThis.Function(...parameters, code);
const scope = (overrides: Members) => {
    const inherited = names.filter((name) => !Object.hasOwn(overrides, name));
    return { keys: [...inherited, ...Object.keys(overrides)], values: [...inherited.map((name) => Reflect.get(indesign, name)), ...Object.values(overrides)] };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Batch, Codec, compiled, enumerator, equal, field, type Item, invoked, type Members, message, names, type Scripted, type Session, Strings, scope };
