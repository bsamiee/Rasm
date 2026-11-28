import { assert, it } from '@effect/vitest';
import { Shell } from '@rasm/shell';
import { Array, Effect, Equal, Match, Option, Path, Struct } from 'effect';
import { processor } from 'sh-syntax/sync';
import { Dialects, Oracle } from '#/oracle';

// --- [TYPES] ---------------------------------------------------------------------------

type Script = Effect.Success<ReturnType<Shell['Service']['read']>>;
type Invocation = Effect.Success<Script['invocations']>[number];
type Recorded = Effect.Success<ReturnType<Oracle['Service']['run']>>['records'][number];
type Origin = Extract<Invocation['words'][number]['field']['pieces'][number], { readonly _tag: 'Unknown' }>['origin'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const printers = (origin: Origin): readonly string[] =>
    Match.value(origin).pipe(
        Match.tag('Substitution', ({ programs }) => programs),
        Match.tag('Variable', ({ origins }) => [...origins].flatMap(printers)),
        Match.orElse(() => []),
    );
const flat = (held: readonly Invocation[]): readonly Invocation[] => held.flatMap((invocation) => [invocation, ...flat(invocation.inner)]);
const known = (fields: Invocation['reads']) => Array.getSomes(fields.map(Struct.get('text'))).filter((text) => !text.endsWith('/**'));
const heard = (invocation: Invocation, all: readonly Invocation[], files: ReadonlyMap<string, string>): Option.Option<string> =>
    Match.valueTags(invocation.stdin, {
        Inherited: () => Option.some(''),
        Closed: () => Option.some(''),
        Text: ({ text }) => text,
        File: ({ path }) => Option.flatMap(path.text, (file) => Option.fromUndefinedOr(files.get(file))),
        Piped: ({ pipeline, index }) =>
            Array.findFirst(all, (other) => other.program._tag === 'Builtin' && other.program.name === 'echo' && Option.exists(other.stage, (stage) => stage.pipeline === pipeline && stage.index === index)).pipe(
                Option.flatMap((echo) => Option.all(echo.words.slice(1).map((word) => word.text))),
                Option.map((words) => `${words.join(' ')}\n`),
            ),
    });
const fits = (record: Recorded, invocation: Invocation, all: readonly Invocation[], files: ReadonlyMap<string, string>) => {
    const words = invocation.words.slice(1);
    const written = words.map(({ field }) => field.text);
    return (
        invocation.program._tag === 'External' &&
        invocation.program.name === record.program &&
        (!words.every(Struct.get('exact')) || words.length === record.args.length) &&
        Array.zipWith(written, record.args, (text, arg) => Option.isNone(text) || Equal.equals(text, Option.some(arg))).every(Boolean) &&
        !Option.exists(heard(invocation, all, files), (input) => input !== record.input)
    );
};
const unmatched = (records: readonly Recorded[], invocations: readonly Invocation[], all: readonly Invocation[], files: ReadonlyMap<string, string>) => records.filter((record) => !invocations.some((invocation) => fits(record, invocation, all, files))).map(({ program, args, input }) => [program, ...args, input]);
const sourced = 'v=sourced\nrec file "$1" "$0"\n';

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Dialects.layer, { excludeTestServices: true })('walk', (suite) => {
    suite.effect.each([
        { shell: 'bash', source: 'x=/safe; y=/safe; for i in 1 2 3; do rec -rf "$y"; y=$x; x=/; done', wrapper: '' },
        { shell: 'zsh', source: 'x="a b"; rec $x', wrapper: '' },
        { shell: 'bash', source: `X=; rec \${X:-/safe}/y`, wrapper: '' },
        { shell: 'bash', source: 'f() { local v=in; rec "$1" "$v"; }; v=out; f a; rec "$v"', wrapper: '' },
        { shell: 'bash', source: 'cd /tmp && rec a; if false; then rec no; else rec yes; fi; case x in x) rec case;; *) rec other;; esac', wrapper: '' },
        { shell: 'bash', source: 'n=0; while [ "$n" != 3 ]; do rec "$n"; n=$((n+1)); done', wrapper: '' },
        { shell: 'bash', source: 'eval "rec ev"; rec $(rec inner) | rec piped', wrapper: '', printers: ['rec'] },
        { shell: 'zsh', source: 'echo 1 | read v; rec "$v"', wrapper: '' },
        { shell: 'aliased', source: 'll a\nquiet G\nx.txt', wrapper: '' },
        { shell: 'bash', source: '/usr/bin/env X=1 rec env', wrapper: 'env' },
        { shell: 'bash', source: '/usr/bin/timeout 5 rec timed', wrapper: 'timeout' },
        { shell: 'bash', source: '/usr/bin/nohup rec hup', wrapper: 'nohup' },
        { shell: 'bash', source: `/bin/sh -c 'rec nested; rec "two words"'`, wrapper: 'sh' },
        { shell: 'zsh', source: `ssh host 'rec remote'`, wrapper: 'ssh' },
        { shell: 'bash', source: `/bin/sh <<'EOF'\nrec heredoc\nEOF`, wrapper: 'sh' },
        { shell: 'bash', source: `/usr/bin/env /bin/sh -c 'rec deep'`, wrapper: 'sh' },
        { shell: 'bash', source: '/usr/bin/find . -maxdepth 0 -exec rec found {} +', wrapper: 'find' },
        { shell: 'bash', source: `x=1; export y=2; z=3 /bin/sh -c 'rec "$x" "$y" "$z" "$0" "$1"' zero one`, wrapper: 'sh' },
        { shell: 'zsh', source: `/bin/sh -c 'x="a b"; rec $x'`, wrapper: 'sh' },
        { shell: 'bash', source: '/bin/sh {file} one', wrapper: 'startup' },
        { shell: 'bash', source: `/bin/cat > {dir}/w.sh <<'EOF'\nrec written "$1"\nEOF\n/bin/sh {dir}/w.sh two`, wrapper: 'w.sh' },
        { shell: 'bash', source: 'f() { . {file} one; rec "$v" "$1"; }; f a', wrapper: '' },
        { shell: 'bash', source: 'echo p | { rec sorted | rec head; } | rec count', wrapper: '' },
        { shell: 'bash', source: `rec h <<'EOF'\nhd\nEOF\nrec f < {file}; rec n < /dev/null; { rec g; } < {file}; while rec w; do break; done <<< 'ws'; rec bg & wait`, wrapper: '' },
        { shell: 'bash', source: 'T=$(rec made); for d in "$T"; do f() { local u="$1"; export E="$u"; rec "$u" "$E"; }; f "$d"; done', wrapper: '', printers: ['rec'] },
        { shell: 'bash', source: `f() { rec fn; }; f <<< 'fs'; rec $(rec sub) <<< 'x'; if rec c <<< 'ci'; then rec t; fi < {file}; case a in a) rec arm;; esac <<< 'ca'; /bin/sh -c 'rec inner' <<< 'ns'`, wrapper: '', printers: ['rec'] },
        { shell: 'bash', source: 'if [ -e a.txt ]; then x=a; else x=b; fi; export y=$x; case $x in a) rec A "$y";; b) rec B "$y";; esac', wrapper: '' },
        { shell: 'bash', source: 'if [ -e none ]; then x=a; else x=b; fi; export y=$x; case $x in a) rec A "$y";; b) rec B "$y";; esac', wrapper: '' },
        { shell: 'bash', source: 'f() { export X=in; local L=in; }; L=out; f; rec "$X" "$L"', wrapper: '' },
    ] as const)(
        '$shell walks $source to invocations every recorded real command fits with the standard input it read, each inner one under wrapper or file "$wrapper", its unknown words naming the recorded programs $printers that printed them',
        Effect.fnUntraced(function* ({ shell: kind, source, wrapper, ...row }) {
            const [shell, oracle, { named, probed }, path] = yield* Effect.all([Shell, Oracle, Dialects, Path.Path]);
            const environment = kind === 'aliased' ? { ZDOTDIR: yield* oracle.zshenv(`alias ll='rec -l' quiet='rec q'; alias -g G=mid; alias -s txt=rec`) } : {};
            const file = yield* oracle.written(sourced);
            const script = yield* shell.read(source.replaceAll('{file}', file).replaceAll('{dir}', path.dirname(file)), { dialect: kind === 'aliased' ? yield* probed('zsh', environment) : named[kind], cwd: oracle.fixture });
            const [invocations, { records }] = yield* Effect.all([script.invocations, oracle.run(script.argv(), environment, oracle.fixture)]);
            const all = flat(invocations);
            const under = all.filter(({ within: [closest] }) => wrapper === '' || (closest?._tag === 'Wrapper' && closest.name === wrapper) || (closest?._tag === 'File' && closest.path.endsWith(`/${wrapper}`)));
            const printed = Array.dedupe(
                under
                    .flatMap(({ words }) => words)
                    .flatMap(({ field }) => field.pieces)
                    .flatMap((piece) => (piece._tag === 'Unknown' ? printers(piece.origin) : [])),
            );
            assert.deepStrictEqual<unknown>(
                [records.length > 0, unmatched(records, under, all, new Map([[file, sourced]])), invocations.some(({ opaque, program }) => opaque && program._tag === 'External' && program.name === wrapper), printed, new Set(records.map(({ program }) => program)).isSupersetOf(new Set(printed))],
                [true, [], false, 'printers' in row ? row.printers : [], true],
            );
        }),
    );

    suite.effect.each([
        { shell: 'bash', source: 'cd dir && rec -r -o out --out=o2 -- a -b < ../a.txt > log', deletes: ['a', '-b'], writes: ['out', 'o2', 'log'], reads: ['../a.txt'], opaque: false },
        { shell: 'bash', source: 'cd dir; rec -r -o out a > log', deletes: ['a'], writes: ['out', 'log'], reads: [], opaque: false },
        { shell: 'zsh', source: 'f=c.txt; rec -ro"$f" --o=o2 x y', deletes: ['x', 'y'], writes: ['c.txt', 'o2'], reads: [], opaque: false },
        { shell: 'bash', source: 'rec --unknown z -r w >> log', deletes: ['z', 'w'], writes: ['log'], reads: [], opaque: true },
    ] as const)(
        '$shell binds $source against a caller row to the flags, operands, and paths the real program received',
        Effect.fnUntraced(function* ({ shell: kind, source, deletes, writes, reads, opaque }) {
            const [oracle, { named }, path] = yield* Effect.all([Oracle, Dialects, Path.Path]);
            const shell = yield* Shell.make({
                processor,
                programs: [
                    {
                        name: 'rec',
                        flags: [
                            ['none', '-r'],
                            ['one', '-o', '--out'],
                        ],
                        values: { '-o': { roles: ['write'] } },
                        operands: [{ roles: ['delete'], count: 'rest', tree: true }],
                        abbreviates: true,
                    },
                ],
            });
            const script = yield* shell.read(source, { dialect: named[kind], cwd: oracle.fixture });
            const [invocations, { records }] = yield* Effect.all([script.invocations, oracle.run(script.argv(), {}, oracle.fixture)]);
            const [record] = records;
            const found = Array.findFirst(invocations, (invocation) => invocation.program._tag === 'External' && invocation.program.name === 'rec');
            const spelled = Option.match(found, { onNone: () => [], onSome: (invocation) => [...invocation.operands.map(Struct.get('word')), ...invocation.flags.map(Struct.get('spelled'))].map(Struct.get('field')).map(Struct.get('text')) });
            const resolved = (names: readonly string[]) => names.map((name) => path.resolve(record?.cwd ?? '', name));
            assert.deepStrictEqual<unknown>(
                [Option.map(found, (invocation) => ({ deletes: known(invocation.deletes), writes: known(invocation.writes), reads: known(invocation.reads), bound: invocation.has('-r'), opaque: invocation.opaque })), spelled.every(Option.isSome) && new Set(record?.args).isSupersetOf(new Set(Array.getSomes(spelled)))],
                [Option.some({ deletes: resolved(deletes), writes: resolved(writes), reads: resolved(reads), bound: true, opaque }), true],
            );
        }),
    );
});
