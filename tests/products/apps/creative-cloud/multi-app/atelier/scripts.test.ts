import { once } from 'node:events';
import { createServer } from 'node:net';
import { DatabaseSync } from 'node:sqlite';
import { pathToFileURL } from 'node:url';
import { runInNewContext } from 'node:vm';
import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { layer as services } from '@effect/platform-node/NodeServices';
import { layer as sockets } from '@effect/platform-node/NodeSocketServer';
import { assert, layer } from '@effect/vitest';
import { Automation, RunningInstance, TermKind, Terms } from '@rasm/applications';
import { routed } from '@rasm/testing/playwright';
import { parse } from 'acorn';
import { Array, Cause, Config, Context, Effect, FileSystem, Layer, MutableRef, Option, Order, Path, Record, Result, Schedule, Schema, Stream, Struct } from 'effect';
import { McpClient, McpProtocol } from 'effect/ai';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { RpcSerialization, RpcServer } from 'effect/rpc';
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
const INSTALLED = [
    { receipt: 'com.adobe.acrobat.DC.scamini.app.pkg.MUI', name: 'Adobe Acrobat', identifier: 'com.adobe.Acrobat.Pro', version: '26.003.22110', dictionary: { command: [SCRIPT], script: SCRIPT } },
    { receipt: 'com.adobe.acrobat.DC.scafull.app.pkg.MUI', name: 'Acrobat Distiller', identifier: 'com.adobe.distiller', version: '26.003.22110', dictionary: { command: [DISTILL] } },
] as const;
const REGISTERED = [
    { code: 'IDSN', version: '21.0', name: 'Adobe InDesign 2027', identifier: 'com.adobe.InDesign', shortVersion: '21.0.1', build: '21.0.1.52', visible: 'true' },
    { code: 'KBRG', version: '16.0', name: 'Adobe Bridge 2027', identifier: 'com.adobe.bridge16', shortVersion: '16.0.2', build: '16.0.2.11', visible: 'false' },
] as const;
const REGISTRY = [
    'CREATE TABLE product_installation_info ( SAPCode varchar(36), ProductVersion varchar(36), ProcessorFamily varchar(36), Status INT, constraint pk PRIMARY KEY (SAPCode, ProductVersion, ProcessorFamily) );',
    'CREATE TABLE product_installation_meta_info ( SAPCode varchar(36), ProductVersion varchar(36), ProcessorFamily varchar(36), Key TEXT, Value TEXT, constraint pk PRIMARY KEY (SAPCode, ProductVersion, ProcessorFamily, Key) );',
].join('\n');
const MARK = 'marked';
const ES3 = { ecmaVersion: 3, allowReserved: 'never' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Bound = Schema.Struct({ port: Schema.Int });
const Instances = Schema.Array(Schema.toCodecJson(RunningInstance));
const Opened = Schema.fromJsonString(Schema.Struct({ instance: Schema.String }));
const Text = Schema.Struct({ text: Schema.String });
const Answered = Schema.fromJsonString(Schema.Struct({ result: Schema.Finite }));
const Listing = Schema.fromJsonString(Schema.Struct({ installed: Schema.Array(Schema.Struct({ name: Schema.String, product: Schema.String, channel: Schema.String, version: Schema.String, build: Schema.optionalKey(Schema.String) })) }));
const Reply = Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Schema.Unknown)));

// --- [SERVICES] ------------------------------------------------------------------------

class Suite extends Context.Service<Suite>()('@rasm/test-atelier/scripts.test/Suite', {
    make: Effect.gen(function* () {
        const [fs, path, spawner, searched] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, Config.String('PATH')]);
        const built = yield* Effect.fromOption(routed('@rasm/nx-vite', '@rasm/atelier'), () => new Cause.NoSuchElementError('nx.json states no Vite outputPath to run @rasm/atelier from'));
        const root = yield* fs.makeTempDirectoryScoped({ prefix: 'atelier-' });
        const probe = yield* Effect.sync(() => createServer().listen(0, '127.0.0.1'));
        yield* Effect.promise(() => once(probe, 'listening'));
        const { port } = yield* Schema.decodeUnknownEffect(Bound)(probe.address());
        yield* Effect.callback<void>((resume) => {
            probe.close(() => resume(Effect.void));
        });
        const location = 'Applications/Adobe Acrobat DC';
        const [contents, receipts, bin, socket] = [path.join(root, 'Atelier.app', 'Contents'), path.join(root, 'receipts'), path.join(root, 'bin'), path.join(root, 'server.sock')];
        const [registry, installs] = [path.join(root, 'hdpim.db'), path.join(root, 'Applications')];
        const bundle = (name: string) => path.join(root, location, `${name}.app`);
        const located = (name: string) => pathToFileURL(`${bundle(name)}/`).href;
        const empty = Struct.Record(TermKind.literals, []);
        const dictionaries = Record.fromEntries(yield* Effect.forEach(INSTALLED, ({ name, dictionary }) => Effect.map(Schema.decodeEffect(Terms)({ ...empty, ...dictionary }), (terms) => [located(name), terms] as const)));
        const instances = yield* Schema.decodeEffect(Instances)(INSTALLED.map(({ name, identifier }, at) => ({ pid: 100 + at, instance: `${100 + at}@1`, identifier, url: located(name), activationPolicy: 'regular', state: { running: { active: false, status: 'responding', windows: [] } }, listening: [] })));
        const executed = MutableRef.make<readonly Sent[]>([]);
        const files: Record<string, string> = {
            [path.join(contents, 'Info.plist')]: build({ CFBundleExecutable: 'Atelier', CFBundleIdentifier: 'app.rasm.atelier.suite', CFBundleShortVersionString: '0.0.0' }),
            [path.join(contents, 'MacOS', 'Atelier')]: `#!/bin/sh\nexec /usr/bin/nc -U '${socket}'\n`,
            [path.join(bin, 'pkgutil')]: `#!/bin/sh\ncase "$1" in\n--pkgs) printf '%s\\n' ${INSTALLED.map(({ receipt }) => receipt).join(' ')} ;;\n--pkg-info-plist) cat '${receipts}/'"$2".plist ;;\n--only-dirs) cat '${receipts}/'"$3".txt ;;\nesac\n`,
            ...Record.fromEntries(
                INSTALLED.flatMap(({ receipt, name, identifier, version }) => [
                    [path.join(receipts, `${receipt}.plist`), build({ pkgid: receipt, 'pkg-version': version, volume: root, 'install-location': location })],
                    [path.join(receipts, `${receipt}.txt`), `${name}.app\n${name}.app/Contents\n`],
                    [path.join(bundle(name), 'Contents', 'Info.plist'), build({ CFBundleIdentifier: identifier, CFBundleShortVersionString: version })],
                ]),
            ),
            ...Record.fromEntries(REGISTERED.map(({ name, identifier, shortVersion }) => [path.join(installs, name, `${name}.app`, 'Contents', 'Info.plist'), build({ CFBundleIdentifier: identifier, CFBundleShortVersionString: shortVersion })])),
        };
        const products = REGISTERED.map(({ code, version }) => [code, version, 'arm64', 1] as const);
        const facts = REGISTERED.flatMap(({ code, version, name, build: registered, visible }) =>
            Record.toEntries({ InstallDir: path.join(installs, name), AppLaunch: `[InstallDir]/${name}.app/Contents/MacOS/${name}`, BuildVersion: registered, isVisibleProduct: visible }).map(([key, value]) => [code, version, 'arm64', key, value] as const),
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
        yield* Effect.forEach([path.join(contents, 'MacOS', 'Atelier'), path.join(bin, 'pkgutil')], (file) => fs.chmod(file, 0o755), { discard: true });
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
        const served = Automation.omit('nodes', 'handler', 'launch', 'act', 'quit', 'capture', 'preview', 'diagnostics');
        yield* Layer.build(
            RpcServer.layer(served).pipe(
                Layer.provide([
                    served.toLayer({
                        installations: () => Stream.concat(Stream.succeed(Result.succeed([])), Stream.never),
                        instances: () => Stream.concat(Stream.succeed(instances), Stream.never),
                        events: () => Stream.never,
                        pressure: () => Stream.never,
                        terms: ({ url }) => Effect.orDie(Effect.fromNullishOr(dictionaries[url.href])),
                        execute: (payload) => executing(executed, payload),
                    }),
                    RpcServer.layerProtocolSocketServer,
                ]),
                Layer.provide([sockets({ path: socket }), RpcSerialization.layerNdJsonRpc()]),
            ),
        );
        const server = yield* spawner.spawn(
            ChildProcess.make(path.join(contents, 'Helpers', 'server'), ['--listen', `127.0.0.1:${port}`, '--output', path.join(root, 'output'), '--registry', registry], { env: { HOME: path.join(root, 'home'), PATH: `${bin}:${searched}` }, extendEnv: true, stdout: 'ignore', stderr: 'inherit' }),
        );
        const connected = McpClient.make({ name: 'atelier-suite', version: '1.0.0' }).pipe(Effect.provideServiceEffect(McpClient.Transport, McpClient.http({ url: `http://127.0.0.1:${port}/`, protocol: McpProtocol.v2025_11_25 })));
        const client = yield* Effect.raceFirst(
            connected.pipe(Effect.retry({ schedule: Schedule.spaced('100 millis') }), Effect.timeout('20 seconds')),
            Effect.flatMap(server.exitCode, (code) => Effect.fail(new Cause.UnknownError(code, `Atelier exits ${code} before it answers MCP`))),
        );
        const call = Effect.fnUntraced(function* (name: string, given: Record<string, unknown>) {
            const tool = yield* client.listTools().pipe(Effect.map(Array.findFirst((listed) => listed.name === name)), Effect.flatMap(Effect.fromOption));
            return yield* client.callTool({ tool, arguments: given });
        });
        return { call, executed: Effect.sync(() => MutableRef.get(executed)) };
    }),
}) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const executing = (executed: MutableRef.MutableRef<readonly Sent[]>, { instance, arguments: given }: { readonly instance: string; readonly arguments: Record.ReadonlyRecord<string, unknown> }) =>
    Effect.orDie(Schema.decodeUnknownEffect(Schema.String)(given['direct'])).pipe(
        Effect.tap((direct) => Effect.sync(() => MutableRef.update(executed, Array.append({ instance, arguments: Record.keys(given), direct })))),
        Effect.flatMap((direct) => {
            const line = direct.split('\n').findIndex((text) => text.includes(MARK)) + 1;
            return line > 0 ? Effect.orDie(Schema.encodeEffect(Reply)({ _tag: 'Thrown', message: MARK, line })) : Effect.orDie(Schema.decodeUnknownEffect(Schema.String)(runInNewContext(direct, {})));
        }),
    );
const lines = ({ content }: { readonly content: readonly unknown[] }) =>
    Array.map(Array.getSomes(Array.map(content, (block) => Schema.decodeUnknownOption(Text)(block))), Struct.get('text'))
        .join('\n')
        .split('\n');
const ran = Effect.fnUntraced(function* (name: string, code: string) {
    const suite = yield* Suite;
    const [minted = '', ...rest] = lines(yield* suite.call('applications', { action: 'open', application: name }));
    const { instance } = yield* Schema.decodeUnknownEffect(Opened)(rest.at(-1));
    const before = (yield* suite.executed).length;
    const result = yield* suite.call('execute_script', { code, handle: minted.split(' ')[1] });
    const text = lines(result);
    return { instance, sent: Array.drop(yield* suite.executed, before), failed: result.isError === true, text: text.join('\n'), answer: Schema.decodeUnknownOption(Answered)(text.at(-1)) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(Layer.effect(Suite, Suite.make).pipe(Layer.provide([services, layerUndici])), { excludeTestServices: true })('built server', (it) => {
    it.effect(
        'lists exactly the applications its registry shows and its installer receipts name, leaving out a registered product hidden from view',
        Effect.fnUntraced(function* () {
            const suite = yield* Suite;
            const { installed } = yield* Effect.flatMap(suite.call('applications', {}), (result) => Schema.decodeUnknownEffect(Listing)(lines(result).at(-1)));
            assert.deepStrictEqual(Array.sort(installed, Order.Struct({ name: Order.String })), [
                { name: 'Acrobat Distiller', product: 'com.adobe.distiller', channel: 'release', version: '26.003.22110' },
                { name: 'Adobe Acrobat', product: 'com.adobe.Acrobat.Pro', channel: 'release', version: '26.003.22110' },
                { name: 'Adobe InDesign 2027', product: 'IDSN', channel: 'release', version: '21.0.1', build: '21.0.1.52' },
            ]);
        }),
    );

    it.effect(
        'runs a body through the do script of an application no UXP host lists, sending to its instance one strict ES3 script as the direct parameter alone, and answers what that script returns',
        Effect.fnUntraced(function* () {
            const { instance, sent, failed, answer } = yield* ran('Adobe Acrobat', 'const doubled: number = [3.5].map((each) => each * 2)[0];\nreturn doubled;');
            assert.deepStrictEqual([Array.map(sent, Struct.pick(['instance', 'arguments'])), failed, answer], [[{ instance, arguments: ['direct'] }], false, Option.some({ result: 7 })]);
            for (const { direct } of sent) {
                assert.doesNotThrow(() => {
                    parse(direct, ES3);
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
        'refuses an application whose dictionary declares commands but no script command, naming it and sending nothing',
        Effect.fnUntraced(function* () {
            const { sent, failed, text } = yield* ran('Acrobat Distiller', 'return 7;');
            assert.deepStrictEqual([sent, failed, text.includes('Acrobat Distiller')], [[], true, true], text);
        }),
    );
});
