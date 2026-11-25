import { Array, Effect, FileSystem, Match, Option, Path } from 'effect';
import { RedirOperator } from 'sh-syntax';
import { type Call, command, type Editor, head, type Invocation, is, leaves, matches, named, nearest, operand, ran, said, type Script, span, spells, staged, type Table, texts, type Verdict, type Word, type Written } from '#/rules/rule';

// --- [TYPES] ---------------------------------------------------------------------------

type Field = Invocation['deletes'][number];
type Form = readonly [RegExp, (groups: Readonly<Record<string, string | undefined>>, invocation: Invocation) => Option.Option<Array.NonEmptyReadonlyArray<string>>];
interface Refusal {
    readonly refuse: string;
    readonly instruct: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const scratchpad = /\/claude-\d+(?:\/-[^/]*(?:\/[\da-f]{8}(?:-[\da-f]{4}){3}-[\da-f]{12}(?:\/scratchpad)?)?)?$/u;
const temporary = /^(?:\/private)?\/tmp(?=\/(?!claude-))/u;
const placeholder = /\{(?:\/{0,2}|\/?\.)\}/u;
const lettered = /[\p{L}\p{N}]/u;
const globbed = /[*?[]/u;
const syntax = /[$|&;<>()`]|^\w+=/u;
const modules = { root: /(?:^|\/)node_modules(?:\/|$)/u, only: /^!?(?:\.?\/|\*\*\/)?node_modules(?:\/(?:\*\*?)?)?$/u, path: /^[.*/]*node_modules[*/]*$/u };
const batchable: ReadonlySet<string> = new Set(['du', 'stat', 'ls', 'file', 'shasum', 'sha256sum', 'md5', 'cat', 'rm', 'touch', 'chmod', 'chown', 'xattr']);
const removers = ['rm', 'rimraf', 'trash', 'unlink', 'shred'];
const walkers = ['find', 'gfind', 'fd', 'fdfind'];
const filters = ['-name', '-iname', '-path', '-ipath', '-regex', '-iregex', '-wholename', '-iwholename'];
const unrestricted = ['--no-ignore', '--no-ignore-vcs', '--unrestricted'];
const printing = ['-print0', '--print0', '-0'];
const feeding: ReadonlySet<number> = new Set([RedirOperator.RdrIn, RedirOperator.RdrInOut, RedirOperator.DplIn, RedirOperator.Hdoc, RedirOperator.DashHdoc, RedirOperator.WordHdoc]);
const grepped = {
    drop: ['--recursive', '--dereference-recursive', '--extended-regexp', '--with-filename', '--exclude-dir'],
    kept: ['--line-number', '--ignore-case', '--files-with-matches', '--count', '--word-regexp', '--invert-match', '--only-matching', '--after-context', '--before-context', '--context', '--regexp', '--fixed-strings', '--no-filename', '--include'],
    rename: { '-h': '-I', '--include': '-g' },
    basic: /\\\\|\\(?<escaped>[(){}+?|])|(?<plain>^\*|[(){}+?|])/gu,
};
const respelled: Readonly<Record<string, readonly string[]>> = { '-type': ['-t'], '-maxdepth': ['-d'], '-mindepth': ['--min-depth'], '-name': ['-g'], '-iname': ['-i', '-g'] };
const flagged: Readonly<Record<string, string>> = { i: '--ignore-case', c: '--case-sensitive', w: '--word-regexp', s: '--multiline-dotall' };
const effects: Readonly<Record<string, RegExp>> = {
    awk: /\bprintf?\b[^;}\n]*\||\|\s*getline\b|\bsystem\s*\(/u,
    node: /\bchild_process\b|\b(?:unlink|rm|rmdir)(?:Sync)?\(/u,
    python: /\bsubprocess\.|\bshutil\.rmtree\(|\bos\.(?:system|popen|exec\w*|spawn\w*|remove|removedirs)\(|\.(?:unlink|rmdir)\(/u,
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const sources = (invocation: Invocation): readonly string[] => Option.getOrElse(texts(invocation.operands.filter(({ roles }) => roles.includes('read')).map(({ word }) => word)), Array.empty);
const forms: Readonly<Record<string, readonly Form[]>> = {
    awk: [
        [/^\s*NR\s*(?:==\s*(?<line>\d+)|>=\s*(?<first>\d+)\s*&&\s*NR\s*<=\s*(?<last>\d+))\s*(?:\{\s*print(?:\s+\$0)?\s*;?\s*\})?\s*$/u, ({ line, first, last }, invocation) => Option.liftPredicate(Array.make('sed', '-n', `${line ?? `${first},${last}`}p`, ...sources(invocation)), () => invocation.flags.length === 0)],
        [/^\s*(?:1|\{\s*print(?:\s+\$0)?\s*;?\s*\})\s*$/u, (_, invocation) => Option.liftPredicate(Array.make('cat', ...sources(invocation)), () => invocation.flags.length === 0)],
        [
            /^\s*\{\s*print\s+\$1\s*;?\s*\}\s*$/u,
            (_, invocation) =>
                Option.map(
                    Option.filter(Option.flatMap(Array.head(invocation.values('-F')), said), (separator) => /^[^\s\\]$/u.test(separator) && invocation.flags.length === 1),
                    (separator) => Array.make('cut', `-d${separator}`, '-f1', ...sources(invocation)),
                ),
        ],
    ],
    node: [
        [/^(?:console\.log\()?require\(['"](?<name>[^'"./][^'"]*)\/package\.json['"]\)\.(?<field>\w+)\)?;?$/u, ({ name = '', field = '' }) => Option.some(Array.make('jq', '-r', `.${field}`, `node_modules/${name}/package.json`))],
        [/^(?:console\.log\()?require\.resolve\(['"](?<name>[^'"./][^'"]*)\/package\.json['"]\)\)?;?$/u, ({ name = '' }) => Option.some(Array.make('realpath', `node_modules/${name}/package.json`))],
        [/^(?:console\.log\()?require\(['"](?:node:)?path['"]\)\.dirname\(require\.resolve\(['"](?<name>[^'"./][^'"]*)\/package\.json['"]\)\)\)?;?$/u, ({ name = '' }) => Option.some(Array.make('realpath', `node_modules/${name}`))],
    ],
    python: [[/^\s*(?:import time\s*[;\n]\s*time|__import__\(['"]time['"]\))\.sleep\(\s*(?<seconds>\d+(?:\.\d+)?)\s*\)\s*;?\s*$/u, ({ seconds = '' }) => Option.some(Array.make('sleep', seconds))]],
    perl: [[/^\s*(?:select\(\s*undef\s*,\s*undef\s*,\s*undef\s*,\s*|sleep\(?\s*)(?<seconds>\d+(?:\.\d+)?)\s*\)?\s*;?\s*$/u, ({ seconds = '' }) => Option.some(Array.make('sleep', seconds))]],
    osascript: [[/^\s*delay\s+(?<seconds>\d+(?:\.\d+)?)\s*$/u, ({ seconds = '' }) => Option.some(Array.make('sleep', seconds))]],
};
const shown = (field: Pick<Field, 'pieces'>) => field.pieces.map((piece) => Match.valueTags(piece, { Text: ({ text }) => text, Wild: ({ text }) => text, Secret: () => '<secret>', Unknown: ({ origin }) => (origin._tag === 'Variable' ? `$${origin.name}` : '<expansion>') })).join('');
const covers = (folder: string, path: string) => {
    const base = folder.replace(/\/+$/u, '');
    return base === '' || path === base || path.startsWith(`${base}/`);
};
const literal = (word: Word) => (word.parts.every(({ type }) => type === 'Lit') ? word.parts.map((part) => (part.type === 'Lit' ? part.value : '')).join('') : '');
const sweeps = (walker: Invocation) => (is(walker, 'fd', 'fdfind') ? !Option.exists(operand(walker), (pattern) => lettered.test(pattern)) : !walker.has(...filters));
const reasons = (invocation: Invocation, walker: Option.Option<Invocation>, cwd: string): readonly Refusal[] => {
    const subject = command(invocation);
    const deep = invocation.has('-r', '-R', '--recursive') || is(invocation, 'rimraf', 'trash');
    const walking = is(invocation, ...walkers);
    const swept = Option.toArray(Option.filter(walker, sweeps)).flatMap((held) =>
        held.reads.filter(({ text }) => Option.exists(text, (root) => covers(root, cwd))).map((root) => ({ refuse: `${command(held)} ${shown(root)} runs ${subject} on everything under it.`, instruct: 'Add a pattern that selects only what to delete.' })),
    );
    const globs = Option.toArray(invocation.node)
        .flatMap(({ cmd }) => (cmd?.type === 'CallExpr' ? cmd.args.slice(1).map(literal) : []))
        .filter((text) => globbed.test(text) && !lettered.test(text.slice(text.lastIndexOf('/', text.search(globbed)) + 1)));
    const emptied = deep && is(invocation, ...removers) ? globs.filter((text) => covers(`${cwd}/${text.slice(0, text.lastIndexOf('/', text.search(globbed)) + 1)}`.replaceAll(/\/(?:\.\/)+/gu, '/'), cwd)) : [];
    const judged = (field: Field): readonly Refusal[] => {
        const origins = field.pieces.flatMap((piece) => (piece._tag === 'Unknown' ? [piece.origin._tag] : []));
        const folder = Option.getOrElse(field.text, () => field.prefix.slice(0, field.prefix.lastIndexOf('/'))) || '/';
        const unnamed = Option.isSome(field.text) || !lettered.test(shown(field).slice(field.prefix.lastIndexOf('/') + 1));
        return Match.value({ fed: origins.includes('Input'), later: origins.length > 0, scratch: unnamed && scratchpad.test(folder), covering: unnamed && covers(folder, cwd) }).pipe(
            Match.when({ fed: true }, () => swept),
            Match.when({ later: true }, () => (deep && covers(folder, cwd) ? [{ refuse: `${subject} ${shown(field)} deletes a path that expands only when it runs.`, instruct: 'Pass the path literally.' }] : [])),
            Match.when({ scratch: true }, () => [{ refuse: `${subject} ${folder} empties the scratchpad the session and every subagent share.`, instruct: 'Delete only the subfolder you made, by name.' }]),
            Match.when({ covering: true }, () => [
                walking ? { refuse: `${subject} ${folder} deletes everything under the working folder.`, instruct: 'Add a -name pattern that selects only what to delete.' } : { refuse: `${subject} ${folder} ${folder === cwd ? 'is' : 'holds'} the working folder.`, instruct: 'Name each path you made, in full.' },
            ]),
            Match.orElse(() => []),
        );
    };
    return walking && !sweeps(invocation) ? [] : [...emptied.map((text) => ({ refuse: `${subject} ${text} empties the working folder ${cwd}.`, instruct: 'Name each path you made, in full.' })), ...invocation.deletes.flatMap(judged)];
};
const guarded = (invocations: readonly Invocation[], walker: Option.Option<Invocation>, cwd: string): readonly Refusal[] =>
    invocations.flatMap((invocation) => {
        const within = is(invocation, ...walkers) ? Option.some(invocation) : walker;
        return [...reasons(invocation, walker, cwd), ...guarded(invocation.inner, within, cwd)];
    });
const active = (invocation: Invocation) => (slot: { readonly when: readonly string[]; readonly unless: readonly string[] }) => (slot.when.length === 0 || invocation.has(...slot.when)) && !invocation.has(...slot.unless);
const reading = (invocation: Invocation) => {
    const usage = Option.getOrUndefined(invocation.usage);
    const valued = invocation.flags.some(({ flag }) =>
        Option.exists(
            Option.flatMap(invocation.usage, (held) => held.value(flag)),
            ({ roles }) => roles.includes('read'),
        ),
    );
    const walking = usage !== undefined && Option.exists(usage.operands, (slots) => slots.some((slot) => slot.tree && active(invocation)(slot)));
    const operands = invocation.operands.some(({ word, roles }) => roles.includes('read') && !Option.contains(said(word), '-'));
    return usage !== undefined && !invocation.opaque && Option.exists(usage.input, (slot) => slot.roles.includes('read') && active(invocation)(slot)) && !walking && !operands && !valued;
};
const fed = (invocation: Invocation) => Option.exists(invocation.stage, ({ index }) => index > 0) || Option.isSome(invocation.repeated) || Option.exists(invocation.node, ({ redirs }) => redirs.some(({ op }) => feeding.has(op))) || invocation.within.some(({ _tag }) => _tag !== 'Substitution');
const translated = (pattern: string) => pattern.replaceAll(grepped.basic, (held: string, escaped?: string, plain?: string) => escaped ?? (plain === undefined ? held : `\\${plain}`));
const roots = (invocation: Invocation, cwd: string) => {
    const given = invocation.reads.flatMap(({ text }) => Option.toArray(text));
    return Array.isReadonlyArrayEmpty(given) ? [cwd] : given;
};
const ignoring = Effect.fnUntraced(function* (invocation: Invocation, call: Call) {
    const found = yield* nearest(call.cwd, '.git');
    const searched = roots(invocation, call.cwd);
    const root = Option.filter(found, (folder) => !invocation.has(...unrestricted) && searched.every((path) => covers(folder, path)));
    const paths = searched.filter((path) => Option.exists(root, (folder) => path !== folder));
    const checked =
        Option.isSome(root) && Array.isReadonlyArrayNonEmpty(paths)
            ? yield* ran(
                  ['git', 'check-ignore', '--no-index', '-q', '--', ...paths],
                  Option.getOrElse(root, () => call.cwd),
              )
            : { code: 1 };
    return { root, ignored: checked.code === 0 || paths.some((path) => modules.root.test(path)) };
});
const coded = (invocation: Invocation): readonly Written[] => [
    ...invocation.flags
        .filter(({ flag }) =>
            Option.exists(
                Option.flatMap(invocation.usage, (usage) => usage.value(flag)),
                ({ roles }) => roles.includes('code'),
            ),
        )
        .flatMap(({ values }) => values.slice(0, 1)),
    ...invocation.operands.filter(({ roles }) => roles.includes('code')).map(({ word }) => word),
];
const installed = Effect.fnUntraced(function* (script: Script, program: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return Option.isSome(yield* Effect.findFirst((script.dialect.environment['PATH'] ?? '').split(':'), (folder) => Effect.orElseSucceed(fs.exists(path.join(folder, program)), () => false)));
});
const replaced = (invocation: Invocation, words: Array.NonEmptyReadonlyArray<string>): readonly Verdict[] => Option.toArray(span(invocation)).map((whole) => ({ edit: (editor: Editor) => [editor.replace(whole, words)] }));

const files: Table = {
    'fd.hidden': { program: 'fd', aliases: ['fdfind'], ensure: ['--hidden'], unless: ['--no-hidden', '--unrestricted'] },
    'fd.batched': {
        program: 'fd',
        aliases: ['fdfind'],
        judge: (invocation) =>
            invocation.flags
                .filter(({ flag }) => flag.spellings.includes('--exec'))
                .flatMap(({ spelled, values }): readonly Verdict[] => {
                    const [program = '', ...rest] = values.map((value) => Option.getOrUndefined(said(value)) ?? '');
                    const placed = rest.filter((word) => placeholder.test(word));
                    const alike = batchable.has(program) && (placed.length === 0 || (placed.length === 1 && rest.at(-1) === '{}'));
                    const bounded = invocation.has('--max-results', '-1') && invocation.values('--threads').some((value) => Option.contains(said(value), '1'));
                    const batch = Option.exists(said(spelled), (text) => text.startsWith('--')) ? '--exec-batch' : '-X';
                    return Option.match(
                        Option.filter(spelled.node, () => alike),
                        {
                            onSome: (word) => [{ edit: (editor: Editor) => [editor.replace(word, Array.of(batch))] }],
                            onNone: () => (bounded ? [] : [{ refuse: `fd -x ${program} starts one process per search result.`, instruct: 'Pass -X for a command taking every path in one run, else --max-results with --threads=1.' }]),
                        },
                    );
                }),
    },
    'find.batched': {
        program: 'find',
        aliases: ['gfind'],
        judge: (invocation) =>
            invocation.flags
                .filter(({ flag }) => flag.spellings.includes('-exec') || flag.spellings.includes('-execdir'))
                .flatMap(({ spelled, values }): readonly Verdict[] => {
                    const ending = Option.filter(Array.get(invocation.words, invocation.words.indexOf(Option.getOrElse(Array.last(values), () => spelled)) + 1), (word) => Option.contains(said(word), ';'));
                    const [program = '', ...rest] = values.map((value) => Option.getOrUndefined(said(value)) ?? '');
                    const alike = batchable.has(program) && rest.filter((word) => placeholder.test(word)).length === 1 && rest.at(-1) === '{}';
                    const batched = Option.filter(
                        Option.flatMap(ending, ({ node }) => node),
                        () => alike,
                    );
                    return Match.value({ batched: Option.getOrUndefined(batched), ending: Option.isSome(ending) }).pipe(
                        Match.when({ batched: Match.defined }, ({ batched: word }) => [{ edit: (editor: Editor) => [editor.replace(word, Array.of('+'))] }]),
                        Match.when({ ending: true }, () => [{ refuse: `find -exec ${program} starts one process per result.`, instruct: 'End -exec with {} + for a command taking every path in one run, else run fd -x with --max-results and --threads=1.' }]),
                        Match.orElse(() => []),
                    );
                }),
    },
    'find.fd': {
        program: 'find',
        aliases: ['gfind'],
        judge: (invocation) => {
            const ignored = invocation.flags.map(({ flag, values }) => flag.spellings.includes('-path') && values.every((value) => matches(value, modules.path)));
            const pruned = invocation.flags.flatMap(({ flag }, at) => (flag.spellings.includes('-not') && ignored[at + 1] === true ? [at, at + 1] : []));
            const options = Option.all(invocation.flags.map(({ flag, values }, at) => (pruned.includes(at) ? Option.some([]) : Option.flatMap(Option.fromUndefinedOr(respelled[flag.spellings[0]]), (spelling) => Option.map(texts(values), (given) => [...spelling, ...given])))));
            const given = Option.filter(texts(invocation.operands.map(({ word }) => word)), (held) => held.every((root) => !/^[-(!]/u.test(root)));
            const alone = invocation.inner.length === 0 && !Option.exists(invocation.stage, ({ index, count }) => index < count - 1) && invocation.values('-name', '-iname').length <= 1;
            return Option.match(
                Option.filter(Option.all([options, given]), () => alone),
                {
                    onNone: () => [],
                    onSome: ([flags, held]) => replaced(invocation, Array.make('fd', '--hidden', '--no-ignore', ...(pruned.length > 0 ? ['-E', 'node_modules'] : []), ...flags.flat(), ...(invocation.has('-name', '-iname') ? [] : ['-g', '*']), ...held.filter((root) => root !== '.'))),
                },
            );
        },
    },
    'grep.rg': {
        program: 'grep',
        judge: (invocation) => {
            const translatable = invocation.flags.every(({ flag, spelled }) => Option.isSome(said(spelled)) && flag.spellings.some((spelling) => grepped.drop.includes(spelling) || grepped.kept.includes(spelling)));
            const extended = invocation.has('--extended-regexp', '--fixed-strings');
            const patterns = [...invocation.operands.filter(({ roles }) => !roles.includes('read')).map(({ word }) => word), ...invocation.values('--regexp')];
            const spelled = Option.all(patterns.map((written) => Option.map(said(written), (text) => ({ node: written.node, text, changed: !extended && translated(text) !== text }))));
            const kept = Option.filter(spelled, (all) => all.every(({ text, node, changed }) => !/\\[1-9]/u.test(text) && (!changed || Option.isSome(node))));
            const filtered = invocation.values('--exclude-dir').every((value) => Option.exists(said(value), (text) => modules.root.test(text)));
            const engine = Array.make('rg', ...(invocation.has('--exclude-dir') ? [] : ['--no-ignore']), '--hidden');
            return Option.match(
                Option.filter(Option.all([head(invocation), kept]), () => invocation.has('--recursive', '--dereference-recursive') && translatable && filtered),
                {
                    onNone: () => [],
                    onSome: ([word, held]) => [{ edit: (editor: Editor) => [editor.replace(word, engine), ...held.flatMap(({ node, text, changed }) => (changed ? Option.toArray(node).map((at) => editor.replace(at, Array.of(translated(text)))) : []))] }, { drop: grepped.drop }, { rename: grepped.rename }],
                },
            );
        },
    },
    'search.ignored': {
        program: 'fd',
        aliases: ['fdfind', 'rg'],
        judge: Effect.fnUntraced(function* (invocation, call) {
            const { root, ignored } = yield* ignoring(invocation, call);
            const excluded = invocation.flags.filter(({ flag, values }) => flag.spellings.some((spelling) => spelling === '--exclude' || spelling === '--glob' || spelling === '--iglob') && values.every((value) => matches(value, modules.only)));
            const removed = excluded.flatMap(({ spelled, values }) => {
                const value = Option.getOrUndefined(Option.flatMap(Array.last(values), ({ node }) => node));
                return Option.toArray(spelled.node).map((word) => (value === undefined || value === word ? word : ([word, value] as const)));
            });
            return Match.value({ ignored, inside: Option.isSome(root) && Array.isReadonlyArrayNonEmpty(removed) }).pipe(
                Match.when({ ignored: true }, () => [{ ensure: Array.of('--no-ignore') }]),
                Match.when({ inside: true }, () => [{ edit: (editor: Editor) => removed.map((target) => editor.remove(target)) }]),
                Match.orElse(() => []),
            );
        }),
    },
    'grep.ignored': {
        program: 'grep',
        judge: Effect.fnUntraced(function* (grep, call) {
            const invocations = yield* call.script.invocations;
            const search = Option.filter(staged(invocations, grep, -1), (held) => is(held, 'fd', 'fdfind', 'rg'));
            const only = grep.has('--invert-match') && grep.flags.length === 1 && Option.exists(operand(grep), (pattern) => modules.only.test(pattern));
            const skipped =
                Option.isSome(search) && only
                    ? yield* ignoring(
                          Option.getOrElse(search, () => grep),
                          call,
                      )
                    : { root: Option.none(), ignored: true };
            return Option.isSome(skipped.root) && !skipped.ignored ? Option.toArray(grep.node).map((stmt) => ({ edit: (editor: Editor) => [editor.remove(stmt)] })) : [];
        }),
    },
    'walk.wide': {
        script: true,
        judge: Effect.fnUntraced(function* (script) {
            const home = script.dialect.environment['HOME'] ?? '/';
            const walked = leaves(yield* script.invocations).flatMap((invocation) => {
                const depths = invocation.values('--max-depth', '-maxdepth').map((value) => Number(Option.getOrUndefined(said(value)) ?? 'NaN'));
                const bounded = !is(invocation, 'du') && Array.isReadonlyArrayNonEmpty(depths) && depths.every((depth) => depth <= 2);
                const trees = invocation.reads.filter(({ pieces }) => pieces.at(-1)?._tag === 'Wild' && shown({ pieces: pieces.slice(-1) }) === '/**').map(({ prefix }) => prefix.replace(/\/+$/u, '') || '/');
                return bounded ? [] : trees.filter((folder) => covers(folder, home)).map((folder) => `${command(invocation)} walks ${folder}, which holds the whole home folder.`);
            });
            return Array.dedupe(walked).map((refuse) => ({ refuse, instruct: 'Pass a project folder, or a depth of 2 at most.' }));
        }),
    },
    'removal.guarded': { script: true, judge: (script, { cwd }) => Effect.map(script.invocations, (invocations) => guarded(invocations, Option.none(), cwd)) },
    'removal.fed': {
        program: 'xargs',
        judge: Effect.fnUntraced(function* (xargs, { script }) {
            const invocations = yield* script.invocations;
            const removal = Option.filter(Array.head(xargs.inner), (inner) => is(inner, ...removers));
            const producer = Option.filter(staged(invocations, xargs, -1), (held) => is(held, ...walkers) && Option.exists(held.stage, ({ index }) => index === 0) && held.inner.length === 0);
            const plain = xargs.flags.every(({ flag }) => flag.spellings.some((spelling) => ['-0', '--null', '-r', '--no-run-if-empty'].includes(spelling))) && Option.exists(xargs.stage, ({ count }) => count === 2);
            const agreed = Option.exists(producer, (held) => held.has(...printing) === xargs.has('-0', '--null'));
            const run = Option.flatMap(removal, (inner) => texts(inner.words));
            const kept = Option.flatMap(producer, (held) => texts(held.words.filter((written) => !spells(written, ...printing))));
            const whole = Option.flatMap(producer, span);
            const exec = Option.map(run, (words) => (Option.exists(producer, (held) => is(held, 'fd', 'fdfind')) ? Array.make('-X', ...words) : Array.make('-exec', ...words, '{}', '+')));
            const deep = Option.exists(removal, (inner) => inner.has('-r', '-R', '--recursive'));
            return Option.match(
                Option.filter(Option.all([kept, exec, whole, xargs.node]), () => plain && agreed),
                {
                    onSome: ([words, batch, target, stmt]) => [{ edit: (editor: Editor) => [editor.replace(target, Array.appendAll(words, batch)), editor.remove(stmt)] }],
                    onNone: () => (deep ? [{ refuse: `xargs ${Option.getOrElse(Option.map(removal, command), () => 'rm')} -r deletes paths it reads from standard input, which the hook cannot judge.`, instruct: 'Run find <folder> -name <pattern> -exec rm -r {} +, or name each path to rm.' }] : []),
                },
            );
        }),
    },
    'stdin.empty': {
        script: true,
        judge: Effect.fnUntraced(function* (script) {
            const invocations = yield* script.invocations;
            const empty = invocations.filter((invocation) => reading(invocation) && !fed(invocation) && !(is(invocation, 'read') && invocation.has('-t')) && !(is(invocation, 'awk') && coded(invocation).every((word) => matches(word, /^\s*BEGIN\s*\{[\s\S]*\}\s*$/u))));
            return empty.map((invocation): Verdict =>
                Option.match(
                    Option.filter(invocation.node, () => invocations.length > 1 && Option.isNone(invocation.stage) && invocation.within.length === 0),
                    {
                        onNone: () => ({ refuse: `${command(invocation)} reads standard input, which this call leaves empty.`, instruct: 'Name the file it should read, or pipe or redirect its input.' }),
                        onSome: (stmt) => ({ edit: (editor: Editor) => [editor.remove(stmt)] }),
                    },
                ),
            );
        }),
    },
    'code.inline': {
        program: 'awk',
        aliases: ['gawk', 'mawk', 'python', 'python3', 'node', 'perl', 'ruby', 'osascript'],
        judge: (invocation) => {
            const language = Option.getOrElse(named(invocation), () => '')
                .replace(/^[gm]?awk$/u, 'awk')
                .replace(/^python3$/u, 'python');
            const code = Option.getOrElse(
                Option.map(Option.all(coded(invocation).map(said)), (pieces) => pieces.join('\n').trim()),
                () => '',
            );
            const shape = Array.findFirst(forms[language] ?? [], ([pattern, rewrite]) => Option.flatMap(Option.fromNullishOr(pattern.exec(code)), ({ groups = {} }) => rewrite(groups, invocation)));
            return Match.value({ shape: Option.getOrUndefined(shape), inPlace: (language === 'perl' || language === 'ruby') && invocation.has('-i'), json: /\bjson\.loads?\(|\bJSON\.parse\(/u.test(code), effect: effects[language]?.test(code) === true }).pipe(
                Match.when({ shape: Match.defined }, ({ shape: words }) => replaced(invocation, words)),
                Match.when({ inPlace: true }, () => [{ refuse: `${language} -i edits files in place where the hook cannot judge the change.`, instruct: 'Edit them with sd where one pattern covers every change, else with your file edit tool.' }]),
                Match.when({ json: true }, () => [{ refuse: `${language} inline code parses JSON.`, instruct: 'Read it with jq.' }]),
                Match.when({ effect: true }, () => [{ refuse: `${language} inline code runs commands or deletes files where the hook cannot judge them.`, instruct: 'Run each as its own shell command.' }]),
                Match.orElse(() => []),
            );
        },
    },
    'sed.suffix': {
        program: 'sed',
        judge: (invocation) =>
            invocation.flags
                .filter(({ flag, values }) => flag.spellings.includes('-i') && values.some((value) => Option.exists(said(value), (text) => text.startsWith('-') || /^s(?<mark>[^\w\s\\]).*\k<mark>.*\k<mark>[gpiI\d]*$/su.test(text))))
                .flatMap(({ spelled }) => Option.toArray(spelled.node))
                .map((word) => ({ edit: (editor: Editor) => [editor.after(word, Array.of(''))] })),
    },
    'ls.options': {
        program: 'ls',
        judge: (invocation) => {
            const first = Option.flatMap(Array.head(invocation.operands), ({ word }) => word.node);
            const late = Array.dedupeWith(
                invocation.flags.flatMap(({ spelled }) => Option.toArray(Option.filter(Option.all([spelled.node, said(spelled), first]), ([word, , operand_]) => word.start > operand_.start))),
                ([left], [right]) => left === right,
            );
            return Option.match(Option.all([first, Array.match(late, { onEmpty: Option.none, onNonEmpty: (held) => Option.some(Array.map(held, ([, text]) => text)) })]), {
                onNone: () => [],
                onSome: ([word, flags]) => [{ edit: (editor: Editor) => [editor.before(word, flags), ...late.map(([moved]) => editor.remove(moved))] }],
            });
        },
    },
    'cat.visible': { program: 'cat', rename: { '-A': '-vet' } },
    'curl.fail': { program: 'curl', ensure: ['--fail-with-body'], unless: ['--fail', '--write-out'] },
    'screencapture.clipboard': { program: 'screencapture', when: ['-c'], refuse: "replaces the owner's clipboard", instruct: 'Pass a file.' },
    'screencapture.interactive': { program: 'screencapture', drop: ['-i', '-W', '-U', '-P', '-M'] },
    'pnpm.patch': { program: 'pnpm', path: ['patch'], refuse: 'copies a package into the tree', instruct: 'Set the package option, a newer release, or the consumer tsconfig.' },
    'pnpm.patch-commit': { program: 'pnpm', path: ['patch-commit'], refuse: 'writes a patched package into the tree', instruct: 'Set the package option, a newer release, or the consumer tsconfig.' },
    'sd.dashed': {
        program: 'sd',
        judge: (invocation) =>
            invocation.flags
                .filter(({ flag }) => !Option.exists(invocation.usage, (usage) => Option.isSome(usage.flag(flag.spellings[0]))))
                .flatMap(({ spelled }) => Option.toArray(Option.all([spelled.node, said(spelled)])))
                .map(([word, text]): Verdict =>
                    Match.value({ multiline: text === '-U' || text === '--multiline', flag: /^--?[A-Za-z][\w-]*$/u.test(text) }).pipe(
                        Match.when({ multiline: true }, () => ({ edit: (editor: Editor) => [invocation.has('--across') ? editor.remove(word) : editor.replace(word, Array.of('-A'))] })),
                        Match.when({ flag: true }, () => ({ refuse: `sd ${text} is no sd option.`, instruct: 'Pass -- before a pattern that starts with -.' })),
                        Match.orElse(() => ({ edit: (editor: Editor) => [editor.before(word, Array.of('--'))] })),
                    ),
                ),
    },
    'sd.empty': { program: 'sd', judge: (invocation) => (Option.contains(operand(invocation), '') && !invocation.has('--preview') ? [{ ensure: Array.of('--preview') }] : []) },
    'sd.same': {
        program: 'sd',
        judge: (invocation) => {
            const [find, replace] = [operand(invocation), operand(invocation, 1)];
            const plain = invocation.has('--fixed-strings') || Option.exists(find, (text) => !/[\\.+*?()|[\]{}^$]/u.test(text));
            const same = Option.isSome(find) && Option.getOrUndefined(find) === Option.getOrUndefined(replace);
            return same && plain && !invocation.has('--flags', '--preview') && invocation.operands.length > 2 ? replaced(invocation, Array.of(':')) : [];
        },
    },
    'sd.across': {
        program: 'sd',
        judge: (invocation) => (Option.exists(operand(invocation), (find) => find.includes('\n') || (!invocation.has('--fixed-strings') && /(?<!\\)(?:\\\\)*\\n/u.test(find))) && !invocation.has('--across') ? [{ ensure: Array.of('--across') }] : []),
    },
    'sd.unmatched': {
        program: 'sd',
        judge: Effect.fnUntraced(function* (invocation, { script, cwd }) {
            const fs = yield* FileSystem.FileSystem;
            const letters = [...(Option.getOrUndefined(Option.flatMap(Array.head(invocation.values('--flags')), said)) ?? '')];
            const paths = Option.getOrElse(Option.all(invocation.reads.map(({ text }) => text)), Array.empty);
            const earlier = new Set(
                leaves(yield* script.invocations)
                    .filter((other) => other.start < invocation.start)
                    .flatMap(({ writes }) => writes.flatMap(({ text }) => Option.toArray(text))),
            );
            const present = yield* Effect.forEach(paths, (path) => Effect.orElseSucceed(fs.exists(path), () => false));
            const judged = Option.filter(operand(invocation), () => Array.isReadonlyArrayNonEmpty(paths) && present.every(Boolean) && !paths.some((path) => earlier.has(path)) && !invocation.has('--preview') && letters.every((letter) => Object.hasOwn(flagged, letter)));
            const pattern = Option.getOrElse(judged, () => '');
            const fixed = Option.isSome(judged) && invocation.has('--fixed-strings') && letters.length === 0;
            const contents = fixed ? yield* Effect.forEach(paths, (path) => Effect.orElseSucceed(fs.readFileString(path), () => pattern)) : [];
            const searched =
                Option.isSome(judged) && !fixed
                    ? yield* ran(
                          Array.make('rg', '--no-config', '--quiet', '--text', ...(invocation.has('--fixed-strings') ? ['--fixed-strings'] : []), ...(invocation.has('--across') ? ['--multiline'] : []), ...letters.flatMap((letter) => Array.fromNullishOr(flagged[letter])), '--regexp', pattern, '--', ...paths),
                          cwd,
                      )
                    : { code: 0 };
            const missing = fixed ? !contents.some((text) => text.includes(pattern)) : searched.code === 1;
            return missing ? [{ refuse: `sd pattern matches nothing in ${paths.join(' ')}.`, instruct: 'Read the current text, then match what it holds.' }] : [];
        }),
    },
    'jq.file': {
        program: 'jq',
        judge: Effect.fnUntraced(function* (jq, { script }) {
            const cat = Option.filter(staged(yield* script.invocations, jq, -1), (held) => is(held, 'cat') && held.flags.length === 0 && held.operands.length === 1 && Option.exists(held.stage, ({ index, count }) => index === 0 && count === 2) && Option.exists(held.node, ({ redirs }) => redirs.length === 0));
            const reads = jq.operands.some(({ roles }) => roles.includes('read')) || jq.rest.length > 0 || jq.within.length > 0;
            const moved = Option.all([Option.flatMap(cat, (held) => operand(held)), Option.flatMap(Array.last(jq.words), ({ node }) => node), Option.flatMap(cat, ({ node }) => node)]);
            return Option.match(
                Option.filter(moved, () => !reads),
                { onNone: () => [], onSome: ([file, last, stmt]) => [{ edit: (editor: Editor) => [editor.remove(stmt), editor.after(last, Array.of(file))] }] },
            );
        }),
    },
    'duckdb.stdin': {
        program: 'duckdb',
        judge: (invocation) => (invocation.words.some((written) => Option.exists(said(written), (text) => text.includes('/dev/stdin'))) && !invocation.has('-markdown', '-ascii', '-box', '-column', '-csv', '-html', '-json', '-jsonlines', '-line', '-list', '-quote', '-table') ? [{ ensure: Array.of('-markdown') }] : []),
    },
    'hyperfine.shell': { program: 'hyperfine', judge: (invocation) => (invocation.operands.some(({ word }) => Option.exists(said(word), (text) => syntax.test(text))) && !invocation.has('-S', '-N', '--shell') ? [{ ensure: Array.of('-S') }] : []) },
    'hyperfine.input': {
        program: 'hyperfine',
        judge: (invocation) => (invocation.values('--input').some(({ node }) => Option.exists(node, ({ parts }) => parts.some(({ type }) => type === 'ProcSubst'))) ? [{ refuse: 'hyperfine --input <(…) is read by the first run alone.', instruct: 'Write the input to a file, then pass that file to --input.' }] : []),
    },
    'nproc.absent': { program: 'nproc', judge: (invocation, { script }) => Effect.map(installed(script, 'nproc'), (present) => (present ? [] : replaced(invocation, Array.make('getconf', '_NPROCESSORS_ONLN')))) },
    'shuf.absent': {
        program: 'shuf',
        judge: Effect.fnUntraced(function* (invocation, { script }) {
            const present = yield* installed(script, 'shuf');
            const lines = Option.flatMap(Array.head(invocation.values('-n')), said);
            const held = Option.getOrElse(texts(invocation.operands.map(({ word }) => word)), (): readonly string[] => []);
            const plain = invocation.flags.every(({ flag }) => flag.spellings.includes('-n'));
            return Match.value({ present: present || !plain, lines: Option.getOrUndefined(lines) }).pipe(
                Match.when({ present: true }, () => []),
                Match.when({ lines: Match.string }, ({ lines: count }) => [{ refuse: 'shuf is not installed.', instruct: `Run sort -R ${held.join(' ')} | head -n ${count}.` }]),
                Match.orElse(() => replaced(invocation, Array.make('sort', '-R', ...held))),
            );
        }),
    },
    'temporary.system': {
        script: true,
        judge: Effect.fnUntraced(function* (script) {
            const all = leaves(yield* script.invocations);
            const folder = Option.map(Option.fromUndefinedOr(script.dialect.environment['TMPDIR']), (given) => given.replace(/\/+$/u, ''));
            const written = all.flatMap(({ writes }) => writes.flatMap(({ text }) => Option.toArray(text))).filter((text) => temporary.test(text));
            const redirected = all.flatMap(({ node }) => Option.toArray(node)).flatMap(({ redirs }) => redirs.flatMap(({ word }) => Array.fromNullishOr(word)));
            const spoken = all.flatMap(({ words }) => words.flatMap((word) => Option.toArray(Option.all([word.node, said(word)]))));
            const targets = redirected.flatMap((word) => {
                const [expansion] = script.expand(word);
                return expansion._tag === 'Expanded' ? Option.toArray(Option.map(expansion.text, (text) => [word, text] as const)) : [];
            });
            const moved = Array.dedupeWith([...spoken, ...targets], ([left], [right]) => left === right).filter(([, text]) => temporary.test(text) && written.some((path) => covers(path, text) || covers(text, path)));
            return Option.match(
                Option.filter(folder, () => Array.isReadonlyArrayNonEmpty(moved)),
                {
                    onNone: () => [],
                    onSome: (tmp) => [
                        { edit: (editor: Editor) => moved.map(([word, text]) => editor.replace(word, Array.of(text.replace(temporary, tmp)))) },
                        ...Array.dedupe(moved.map(([, text]) => text)).map((text) => ({ note: `${text} became ${text.replace(temporary, tmp)}, as /tmp is shared by every user; later calls use that path.`, once: 'call' as const })),
                    ],
                },
            );
        }),
    },
} satisfies Table;

// --- [EXPORTS] -------------------------------------------------------------------------

export { files };
