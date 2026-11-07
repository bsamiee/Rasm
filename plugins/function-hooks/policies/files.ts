import { Array, Cache, Effect, Function, Option, Predicate } from 'effect';
import { absent, become, type Call, cut, dropped, edited, ensure, type Fix, feedsJq, folderOf, has, holds, inserted, type Policy, quote, redirected, refusal, refuse, respell, silent, spoken, stages, subject, unresolved, type Verdicts, when, whole } from '#/policies/fix';
import { lockDeletes } from '#/policies/ghidra';
import type { Word } from '#/shell/script';
import { Shell, type Site } from '#/shell/site';

// --- [CONSTANTS] -----------------------------------------------------------------------

const IGNORED = /(?:^|\/)node_modules(?:\/|$)/u;
const ONLY_IGNORED = /^(?:\.?\/|\*\*\/)?node_modules(?:\/(?:\*\*?)?)?$/u;
const IGNORED_PATH = /^[.*/]*node_modules[*/]*$/u;
const NEGATED_IGNORED = new RegExp(`^!${ONLY_IGNORED.source.slice(1)}`, 'u');
const BASIC_REGEX_SWAPS = /\\\\|\\(?<escaped>[(){}+?|])|(?<unescaped>^\*|[(){}+?|])/gu;
const BACK_REFERENCE = /\\[1-9]/u;
const GITIGNORE_READERS = new Set(['rg', 'fd', 'fdfind', 'tree']);
const NO_IGNORE = ['--no-ignore', '--no-ignore-vcs', '--unrestricted'];
const DEPTH = ['--max-depth', '-maxdepth', '-L', '--level'];
const LEADING_OPTION = /^-[A-Za-z]$/u;
const HIDDEN_EXCEPT_GIT = "--hidden --glob '!.git'";
const GLOB_START = /[*?[{(]/u;
const NAME_CHARACTER = /[\p{L}\p{N}]/u;
const ASCENDS = /(?:^|\/)\.{1,2}(?:\/|$)/u;
const EXPANSION = /[$`<]/u;
const FLAG_SHAPED = /^--?[A-Za-z][\w-]*$/u;
const ESCAPED_NEWLINE = /(?<!\\)(?:\\\\)*\\n/u;
const INLINE_LOOKUPS = [
    [/^(?:console\.log\()?require\(['"](?<name>[^'"./][^'"]*)\/package\.json['"]\)\.(?<field>\w+)\)?;?$/u, 'jq -r .$<field> node_modules/$<name>/package.json'],
    [/^(?:console\.log\()?require\.resolve\(['"](?<name>[^'"./][^'"]*)\/package\.json['"](?:,\s*\{\s*paths:\s*\[\s*process\.cwd\(\)\s*\]\s*\})?\)\)?;?$/u, 'realpath node_modules/$<name>/package.json'],
    [/^(?:console\.log\()?require\(['"](?:node:)?path['"]\)\.dirname\(require\.resolve\(['"](?<name>[^'"./][^'"]*)\/package\.json['"](?:,\s*\{\s*paths:\s*\[\s*process\.cwd\(\)\s*\]\s*\})?\)\)\)?;?$/u, 'realpath node_modules/$<name>'],
] as const;
const GREP_AS_RG: Readonly<Record<string, string>> = {
    '--recursive': '',
    '--dereference-recursive': '',
    '--extended-regexp': '',
    '--with-filename': '',
    '--exclude-dir': '',
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
};
const NEXT_STEP = /^(?:\s*(?:;|&&)\s*|\s*\n\s*)/u;
const SHELL_SYNTAX = /[$|&;<>()`]|^\w+=/u;
const FIND_AS_FD: Readonly<Record<string, string>> = { '-type': '-t', '-maxdepth': '-d', '-mindepth': '--min-depth', '-name': '-g', '-iname': '-i -g', '-not': '', '-path': '' };

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ROOTS]

const walkRoots = (site: Site): readonly Word[] => {
    const { walks } = site.row;
    if (walks === undefined || (walks.when !== undefined && !site.has(...walks.when))) {
        return [];
    }
    const { before, paths = [] } = walks;
    const positional =
        before === undefined
            ? site.inputs
            : Array.takeWhile(
                  Array.dropWhile(site.words.slice(1), ({ text }) => LEADING_OPTION.test(text)),
                  ({ text }) => !before.some((prefix) => text.startsWith(prefix)),
              );
    const places = [...site.supplied(...paths), ...positional];
    return places.length === 0 && !site.command.fed ? [{ ...site.head, raw: '.', text: '.', known: true, glob: false }] : places;
};

const rootPlaces = (site: Site, { host }: Call): readonly string[] => {
    const start = folderOf(host, site, ...(site.row.walks?.base ?? []));
    return Array.getSomes(
        walkRoots(site)
            .filter(({ known }) => known)
            .map(({ text }) => host.place(text, start)),
    );
};

const tooWide: Fix = (site, call) => {
    const { host } = call;
    const depth = site.values(...DEPTH);
    return (depth.length > 0 && depth.every((levels) => levels === '1') && site.row.walks?.stats !== true ? [] : rootPlaces(site, call))
        .filter((place) => host.path.dirname(place) === '/' || Option.exists(host.home, (home) => host.inside(home, place)) || (host.root !== place && host.inside(host.root, place)) || (site.row.walks?.stats !== true && place.endsWith('/node_modules/.pnpm')))
        .flatMap((place) => refusal(`${site.program} root ${place} too wide, pass a package folder`));
};

const rootIgnored = (site: Site, call: Call) => Effect.flatMap(Shell, ({ ignored }) => Cache.get(ignored, rootPlaces(site, call)));

const unignored =
    (named: Predicate.Predicate<Site>): Fix =>
    (site, call) =>
        Effect.map(rootIgnored(site, call), (held) => (held || named(site) ? ensure('--no-ignore', ...NO_IGNORE)(site) : []));

// --- [DELETES]

const deletedPlace = Effect.fnUntraced(function* (site: Site, walker: Site | undefined, word: Word, start: Option.Option<string>, place: string, globbed: string, { host }: Call) {
    const patterns = walker === undefined ? [] : [...walker.leading.map(({ text }) => text), ...walker.values(...(walker.row.walks?.names ?? []))];
    if (![globbed, ...patterns].some((pattern) => NAME_CHARACTER.test(pattern)) && host.inside(host.root, place)) {
        return refusal(walker === undefined ? `${subject(site)} ${word.text} covers root, run mise run clean` : `${subject(site)} deletes everything under ${word.text}, add a name pattern`);
    }
    const target = Option.getOrElse(host.place(word.text, start), () => place);
    return yield* lockDeletes(site, host, patterns.length === 0 ? [target] : patterns.map((pattern) => host.path.join(target, pattern)));
});

const prefixOutside = ({ text }: Word, start: Option.Option<string>, { host }: Call): boolean => {
    const folder = text.slice(0, text.lastIndexOf('/', text.search(EXPANSION)) + 1);
    return folder !== '' && Option.exists(host.place(folder, start), (place) => !(host.inside(place, host.root) || host.inside(host.root, place)));
};

const deletedWord = (site: Site, walker: Site | undefined, word: Word, call: Call) => {
    const start = walker === undefined ? site.command.cwd : folderOf(call.host, walker, ...(walker.row.walks?.base ?? []));
    const wild = word.glob ? word.text.search(GLOB_START) : -1;
    const fixed = wild < 0 ? word.text.length : word.text.lastIndexOf('/', wild) + 1;
    return word.known
        ? Option.match(call.host.place(word.text.slice(0, fixed), start), {
              onNone: () => Effect.succeed(ASCENDS.test(word.text) || !NAME_CHARACTER.test(word.text) ? unresolved(site) : []),
              onSome: (place) => deletedPlace(site, walker, word, start, place, word.text.slice(fixed), call),
          })
        : Effect.succeed(site.command.origin === 'file' || prefixOutside(word, start, call) ? [] : refusal(`${subject(site)} ${word.raw} expands later, pass a literal path`));
};

const removed: Fix = (site, call) => {
    const feeder = site.outer.find(({ row }) => row.reads === 'always');
    if (feeder !== undefined) {
        return refusal(`${subject(site)} reads ${feeder.program} paths, use fd -X ${site.program}`);
    }
    const walker = site.row.walks === undefined ? site.outer.findLast(({ row }) => row.walks !== undefined) : site;
    return Effect.map(
        Effect.forEach(walker === undefined ? site.inputs : walkRoots(walker), (word) => deletedWord(site, walker, word, call), { concurrency: 'unbounded' }),
        (verdicts) => verdicts.flat(),
    );
};

// --- [INPUT]

const waitingOnInput = (site: Site): boolean => {
    const { row, inputs, leading } = site;
    const needsNothing = leading.some(({ text }) => row.readsNothing?.test(text) === true);
    return row.reads !== undefined && !needsNothing && !site.has(...(row.readsUnless ?? []), ...(row.code ?? []), ...(row.shell ?? [])) && (row.reads === 'always' || (row.stops === true ? inputs.slice(0, 1) : inputs).some(({ text }) => text === '-') || (inputs.length === 0 && walkRoots(site).length === 0));
};

const struck = (site: Site): Verdicts => {
    const { command, outer } = site;
    const tail = new TextDecoder().decode(command.source.subarray(command.end));
    const skipped = NEXT_STEP.exec(tail)?.[0].length ?? 0;
    return command.start === 0 && outer.length === 0 && (skipped > 0 || tail.trim() === '') ? edited(site, { start: 0, end: command.end + skipped }, tail.slice(skipped).trim() === '' ? ':' : '') : [];
};

const unfed: Fix = (site, { request }) => {
    if (site.command.fed || site.outer.some(({ row }) => row.execs !== undefined) || !waitingOnInput(site)) {
        return [];
    }
    const discarded = (request.kind === 'foreground' || request.kind === 'background') && site.command.redirects.some(({ op, descriptor, target }) => op.startsWith('>') && !Option.contains(descriptor, '2') && Option.exists(target, ({ text }) => text === '/dev/null'));
    if (!discarded) {
        return refusal(`${site.program} reads empty stdin, pass a file or pipe`);
    }
    const removal = struck(site);
    return removal.length > 0 ? removal : edited(site, site.command, ':');
};

const inlineCode: Fix = (site) => {
    const flags = site.row.code ?? [];
    const coded = site.has(...flags) || (site.command.fed && waitingOnInput(site));
    if (site.row.code === undefined || !coded) {
        return [];
    }
    const code = site.values(...flags).join('\n');
    const [lookup] = INLINE_LOOKUPS.filter(([pattern]) => pattern.test(code)).map(([pattern, answer]) => code.replace(pattern, answer));
    return lookup === undefined ? refusal(`${site.program} runs inline code, use Edit or a tool`) : silent(whole(site, lookup, `${site.program} inline code`));
};

// --- [REPLACEMENTS]

const unseparated: Fix = (site) => {
    const dashed = Option.isNone(site.separator) ? site.operands.find(({ text }) => text.startsWith('-')) : undefined;
    if (dashed === undefined) {
        return [];
    }
    return dashed === site.operands[0] && FLAG_SHAPED.test(dashed.text) ? refusal(`sd ${dashed.text} parses as option, add -- before it`) : inserted(site, dashed.start, '-- ');
};

const sameTexts: Fix = (site) => {
    const [find, replaced] = site.leading;
    return find !== undefined && find.text === replaced?.text && site.inputs.length > 0 && !site.has('--preview') ? dropped(site, 'sd replacing text with itself') : [];
};

// --- [TRANSLATIONS]

const grepsAsRg: Predicate.Predicate<Site> = (site) => {
    const patterns = [...site.leading, ...site.supplied('--regexp')];
    return (
        site.program === 'grep' &&
        !patterns.some(({ text }) => BACK_REFERENCE.test(text)) &&
        site.has('--recursive', '--dereference-recursive') &&
        [...site.names].every((name) => GREP_AS_RG[name] !== undefined) &&
        site.values('--exclude-dir').every((value) => IGNORED.test(value)) &&
        site.items.every(({ names, words: [word] }) => names.length === 0 || word.known) &&
        (site.has('--extended-regexp', '--fixed-strings') || (patterns.every(({ known }) => known) && site.values('--regexp').length === site.supplied('--regexp').length))
    );
};

const swappedEscape = (held: string, escaped?: string, unescaped?: string): string => escaped ?? (unescaped === undefined ? held : `\\${unescaped}`);

const extendedPatterns: Fix = (site) =>
    site.has('--extended-regexp', '--fixed-strings')
        ? []
        : [...site.leading, ...site.supplied('--regexp')].flatMap((word) => {
              const extended = word.text.replace(BASIC_REGEX_SWAPS, swappedEscape);
              return extended === word.text ? [] : edited(site, word, quote(extended));
          });

const findsAsFd: Predicate.Predicate<Site> = ({ row, last, values, items, operands }) => {
    const { before } = row.walks ?? {};
    const excludesIgnored = items.every(({ names, values: held }, at) => (!names.includes('-not') || items[at + 1]?.names.includes('-path') === true) && (!names.includes('-path') || (items[at - 1]?.names.includes('-not') === true && held.every((value) => IGNORED_PATH.test(value)))));
    return before !== undefined && last && values('-name', '-iname').length <= 1 && excludesIgnored && items.every(({ names }) => names.every((name) => FIND_AS_FD[name] !== undefined)) && !operands.some(({ text }) => before.some((prefix) => text.startsWith(prefix)));
};

const skipsIgnored = (site: Site, call: Call) =>
    (GITIGNORE_READERS.has(site.program) || grepsAsRg(site) || findsAsFd(site)) && !site.has(...NO_IGNORE) && walkRoots(site).every(({ known, text }) => known && Option.exists(call.host.place(text, site.command.cwd), (place) => call.host.inside(place, call.host.root)))
        ? Effect.map(rootIgnored(site, call), (held) => !held)
        : Effect.succeed(false);

const unfiltered =
    (table: Readonly<Record<string, string>>, only: RegExp): Fix =>
    (site, call) => {
        const edits = respell(table, { only })(site);
        return edits.length === 0 ? [] : Effect.map(skipsIgnored(site, call), (skips) => (skips ? edits : []));
    };

const excludedStage: Fix = (site, call) => {
    const { earlier } = stages(site, call);
    const previous = earlier.at(-1)?.command;
    return previous === undefined || site.names.size > 1 || !ONLY_IGNORED.test(site.leading[0]?.text ?? '')
        ? []
        : Effect.map(
              Effect.forEach(
                  earlier.filter(({ command }) => command === previous),
                  (held) => skipsIgnored(held, call),
              ),
              (skipping) => (skipping.some(Boolean) ? edited(site, { start: previous.end, end: site.command.end }, '') : []),
          );
};

const findAsFd: Fix = Effect.fnUntraced(function* (site, call) {
    const options = site.items.filter(({ names: [name] }) => name !== undefined && FIND_AS_FD[name] !== '').map(({ names: [name = ''], words: [, ...values] }) => `${FIND_AS_FD[name] ?? name} ${spoken(values)}`);
    const roots = walkRoots(site)
        .map(({ raw }) => raw)
        .filter((raw) => raw !== '.');
    if (site.has('-path') && !(yield* skipsIgnored(site, call))) {
        return [];
    }
    return whole(site, ['fd --hidden', ...((yield* rootIgnored(site, call)) ? ['--no-ignore'] : []), ...options, ...(site.has('-name', '-iname') ? [] : ["-g '*'"]), ...roots].join(' '), 'find');
});

const grepAsRg: Fix = (site, call) => Effect.map(rootIgnored(site, call), (held) => become(`rg ${HIDDEN_EXCEPT_GIT}${held ? ' --no-ignore' : ''}`)(site));

const treeOnly: Predicate.Predicate<Site> = Predicate.and(has('-R'), ({ names }) => names.size === 1);

const optionsFirst: Fix = (site) => {
    const [first] = site.operands;
    const late = first === undefined ? [] : site.items.filter(({ names, words: [word] }) => names.length > 0 && word.start > first.start);
    return first === undefined || late.length === 0 ? [] : [...inserted(site, first.start, `${late.map(({ words }) => spoken(words)).join(' ')} `), ...late.flatMap(({ words }) => cut(site, words))];
};

const silenced: Fix = (site) => {
    const quieted = [...redirected(site, '&>'), ...redirected(site, '>', undefined, '/dev/null'), ...redirected(site, '>&', '2')];
    return quieted.length >= 2 || quieted.some(({ op }) => op === '&>') ? [...ensure('--style none')(site), ...quieted.flatMap((redirect) => cut(site, [redirect]))] : [];
};

const mergedIntoJson: Fix = (site, call) => {
    const merged = redirected(site, '>&', '2', '1');
    return merged.length > 0 && feedsJq(site, call) ? merged.flatMap((redirect) => cut(site, [redirect])) : [];
};

const redundantCd: Fix = (site, { host }) => {
    const [target] = site.operands;
    return target?.known === true && Option.exists(host.place(target.text, site.command.cwd), (place) => Option.contains(site.command.cwd, place)) ? struck(site) : [];
};

const readsItsOwnFile: Fix = (site, call) => {
    const [source, ...others] = stages(site, call).earlier;
    const [file] = source?.operands ?? [];
    return others.length === 0 &&
        source?.program === 'cat' &&
        source.names.size === 0 &&
        source.operands.length === 1 &&
        source.command.redirects.length === 0 &&
        file !== undefined &&
        (site.leading.length > 0 || site.has('--from-file')) &&
        site.inputs.length === 0 &&
        site.rest.length === 0 &&
        site.outer.length === 0 &&
        site.assigned.length === 0
        ? edited(site, { start: source.command.start, end: Array.lastNonEmpty(site.words).end }, `${spoken(site.words)} ${file.raw}`)
        : [];
};

const shellBodies: Fix = (site) => {
    const given = site.supplied('--shell');
    const needed = !site.has('-N') && site.operands.some(({ text }) => SHELL_SYNTAX.test(text));
    return [...given.filter(({ text }) => text === 'sh -c').flatMap((word) => edited(site, word, 'default')), ...(needed && given.length === 0 ? ensure('-S default')(site) : [])];
};

const shuffled: Fix = (site) => {
    const [count] = site.values('-n');
    return site.names.size > (count === undefined ? 0 : 1) ? [] : silent(whole(site, ['sort -R', ...(site.operands.length > 0 ? [spoken(site.operands)] : []), ...(count === undefined ? [] : [`| head -n ${count}`])].join(' '), 'shuf'));
};

const inputFlagged: Fix = (site) => {
    const [file] = site.inputs;
    return file === undefined || file.text === '-' || site.has('-i') ? [] : inserted(site, file.start, '-i ');
};

// --- [TABLES] --------------------------------------------------------------------------

const FILES: Policy = {
    programs: {
        fd: {
            aliases: ['fdfind'],
            leading: 1,
            execs: ['--exec', '--exec-batch'],
            walks: { paths: ['--search-path'], base: ['--base-directory'], names: ['--extension'] },
            fixes: [ensure('--hidden', '--no-hidden', '--unrestricted'), unfiltered({ '--exclude': '' }, ONLY_IGNORED), unignored(holds(IGNORED))],
        },
        find: {
            aliases: ['gfind'],
            flags: ['-type=', '-maxdepth=', '-mindepth='],
            execs: ['-exec', '-execdir', '-ok', '-okdir'],
            walks: { before: ['-', '(', '!'], names: ['-name', '-iname', '-path', '-ipath', '-wholename', '-iwholename', '-regex', '-iregex'] },
            fixes: [when(has('-delete'), removed), when(findsAsFd, findAsFd)],
        },
        rm: { aliases: ['rimraf', 'trash', 'unlink', 'shred'], fixes: [removed] },
        tree: { walks: {}, fixes: [respell({ '-a': '', '--all': '', '--noreport': '', '--dirsfirst': '--group-directories-first', '-d': '-D' }), unfiltered({ '-I': '' }, ONLY_IGNORED)] },
        du: { flags: ['-I='], walks: { stats: true } },
        grep: {
            flags: ['--include-dir='],
            leading: 1,
            leadingUnless: ['--regexp', '--file'],
            reads: 'default',
            walks: { when: ['--recursive', '--dereference-recursive'] },
            closes: ['--quiet', '--silent', '--max-count', '--files-with-matches'],
            fixes: [when(grepsAsRg, grepAsRg, respell(GREP_AS_RG), extendedPatterns), when(has('--invert-match'), excludedStage)],
        },
        rg: {
            leading: 1,
            leadingUnless: ['--regexp', '--file', '--files', '--type-list', '--generate'],
            walks: {},
            closes: ['--quiet', '--max-count', '--files-with-matches'],
            fixes: [when((site) => walkRoots(site).length > 0, ensure(HIDDEN_EXCEPT_GIT, '--no-hidden', '--unrestricted', '--no-config')), unfiltered({ '--glob': '', '--iglob': '' }, NEGATED_IGNORED), unignored(Function.constFalse)],
        },
        cat: { reads: 'default', fixes: [respell({ '-A': '-vet' })] },
        ls: { fixes: [when(treeOnly, become('tree', false), respell({ '-R': '' })), when(Predicate.not(treeOnly), optionsFirst)] },
        sd: {
            closed: true,
            leading: 2,
            reads: 'default',
            fixes: [
                unseparated,
                when(({ leading: [find] }) => find?.text === '', refuse('empty pattern matches everywhere', 'preview with sd -p')),
                sameTexts,
                when(({ leading: [find], has: holding }) => find !== undefined && (find.text.includes('\n') || (!holding('--fixed-strings') && ESCAPED_NEWLINE.test(find.text))), ensure('-A')),
            ],
        },
        cd: { fixes: [redundantCd] },
        jq: { flags: ['--arg=2', '--argjson=2', '--slurpfile=2', '--rawfile=2'], leading: 1, leadingUnless: ['--from-file'], ends: ['--args', '--jsonargs'], reads: 'default', readsUnless: ['--null-input'], fixes: [readsItsOwnFile] },
        hyperfine: {
            flags: ['-P, --parameter-scan=3', '-L, --parameter-list=2'],
            fixes: [ensure('-i'), shellBodies, silenced, when(({ supplied }) => supplied('--input').some(({ raw }) => raw.startsWith('<(')), refuse('FIFO feeds one run', 'pass a file', '--input'))],
        },
        stubtest: { fixes: [(site, { host }) => ensure(`--mypy-config-file ${host.path.join(host.root, '.config/stubtest.ini')}`)(site)] },
        nproc: { fixes: [absent((site) => whole(site, 'sysctl -n hw.ncpu'))] },
        shuf: { flags: ['-n='], fixes: [absent(shuffled)] },
        base64: { flags: ['-i=', '-o=', '-b='], reads: 'default', readsUnless: ['-i'], fixes: [inputFlagged] },
    },
    every: [mergedIntoJson, tooWide, unfed, inlineCode],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { FILES, removed };
