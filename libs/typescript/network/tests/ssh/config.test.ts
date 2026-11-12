import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { Host, Ssh } from '@rasm/ssh';
import { Array, Effect, FileSystem, Path, pipe, Record, Result, Schema, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const key = 'ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIOMqqnkVzrm0SdG6UOoqKLsabgH5C9okWi0dh2l9GKJl';
const stated = { name: 'somehost', host: '192.0.2.7', port: 2222, user: 'remuser', trust: { _tag: 'HostKeys', keys: [key] }, path: { _tag: 'Direct', preferred: [] }, persist: 30_000, timeout: 5000, alive: { interval: 1000, count: 2 } } as const;
const exported = { FOO: 'bar' };
const tokens = ['%h', '%%', '%C', '%n-%p-%r', ['$', '{FOO}'].join(''), '~'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const named = (options: Record.ReadonlyRecord<string, readonly string[]>, name: string) => options[name] ?? [];
const resolved = Effect.fnUntraced(function* (folder: string, host: Host) {
    const [fs, path, spawner, ssh] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, Ssh.make(folder)]);
    yield* ssh.written(host);
    const words = host.client(path, folder, '-G');
    const shown = yield* pipe(spawner.streamString(ChildProcess.make(Array.headNonEmpty(words), Array.tailNonEmpty(words), { stdin: 'ignore', env: exported, extendEnv: true })), Stream.mkString);
    const options = Record.map(
        Array.groupBy(shown.trim().split('\n'), (line) => line.slice(0, line.indexOf(' '))),
        Array.map((line) => line.slice(line.indexOf(' ') + 1)),
    );
    const known = yield* Effect.forEach(
        named(options, 'userknownhostsfile').flatMap((files) => files.split(' ')),
        (file) => Effect.orElseSucceed(fs.readFileString(file), () => file),
    );
    return { options, known, control: host.control(path, folder) };
}, Effect.scoped);
const listed = (options: Record.ReadonlyRecord<string, readonly string[]>, ...names: readonly string[]) => Record.fromIterableWith(names, (name) => [name, named(options, name)]);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('config', (methods) => {
    methods.effect(
        'writes a config the OpenSSH client resolves to the fields of its host',
        Effect.fnUntraced(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const folder = yield* fs.makeTempDirectoryScoped();
            const [identity, certificate, socket] = [path.join(folder, 'id'), path.join(folder, 'id-cert.pub'), path.join(folder, 'agent')];
            const keyed = yield* Effect.flatMap(Schema.decodeEffect(Host)({ ...stated, identity: { _tag: 'Key', file: identity, certificate } }), (host) => resolved(folder, host));
            const agent = yield* Effect.flatMap(Schema.decodeEffect(Host)({ ...stated, identity: { _tag: 'Agent', socket } }), (host) => resolved(folder, host));
            assert.deepStrictEqual(listed(keyed.options, 'hostname', 'port', 'user', 'identityfile', 'certificatefile', 'controlpath', 'controlpersist', 'connecttimeout', 'serveraliveinterval', 'serveralivecountmax', 'stricthostkeychecking'), {
                hostname: [stated.host],
                port: [`${stated.port}`],
                user: [stated.user],
                identityfile: [identity],
                certificatefile: [certificate],
                controlpath: [keyed.control],
                controlpersist: ['30'],
                connecttimeout: ['5'],
                serveraliveinterval: ['1'],
                serveralivecountmax: ['2'],
                stricthostkeychecking: ['true'],
            });
            assert.deepStrictEqual(listed(agent.options, 'identityagent'), { identityagent: [socket] });
            assert.isTrue(keyed.known.some((content) => content.includes(key)));
        }, Effect.scoped),
    );

    methods.effect.each(tokens)(
        'keeps %s in a user name and an agent socket as written, or refuses the host',
        Effect.fnUntraced(function* (token) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const folder = yield* fs.makeTempDirectoryScoped();
            const [user, socket] = [`user-${token}`, path.join(folder, `agent-${token}`)];
            const decoded = yield* Effect.result(Schema.decodeEffect(Host)({ ...stated, user, identity: { _tag: 'Agent', socket } }));
            const shown = yield* Result.match(decoded, { onFailure: () => Effect.succeed({ identityagent: [socket], user: [user] }), onSuccess: (host) => Effect.map(resolved(folder, host), ({ options }) => listed(options, 'identityagent', 'user')) });
            assert.deepStrictEqual(shown, { identityagent: [socket], user: [user] });
        }, Effect.scoped),
    );

    methods.effect.each(tokens)(
        'keeps %s in the folder of its control socket as written',
        Effect.fnUntraced(function* (token) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const [kept, folder] = [yield* fs.makeTempDirectoryScoped(), path.join(yield* fs.makeTempDirectoryScoped(), `in-${token}`)];
            const host = yield* Schema.decodeEffect(Host)({ ...stated, identity: { _tag: 'Key', file: path.join(kept, 'id') } });
            const shown = yield* Effect.match(resolved(folder, host), { onFailure: () => [folder], onSuccess: ({ options }) => Array.map(named(options, 'controlpath'), (control) => control.slice(0, folder.length)) });
            assert.deepStrictEqual(shown, [folder]);
        }, Effect.scoped),
    );
});
