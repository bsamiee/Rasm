import { assert, it } from '@effect/vitest';
import { Shell } from '@rasm/shell';
import { Arbitrary, Array, Effect, FileSystem, Formatter, Match, Option, Redacted, Schema, Struct } from 'effect';
import { walk, type Word } from 'sh-syntax';
import { Dialects, Oracle } from '#/oracle';

// --- [TYPES] ---------------------------------------------------------------------------

type Script = Effect.Success<ReturnType<Shell['Service']['read']>>;
type Expansion = ReturnType<Script['expand']>[0];
type Field = Extract<Expansion, { readonly _tag: 'Expanded' }>['fields'][number];
type Recorded = Effect.Success<ReturnType<Oracle['Service']['run']>>['records'][number];
type Kind = 'bash' | 'zsh' | 'zsh -f' | '/bin/bash' | '/bin/sh' | 'dash';
interface Row {
    readonly shell: Kind;
    readonly word: string;
    readonly environment?: Readonly<Record<string, string>>;
    readonly operands?: readonly string[];
    readonly startup?: string;
    readonly fields: readonly string[];
    readonly exact: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const units = [
    'a',
    '-',
    '/',
    ':',
    "'a b'",
    "'*'",
    "''",
    '"a b"',
    '""',
    String.raw`"\$"`,
    `"\${x}"`,
    `"\${1}"`,
    `"\${@}"`,
    `"\${*}"`,
    `"a\${x}b"`,
    `\${x}`,
    `\${y}`,
    `\${u}`,
    `\${1}`,
    `\${@}`,
    `\${*}`,
    `\${#}`,
    `\${#x}`,
    `\${x:-d e}`,
    `\${u-d}`,
    `\${x:+p}`,
    `\${x#a}`,
    `\${y##*/}`,
    `\${x/a/b}`,
    `\${u:?}`,
    '{a,b}',
    '{1..3}',
    '{,c}',
    '*',
    '?',
    '[ab]',
    '$((1+2))',
    '$((1/0))',
    `$((\${#x}*2))`,
    String.raw`$'\x41b'`,
    '=rec',
    '=nosuch',
] as const;
const leading = ['', '~/', '~+/', '~-/', '~root/', '~nosuch/'] as const;
const values = Arbitrary.map(Arbitrary.array(Arbitrary.schema(Schema.Literals(['a', 'b c', ' ', '*', ':', '/', '.txt', '', '\n', '-'])), { maxLength: 3 }), (parts) => parts.join(''));
const words = Arbitrary.map(Arbitrary.all([Arbitrary.schema(Schema.Literals(leading)), Arbitrary.array(Arbitrary.schema(Schema.Literals(units)), { minLength: 1, maxLength: 4 })]), ([head, rest]) => `${head}${rest.join('')}`);
const roles = {
    Assignment: { at: () => ['x=', '; rec "$x"'] as const, shown: (field: Field) => Option.getOrThrow(field.text), real: (records: readonly Recorded[]) => records.map(({ args }) => args[0] ?? '') },
    Heredoc: { at: () => ['rec <<EOF\n', '\nEOF\n'] as const, shown: (field: Field) => Option.getOrThrow(field.text), real: (records: readonly Recorded[]) => records.map(({ input }) => input) },
    Operand: { at: () => ['rec <<< ', ''] as const, shown: (field: Field) => Option.getOrThrow(field.text), real: (records: readonly Recorded[]) => records.map(({ input }) => input.slice(0, -1)) },
    Pattern: { at: (subject: string) => [`case '${subject}' in `, ') rec y;; *) rec n;; esac'] as const, shown: (field: Field, subject: string) => (Option.getOrThrow(field.matches(subject)) ? 'y' : 'n'), real: (records: readonly Recorded[]) => records.map(({ args }) => args[0] ?? '') },
    Redirect: { at: () => ['rec >', '; rec *'] as const, shown: (field: Field) => Option.getOrThrow(field.text), real: (records: readonly Recorded[]) => (records.length === 2 ? (records[1]?.args ?? []).filter((name) => name !== 'record') : []) },
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const described = (expansion: Expansion): readonly string[] =>
    Match.valueTags(expansion, {
        Refused: ({ origin }) => [`Refused ${origin._tag}`],
        Expanded: ({ fields }) => fields.map((field) => Option.getOrElse(Option.as(field.text, 'Known'), () => Option.match(field.matching, { onSome: (matching) => (matching._tag === 'Paths' ? `Paths ${matching.unmatched._tag}` : 'Strings'), onNone: () => `${field.prefix}…${field.suffix}` }))),
    });
const covers = (field: Field, real: string) => real.startsWith(field.prefix) && real.endsWith(field.suffix) && (Option.getOrElse(field.matches(real), () => true) || field.pieces.map((piece) => (piece._tag === 'Text' || piece._tag === 'Wild' ? piece.text : '')).join('') === real);
const aligned = (fields: readonly Field[], reals: readonly string[]): boolean =>
    Option.match(Array.head(fields), {
        onNone: () => reals.length === 0,
        onSome: (field) => (Option.isSome(field.text) ? reals[0] !== undefined && covers(field, reals[0]) && aligned(fields.slice(1), reals.slice(1)) : Array.range(0, reals.length).some((taken) => reals.slice(0, taken).every((real) => covers(field, real)) && aligned(fields.slice(1), reals.slice(taken)))),
    });
const related = (expansion: Expansion, records: readonly Recorded[]) =>
    expansion._tag === 'Refused'
        ? records.length === 0
        : Option.match(Array.head(records), {
              onNone: () => expansion.refusals.length > 0,
              onSome: ({ args }) => (expansion.exact ? expansion.fields.length === args.length && Array.zip(expansion.fields, args).every(([field, real]) => covers(field, real)) : aligned(expansion.fields, args)),
          });
const read = Effect.fnUntraced(function* (kind: Kind, source: string, at: { readonly environment?: Readonly<Record<string, string>>; readonly operands?: readonly string[]; readonly startup?: string; readonly cwd?: string }) {
    const [shell, oracle, { probed }] = yield* Effect.all([Shell, Oracle, Dialects]);
    const started = yield* Option.match(Option.fromUndefinedOr(at.startup), { onNone: () => Effect.succeed({}), onSome: (lines) => (kind === 'bash' ? Effect.map(oracle.written(lines), (file) => ({ BASH_ENV: file })) : Effect.map(oracle.zshenv(lines), (home) => ({ ZDOTDIR: home }))) });
    const environment = { ...started, ...at.environment };
    const script = yield* shell.read(source, { dialect: yield* probed(kind === 'zsh -f' ? 'zsh' : kind, environment), operands: at.operands ?? [], ...(at.cwd === undefined ? {} : { cwd: at.cwd }) });
    const { records } = yield* oracle.run(kind === 'zsh -f' ? [script.dialect.binary, '-f', '-c', script.source, script.dialect.binary, ...(at.operands ?? [])] : script.argv(...(at.operands ?? [])), environment, at.cwd);
    return { script, records };
}, Effect.scoped);
const located = (script: Script, start: number) => {
    const found: Word[] = [];
    walk(script.syntax, (node) => {
        if (node.type === 'Word' && node.start === start) {
            found.push(node);
        }
    });
    return script.expand(Option.getOrThrow(Array.last(found)))[0];
};
const observed = Effect.fnUntraced(function* ({ shell, word, ...at }: Omit<Row, 'fields' | 'exact'>) {
    const { script, records } = yield* read(shell, `rec ${word}`, { ...at, cwd: (yield* Oracle).fixture });
    return { expansion: located(script, 4), records };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Dialects.layer, { excludeTestServices: true })('expansion', (suite) => {
    suite.effect.each<Row>([
        { shell: 'bash', word: String.raw`a"b c"'d'\ e`, fields: ['Known'], exact: true },
        { shell: 'bash', word: String.raw`"a\$b\\c\d\"e"`, fields: ['Known'], exact: true },
        { shell: 'bash', word: String.raw`$'\u00410'`, fields: ['Known'], exact: true },
        { shell: '/bin/bash', word: String.raw`$'\u00410'`, fields: ['Known'], exact: true },
        { shell: 'bash', word: String.raw`$'\x4g\101\cA\e\q\U0001F600'`, fields: ['Known'], exact: true },
        { shell: 'zsh', word: String.raw`$'\x4g\101\C-a\e\q\U0001F600'`, fields: ['Known'], exact: true },
        { shell: 'bash', word: String.raw`$'a\0b'c`, fields: ['Known'], exact: true },
        { shell: 'bash', word: String.raw`$'\xff'`, fields: ['…'], exact: true },
        { shell: 'bash', word: '$x', environment: { x: 'a b' }, fields: ['Known', 'Known'], exact: true },
        { shell: 'zsh', word: '$x', environment: { x: 'a b' }, fields: ['Known'], exact: true },
        { shell: 'bash', word: '$x', environment: { x: 'a:b c' }, startup: 'IFS=:', fields: ['Known', 'Known'], exact: true },
        { shell: 'bash', word: '$x', environment: { x: 'a :b::c' }, startup: "IFS=' :'", fields: ['Known', 'Known', 'Known', 'Known'], exact: true },
        { shell: 'bash', word: '/tmp/$x', startup: 'x=changed', fields: ['/tmp/…'], exact: false },
        { shell: 'zsh', word: '"/tmp/$x/y"', startup: 'x=changed', fields: ['/tmp/…/y'], exact: true },
        { shell: 'bash', word: '$nope', fields: [], exact: true },
        { shell: 'bash', word: '"$nope"', fields: ['Known'], exact: true },
        { shell: 'bash', word: `\${X:-/safe}/y`, environment: { X: '' }, fields: ['Known'], exact: true },
        { shell: 'bash', word: `\${X-/safe}/y`, environment: { X: '' }, fields: ['Known'], exact: true },
        { shell: 'bash', word: `\${5:-a b}`, fields: ['Known', 'Known'], exact: true },
        { shell: 'bash', word: `"\${u:-'a'}"\${u-"b c"}`, fields: ['Known'], exact: true },
        { shell: 'bash', word: `\${x:=v}$x`, environment: { x: '' }, fields: ['Known'], exact: true },
        { shell: 'bash', word: `\${x:+set}`, environment: { x: '' }, fields: [], exact: true },
        { shell: 'bash', word: `\${x?m}\${u:?}`, environment: { x: 'v' }, fields: ['Refused Variable'], exact: true },
        { shell: 'zsh', word: `\${x:?}`, environment: { x: '' }, fields: ['Refused Variable'], exact: true },
        { shell: 'bash', word: `\${#x}.\${x:t}`, environment: { x: 'abc' }, fields: ['Known'], exact: true },
        { shell: 'bash', word: `\${x#*/}.\${x##*/}.\${x%/*}.\${x%%/*}`, environment: { x: 'a/b/c' }, fields: ['Known'], exact: true },
        { shell: 'bash', word: `\${x/b/B}.\${x//b/B}.\${x/#a/A}.\${x/%b/B}.\${x/b*/Z}.\${x//[bc]/Z}.\${x/b/<&>}`, environment: { x: 'abcb' }, fields: ['Known'], exact: true },
        { shell: '/bin/bash', word: `\${x/b/<&>}`, environment: { x: 'abcb' }, fields: ['Known'], exact: true },
        { shell: 'bash', word: `\${x^^}\${x,}\${x^}`, environment: { x: 'aBc' }, fields: ['Known'], exact: true },
        { shell: '/bin/bash', word: `\${x^^}`, environment: { x: 'aBc' }, fields: ['Refused Unsupported'], exact: true },
        { shell: 'bash', word: `\${x:1:-1}\${x: -2}`, environment: { x: 'abcd' }, fields: ['Known'], exact: true },
        { shell: '/bin/bash', word: `\${x:1:-1}`, environment: { x: 'abcd' }, fields: ['Refused Arithmetic'], exact: true },
        { shell: 'zsh', word: `\${x:h:r}.\${x:e}.\${x:u}`, environment: { x: '/a/b.c' }, fields: ['Known'], exact: true },
        { shell: 'zsh', word: '$x:t:r.$x:/bin', environment: { x: '/a/b.c' }, fields: ['Known'], exact: true },
        { shell: 'zsh', word: `\${x:h}.\${y:a}`, environment: { x: 'b.c', y: '../a/./b' }, fields: ['Known'], exact: true },
        { shell: 'zsh', word: `\${(U)x}\${+x}\${+nope}`, environment: { x: 'aBc' }, fields: ['Known'], exact: true },
        { shell: 'zsh', word: `\${(j.,.)@}`, operands: ['a', '', 'b c'], fields: ['Known'], exact: true },
        { shell: 'zsh', word: `\${(s.:.)x}`, environment: { x: 'a::b:' }, fields: ['Known', 'Known'], exact: true },
        { shell: 'zsh', word: `"\${(s.:.)x}"`, environment: { x: 'a::b:' }, fields: ['Known', 'Known', 'Known'], exact: true },
        { shell: 'zsh', word: `"\${(@s.:.)x}"`, environment: { x: 'a::b:' }, fields: ['Known', 'Known', 'Known', 'Known'], exact: true },
        { shell: 'zsh', word: `\${(f)x}`, environment: { x: 'l1\nl2' }, fields: ['Known', 'Known'], exact: true },
        { shell: 'bash', word: '"$@"', operands: ['a b', '', 'c'], fields: ['Known', 'Known', 'Known'], exact: true },
        { shell: 'bash', word: '$@', operands: ['a b', '', 'c'], fields: ['Known', 'Known', 'Known'], exact: true },
        { shell: 'zsh', word: '$@', operands: ['a b', '', 'c'], fields: ['Known', 'Known'], exact: true },
        { shell: 'bash', word: '"$*"', operands: ['a b', '', 'c'], fields: ['Known'], exact: true },
        { shell: 'bash', word: '"x$@y"', operands: ['1', '2'], fields: ['Known', 'Known'], exact: true },
        { shell: 'bash', word: '"$@"', fields: [], exact: true },
        { shell: '/bin/bash', word: `"\${*}"\${y##*/}`, environment: { y: ' ' }, fields: [], exact: true },
        { shell: '/bin/sh', word: `""\${y}""`, environment: { y: ' ' }, fields: ['Known'], exact: true },
        { shell: 'bash', word: `""\${y}""`, environment: { y: ' ' }, fields: ['Known', 'Known'], exact: true },
        { shell: 'bash', word: `\${@:-x}`, fields: ['Known'], exact: true },
        { shell: 'bash', word: '$#.$0.$?', operands: ['a', 'b'], fields: ['Known'], exact: true },
        { shell: 'bash', word: `"\${@:2}"`, operands: ['a', 'b', 'c'], fields: ['Known', 'Known'], exact: true },
        { shell: 'zsh', word: `\${@:0:1}.\${@[1]}`, operands: ['a b', 'c'], fields: ['Known'], exact: true },
        { shell: 'bash', word: '~/x', environment: { HOME: '/h' }, fields: ['Known'], exact: true },
        { shell: 'zsh', word: "~+'*'", fields: ['…*'], exact: true },
        { shell: 'bash', word: "~+'*'", fields: ['Known'], exact: true },
        { shell: 'bash', word: '~-', fields: ['Known'], exact: true },
        { shell: 'zsh', word: '~-', fields: ['…'], exact: true },
        { shell: 'bash', word: '~root', fields: ['…'], exact: true },
        { shell: 'zsh', word: '~nosuch/x', fields: ['…/x'], exact: true },
        { shell: 'bash', word: '~"/x"', environment: { HOME: '/h' }, fields: ['Known'], exact: true },
        { shell: 'bash', word: 'a=~/x:~/y', environment: { HOME: '/h' }, fields: ['Known'], exact: true },
        { shell: '/bin/sh', word: 'a=~/x', environment: { HOME: '/h' }, fields: ['Known'], exact: true },
        { shell: 'bash', word: '{,c}', fields: ['Known'], exact: true },
        { shell: 'zsh', word: '{,c}', fields: ['Known', 'Known'], exact: true },
        { shell: 'dash', word: '{a,b}', fields: ['Known'], exact: true },
        { shell: 'zsh', word: `{a,b}\${@}`, operands: ['', ' '], fields: ['…'], exact: false },
        { shell: 'bash', word: 'x{Z..a}', fields: ['Known', 'Known', 'Known', 'Known', 'Known', 'Known', 'Known', 'Known'], exact: true },
        { shell: 'zsh', word: '{Z..a}', fields: ['Known', 'Known', 'Known', 'Known', 'Known', 'Known', 'Known', 'Known'], exact: true },
        { shell: 'bash', word: '{01..03}', fields: ['Known', 'Known', 'Known'], exact: true },
        { shell: '/bin/bash', word: '{01..03}', fields: ['Known', 'Known', 'Known'], exact: true },
        { shell: 'bash', word: '{a..e..2}', fields: ['Known', 'Known', 'Known'], exact: true },
        { shell: '/bin/bash', word: '{1..5..2}', fields: ['Known'], exact: true },
        { shell: 'zsh', word: '{a..e..2}', fields: ['Known'], exact: true },
        { shell: 'zsh', word: '{1..3..0}', fields: ['…'], exact: false },
        { shell: 'bash', word: '{1..3..-1}', fields: ['Known', 'Known', 'Known'], exact: true },
        { shell: 'bash', word: 'x{1,2}{a,b}{a}', fields: ['Known', 'Known', 'Known', 'Known'], exact: true },
        { shell: 'bash', word: '$((y=4))$y.$((x+1)).$((2**62*4)).$((16#ff)).$((64#@)).$((010))', environment: { x: '2' }, fields: ['Known'], exact: true },
        { shell: 'bash', word: '$((1<<64)).$((1<<-1)).$((-8>>1)).$((5|3)).$((5^3)).$((~5)).$((7/-2)).$((-7%3))', fields: ['Known'], exact: true },
        { shell: 'zsh', word: '$((x=1,x&&=0,x)).$((010))', fields: ['Known'], exact: true },
        { shell: 'dash', word: `\${x#a}$((1<<2))$y`, environment: { x: 'ab', y: 'c d' }, fields: ['Known', 'Known'], exact: true },
        { shell: 'bash', word: '$((x))', environment: { x: '1+2' }, fields: ['…'], exact: false },
        { shell: 'bash', word: '$((1/0))', fields: ['Refused Arithmetic'], exact: true },
        { shell: 'bash', word: '$((2**-1))', fields: ['Refused Arithmetic'], exact: true },
        { shell: 'bash', word: '$((08))', fields: ['Refused Arithmetic'], exact: true },
        { shell: 'bash', word: `"$(printf 'a b')"`, fields: ['…'], exact: true },
        { shell: 'bash', word: `$(printf 'a b')`, fields: ['…'], exact: false },
        { shell: 'bash', word: '<(:)', fields: ['…'], exact: true },
        { shell: 'zsh', word: '=nosuch', fields: ['…'], exact: true },
        { shell: 'zsh', word: '=rec', startup: 'setopt noequals', fields: ['Known'], exact: true },
        { shell: 'zsh', word: '$x', environment: { x: 'a' }, startup: 'setopt ksharrays', fields: ['Known'], exact: true },
        { shell: 'bash', word: '*.txt', fields: ['Paths Kept'], exact: false },
        { shell: 'bash', word: '*.zz', startup: 'shopt -s nullglob', fields: ['Paths Dropped'], exact: false },
        { shell: 'bash', word: '*.zz', startup: 'shopt -s nullglob failglob', fields: ['Paths Refused'], exact: false },
        { shell: 'zsh -f', word: '*.zz', fields: ['Paths Refused'], exact: false },
        { shell: 'zsh', word: '*.zz', startup: 'setopt nonomatch', fields: ['Paths Kept'], exact: false },
        { shell: 'zsh', word: '*.zz', startup: 'setopt nullglob', fields: ['Paths Dropped'], exact: false },
        { shell: 'bash', word: String.raw`"*".txt\*`, fields: ['Known'], exact: true },
        { shell: 'bash', word: '$g', environment: { g: '*.txt' }, fields: ['Paths Kept'], exact: false },
        { shell: 'zsh', word: '$g', environment: { g: '*.txt' }, fields: ['Known'], exact: true },
        { shell: 'zsh -f', word: `\${~g}`, environment: { g: '*.txt' }, fields: ['Paths Refused'], exact: false },
        { shell: 'bash', word: '**/*.txt', fields: ['Paths Kept'], exact: false },
        { shell: 'bash', word: '*.txt', startup: 'shopt -s dotglob', fields: ['Paths Kept'], exact: false },
        { shell: '/bin/bash', word: '.*', fields: ['Paths Kept'], exact: false },
        { shell: 'bash', word: '*.txt', startup: 'set -f', fields: ['Known'], exact: true },
        { shell: 'zsh -f', word: '*(.)', fields: ['Paths Refused'], exact: false },
    ])(
        '$shell expands $word under $environment, operands $operands, and startup "$startup" to $fields with exact $exact, which the real run fits',
        Effect.fnUntraced(function* ({ fields, exact, ...row }) {
            const { expansion, records } = yield* observed(row);
            assert.deepStrictEqual([described(expansion), expansion._tag === 'Refused' || expansion.exact, related(expansion, records)], [fields, exact, true], Formatter.format({ expansion, records: records.map(({ args }) => args) }));
        }),
    );

    suite.effect.each<{ readonly shell: Kind; readonly role: keyof typeof roles; readonly word: string; readonly environment?: Readonly<Record<string, string>>; readonly operands?: readonly string[]; readonly subject?: string; readonly fields: readonly string[] }>([
        { shell: 'bash', role: 'Assignment', word: '$y', environment: { y: 'a  b *' }, fields: ['Known'] },
        { shell: 'bash', role: 'Assignment', word: '{a,b}', fields: ['Known'] },
        { shell: 'bash', role: 'Assignment', word: '~/a:~/b', environment: { HOME: '/h' }, fields: ['Known'] },
        { shell: 'bash', role: 'Assignment', word: '$@', environment: { IFS: ':' }, operands: ['p', 'q'], fields: ['Known'] },
        { shell: 'zsh', role: 'Assignment', word: '$@', environment: { IFS: ':' }, operands: ['p', 'q'], fields: ['Known'] },
        { shell: 'bash', role: 'Heredoc', word: String.raw`"a" \"$y\" \$y ~/z * \q $@`, environment: { y: 'v w' }, operands: ['p', 'q'], fields: ['Known'] },
        { shell: 'bash', role: 'Operand', word: '$y', environment: { y: 'a  b *' }, fields: ['Known'] },
        { shell: 'bash', role: 'Pattern', word: '"a*"*', subject: 'a*b', fields: ['Strings'] },
        { shell: 'bash', role: 'Pattern', word: '"a*"*', subject: 'axb', fields: ['Strings'] },
        { shell: 'bash', role: 'Pattern', word: '$z', environment: { z: 'a*' }, subject: 'axb', fields: ['Strings'] },
        { shell: 'zsh', role: 'Pattern', word: '$z', environment: { z: 'a*' }, subject: 'axb', fields: ['Known'] },
        { shell: 'bash', role: 'Redirect', word: '$y', environment: { y: 'a b' }, fields: ['Refused Ambiguous'] },
        { shell: 'bash', role: 'Redirect', word: '{m,n}', fields: ['Refused Ambiguous'] },
        { shell: 'zsh', role: 'Redirect', word: '{m,n}', fields: ['Known', 'Known'] },
        { shell: 'dash', role: 'Redirect', word: '$y', environment: { y: 'a b' }, fields: ['Known'] },
        { shell: '/bin/sh', role: 'Redirect', word: '$y', environment: { y: 'a b' }, fields: ['Known'] },
    ])(
        '$shell expands $word as a $role under $environment to $fields, as the real run shows',
        Effect.fnUntraced(function* ({ shell, role, word, subject = '', fields, ...at }) {
            const [before, after] = roles[role].at(subject);
            const { script, records } = yield* read(shell, `${before}${word}${after}`, at);
            const expansion = located(script, before.length);
            assert.deepStrictEqual([described(expansion), expansion._tag === 'Refused' ? [] : expansion.fields.map((field) => roles[role].shown(field, subject))], [fields, roles[role].real(records)]);
        }),
    );

    suite.effect(
        'expands a word holding a redacted hole to a secret piece that no field text or formatted form shows, as the real run receives it whole',
        Effect.fnUntraced(function* () {
            const [shell, oracle, { named }] = yield* Effect.all([Shell, Oracle, Dialects]);
            const script = yield* shell.script(named.bash)`rec ${Redacted.make('hunter2')} "a${Redacted.make('b c')}"`;
            const operands = script.syntax.stmts[0]?.cmd?.type === 'CallExpr' ? script.syntax.stmts[0].cmd.args.slice(1) : [];
            const fields = operands.flatMap((word) => Match.valueTags(script.expand(word)[0], { Expanded: (expansion) => expansion.fields, Refused: () => [] }));
            const { records } = yield* oracle.run(script.argv());
            assert.deepStrictEqual(
                [fields.map((field) => [Option.isNone(field.text), field.pieces.map(Struct.get('_tag'))]), fields.some((field) => /hunter2|b c/u.test(Formatter.format(field))), records.map(({ args }) => args)],
                [
                    [
                        [true, ['Secret']],
                        [true, ['Secret']],
                    ],
                    false,
                    [['hunter2', 'ab c']],
                ],
            );
        }),
    );

    suite.effect.each([
        { dialect: 'bash', logical: true },
        { dialect: 'zsh', logical: false },
    ] as const)(
        '$dialect expands ~+ to the working folder as PWD names it when logical is $logical, and as its physical path otherwise',
        Effect.fnUntraced(function* ({ dialect, logical }) {
            const [shell, oracle, fs, { probed }] = yield* Effect.all([Shell, Oracle, FileSystem.FileSystem, Dialects]);
            const physical = yield* fs.realPath(oracle.fixture);
            const environment = { PWD: logical ? oracle.fixture : '/' };
            const script = yield* shell.read('rec ~+', { dialect: yield* probed(dialect, environment), cwd: oracle.fixture });
            const { records } = yield* oracle.run(script.argv(), environment, oracle.fixture);
            const expansion = located(script, 4);
            const expected = logical ? oracle.fixture : physical;
            assert.deepStrictEqual([oracle.fixture === physical, expansion._tag === 'Expanded' ? expansion.fields.map((field) => field.text) : [], records.map(({ args }) => args)], [false, [Option.some(expected)], [[expected]]]);
        }),
    );

    suite.effect.prop(
        'expands any word of quotes, parameters, operators that refuse, braces, globs, arithmetic, equals, and tilde forms to fields or a refusal the real run fits',
        [Arbitrary.schema(Schema.Literals(['bash', 'zsh', '/bin/bash'])), words, values, values, Arbitrary.array(values, { maxLength: 3 })],
        Effect.fnUntraced(function* ([shell, word, x, y, operands]) {
            const { expansion, records } = yield* observed({ shell, word, environment: { x, y, HOME: '/home/u' }, operands });
            assert.isTrue(related(expansion, records), Formatter.format({ shell, word, x, y, operands, expansion, records: records.map(({ args }) => args) }));
        }),
        { arbitrary: { runs: 60 } },
    );
});
