import { Array, Data, Effect, Number, Option, Order, Predicate, Record, Schema, SchemaGetter } from 'effect';
import { Digest } from 'effect/crypto';
import type { Face, NameEntry } from 'harfbuzzjs';
import type { Tables } from '#/tables';
import { Tag } from '#/tags';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LEADING_ZERO = /^(?<sign>-?)0\./u;
const NON_ALPHANUMERIC = /[^A-Za-z0-9]/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Descriptor = Schema.TemplateLiteralParser([Schema.String.check(Schema.isStringFinite()).pipe(Schema.decodeTo(Schema.Finite, { decode: SchemaGetter.Number(), encode: SchemaGetter.transform((value: number) => `${value}`.replace(LEADING_ZERO, '$<sign>.')) })), Tag]).pipe(
    Schema.decodeTo(Schema.Tuple([Schema.String, Schema.Finite]), { decode: SchemaGetter.transform(([value, tag]) => [tag, value] as const), encode: SchemaGetter.transform(([tag, value]) => [value, tag] as const) }),
);
class Names extends Data.Class<{ readonly handle: Face; readonly entries: readonly NameEntry[] }> {
    static readonly ids = { family: { typographicFamily: 16, family: 1 }, style: { typographicSubfamily: 17, subfamily: 2 }, fullName: { fullName: 4 }, version: { version: 5 }, postscriptName: { postscriptName: 6 }, prefix: { variationsPrefix: 25 } } as const;
    static readonly Generated = Schema.TemplateLiteralParser([Schema.String.check(Schema.isPattern(/^[^_]+$/u)), '_', Schema.String.pipe(Schema.decodeTo(Schema.Array(Descriptor), { decode: SchemaGetter.split({ separator: '_' }), encode: SchemaGetter.transform(Array.join('_')) }))]).pipe(
        Schema.decodeTo(Schema.Struct({ prefix: Schema.String, variations: Schema.Record(Schema.String, Schema.Finite) }), {
            decode: SchemaGetter.transform(([prefix, , descriptors]) => ({ prefix, variations: Record.fromEntries(descriptors) })),
            encode: SchemaGetter.transform(({ prefix, variations }) => [prefix, '_', Record.toEntries(variations)] as const),
        }),
    );
    static readonly of = (handle: Face) => new Names({ handle, entries: Array.sortWith(handle.listNames(), ({ language }) => language !== 'en', Order.Boolean) });
    static readonly coordinate = (value: number) => {
        const fractionBits = 16;
        const fixed = (given: number) => Math.round(given * 2 ** fractionBits);
        const roundings = Array.map(Array.range(0, fractionBits), (places) => Number.round(value, places));
        return Array.min(
            roundings,
            Order.mapInput(Order.Boolean, (rounded) => fixed(rounded) !== fixed(value)),
        );
    };

    named(id: keyof typeof Names.ids | number | undefined) {
        const ids: ReadonlyArray<number | undefined> = Predicate.isString(id) ? Object.values(Names.ids[id]) : [id];
        const records = ids.flatMap((each) => this.entries.filter(({ nameId }) => nameId === each));
        return Array.findFirst(records, ({ nameId, language }) => Schema.decodeOption(Schema.NonEmptyString)(this.handle.getName(nameId, language)));
    }
    get prefix() {
        return Option.orElse(this.named('prefix'), () => Option.map(this.named('family'), (family) => family.replaceAll(NON_ALPHANUMERIC, '')));
    }
    instance({ nameID, postscriptNameID }: (typeof Tables.Type)['fvar']['instance'][number]) {
        const style = this.named(nameID);
        const postscriptName = Option.firstSomeOf([Option.flatMap(postscriptNameID, (id) => this.named(id)), Option.map(Option.all([this.prefix, Option.map(style, (given) => given.replaceAll(NON_ALPHANUMERIC, ''))]), Array.join('-'))]);
        return {
            names: { postscriptName, fullName: Option.map(Option.all([this.named('family'), style]), Array.join(' ')), style },
            keys: Array.getSomes([postscriptName, Option.map(Option.all([this.named('postscriptName'), Option.map(style, (given) => given.replaceAll(' ', '-'))]), Array.join('_'))]),
        };
    }
    generated(variations: (typeof Names.Generated.Type)['variations']) {
        const nameLength = 127;
        return Effect.transposeOption(
            Option.flatMap(this.prefix, (prefix) =>
                Option.map(Schema.encodeOption(Names.Generated)({ prefix, variations }), (name) => (name.length > nameLength ? Digest.Digest.use((digest) => digest.digestHex(Digest.sha1, new TextEncoder().encode(name))).pipe(Effect.map((hex) => `${prefix}-${hex}...`)) : Effect.succeed(name))),
            ),
        );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Names };
