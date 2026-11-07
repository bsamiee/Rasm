import { Effect, flow, Result, Schema, SchemaGetter, SchemaIssue } from 'effect';
import { Json5, Toml, Yaml } from 'effect/encoding';

// --- [MODELS] --------------------------------------------------------------------------

const fromParser = (expected: string, parser: (text: string) => unknown) =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.Unknown, {
            decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => parser(text), catch: (cause) => new SchemaIssue.InvalidValue({ expected, cause }, text, options) })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );
const TextFormat = Object.assign(fromParser, { Json5: fromParser('JSON5', flow(Json5.parse, Result.getOrThrow)), Toml: fromParser('TOML', flow(Toml.parse, Result.getOrThrow)), Yaml: fromParser('YAML', Yaml.parse) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { TextFormat };
