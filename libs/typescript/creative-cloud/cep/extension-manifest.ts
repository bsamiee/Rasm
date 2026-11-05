import { Version } from '@rasm/automation';
import { Record, Schema, SchemaGetter, Struct } from 'effect';
import Builder from 'fast-xml-builder';
import { XMLParser } from 'fast-xml-parser';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTS = ['ILST', 'AEFT', 'PHXS', 'IDSN', 'PPRO', 'AUDT', 'FLPR', 'DRWV', 'KBRG', 'AME'] as const;
const ATTRIBUTE = '@_';
const BUILDER = new Builder({ ignoreAttributes: false, attributeNamePrefix: ATTRIBUTE, format: true, indentBy: '    ', suppressEmptyNode: true });
const PARSER = new XMLParser({ ignoreAttributes: false, attributeNamePrefix: ATTRIBUTE });
const PROLOG = { '@_version': '1.0', '@_encoding': 'UTF-8' };
const LAUNCH_EVENTS = ['applicationActivate', 'com.adobe.csxs.events.ApplicationActivate'];
const INK_OPACITY = 0.65;
const INKS = { light: { color: '#000000', opacity: INK_OPACITY }, dark: { color: '#FFFFFF', opacity: INK_OPACITY } } as const;
const STATES = { Normal: 'light', RollOver: 'light', DarkNormal: 'dark', DarkRollOver: 'dark' } as const;

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
const Size = Schema.Struct({ height: Schema.Finite, width: Schema.Finite });
const Face = Schema.Struct({
    type: Schema.Literals(['Panel', 'Modeless', 'ModalDialog']),
    menu: Schema.String,
    size: Size,
    minimum: Schema.optionalKey(Size),
    maximum: Schema.optionalKey(Size),
    icons: Schema.optionalKey(Schema.Struct(Struct.Record(Struct.keys(STATES), Schema.String))),
});

class ExtensionManifest extends Schema.Class<ExtensionManifest, { readonly brand: unique symbol }>('ExtensionManifest')({
    id: Schema.String,
    name: Schema.String,
    version: Version,
    host: Schema.Struct({ app: Schema.Literals(HOSTS), minVersion: Version, maxVersion: Schema.optionalKey(Version) }),
    face: Schema.optionalKey(Face),
    parameters: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'CEF command line switches' })),
    scripted: Schema.optionalKey(Schema.Boolean),
    atLaunch: Schema.optionalKey(Schema.Boolean),
}) {
    static readonly inks = INKS;
    static readonly states = STATES;
    static readonly target = 'es3';
    static readonly chromium = 'chrome99';
    static readonly runtime = '12.0';
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

    get xml(): string {
        const { host, face, parameters, scripted, atLaunch } = this;
        const sized = ({ height, width }: typeof Size.Type) => ({ Height: height, Width: width });
        return BUILDER.build({
            '?xml': PROLOG,
            ExtensionManifest: {
                '@_Version': ExtensionManifest.runtime,
                '@_ExtensionBundleId': this.id,
                '@_ExtensionBundleVersion': this.version,
                '@_ExtensionBundleName': this.name,
                ExtensionList: { Extension: { '@_Id': this.id, '@_Version': this.version } },
                ExecutionEnvironment: {
                    HostList: { Host: { '@_Name': host.app, '@_Version': host.maxVersion === undefined ? host.minVersion : `[${host.minVersion},${host.maxVersion}]` } },
                    LocaleList: { Locale: { '@_Code': 'All' } },
                    RequiredRuntimeList: { RequiredRuntime: { '@_Name': 'CSXS', '@_Version': ExtensionManifest.runtime } },
                },
                DispatchInfoList: {
                    Extension: {
                        '@_Id': this.id,
                        DispatchInfo: {
                            Resources: { MainPath: `./${ExtensionManifest.panel}`, ...(scripted === true ? { ScriptPath: `./${ExtensionManifest.script}` } : {}), ...(parameters === undefined ? {} : { CEFCommandLine: { Parameter: parameters } }) },
                            Lifecycle: { AutoVisible: face !== undefined, ...(face === undefined || atLaunch === true ? { StartOn: { Event: LAUNCH_EVENTS } } : {}) },
                            UI:
                                face === undefined
                                    ? { Type: 'Custom', Geometry: { Size: { Height: 1, Width: 1 } } }
                                    : {
                                          Type: face.type,
                                          Menu: face.menu,
                                          Geometry: { Size: sized(face.size), ...(face.minimum === undefined ? {} : { MinSize: sized(face.minimum) }), ...(face.maximum === undefined ? {} : { MaxSize: sized(face.maximum) }) },
                                          ...(face.icons === undefined ? {} : { Icons: { Icon: Record.collect(face.icons, (type, path) => ({ '@_Type': type, '#text': `./${path}` })) } }),
                                      },
                        },
                    },
                },
            },
        });
    }
    debug(base: number): string {
        return BUILDER.build({
            '?xml': PROLOG,
            ExtensionList: { Extension: { '@_Id': this.id, HostList: { Host: { '@_Name': this.host.app, '@_Port': base + HOSTS.indexOf(this.host.app) } } } },
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ExtensionManifest };
