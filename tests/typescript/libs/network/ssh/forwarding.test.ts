import { assert, it } from '@effect/vitest';
import { Forward, Ssh } from '@rasm/ssh';
import { Array, Effect, FileSystem, Path, Record, Result, Schema } from 'effect';
import { answered, bounds, greeted, greeting, type Listeners, reasoned, Sshd, served, vacant } from '#/sshd';

// --- [CONSTANTS] -----------------------------------------------------------------------

const opened = `127.0.0.1:${greeting.port}`;
const others = ['127.0.0.1:22', '127.0.0.2:22'] as const;
const listened = 3300;
const policies = {
    yes: { permitOpen: ['YY', 'NY', 'YY'], permitListen: ['YY', 'YN'] },
    local: { permitOpen: ['YN', 'NN', 'YN'], permitListen: ['YN', 'YN'] },
    remote: { permitOpen: ['NY', 'NY', 'NY'], permitListen: ['NY', 'NN'] },
    no: { permitOpen: ['NN', 'NN', 'NN'], permitListen: ['NN', 'NN'] },
} as const;
const outcomes = { Y: { local: true, remote: 'forwarded' }, N: { local: false, remote: 'Refused' } } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Chain = Schema.NonEmptyArray(Forward);

// --- [OPERATIONS] ----------------------------------------------------------------------

const controls = Record.fromEntries(
    Array.flatMap(Record.toEntries(policies), ([policy, { permitOpen, permitListen }], row) => {
        const kinds = {
            plain: { expected: permitOpen[0] },
            'permit-open-other': { options: [`PermitOpen=${others.join(' ')}`], expected: permitOpen[1] },
            'permit-open': { options: [`PermitOpen=${[others[0], opened, others[1]].join(' ')}`], expected: permitOpen[0] },
            'key-open-other': { keyOptions: Array.map(others, (other) => `permitopen="${other}"`), expected: permitOpen[1] },
            'key-open': { keyOptions: [`permitopen="${others[0]}"`, `permitopen="${opened}"`], expected: permitOpen[2] },
            'key-no-port-forwarding': { keyOptions: ['no-port-forwarding'], expected: 'NN' },
            'key-restrict': { keyOptions: ['restrict'], expected: 'NN' },
            'key-restrict-port-forwarding': { keyOptions: ['restrict', 'port-forwarding'], expected: permitOpen[0] },
            'permit-listen-other': { options: (port: number) => [`PermitListen=127.0.0.1:22 127.0.0.2:${port}`], expected: permitListen[1] },
            'permit-listen': { options: (port: number) => [`PermitListen=127.0.0.1:22 127.0.0.1:${port} 127.0.0.2:${port}`], expected: permitListen[0] },
        } as const;
        return Array.map(Record.toEntries<string, { readonly options?: readonly string[] | ((port: number) => readonly string[]); readonly keyOptions?: readonly string[]; readonly expected: string }>(kinds), ([kind, { options = [], keyOptions = [], expected }], column) => {
            const port = listened + row * Record.size(kinds) + column;
            return [`${policy}-${kind}`, { port, expected, keyOptions, options: [`AllowTcpForwarding=${policy}`, ...(typeof options === 'function' ? options(port) : options)] }] as const;
        });
    }),
);
const listeners: Listeners = Record.map(controls, ({ options, keyOptions }) => ({ options, keyOptions }));
const controlled = Effect.fnUntraced(function* (name: string, port: number, bind: string) {
    const [ssh, { hosted, far, trusted }] = yield* Effect.all([Ssh, Sshd]);
    const [main, host] = yield* Effect.all([hosted('main'), hosted(name, far.dials[name])]);
    const forwards = yield* Effect.all({
        local: Schema.decodeEffect(Forward)({ _tag: 'Local', bind, connect: opened }),
        remote: Schema.decodeEffect(Forward)({ _tag: 'Remote', bind: `127.0.0.1:${port}`, connect: `${far.dial.host}:${far.dial.port}` }),
    });
    const local = yield* Effect.andThen(ssh.forward(host, forwards.local), answered({ path: bind }, greeted)).pipe(Effect.scoped, Effect.result);
    const remote = yield* ssh.forward(host, forwards.remote).pipe(
        Effect.andThen(ssh.run(main, ['ssh-keyscan', '-T', '5', '-t', 'ed25519', '-p', `${port}`, '127.0.0.1'])),
        Effect.map((scanned) => (scanned.includes(trusted) ? outcomes.Y.remote : scanned)),
        Effect.scoped,
        Effect.result,
    );
    return [name, { local: Result.getOrElse(local, reasoned), remote: Result.getOrElse(remote, reasoned) }] as const;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Sshd.layer(listeners), { timeout: bounds.started, excludeTestServices: true })('forwarding', (methods) => {
    methods.effect(
        'opens local forwards and binds remote forwards as each server forwarding policy, open permission, listen permission, and key option allows',
        Effect.fnUntraced(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const folder = yield* fs.makeTempDirectoryScoped();
            const found = yield* Effect.forEach(Record.toEntries(controls), ([name, { port }], index) => controlled(name, port, path.join(folder, `${index}.fwd`)), { concurrency: 8 });
            assert.deepStrictEqual(
                Record.fromEntries(found),
                Record.map(controls, ({ expected }) => ({ local: outcomes[expected.startsWith('Y') ? 'Y' : 'N'].local, remote: outcomes[expected.endsWith('Y') ? 'Y' : 'N'].remote })),
            );
        }, Effect.scoped),
        bounds.test,
    );

    methods.effect.each(['Local', 'Remote'] as const)(
        'refuses a %s forward that binds an address a live forward holds',
        Effect.fnUntraced(function* (direction) {
            const [ssh, { hosted, far }] = yield* Effect.all([Ssh, Sshd]);
            const host = yield* hosted(`duplicate-${direction.toLowerCase()}`);
            const sides = { Local: { bind: `127.0.0.1:${yield* vacant}`, connect: [`127.0.0.1:${served}`, `localhost:${served}`] }, Remote: { bind: `127.0.0.1:${listened - 1}`, connect: [`${far.dial.host}:${far.dial.port}`, `localhost:${far.dial.port}`] } } as const;
            const { bind, connect } = sides[direction];
            const [held, repeated] = yield* Effect.all([Schema.decodeEffect(Forward)({ _tag: direction, bind, connect: connect[0] }), Schema.decodeEffect(Forward)({ _tag: direction, bind, connect: connect[1] })]);
            const refused = yield* Effect.andThen(ssh.forward(host, held), Effect.flip(ssh.forward(host, repeated)));
            assert.strictEqual(reasoned(refused), 'Refused');
        }, Effect.scoped),
        bounds.test,
    );

    methods.effect(
        'transfers over a chain of socket path forwards between a remote port and the server',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, trusted }, fs, path] = yield* Effect.all([Ssh, Sshd, FileSystem.FileSystem, Path.Path]);
            const [host, folder] = yield* Effect.all([hosted('chained'), fs.makeTempDirectoryScoped()]);
            const [first, second, third] = [path.join(folder, 'unix-1.fwd'), '/tmp/unix-2.fwd', path.join(folder, 'unix-3.fwd')];
            const links = yield* Schema.decodeEffect(Chain)([
                { _tag: 'Remote', bind: '127.0.0.1:0', connect: first },
                { _tag: 'Local', bind: first, connect: second },
                { _tag: 'Remote', bind: second, connect: third },
                { _tag: 'Local', bind: third, connect: `127.0.0.1:${served}` },
            ]);
            const chain = yield* Effect.forEach(links, (wanted) => ssh.forward(host, wanted));
            const { bind } = yield* Schema.encodeEffect(Forward)(Array.headNonEmpty(chain));
            const { port } = yield* Schema.decodeEffect(Schema.HostPortFromString)(bind);
            assert.include(yield* ssh.run(host, ['ssh-keyscan', '-T', '5', '-t', 'ed25519', '-p', `${port}`, '127.0.0.1']), trusted);
        }, Effect.scoped),
        bounds.test,
    );
});
