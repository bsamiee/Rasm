import { Array, Effect, Function, Record, Schema, SchemaGetter, SchemaIssue, SchemaParser, Struct } from 'effect';
import isEmail from 'validator/lib/isEmail.js';
import { type AssetId, Assets } from '#/model/asset.ts';
import { Composition, Placement } from '#/model/placement.ts';

// --- [MODELS] --------------------------------------------------------------------------

const EntryId = Schema.String.check(Schema.isUUID()).pipe(Schema.brand('EntryId'));
const text = Schema.String.pipe(Schema.withConstructorDefault(Effect.succeed('')));
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
    compositions: Schema.Array(Composition).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
});
const PortfolioStructure = Schema.Struct({
    name: text,
    introduction: text,
    email: Schema.String.check(Schema.makeFilter((value) => value === '' || isEmail(value), { expected: 'email address or empty string' })).pipe(Schema.withConstructorDefault(Effect.succeed(''))),
    hero: Schema.optionalKey(Placement),
    entries: Schema.Array(Entry).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
});
const uniqueIds = Schema.makeFilter((document: typeof PortfolioStructure.Type | typeof PortfolioStructure.Encoded) =>
    [{ path: ['entries'], ids: document.entries.map(({ id }) => id) }, ...document.entries.map((entry, index) => ({ path: ['entries', index, 'compositions'], ids: entry.compositions.map(({ id }) => id) }))].flatMap(({ path, ids }) =>
        Array.match(Record.keys(Record.filter(Array.groupBy(ids, Function.identity), (repeats) => repeats.length > 1)), { onEmpty: () => [], onNonEmpty: (repeated) => [{ path, issue: `IDs repeat: ${repeated.join(', ')}` }] }),
    ),
);
const Portfolio = PortfolioStructure.check(uniqueIds);
const StoredPortfolio = Schema.toEncoded(PortfolioStructure).check(uniqueIds);
const PortfolioData = Schema.Struct({ portfolio: Schema.Unknown, assets: Assets.schema }).pipe(
    Schema.decodeTo(Schema.Struct({ portfolio: Schema.toType(Portfolio), assets: Schema.toType(Assets.schema) }), {
        decode: SchemaGetter.transformEffect(({ portfolio, assets }, options) =>
            SchemaParser.decodeUnknownEffect(Portfolio)(portfolio, options).pipe(Effect.provideService(Assets, assets), Effect.mapBoth({ onFailure: (issue) => new SchemaIssue.Pointer(['portfolio'], issue), onSuccess: (resolved) => ({ portfolio: resolved, assets }) })),
        ),
        encode: SchemaGetter.transformEffect(({ portfolio, assets }, options) => Effect.map(SchemaParser.encodeEffect(Portfolio)(portfolio, options), (stored) => ({ portfolio: stored, assets }))),
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const assetIds = (portfolio: typeof Portfolio.Type): ReadonlySet<typeof AssetId.Type> =>
    new Set(
        [...Array.fromNullishOr(portfolio.hero), ...portfolio.entries.flatMap((entry) => [...Array.fromNullishOr(entry.cover), ...entry.compositions.flatMap(Struct.get('items'))])].flatMap((placement) => [
            placement.asset.id,
            ...(placement.kind === 'video' && placement.poster !== undefined ? [placement.poster.id] : []),
        ]),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { assetIds, Entry, EntryId, Portfolio, PortfolioData, StoredPortfolio };
