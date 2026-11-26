import { assert, it } from '@effect/vitest';
import { Shell } from '@rasm/shell';
import { Array, Effect, Option } from 'effect';
import { createSourceIndex, type Node, walk } from 'sh-syntax';
import { Dialects, Oracle } from '#/oracle';

// --- [TYPES] ---------------------------------------------------------------------------

type Script = Effect.Success<ReturnType<Shell['Service']['read']>>;
type Editor = Parameters<Parameters<Shell['Service']['edit']>[1]>[0];
type Within = Parameters<Parameters<Shell['Service']['edit']>[1]>[1];
type Scripts = Readonly<Record<'pair' | 'one' | 'single' | 'double', Script>>;
type Found = <T extends Node['type']>(type: T, written: string) => Extract<Node, { readonly type: T }>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const finder =
    (script: Script): Found =>
    (type, written) => {
        const located = createSourceIndex(script.source);
        const found: Node[] = [];
        walk(script.syntax, (node) => {
            found.push(...(node.type === type && script.source.slice(located(node.start), located(node.end)) === written ? [node] : []));
        });
        return Option.getOrThrow(Array.findFirst(found, (node): node is Extract<Node, { readonly type: typeof type }> => node.type === type));
    };

// --- [CONSTANTS] -----------------------------------------------------------------------

const rows = [
    {
        name: 'words: a run replaced, a word inserted after, a prefix before the program',
        source: 'rec a -o out b; rec c',
        edits: (at: Found) => (editor: Editor) => [editor.replace([at('Word', '-o'), at('Word', 'out')], ['--x', "it's $HOME *"]), editor.after(at('Word', 'b'), ['{a,b}']), editor.before(at('Word', 'rec'), ['rec', 'pre'])],
        promised: [
            ['rec', 'pre', 'rec', 'a', '--x', "it's $HOME *", 'b', '{a,b}'],
            ['rec', 'c'],
        ],
    },
    {
        name: 'a value inside double quotes and inside an unquoted heredoc body',
        source: 'rec "pre $x post" <<EOF\nline $x end\nEOF\nrec after',
        edits: (at: Found) => (editor: Editor) => [editor.replace(at('ParamExp', '$x'), ['a"$b`c\\']), editor.replace(finder(editor.script)('Lit', 'line '), ['$y `z` \\ '])],
        promised: [
            ['rec', 'pre a"$b`c\\ post', '<$y `z` \\  end\n'],
            ['rec', 'after'],
        ],
    },
    {
        name: 'an assignment, a flag with its value, an assignment, a redirect, and a wrapper word removed',
        source: 'ssh h \'rec "$V"\'; X=1 rec a -o out b <<<in; wrap rec y',
        edits: (at: Found) => (editor: Editor) => [editor.assign(at('Stmt', 'ssh h \'rec "$V"\';'), 'V', "x 'y'"), editor.remove([at('Word', '-o'), at('Word', 'out')]), editor.remove(at('Assign', 'X=1')), editor.remove(at('Redirect', '<<<in')), editor.remove(at('Word', 'wrap'))],
        promised: [
            ['rec', "x 'y'"],
            ['rec', 'a', 'b'],
            ['rec', 'y'],
        ],
    },
    {
        name: 'commands removed with their separator, comment, operator, and heredoc',
        source: 'rec one; rec two # note\nrec three <<EOF\nbody\nEOF\nrec four && rec five && rec six\nrec seven',
        edits: (at: Found) => (editor: Editor) => [editor.remove(at('Stmt', 'rec two')), editor.remove(at('Stmt', 'rec three <<EOF\nbody\nEOF')), editor.remove(at('Stmt', 'rec five'))],
        promised: [
            ['rec', 'one'],
            ['rec', 'four'],
            ['rec', 'six'],
            ['rec', 'seven'],
        ],
    },
    {
        name: 'a command replaced and scripts joined before and after commands, grouped where an operator binds them',
        source: 'true && rec b\nfalse && rec c\nfalse && rec e\nfalse\nrec d',
        edits: (at: Found, scripts: Scripts) => (editor: Editor) => [editor.command(at('Stmt', 'rec b'), scripts.pair), editor.precede(scripts.one, ';', at('Stmt', 'rec c')), editor.precede(scripts.one, '||', at('Stmt', 'rec e')), editor.follow(at('Stmt', 'false'), '&&', scripts.pair)],
        promised: [
            ['rec', 'x'],
            ['rec', 'y'],
            ['rec', 'd'],
        ],
    },
    {
        name: 'an edit inside a single-quoted and a double-quoted nested script, quoted again for its quote',
        source: 'ssh h \'rec b c\'; ssh h "rec d"',
        edits: (at: Found, scripts: Scripts) => (editor: Editor, within: Within) => [
            editor.nested(at('Word', "'rec b c'"), within(scripts.single).replace(finder(scripts.single)('Word', 'c'), ['it\'s "$q"'])),
            editor.nested(at('Word', '"rec d"'), within(scripts.double).replace(finder(scripts.double)('Word', 'd'), ['it\'s "$q"'])),
        ],
        promised: [
            ['rec', 'b', 'it\'s "$q"'],
            ['rec', 'it\'s "$q"'],
        ],
    },
] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Dialects.layer, { excludeTestServices: true })('edits', (suite) => {
    suite.effect(
        'runs every promised argument list and leaves every other command as the real shell ran it, under bash and zsh, and refuses an edit that overlaps another, ends a heredoc, or leaves a loop without a condition',
        Effect.fnUntraced(function* () {
            const [shell, oracle, { named }] = yield* Effect.all([Shell, Oracle, Dialects]);
            const pairs = Array.cartesian(['bash', 'zsh'] as const, rows);
            const ran = yield* Effect.forEach(
                pairs,
                Effect.fnUntraced(function* ([dialect, { source, edits }]) {
                    const script = yield* shell.read(source, { dialect: named[dialect] });
                    const scripts = yield* Effect.all({ pair: shell.read('rec x; rec y', { dialect: named[dialect] }), one: shell.read('rec p', { dialect: named[dialect] }), single: shell.read('rec b c', { dialect: named.sh }), double: shell.read('rec d', { dialect: named.sh }) });
                    const { source: edited } = yield* shell.edit(script, edits(finder(script), scripts));
                    const { records } = yield* oracle.shell(dialect, edited);
                    return records;
                }),
            );
            const refusals = [
                { source: 'rec a', edits: (at: Found) => (editor: Editor) => [editor.replace(at('Word', 'a'), ['b']), editor.replace(at('Word', 'a'), ['c'])], reason: { _tag: 'Edit', offset: 4, kind: 'Overlap' } },
                { source: "rec <<'EOF'\nline\nEOF\n", edits: (at: Found) => (editor: Editor) => [editor.replace(at('Lit', 'line\nEOF'), ['x\nEOF\n'])], reason: { _tag: 'Edit', offset: 12, kind: 'Delimiter' } },
                { source: 'while rec a; do :; done', edits: (at: Found) => (editor: Editor) => [editor.remove(at('Stmt', 'rec a;'))], reason: { _tag: 'Edit', offset: 6, kind: 'Position' } },
            ] as const;
            const refused = yield* Effect.forEach(
                refusals,
                Effect.fnUntraced(function* ({ source, edits }) {
                    const script = yield* shell.read(source, { dialect: named.bash });
                    return yield* Effect.flip(shell.edit(script, edits(finder(script))));
                }),
            );
            assert.deepStrictEqual([ran.map(Array.map(({ program, args, input }) => [program, ...args, ...(input === '' ? [] : [`<${input}`])])), refused.map(({ reason }) => reason)], [pairs.map(([, { promised }]) => promised), refusals.map(({ reason }) => reason)]);
        }),
    );
});
