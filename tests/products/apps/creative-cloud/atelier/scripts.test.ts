import { createServer } from 'node:http';
import { DatabaseSync } from 'node:sqlite';
import { pathToFileURL } from 'node:url';
import { runInNewContext } from 'node:vm';
import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { layer as services } from '@effect/platform-node/NodeServices';
import { layer as sockets } from '@effect/platform-node/NodeSocketServer';
import { assert, layer } from '@effect/vitest';
import { ApplicationRpcs, Installation, InstanceSnapshot, ScriptingDictionary } from '@rasm/applications/contract';
import { routed } from '@rasm/testing/playwright';
import { parse } from 'acorn';
import { Array, Cause, Context, Effect, Equal, FileSystem, Layer, MutableRef, Option, Order, Path, Predicate, Record, Result, Schedule, Schema, Stream, Struct } from 'effect';
import { McpClient, McpProtocol } from 'effect/ai';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { RpcSerialization, RpcServer } from 'effect/rpc';
import getPort, { portNumbers } from 'get-port';
import { build } from 'plist';

// --- [TYPES] ---------------------------------------------------------------------------

interface Sent {
    readonly instance: string;
    readonly arguments: readonly string[];
    readonly direct: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SCRIPT = { name: 'do script', code: 'miscdosc', parameters: [{ name: 'file', code: 'file', types: [], list: false, enumerators: [] }] };
const DISTILL = {
    name: 'Distill',
    code: 'DSTLDIST',
    parameters: [
        { name: 'sourcePath', code: 'DSRC', types: [], list: false, enumerators: [] },
        { name: 'destinationPath', code: 'DDST', types: [], list: false, enumerators: [] },
        { name: 'adobePDFSettingsPath', code: 'DAPS', types: [], list: false, enumerators: [] },
    ],
};
const APPLICATIONS = [
    { name: 'Adobe Acrobat', identifier: 'com.adobe.Acrobat.Pro', version: '26.003.22110', dictionary: { suite: [{ name: 'Acrobat Viewer Suite', code: 'CARO' }], command: [SCRIPT], script: SCRIPT } },
    { name: 'Acrobat Distiller', identifier: 'com.adobe.distiller', version: '26.003.22110', dictionary: { command: [DISTILL] } },
    { name: 'Adobe Photoshop 2027', identifier: 'com.adobe.Photoshop', version: '28.2.0', dictionary: { command: [SCRIPT], script: SCRIPT } },
    { name: 'Adobe After Effects 2027', identifier: 'com.adobe.AfterEffects.application', version: '27.1.0', dictionary: { command: [SCRIPT], script: SCRIPT } },
    { name: 'Adobe Illustrator 2027 (Beta)', identifier: 'com.adobe.illustratorBeta', version: '31.1.0', dictionary: { command: [SCRIPT], script: SCRIPT } },
    { name: 'Adobe Illustrator 2027', identifier: 'com.adobe.illustrator', version: '31.0.0', dictionary: { command: [SCRIPT], script: SCRIPT } },
    { name: 'Adobe Media Encoder (Beta)', identifier: 'com.adobe.amebeta.application.27', version: '27.1.0', dictionary: { command: [SCRIPT], script: SCRIPT } },
] as const;
const REGISTERED = {
    'Adobe Photoshop 2027': { code: 'PHSP', version: '28.2', build: '28.2.0.36' },
    'Adobe After Effects 2027': { code: 'AEFT', version: '27.1', build: '27.1.0.12' },
    'Adobe Illustrator 2027 (Beta)': { code: 'ILSTBETA', version: '31.1', build: '31.1.0.127' },
    'Adobe Illustrator 2027': { code: 'ILST', version: '31.0', build: '31.0.0.84' },
    'Adobe Media Encoder (Beta)': { code: 'AMEBETA', version: '27.1', build: '27.1.0.57' },
} as const;
const BUNDLES: Record.ReadonlyRecord<string, { readonly folder: string; readonly file: string; readonly bundleName: string }> = {
    'Adobe Illustrator 2027 (Beta)': { folder: 'Adobe Illustrator (Beta)', file: 'Adobe Illustrator', bundleName: 'Illustrator (Beta)' },
    'Adobe Illustrator 2027': { folder: 'Adobe Illustrator 2027', file: 'Adobe Illustrator', bundleName: 'Illustrator' },
};
const ILLUSTRATOR_PREFERENCES = {
    'Adobe Illustrator 31.1.0 Beta Settings': { version: '31.1.0', plugin: '/plugin {\t/AIMCPServer {\t\t/AuthToken [ 9\t\t\t696c73745f62657461\t\t]\t\t/ServerEnabled 1\t}}' },
    'Adobe Illustrator 31 Settings': { version: '31.0.0', plugin: '/plugin {}' },
} as const;
const PLUGINS_INFO = { PS: '28.0.0', aftereffects: '27.2.0' } as const;
const FOREIGN = { pluginId: 'com.example.panel', status: 'enabled', type: 'uxp', hostMinVersion: '1.0.0' } as const;
const REGISTRY = [
    'CREATE TABLE product_installation_info ( SAPCode varchar(36), ProductVersion varchar(36), ProcessorFamily varchar(36), Status INT, constraint pk PRIMARY KEY (SAPCode, ProductVersion, ProcessorFamily) );',
    'CREATE TABLE product_installation_meta_info ( SAPCode varchar(36), ProductVersion varchar(36), ProcessorFamily varchar(36), Key TEXT, Value TEXT, constraint pk PRIMARY KEY (SAPCode, ProductVersion, ProcessorFamily, Key) );',
].join('\n');
const MARK = 'marked';
const ES3 = { ecmaVersion: 3, allowReserved: 'never' } as const;
const ES5 = { ecmaVersion: 5 } as const;
const DIALOGS = Record.fromIterableWith(['alert', 'confirm', 'prompt'], (name) => [
    name,
    () => {
        throw new Error(`host ${name}`);
    },
]);

// --- [MODELS] --------------------------------------------------------------------------

const Instances = InstanceSnapshot.pipe(Schema.toCodecJson, Schema.Array);
const Installations = Installation.pipe(Schema.toCodecJson, Schema.Array);
const Opened = Schema.fromJsonString(Schema.Struct({ instance: Schema.String }));
const Text = Schema.Struct({ text: Schema.String });
const Answered = Schema.fromJsonString(Schema.Struct({ result: Schema.Json }));
const Listing = Schema.fromJsonString(Schema.Struct({ installed: Schema.Array(Schema.Struct({ name: Schema.String, product: Schema.String, channel: Schema.String, version: Schema.String, build: Schema.optionalKey(Schema.String) })) }));
const Reply = Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Schema.Unknown)));

// --- [SERVICES] ------------------------------------------------------------------------

class Suite extends Context.Service<Suite>()('@rasm/test-atelier/scripts.test/Suite', {
    make: Effect.gen(function* () {
        const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
        const built = yield* Effect.fromOption(routed('@rasm/nx-vite', '@rasm/atelier'), () => new Cause.NoSuchElementError('nx.json states no Vite outputPath to run @rasm/atelier from'));
        const root = yield* fs.makeTempDirectoryScoped({ prefix: 'atelier-' });
        const port = yield* Effect.promise(() => getPort());
        const listening = yield* Effect.all(Record.map(BUNDLES, () => Effect.promise(() => getPort({ port: portNumbers(18_412, 18_422) }))));
        const [contents, socket, home] = [path.join(root, 'Atelier.app', 'Contents'), path.join(root, 'server.sock'), path.join(root, 'home')];
        const [registry, installs] = [path.join(root, 'hdpim.db'), path.join(root, 'Applications')];
        const placed = (name: string) => Option.getOrElse(Record.get(BUNDLES, name), () => ({ folder: name, file: name }));
        const bundle = (name: string) => path.join(installs, placed(name).folder, `${placed(name).file}.app`);
        const located = (name: string) => pathToFileURL(`${bundle(name)}/`).href;
        const empty = Struct.Record(ScriptingDictionary.Kind.literals, []);
        const dictionaries = Record.fromEntries(yield* Effect.forEach(APPLICATIONS, ({ name, dictionary }) => Effect.map(Schema.decodeEffect(ScriptingDictionary)({ ...empty, ...dictionary }), (terms) => [located(name), terms] as const)));
        const installations = yield* Schema.decodeEffect(Installations)(
            APPLICATIONS.map(({ name, identifier, version }) => ({
                url: located(name),
                executable: pathToFileURL(path.join(bundle(name), 'Contents', 'MacOS', placed(name).file)).href,
                identifier,
                name,
                ...Option.getOrUndefined(Option.map(Record.get(BUNDLES, name), Struct.pick(['bundleName']))),
                version,
                scriptable: true,
                activationPolicy: 'regular',
                appIntents: [],
            })),
        );
        const instances = yield* Schema.decodeEffect(Instances)(
            APPLICATIONS.map(({ name, identifier }, at) => ({ pid: 100 + at, instance: `${100 + at}@1`, identifier, url: located(name), activationPolicy: 'regular', state: { _tag: 'running', active: false, status: { _tag: 'responding' }, windows: [] }, listening: Option.toArray(Record.get(listening, name)) })),
        );
        const [executed, heard, folders] = [MutableRef.make<readonly Sent[]>([]), MutableRef.make<readonly { readonly port: Option.Option<number>; readonly authorization: string | undefined }[]>([]), MutableRef.make<readonly { readonly url: string; readonly glob?: string }[]>([])];
        const files: Record<string, string> = {
            [path.join(contents, 'Info.plist')]: build({ CFBundleExecutable: 'Atelier', CFBundleIdentifier: 'app.rasm.atelier.suite', CFBundleShortVersionString: '0.0.0' }),
            [path.join(contents, 'MacOS', 'Atelier')]: `#!/bin/sh\nexec /usr/bin/nc -U '${socket}'\n`,
            ...Record.fromEntries(
                Record.collect(PLUGINS_INFO, (app, hostMinVersion) => [path.join(home, 'Library', 'Application Support', 'Adobe', 'UXP', 'PluginsInfo', 'v1', `${app}.json`), JSON.stringify({ plugins: [{ pluginId: `app.rasm.atelier.suite.${app}`, status: 'enabled', type: 'uxp', hostMinVersion }, FOREIGN] })]),
            ),
            ...Record.fromEntries(
                Array.flatten(
                    Record.collect(ILLUSTRATOR_PREFERENCES, (settings, { version, plugin }) => [
                        [path.join(home, 'Library', 'Preferences', settings, 'en_US', '.runConfig'), JSON.stringify({ [version]: {} })] as const,
                        [path.join(home, 'Library', 'Preferences', settings, 'en_US', 'Adobe Illustrator Prefs'), plugin] as const,
                    ]),
                ),
            ),
            ...Record.fromEntries(Array.map(['Adobe Adobe Media Encoder (Beta)', 'Adobe Illustrator', 'AdobeCRCommon'], (folder) => [path.join(home, 'Library', 'Logs', 'Adobe', folder, 'UXPLogs_2026-11-26_05-12-31_259536.log'), 'upic::Upic version 2.7.0 is initialized\n'])),
        };
        const products = Record.collect(REGISTERED, (_, { code, version }) => [code, version, 'arm64', 1] as const);
        const facts = Array.flatten(
            Record.collect(REGISTERED, (name, { code, version, build: registered }) =>
                Record.collect({ InstallDir: path.dirname(bundle(name)), AppLaunch: `[InstallDir]/${placed(name).file}.app/Contents/MacOS/${placed(name).file}`, BuildVersion: registered }, (key, value) => [code, version, 'arm64', key, value] as const),
            ),
        );
        const database = yield* Effect.acquireRelease(
            Effect.sync(() => new DatabaseSync(registry)),
            (opened) => Effect.sync(() => opened.close()),
        );
        const [product, fact] = yield* Effect.sync(() => {
            database.exec(REGISTRY);
            return [database.prepare('INSERT INTO product_installation_info VALUES (?, ?, ?, ?)'), database.prepare('INSERT INTO product_installation_meta_info VALUES (?, ?, ?, ?, ?)')] as const;
        });
        yield* Effect.forEach(products, (row) => Effect.sync(() => product.run(...row)), { discard: true });
        yield* Effect.forEach(facts, (row) => Effect.sync(() => fact.run(...row)), { discard: true });
        yield* Effect.forEach(Record.toEntries(files), ([file, text]) => Effect.andThen(fs.makeDirectory(path.dirname(file), { recursive: true }), fs.writeFileString(file, text)), { discard: true });
        yield* fs.chmod(path.join(contents, 'MacOS', 'Atelier'), 0o755);
        yield* Effect.forEach(['Helpers', 'Resources'], (folder) => fs.makeDirectory(path.join(contents, folder)), { discard: true });
        yield* Effect.all(
            [
                fs.link(path.join(built, 'server'), path.join(contents, 'Helpers', 'server')),
                fs.link(path.join(built, 'tsc'), path.join(contents, 'Helpers', 'tsc')),
                fs.copyFile(path.join(built, 'typescript', 'lib.d.ts'), path.join(contents, 'Resources', 'lib.d.ts')),
                fs.symlink(path.join('..', 'Resources', 'lib.d.ts'), path.join(contents, 'Helpers', 'lib.d.ts')),
            ],
            { discard: true },
        );
        const answering = Record.values(listening).map((listened) =>
            createServer((request, response) => {
                MutableRef.update(heard, Array.append({ port: Option.fromUndefinedOr(request.socket.localPort), authorization: request.headers.authorization }));
                response.writeHead(401, { 'content-type': 'application/json' }).end('{}');
            }).listen(listened, '127.0.0.1'),
        );
        yield* Effect.addFinalizer(() =>
            Effect.sync(() => {
                for (const server of answering) {
                    server.closeAllConnections();
                    server.close();
                }
            }),
        );
        const served = ApplicationRpcs.omit('installation', 'pasteboard', 'model', 'intern', 'handlers', 'launch', 'act', 'quit', 'capture', 'record', 'scale');
        yield* served.pipe(
            RpcServer.layer,
            Layer.provide([
                served.toLayer({
                    installations: () => Stream.concat(Stream.succeed({ installations, unreadable: [] }), Stream.never),
                    instances: () => Stream.concat(Stream.succeed(Result.succeed(instances)), Stream.never),
                    release: () => Effect.void,
                    events: () => Stream.never,
                    observations: () => Stream.never,
                    pressure: () => Stream.never,
                    session: () => Stream.concat(Stream.succeed(Result.succeed({ onConsole: true, locked: false, asleep: false, secureInput: false, spaces: [1] })), Stream.never),
                    watch: ({ instance }) =>
                        Array.findFirst(instances, (row) => row.instance === instance).pipe(
                            Effect.fromOption,
                            Effect.map((row) => ({ ...row, language: 'en' })),
                            Effect.orDie,
                        ),
                    dictionary: ({ url }) => Effect.orDie(Effect.fromNullishOr(dictionaries[url.href])),
                    execute: (payload) => executing(executed, payload),
                    diagnostics: (payload) =>
                        Effect.sync(() => {
                            MutableRef.set(
                                folders,
                                payload.folders.map(({ url, glob }) => ({ url: url.href, ...Record.getSomes({ glob: Option.fromUndefinedOr(glob) }) })),
                            );
                            return { reports: [], samples: [], logs: [], unified: Result.succeed([]), unreadable: [] };
                        }),
                }),
                RpcServer.layerProtocolSocketServer,
            ]),
            Layer.provide([sockets({ path: socket }), RpcSerialization.layerNdJsonRpc()]),
            Layer.build,
        );
        const server = yield* spawner.spawn(ChildProcess.make(path.join(contents, 'Helpers', 'server'), ['--listen', `127.0.0.1:${port}`, '--output-dir', path.join(root, 'output'), '--registry', registry], { env: { HOME: home }, extendEnv: true, stdout: 'ignore', stderr: 'inherit' }));
        const connected = McpClient.make({ name: 'atelier-suite', version: '1.0.0' }).pipe(Effect.provideServiceEffect(McpClient.Transport, McpClient.http({ url: `http://127.0.0.1:${port}/`, protocol: McpProtocol.v2025_11_25 })));
        const client = yield* Effect.raceFirst(
            connected.pipe(Effect.retry({ schedule: Schedule.spaced('100 millis') }), Effect.timeout('20 seconds')),
            Effect.flatMap(server.exitCode, (code) => Effect.fail(new Cause.UnknownError(code, `Atelier exits ${code} before it answers MCP`))),
        );
        const call = Effect.fnUntraced(function* (name: string, given: Record<string, unknown>) {
            const tool = yield* client.listTools().pipe(Effect.map(Array.findFirst((listed) => listed.name === name)), Effect.flatMap(Effect.fromOption));
            return yield* client.callTool({ tool, arguments: given });
        });
        const logged = (text: string) =>
            fs.readFileString(path.join(root, 'output', 'suite', 'logs', 'server.log')).pipe(
                Effect.filterOrFail((read) => read.includes(text)),
                Effect.retry({ schedule: Schedule.spaced('200 millis') }),
                Effect.timeout('20 seconds'),
            );
        return {
            call,
            logged,
            executed: Effect.sync(() => MutableRef.get(executed)),
            heard: (name: string) =>
                Effect.sync(() =>
                    MutableRef.get(heard)
                        .filter((each) => Equal.equals(each.port, Record.get(listening, name)))
                        .map(Struct.get('authorization')),
                ),
            folders: Effect.sync(() => MutableRef.get(folders)),
        };
    }),
}) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const shown = (names: readonly string[]) =>
    Record.fromIterableWith(names, (name) => [
        name,
        () => {
            throw new Error(`host ${name}`);
        },
    ]);
const host = () => {
    const field = { fileSelect: true, ...shown(['browseForFileToSubmit', 'buttonImportIcon']) };
    const doc = {
        path: '/tmp/a.pdf',
        dirty: true,
        info: { Title: 'probe' },
        getField: () => field,
        closeDoc(this: unknown, discard: unknown) {
            if (this !== doc || discard !== true) {
                throw new Error('host closeDoc prompt or a wrapper as this');
            }
            return 'closed';
        },
        ...shown(['importIcon', 'importTextData', 'importDataObject', 'exportAsFDF', 'saveAs']),
    };
    const app = {
        viewerVersion: 26.003,
        runtimeHighlight: true,
        activeDocs: [doc],
        openDoc(this: unknown, given: unknown) {
            const [path, base] = Predicate.hasProperty(given, 'cPath') ? [given.cPath, Predicate.hasProperty(given, 'oDoc') ? given.oDoc : undefined] : [given, doc];
            if (this !== app || base !== doc) {
                throw new Error('openDoc received a wrapper of app or of a Doc');
            }
            return path;
        },
        trustedFunction:
            (body: (...args: readonly unknown[]) => unknown) =>
            (...args: readonly unknown[]) =>
                body(...args),
        trustPropagatorFunction: (body: unknown) => body,
        execMenuItem: (item: string) => `ran ${item}`,
        ...shown(['alert', 'response', 'browseForDoc', 'popUpMenu', 'popUpMenuEx', 'execDialog', 'mailGetAddrs', 'mailMsg']),
    };
    return { app, event: { target: doc }, spell: shown(['check']), util: shown(['readFileIntoStream']) };
};
const executing = (executed: MutableRef.MutableRef<readonly Sent[]>, { instance, arguments: given }: { readonly instance: string; readonly arguments: Record.ReadonlyRecord<string, unknown> }) =>
    Effect.orDie(Schema.decodeUnknownEffect(Schema.String)(given['direct'])).pipe(
        Effect.tap((direct) => Effect.sync(() => MutableRef.update(executed, Array.append({ instance, arguments: Record.keys(given), direct })))),
        Effect.flatMap((direct) => {
            const line = direct.split('\n').findIndex((text) => text.includes(MARK)) + 1;
            return line > 0 ? Effect.orDie(Schema.encodeEffect(Reply)({ _tag: 'Thrown', message: MARK, line })) : Effect.orDie(Schema.decodeUnknownEffect(Schema.String)(runInNewContext(direct, { ...DIALOGS, ...host() })));
        }),
        Effect.map((reply) => ({ reply, written: Result.succeed([]) })),
    );
const lines = ({ content }: { readonly content: readonly unknown[] }) =>
    Array.map(Array.getSomes(Array.map(content, (block) => Schema.decodeUnknownOption(Text)(block))), Struct.get('text'))
        .join('\n')
        .split('\n');
const ran = Effect.fnUntraced(function* (name: string, code: string) {
    const suite = yield* Suite;
    const [issued = '', ...rest] = lines(yield* suite.call('applications', { action: 'open', application: name }));
    const { instance } = yield* Schema.decodeUnknownEffect(Opened)(rest.at(-1));
    const before = (yield* suite.executed).length;
    const result = yield* suite.call('execute_script', { code, handle: issued.split(' ')[1] });
    const text = lines(result);
    return { instance, sent: Array.drop(yield* suite.executed, before), failed: result.isError === true, text: text.join('\n'), answer: Schema.decodeUnknownOption(Answered)(text.at(-1)) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(Layer.effect(Suite, Suite.make).pipe(Layer.provide([services, layerUndici])), { excludeTestServices: true })('built server', (it) => {
    it.effect(
        'lists each installation the application server offers, joined to the product and build of its Creative Cloud registry row',
        Effect.fnUntraced(function* () {
            const suite = yield* Suite;
            const { installed } = yield* Effect.flatMap(suite.call('applications', {}), (result) => Schema.decodeUnknownEffect(Listing)(lines(result).at(-1)));
            assert.deepStrictEqual(Array.sort(installed, Order.Struct({ name: Order.String })), [
                { name: 'Acrobat Distiller', product: 'com.adobe.distiller', channel: 'release', version: '26.003.22110' },
                { name: 'Adobe Acrobat', product: 'com.adobe.Acrobat.Pro', channel: 'release', version: '26.003.22110' },
                { name: 'Adobe After Effects 2027', product: 'AEFT', channel: 'release', version: '27.1.0', build: '27.1.0.12' },
                { name: 'Adobe Illustrator 2027', product: 'ILST', channel: 'release', version: '31.0.0', build: '31.0.0.84' },
                { name: 'Adobe Illustrator 2027 (Beta)', product: 'ILST', channel: 'beta', version: '31.1.0', build: '31.1.0.127' },
                { name: 'Adobe Media Encoder (Beta)', product: 'AME', channel: 'beta', version: '27.1.0', build: '27.1.0.57' },
                { name: 'Adobe Photoshop 2027', product: 'PHSP', channel: 'release', version: '28.2.0', build: '28.2.0.36' },
            ]);
        }),
    );

    it.effect(
        'opens the Beta of a product its product code or a name short of every listed one reaches, and the year release by its listed name alone',
        Effect.fnUntraced(function* () {
            const suite = yield* Suite;
            const opened = (application: string) => Effect.flatMap(suite.call('applications', { action: 'open', application }), (result) => Schema.decodeUnknownEffect(Opened)(lines(result).at(-1)));
            const [beta, release] = yield* Effect.all([opened('Adobe Illustrator 2027 (Beta)'), opened('Adobe Illustrator 2027')]);
            const reached = yield* Effect.forEach(['ILST', 'Illustrator', 'Adobe Illustrator'], opened);
            assert.notStrictEqual(beta.instance, release.instance);
            assert.deepStrictEqual(Array.map(reached, Struct.get('instance')), [beta.instance, beta.instance, beta.instance]);
        }),
    );

    it.effect(
        "sends Illustrator's MCP server the bearer token the preferences of its build hold",
        Effect.fnUntraced(function* () {
            const suite = yield* Suite;
            const heard = yield* suite.heard('Adobe Illustrator 2027 (Beta)').pipe(
                Effect.filterOrFail((held) => held.length > 0),
                Effect.retry({ schedule: Schedule.spaced('200 millis') }),
                Effect.timeout('20 seconds'),
            );
            assert.deepStrictEqual(Array.dedupe(heard), ['Bearer ilst_beta']);
        }),
    );

    it.effect(
        "sends nothing to Illustrator's MCP server while its build's preferences hold no token, logging the missing AIMCPServer AuthToken in place of a refused login",
        Effect.fnUntraced(function* () {
            const suite = yield* Suite;
            yield* suite.logged('Adobe Illustrator 2027 preferences hold no AIMCPServer AuthToken');
            assert.deepStrictEqual(yield* suite.heard('Adobe Illustrator 2027'), []);
        }),
    );

    it.effect(
        'reads the UXP logs of an application from the log folder whose words its name holds, a folder doubling the vendor word included',
        Effect.fnUntraced(function* () {
            const suite = yield* Suite;
            const result = yield* suite.call('diagnostics', { application: 'Adobe Media Encoder (Beta)' });
            const [folder] = yield* suite.folders;
            assert.deepStrictEqual([result.isError === true, folder?.url.endsWith('/Library/Logs/Adobe/Adobe%20Adobe%20Media%20Encoder%20(Beta)/'), folder?.glob], [false, true, 'UXPLogs_*.log'], lines(result).join('\n'));
        }),
    );

    it.effect(
        'refuses a body for a host whose PluginsInfo lists an enabled Atelier plugin at or below its version, naming the unlinked plugin and sending nothing',
        Effect.fnUntraced(function* () {
            const { sent, failed, text } = yield* ran('Adobe Photoshop 2027', 'return 7;');
            assert.deepStrictEqual([sent, failed, text.includes('linked Atelier plugin')], [[], true, true], text);
        }),
    );

    it.effect(
        'runs a body through do script for a host whose Atelier plugin floor sits above its version, an enabled plugin of another vendor counting for none',
        Effect.fnUntraced(function* () {
            const { sent, failed, answer, text } = yield* ran('Adobe After Effects 2027', 'return 7;');
            assert.deepStrictEqual([sent.length, failed, answer], [1, false, Option.some({ result: 7 })], text);
            for (const { direct } of sent) {
                assert.doesNotThrow(() => {
                    parse(direct, ES3);
                });
            }
        }),
    );

    it.effect(
        'runs a body through the do script of an application declaring the Acrobat Viewer Suite, sending to its instance one ES5 script SpiderMonkey 24 parses as the direct parameter alone, and answers what that script returns',
        Effect.fnUntraced(function* () {
            const { instance, sent, failed, answer } = yield* ran('Adobe Acrobat', 'const doubled: number = [3.5].map((each) => each * 2)[0];\nreturn doubled;');
            assert.deepStrictEqual([Array.map(sent, Struct.pick(['instance', 'arguments'])), failed, answer], [[{ instance, arguments: ['direct'] }], false, Option.some({ result: 7 })]);
            for (const { direct } of sent) {
                assert.doesNotThrow(() => {
                    parse(direct, ES5);
                });
            }
        }),
    );

    it.effect.each([
        { named: 'a throw the host reports at a lowered script line', code: `const first = 1;\nconst second = first + 1;\nthrow new Error('${MARK}' + second);`, line: 3, sends: 1 },
        { named: 'a syntax error, sending nothing to the host', code: 'const first = 1;\nconst = 2;\nreturn first;', line: 2, sends: 0 },
    ])(
        'names the body line of $named',
        Effect.fnUntraced(function* ({ code, line, sends }) {
            const { sent, failed, text } = yield* ran('Adobe Acrobat', code);
            assert.deepStrictEqual([sent.length, failed, new RegExp(`\\bline ${line}\\b`, 'u').test(text), text.includes(MARK)], [sends, true, true, sends > 0], text);
        }),
    );

    it.effect(
        'answers alert with undefined, confirm with false, and prompt with null in a do script body, reaching no host dialog',
        Effect.fnUntraced(function* () {
            const { failed, answer, text } = yield* ran('Adobe Acrobat', "return Number(alert('saved') === undefined) + Number(confirm('replace?')) * 2 + Number(prompt('name', 'untitled') === null) * 4;");
            assert.deepStrictEqual([failed, answer], [false, Option.some({ result: 5 })], text);
        }),
    );

    it.effect(
        'answers each Acrobat app dialog as its dismissal without reaching the host member, leaving the check box as it began',
        Effect.fnUntraced(function* () {
            const { failed, answer, text } = yield* ran(
                'Adobe Acrobat',
                "const checkbox = { cMsg: 'again?', bInitialValue: true, bAfterValue: false };\nreturn [app.alert('saved'), app.alert({ cMsg: 'replace?', nType: 1 }), app.alert('keep?', 2, 2), app.alert({ cMsg: 'quit?', nType: 3, oCheckbox: checkbox }), checkbox.bAfterValue, app.response('name'), app.browseForDoc(), app.popUpMenu('one'), app.popUpMenuEx({ cName: 'one' }), app.execDialog({ description: {} })];",
            );
            assert.deepStrictEqual([failed, answer], [false, Option.some({ result: [1, 2, 3, 2, true, null, null, null, null, 'cancel'] })], text);
        }),
    );

    it.effect(
        'reaches every other Acrobat app member on app itself, its methods called with app as this and its writes landing on app',
        Effect.fnUntraced(function* () {
            const { failed, answer, text } = yield* ran('Adobe Acrobat', "app.runtimeHighlight = false;\nreturn [app.openDoc('/tmp/a.pdf'), app.viewerVersion, app.runtimeHighlight, 'openDoc' in app];");
            assert.deepStrictEqual([failed, answer], [false, Option.some({ result: ['/tmp/a.pdf', 26.003, false, true] })], text);
        }),
    );

    it.effect(
        'answers dialogs reached through this, event.target, activeDocs, getField, and callbacks the host calls, as each member states its dismissal, passing the host its own objects',
        Effect.fnUntraced(function* () {
            const { failed, answer, text } = yield* ran(
                'Adobe Acrobat',
                [
                    'const doc = event.target;',
                    'const field = doc.getField("file");',
                    "return [this.app.alert('this.app'), this.app === app, app.activeDocs[0] === doc, field.browseForFileToSubmit(), field.buttonImportIcon(), doc.importIcon('icon'), doc.importTextData(), doc.importTextData('/tmp/rows.txt'), doc.exportAsFDF(), app.openDoc({ cPath: '/tmp/b.pdf', oDoc: doc }), app.trustedFunction((given) => given.importIcon('icon'))(doc), app.trustPropagatorFunction((given) => given.importIcon('icon'))(doc), doc.info, spell.check(), app.mailGetAddrs(), app.mailMsg(true), app.execMenuItem('FitPage'), doc.closeDoc(true)];",
                ].join('\n'),
            );
            assert.deepStrictEqual([failed, answer], [false, Option.some({ result: [1, true, true, null, 1, 1, -1, -2, null, '/tmp/b.pdf', 1, 1, { Title: 'probe' }, false, null, null, 'ran FitPage', 'closed'] })], text);
        }),
    );

    it.effect.each([
        { named: 'a dialog menu item', code: "return app.execMenuItem('SaveAs');", names: 'SaveAs' },
        { named: 'closing a dirty document', code: 'return event.target.closeDoc();', names: '/tmp/a.pdf' },
        { named: 'a picker returning only a success value', code: "return event.target.importDataObject('data');", names: 'cDIPath' },
        { named: 'an overwrite prompt', code: "return event.target.saveAs({ cPath: '/tmp/c.pdf', bPromptToOverwrite: true });", names: 'bPromptToOverwrite' },
        { named: 'a global object picker', code: 'return util.readFileIntoStream();', names: 'readFileIntoStream' },
    ])(
        'refuses $named with a reply naming it, reaching no host member',
        Effect.fnUntraced(function* ({ code, names }) {
            const { sent, failed, text } = yield* ran('Adobe Acrobat', code);
            assert.deepStrictEqual([sent.length, failed, text.includes(names), text.includes('host ')], [1, true, true, false], text);
        }),
    );

    it.effect(
        'refuses an application whose dictionary declares commands but no script command, naming it and sending nothing',
        Effect.fnUntraced(function* () {
            const { sent, failed, text } = yield* ran('Acrobat Distiller', 'return 7;');
            assert.deepStrictEqual([sent, failed, text.includes('Acrobat Distiller')], [[], true, true], text);
        }),
    );
});
