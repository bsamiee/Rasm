import { once } from 'node:events';
import { createServer } from 'node:net';
import { pathToFileURL } from 'node:url';
import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { layer as services } from '@effect/platform-node/NodeServices';
import { layer as sockets } from '@effect/platform-node/NodeSocketServer';
import { assert, layer } from '@effect/vitest';
import { Automation, RunningInstance, TermKind, Terms } from '@rasm/applications';
import { routed } from '@rasm/testing/playwright';
import { Array, Cause, Config, Context, Effect, FileSystem, Layer, MutableRef, Option, Path, Record, Result, Schedule, Schema, Stream, Struct } from 'effect';
import { McpClient, McpProtocol } from 'effect/ai';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { RpcSerialization, RpcServer } from 'effect/rpc';
import { build } from 'plist';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SCRIPT = { name: 'do script', code: 'miscdosc', parameters: [{ name: 'file', code: 'file', list: false, enumerators: [] }] };
const DISTILL = {
    name: 'Distill',
    code: 'DSTLDIST',
    parameters: [
        { name: 'sourcePath', code: 'DSRC', list: false, enumerators: [] },
        { name: 'destinationPath', code: 'DDST', list: false, enumerators: [] },
        { name: 'adobePDFSettingsPath', code: 'DAPS', list: false, enumerators: [] },
    ],
};
const INSTALLED = [
    { receipt: 'com.adobe.acrobat.DC.scamini.app.pkg.MUI', name: 'Adobe Acrobat', identifier: 'com.adobe.Acrobat.Pro', version: '26.003.22110', dictionary: { command: [SCRIPT], script: SCRIPT } },
    { receipt: 'com.adobe.acrobat.DC.scafull.app.pkg.MUI', name: 'Acrobat Distiller', identifier: 'com.adobe.distiller', version: '26.003.22110', dictionary: { command: [DISTILL] } },
] as const;
const REPLY = encodeURIComponent(JSON.stringify({ _tag: 'Value', value: { result: 7 } }));

// --- [MODELS] --------------------------------------------------------------------------

const Bound = Schema.Struct({ port: Schema.Int });
const Instances = Schema.Array(Schema.toCodecJson(RunningInstance));
const Opened = Schema.Struct({ handle: Schema.String, result: Schema.Struct({ instance: Schema.String }) });
const Text = Schema.Struct({ text: Schema.String });

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
        const bundle = (name: string) => path.join(root, location, `${name}.app`);
        const located = (name: string) => pathToFileURL(`${bundle(name)}/`).href;
        const empty = Struct.Record(TermKind.literals, []);
        const dictionaries = Record.fromEntries(yield* Effect.forEach(INSTALLED, ({ name, dictionary }) => Effect.map(Schema.decodeEffect(Terms)({ ...empty, ...dictionary }), (terms) => [located(name), terms] as const)));
        const instances = yield* Schema.decodeEffect(Instances)(INSTALLED.map(({ name, identifier }, at) => ({ pid: 100 + at, instance: `${100 + at}@1`, identifier, url: located(name), activationPolicy: 'regular', state: { running: { active: false, status: 'responding', windows: [] } }, listening: [] })));
        const executed = MutableRef.make<readonly { readonly instance: string; readonly arguments: readonly string[] }[]>([]);
        const files: Record<string, string> = {
            [path.join(contents, 'Info.plist')]: build({ CFBundleExecutable: 'Atelier', CFBundleIdentifier: 'app.rasm.atelier.suite', CFBundleShortVersionString: '0.0.0' }),
            [path.join(contents, 'MacOS', 'Atelier')]: `#!/bin/sh\nexec /usr/bin/nc -U '${socket}'\n`,
            [path.join(bin, 'pkgutil')]: `#!/bin/sh\ncase "$1" in\n--pkgs=*) printf '%s\\n' ${INSTALLED.map(({ receipt }) => receipt).join(' ')} ;;\n--pkg-info-plist) cat '${receipts}/'"$2".plist ;;\n--only-dirs) cat '${receipts}/'"$3".txt ;;\nesac\n`,
            ...Record.fromEntries(
                INSTALLED.flatMap(({ receipt, name, identifier, version }) => [
                    [path.join(receipts, `${receipt}.plist`), build({ pkgid: receipt, 'pkg-version': version, volume: root, 'install-location': location })],
                    [path.join(receipts, `${receipt}.txt`), `${name}.app\n${name}.app/Contents\n`],
                    [path.join(bundle(name), 'Contents', 'Info.plist'), build({ CFBundleIdentifier: identifier, CFBundleShortVersionString: version })],
                ]),
            ),
        };
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
        const served = Automation.omit('nodes', 'launch', 'act', 'quit', 'definition', 'capture', 'preview', 'diagnostics');
        yield* Layer.build(
            RpcServer.layer(served).pipe(
                Layer.provide([
                    served.toLayer({
                        installations: () => Stream.concat(Stream.succeed(Result.succeed([])), Stream.never),
                        instances: () => Stream.concat(Stream.succeed(instances), Stream.never),
                        events: () => Stream.never,
                        pressure: () => Stream.never,
                        terms: ({ url }) => Effect.orDie(Effect.fromNullishOr(dictionaries[url.href])),
                        execute: ({ instance, arguments: given }) =>
                            Effect.as(
                                Effect.sync(() => MutableRef.update(executed, Array.append({ instance, arguments: Record.keys(given) }))),
                                { reply: REPLY, artifacts: [] },
                            ),
                    }),
                    RpcServer.layerProtocolSocketServer,
                ]),
                Layer.provide([sockets({ path: socket }), RpcSerialization.layerNdJsonRpc()]),
            ),
        );
        const server = yield* spawner.spawn(ChildProcess.make(path.join(contents, 'Helpers', 'server'), ['--listen', `127.0.0.1:${port}`, '--output', path.join(root, 'output')], { env: { HOME: path.join(root, 'home'), PATH: `${bin}:${searched}` }, extendEnv: true, stdout: 'ignore', stderr: 'inherit' }));
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

const ran = Effect.fnUntraced(function* (name: string) {
    const suite = yield* Suite;
    const {
        handle,
        result: { instance },
    } = yield* Effect.flatMap(suite.call('applications', { action: 'open', application: name }), ({ structuredContent }) => Schema.decodeUnknownEffect(Opened)(structuredContent));
    const result = yield* suite.call('execute_script', { code: 'return 7;', handle });
    const executed = yield* suite.executed;
    return {
        arguments: Option.map(
            Array.findFirst(executed, (each) => each.instance === instance),
            Struct.get('arguments'),
        ),
        refused: Option.liftPredicate(Array.map(Array.getSomes(Array.map(result.content, (block) => Schema.decodeUnknownOption(Text)(block))), Struct.get('text')), () => result.isError === true),
    };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(Layer.effect(Suite, Suite.make).pipe(Layer.provide([services, layerUndici])), { timeout: '1 minute', excludeTestServices: true })('script route', (it) => {
    it.effect.each([
        { named: 'runs a body through the do script of an application no UXP host lists, sending its direct parameter alone', name: 'Adobe Acrobat', expected: { arguments: Option.some(['direct']), refused: Option.none() } },
        { named: 'refuses an application whose dictionary declares commands but no script command', name: 'Acrobat Distiller', expected: { arguments: Option.none(), refused: Option.some(['Acrobat Distiller declares no script command and links no extension']) } },
    ])('$named', ({ name, expected }) => Effect.map(ran(name), (taken) => assert.deepStrictEqual(taken, expected)));
});
