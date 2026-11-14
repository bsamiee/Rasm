import { Array, Effect, Function, Match, Option, Predicate, Record, String } from 'effect';
import { absent, type Call, commandOf, cut, deleted, edited, ensure, erased, feedsJq, folderOf, holds, inserted, perSubcommand, quote, type Rule, type Ruleset, redirected, refusal, refuse, respell, silent, spelled, stages, unresolved, when, whole } from '#/rules/rule';
import type { Invocation } from '#/shell/invocation';
import type { Word } from '#/shell/script';

// --- [CONSTANTS] -----------------------------------------------------------------------

const IGNORED = /(?:^|\/)node_modules(?:\/|$)/u;
const ONLY_IGNORED = /^(?:\.?\/|\*\*\/)?node_modules(?:\/(?:\*\*?)?)?$/u;
const IGNORED_PATH = /^[.*/]*node_modules[*/]*$/u;
const NEGATED_IGNORED = new RegExp(`^!${ONLY_IGNORED.source.slice(1)}`, 'u');
const BASIC_REGEX_SWAPS = /\\\\|\\(?<escaped>[(){}+?|])|(?<unescaped>^\*|[(){}+?|])/gu;
const BACK_REFERENCE = /\\[1-9]/u;
const NO_IGNORE = ['--no-ignore', '--no-ignore-vcs', '--unrestricted'];
const GLOB_START = /[*?[{(]/u;
const NAME_CHARACTER = /[\p{L}\p{N}]/u;
const ASCENDS = /(?:^|\/)\.{1,2}(?:\/|$)/u;
const EXPANSION = /[$`<]/u;
const FLAG_SHAPED = /^--?[A-Za-z][\w-]*$/u;
const ESCAPED_NEWLINE = /(?<!\\)(?:\\\\)*\\n/u;
const SHELL_SYNTAX = /[$|&;<>()`]|^\w+=/u;
const NEXT_COMMAND = /^(?:\s*(?:;|&&)\s*|\s*\n\s*)/u;
const SUBSTITUTION = /^s(?<delimiter>[^\w\s\\])(?:\\.|(?!\k<delimiter>).)*\k<delimiter>(?:\\.|(?!\k<delimiter>).)*\k<delimiter>[gpiI\d]*$/su;
const CURL_DOWNLOAD: ReadonlySet<string> = new Set(['--fail', '--location', '--show-error', '--silent']);
const CURL_OUTPUT: ReadonlySet<string> = new Set(['--output']);
const WEB_SCHEME = /^https?:\/\//u;
const URL_TEXT = /^(?:(?!%2e)[\w.~:/?#@!$&()*+,;=%-])*$/iu;
const JSON_PARSE = /\bjson\.loads?\(|\bJSON\.parse\(/u;
const INLINE_LOOKUPS = [
    [/^(?:console\.log\()?require\(['"](?<name>[^'"./][^'"]*)\/package\.json['"]\)\.(?<field>\w+)\)?;?$/u, 'jq -r .$<field> node_modules/$<name>/package.json'],
    [/^(?:console\.log\()?require\.resolve\(['"](?<name>[^'"./][^'"]*)\/package\.json['"](?:,\s*\{\s*paths:\s*\[\s*process\.cwd\(\)\s*\]\s*\})?\)\)?;?$/u, 'realpath node_modules/$<name>/package.json'],
    [/^(?:console\.log\()?require\(['"](?:node:)?path['"]\)\.dirname\(require\.resolve\(['"](?<name>[^'"./][^'"]*)\/package\.json['"](?:,\s*\{\s*paths:\s*\[\s*process\.cwd\(\)\s*\]\s*\})?\)\)\)?;?$/u, 'realpath node_modules/$<name>'],
] as const;
const GREP_AS_RG: { readonly drops: readonly string[]; readonly renames: Record.ReadonlyRecord<string, string> } = {
    drops: ['--recursive', '--dereference-recursive', '--extended-regexp', '--with-filename', '--exclude-dir'],
    renames: {
        '--line-number': '-n',
        '--ignore-case': '-i',
        '--files-with-matches': '-l',
        '--count': '-c',
        '--word-regexp': '-w',
        '--invert-match': '-v',
        '--only-matching': '-o',
        '--after-context': '-A',
        '--before-context': '-B',
        '--context': '-C',
        '--regexp': '-e',
        '--fixed-strings': '-F',
        '--no-filename': '-I',
        '--include': '-g',
    },
};
const FIND_AS_FD: Record.ReadonlyRecord<string, string> = { '-type': '-t', '-maxdepth': '-d', '-mindepth': '--min-depth', '-name': '-g', '-iname': '-i -g' };

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [TREE_WALKS]

const rootFolders = (invocation: Invocation, { host }: Call): readonly string[] => {
    const start = folderOf(host, invocation, ...(invocation.row.recursive?.base ?? []));
    return Array.getSomes(invocation.roots.filter(({ known }) => known).map(({ text }) => host.resolve(text, start)));
};

const wideRoot: Rule = (invocation, call) => {
    const { host } = call;
    const stats = invocation.row.recursive?.stats === true;
    const depth = invocation.values('--max-depth', '-maxdepth', '-L');
    return (depth.length > 0 && depth.every((levels) => levels === '1') && !stats ? [] : rootFolders(invocation, call))
        .filter((folder) => host.path.dirname(folder) === '/' || Option.exists(host.home, (home) => host.inside(home, folder)) || (host.root !== folder && host.inside(host.root, folder)) || (!stats && folder.endsWith('/node_modules/.pnpm')))
        .flatMap((folder) => refusal(`${invocation.program} root ${folder} too wide, pass a package folder`));
};

const unignored =
    (named: Predicate.Predicate<Invocation>): Rule =>
    (invocation, call) =>
        Effect.map(call.host.ignored(rootFolders(invocation, call)), (ignored) => (ignored || named(invocation) ? ensure(['--no-ignore'], ...NO_IGNORE)(invocation) : []));

// --- [DELETIONS]

const removes =
    (...base: readonly string[]): Rule =>
    (invocation, call) => {
        const { host } = call;
        const subject = commandOf(invocation);
        const feeder = invocation.outer.find(({ row }) => row.wraps !== undefined && row.reads === true);
        const { walker, words, start, patterns } = deleted(invocation, call, ...base);
        return feeder === undefined
            ? words.flatMap(({ raw, text, known, glob }) => {
                  const wild = glob ? text.search(GLOB_START) : -1;
                  const fixed = wild < 0 ? text.length : text.lastIndexOf('/', wild) + 1;
                  const prefix = text.slice(0, text.lastIndexOf('/', text.search(EXPANSION)) + 1);
                  const folder = host.resolve(text.slice(0, fixed), start);
                  return Match.value({
                      known,
                      walks: walker !== undefined,
                      skipped: invocation.command.origin === 'file' || (prefix !== '' && Option.exists(host.resolve(prefix, start), (outside) => !(host.inside(outside, host.root) || host.inside(host.root, outside)))),
                      placed: Option.isSome(folder),
                      covers: Option.exists(folder, (target) => host.inside(host.root, target)) && ![text.slice(fixed), ...patterns].some((pattern) => NAME_CHARACTER.test(pattern)),
                      vague: ASCENDS.test(text) || !NAME_CHARACTER.test(text),
                  }).pipe(
                      Match.when({ known: false, skipped: false }, () => refusal(`${subject} ${raw} expands later, pass a literal path`)),
                      Match.when({ known: true, covers: true, walks: false }, () => refusal(`${subject} ${text} covers root, name each folder`)),
                      Match.when({ known: true, covers: true }, () => refusal(`${subject} deletes everything under ${text}, add a name pattern`)),
                      Match.when({ known: true, placed: false, vague: true }, () => unresolved(invocation)),
                      Match.orElse(() => []),
                  );
              })
            : refusal(`${subject} reads ${feeder.program} paths, use fd -X ${invocation.program}`);
    };

// --- [STDIN]

const withoutCommand = (invocation: Invocation) => {
    const { command, outer } = invocation;
    const after = new TextDecoder().decode(command.source.subarray(command.end));
    const skipped = NEXT_COMMAND.exec(after)?.[0].length ?? 0;
    const span = { start: 0, end: command.end + skipped };
    if (command.start > 0 || outer.length > 0 || (skipped === 0 && after.trim() !== '')) {
        return [];
    }
    return after.slice(skipped).trim() === '' ? edited(invocation, span, ':') : erased(invocation, span);
};

const emptyStdin: Rule = (invocation, { request }) => {
    const { command } = invocation;
    if (command.fed || invocation.outer.some(({ row }) => row.execs !== undefined) || !invocation.readsStdin) {
        return [];
    }
    const discarded = (request.kind === 'foreground' || request.kind === 'background') && command.redirects.some(({ op, descriptor, target }) => op.startsWith('>') && !Option.contains(descriptor, '2') && Option.exists(target, ({ text }) => text === '/dev/null'));
    if (!discarded) {
        return refusal(`${invocation.program} reads empty stdin, pass a file or pipe`);
    }
    const without = withoutCommand(invocation);
    return without.length > 0 ? without : edited(invocation, command, ':');
};

const inlineCode: Rule = (invocation) => {
    const { code } = invocation.row;
    if (code === undefined || !(invocation.has(...code) || (invocation.command.fed && invocation.readsStdin))) {
        return [];
    }
    const source = invocation.values(...code).join('\n');
    const [lookup] = INLINE_LOOKUPS.filter(([pattern]) => pattern.test(source)).map(([pattern, answer]) => source.replace(pattern, answer));
    if (lookup !== undefined) {
        return silent(whole(invocation, lookup, `${invocation.program} inline code`));
    }
    return refusal((invocation.has(...code) ? [source] : Option.toArray(invocation.command.input)).some((text) => JSON_PARSE.test(text)) ? `${invocation.program} parses JSON inline, use jq` : `${invocation.program} runs inline code, use Edit or a tool`);
};

// --- [REPLACEMENTS]

const dashedOperand: Rule = (invocation) => {
    const dashed = Option.isNone(invocation.separator) ? invocation.operands.find(({ text }) => text.startsWith('-')) : undefined;
    if (dashed === undefined) {
        return [];
    }
    return dashed === invocation.operands[0] && FLAG_SHAPED.test(dashed.text) ? refusal(`sd ${dashed.text} parses as option, add -- before it`) : inserted(invocation, dashed.start, '-- ');
};

const sameTexts: Rule = (invocation) => {
    const [find, replaced] = invocation.leading;
    return find !== undefined && find.text === replaced?.text && invocation.inputs.length > 0 && !invocation.has('--preview') ? edited(invocation, invocation.command, ':', 'sd replacing text with itself') : [];
};

// --- [REWRITES]

const grepsAsRg: Predicate.Predicate<Invocation> = (invocation) => {
    const patterns = [...invocation.leading, ...invocation.supplied('--regexp')];
    return (
        invocation.program === 'grep' &&
        !patterns.some(({ text }) => BACK_REFERENCE.test(text)) &&
        invocation.has('--recursive', '--dereference-recursive') &&
        [...invocation.names].every((name) => Record.has(GREP_AS_RG.renames, name) || GREP_AS_RG.drops.includes(name)) &&
        invocation.values('--exclude-dir').every((value) => IGNORED.test(value)) &&
        invocation.args.every(({ names, words: [word] }) => names.length === 0 || word.known) &&
        (invocation.has('--extended-regexp', '--fixed-strings') || (patterns.every(({ known }) => known) && invocation.values('--regexp').length === invocation.supplied('--regexp').length))
    );
};

const grepArguments: Rule = (invocation) => {
    const pattern = (word: Word): string => {
        const extended = invocation.has('--extended-regexp', '--fixed-strings') ? word.text : word.text.replace(BASIC_REGEX_SWAPS, (held: string, escaped?: string, unescaped?: string) => escaped ?? (unescaped === undefined ? held : `\\${unescaped}`));
        return extended === word.text ? word.raw : quote(extended);
    };
    return [
        ...respell({ ...GREP_AS_RG, values: (word, names) => (names.includes('--regexp') ? pattern(word) : word.raw) })(invocation),
        ...invocation.leading.flatMap((word) => {
            const raw = pattern(word);
            return raw === word.raw ? [] : edited(invocation, word, raw);
        }),
    ];
};

const findsAsFd: Predicate.Predicate<Invocation> = ({ row, last, values, args, operands }) => {
    const { until } = row.recursive ?? {};
    const excludesIgnored = args.every(({ names, values: held }, at) => (!names.includes('-not') || args[at + 1]?.names.includes('-path') === true) && (!names.includes('-path') || (args[at - 1]?.names.includes('-not') === true && held.every(({ text }) => IGNORED_PATH.test(text)))));
    return until !== undefined && last && values('-name', '-iname').length <= 1 && excludesIgnored && args.every(({ names }) => names.every((name) => Record.has(FIND_AS_FD, name) || name === '-not' || name === '-path')) && !operands.some(({ text }) => until.some((prefix) => text.startsWith(prefix)));
};

const skipsIgnored = (invocation: Invocation, call: Call) =>
    (['rg', 'fd', 'fdfind'].includes(invocation.program) || grepsAsRg(invocation) || findsAsFd(invocation)) && !invocation.has(...NO_IGNORE) && invocation.roots.every(({ known, text }) => known && Option.exists(call.host.resolve(text, invocation.command.cwd), (folder) => call.host.inside(folder, call.host.root)))
        ? Effect.map(call.host.ignored(rootFolders(invocation, call)), (ignored) => !ignored)
        : Effect.succeed(false);

const unfiltered =
    (drops: readonly string[], only: RegExp): Rule =>
    (invocation, call) => {
        const edits = respell({ drops, only })(invocation);
        return edits.length === 0 ? [] : Effect.map(skipsIgnored(invocation, call), (skips) => (skips ? edits : []));
    };

const excludedStage: Rule = (invocation, call) => {
    const { earlier } = stages(invocation, call);
    const previous = earlier.at(-1)?.command;
    return previous === undefined || invocation.names.size > 1 || !Option.exists(Array.head(invocation.leading), ({ text }) => ONLY_IGNORED.test(text))
        ? []
        : Effect.map(
              Effect.forEach(
                  earlier.filter(({ command }) => command === previous),
                  (stage) => skipsIgnored(stage, call),
              ),
              (skipping) => (skipping.some(Boolean) ? erased(invocation, { start: previous.end, end: invocation.command.end }) : []),
          );
};

const findAsFd: Rule = Effect.fnUntraced(function* (invocation, call) {
    const options = Array.getSomes(
        invocation.args.map(({ names: [name], values }) =>
            Option.fromUndefinedOr(name).pipe(
                Option.flatMap((flag) => Record.get(FIND_AS_FD, flag)),
                Option.map((flag) => `${flag} ${spelled(values)}`),
            ),
        ),
    );
    const roots = invocation.roots.map(({ raw }) => raw).filter((raw) => raw !== '.');
    if (invocation.has('-path') && !(yield* skipsIgnored(invocation, call))) {
        return [];
    }
    return whole(invocation, ['fd --hidden', ...((yield* call.host.ignored(rootFolders(invocation, call))) ? ['--no-ignore'] : []), ...options, ...(invocation.has('-name', '-iname') ? [] : ["-g '*'"]), ...roots].join(' '), 'find');
});

const grepAsRg: Rule = (invocation, call) => Effect.map(call.host.ignored(rootFolders(invocation, call)), (ignored) => edited(invocation, invocation.head, ignored ? 'rg --no-ignore' : 'rg', invocation.head.raw));

const curlAsXh: Rule = (invocation, { host }) => {
    const [url, ...urls] = invocation.operands;
    const [output, ...outputs] = invocation.args.filter(({ names }) => names.includes('--output'));
    const [file] = output?.values ?? [];
    const merged = [...redirected(invocation, '>&', '2'), ...redirected(invocation, '&>'), ...redirected(invocation, '&>>')];
    const downloads =
        invocation.command.origin === 'call' &&
        invocation.assigned.length === 0 &&
        merged.length === 0 &&
        CURL_DOWNLOAD.isSubsetOf(invocation.names) &&
        invocation.names.difference(CURL_DOWNLOAD).isSubsetOf(CURL_OUTPUT) &&
        outputs.length === 0 &&
        (output === undefined || (output.words.length === 2 && file?.text !== '-')) &&
        url !== undefined &&
        urls.length === 0 &&
        WEB_SCHEME.test(url.text) &&
        URL_TEXT.test(
            url.parts
                .filter(({ known }) => known)
                .map(({ text }) => text)
                .join(''),
        );
    const target = [...(output?.values ?? []).flatMap(({ raw }) => ['-o', raw]), ...invocation.operands.map(({ raw }) => raw)];
    return downloads ? Effect.map(host.onPath('xh'), (found) => (found ? whole(invocation, ['xh -Iqd --ignore-netrc --timeout=0 --max-redirects=50', ...target].join(' '), 'curl') : [])) : [];
};

const substitutesInPlace: Predicate.Predicate<Invocation> = ({ has, values, leading }) => {
    const scripts = has('-e') ? values('-e') : leading.map(({ text }) => text);
    return has('-i') && values('-i').every(String.isEmpty) && scripts.length > 0 && scripts.every((script) => SUBSTITUTION.test(script));
};

const treeOnly: Predicate.Predicate<Invocation> = ({ has, names }) => has('-R') && names.size === 1;

const optionsFirst: Rule = (invocation) => {
    const [first] = invocation.operands;
    const late = first === undefined ? [] : invocation.args.filter(({ names, words: [word] }) => names.length > 0 && word.start > first.start);
    return first === undefined || late.length === 0 ? [] : [...inserted(invocation, first.start, `${late.map(({ words }) => spelled(words)).join(' ')} `), ...late.flatMap(({ words }) => cut(invocation, words))];
};

const silenced: Rule = (invocation) => {
    const quieted = [...redirected(invocation, '&>'), ...redirected(invocation, '>', undefined, '/dev/null'), ...redirected(invocation, '>&', '2')];
    return quieted.length >= 2 || quieted.some(({ op }) => op === '&>') ? [...ensure(['--style', 'none'])(invocation), ...quieted.flatMap((redirect) => cut(invocation, [redirect]))] : [];
};

const mergedIntoJson: Rule = (invocation, call) => {
    const merged = redirected(invocation, '>&', '2', '1');
    return merged.length > 0 && feedsJq(invocation, call) ? merged.flatMap((redirect) => cut(invocation, [redirect])) : [];
};

const redundantCd: Rule = (invocation, { host }) => {
    const [target] = invocation.operands;
    const { cwd } = invocation.command;
    return target?.known === true && Option.exists(host.resolve(target.text, cwd), (folder) => Option.contains(cwd, folder)) ? withoutCommand(invocation) : [];
};

const cdOutsideRoot: Rule = Effect.fnUntraced(function* (invocation, { host, analysis }) {
    const [target] = invocation.operands;
    const { command } = invocation;
    const outside = Option.filter((folder: string) => !host.inside(folder, host.root));
    const left = target?.known === true && invocation.outer.length === 0 && command.origin === 'call' ? outside(host.resolve(target.text, command.cwd)) : Option.none();
    const later = Array.dedupe(analysis.invocations.filter((other) => other.command.source === command.source && other.command.start > command.start && Option.isSome(outside(other.command.cwd))).map(({ program }) => program));
    const found = yield* Effect.forEach(Array.cartesian(Option.toArray(left), later), ([folder, program]) => Effect.all([host.onPath(program), Effect.map(host.output(['mise', '-C', folder, 'exec', '--', 'which', program]), String.isNonEmpty)]), { concurrency: 'unbounded' });
    return found.some(([here, away]) => here && !away) && !found.some(([here, away]) => away && !here) ? edited(invocation, invocation.head, 'builtin cd', invocation.head.raw) : [];
});

const readsItsOwnFile: Rule = (invocation, call) => {
    const [source, ...others] = stages(invocation, call).earlier;
    const [file] = source?.operands ?? [];
    return others.length === 0 &&
        source?.program === 'cat' &&
        source.names.size === 0 &&
        source.operands.length === 1 &&
        source.command.redirects.length === 0 &&
        file !== undefined &&
        (invocation.leading.length > 0 || invocation.has('--from-file')) &&
        invocation.inputs.length === 0 &&
        invocation.rest.length === 0 &&
        invocation.outer.length === 0 &&
        invocation.assigned.length === 0
        ? edited(invocation, { start: source.command.start, end: Array.lastNonEmpty(invocation.words).end }, `${spelled(invocation.words)} ${file.raw}`)
        : [];
};

const shellBodies: Rule = (invocation) => {
    const given = invocation.supplied('--shell');
    const needed = !invocation.has('-N') && invocation.operands.some(({ text }) => SHELL_SYNTAX.test(text));
    return [...given.filter(({ text }) => text === 'sh -c').flatMap((word) => edited(invocation, word, 'default')), ...(needed && given.length === 0 ? ensure(['-S', 'default'])(invocation) : [])];
};

const shuffled: Rule = (invocation) => {
    const [count] = invocation.values('-n');
    return invocation.names.size > (count === undefined ? 0 : 1) ? [] : silent(whole(invocation, ['sort -R', ...(invocation.operands.length > 0 ? [spelled(invocation.operands)] : []), ...(count === undefined ? [] : [`| head -n ${count}`])].join(' '), 'shuf'));
};

const inputFlagged: Rule = (invocation) => {
    const [file] = invocation.inputs;
    return file === undefined || file.text === '-' || invocation.has('-i') ? [] : inserted(invocation, file.start, '-i ');
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const FILES: Ruleset = {
    programs: {
        fd: {
            aliases: ['fdfind'],
            leading: 1,
            execs: ['--exec', '--exec-batch'],
            recursive: { roots: ['--search-path'], base: ['--base-directory'], names: ['--extension'] },
            rules: [ensure(['--hidden'], '--no-hidden', '--unrestricted'), unfiltered(['--exclude'], ONLY_IGNORED), unignored(holds(IGNORED))],
        },
        find: {
            aliases: ['gfind'],
            flags: ['-type=', '-maxdepth=', '-mindepth='],
            execs: ['-exec', '-execdir', '-ok', '-okdir'],
            recursive: { until: ['-', '(', '!'], names: ['-name', '-iname', '-path', '-ipath', '-wholename', '-iwholename', '-regex', '-iregex'] },
            rules: [when(({ has }) => has('-delete'), removes()), when(findsAsFd, findAsFd)],
        },
        rm: { aliases: ['rimraf', 'trash', 'unlink', 'shred'], rules: [removes()] },
        git: { rules: [perSubcommand({ rm: [when(({ has }) => !has('--cached'), removes('-C'))] })] },
        tree: { recursive: {} },
        du: { flags: ['-I='], recursive: { stats: true } },
        grep: {
            flags: ['--include-dir='],
            leading: 1,
            leadingUnless: ['--regexp', '--file'],
            reads: true,
            recursive: { when: ['--recursive', '--dereference-recursive'] },
            rules: [when(grepsAsRg, grepAsRg, grepArguments), when(({ has }) => has('--invert-match'), excludedStage)],
        },
        rg: {
            leading: 1,
            leadingUnless: ['--regexp', '--file', '--files', '--type-list', '--generate'],
            recursive: {},
            rules: [unfiltered(['--glob', '--iglob'], NEGATED_IGNORED), unignored(Function.constFalse)],
        },
        curl: { rules: [curlAsXh] },
        sed: { rules: [when(substitutesInPlace, refuse('substitutes in place', "use sd '<regex>' '<replacement>' <files>", '-i'))] },
        cat: { reads: true, rules: [respell({ renames: { '-A': '-vet' } })] },
        ls: { rules: [when(treeOnly, (invocation) => edited(invocation, invocation.head, 'tree'), respell({ drops: ['-R'] })), when(Predicate.not(treeOnly), optionsFirst)] },
        sd: {
            strict: true,
            leading: 2,
            reads: true,
            rules: [
                dashedOperand,
                when(({ leading: [find] }) => find?.text === '', refuse('empty pattern matches everywhere', 'preview with sd -p')),
                sameTexts,
                when(({ leading: [find], has }) => find !== undefined && (find.text.includes('\n') || (!has('--fixed-strings') && ESCAPED_NEWLINE.test(find.text))), ensure(['-A'])),
            ],
        },
        cd: { rules: [redundantCd, cdOutsideRoot] },
        jq: { flags: ['--arg=2', '--argjson=2', '--slurpfile=2', '--rawfile=2'], leading: 1, leadingUnless: ['--from-file'], restAfter: ['--args', '--jsonargs'], reads: true, readsUnless: ['--null-input'], rules: [readsItsOwnFile] },
        hyperfine: {
            flags: ['-P, --parameter-scan=3', '-L, --parameter-list=2'],
            rules: [ensure(['-i']), shellBodies, silenced, when(({ supplied }) => supplied('--input').some(({ raw }) => raw.startsWith('<(')), refuse('process substitution is read once', 'pass a file every run reads', '--input'))],
        },
        nproc: { rules: [absent((invocation) => whole(invocation, 'sysctl -n hw.ncpu'))] },
        shuf: { flags: ['-n='], rules: [absent(shuffled)] },
        base64: { flags: ['-i=', '-o=', '-b='], reads: true, readsUnless: ['-i'], rules: [inputFlagged] },
    },
    every: [mergedIntoJson, wideRoot, emptyStdin, inlineCode],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { FILES };
