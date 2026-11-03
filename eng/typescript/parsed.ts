import { Effect, Schema, SchemaGetter, SchemaIssue } from 'effect';
import { parse as parsePlist } from 'plist';
import { parse as parseToml } from 'smol-toml';
import { parse as parseYaml } from 'yaml';

// --- [MODELS] --------------------------------------------------------------------------

const fromString = (expected: string, parse: (text: string) => unknown) =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.Unknown, {
            decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => parse(text), catch: (cause) => new SchemaIssue.InvalidValue({ expected, cause }, text, options) })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );
const Toml = fromString('TOML', parseToml);
const Yaml = fromString('YAML', parseYaml);
const Plist = fromString('property list', parsePlist);

// --- [EXPORTS] -------------------------------------------------------------------------

export { fromString, Plist, Toml, Yaml };
