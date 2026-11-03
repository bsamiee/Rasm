import { Array, Data, Effect, Number, Option, Order, Record, Schema, SchemaGetter } from 'effect';
import { Digest } from 'effect/crypto';
import type { Face, NameEntry } from 'harfbuzzjs';
import type { Tables } from '#/tables';
import { Tag } from '#/tag';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FRACTION_BITS = 16;
const LEADING_ZERO = /^(?<sign>-?)0\./u;
const NON_ALPHANUMERIC = /[^A-Za-z0-9]/gu;

// --- [MODELS] --------------------------------------------------------------------------

class Names extends Data.Class<{ readonly handle: Face; readonly entries: readonly NameEntry[] }> {
    static readonly ids = { family: { typographicFamily: 16, family: 1 }, style: { typographicSubfamily: 17, subfamily: 2 }, fullName: { fullName: 4 }, version: { version: 5 }, postscriptName: { postscriptName: 6 }, prefix: { variationsPrefix: 25 } } as const;
    static readonly spelled = (value: number) => {
        const fixed = (given: number) => Math.round(given * 2 ** FRACTION_BITS);
        const shortest = Array.min(
            Array.map(Array.range(0, FRACTION_BITS), (places) => Number.round(value, places)),
            Order.mapInput(Order.Boolean, (rounded) => fixed(rounded) !== fixed(value)),
        );
        return `${shortest}`.replace(LEADING_ZERO, '$<sign>.');
    };
    static readonly Generated = Schema.TemplateLiteralParser([
        Schema.String.check(Schema.isPattern(/^[^_]+$/u)),
        '_',
        Schema.String.pipe(
            Schema.decodeTo(
                Schema.Array(
                    Schema.TemplateLiteralParser([Schema.String.check(Schema.isStringFinite()).pipe(Schema.decodeTo(Schema.Finite, { decode: SchemaGetter.Number(), encode: SchemaGetter.transform(Names.spelled) })), Tag]).pipe(
                        Schema.decodeTo(Schema.Tuple([Schema.String, Schema.Finite]), { decode: SchemaGetter.transform(([value, tag]) => [tag, value] as const), encode: SchemaGetter.transform(([tag, value]) => [value, tag] as const) }),
                    ),
                ),
                { decode: SchemaGetter.split({ separator: '_' }), encode: SchemaGetter.transform(Array.join('_')) },
            ),
        ),
    ]).pipe(
        Schema.decodeTo(Schema.Struct({ prefix: Schema.String, variations: Schema.Record(Schema.String, Schema.Finite) }), {
            decode: SchemaGetter.transform(([prefix, , descriptors]) => ({ prefix, variations: Record.fromEntries(descriptors) })),
            encode: SchemaGetter.transform(({ prefix, variations }) => [prefix, '_', Record.toEntries(variations)] as const),
        }),
    );
    static readonly of = (handle: Face) => new Names({ handle, entries: Array.sortWith(handle.listNames(), ({ language }) => language !== 'en', Order.Boolean) });

    find(kind: keyof typeof Names.ids) {
        return Array.findFirst(Object.values(Names.ids[kind]), (id: number) => this.findById(id));
    }
    findById(id: number) {
        return Array.findFirst(this.entries, ({ nameId, language }) => (nameId === id ? Schema.decodeOption(Schema.NonEmptyString)(this.handle.getName(nameId, language)) : Option.none()));
    }
    get prefix() {
        return Option.orElse(this.find('prefix'), () => Option.map(this.find('family'), (family) => family.replaceAll(NON_ALPHANUMERIC, '')));
    }
    namedInstance({ nameID, postscriptNameID }: (typeof Tables.Type)['fvar']['instance'][number]) {
        const style = this.findById(nameID);
        const postscriptName = Option.firstSomeOf([Option.flatMap(postscriptNameID, (id) => this.findById(id)), Option.map(Option.all([this.prefix, Option.map(style, (given) => given.replaceAll(NON_ALPHANUMERIC, ''))]), Array.join('-'))]);
        return {
            names: { postscriptName, fullName: Option.map(Option.all([this.find('family'), style]), Array.join(' ')), style },
            keys: Array.getSomes([postscriptName, Option.map(Option.all([this.find('postscriptName'), Option.map(style, (given) => given.replaceAll(' ', '-'))]), Array.join('_'))]),
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
