import { Effect, flow, Result, Schema, SchemaGetter, SchemaIssue } from 'effect';
import { Toml, Yaml } from 'effect/encoding';
import { parse } from 'plist';

// --- [MODELS] --------------------------------------------------------------------------

const fromParser = (expected: string, parser: (text: string) => unknown) =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.Unknown, {
            decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => parser(text), catch: (cause) => new SchemaIssue.InvalidValue({ expected, cause }, text, options) })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );
const TextFormat = Object.assign(fromParser, { Plist: fromParser('property list', parse), Toml: fromParser('TOML', flow(Toml.parse, Result.getOrThrow)), Yaml: fromParser('YAML', Yaml.parse) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { TextFormat };
