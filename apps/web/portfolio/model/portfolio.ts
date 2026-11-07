import { Array, Effect, Predicate, Record, Schema, SchemaGetter, SchemaIssue, SchemaParser, Struct } from 'effect';
import isEmail from 'validator/lib/isEmail.js';
import { type AssetId, Assets } from '#/model/asset';
import { defaulted } from '#/model/defaulted';
import { Composition, Placement } from '#/model/placement';

// --- [MODELS] --------------------------------------------------------------------------

const EntryId = Schema.String.check(Schema.isUUID()).pipe(Schema.brand('EntryId'));
const text = defaulted(Schema.String, '');
const Entry = Schema.Struct({
    id: EntryId,
    kind: Schema.Literals(['project', 'study']),
    title: text,
    year: text,
    location: text,
    description: text,
    role: text,
    credits: text,
    cover: Schema.optionalKey(Placement),
    compositions: defaulted(Schema.Array(Composition), []),
});
const PortfolioStructure = Schema.Struct({
    name: text,
    introduction: text,
    email: defaulted(Schema.String.check(Schema.makeFilter((value) => value === '' || isEmail(value), { expected: 'email address or empty string' })), ''),
    hero: Schema.optionalKey(Placement),
    entries: defaulted(Schema.Array(Entry), []),
});
const uniqueIds = Schema.makeFilter(({ entries = [] }: typeof PortfolioStructure.Type | typeof PortfolioStructure.Encoded) =>
    [{ path: ['entries'], values: entries }, ...entries.map(({ compositions = [] }, index) => ({ path: ['entries', index, 'compositions'], values: compositions }))]
        .map(({ path, values }) => ({ path, repeated: Record.keys(Record.filter(Array.groupBy<{ readonly id: string }, string>(values, Struct.get('id')), Predicate.not(Predicate.isTupleOf(1)))) }))
        .filter(({ repeated }) => Array.isReadonlyArrayNonEmpty(repeated))
        .map(({ path, repeated }) => ({ path, issue: `IDs repeat: ${repeated.join(', ')}` })),
);
const Portfolio = Object.assign(PortfolioStructure.check(uniqueIds), {
    assetIds: (portfolio: typeof PortfolioStructure.Type): ReadonlySet<typeof AssetId.Type> =>
        new Set(
            [...Array.fromNullishOr(portfolio.hero), ...portfolio.entries.flatMap((entry) => [...Array.fromNullishOr(entry.cover), ...entry.compositions.flatMap(Struct.get('items'))])].flatMap((placement) => [
                placement.asset.id,
                ...(placement.kind === 'video' && placement.poster !== undefined ? [placement.poster.id] : []),
            ]),
        ),
});
const StoredPortfolio = Schema.toEncoded(PortfolioStructure).check(uniqueIds);
const PortfolioData = Schema.Struct({ portfolio: Schema.Unknown, assets: Assets.schema }).pipe(
    Schema.decodeTo(Schema.Struct({ portfolio: Schema.toType(Portfolio), assets: Schema.toType(Assets.schema) }), {
        decode: SchemaGetter.transformEffect(({ portfolio, assets }, options) =>
            SchemaParser.decodeUnknownEffect(Portfolio)(portfolio, options).pipe(Effect.provideService(Assets, assets), Effect.mapBoth({ onFailure: (issue) => new SchemaIssue.Pointer(['portfolio'], issue), onSuccess: (resolved) => ({ portfolio: resolved, assets }) })),
        ),
        encode: SchemaGetter.transformEffect(({ portfolio, assets }, options) => Effect.map(SchemaParser.encodeEffect(Portfolio)(portfolio, options), (stored) => ({ portfolio: stored, assets }))),
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Entry, EntryId, Portfolio, PortfolioData, StoredPortfolio };
