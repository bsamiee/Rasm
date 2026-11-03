import { Manifest as Uxp } from '@rasm/uxp/manifest';
import { Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

class Manifest extends Schema.Class<Manifest, { readonly brand: unique symbol }>('Manifest')({
    id: Schema.String,
    name: Schema.String,
    version: Uxp.Version,
    host: Schema.Struct({ app: Schema.Literals(['ILST', 'AEFT', 'PHXS', 'IDSN', 'PPRO', 'AUDT', 'FLPR', 'DRWV', 'KBRG', 'AME']), minVersion: Uxp.Version }),
}) {
    static readonly target = 'es3';
    static readonly chromium = 'chrome99';

    xml({ main, script }: { readonly main: string; readonly script: string }): string {
        const runtime = '12.0';
        const extension = `<Extension Id="${this.id}.link"`;
        return [
            '<?xml version="1.0" encoding="UTF-8"?>',
            `<ExtensionManifest Version="${runtime}" ExtensionBundleId="${this.id}" ExtensionBundleVersion="${this.version}" ExtensionBundleName="${this.name}">`,
            `    <ExtensionList>${extension} Version="${this.version}"/></ExtensionList>`,
            '    <ExecutionEnvironment>',
            `        <HostList><Host Name="${this.host.app}" Version="${this.host.minVersion}"/></HostList>`,
            '        <LocaleList><Locale Code="All"/></LocaleList>',
            `        <RequiredRuntimeList><RequiredRuntime Name="CSXS" Version="${runtime}"/></RequiredRuntimeList>`,
            '    </ExecutionEnvironment>',
            '    <DispatchInfoList>',
            `        ${extension}>`,
            '            <DispatchInfo>',
            `                <Resources><MainPath>./${main}</MainPath><ScriptPath>./${script}</ScriptPath></Resources>`,
            '                <Lifecycle><AutoVisible>false</AutoVisible><StartOn><Event>applicationActivate</Event><Event>com.adobe.csxs.events.ApplicationActivate</Event></StartOn></Lifecycle>',
            '                <UI><Type>Custom</Type><Geometry><Size><Height>1</Height><Width>1</Width></Size></Geometry></UI>',
            '            </DispatchInfo>',
            '        </Extension>',
            '    </DispatchInfoList>',
            '</ExtensionManifest>',
            '',
        ].join('\n');
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Manifest };
