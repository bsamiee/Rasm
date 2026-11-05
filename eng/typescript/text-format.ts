import { Effect, flow, Result, Schema, SchemaGetter, SchemaIssue } from 'effect';
import { Toml, Yaml } from 'effect/encoding';
import JSON5 from 'json5';
import { parse } from 'plist';

// --- [MODELS] --------------------------------------------------------------------------

const fromParser = (expected: string, parser: (text: string) => unknown) =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.Unknown, {
            decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => parser(text), catch: (cause) => new SchemaIssue.InvalidValue({ expected, cause }, text, options) })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );
const TextFormat = Object.assign(fromParser, { Json5: fromParser('JSON5', JSON5.parse), Plist: fromParser('property list', parse), Toml: fromParser('TOML', flow(Toml.parse, Result.getOrThrow)), Yaml: fromParser('YAML', Yaml.parse) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { TextFormat };
