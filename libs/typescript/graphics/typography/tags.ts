import { Effect, Option, Schema, SchemaGetter, SchemaIssue, SchemaTransformation, String } from 'effect';
import type { Feature, Variation } from 'harfbuzzjs';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TAG_LENGTH = 4;

// --- [MODELS] --------------------------------------------------------------------------

const Tag = Schema.String.check(Schema.isBetweenLength(1, TAG_LENGTH)).pipe(Schema.decodeTo(Schema.String.check(Schema.isBetweenLength(TAG_LENGTH, TAG_LENGTH)), SchemaTransformation.transform({ decode: String.padEnd(TAG_LENGTH), encode: String.trimEnd })));
const TagSettings = <A extends Feature | Variation>(host: { new (tag: string, value?: number): A; fromString: (given: string) => A | undefined }, description: string) =>
    Schema.Array(
        Schema.String.annotate({ description }).pipe(
            Schema.decodeTo(Schema.instanceOf(host), {
                decode: SchemaGetter.transformEffect((given: string, options) => Effect.fromOption(Option.fromNullishOr(host.fromString(given)), () => new SchemaIssue.InvalidValue({ message: `Invalid HarfBuzz setting ${given}` }, given, options))),
                encode: SchemaGetter.String(),
            }),
        ),
    ).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Tag, TagSettings };
