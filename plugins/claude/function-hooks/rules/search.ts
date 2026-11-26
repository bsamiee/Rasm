import { Array, Cause, Effect, FileSystem, Match, Option } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import type { Word } from 'sh-syntax';
import { covers, findUp, type Invocation, patternOf, rewrite, roots, type Table, type Verdict } from '#/rules/rule';

// --- [CONSTANTS] -----------------------------------------------------------------------

const placeholder = /\{(?:\/{0,2}|\/?\.)\}/u;
const modules = { inside: /(?:^|\/)node_modules(?:\/|$)/u, exclusion: /^!?(?:\.?\/|\*\*\/)?node_modules(?:\/(?:\*\*?)?)?$/u, path: /^[.*/]*node_modules[*/]*$/u };
const basicSyntax = /\\\\|\\(?<escaped>[(){}+?|])|(?<plain>^\*|[(){}+?|])/gu;
const backreference = /\\[1-9]/u;
const batchable: ReadonlySet<string> = new Set(['du', 'stat', 'ls', 'file', 'shasum', 'sha256sum', 'md5', 'cat', 'rm', 'touch', 'chmod', 'chown', 'xattr']);
const unrestricted = ['--no-ignore', '--no-ignore-vcs', '--unrestricted'];
const fdOptions: Readonly<Record<string, readonly string[]>> = { '-type': ['-t'], '-maxdepth': ['-d'], '-mindepth': ['--min-depth'], '-name': ['-g'], '-iname': ['-i', '-g'] };
const rgOptions = {
    dropped: ['--recursive', '--dereference-recursive', '--extended-regexp', '--with-filename', '--exclude-dir'],
    kept: ['--line-number', '--ignore-case', '--files-with-matches', '--count', '--word-regexp', '--invert-match', '--only-matching', '--after-context', '--before-context', '--context', '--regexp', '--fixed-strings', '--no-filename', '--include'],
    renamed: { '-h': '-I', '--include': '-g' },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const translate = (pattern: string) => pattern.replaceAll(basicSyntax, (matched: string, escaped?: string, plain?: string) => escaped ?? (plain === undefined ? matched : `\\${plain}`));
const parseExec = (values: Invocation['words'], appended: boolean) => {
    const [program = '', ...rest] = values.map(({ text }) => Option.getOrElse(text, () => ''));
    const holes = rest.filter((word) => placeholder.test(word)).length;
    return { program, batches: batchable.has(program) && ((appended && holes === 0) || (holes === 1 && rest.at(-1) === '{}')) };
};
const ignores = Effect.fnUntraced(function* (invocation: Invocation, cwd: string) {
    const [fs, spawner] = yield* Effect.all([FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner]);
    const searched = roots(invocation, cwd);
    const repository = Option.filter(yield* findUp(cwd, '.git'), (root) => !invocation.has(...unrestricted) && searched.every((path) => covers(root, path)));
    const paths = Option.match(repository, { onNone: () => [], onSome: (root) => searched.filter((path) => path !== root) });
    const real = yield* Effect.forEach(paths, (path) => fs.realPath(path).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(path))));
    const judged = Option.filter(Option.map(repository, (root) => ({ root, held: real.filter((path) => covers(root, path)) })), ({ held }) => Array.isReadonlyArrayNonEmpty(held));
    const code = yield* Option.match(judged, { onNone: () => Effect.succeed(1), onSome: ({ root, held }) => spawner.exitCode(ChildProcess.make({ cwd: root, stdin: 'ignore' })`git check-ignore --no-index -- ${held}`) });
    yield* Effect.when(new Cause.UnknownError(code, `git check-ignore exited ${code} on ${real.join(' ')}`), Effect.succeed(code > 1));
    return { repository, ignored: code === 0 || paths.some((path) => modules.inside.test(path)) };
});

// --- [RULES] ---------------------------------------------------------------------------

const search: Table = {
    'fd.hidden': { program: 'fd', aliases: ['fdfind'], ensure: ['--hidden'], unless: ['--no-hidden', '--unrestricted'] },
    'fd.batched': {
        program: 'fd',
        aliases: ['fdfind'],
        decide: (fd) =>
            fd.flags
                .filter(({ flag }) => flag.spellings.includes('--exec'))
                .map(({ spelling, values }) => {
                    const { program, batches } = parseExec(values, true);
                    return batches ? { rename: { '--exec': spelling.startsWith('--') ? '--exec-batch' : '-X' } } : { deny: `fd -x ${program} starts one process per search result.`, advice: 'Pass -X for a command taking every path in one run.' };
                }),
    },
    'find.batched': {
        program: 'find',
        aliases: ['gfind'],
        decide: (find) =>
            find.flags
                .filter(({ flag }) => flag.spellings.includes('-exec') || flag.spellings.includes('-execdir'))
                .flatMap(({ spelled, values }) => {
                    const { program, batches } = parseExec(values, false);
                    const terminator = Option.filter(Array.get(find.words, find.words.indexOf(Option.getOrElse(Array.last(values), () => spelled)) + 1), (word) => word.spells(';'));
                    const plus = Option.filter(
                        Option.flatMap(terminator, ({ node }) => node),
                        () => batches,
                    );
                    return Match.value({ plus: Option.getOrUndefined(plus), each: Option.isSome(terminator) }).pipe(
                        Match.when({ plus: Match.defined }, ({ plus: word }): readonly Verdict[] => [{ edit: (editor) => [editor.replace(word, ['+'])] }]),
                        Match.when({ each: true }, (): readonly Verdict[] => [{ deny: `find -exec ${program} starts one process per result.`, advice: 'End -exec with {} + for a command taking every path in one run.' }]),
                        Match.orElse(() => []),
                    );
                }),
    },
    'find.fd': {
        program: 'find',
        aliases: ['gfind'],
        decide: (find, { invocations }) => {
            const piped = invocations.some((next) => next.runs('xargs') && find.feeds(next));
            const excluded = find.flags.flatMap(({ flag }, at) => (flag.spellings.includes('-not') && Option.exists(Array.get(find.flags, at + 1), (next) => next.flag.spellings.includes('-path') && next.values.every((value) => value.test(modules.path))) ? [at, at + 1] : []));
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
            const translatable = Option.filter(patterns, (given) => given.every(({ node, basic, extended }) => !backreference.test(basic) && (basic === extended || Option.isSome(node))));
            const known = grep.only(...rgOptions.dropped, ...rgOptions.kept) && grep.flags.every(({ spelled }) => Option.isSome(spelled.text) || includes.some(([word]) => Option.contains(spelled.node, word)));
            const scoped = grep.values('--exclude-dir').every((value) => value.test(modules.inside));
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
                .filter(({ flag, values }) => flag.spellings.some((spelling) => spelling === '--exclude' || spelling === '--glob' || spelling === '--iglob') && values.every((value) => value.test(modules.exclusion)))
                .flatMap(({ spelled, values }) => {
                    const value = Option.flatMap(Array.last(values), ({ node }) => node);
                    return Option.toArray(Option.orElse<Word | readonly [Word, Word], Word>(Option.all([spelled.node, value]), () => spelled.node));
                });
            return Match.value({ ignored, redundant: Option.isSome(repository) && Array.isReadonlyArrayNonEmpty(exclusions) }).pipe(
                Match.when({ ignored: true }, () => [{ ensure: Array.of('--no-ignore') }]),
                Match.when({ redundant: true }, (): readonly Verdict[] => [{ edit: (editor) => exclusions.map((exclusion) => editor.remove(exclusion)) }]),
                Match.orElse(() => []),
            );
        }),
    },
    'grep.ignored': {
        program: 'grep',
        decide: Effect.fnUntraced(function* (grep, { invocations, cwd }) {
            const excludes = grep.has('--invert-match') && grep.flags.length === 1 && Option.exists(grep.operand(), (pattern) => modules.exclusion.test(pattern));
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
                return shallow ? [] : trees.filter((folder) => covers(folder, home)).map((folder) => `${invocation.label} walks ${folder}, which holds the whole home folder.`);
            });
            return Array.dedupe(walks).map((deny) => ({ deny, advice: 'Pass a project folder, or a depth of 2 at most.' }));
        },
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { search };
