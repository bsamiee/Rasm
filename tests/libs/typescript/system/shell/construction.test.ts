import { inspect } from 'node:util';
import { assert, it } from '@effect/vitest';
import { Shell } from '@rasm/shell';
import { Arbitrary, Array, Effect, Formatter, Option, Redactable, Redacted, Schema, Struct } from 'effect';
import { charsets, Dialects, Oracle } from '#/oracle';

// --- [TYPES] ---------------------------------------------------------------------------

type Template = ReturnType<Shell['Service']['script']>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const special = ["'", '"', '$', '`', '\\', '\n', '\nEOF\n', 'EOF', ' ', '\t', '\n\t', '*', '?', '[', ']', '~', '!', '{', '}', ',', '..', '#', ';', '&', '|', '<', '>', '(', ')', '=', '-', '+', '%', '^', 'a', 'é', '𝄞', '$(rec x)', `\${x}`, '\\\n', 'y=1'] as const;
const unit = Arbitrary.filter(Arbitrary.schema(Schema.Union([Schema.Literals(special), Schema.String.check(Schema.isMinLength(1), Schema.isMaxLength(1))])), (text) => !text.includes('\0') && text.isWellFormed());
const value = Arbitrary.map(Arbitrary.array(unit, { maxLength: 12 }), (units) => units.join(''));
const lined = Arbitrary.filter(value, (text) => !`<${text}>`.split('\n').some((line) => line === 'EOF' || line.startsWith('\t')));
const posixy = Arbitrary.map(Arbitrary.array(Arbitrary.schema(Schema.Literals([',', '..', '.', 'a', '1', '-', '+', '/', '@', ':', '_'])), { maxLength: 6 }), (units) => units.join(''));
const runs = { arbitrary: { runs: 60 } } as const;
const heredocs = [
    { stripped: false, build: (script: Template, held: string) => script`rec <<'EOF'\n<${held}>\nEOF\n` },
    { stripped: false, build: (script: Template, held: string) => script`rec <<EOF\n<${held}>\nEOF\n` },
    { stripped: true, build: (script: Template, held: string) => script`rec <<-'EOF'\n\t<${held}>\n\tEOF\n` },
    { stripped: true, build: (script: Template, held: string) => script`rec <<-EOF\n\t<${held}>\n\tEOF\n` },
    { stripped: false, build: (script: Template, held: string) => script`rec <<$'EOF'\n<${held}>\nEOF\n` },
] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Dialects.layer, { excludeTestServices: true })('construction', (suite) => {
    Struct.keys(charsets).forEach((charset) => {
        suite.effect.prop(
            `carries any value through every portable hole site and heredoc form, and any operands through argv, as exactly their text, under the ${charset} charset`,
            [Arbitrary.schema(Schema.Literals(['sh', '/bin/sh', 'bash', 'zsh'])), value, value, Arbitrary.schema(Schema.Boolean), Arbitrary.array(value, { maxLength: 4 }), Arbitrary.schema(Schema.Literals([0, 1, 2, 3])), lined, Arbitrary.array(value, { maxLength: 3 })],
            Effect.fnUntraced(function* ([shell, held, other, same, list, form, body, operands]) {
                const [{ script }, oracle, { localized }] = yield* Effect.all([Shell, Oracle, Dialects]);
                const named = localized[charset];
                const right = same ? held : other;
                const [built, heredoc] = yield* Effect.all([
                    script(named[shell])`case ${held} in ${right}) v=same;; *) v=apart;; esac; rec "$v" ${held} "<${held}>" '<${held}>' "$(printf %s ${held})" "$(printf %s "<${held}>")" "$(printf %s '<${held}>')" first ${list} last "$0" "$@"`,
                    heredocs[form].build(script(named[shell]), body),
                ]);
                const ran = yield* Effect.all([oracle.run(built.argv(...operands, Redacted.make('kept')), charsets[charset]), oracle.run(heredoc.argv(), charsets[charset])], { concurrency: 'unbounded' });
                assert.deepStrictEqual(ran.flatMap(Struct.get('records')).map(Struct.pick(['args', 'input'])), [
                    { args: [held === right ? 'same' : 'apart', held, `<${held}>`, `<${held}>`, held.replace(/\n+$/u, ''), `<${held}>`, `<${held}>`, 'first', ...list, 'last', yield* oracle.located(shell), ...operands, 'kept'], input: '' },
                    { args: [], input: `<${body}>\n` },
                ]);
            }),
            runs,
        );

        suite.effect.prop(
            `carries any value through brace alternatives, a here-string, a [[ ]] operand, and an ANSI-C quoted heredoc as exactly its text, under the ${charset} charset`,
            [Arbitrary.schema(Schema.Literals(['bash', 'zsh'])), value, value, Arbitrary.schema(Schema.Boolean), posixy, lined],
            Effect.fnUntraced(function* ([shell, held, other, same, bare, body]) {
                const [{ script }, oracle, { localized }] = yield* Effect.all([Shell, Oracle, Dialects]);
                const named = localized[charset];
                const right = same ? held : other;
                const [built, heredoc] = yield* Effect.all([script(named[shell])`[[ ${held} == ${right} ]] && v=same || v=apart; rec "$v" {x,${held}} {x,${bare}} <<< ${held}`, heredocs[4].build(script(named[shell]), body)]);
                const ran = yield* Effect.all([oracle.run(built.argv(), charsets[charset]), oracle.run(heredoc.argv(), charsets[charset])], { concurrency: 'unbounded' });
                assert.deepStrictEqual(ran.flatMap(Struct.get('records')).map(Struct.pick(['args', 'input'])), [
                    { args: [held === right ? 'same' : 'apart', 'x', held, 'x', bare], input: `${held}\n` },
                    { args: [], input: `<${body}>\n` },
                ]);
            }),
            runs,
        );
    });

    suite.effect.prop(
        'refuses a heredoc hole exactly when a line of its value would end the heredoc or lose the tabs <<- strips',
        [Arbitrary.schema(Schema.Literals(['bash', 'zsh'])), Arbitrary.schema(Schema.Literals([0, 1, 2, 3, 4])), value, Arbitrary.schema(Schema.Literals(['', '\nEOF\n', '\n\t'])), value],
        Effect.fnUntraced(function* ([shell, form, before, inserted, after]) {
            const { build, stripped } = heredocs[form];
            const held = `${before}${inserted}${after}`;
            const refused = yield* Effect.match(build((yield* Shell).script((yield* Dialects).named[shell]), held), { onSuccess: () => Option.none(), onFailure: ({ reason }) => Option.some(reason) });
            const ends = `<${held}>`.split('\n').some((line) => line === 'EOF' || (stripped && line.startsWith('\t')));
            assert.deepStrictEqual(refused, ends ? Option.some({ _tag: 'Construction', hole: 0, kind: 'Delimiter' }) : Option.none());
        }),
        runs,
    );

    suite.effect(
        'carries a hole as its text where the shell would read that text as syntax: the program word, numbers and booleans, a whole bash brace, a bash regex, a zsh global alias, a brace under ignorebraces, and an argument after words naming a shell',
        Effect.fnUntraced(function* () {
            const [shell, oracle, { named, probed }] = yield* Effect.all([Shell, Oracle, Dialects]);
            const home = yield* oracle.zshenv('alias -g G=mid; setopt ignorebraces');
            const [bash, zsh] = yield* Effect.all([
                shell.script(named.bash)`${'rec'} a; command ${'rec'} b; X=1 ${['rec', 'c']}; rec ${-1.5} ${true} ${[0, false]} {${'1..3'}} {${'a,b'}}; [[ xb.y =~ ${'b.'} ]] && rec has; [[ abc =~ ${'b.'} ]] || rec lacks; rec sh "$0" ${'rec ran'}`,
                Effect.flatMap(probed('zsh', { ZDOTDIR: home }), (dialect) => shell.script(dialect)`rec ${'G'} ${['G']} {${'1..3'}}`),
            ]);
            const ran = yield* Effect.all([oracle.run(bash.argv()), oracle.run(zsh.argv(), { ZDOTDIR: home }), oracle.shell('zsh', 'rec G {1..3}', { ZDOTDIR: home })], { concurrency: 'unbounded' });
            assert.deepStrictEqual(
                ran.flatMap(Struct.get('records')).map(({ program, args }) => [program, ...args]),
                [
                    ['rec', 'a'],
                    ['rec', 'b'],
                    ['rec', 'c'],
                    ['rec', '-1.5', 'true', '0', 'false', '{1..3}', '{a,b}'],
                    ['rec', 'has'],
                    ['rec', 'lacks'],
                    ['rec', 'sh', named.bash.binary, 'rec ran'],
                    ['rec', 'G', 'G', '{1..3}'],
                    ['rec', 'mid', '{1..3}'],
                ],
            );
        }),
    );

    suite.effect(
        'runs redacted holes with their values while its text, argv, JSON, and every formatted form hide each and show the rest',
        Effect.fnUntraced(function* () {
            const [shell, oracle, { named }] = yield* Effect.all([Shell, Oracle, Dialects]);
            const secret = 'hunter2 "and" it\'s';
            const built = yield* shell.script(named.bash)`rec ${Redacted.make(secret)} "${Redacted.make(secret, { label: 'token' })}" ${['open', Redacted.make(secret)]} plain`;
            const [{ records }, json] = yield* Effect.all([oracle.run(built.argv()), Schema.encodeUnknownEffect(Schema.fromJsonString(Schema.Unknown))(built)]);
            const shown = [Formatter.format(built), built.pipe(Redactable.redact, Formatter.format), Formatter.format(built.argv()), json, inspect(built)];
            assert.deepStrictEqual([records.map(({ args }) => args), shown.filter((text) => text.includes('hunter2')), built.toString()], [[[secret, secret, 'open', secret, 'plain']], [], 'rec <redacted> "<redacted:token>" open <redacted> plain']);
        }),
    );

    suite.effect(
        'refuses a hole where the shell evaluates its value as code, a name, or a number even when quoted, as that value written as a quoted word shows',
        Effect.fnUntraced(function* () {
            const [shell, oracle, { named }] = yield* Effect.all([Shell, Oracle, Dialects]);
            const rows = {
                bash: [
                    { build: (script: Template) => script`[[ ${'y=1'} -eq 1 ]]; rec "$y"`, raw: `[[ 'y=1' -eq 1 ]]; rec "$y"`, kind: 'Position', evaluated: [[['1'], '']] },
                    { build: (script: Template) => script`[[ -v ${'a[$(rec ran)]'} ]]`, raw: `[[ -v 'a[$(rec ran)]' ]]`, kind: 'Position', evaluated: [[['ran'], '']] },
                    { build: (script: Template) => script`test -v ${'a[$(rec ran)]'}`, raw: `test -v 'a[$(rec ran)]'`, kind: 'Position', evaluated: [[['ran'], '']] },
                    { build: (script: Template) => script`printf -v ${'a[$(rec ran)]'} x`, raw: `printf -v 'a[$(rec ran)]' x`, kind: 'Position', evaluated: [[['ran'], '']] },
                    { build: (script: Template) => script`printf -v${'a[$(rec ran)]'} x`, raw: `printf -v'a[$(rec ran)]' x`, kind: 'Position', evaluated: [[['ran'], '']] },
                    { build: (script: Template) => script`read ${'a[$(rec ran)]'} <<< x`, raw: `read 'a[$(rec ran)]' <<< x`, kind: 'Position', evaluated: [[['ran'], '']] },
                    { build: (script: Template) => script`declare -a x=${'(1 $(rec ran))'}`, raw: `declare -a x='(1 $(rec ran))'`, kind: 'Position', evaluated: [[['ran'], '']] },
                    { build: (script: Template) => script`declare -n r=${'a[$(rec ran)]'}; : "$r"`, raw: `declare -n r='a[$(rec ran)]'; : "$r"`, kind: 'Position', evaluated: [[['ran'], '']] },
                    { build: (script: Template) => script`eval ${'rec ran'}`, raw: `eval 'rec ran'`, kind: 'Position', evaluated: [[['ran'], '']] },
                    { build: (script: Template) => script`command eval ${'rec ran'}`, raw: `command eval 'rec ran'`, kind: 'Position', evaluated: [[['ran'], '']] },
                    { build: (script: Template) => script`eval "$(printf %s ${'rec ran'})"`, raw: `eval "$(printf %s 'rec ran')"`, kind: 'Position', evaluated: [[['ran'], '']] },
                    { build: (script: Template) => script`/bin/sh -c ${'rec ran'}`, raw: `/bin/sh -c 'rec ran'`, kind: 'Position', evaluated: [[['ran'], '']] },
                    { build: (script: Template) => script`ssh host ${'rec ran'}`, raw: `ssh host 'rec ran'`, kind: 'Position', evaluated: [[['ran'], '']] },
                    { build: (script: Template) => script`coproc ${'a b'} { rec ran; }; wait`, raw: `coproc 'a b' { rec ran; }; wait`, kind: 'Position', evaluated: [] },
                    { build: (script: Template) => script`rec <<EOF\n$(printf %s ${'a\nEOF\nb'})\nEOF\n`, raw: `rec <<EOF\n$(printf %s 'a\nEOF\nb')\nEOF\n`, kind: 'Delimiter', evaluated: [] },
                    { build: (script: Template) => script`rec <<-EOF\n\t$(printf %s ${'a\n\tb'})\n\tEOF\n`, raw: `rec <<-EOF\n\t$(printf %s 'a\n\tb')\n\tEOF\n`, kind: 'Delimiter', evaluated: [[[], 'a\nb\n']] },
                ],
                zsh: [
                    { build: (script: Template) => script`repeat ${'y=1'} do :; done; rec "$y"`, raw: `repeat 'y=1' do :; done; rec "$y"`, kind: 'Position', evaluated: [[['1'], '']] },
                    { build: (script: Template) => script`rec {${'1..3'}}`, raw: `rec {'1..3'}`, kind: 'Position', evaluated: [[['1', '2', '3'], '']] },
                    { build: (script: Template) => script`[[ abc =~ ${'a.c'} ]] && rec matched`, raw: `[[ abc =~ 'a.c' ]] && rec matched`, kind: 'Position', evaluated: [[['matched'], '']] },
                    { build: (script: Template) => script`print -v ${'a[$(rec ran)]'} x`, raw: `print -v 'a[$(rec ran)]' x`, kind: 'Position', evaluated: [[['ran'], '']] },
                    { build: (script: Template) => script`typeset -i n=${'y=1'}; rec "$y"`, raw: `typeset -i n='y=1'; rec "$y"`, kind: 'Position', evaluated: [[['1'], '']] },
                    { build: (script: Template) => script`integer n=${'y=1'}; rec "$y"`, raw: `integer n='y=1'; rec "$y"`, kind: 'Position', evaluated: [[['1'], '']] },
                ],
            } as const;
            const [bash, zsh, binaries] = yield* Effect.all([Effect.forEach(rows.bash, ({ build }) => Effect.flip(build(shell.script(named.bash)))), Effect.forEach(rows.zsh, ({ build }) => Effect.flip(build(shell.script(named.zsh)))), Effect.all([oracle.located('bash'), oracle.located('zsh')])]);
            const ran = yield* Effect.forEach(Array.zip(binaries, [rows.bash.map(Struct.get('raw')), rows.zsh.map(Struct.get('raw'))]), ([binary, raws]) => oracle.run([binary, '-c', 'for row; do (eval "$row"); rec "#"; done', binary, ...raws]), { concurrency: 'unbounded' });
            assert.deepStrictEqual(
                [[...bash, ...zsh].map(Struct.get('reason')), ran.flatMap(Struct.get('records')).map(({ args, input }) => [args, input])],
                [[...rows.bash, ...rows.zsh].map(({ kind }) => ({ _tag: 'Construction', hole: 0, kind })), [...rows.bash, ...rows.zsh].flatMap(({ evaluated }) => [...evaluated, [['#'], '']])],
            );
        }),
    );

    suite.effect(
        'refuses a template at each site with no quoting form for a hole, with the reason and the hole or text it names',
        Effect.fnUntraced(function* () {
            const [shell, { named }] = yield* Effect.all([Shell, Dialects]);
            const rows = [
                { name: 'arithmetic', dialect: 'bash', build: (script: Template) => script`rec $(( ${'v'} + 1 ))`, reason: { _tag: 'Construction', hole: 0, kind: 'Position' } },
                { name: 'a comment', dialect: 'bash', build: (script: Template) => script`rec x # ${'v'}`, reason: { _tag: 'Construction', hole: 0, kind: 'Position' } },
                { name: 'a parameter expansion', dialect: 'bash', build: (script: Template) => script`rec \${x:-${'v'}}`, reason: { _tag: 'Construction', hole: 0, kind: 'Position' } },
                { name: 'backquotes', dialect: 'bash', build: (script: Template) => script`rec \`rec ${'v'}\``, reason: { _tag: 'Construction', hole: 0, kind: 'Position' } },
                { name: 'after a dollar sign', dialect: 'bash', build: (script: Template) => script`rec $${'v'}`, reason: { _tag: 'Construction', hole: 0, kind: 'Position' } },
                { name: 'after a backslash', dialect: 'bash', build: (script: Template) => script`rec "\\${'v'}"`, reason: { _tag: 'Construction', hole: 0, kind: 'Position' } },
                { name: 'a heredoc delimiter', dialect: 'bash', build: (script: Template) => script`rec <<${'v'}\nbody\nEOF\n`, reason: { _tag: 'Construction', hole: 0, kind: 'Position' } },
                { name: 'an ANSI-C quoted string', dialect: 'bash', build: (script: Template) => script`rec $'${'v'}'`, reason: { _tag: 'Construction', hole: 0, kind: 'Position' } },
                { name: 'an array inside double quotes', dialect: 'bash', build: (script: Template) => script`rec "x${['a', 'b']}"`, reason: { _tag: 'Construction', hole: 0, kind: 'Position' } },
                { name: 'an array beside quoted text in one word', dialect: 'bash', build: (script: Template) => script`rec "x"${['a', 'b']}`, reason: { _tag: 'Construction', hole: 0, kind: 'Position' } },
                { name: 'an array as an assignment value', dialect: 'bash', build: (script: Template) => script`X=${['a', 'b']} rec`, reason: { _tag: 'Construction', hole: 0, kind: 'Position' } },
                { name: 'a value holding NUL', dialect: 'bash', build: (script: Template) => script`rec ${'a\0b'}`, reason: { _tag: 'Construction', hole: 0, kind: 'Value' } },
                { name: 'an invalid escape before a hole', dialect: 'bash', build: (script: Template) => script`rec \u00ZZ ${'v'}`, reason: { _tag: 'Construction', hole: 0, kind: 'Escape' } },
                { name: 'an invalid escape after a hole', dialect: 'zsh', build: (script: Template) => script`rec ${'v'} \u`, reason: { _tag: 'Construction', hole: 1, kind: 'Escape' } },
                { name: 'bash syntax under dash', dialect: 'dash', build: (script: Template) => script`[[ -n ${'v'} ]]`, reason: { _tag: 'Dialect' } },
                { name: 'a syntax error after a hole, at its offset in the text without holes', dialect: 'bash', build: (script: Template) => script`rec ${'value'} "open`, reason: { _tag: 'Syntax', offset: Option.some('rec  "open'.indexOf('"')), incomplete: true } },
            ] as const;
            const refused = yield* Effect.forEach(rows, ({ dialect, build }) => Effect.flip(build(shell.script(named[dialect]))));
            assert.deepStrictEqual(Array.zip(rows.map(Struct.get('name')), refused.map(Struct.get('reason'))), Array.zip(rows.map(Struct.get('name')), rows.map(Struct.get('reason'))));
        }),
    );
});
