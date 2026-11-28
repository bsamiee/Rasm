import type { FlagEdit, Invocation, Table, Verdict } from '@rasm/policy/rule';
import { Array, Cause, Effect, FileSystem, Match, Option, Record } from 'effect';
import type { Word } from 'sh-syntax';
import { covers, execute, findUp, patternOf, rewrite, roots } from '#/rules/rule';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLACEHOLDER = /\{(?:\/{0,2}|\/?\.)\}/u;
const MODULES = { inside: /(?:^|\/)node_modules(?:\/|$)/u, exclusion: /^!?(?:\.?\/|\*\*\/)?node_modules(?:\/(?:\*\*?)?)?$/u, path: /^[.*/]*node_modules[*/]*$/u };
const BASIC_SYNTAX = /\\\\|\\(?<escaped>[(){}+?|])|(?<plain>^\*|[(){}+?|])/gu;
const BACKREFERENCE = /\\[1-9]/u;
const BATCHABLE: ReadonlySet<string> = new Set(['du', 'stat', 'ls', 'file', 'wc', 'shasum', 'sha1sum', 'sha256sum', 'sha512sum', 'md5', 'md5sum', 'cksum', 'cat', 'rm', 'touch', 'chmod', 'chown', 'xattr']);
const perResult = 1000;
const walkers = {
    fd: { execs: ['--exec', '--exec-batch'], nul: ['--print0'], capped: (words: Array.NonEmptyReadonlyArray<string>) => Array.appendAll(words, ['--max-results', `${perResult + 1}`]) },
    find: { execs: ['-exec', '-execdir', '-ok', '-okdir'], nul: ['-print0'], capped: (words: Array.NonEmptyReadonlyArray<string>) => Array.make('/bin/sh', '-c', `"$@" | head -n ${perResult + 1}`, 'sh', ...words) },
} as const;
const printers: Readonly<Record<string, string>> = { basename: '{/}', dirname: '{//}' };
const unrestricted = ['--no-ignore', '--no-ignore-vcs', '--unrestricted'];
const fdOptions: Readonly<Record<string, readonly string[]>> = { '-type': ['-t'], '-maxdepth': ['-d'], '-mindepth': ['--min-depth'], '-name': ['-g'], '-iname': ['-i', '-g'] };
const rgOptions = {
    dropped: ['--recursive', '--dereference-recursive', '--extended-regexp', '--with-filename', '--exclude-dir'],
    kept: ['--line-number', '--ignore-case', '--files-with-matches', '--count', '--word-regexp', '--invert-match', '--only-matching', '--after-context', '--before-context', '--context', '--regexp', '--fixed-strings', '--no-filename', '--include'],
    renamed: { '-h': '-I', '--include': '-g' },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const translate = (pattern: string) => pattern.replaceAll(BASIC_SYNTAX, (matched: string, escaped?: string, plain?: string) => escaped ?? (plain === undefined ? matched : `\\${plain}`));
const parseExec = ({ words }: Invocation, { spelled, values }: Invocation['flags'][number], appended: boolean) => {
    const [program = '', ...rest] = values.map(({ text }) => Option.getOrElse(text, () => ''));
    const holes = rest.filter((word) => PLACEHOLDER.test(word)).length;
    const trailing = (appended && holes === 0) || (holes === 1 && rest.at(-1) === '{}');
    const terminator = Option.filter(Array.get(words, words.indexOf(Option.getOrElse(Array.last(values), () => spelled)) + 1), (word) => word.spells(';'));
    return { program, trailing, batches: trailing && BATCHABLE.has(program), alone: trailing && rest.length === holes, terminator };
};
const results = Effect.fnUntraced(function* (invocation: Invocation, cwd: string, { execs, nul, capped }: (typeof walkers)[keyof typeof walkers]) {
    const among = (names: readonly string[]) => invocation.flags.filter(({ flag }) => flag.spellings.some((spelling) => names.includes(spelling)));
    const dropped = new Set([...among(execs).flatMap((bound) => [bound.spelled, ...bound.values, ...Option.toArray(parseExec(invocation, bound, false).terminator)]), ...among(nul).map(({ spelled }) => spelled)]);
    const words = Option.filter(Option.all(invocation.words.filter((word) => !dropped.has(word)).map(({ text }) => text)), Array.isArrayNonEmpty);
    const folder = Option.filter(invocation.place._tag === 'Local' ? Option.some(Option.getOrElse(invocation.place.cwd.text, () => cwd)) : Option.none(), () => !invocation.has('-delete', '-ok', '-okdir', '-fprint', '-fprint0', '-fls'));
    const found = yield* Effect.transposeOption(Option.map(Option.all([words, folder]), ([given, at]) => execute(at)(capped(given))));
    return Option.flatMap(found, ({ stdout, code }) => Option.liftPredicate(stdout.split('\n').filter((line) => line !== '').length, () => code === 0));
});
const ignores = Effect.fnUntraced(function* (invocation: Invocation, cwd: string) {
    const fs = yield* FileSystem.FileSystem;
    const searched = roots(invocation, cwd);
    const repository = Option.filter(yield* findUp(cwd, '.git'), (root) => !invocation.has(...unrestricted) && searched.every((path) => covers(root, path)));
    const paths = Option.match(repository, { onNone: () => [], onSome: (root) => searched.filter((path) => path !== root) });
    const real = yield* Effect.forEach(paths, (path) => fs.realPath(path).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(path))));
    const judged = Option.filter(
        Option.map(repository, (root) => ({ root, held: real.filter((path) => covers(root, path)) })),
        ({ held }) => Array.isReadonlyArrayNonEmpty(held),
    );
    const { code } = yield* Option.match(judged, { onNone: () => Effect.succeed({ code: 1 }), onSome: ({ root, held }) => execute(root)(['git', 'check-ignore', '--no-index', '--', ...held]) });
    yield* Effect.when(new Cause.UnknownError(code, `git check-ignore exited ${code} on ${real.join(' ')}`), Effect.succeed(code > 1));
    return { repository, ignored: code === 0 || paths.some((path) => MODULES.inside.test(path)) };
});

// --- [RULES] ---------------------------------------------------------------------------

const search: Table = {
    'fd.hidden': { program: 'fd', aliases: ['fdfind'], ensure: ['--hidden'], unless: ['--no-hidden', '--unrestricted'] },
    'fd.batched': {
        program: 'fd',
        aliases: ['fdfind'],
        decide: Effect.fnUntraced(function* (fd, { cwd }) {
            const clauses = fd.flags
                .filter(({ flag }) => flag.spellings.includes('--exec'))
                .map((bound) => {
                    const parsed = parseExec(fd, bound, true);
                    const format = Option.filter(Record.get(printers, parsed.program), () => parsed.alone);
                    const end = Option.flatMap(
                        Option.orElse(parsed.terminator, () => Array.last(bound.values)),
                        ({ node }) => node,
                    );
                    return { ...parsed, bound, printed: Option.all([format, bound.spelled.node, end]) };
                });
            const found = clauses.some(({ printed, batches }) => Option.isNone(printed) && !batches) ? yield* results(fd, cwd, walkers.fd) : Option.none();
            return clauses.flatMap<Verdict<'Shell'> | FlagEdit>(({ program, trailing, batches, bound, printed }) =>
                Match.value({ printed: Option.getOrUndefined(printed), batches, trailing, flood: Option.exists(found, (count) => count > perResult) }).pipe(
                    Match.when({ printed: Match.defined }, ({ printed: [template, flag, last] }): readonly Verdict<'Shell'>[] => [{ edit: (editor) => [editor.replace([flag, last], ['--format', template])] }]),
                    Match.when({ batches: true }, () => [{ rename: { '--exec': bound.spelling.startsWith('--') ? '--exec-batch' : '-X' } }]),
                    Match.when({ flood: true }, (): readonly Verdict<'Shell'>[] => [
                        { subject: `fd -x ${program}`, deny: `starts one process per search result, and this search finds more than ${perResult}, a spawn rate that exhausts the kernel's task table`, values: { instead: 'pass -X where the command takes many paths, else narrow the search' } },
                    ]),
                    Match.when({ trailing: true }, (): readonly Verdict<'Shell'>[] => [{ subject: `fd -x ${program}`, note: 'starts one process per search result, where -X passes every path to one run of a command taking many', once: 'agent' }]),
                    Match.orElse(() => []),
                ),
            );
        }),
    },
    'find.batched': {
        program: 'find',
        aliases: ['gfind'],
        decide: Effect.fnUntraced(function* (find, { cwd }) {
            const clauses = find.flags
                .filter(({ flag }) => flag.spellings.includes('-exec') || flag.spellings.includes('-execdir'))
                .map((bound) => {
                    const parsed = parseExec(find, bound, false);
                    return {
                        ...parsed,
                        plus: Option.filter(
                            Option.flatMap(parsed.terminator, ({ node }) => node),
                            () => parsed.batches,
                        ),
                    };
                });
            const found = clauses.some(({ plus, terminator }) => Option.isNone(plus) && Option.isSome(terminator)) ? yield* results(find, cwd, walkers.find) : Option.none();
            return clauses.flatMap(({ program, trailing, terminator, plus }) =>
                Match.value({ plus: Option.getOrUndefined(plus), each: Option.isSome(terminator), trailing, flood: Option.exists(found, (count) => count > perResult) }).pipe(
                    Match.when({ plus: Match.defined }, ({ plus: word }): readonly Verdict<'Shell'>[] => [{ edit: (editor) => [editor.replace(word, ['+'])] }]),
                    Match.when({ each: true, flood: true }, (): readonly Verdict<'Shell'>[] => [
                        { subject: `find -exec ${program}`, deny: `starts one process per result, and this search finds more than ${perResult}, a spawn rate that exhausts the kernel's task table`, values: { instead: 'end -exec with {} + where the command takes many paths, else narrow the search' } },
                    ]),
                    Match.when({ each: true, trailing: true }, (): readonly Verdict<'Shell'>[] => [{ subject: `find -exec ${program}`, note: 'starts one process per result, where {} + passes every path to one run of a command taking many', once: 'agent' }]),
                    Match.orElse(() => []),
                ),
            );
        }),
    },
    'find.fd': {
        program: 'find',
        aliases: ['gfind'],
        decide: (find, { invocations }) => {
            const piped = invocations.some((next) => next.runs('xargs') && find.feeds(next));
            const excluded = find.flags.flatMap(({ flag }, at) => (flag.spellings.includes('-not') && Option.exists(Array.get(find.flags, at + 1), (next) => next.flag.spellings.includes('-path') && next.values.every((value) => value.test(MODULES.path))) ? [at, at + 1] : []));
            const options = Option.all(find.flags.map(({ flag, values }, at) => (excluded.includes(at) ? Option.some<readonly string[]>([]) : Option.map(Option.all([Option.fromUndefinedOr(fdOptions[flag.spellings[0]]), Option.all(values.map(({ text }) => text))]), ([spelling, given]) => [...spelling, ...given]))));
            const folders = Option.filter(Option.all(find.operands.map(({ word }) => word.text)), (given) => given.every((folder) => !/^[-(!]/u.test(folder)));
            const simple = find.inner.length === 0 && !piped && find.values('-name', '-iname').length <= 1;
            return Option.toArray(Option.filter(Option.all([options, folders]), () => simple)).flatMap(([flags, given]) =>
                rewrite(find, Array.make('fd', '--hidden', '--no-ignore', ...(excluded.length > 0 ? ['-E', 'node_modules'] : []), ...flags.flat(), ...(find.has('-name', '-iname') ? [] : ['-g', '*']), ...given.filter((folder) => folder !== '.'))),
            );
        },
    },
    'grep.rg': {
        program: 'grep',
        decide: Effect.fnUntraced(function* (grep, { cwd }) {
            const entries = yield* Effect.orElseSucceed(
                FileSystem.FileSystem.use((fs) => fs.readDirectory(cwd)),
                Array.empty<string>,
            );
            const includes = grep.flags.flatMap(({ flag, spelled, values }) => {
                const unmatched = Option.exists(spelled.field.matching, (matching) => matching._tag === 'Paths' && matching.unmatched._tag === 'Kept') && entries.every((entry) => Option.contains(spelled.field.matches(entry), false));
                const attached = Option.filter(Array.head(values), ({ node }) => Option.isNone(node));
                const glob = Option.flatMap(attached, ({ field }) => patternOf(field.pieces));
                return flag.spellings.includes('--include') && unmatched ? Option.toArray(Option.all([spelled.node, glob])) : [];
            });
            const literal = grep.has('--extended-regexp', '--fixed-strings');
            const written = [...grep.operands.filter(({ roles }) => !roles.includes('read')).map(({ word }) => word), ...grep.values('--regexp')];
            const patterns = Option.all(written.map(({ text, node }) => Option.map(text, (basic) => ({ node, basic, extended: literal ? basic : translate(basic) }))));
            const translatable = Option.filter(patterns, (given) => given.every(({ node, basic, extended }) => !BACKREFERENCE.test(basic) && (basic === extended || Option.isSome(node))));
            const known = grep.only(...rgOptions.dropped, ...rgOptions.kept) && grep.flags.every(({ spelled }) => Option.isSome(spelled.text) || includes.some(([word]) => Option.contains(spelled.node, word)));
            const scoped = grep.values('--exclude-dir').every((value) => value.test(MODULES.inside));
            const rg = Array.make('rg', ...(grep.has('--exclude-dir') ? [] : ['--no-ignore']), '--hidden');
            return Option.match(
                Option.filter(Option.all([grep.head, translatable]), () => grep.has('--recursive', '--dereference-recursive') && known && scoped),
                {
                    onNone: () => [],
                    onSome: ([head, given]) => [
                        { edit: (editor) => [editor.replace(head, rg), ...given.flatMap(({ node, basic, extended }) => (basic === extended ? [] : Option.toArray(node).map((word) => editor.replace(word, [extended])))), ...includes.map(([word, glob]) => editor.replace(word, ['-g', glob]))] },
                        { drop: rgOptions.dropped },
                        { rename: rgOptions.renamed },
                    ],
                },
            );
        }),
    },
    'search.ignored': {
        program: 'fd',
        aliases: ['fdfind', 'rg'],
        decide: Effect.fnUntraced(function* (invocation, { cwd }) {
            const { repository, ignored } = yield* ignores(invocation, cwd);
            const exclusions = invocation.flags
                .filter(({ flag, values }) => flag.spellings.some((spelling) => spelling === '--exclude' || spelling === '--glob' || spelling === '--iglob') && values.every((value) => value.test(MODULES.exclusion)))
                .flatMap(({ spelled, values }) => {
                    const value = Option.flatMap(Array.last(values), ({ node }) => node);
                    return Option.toArray(Option.orElse<Word | readonly [Word, Word], Word>(Option.all([spelled.node, value]), () => spelled.node));
                });
            return Match.value({ ignored, redundant: Option.isSome(repository) && Array.isReadonlyArrayNonEmpty(exclusions) }).pipe(
                Match.when({ ignored: true }, () => [{ ensure: Array.of('--no-ignore') }]),
                Match.when({ redundant: true }, (): readonly Verdict<'Shell'>[] => [{ edit: (editor) => exclusions.map((exclusion) => editor.remove(exclusion)) }]),
                Match.orElse(() => []),
            );
        }),
    },
    'grep.ignored': {
        program: 'grep',
        decide: Effect.fnUntraced(function* (grep, { invocations, cwd }) {
            const excludes = grep.has('--invert-match') && grep.flags.length === 1 && Option.exists(grep.operand(), (pattern) => MODULES.exclusion.test(pattern));
            const producer = Option.filter(
                Array.findFirst(invocations, (other) => other.runs('fd', 'fdfind', 'rg') && other.feeds(grep)),
                () => excludes,
            );
            const redundant = yield* Option.match(producer, { onNone: () => Effect.succeed(false), onSome: (found) => Effect.map(ignores(found, cwd), ({ repository, ignored }) => Option.isSome(repository) && !ignored) });
            return redundant ? Option.toArray(grep.node).map((stmt) => ({ edit: (editor) => [editor.remove(stmt)] })) : [];
        }),
    },
    'walk.wide': {
        action: 'Shell',
        decide: ({ dialect }, { invocations }) => {
            const home = dialect.environment['HOME'] ?? '/';
            const walks = invocations.flatMap((invocation) => {
                const depths = invocation.values('--max-depth', '-maxdepth').map(({ text }) => Option.match(text, { onNone: () => Number.NaN, onSome: Number }));
                const shallow = !invocation.runs('du') && Array.isReadonlyArrayNonEmpty(depths) && depths.every((depth) => depth <= 2);
                const trees = invocation.reads.filter(({ pieces }) => Option.exists(Array.last(pieces), (piece) => piece._tag === 'Wild' && piece.text === '/**')).map(({ prefix }) => prefix.replace(/\/+$/u, '') || '/');
                return shallow ? [] : trees.filter((folder) => covers(folder, home)).map((folder) => `${invocation.label} ${folder}`);
            });
            return Array.dedupe(walks).map((subject) => ({ subject, deny: 'walks a folder holding the whole home folder', values: { instead: 'pass a project folder, or a depth of 2 at most' } }));
        },
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { search };
