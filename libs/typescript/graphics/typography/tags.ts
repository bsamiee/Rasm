import { Effect, Option, Schema, SchemaGetter, SchemaIssue } from 'effect';
import type { Feature, Variation } from 'harfbuzzjs';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TAG_LENGTH = 4;

// --- [MODELS] --------------------------------------------------------------------------

const Tag = Schema.String.check(Schema.isMaxLength(TAG_LENGTH)).pipe(Schema.decodeTo(Schema.String, { decode: SchemaGetter.transform((tag: string) => tag.padEnd(TAG_LENGTH)), encode: SchemaGetter.transform((tag: string) => tag.trimEnd()) }));
const Settings = <A extends Feature | Variation>(host: { new (tag: string, value?: number): A; fromString: (given: string) => A | undefined }, description: string) =>
    Schema.Array(
        Schema.String.annotate({ description }).pipe(
            Schema.decodeTo(Schema.instanceOf(host), {
                decode: SchemaGetter.transformEffect((given: string, options) => Effect.fromOption(Option.fromNullishOr(host.fromString(given)), () => new SchemaIssue.InvalidValue({ message: `Not a setting in HarfBuzz syntax: ${given}` }, given, options))),
                encode: SchemaGetter.transform((setting: A) => setting.toString()),
            }),
        ),
    ).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Settings, Tag };
