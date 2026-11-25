import { assert, it } from '@effect/vitest';
import { Policy } from '@rasm/policy';
import { Shell } from '@rasm/shell';
import { Array, Effect, Match, Option, String } from 'effect';
import { call, dialect, judging } from '#/fixtures';

// --- [TYPES] ---------------------------------------------------------------------------

type Judgment = Effect.Success<ReturnType<Policy['judge']>>;
type Invocation = Effect.Success<Effect.Success<ReturnType<Shell['Service']['read']>>['invocations']>[number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const policy = new Policy({
    'stand.refuse': { program: 'rm', refuse: 'deletes files', instruct: 'Move them aside.' },
    'stand.patch': { program: 'zzpm', path: ['patch'], refuse: 'copies a package into the tree', instruct: 'Set the package option.' },
    'stand.rename': { program: 'grep', rename: { '-i': '--ignore-case' } },
    'stand.follow': {
        program: 'grep',
        judge: ({ words }) => {
            const held = words.find(({ field }) => Option.contains(field.text, '--ignore-case'));
            return held === undefined || words.some(({ field }) => Option.contains(field.text, '-n')) ? [] : Option.match(held.node, { onNone: () => [], onSome: (word) => [{ edit: (editor) => [editor.after(word, ['-n'])] }] });
        },
    },
    'stand.left': {
        program: 'cat',
        judge: ({ words }) =>
            Option.match(
                Option.flatMap(Array.head(words), ({ node }) => node),
                { onNone: () => [], onSome: (word) => [{ edit: (editor) => [editor.replace(word, ['bat'])] }] },
            ),
    },
    'stand.right': {
        program: 'cat',
        judge: ({ words }) =>
            Option.match(
                Option.flatMap(Array.head(words), ({ node }) => node),
                { onNone: () => [], onSome: (word) => [{ edit: (editor) => [editor.replace(word, ['dog'])] }] },
            ),
    },
    'stand.throws': {
        program: 'echo',
        judge: () => {
            throw new Error('boom');
        },
    },
});
const rows = [
    { name: 'a refusal beats a rewrite in the same script', command: 'grep -i x f; rm a', tag: 'Refuse', text: 'rm deletes files. Move them aside.' },
    { name: 'a rename is judged again and a later rule edits what it wrote', command: 'grep -i x f', tag: 'Rewrite', text: 'grep --ignore-case -n x f' },
    { name: 'overlapping edits apply none and name both rules', command: 'cat f', tag: 'Pass', text: 'stand.left, stand.right: edits overlap' },
    { name: 'a throwing rule lets the call run with a note', command: 'echo hi', tag: 'Pass', text: 'stand.throws: failed and judged nothing' },
    { name: 'no rule speaks and the call passes', command: 'true', tag: 'Pass', text: '' },
    { name: 'a refusal names the subcommand of a program with no usage', command: 'zzpm patch left-pad', tag: 'Refuse', text: 'zzpm patch copies a package into the tree. Set the package option.' },
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const spoken = ({ words }: Invocation) => Array.getSomes(words.map(({ field }) => field.text)).join(' ');
const said = (judgment: Judgment, found: Effect.Success<typeof dialect>) =>
    Match.valueTags(judgment, {
        Refuse: ({ reasons }) => Effect.succeed<readonly string[]>(reasons),
        Rewrite: ({ command }) =>
            Effect.map(
                Effect.flatMap(
                    Shell.use((shell) => shell.read(command, { dialect: found })),
                    (script) => script.invocations,
                ),
                Array.map(spoken),
            ),
        Pass: ({ notes }) => Effect.succeed(notes.map(({ text }) => text)),
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(judging, { excludeTestServices: true })('fold', (suite) => {
    suite.effect.each(rows.map((row, index) => ({ ...row, index })))('$name', ({ command, index, tag, text }) =>
        Effect.gen(function* () {
            const found = yield* dialect;
            const judgment = yield* policy.judge(call(command, index), Effect.succeed(found), import.meta.dirname);
            const lines = yield* said(judgment, found);
            assert.strictEqual(judgment._tag, tag);
            assert.isTrue(text === '' ? Array.isReadonlyArrayEmpty(lines) : lines.some(String.startsWith(text)), `${judgment._tag}: ${lines.join(' | ')}`);
        }),
    );
});

it('a policy hears the agent calls of the kinds its rules name', () => {
    assert.deepStrictEqual(
        [call('true', 0), { tool: 'Read', tool_use_id: 'read', file_path: '/etc/hosts' }, { tool: 'Bash', command: 'true' }].map((input) => policy.hears(input)),
        [true, false, false],
    );
});
