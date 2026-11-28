import { layer as platform } from '@effect/platform-node/NodeServices';
import { Shell } from '@rasm/shell';
import { Array, Cause, Config, Context, Effect, FileSystem, Layer, Option, Path, Record, Redacted, Schema } from 'effect';
import { ChildProcess, ChildProcessSpawner, type CommandLine } from 'effect/process';
import { processor } from 'sh-syntax/sync';

// --- [CONSTANTS] -----------------------------------------------------------------------

const recorder = `#!/bin/sh\nt=$(/usr/bin/mktemp) || exit 1\n{ printf '%s\\0' "$#" "$PWD" "\${0##*/}" "$@"; /bin/cat; printf '\\0'; } > "$t"\n/bin/cat "$t" >> "$RECORD"\n/bin/rm -f "$t"\n`;
const remote = '#!/bin/sh\nshift\nexec /bin/sh -c "$*"\n';
const fixture = ['a.txt', 'b.txt', '.dot.txt', 'dir/c.txt', 'space name.txt', 'ab'] as const;
const charsets = { ascii: { LC_ALL: 'C' }, utf8: { LC_ALL: 'C.UTF-8' } } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Count = Schema.NumberFromString.check(Schema.isInt(), Schema.isGreaterThanOrEqualTo(0));

// --- [OPERATIONS] ----------------------------------------------------------------------

const records = (fields: readonly string[]): readonly { readonly cwd: string; readonly program: string; readonly args: readonly string[]; readonly input: string }[] =>
    Option.match(
        Option.flatMap(
            Option.filter(Array.head(fields), (head) => head !== ''),
            Schema.decodeUnknownOption(Count),
        ),
        {
            onNone: () => [],
            onSome: (count) => [{ cwd: fields[1] ?? '', program: fields[2] ?? '', args: fields.slice(3, 3 + count), input: fields[3 + count] ?? '' }, ...records(fields.slice(4 + count))],
        },
    );

// --- [SERVICES] ------------------------------------------------------------------------

class Oracle extends Context.Service<Oracle>()('@rasm/test-shell/oracle', {
    make: Effect.gen(function* () {
        const [fs, path, spawner, searched] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, Config.String('PATH')]);
        const folder = yield* fs.makeTempDirectoryScoped();
        const bin = path.join(folder, 'bin');
        const files = path.join(folder, 'fixture');
        yield* fs.makeDirectory(bin);
        yield* fs.makeDirectory(path.join(files, 'dir'), { recursive: true });
        yield* Effect.forEach(fixture, (name) => fs.writeFileString(path.join(files, name), ''), { concurrency: 'unbounded' });
        yield* Effect.forEach(
            [
                ['rec', recorder],
                ['ssh', remote],
            ] as const,
            ([name, body]) => Effect.andThen(fs.writeFileString(path.join(bin, name), body), fs.chmod(path.join(bin, name), 0o755)),
            { concurrency: 'unbounded' },
        );
        const zshenv = Effect.fnUntraced(function* (lines: string) {
            const home = yield* fs.makeTempDirectoryScoped();
            yield* fs.writeFileString(path.join(home, '.zshenv'), `${lines}\nPATH=${bin}\n`);
            return home;
        });
        const written = Effect.fnUntraced(function* (lines: string) {
            const file = path.join(yield* fs.makeTempDirectoryScoped(), 'startup');
            yield* fs.writeFileString(file, lines);
            return file;
        });
        const environment = { PATH: bin, ZDOTDIR: yield* zshenv(''), BASH_ENV: yield* written('') };
        const located = (program: string) =>
            Effect.findFirst(path.isAbsolute(program) ? [program] : searched.split(':').map((directory) => path.join(directory, program)), (candidate) => fs.exists(candidate)).pipe(
                Effect.flatMap(Effect.fromOption),
                Effect.mapError((cause) => new Cause.UnknownError(cause, `${program} is on no PATH directory`)),
            );
        const run = Effect.fnUntraced(function* (argv: Array.NonEmptyReadonlyArray<CommandLine.Word>, given: Readonly<Record<string, string>> = {}, cwd?: string) {
            const scratch = yield* fs.makeTempDirectoryScoped();
            const record = path.join(scratch, 'record');
            const head = Array.headNonEmpty(argv);
            const exit = yield* spawner.exitCode(ChildProcess.make(Redacted.isRedacted(head) ? Redacted.value(head) : head, Array.tailNonEmpty(argv), { cwd: cwd ?? scratch, env: { ...environment, HOME: scratch, RECORD: record, ...given }, extendEnv: false, stdin: 'ignore', stdout: 'ignore', stderr: 'ignore' }));
            const output = yield* Effect.flatMap(fs.exists(record), (present) => (present ? fs.readFileString(record) : Effect.succeed('')));
            return { exit, records: records(output.split('\0')) };
        }, Effect.scoped);
        const shell = (program: string, source: string, given?: Readonly<Record<string, string>>) => Effect.flatMap(located(program), (binary) => run([binary, '-c', source], given));
        return { environment, fixture: files, located, run, shell, written, zshenv };
    }),
}) {
    static readonly layer = Layer.provideMerge(Layer.effect(this, this.make), platform);
}

class Dialects extends Context.Service<Dialects>()('@rasm/test-shell/oracle/Dialects', {
    make: Effect.gen(function* () {
        const [shell, oracle] = yield* Effect.all([Shell, Oracle]);
        const probed = (program: string, given: Readonly<Record<string, string>> = {}, source?: string) => Effect.flatMap(oracle.located(program), (binary) => shell.dialect({ shell: binary, environment: { ...oracle.environment, ...given }, timeout: '2 minutes', ...(source === undefined ? {} : { source }) }));
        const named = (given: Readonly<Record<string, string>>) => Effect.all({ sh: probed('sh', given), '/bin/sh': probed('/bin/sh', given), bash: probed('bash', given), zsh: probed('zsh', given), dash: probed('dash', given) }, { concurrency: 'unbounded' });
        const [inherited, localized] = yield* Effect.all([named({}), Effect.all(Record.map(charsets, named), { concurrency: 'unbounded' })], { concurrency: 'unbounded' });
        return { probed, named: inherited, localized };
    }),
}) {
    static readonly layer = Layer.provideMerge(Layer.effect(this, this.make), Layer.provideMerge(Layer.effect(Shell, Shell.make({ processor })), Oracle.layer));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { charsets, Dialects, Oracle };
