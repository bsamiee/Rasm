import { assert, it } from '@effect/vitest';
import { Shell } from '@rasm/shell';
import { Array, Effect, Equal, Option, Path, Struct } from 'effect';
import { processor } from 'sh-syntax/sync';
import { Dialects, Oracle } from '#/oracle';

// --- [TYPES] ---------------------------------------------------------------------------

type Script = Effect.Success<ReturnType<Shell['Service']['read']>>;
type Invocation = Effect.Success<Script['invocations']>[number];
type Recorded = Effect.Success<ReturnType<Oracle['Service']['run']>>['records'][number];

// --- [OPERATIONS] ----------------------------------------------------------------------

const flat = (held: readonly Invocation[]): readonly Invocation[] => held.flatMap((invocation) => [invocation, ...flat(invocation.inner)]);
const known = (fields: Invocation['reads']) => Array.getSomes(fields.map(Struct.get('text'))).filter((text) => !text.endsWith('/**'));
const fits = (record: Recorded, invocation: Invocation) => {
    const words = invocation.words.slice(1);
    const written = words.map(({ field }) => field.text);
    return invocation.program._tag === 'External' && invocation.program.name === record.program && (!words.every(Struct.get('exact')) || words.length === record.args.length) && Array.zipWith(written, record.args, (text, arg) => Option.isNone(text) || Equal.equals(text, Option.some(arg))).every(Boolean);
};
const unmatched = (records: readonly Recorded[], invocations: readonly Invocation[]) => records.filter((record) => !invocations.some((invocation) => fits(record, invocation))).map(({ program, args }) => [program, ...args]);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Dialects.layer, { excludeTestServices: true })('walk', (suite) => {
    suite.effect.each([
        { shell: 'bash', source: 'x=/safe; y=/safe; for i in 1 2 3; do rec -rf "$y"; y=$x; x=/; done', wrapper: '' },
        { shell: 'zsh', source: 'x="a b"; rec $x', wrapper: '' },
        { shell: 'bash', source: `X=; rec \${X:-/safe}/y`, wrapper: '' },
        { shell: 'bash', source: 'f() { local v=in; rec "$1" "$v"; }; v=out; f a; rec "$v"', wrapper: '' },
        { shell: 'bash', source: 'cd /tmp && rec a; if false; then rec no; else rec yes; fi; case x in x) rec case;; *) rec other;; esac', wrapper: '' },
        { shell: 'bash', source: 'n=0; while [ "$n" != 3 ]; do rec "$n"; n=$((n+1)); done', wrapper: '' },
        { shell: 'bash', source: 'eval "rec ev"; rec $(rec inner) | rec piped', wrapper: '' },
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
    ] as const)(
        '$shell walks $source to invocations every recorded real command fits, each inner one under wrapper "$wrapper"',
        Effect.fnUntraced(function* ({ shell: kind, source, wrapper }) {
            const [shell, oracle, { named, probed }] = yield* Effect.all([Shell, Oracle, Dialects]);
            const environment = kind === 'aliased' ? { ZDOTDIR: yield* oracle.zshenv(`alias ll='rec -l' quiet='rec q'; alias -g G=mid; alias -s txt=rec`) } : {};
            const script = yield* shell.read(source, { dialect: kind === 'aliased' ? yield* probed('zsh', environment) : named[kind], cwd: oracle.fixture });
            const [invocations, { records }] = yield* Effect.all([script.invocations, oracle.run(script.argv(), environment, oracle.fixture)]);
            const under = flat(invocations).filter(({ within: [closest] }) => wrapper === '' || (closest?._tag === 'Wrapper' && closest.name === wrapper));
            assert.deepStrictEqual<unknown>([records.length > 0, unmatched(records, under), invocations.some(({ opaque, program }) => opaque && program._tag === 'External' && program.name === wrapper)], [true, [], false]);
        }),
    );

    suite.effect.each([
        { shell: 'bash', source: 'cd dir && rec -r -o out --out=o2 -- a -b < ../a.txt > log', deletes: ['a', '-b'], writes: ['out', 'o2', 'log'], reads: ['../a.txt'], opaque: false },
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
