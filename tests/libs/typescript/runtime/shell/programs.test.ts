import { assert, it } from '@effect/vitest';
import { Shell } from '@rasm/shell';
import { Array, Effect, FileSystem, Option, Order, Path, Result, Struct } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { processor } from 'sh-syntax/sync';
import { Dialects, Oracle } from '#/oracle';

// --- [TYPES] ---------------------------------------------------------------------------

type Read = Effect.Success<ReturnType<Shell['Service']['read']>>;
type Invocation = Effect.Success<Read['invocations']>[number];

// --- [OPERATIONS] ----------------------------------------------------------------------

const flat = (held: readonly Invocation[]): readonly Invocation[] => held.flatMap((invocation) => [invocation, ...flat(invocation.inner)]);
const shown = (fields: Invocation['writes']) => fields.map((field) => Option.getOrElse(field.text, () => `${field.prefix}*`));
const exported = (version: string, rm: string) => `case "$*" in --version) echo '${version}' ;; 'rm export') ${rm} ;; *) exec "$CARAPACE" "$@" ;; esac`;

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Dialects.layer, { excludeTestServices: true })('programs', (suite) => {
    suite.effect.each([
        {
            case: 'no carapace on PATH reads the carried rows',
            stand: Option.none<string>(),
            source: 'rm -R -W x; git commit -S x',
            answer: Result.succeed([
                [['--recursive', '-R', '-r'], ['-W'], ['--gpg-sign', '-S']],
                ['x', 'x'],
            ]),
            calls: [],
        },
        { case: 'a carapace at the carried release reads the carried rows', stand: Option.some('exec "$CARAPACE" "$@"'), source: 'rm -W x', answer: Result.succeed([[['-W']], ['x']]), calls: ['--version', '--list'] },
        {
            case: 'a carapace at another release reads its export',
            stand: Option.some(exported('carapace-bin 0.0.0', `echo '{"Name":"rm","LocalFlags":[{"Shorthand":"Z","Type":"bool"}]}'`)),
            source: 'rm -Z -r x',
            answer: Result.succeed([[['-Z'], ['--recursive', '-R', '-r']], ['x']]),
            calls: ['--version', '--list', 'rm export'],
        },
        {
            case: 'an export flag of a type outside pflag’s value-less ones takes a value',
            stand: Option.some(exported('carapace-bin 0.0.0', `echo '{"Name":"rm","LocalFlags":[{"Shorthand":"Y","Type":"boolfunc","NoOptDefVal":"true"},{"Shorthand":"Z","Type":"ipNet"}]}'`)),
            source: 'rm -Y -Z v x',
            answer: Result.succeed([[['-Y'], ['-Z']], ['x']]),
            calls: ['--version', '--list', 'rm export'],
        },
        { case: 'a failing export fails the walk naming it', stand: Option.some(exported('carapace-bin 0.0.0', 'echo refused >&2; exit 3')), source: 'rm x', answer: Result.fail('Host carapace rm export'), calls: ['--version', '--list', 'rm export'] },
    ])(
        'binds a program by its live carapace export when the installed carapace differs from the carried rows, else by the carried rows, under the signature rows: $case',
        Effect.fnUntraced(function* ({ stand, source, answer, calls }) {
            const [fs, path, spawner, oracle, { named }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, Oracle, Dialects]);
            const [folder, carapace] = yield* Effect.all([fs.makeTempDirectoryScoped(), oracle.located('carapace')]);
            yield* Effect.forEach(Option.toArray(stand), (body) => Effect.andThen(fs.writeFileString(path.join(folder, 'carapace'), `#!/bin/sh\nprintf '%s\\n' "$*" >> "\${0%/*}/calls"\n${body}\n`), fs.chmod(path.join(folder, 'carapace'), 0o755)));
            const placed = ChildProcessSpawner.make((command) => spawner.spawn(ChildProcess.isStandardCommand(command) ? ChildProcess.setEnv(command, { PATH: `${folder}:/usr/bin:/bin`, HOME: folder, CARAPACE: carapace }) : command));
            const shell = yield* Effect.provideService(Shell.make({ processor }), ChildProcessSpawner.ChildProcessSpawner, placed);
            const found = yield* Effect.result(Effect.flatMap(shell.read(source, { dialect: named.bash }), (script) => script.invocations));
            const ran = yield* Effect.orElseSucceed(fs.readFileString(path.join(folder, 'calls')), () => '');
            const invocations = Result.getOrElse(found, (): readonly Invocation[] => []);
            const observed = [invocations.flatMap(Struct.get('flags')).map(({ flag }) => Array.sort(flag.spellings, Order.String)), invocations.flatMap(Struct.get('operands')).flatMap(({ word }) => Option.toArray(word.field.text))];
            const bound = Result.map(found, () => observed);
            assert.deepStrictEqual<unknown>([Result.mapError(bound, ({ reason, subject }) => `${reason._tag} ${subject}`), ran.split('\n').filter((line) => line !== '')], [answer, calls]);
        }, Effect.scoped),
    );

    suite.effect.each([
        { source: 'rm -r a', reads: [], writes: [], deletes: ['/a', '/a/*'], inner: [] },
        { source: 'cp -R a b', reads: ['/a', '/a/*'], writes: ['/b'], deletes: [], inner: [] },
        { source: 'cp -t d a', reads: ['/a'], writes: ['/d'], deletes: [], inner: [] },
        { source: 'mv a b', reads: ['/a', '/a/*'], writes: ['/b'], deletes: ['/a', '/a/*'], inner: [] },
        { source: 'sed -i.bak -e s/x/y/ f', reads: ['/f'], writes: ['/f'], deletes: [], inner: [] },
        { source: 'sed s/x/y/ f > g', reads: ['/f'], writes: ['/g'], deletes: [], inner: [] },
        { source: 'tee f < g', reads: ['/g'], writes: ['/f'], deletes: [], inner: [] },
        { source: 'find d -name x -delete', reads: ['/d', '/d/*'], writes: [], deletes: ['/d', '/d/*'], inner: [] },
        { source: 'find d -exec rm {} +', reads: ['/d', '/d/*'], writes: [], deletes: [], inner: ['rm'] },
        { source: 'git rm --cached x', reads: ['/x'], writes: [], deletes: [], inner: [] },
        { source: 'git -C /r rm x', reads: [], writes: [], deletes: ['/x'], inner: [] },
        { source: 'env X=1 timeout 5 nohup rm a', reads: [], writes: [], deletes: [], inner: ['timeout', 'nohup', 'rm'] },
        { source: 'xargs -I{} rm {}', reads: [], writes: [], deletes: [], inner: ['rm'] },
        { source: 'nx run a --outputs o', reads: [], writes: ['/o'], deletes: [], inner: [] },
    ])(
        'states what $source reads, writes, deletes, and runs as its manual describes it, and what a caller row adds',
        Effect.fnUntraced(function* ({ source, reads, writes, deletes, inner }) {
            const { named } = yield* Dialects;
            const shell = yield* Shell.make({ processor, programs: [{ name: 'nx', flags: [['one', '--outputs']], values: { '--outputs': { roles: ['write'] } } }] });
            const [first, ...rest] = yield* Effect.flatMap(shell.read(source, { dialect: named.bash, cwd: '/' }), (script) => script.invocations);
            const ran = flat([...(first?.inner ?? []), ...rest]).flatMap(({ program }) => (program._tag === 'Unknown' ? [] : [program.name]));
            assert.deepStrictEqual<unknown>({ reads: shown(first?.reads ?? []), writes: shown(first?.writes ?? []), deletes: shown(first?.deletes ?? []), inner: ran }, { reads, writes, deletes, inner });
        }),
    );
});
