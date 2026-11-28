import { assert, it } from '@effect/vitest';
import { Shell } from '@rasm/shell';
import { Array, Cause, Effect, Option, Path, Result, Schema, Struct } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { charsets, Dialects, Oracle } from '#/oracle';

// --- [MODELS] --------------------------------------------------------------------------

const Hosted = Schema.fromJsonString(Schema.Struct({ reached: Schema.Array(Schema.String), left: Schema.Array(Schema.String), syntax: Schema.Unknown }));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Dialects.layer, { excludeTestServices: true })('syntax', (suite) => {
    suite.effect(
        'reads a source as a dialect refusal exactly when the binary its dialect names refuses to run it',
        Effect.fnUntraced(function* () {
            const [shell, oracle, { named }] = yield* Effect.all([Shell, Oracle, Dialects]);
            const rows = [
                ['dash', '[[ -n x ]]'],
                ['/bin/sh', '[[ -n x ]]'],
                ['bash', `: \${(f)x}`],
                ['zsh', `: \${(f)x}`],
            ] as const;
            const [read, ran] = yield* Effect.all([Effect.forEach(rows, ([binary, source]) => Effect.result(shell.read(source, { dialect: named[binary] }))), Effect.forEach(rows, ([binary, source]) => oracle.shell(binary, source), { concurrency: 'unbounded' })]);
            assert.deepStrictEqual(
                Array.zip(
                    rows,
                    read.map((result) => (Result.isSuccess(result) ? 'read' : result.failure.reason._tag)),
                ),
                Array.zip(
                    rows,
                    ran.map(({ exit }) => (exit === 0 ? 'read' : 'Dialect')),
                ),
            );
        }),
    );

    suite.effect.each(Struct.keys(charsets))(
        'reads a source bash refuses as a syntax error at the UTF-16 offset of its unclosed token, incomplete exactly when bash runs it once completed, under the %s charset',
        Effect.fnUntraced(function* (charset) {
            const [shell, oracle, { localized }] = yield* Effect.all([Shell, Oracle, Dialects]);
            const rows = [
                { source: 'echo "𝄞" "open', completion: '"', reason: { _tag: 'Syntax', offset: Option.some(10), incomplete: true } },
                { source: 'echo )', completion: '', reason: { _tag: 'Syntax', offset: Option.some(5), incomplete: false } },
            ] as const;
            const observed = yield* Effect.forEach(
                rows,
                ({ source, completion }) => Effect.all([Effect.map(Effect.flip(shell.read(source, { dialect: localized[charset].bash })), Struct.get('reason')), oracle.shell('bash', source, charsets[charset]), oracle.shell('bash', `${source}${completion}`, charsets[charset])]),
                { concurrency: 'unbounded' },
            );
            assert.deepStrictEqual(
                observed.map(([reason, refused, completed]) => [reason, refused.exit === 0, completed.exit === 0]),
                rows.map(({ reason }) => [reason, false, reason.incomplete]),
            );
        }),
    );

    suite.effect(
        'probes the options and the command, global, and suffix aliases a shell sets at startup or in a file sourced first, whatever the startup prints, as each shell running them shows',
        Effect.fnUntraced(function* () {
            const [oracle, { probed }] = yield* Effect.all([Oracle, Dialects]);
            const home = yield* oracle.zshenv(String.raw`print -r -- '"'; printf '\0rasm'; setopt nonomatch; alias ll='rec -l' quoted='it'\''s' tabbed=$'rec \tt'; alias -g G=mid; alias -s txt=rec`);
            const [bashenv, sourced] = yield* Effect.all([oracle.written('echo noise; shopt -s expand_aliases nullglob; alias q=\'rec "b c" d\' p=plain'), oracle.written('alias s=\'rec "s t"\'')]);
            const [zsh, bash, dash] = yield* Effect.all([probed('zsh', { ZDOTDIR: home }), probed('bash', { BASH_ENV: bashenv }), probed('dash', {}, sourced)], { concurrency: 'unbounded' });
            const binary = yield* oracle.located('dash');
            const ran = yield* Effect.all([oracle.shell('zsh', 'll\nrec a G\nx.txt\ntabbed\nrec zz*', { ZDOTDIR: home }), oracle.shell('bash', 'q\nrec zz*', { BASH_ENV: bashenv }), oracle.run([binary, '-c', '. "$1"\ns', binary, sourced])], { concurrency: 'unbounded' });
            assert.deepStrictEqual(
                [zsh.options.nomatch, bash.options.nullglob, zsh.aliases.get('ll'), zsh.aliases.get('quoted'), zsh.aliases.get('tabbed'), zsh.aliases.get('G'), zsh.suffixes.get('txt'), bash.aliases.get('q'), bash.aliases.get('p'), dash.aliases.get('s'), ran.map(({ records }) => records.map(Struct.get('args')))],
                [
                    false,
                    true,
                    { text: 'rec -l', global: false },
                    { text: "it's", global: false },
                    { text: 'rec \tt', global: false },
                    { text: 'mid', global: true },
                    'rec',
                    { text: 'rec "b c" d', global: false },
                    { text: 'plain', global: false },
                    { text: 'rec "s t"', global: false },
                    [[['-l'], ['a', 'mid'], ['x.txt'], ['t'], ['zz*']], [['b c', 'd'], []], [['s t']]],
                ],
            );
        }),
    );

    suite.effect(
        'fails a probe as a host error carrying a timeout once its shell outlasts the time limit',
        Effect.fnUntraced(function* () {
            const [shell, oracle] = yield* Effect.all([Shell, Oracle]);
            const home = yield* oracle.zshenv('exec /bin/sleep 30');
            const error = yield* Effect.flip(shell.dialect({ shell: yield* oracle.located('zsh'), environment: { ...oracle.environment, ZDOTDIR: home }, timeout: '10 millis' }));
            assert.deepStrictEqual([error.reason, Cause.isTimeoutError(error.cause)], [{ _tag: 'Host' }, true]);
        }),
    );

    suite.effect(
        'parses in a host lacking timers, microtasks, and fetch through a processor built from a compiled module, reaching no node: module from the library entry under the browser condition, as the Node host parses',
        Effect.fnUntraced(function* () {
            const [shell, spawner, path, { named }] = yield* Effect.all([Shell, ChildProcessSpawner.ChildProcessSpawner, Path.Path, Dialects]);
            const source = 'x=1; rec "$x" {a,b} [[ -n $x ]]';
            const entry = yield* path.fromFileUrl(new URL('sandbox.ts', import.meta.url));
            const printed = yield* spawner.string(ChildProcess.make(process.execPath, [entry, named.bash.binary, source], { env: {}, extendEnv: false, stderr: 'inherit' }));
            const [{ reached, left, syntax }, read] = yield* Effect.all([Schema.decodeEffect(Hosted)(printed), shell.read(source, { dialect: named.bash })]);
            assert.deepStrictEqual([reached.filter((url) => url.startsWith('node:')), reached.some((url) => url.endsWith('/sh-syntax/lib/browser.js')), left, syntax], [[], true, [], read.syntax]);
        }),
    );
});
