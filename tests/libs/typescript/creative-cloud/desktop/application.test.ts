import { DatabaseSync } from 'node:sqlite';
import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, describe, it } from '@effect/vitest';
import { Installation } from '@rasm/applications/contract';
import { Application, Registry } from '@rasm/creative-cloud-desktop/application';
import { Effect, FileSystem, Option, Path } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REGISTRY = [
    ['PHSPBETA', '28.2', 'InstallDir', '/Applications/Adobe Photoshop (Beta)'],
    ['PHSPBETA', '28.2', 'AppLaunch', '[INSTALLDIR]/Adobe Photoshop (Beta).app/Contents/MacOS/Adobe Photoshop 2027'],
    ['PHSPBETA', '28.2', 'BuildVersion', '28.2.0.3749'],
    ['ILSTBETA', '31.1', 'InstallDir', '/Applications/Adobe Illustrator (Beta)'],
    ['ILSTBETA', '31.1', 'AppLaunch', '[InstallDir]/Adobe Illustrator.app/Contents/MacOS/Adobe Illustrator'],
    ['ILSTBETA', '31.1', 'BuildVersion', '31.1.0.127'],
    ['ILST', '31.0', 'InstallDir', '/Applications/Adobe Illustrator 2027'],
    ['ILST', '31.0', 'AppLaunch', '[InstallDir]/Adobe Illustrator.app/Contents/MacOS/Adobe Illustrator'],
    ['ILST', '31.0', 'BuildVersion', '31.0.0.84'],
    ['ACR', '19.0', 'InstallDir', '/Library/Application Support/Adobe'],
    ['ACR', '19.0', 'AppLaunch', ''],
    ['ACR', '19.0', 'BuildVersion', '19.0.0.544'],
] as const;
const OFFERED = [
    ['/Applications/Adobe Illustrator 2027/Adobe Illustrator.app', 'Adobe Illustrator', 'Adobe Illustrator 2027', 'com.adobe.illustrator', '31.0.0'],
    ['/Applications/Adobe Photoshop (Beta)/Adobe Photoshop (Beta).app', 'Adobe Photoshop 2027', 'Adobe Photoshop (Beta)', 'com.adobe.Photoshop', '28.2.0'],
    ['/Applications/Adobe Illustrator (Beta)/Adobe Illustrator.app', 'Adobe Illustrator', 'Adobe Illustrator 2027 (Beta)', 'com.adobe.illustratorBeta', '31.1.0'],
    ['/Applications/Adobe Acrobat DC/Adobe Acrobat.app', 'AdobeAcrobat', 'Adobe Acrobat', 'com.adobe.Acrobat.Pro', '26.3.0'],
    ['/Volumes/Archive/Adobe Acrobat.app', 'AdobeAcrobat', 'Adobe Acrobat', 'com.adobe.Acrobat.Pro', '26.10.0'],
    ['/Applications/Adobe Acrobat DC/Acrobat Uninstaller.app', 'Acrobat Uninstaller', 'Acrobat Uninstaller', 'com.adobe.Acrobat.Uninstaller', undefined],
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const offered = OFFERED.map(([bundle, executable, name, identifier, version]) =>
    Installation.make({
        url: pathToFileURL(`${bundle}/`),
        executable: Option.some(pathToFileURL(`${bundle}/Contents/MacOS/${executable}`)),
        identifier,
        name,
        bundleName: Option.none(),
        version: Option.fromUndefinedOr(version),
        build: Option.none(),
        scriptable: Option.some(false),
        scriptingDefinitionFile: Option.none(),
        activationPolicy: 'regular',
        documentTypes: Option.none(),
        urlSchemes: Option.none(),
        exportedTypes: Option.none(),
        multipleInstancesProhibited: Option.none(),
        environment: Option.none(),
        architectures: Option.none(),
        quarantine: Option.none(),
        claim: Option.none(),
        appIntents: Option.some([]),
    }),
);
const listed = Effect.fnUntraced(
    function* (registered: boolean) {
        const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const file = path.join(yield* files.makeTempDirectoryScoped(), 'hdpim.db');
        yield* Effect.when(
            Effect.sync(() => {
                const database = new DatabaseSync(file);
                database.exec('CREATE TABLE product_installation_info (SAPCode, ProductVersion, ProcessorFamily, Status, PRIMARY KEY (SAPCode, ProductVersion, ProcessorFamily)); CREATE TABLE product_installation_meta_info (SAPCode, ProductVersion, ProcessorFamily, Key, Value)');
                const product = database.prepare("INSERT OR IGNORE INTO product_installation_info VALUES (?, ?, '64Bit', 1)");
                const meta = database.prepare("INSERT INTO product_installation_meta_info VALUES (?, ?, '64Bit', ?, ?)");
                REGISTRY.forEach(([code, version, key, value]) => {
                    product.run(code, version);
                    meta.run(code, version, key, value);
                });
                database.close();
            }),
            Effect.succeed(registered),
        );
        return yield* Effect.provideService(Application.fromInstallations(offered), Registry, file);
    },
    Effect.scoped,
    Effect.provide(layer),
);
const facts = (applications: readonly Application[]) => applications.map(({ url, product, channel, version, build }) => [url.pathname, product, channel, version, Option.getOrUndefined(build)]);

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('Application.fromInstallations', () => {
    it.effect.each([
        {
            registry: 'the registry row whose AppLaunch names its executable, a BETA code naming the Beta of its product',
            registered: true,
            expected: [
                ['/Volumes/Archive/Adobe%20Acrobat.app/', 'com.adobe.Acrobat.Pro', 'release', '26.10.0', undefined],
                ['/Applications/Adobe%20Acrobat%20DC/Adobe%20Acrobat.app/', 'com.adobe.Acrobat.Pro', 'release', '26.3.0', undefined],
                ['/Applications/Adobe%20Illustrator%20(Beta)/Adobe%20Illustrator.app/', 'ILST', 'beta', '31.1.0', '31.1.0.127'],
                ['/Applications/Adobe%20Illustrator%202027/Adobe%20Illustrator.app/', 'ILST', 'release', '31.0.0', '31.0.0.84'],
                ['/Applications/Adobe%20Photoshop%20(Beta)/Adobe%20Photoshop%20(Beta).app/', 'PHSP', 'beta', '28.2.0', '28.2.0.3749'],
            ],
        },
        {
            registry: 'no registry, each a release under its bundle identifier',
            registered: false,
            expected: [
                ['/Volumes/Archive/Adobe%20Acrobat.app/', 'com.adobe.Acrobat.Pro', 'release', '26.10.0', undefined],
                ['/Applications/Adobe%20Acrobat%20DC/Adobe%20Acrobat.app/', 'com.adobe.Acrobat.Pro', 'release', '26.3.0', undefined],
                ['/Applications/Adobe%20Illustrator%202027/Adobe%20Illustrator.app/', 'com.adobe.illustrator', 'release', '31.0.0', undefined],
                ['/Applications/Adobe%20Illustrator%20(Beta)/Adobe%20Illustrator.app/', 'com.adobe.illustratorBeta', 'release', '31.1.0', undefined],
                ['/Applications/Adobe%20Photoshop%20(Beta)/Adobe%20Photoshop%20(Beta).app/', 'com.adobe.Photoshop', 'release', '28.2.0', undefined],
            ],
        },
    ])('joins each versioned installation to $registry, newest form of a product first', ({ registered, expected }) => Effect.map(listed(registered), (applications) => assert.deepStrictEqual(facts(applications), expected)));
});
