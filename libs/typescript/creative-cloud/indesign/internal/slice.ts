import { Effect, Option, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

class Slice extends Schema.Class<Slice, { readonly brand: unique symbol }>('Slice')({
    offset: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0))
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed(0)))
        .annotate({ description: 'Items skipped' }),
    limit: Schema.OptionFromOptionalKey(Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Items returned, absent returns the rest' })),
}) {
    static readonly Every = Slice.pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));

    of<A>(items: readonly A[]): readonly A[] {
        return items.slice(this.offset, Option.getOrUndefined(Option.map(this.limit, (limit) => this.offset + limit)));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Slice };
