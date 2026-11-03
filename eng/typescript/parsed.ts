import { Effect, flow, Result, Schema, SchemaGetter, SchemaIssue } from 'effect';
import { Toml as TomlText, Yaml as YamlText } from 'effect/encoding';
import { parse as parsePlist } from 'plist';

// --- [MODELS] --------------------------------------------------------------------------

const fromString = (expected: string, parse: (text: string) => unknown) =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.Unknown, {
            decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => parse(text), catch: (cause) => new SchemaIssue.InvalidValue({ expected, cause }, text, options) })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );
const Toml = fromString('TOML', flow(TomlText.parse, Result.getOrThrow));
const Yaml = fromString('YAML', YamlText.parse);
const Plist = fromString('property list', parsePlist);

// --- [EXPORTS] -------------------------------------------------------------------------

export { fromString, Plist, Toml, Yaml };
