import { Version } from '@rasm/automation';
import { Schema, SchemaGetter } from 'effect';
import Builder from 'fast-xml-builder';
import { XMLParser } from 'fast-xml-parser';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTS = ['ILST', 'AEFT', 'PHXS', 'IDSN', 'PPRO', 'AUDT', 'FLPR', 'DRWV', 'KBRG', 'AME'] as const;
const ATTRIBUTE = '@_';
const BUILDER = new Builder({ ignoreAttributes: false, attributeNamePrefix: ATTRIBUTE, format: true, indentBy: '    ', suppressEmptyNode: true });
const PARSER = new XMLParser({ ignoreAttributes: false, attributeNamePrefix: ATTRIBUTE });
const PROLOG = { '@_version': '1.0', '@_encoding': 'UTF-8' };
const FILL_OPACITY = 0.65;
const FILLS = { light: { color: '#000000', opacity: FILL_OPACITY }, dark: { color: '#FFFFFF', opacity: FILL_OPACITY } } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Range = Schema.Union([Schema.TemplateLiteralParser(['[', Version, ',', Version, ']']), Version]).annotate({ description: 'Host version attribute, a floor or a bracketed range' });
const Document = Schema.Struct({
    ExtensionManifest: Schema.Struct({
        '@_ExtensionBundleId': Schema.String,
        '@_ExtensionBundleVersion': Version,
        '@_ExtensionBundleName': Schema.String,
        ExecutionEnvironment: Schema.Struct({ HostList: Schema.Struct({ Host: Schema.Struct({ '@_Name': Schema.Literals(HOSTS), '@_Version': Range }) }) }),
    }),
});

class ExtensionManifest extends Schema.Class<ExtensionManifest, { readonly brand: unique symbol }>('ExtensionManifest')({
    id: Schema.String,
    name: Schema.String,
    version: Version,
    host: Schema.Struct({ app: Schema.Literals(HOSTS), minVersion: Version, maxVersion: Schema.optionalKey(Version) }),
}) {
    static readonly fills = FILLS;
    static readonly target = 'es3';
    static readonly chromium = 'chrome99';
    static readonly global = 'handle';
    static readonly panel = 'index.html';
    static readonly script = 'host.jsx';
    static readonly file = 'CSXS/manifest.xml';
    static readonly debugFile = '.debug';
    static readonly debugPort = 8000;
    static readonly fromXml: Schema.Codec<ExtensionManifest, string> = Schema.String.pipe(
        Schema.decodeTo(Document, { decode: SchemaGetter.transform(PARSER.parse.bind(PARSER)), encode: SchemaGetter.forbiddenEncoding }),
        Schema.decodeTo(ExtensionManifest, {
            decode: SchemaGetter.transform(
                ({
                    ExtensionManifest: {
                        '@_ExtensionBundleId': id,
                        '@_ExtensionBundleName': name,
                        '@_ExtensionBundleVersion': version,
                        ExecutionEnvironment: {
                            HostList: {
                                Host: { '@_Name': app, '@_Version': range },
                            },
                        },
                    },
                }) => ({
                    id,
                    name,
                    version,
                    host: typeof range === 'string' ? { app, minVersion: range } : { app, minVersion: range[1], maxVersion: range[3] },
                }),
            ),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );

    debug(base: number): string {
        return BUILDER.build({
            '?xml': PROLOG,
            ExtensionList: { Extension: { '@_Id': this.id, HostList: { Host: { '@_Name': this.host.app, '@_Port': base + HOSTS.indexOf(this.host.app) } } } },
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ExtensionManifest };
