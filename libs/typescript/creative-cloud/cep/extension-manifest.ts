import { Version } from '@rasm/automation';
import { Schema } from 'effect';
import Builder from 'fast-xml-builder';

// --- [CONSTANTS] -----------------------------------------------------------------------

const XML = new Builder({ ignoreAttributes: false, format: true, indentBy: '    ', suppressEmptyNode: true });

// --- [MODELS] --------------------------------------------------------------------------

class ExtensionManifest extends Schema.Class<ExtensionManifest, { readonly brand: unique symbol }>('ExtensionManifest')({
    id: Schema.String,
    name: Schema.String,
    version: Version,
    host: Schema.Struct({ app: Schema.Literals(['ILST', 'AEFT', 'PHXS', 'IDSN', 'PPRO', 'AUDT', 'FLPR', 'DRWV', 'KBRG', 'AME']), minVersion: Version, maxVersion: Schema.optionalKey(Version) }),
}) {
    static readonly target = 'es3';
    static readonly chromium = 'chrome99';
    static readonly runtime = '12.0';
    static readonly global = 'handle';
    static readonly panel = 'index.html';
    static readonly script = 'host.jsx';
    static readonly file = 'CSXS/manifest.xml';
    static readonly debugFile = '.debug';

    get xml(): string {
        const { app, minVersion, maxVersion } = this.host;
        return XML.build({
            '?xml': { '@_version': '1.0', '@_encoding': 'UTF-8' },
            ExtensionManifest: {
                '@_Version': ExtensionManifest.runtime,
                '@_ExtensionBundleId': this.id,
                '@_ExtensionBundleVersion': this.version,
                '@_ExtensionBundleName': this.name,
                ExtensionList: { Extension: { '@_Id': this.id, '@_Version': this.version } },
                ExecutionEnvironment: {
                    HostList: { Host: { '@_Name': app, '@_Version': maxVersion === undefined ? minVersion : `[${minVersion},${maxVersion}]` } },
                    LocaleList: { Locale: { '@_Code': 'All' } },
                    RequiredRuntimeList: { RequiredRuntime: { '@_Name': 'CSXS', '@_Version': ExtensionManifest.runtime } },
                },
                DispatchInfoList: {
                    Extension: {
                        '@_Id': this.id,
                        DispatchInfo: {
                            Resources: { MainPath: `./${ExtensionManifest.panel}`, ScriptPath: `./${ExtensionManifest.script}` },
                            Lifecycle: { AutoVisible: false, StartOn: { Event: ['applicationActivate', 'com.adobe.csxs.events.ApplicationActivate'] } },
                            UI: { Type: 'Custom', Geometry: { Size: { Height: 1, Width: 1 } } },
                        },
                    },
                },
            },
        });
    }
    debug(port: number): string {
        return XML.build({
            '?xml': { '@_version': '1.0', '@_encoding': 'UTF-8' },
            ExtensionList: { Extension: { '@_Id': this.id, HostList: { Host: { '@_Name': this.host.app, '@_Port': port } } } },
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ExtensionManifest };
