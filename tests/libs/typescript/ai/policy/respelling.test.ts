import { assert, it } from '@effect/vitest';
import { Policy } from '@rasm/policy';
import { Effect, Formatter, Match } from 'effect';
import { call, dialect, judging } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const policy = new Policy({
    'respell.sed': { program: 'sed', drop: ['-E'] },
    'respell.sort': { program: 'sort', drop: ['-k'] },
    'respell.grep': { program: 'grep', rename: { '-h': '-I', '--include': '-g' } },
    'respell.ls': { program: 'ls', rename: { '-a': '--all' } },
    'respell.log': { program: 'git', path: ['log'], ensure: ['--oneline'], unless: ['--format'] },
});
const rows = [
    { name: 'a drop takes one letter out of a cluster', command: 'sed -nE p f', ran: 'sed -n p f' },
    { name: 'a drop takes the separate value word with its flag', command: 'sort -k 2 -n f', ran: 'sort -n f' },
    { name: 'a drop leaves a value spelled like the flag', command: 'sed -e -E f', ran: 'sed -e -E f' },
    { name: 'a rename to a short flag swaps the letter in a cluster', command: 'grep -ch x f', ran: 'grep -cI x f' },
    { name: 'a rename matches any spelling of the flag', command: 'grep --no-filename x f', ran: 'grep -I x f' },
    { name: 'a rename to a short flag moves an attached value to its own word', command: "grep --include='*.ts' x .", ran: "grep -g '*.ts' x ." },
    { name: 'a rename to a long flag spills out of a cluster', command: 'ls -la d', ran: 'ls -l --all d' },
    { name: 'an ensure lands after the subcommand', command: 'git log -p', ran: 'git log --oneline -p' },
    { name: 'an ensure yields to a flag it names as unless', command: 'git log --format=%H', ran: 'git log --format=%H' },
] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(judging, { excludeTestServices: true })('respelling', (suite) => {
    suite.effect.each(rows.map((row, index) => ({ ...row, index })))('$name', ({ command, index, ran }) =>
        Effect.gen(function* () {
            const judgment = yield* policy.judge(call(command, index), dialect, import.meta.dirname);
            const said = Match.valueTags(judgment, { Rewrite: (rewrite) => rewrite.command, Pass: ({ notes }) => (notes.length === 0 ? command : Formatter.format(notes)), Refuse: ({ reasons }) => reasons.join('\n') });
            assert.strictEqual(said, ran);
        }),
    );
});
