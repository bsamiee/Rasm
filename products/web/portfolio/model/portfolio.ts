import { Array, Effect, Function, Predicate, Record, Result, Schema, SchemaGetter, SchemaIssue, SchemaParser, Struct } from 'effect';
import isEmail from 'validator/lib/isEmail.js';
import { Assets } from '#/model/asset';
import { Composition, defaulted, Placement, Prose } from '#/model/placement';

// --- [MODELS] --------------------------------------------------------------------------

class Entry extends Schema.Opaque<Entry>()(
    Schema.Struct({
        id: Schema.String.check(Schema.isUUID()).pipe(Schema.brand('EntryId')),
        kind: Schema.Literals(['project', 'study']),
        title: Prose,
        year: Prose,
        location: Prose,
        description: Prose,
        role: Prose,
        credits: Prose,
        cover: Schema.optionalKey(Placement),
        compositions: defaulted(Schema.Array(Composition), []),
    }),
) {}
const uniqueIds = Schema.makeFilter(({ entries = [] }: { readonly entries?: readonly { readonly id: string; readonly compositions?: readonly { readonly id: string }[] }[] }) =>
    Array.filterMap([{ path: ['entries'], values: entries }, ...entries.map(({ compositions = [] }, index) => ({ path: ['entries', index, 'compositions'], values: compositions }))], ({ path, values }) =>
        Result.map(Result.liftPredicate(Record.keys(Record.filter(Array.groupBy(values, Struct.get('id')), Predicate.not(Predicate.isTupleOf(1)))), Array.isReadonlyArrayNonEmpty, Function.constVoid), (repeated) => ({ path, issue: `IDs repeat: ${repeated.join(', ')}` })),
    ),
);
class Portfolio extends Schema.Opaque<Portfolio>()(
    Schema.Struct({
        name: Prose,
        introduction: Prose,
        email: Prose.of(Schema.String.check(Schema.makeFilter((value) => isEmail(value), { expected: 'email address' }))),
        hero: Schema.optionalKey(Placement),
        entries: defaulted(Schema.Array(Entry), []),
    }).check(uniqueIds),
) {
    static readonly assetIds = (portfolio: Portfolio) =>
        new Set(
            [...Array.fromNullishOr(portfolio.hero), ...portfolio.entries.flatMap((entry) => [...Array.fromNullishOr(entry.cover), ...entry.compositions.flatMap(Struct.get('items'))])].flatMap((placement) => [
                placement.asset.id,
                ...(placement.kind === 'video' && placement.poster !== undefined ? [placement.poster.id] : []),
            ]),
        );
}
class StoredPortfolio extends Schema.Opaque<StoredPortfolio>()(Schema.toEncoded(Portfolio).check(uniqueIds)) {}
const PortfolioData = Schema.Struct({ portfolio: Schema.Unknown, assets: Assets.schema }).pipe(
    Schema.decodeTo(Schema.Struct({ portfolio: Schema.toType(Portfolio), assets: Schema.toType(Assets.schema) }), {
        decode: SchemaGetter.transformEffect(({ portfolio, assets }, options) =>
            SchemaParser.decodeUnknownEffect(Portfolio)(portfolio, options).pipe(Effect.provideService(Assets, assets), Effect.mapBoth({ onFailure: (issue) => new SchemaIssue.Pointer(['portfolio'], issue), onSuccess: (resolved) => ({ portfolio: resolved, assets }) })),
        ),
        encode: SchemaGetter.transformEffect(({ portfolio, assets }, options) => Effect.map(SchemaParser.encodeEffect(Portfolio)(portfolio, options), (stored) => ({ portfolio: stored, assets }))),
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Entry, Portfolio, PortfolioData, StoredPortfolio };
