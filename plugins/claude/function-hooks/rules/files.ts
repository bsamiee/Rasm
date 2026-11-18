import { Array, Effect, Function, Match, Option, Predicate, Record, String } from 'effect';
import { RedirOperator } from 'sh-syntax/sync';
import { Verdict } from '#/rules/decision';
import { absent, type Call, commandOf, cut, deleted, edited, ensure, erased, folderOf, holds, inserted, listed, NUL, perSubcommand, quote, type Rule, type Ruleset, redirected, refusal, relocated, respell, silent, spelled, stages, unresolved, when, whole } from '#/rules/rule';
import { type Invocation, WRITES } from '#/shell/invocation';
import type { Word } from '#/shell/script';

// --- [CONSTANTS] -----------------------------------------------------------------------

const IGNORED = /(?:^|\/)node_modules(?:\/|$)/u;
const ONLY_IGNORED = /^(?:\.?\/|\*\*\/)?node_modules(?:\/(?:\*\*?)?)?$/u;
const IGNORED_PATH = /^[.*/]*node_modules[*/]*$/u;
const NEGATED_IGNORED = new RegExp(`^!${ONLY_IGNORED.source.slice(1)}`, 'u');
const BASIC_REGEX_SWAPS = /\\\\|\\(?<escaped>[(){}+?|])|(?<unescaped>^\*|[(){}+?|])/gu;
const BACK_REFERENCE = /\\[1-9]/u;
const PNPM_STORE_DEPTH = 3;
const NO_IGNORE = ['--no-ignore', '--no-ignore-vcs', '--unrestricted'];
const GLOB_START = /[*?[{(]/u;
const NAME_CHARACTER = /[\p{L}\p{N}]/u;
const ASCENDS = /(?:^|\/)\.{1,2}(?:\/|$)/u;
const EXPANSION = /[$`<]/u;
const FLAG_SHAPED = /^--?[A-Za-z][\w-]*$/u;
const ESCAPED_NEWLINE = /(?<!\\)(?:\\\\)*\\n/u;
const SHELL_SYNTAX = /[$|&;<>()`]|^\w+=/u;
const MULTILINE: ReadonlySet<string> = new Set(['-U', '--multiline']);
const PREFIXES: ReadonlySet<string> = new Set(['builtin', 'command']);
const DECODER = new TextDecoder();
const SUBSTITUTION = /^s(?<delimiter>[^\w\s\\])(?:\\.|(?!\k<delimiter>).)*\k<delimiter>(?:\\.|(?!\k<delimiter>).)*\k<delimiter>[gpiI\d]*$/su;
const JSON_PARSE = /\bjson\.loads?\(|\bJSON\.parse\(/u;
const STDIN_FILE = /\/dev\/stdin\b/u;
const DUCKDB_MODES = ['-no-agent', '-ascii', '-box', '-column', '-csv', '-html', '-json', '-jsonlines', '-line', '-list', '-quote', '-table'];
const SD_AS_RG: Record.ReadonlyRecord<string, string> = { i: '--ignore-case', c: '--case-sensitive', w: '--word-regexp', s: '--multiline-dotall' };
const INLINE_LOOKUPS = [
    [/^(?:console\.log\()?require\(['"](?<name>[^'"./][^'"]*)\/package\.json['"]\)\.(?<field>\w+)\)?;?$/u, 'jq -r .$<field> node_modules/$<name>/package.json'],
    [/^(?:console\.log\()?require\.resolve\(['"](?<name>[^'"./][^'"]*)\/package\.json['"](?:,\s*\{\s*paths:\s*\[\s*process\.cwd\(\)\s*\]\s*\})?\)\)?;?$/u, 'realpath node_modules/$<name>/package.json'],
    [/^(?:console\.log\()?require\(['"](?:node:)?path['"]\)\.dirname\(require\.resolve\(['"](?<name>[^'"./][^'"]*)\/package\.json['"](?:,\s*\{\s*paths:\s*\[\s*process\.cwd\(\)\s*\]\s*\})?\)\)\)?;?$/u, 'realpath node_modules/$<name>'],
    [/^\s*(?:import time\s*[;\n]\s*time|__import__\(['"]time['"]\))\.sleep\(\s*(?<seconds>\d+(?:\.\d+)?)\s*\)\s*;?\s*$/u, 'sleep $<seconds>'],
    [/^\s*(?:select\(\s*undef\s*,\s*undef\s*,\s*undef\s*,\s*|sleep\(?\s*)(?<seconds>\d+(?:\.\d+)?)\s*\)?\s*;?\s*$/u, 'sleep $<seconds>'],
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
const XARGS_FLAGS: ReadonlySet<string> = new Set(['-0', '-r']);
const PRINT0: ReadonlySet<string> = new Set(['--print0', '-print0']);
const BATCHED = (removal: string) => `-X ${removal}`;
const EXECUTED = (removal: string) => `-exec ${removal} {} +`;
const WALKERS: Record.ReadonlyRecord<string, (removal: string) => string> = { fd: BATCHED, fdfind: BATCHED, find: EXECUTED, gfind: EXECUTED };
const EVERY_OPERAND = (operands: readonly Word[]) => operands;
const LAST_OPERAND = (operands: readonly Word[]) => operands.slice(-1);
const TEMP_WRITERS: Record.ReadonlyRecord<string, (operands: readonly Word[]) => readonly Word[]> = { tee: EVERY_OPERAND, mkdir: EVERY_OPERAND, touch: EVERY_OPERAND, cp: LAST_OPERAND, mv: LAST_OPERAND, ln: LAST_OPERAND };

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [TREE_WALKS]

const rootFolders = (invocation: Invocation, { host }: Call): readonly string[] => {
    const start = folderOf(host, invocation, ...(invocation.row.recursive?.base ?? []));
    return Array.getSomes(invocation.roots.filter(({ known }) => known).map(({ text }) => host.resolve(text, start)));
};

const wideRoot: Rule = (invocation, call) => {
    const { host } = call;
    const stats = invocation.row.recursive?.stats === true;
    const depth = invocation.values('--max-depth', '-maxdepth', '-L').map(Number);
    const bounded = (levels: number) => !stats && depth.length > 0 && depth.every((given) => given <= levels);
    return rootFolders(invocation, call)
        .filter((folder) => (!bounded(1) && (host.path.dirname(folder) === '/' || Option.exists(host.home, (home) => host.inside(home, folder)) || (host.root !== folder && host.inside(host.root, folder)))) || (!stats && !bounded(PNPM_STORE_DEPTH) && folder.endsWith('/node_modules/.pnpm')))
        .flatMap((folder) => refusal(`${invocation.program} root ${folder} too wide, pass a package folder`));
};

const unignored =
    (named: Predicate.Predicate<Invocation>): Rule =>
    (invocation, call) =>
        Effect.map(call.host.ignored(rootFolders(invocation, call)), (ignored) => (ignored || named(invocation) ? ensure(['--no-ignore'], ...NO_IGNORE)(invocation) : []));

// --- [DELETIONS]

const removes =
    (...base: readonly string[]) =>
    (invocation: Invocation, call: Call): readonly Verdict[] => {
        const { host } = call;
        const subject = commandOf(invocation);
        const { walker, words, start, patterns } = deleted(invocation, call, ...base);
        return words.flatMap(({ raw, text, known, glob }) => {
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
        });
    };

const fedRemoval =
    (...base: readonly string[]): Rule =>
    (invocation, call) => {
        const feeder = invocation.outer.findLast(({ row }) => row.wraps !== undefined && row.reads === true);
        if (feeder === undefined) {
            return removes(...base)(invocation, call);
        }
        const nulled = feeder.has('-0');
        const removal = spelled(invocation.words);
        const subject = `xargs ${commandOf(invocation)}`;
        const separated = Option.isSome(invocation.separator) ? [removal] : [removal, '--'];
        const [producer, ...others] = stages(invocation, call).earlier.filter(({ outer }) => outer.length === 0);
        const [listing] = Array.getSomes(redirected(feeder, RedirOperator.RdrIn).map(({ target }) => target));
        const { end } = Array.lastNonEmpty(invocation.words);
        if (invocation.outer.at(-1) !== feeder || ![...feeder.names].every((name) => XARGS_FLAGS.has(name))) {
            return refusal(`${subject} takes xargs options, name each path to ${invocation.program}`);
        }
        if (listing?.known === true) {
            return Effect.fromOption(call.host.resolve(listing.text, feeder.command.cwd)).pipe(
                Effect.flatMap(call.host.fs.readFileString),
                Effect.option,
                Effect.map((content) =>
                    Option.getOrElse(
                        Option.map(content, (text) => text.split(nulled ? NUL : '\n').filter(String.isNonEmpty)),
                        () => [],
                    ),
                ),
                Effect.map((paths) => (paths.length === 0 ? refusal(`${subject} < ${listing.raw} lists no path, name each path to ${invocation.program}`) : edited(invocation, feeder.command, [...separated, ...paths.map((path) => quote(path))].join(' '), `${subject} < ${listing.raw}`))),
            );
        }
        const walked = Option.fromNullishOr(others.length === 0 && producer?.last === true ? producer : undefined);
        const exec = Option.flatMap(walked, (found) => (findsAsFd(found) ? Option.some(BATCHED) : Record.get(WALKERS, found.program)));
        if (Option.isNone(walked) || Option.isNone(exec)) {
            return refusal(`${subject} reads ${producer?.program ?? 'stdin'} output, name each path to ${invocation.program}`);
        }
        const found = walked.value;
        const printed = found.args.filter(({ names }) => names.some((name) => PRINT0.has(name)));
        const gap = DECODER.decode(found.command.source.subarray(found.command.end, found.command.end + 1)) === ' ' ? 1 : 0;
        return printed.length > 0 === nulled ? [...printed.flatMap(({ words }) => cut(found, words)), ...edited(invocation, { start: found.command.end + gap, end }, `${' '.repeat(1 - gap)}${exec.value(removal)}`, subject)] : refusal(`${found.program} and ${subject} disagree on -0, pass -0 to both or neither`);
    };

// --- [STDIN]

const emptyStdin: Rule = (invocation) => {
    const { command, program, outer } = invocation;
    if (command.fed || outer.some(({ row }) => row.execs !== undefined) || !invocation.readsStdin) {
        return [];
    }
    const subject = `${program} reading empty stdin`;
    const refused = `${program} reads empty stdin, name its file`;
    const removable = command.redirects.length === 0 && Option.isNone(command.stage) && outer.every(({ program: wrapper }) => PREFIXES.has(wrapper));
    const alone = [command.source.subarray(0, command.start), command.source.subarray(command.end)].every((part) => DECODER.decode(part).trim() === '');
    if (command.origin !== 'call' || (removable && alone)) {
        return refusal(refused);
    }
    return Option.match(removable ? listed(command.source, command) : Option.none(), { onNone: () => whole(invocation, ':', subject), onSome: (span) => erased(invocation, span, subject) });
};

const inlineCode: Rule = (invocation, { analysis, host }) => {
    const { row, script, command, sources, program } = invocation;
    if (row.code === undefined) {
        return [];
    }
    const inline = [...invocation.values(...row.code), ...invocation.leading.map(({ text }) => text)];
    const files = Array.getSomes(sources.map(({ text }) => host.resolve(text, command.cwd)));
    const written = analysis.invocations.some(({ command: writer, program: named, operands: targets }) =>
        [...Array.getSomes(writer.redirects.filter(({ op }) => WRITES.has(op)).map(({ target }) => target)), ...(named === 'tee' ? targets : [])].some(({ text }) => files.some((file) => Option.contains(host.resolve(text, writer.cwd), file))),
    );
    const probed = written ? Option.orElse(script, () => Option.some('')) : Option.filter(script, () => files.length < sources.length || files.some((path) => !host.inside(path, host.root)));
    const stated = Option.orElse(probed, () => Option.liftPredicate(inline.join('\n'), () => inline.length > 0));
    if (Option.isNone(stated) && !(sources.length === 0 && command.fed && invocation.readsStdin)) {
        return [];
    }
    const source = Option.getOrElse(
        Option.orElse(stated, () => command.input),
        () => '',
    );
    const [lookup] = INLINE_LOOKUPS.filter(([pattern]) => pattern.test(source)).map(([pattern, answer]) => source.replace(pattern, answer));
    if (lookup !== undefined) {
        return silent(whole(invocation, lookup, `${program} inline code`));
    }
    const writes = invocation.has(...(row.rewrites ?? [])) || command.redirects.some(({ op, target }) => WRITES.has(op) && Option.exists(target, ({ text }) => !text.startsWith('/dev/')));
    return refusal(
        `${program} ${Match.value({ json: JSON_PARSE.test(source), writes }).pipe(
            Match.when({ json: true }, () => 'parses JSON inline, use jq'),
            Match.when({ writes: true }, () => 'writes files from inline code, edit them with sd where one pattern covers every change, else with your file edit tool'),
            Match.orElse(() => 'runs inline code, answer it with a CLI (rg, ast-grep, jq, yq, duckdb) or an nx target'),
        )}`,
    );
};

// --- [REPLACEMENTS]

const dashedOperand: Rule = (invocation) => {
    const dashed = Option.isNone(invocation.separator) ? invocation.operands.find(({ text }) => text.startsWith('-')) : undefined;
    if (dashed === undefined) {
        return [];
    }
    if (dashed !== invocation.operands[0] || !FLAG_SHAPED.test(dashed.text)) {
        return inserted(invocation, dashed.start, '-- ');
    }
    if (!MULTILINE.has(dashed.text)) {
        return refusal(`sd ${dashed.text} is no sd option, pass -- first when the pattern starts with -`);
    }
    return invocation.has('--across') ? cut(invocation, [dashed], `sd ${dashed.text}`) : edited(invocation, dashed, '-A', `sd ${dashed.text}`);
};

const unmatched: Rule = Effect.fnUntraced(function* (invocation, { analysis, host }) {
    const {
        leading: [find],
        inputs,
        command,
    } = invocation;
    const files = Array.getSomes(inputs.map(({ text }) => host.resolve(text, command.cwd)));
    const letters = Array.fromIterable(invocation.values('--flags').join(''));
    const named = new Set(
        analysis.invocations.filter(({ command: other }) => other.source === command.source && other.start < command.start).flatMap(({ command: other, operands }) => Array.getSomes([...operands, ...Array.getSomes(other.redirects.map(({ target }) => target))].map(({ text }) => host.resolve(text, other.cwd)))),
    );
    if (find?.known !== true || inputs.length === 0 || files.length < inputs.length || inputs.some(({ known }) => !known) || files.some((file) => named.has(file)) || invocation.has('--preview') || !letters.every((letter) => Record.has(SD_AS_RG, letter))) {
        return [];
    }
    const flags = [...(invocation.has('--fixed-strings') ? ['--fixed-strings'] : []), ...(invocation.has('--across') ? ['--multiline'] : []), ...Array.getSomes(letters.map((letter) => Record.get(SD_AS_RG, letter)))];
    const { code } = yield* host.run(['rg', '--no-config', '--quiet', '--text', ...flags, '--regexp', find.text, '--', ...files]);
    return code === 1 ? refusal(`sd pattern matches nothing in ${spelled(inputs)}, read the current text`) : [];
});

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
        const extended = invocation.has('--extended-regexp', '--fixed-strings') ? word.text : word.text.replaceAll(BASIC_REGEX_SWAPS, (held: string, escaped?: string, unescaped?: string) => escaped ?? (unescaped === undefined ? held : `\\${unescaped}`));
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

const misreadSuffix = (suffix: string): boolean => suffix.startsWith('-') || SUBSTITUTION.test(suffix);

const treeOnly: Predicate.Predicate<Invocation> = ({ has, names }) => has('-R') && names.size === 1;

const optionsFirst: Rule = (invocation) => {
    const [first] = invocation.operands;
    const late = first === undefined ? [] : invocation.args.filter(({ names, words: [word] }) => names.length > 0 && word.start > first.start);
    return first === undefined || late.length === 0 ? [] : [...inserted(invocation, first.start, `${late.map(({ words }) => spelled(words)).join(' ')} `), ...late.flatMap(({ words }) => cut(invocation, words))];
};

const silenced: Rule = (invocation) => {
    const quieted = [...redirected(invocation, RedirOperator.RdrAll), ...redirected(invocation, RedirOperator.RdrOut, undefined, '/dev/null'), ...redirected(invocation, RedirOperator.DplOut, '2')];
    return quieted.length >= 2 || quieted.some(({ op }) => op === RedirOperator.RdrAll) ? [...ensure(['--style', 'none'])(invocation), ...quieted.flatMap((redirect) => cut(invocation, [redirect]))] : [];
};

const cdOutsideRoot: Rule = Effect.fnUntraced(function* (invocation, { host, analysis }) {
    const [target] = invocation.operands;
    const { command } = invocation;
    const outside = Option.filter((folder: string) => !host.inside(folder, host.root));
    const left = target?.known === true && invocation.outer.length === 0 && command.origin === 'call' ? outside(host.resolve(target.text, command.cwd)) : Option.none();
    const later = Array.dedupe(analysis.invocations.filter((other) => other.command.source === command.source && other.command.start > command.start && Option.isSome(outside(other.command.cwd))).map(({ program }) => program));
    const found = yield* Effect.forEach(Array.cartesian(Option.toArray(left), later), ([folder, program]) => Effect.all([host.onPath(program), Effect.orElseSucceed(Effect.map(host.output(['mise', '-C', folder, 'exec', '--', 'which', program]), String.isNonEmpty), () => true)]), { concurrency: 'unbounded' });
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

const backupSuffix: Rule = (invocation) =>
    invocation.args
        .filter(({ names, values }) => names.includes('-i') && values.some(({ text }) => misreadSuffix(text)))
        .flatMap(({ words: [flag, ...given] }) => (given.length === 0 ? refusal(`sed ${flag.raw} reads its tail as backup suffix, pass -i and '' as two words`) : edited(invocation, flag, `${flag.raw} ''`, `sed ${flag.raw}`)));

const emptyPattern: Rule = (invocation) => (invocation.leading[0]?.text === '' && !invocation.has('--preview') ? edited(invocation, invocation.head, `${invocation.head.raw} -p`, 'sd with an empty pattern') : []);

const substitutedInput: Rule = (invocation) => {
    const read = invocation.supplied('--input').find(({ raw }) => raw.startsWith('<(') && raw.endsWith(')'));
    const { command } = invocation;
    if (read === undefined) {
        return [];
    }
    return command.origin === 'call' && Option.isNone(command.stage)
        ? [...inserted(invocation, command.start, `{ hyperfine_input=$(mktemp) && { ${read.raw.slice(2, -1)}; } > "$hyperfine_input" && `), ...edited(invocation, read, '"$hyperfine_input"', `hyperfine --input ${read.raw}`), ...inserted(invocation, command.end, '; }')]
        : refusal(`hyperfine --input ${read.raw} is read once, pass a file every run reads`);
};

// --- [TEMPORARY]

const systemTemp = ({ analysis }: Call): readonly Verdict[] => {
    const own = analysis.invocations.filter(({ command }) => command.origin === 'call');
    const targets = [
        ...Array.dedupe(own.map(({ command }) => command)).flatMap(({ redirects }) => Array.getSomes(redirects.filter(({ op }) => WRITES.has(op)).map(({ target }) => target))),
        ...own.flatMap(({ program, operands }) =>
            Option.getOrElse(
                Option.map(Record.get(TEMP_WRITERS, program), (written) => written(operands)),
                () => [],
            ),
        ),
    ];
    const written = new Set(targets.filter(({ known, raw }) => known && relocated(raw) !== raw).map(({ raw }) => raw));
    const words = Array.dedupe(own.map(({ command }) => command)).flatMap(({ words: held, redirects }) => [...held, ...Array.getSomes(redirects.map(({ target }) => target))]);
    return words.filter(({ raw, field, prefix }) => written.has(raw) && field === undefined && prefix === undefined).map(({ start, end, raw }) => Verdict.Edit({ start, end, text: Option.some(relocated(raw)), subject: Option.some(raw) }));
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
            rules: [when(({ has }) => has('-delete'), fedRemoval()), when(findsAsFd, findAsFd)],
        },
        rm: { aliases: ['rimraf', 'trash', 'unlink', 'shred'], rules: [fedRemoval()] },
        git: { rules: [perSubcommand({ rm: [when(({ has }) => !has('--cached'), fedRemoval('-C'))] })] },
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
        sed: { rules: [backupSuffix] },
        cat: { reads: true, rules: [respell({ renames: { '-A': '-vet' } })] },
        ls: { rules: [when(treeOnly, (invocation) => edited(invocation, invocation.head, 'tree'), respell({ drops: ['-R'] })), when(Predicate.not(treeOnly), optionsFirst)] },
        sd: {
            strict: true,
            leading: 2,
            reads: true,
            rules: [dashedOperand, emptyPattern, sameTexts, when(({ leading: [find], has }) => find !== undefined && (find.text.includes('\n') || (!has('--fixed-strings') && ESCAPED_NEWLINE.test(find.text))), ensure(['-A'])), unmatched],
        },
        curl: { pipefail: true, rules: [ensure(['--fail-with-body'], '--fail', '--write-out')] },
        duckdb: { rules: [when(Predicate.or(holds(STDIN_FILE, '-c', '-s'), holds(STDIN_FILE)), ensure(['-markdown'], ...DUCKDB_MODES))] },
        cd: { rules: [cdOutsideRoot] },
        jq: { flags: ['--arg=2', '--argjson=2', '--slurpfile=2', '--rawfile=2'], leading: 1, leadingUnless: ['--from-file'], restAfter: ['--args', '--jsonargs'], reads: true, readsUnless: ['--null-input'], rules: [readsItsOwnFile] },
        hyperfine: {
            flags: ['-P, --parameter-scan=3', '-L, --parameter-list=2'],
            rules: [ensure(['-i']), shellBodies, silenced, substitutedInput],
        },
        nproc: { rules: [absent((invocation) => whole(invocation, 'sysctl -n hw.ncpu'))] },
        shuf: { flags: ['-n='], rules: [absent(shuffled)] },
        base64: { flags: ['-i=', '-o=', '-b='], reads: true, readsUnless: ['-i'], rules: [inputFlagged] },
    },
    every: [wideRoot, emptyStdin, inlineCode],
    calls: [systemTemp],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { FILES };
