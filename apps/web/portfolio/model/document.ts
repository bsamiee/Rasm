import { Array, Effect, Schema, type SchemaAST, SchemaIssue, SchemaParser, Struct } from 'effect';
import isEmail from 'validator/lib/isEmail.js';
import { AssetCollection, Id } from './asset.ts';
import { Assets, Composition, Placement } from './placement.ts';

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
const PortfolioStructure = Schema.Struct({
    name: Schema.String,
    introduction: Schema.String,
    email: Schema.String.check(Schema.makeFilter((value) => value === '' || isEmail(value), { expected: 'a complete email address or an empty field' })),
    hero: Schema.optionalKey(Placement),
    entries: Schema.Array(Entry),
});
const uniqueIds = Schema.makeFilter((document: typeof PortfolioStructure.Type | typeof PortfolioStructure.Encoded) =>
    [{ path: ['entries'], ids: document.entries.map(({ id }) => id) }, ...document.entries.map((entry, index) => ({ path: ['entries', index, 'compositions'], ids: entry.compositions.map(({ id }) => id) }))].flatMap(({ path, ids }) =>
        new Set(ids).size === ids.length ? [] : [{ path, issue: 'Each item needs a unique ID' }],
    ),
);
const Portfolio = PortfolioStructure.check(uniqueIds);
const StoredPortfolio = Schema.toEncoded(PortfolioStructure).check(uniqueIds);
const envelope = Schema.Struct({ portfolio: Portfolio, assets: AssetCollection });
const PortfolioData = Schema.make<Schema.Codec<typeof envelope.Type, typeof envelope.Encoded>>(
    Schema.declareConstructor<typeof envelope.Type, typeof envelope.Encoded>()(
        [envelope.fields.portfolio, envelope.fields.assets],
        ([portfolio, assets]) => {
            const parse = SchemaParser.decodeUnknownEffect(Schema.Struct({ portfolio: Schema.Unknown, assets }));
            const resolve = SchemaParser.decodeUnknownEffect(portfolio);
            return Effect.fnUntraced(function* (input: unknown, _ast: SchemaAST.Declaration, options: SchemaAST.ParseOptions) {
                const data = yield* parse(input, options);
                const resolved = yield* resolve(data.portfolio, options).pipe(
                    Effect.provideService(Assets, data.assets),
                    Effect.mapError((issue) => new SchemaIssue.Pointer(['portfolio'], issue)),
                );
                return { ...data, portfolio: resolved };
            });
        },
        { toCodecJson: () => undefined },
    ).ast,
);
const Session = Schema.Union([Schema.Struct({ kind: Schema.Literal('owner') }), Schema.Struct({ kind: Schema.Literal('visitor'), signedIn: Schema.Boolean })]);
const Bootstrap = Schema.Struct({ initial: PortfolioData, session: Session });
const shell = { root: 'portfolio', data: 'portfolio-data', schema: Bootstrap };

// --- [OPERATIONS] ----------------------------------------------------------------------

const placements = (portfolio: typeof Portfolio.Type) => [...Array.fromNullishOr(portfolio.hero), ...portfolio.entries.flatMap((entry) => [...Array.fromNullishOr(entry.cover), ...entry.compositions.flatMap(Struct.get('items'))])];
const createEntry = (id: string, kind: typeof Entry.Type.kind) => ({ id, kind, title: '', year: '', location: '', description: '', role: '', credits: '', compositions: [] });

// --- [EXPORTS] -------------------------------------------------------------------------

export { createEntry, Entry, Portfolio, PortfolioData, placements, Session, StoredPortfolio, shell };
