import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { Ssh } from '@rasm/ssh/client';
import { Forward } from '@rasm/ssh/forward';
import { ControlPath, Host } from '@rasm/ssh/host';
import { Array, Cause, Effect, FileSystem, Option, Path, Predicate, Record, Result, Schema, Stream, Struct } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const key = 'ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIOMqqnkVzrm0SdG6UOoqKLsabgH5C9okWi0dh2l9GKJl';
const limit = { arbitrary: { concurrency: 8 } };
const stated = { name: 'somehost', host: '192.0.2.7', port: 2222, user: 'remuser', identity: { _tag: 'Agent', socket: '/run/agent.sock' }, trust: { _tag: 'HostKeys', keys: [key] }, route: { _tag: 'Direct', preferred: [] }, persist: 90_000 } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Expanded = Schema.Literals(['%', '$', '"', "'", '\\', '\n', '\r', '\0']);

// --- [OPERATIONS] ----------------------------------------------------------------------

const resolved = Effect.fnUntraced(function* (words: readonly string[]) {
    const handle = yield* ChildProcess.make('ssh', ['-G', ...words], { stdin: 'ignore' });
    const [lines, code] = yield* Effect.all([handle.stdout.pipe(Stream.decodeText, Stream.splitLines, Stream.runCollect), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0
        ? Record.map(
              Array.groupBy(lines, (line) => line.slice(0, line.indexOf(' '))),
              Array.map((line) => line.slice(line.indexOf(' ') + 1)),
          )
        : yield* new Cause.UnknownError(code, `ssh -G ${words.join(' ')} exited ${code}`);
}, Effect.scoped);
const written = Effect.fnUntraced(function* (fields: Readonly<Record<string, unknown>>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = yield* fs.makeTempDirectoryScoped();
    const [ssh, host] = yield* Effect.all([Ssh.make(folder), Schema.decodeEffect(Host)({ ...stated, ...fields })]);
    yield* ssh.place(host);
    return { folder, host, config: path.join(folder, host.name, 'config') };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('config', (methods) => {
    methods.effect(
        'writes a config the OpenSSH client resolves to the dial, user, key, certificate, shared connection, and timings of its host',
        Effect.fnUntraced(function* () {
            const { folder, host, config } = yield* written({ identity: { _tag: 'Key', file: '/keys/id', certificate: '/keys/id-cert.pub' }, timeout: 7000, alive: { interval: 4000, count: 5 } });
            const [shared, own] = yield* Effect.all([resolved(['-F', config, host.name]), resolved(['-F', config, '-S', 'none', host.name])]);
            assert.deepStrictEqual(Struct.pick(shared, ['hostname', 'port', 'user', 'hostkeyalias', 'identityfile', 'certificatefile', 'controlmaster', 'controlpath', 'controlpersist', 'connecttimeout', 'serveraliveinterval', 'serveralivecountmax', 'stricthostkeychecking', 'batchmode']), {
                hostname: ['192.0.2.7'],
                port: ['2222'],
                user: ['remuser'],
                hostkeyalias: ['somehost'],
                identityfile: ['/keys/id'],
                certificatefile: ['/keys/id-cert.pub'],
                controlmaster: ['auto'],
                controlpath: [`${folder}/somehost/control`],
                controlpersist: ['90'],
                connecttimeout: ['7'],
                serveraliveinterval: ['4'],
                serveralivecountmax: ['5'],
                stricthostkeychecking: ['true'],
                batchmode: ['yes'],
            });
            assert.deepStrictEqual([Struct.pick(own, ['hostname', 'port', 'user', 'hostkeyalias', 'identityfile']), Record.has(own, 'controlpath')], [{ hostname: ['192.0.2.7'], port: ['2222'], user: ['remuser'], hostkeyalias: ['somehost'], identityfile: ['/keys/id'] }, false]);
        }),
    );

    methods.effect(
        'writes the agent socket of an agent identity, and leaves the timings a host omits at the OpenSSH defaults',
        Effect.fnUntraced(function* () {
            const { host, config } = yield* written({});
            assert.deepStrictEqual(Struct.pick(yield* resolved(['-F', config, host.name]), ['identityagent', 'connecttimeout', 'serveraliveinterval', 'serveralivecountmax']), {
                identityagent: ['/run/agent.sock'],
                connecttimeout: ['none'],
                serveraliveinterval: ['0'],
                serveralivecountmax: ['3'],
            });
        }),
    );

    methods.effect(
        'routes a host through its hops in order, each hop resolving to its own dial and user',
        Effect.fnUntraced(function* () {
            const hops = [
                { name: 'first.hop', host: '198.51.100.4', port: 2201, user: 'hopper', identity: { _tag: 'Key', file: '/keys/first' }, trust: { _tag: 'HostKeys', keys: [key] } },
                { name: 'second.hop', host: '203.0.113.9', user: 'skipper', identity: { _tag: 'Agent', socket: '/run/second.sock' }, trust: { _tag: 'HostKeys', keys: [key] } },
            ];
            const { host, config } = yield* written({ route: { _tag: 'Jump', hops } });
            const [routed, first, second] = yield* Effect.all([resolved(['-F', config, host.name]), resolved(['-F', config, 'first.hop']), resolved(['-F', config, 'second.hop'])]);
            assert.deepStrictEqual(
                [Struct.pick(routed, ['proxyjump']), Struct.pick(first, ['hostname', 'port', 'user', 'identityfile']), Struct.pick(second, ['hostname', 'port', 'user', 'identityagent'])],
                [{ proxyjump: ['first.hop,second.hop'] }, { hostname: ['198.51.100.4'], port: ['2201'], user: ['hopper'], identityfile: ['/keys/first'] }, { hostname: ['203.0.113.9'], port: ['22'], user: ['skipper'], identityagent: ['/run/second.sock'] }],
            );
        }),
    );

    methods.effect(
        'writes its files and the public key its agent identity offers, readable by their owner alone',
        Effect.fnUntraced(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const { folder, config } = yield* written({ identity: { _tag: 'Agent', socket: '/run/agent.sock', key } });
            const [stats, texts, read] = yield* Effect.all([
                Effect.forEach(['somehost', 'somehost/config', 'somehost/known_hosts', 'somehost/agent.pub'], (file) => fs.stat(path.join(folder, file))),
                Effect.forEach(['somehost/agent.pub', 'somehost/known_hosts'], (file) => fs.readFileString(path.join(folder, file))),
                resolved(['-F', config, 'somehost']),
            ]);
            assert.deepStrictEqual([stats.map(({ mode }) => (mode & 0o777).toString(8)), texts, Struct.pick(read, ['identityfile', 'identitiesonly'])], [['700', '600', '600', '600'], [`${key}\n`, `somehost ${key}\n`], { identityfile: [path.join(folder, 'somehost', 'agent.pub')], identitiesonly: ['yes'] }]);
        }),
    );

    methods.effect.prop(
        'refuses a user name, an identity file, an agent socket, and a folder holding a character the OpenSSH client or its shell expands',
        { before: Schema.String, expanded: Expanded, after: Schema.String },
        Effect.fnUntraced(function* ({ before, expanded, after }) {
            const text = `${before}${expanded}${after}`;
            const refused = yield* Effect.all({
                user: Effect.isFailure(Schema.decodeEffect(Host)({ ...stated, user: text })),
                file: Effect.isFailure(Schema.decodeEffect(Host)({ ...stated, identity: { _tag: 'Key', file: text } })),
                socket: Effect.isFailure(Schema.decodeEffect(Host)({ ...stated, identity: { _tag: 'Agent', socket: text } })),
                folder: Effect.isFailure(Ssh.make(text)),
            });
            assert.deepStrictEqual(refused, { user: true, file: true, socket: true, folder: true });
        }),
        limit,
    );

    methods.for(['#remuser', '=remuser', '~remuser', 'rem\uD800user'])('refuses %j, which the OpenSSH client reads as a comment, a separator, or a home folder, or no UTF-8 file can hold', (text) => {
        assert.deepStrictEqual([Schema.decodeResult(Host)({ ...stated, user: text }), Schema.decodeResult(Host)({ ...stated, identity: { _tag: 'Agent', socket: text } })].map(Result.isFailure), [true, true]);
    });

    methods.effect.each(['two words', ' padded ', '\tindented', 'rem#user', 'rem=user', 'rem~user', 'rem user #1', 'ユーザー'])(
        'writes %j so the OpenSSH client reads it back unchanged as a user name and an agent socket',
        Effect.fnUntraced(function* (text) {
            const { host, config } = yield* written({ user: text, identity: { _tag: 'Agent', socket: text } });
            assert.deepStrictEqual(Struct.pick(yield* resolved(['-F', config, host.name]), ['user', 'identityagent']), { user: [text], identityagent: [text] });
        }),
    );

    methods.effect.prop(
        'writes every user name, identity file, certificate file, and folder it accepts so the OpenSSH client reads it back unchanged, and refuses a folder whose control socket an OpenSSH master cannot bind',
        { user: Host.fields.user, file: Host.fields.user, certificate: Host.fields.user, inside: Host.fields.user },
        Effect.fnUntraced(function* ({ user, file, certificate, inside }) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const folder = path.join(yield* fs.makeTempDirectoryScoped(), 'in', inside);
            const [ssh, host] = yield* Effect.all([Ssh.make(folder), Schema.decodeEffect(Host)({ ...stated, user, identity: { _tag: 'Key', file, certificate } })]);
            const control = path.join(folder, 'somehost', 'control');
            const placed = yield* Effect.option(ssh.place(host));
            const read = yield* Effect.transposeOption(Option.map(placed, () => resolved(['-F', path.join(folder, 'somehost', 'config'), 'somehost'])));
            assert.deepStrictEqual(
                Option.map(read, Struct.pick(['user', 'identityfile', 'certificatefile', 'controlpath', 'userknownhostsfile'])),
                Option.liftPredicate({ user: [user], identityfile: [file], certificatefile: [certificate], controlpath: [control], userknownhostsfile: [path.join(folder, 'somehost', 'known_hosts')] }, () => Schema.is(ControlPath)(control)),
            );
        }),
        limit,
    );

    methods.prop(
        'accepts a duration in every timing field exactly when it is a whole number of seconds above zero, the unit OpenSSH reads',
        { seconds: Schema.Int.check(Schema.isBetween({ minimum: -2, maximum: 9 })), offset: Schema.Union([Schema.Literal(0), Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 999 }))]) },
        ({ seconds, offset }) => {
            const millis = seconds * 1000 + offset;
            const fields = { persist: { persist: millis }, timeout: { timeout: millis }, interval: { alive: { interval: millis, count: 1 } }, within: { route: { _tag: 'Direct', preferred: [{ host: '192.0.2.8', within: millis }] } } } as const;
            const accepted = millis > 0 && millis % 1000 === 0;
            assert.deepStrictEqual(
                Record.map(fields, (field) => Result.isSuccess(Schema.decodeResult(Host)({ ...stated, ...field }))),
                Record.map(fields, () => accepted),
                `${millis} ms`,
            );
        },
    );

    methods.effect.each([
        { flag: '-R', ends: ['127.0.0.1:0', '127.0.0.1:22'], accepted: true },
        { flag: '-L', ends: ['127.0.0.1:0', '127.0.0.1:22'], accepted: false },
        { flag: '-L', ends: ['127.0.0.1:1', '127.0.0.1:22'], accepted: true },
        { flag: '-L', ends: ['127.0.0.1:4022', '127.0.0.1:0'], accepted: false },
        { flag: '-R', ends: ['127.0.0.1:4022', '127.0.0.1:0'], accepted: false },
        { flag: '-R', ends: ['/run/bound.sock', '127.0.0.1:0'], accepted: false },
        { flag: '-D', ends: ['127.0.0.1:0'], accepted: false },
        { flag: '-D', ends: ['127.0.0.1:1'], accepted: true },
    ] as const)(
        'decides the forward %j as the OpenSSH client does, which takes port 0 for a remote bind alone',
        Effect.fnUntraced(function* ({ flag, ends: [bind, connect], accepted }) {
            const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
            const taken = yield* spawner.exitCode(ChildProcess.make('ssh', ['-F', 'none', '-G', flag, [bind, connect].filter(Predicate.isNotUndefined).join(':'), 'somehost'], { stdin: 'ignore' }));
            const decoded = Schema.decodeUnknownResult(Forward)({ _tag: { '-L': 'Local', '-R': 'Remote', '-D': 'Dynamic' }[flag], bind, connect });
            assert.deepStrictEqual([taken === 0, Result.isSuccess(decoded)], [accepted, accepted]);
        }),
    );
});
