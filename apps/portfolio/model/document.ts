import { Array, Effect, Schema, SchemaGetter, SchemaParser, Struct } from 'effect';
import { AssetCollection, Id } from './asset.ts';
import { Assets, Composition, Placement } from './placement.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const emptyPortfolio: typeof StoredPortfolio.Type = { name: '', introduction: '', email: '', entries: [] };

// --- [MODELS] --------------------------------------------------------------------------

const Entry = Schema.Struct({
    id: Id,
    kind: Schema.Literals(['project', 'study']),
    title: Schema.String,
    year: Schema.String,
    location: Schema.String,
    description: Schema.String,
    role: Schema.String,
    credits: Schema.String,
    cover: Schema.optionalKey(Placement),
    compositions: Schema.Array(Composition),
});
const Portfolio = Schema.Struct({ name: Schema.String, introduction: Schema.String, email: Schema.String, hero: Schema.optionalKey(Placement), entries: Schema.Array(Entry) }).check(
    Schema.makeFilter((portfolio) =>
        [{ path: ['entries'], ids: portfolio.entries.map(Struct.get('id')) }, ...portfolio.entries.map((entry, index) => ({ path: ['entries', index, 'compositions'], ids: entry.compositions.map(Struct.get('id')) }))].flatMap(({ path, ids }) =>
            new Set(ids).size === ids.length ? [] : [{ path, issue: 'Each item needs a unique ID' }],
        ),
    ),
);
const StoredPortfolio = Schema.toEncoded(Portfolio);
const PortfolioData = Schema.Struct({ portfolio: StoredPortfolio, assets: AssetCollection }).pipe(
    Schema.decodeTo(Schema.Struct({ portfolio: Schema.toType(Portfolio), assets: AssetCollection }), {
        decode: SchemaGetter.transformEffect((data) =>
            SchemaParser.decodeEffect(Portfolio)(data.portfolio).pipe(
                Effect.provideService(Assets, data.assets),
                Effect.map((portfolio) => ({ ...data, portfolio })),
            ),
        ),
        encode: SchemaGetter.transformEffect((data) => SchemaParser.encodeEffect(Portfolio)(data.portfolio).pipe(Effect.map((portfolio) => ({ ...data, portfolio })))),
    }),
);
const Session = Schema.Union([Schema.Struct({ kind: Schema.Literal('owner') }), Schema.Struct({ kind: Schema.Literal('visitor'), signedIn: Schema.Boolean })]);
const Bootstrap = Schema.Struct({ initial: PortfolioData, session: Session });

// --- [OPERATIONS] ----------------------------------------------------------------------

const placements = (portfolio: typeof Portfolio.Type): readonly (typeof Placement.Type)[] => [...Array.fromNullishOr(portfolio.hero), ...portfolio.entries.flatMap((entry) => [...Array.fromNullishOr(entry.cover), ...entry.compositions.flatMap(Struct.get('items'))])];
const createEntry = (id: string, kind: typeof Entry.Type.kind): typeof Entry.Type => ({ id, kind, title: '', year: '', location: '', description: '', role: '', credits: '', compositions: [] });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Bootstrap, createEntry, Entry, emptyPortfolio, Portfolio, PortfolioData, placements, Session, StoredPortfolio };
