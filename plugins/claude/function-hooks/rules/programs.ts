import { Array, Effect, Equal, FileSystem, Match, Option, Path, Record, String } from 'effect';
import { RedirOperator } from 'sh-syntax';
import { type Call, covers, execute, type Invocation, rewrite, type Script, type Table, type Verdict } from '#/rules/rule';

// --- [TYPES] ---------------------------------------------------------------------------

type Idiom = readonly [RegExp, (groups: Readonly<Record<string, string | undefined>>, invocation: Invocation) => Option.Option<Array.NonEmptyReadonlyArray<string>>];
interface Language {
    readonly aliases: readonly string[];
    readonly idioms: readonly Idiom[];
    readonly effects?: RegExp;
    readonly inPlace?: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const sedSubstitution = /^s(?<mark>[^\w\s\\]).*\k<mark>.*\k<mark>[gpiI\d]*$/su;
const systemTemporary = /^(?:\/private)?\/tmp(?=\/(?!claude-))/u;
const shellSyntax = /[$|&;<>()`]|^\w+=/u;
const shellWrapper = /^(?:\S*\/)?(?:ba|z|da|k)?sh -c$/u;
const jsonParse = /\bjson\.loads?\(|\bJSON\.parse\(/u;
const stringLiteral = /"(?:[^"\\\n]|\\.)*"|'(?:[^'\\\n]|\\.)*'/gu;
const statuses = ['nx', 'uv', 'dotnet', 'xcodebuild', 'swift', 'npm', 'pnpm', 'cargo', 'curl', 'pytest', 'vitest'];
const altering: ReadonlySet<string> = new Set(['tied', 'readonly', 'association']);
const rgFlags: Readonly<Record<string, string>> = { i: '--ignore-case', c: '--case-sensitive', w: '--word-regexp', s: '--multiline-dotall' };
const patchAdvice = 'Set the package option, a newer release, or the consumer tsconfig.';

// --- [OPERATIONS] ----------------------------------------------------------------------

const filesOf = (invocation: Invocation) => Option.all(invocation.operands.filter(({ roles }) => roles.includes('read')).map(({ word }) => word.text));
const sleep: Idiom[1] = ({ seconds = '' }) => Option.some(Array.make('sleep', seconds));
const languages = {
    awk: {
        aliases: ['gawk', 'mawk'],
        idioms: [
            [
                /^\s*NR\s*(?:==\s*(?<line>\d+)|>=\s*(?<first>\d+)\s*&&\s*NR\s*<=\s*(?<last>\d+))\s*(?:\{\s*print(?:\s+\$0)?\s*;?\s*\})?\s*$/u,
                ({ line, first, last }, invocation) =>
                    Option.map(
                        Option.filter(filesOf(invocation), () => invocation.flags.length === 0),
                        (given) => Array.make('sed', '-n', `${line ?? `${first},${last}`}p`, ...given),
                    ),
            ],
            [
                /^\s*(?:1|\{\s*print(?:\s+\$0)?\s*;?\s*\})\s*$/u,
                (_, invocation) =>
                    Option.map(
                        Option.filter(filesOf(invocation), () => invocation.flags.length === 0),
                        (given) => Array.make('cat', ...given),
                    ),
            ],
            [
                /^\s*\{\s*print\s+\$1\s*;?\s*\}\s*$/u,
                (_, invocation) => {
                    const separator = Option.filter(
                        Option.flatMap(Array.head(invocation.values('-F')), ({ text }) => text),
                        (given) => /^[^\s\\]$/u.test(given) && invocation.flags.length === 1,
                    );
                    return Option.map(Option.all([separator, filesOf(invocation)]), ([given, read]) => Array.make('cut', `-d${given}`, '-f1', ...read));
                },
            ],
        ],
        effects: /\bprintf?\b[^;}\n]*\||\|\s*getline\b|\bsystem\s*\(/u,
    },
    python: {
        aliases: ['python3'],
        idioms: [[/^\s*(?:import time\s*[;\n]\s*time|__import__\(['"]time['"]\))\.sleep\(\s*(?<seconds>\d+(?:\.\d+)?)\s*\)\s*;?\s*$/u, sleep]],
        effects: /\bsubprocess\.|\bshutil\.rmtree\(|\bos\.(?:system|popen|exec\w*|spawn\w*|remove|removedirs)\(|\.(?:unlink|rmdir)\(/u,
    },
    node: {
        aliases: [],
        idioms: [
            [/^(?:console\.log\()?require\(['"](?<name>[^'"./][^'"]*)\/package\.json['"]\)\.(?<field>\w+)\)?;?$/u, ({ name = '', field = '' }) => Option.some(Array.make('jq', '-r', `.${field}`, `node_modules/${name}/package.json`))],
            [/^(?:console\.log\()?require\.resolve\(['"](?<name>[^'"./][^'"]*)\/package\.json['"]\)\)?;?$/u, ({ name = '' }) => Option.some(Array.make('realpath', `node_modules/${name}/package.json`))],
            [/^(?:console\.log\()?require\(['"](?:node:)?path['"]\)\.dirname\(require\.resolve\(['"](?<name>[^'"./][^'"]*)\/package\.json['"]\)\)\)?;?$/u, ({ name = '' }) => Option.some(Array.make('realpath', `node_modules/${name}`))],
        ],
        effects: /\bchild_process\b|\b(?:unlink|rm|rmdir)(?:Sync)?\(/u,
    },
    perl: { aliases: [], idioms: [[/^\s*(?:select\(\s*undef\s*,\s*undef\s*,\s*undef\s*,\s*|sleep\(?\s*)(?<seconds>\d+(?:\.\d+)?)\s*\)?\s*;?\s*$/u, sleep]], inPlace: true },
    ruby: { aliases: [], idioms: [], inPlace: true },
    osascript: { aliases: [], idioms: [[/^\s*delay\s+(?<seconds>\d+(?:\.\d+)?)\s*$/u, sleep]] },
} satisfies Record<string, Language>;
const codeOf = (invocation: Invocation) => invocation.arguments.collect('code').flatMap(({ words }) => Array.take(words, 1));
const takesScript = ({ flag, values }: Invocation['flags'][number]) => flag.spellings.includes('-i') && values.some((value) => value.test(/^-/u) || value.test(sedSubstitution));
const readsInput = (invocation: Invocation) => {
    const { input, positional, opaque } = invocation.arguments;
    const valued = invocation.flags.some(({ flag }) =>
        Option.exists(
            Option.flatMap(invocation.usage, (usage) => usage.value(flag)),
            ({ roles }) => roles.includes('read'),
        ),
    );
    const exempt = (invocation.runs('sed') && invocation.flags.some(takesScript)) || (invocation.runs('read') && invocation.has('-t')) || (invocation.runs('awk') && codeOf(invocation).every((word) => word.test(/^\s*BEGIN\s*\{[\s\S]*\}\s*$/u)));
    return !opaque && Option.exists(input, ({ roles }) => roles.includes('read') || roles.includes('shell')) && !positional.some(({ slot }) => slot.tree) && !valued && !exempt;
};
const closes = (invocation: Invocation) => invocation.runs('head') || (invocation.runs('grep', 'rg') && invocation.has('-q', '--quiet', '-m', '--max-count', '-l', '--files-with-matches'));
const writesBefore = (invocations: readonly Invocation[], reader: Invocation, path: string) => invocations.some((other) => other.start < reader.start && other.writes.some(({ text }) => Option.contains(text, path)));
const isInstalled = Effect.fnUntraced(function* (invocation: Invocation, { dialect }: Script) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folders = (dialect.environment['PATH'] ?? '').split(':');
    const candidates = Option.toArray(invocation.name).flatMap((name) => folders.map((folder) => path.join(folder, name)));
    return Option.isSome(yield* Effect.findFirst(candidates, (file) => Effect.orElseSucceed(fs.exists(file), () => false)));
});
const judgeCode = (name: keyof typeof languages) => {
    const { idioms, effects, inPlace }: Language = languages[name];
    return Effect.fnUntraced(function* (invocation: Invocation, { invocations }: Call) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const executed = Array.findFirst(invocation.operands, ({ roles }) => roles.includes('execute'));
        const readsCode = Option.exists(invocation.usage, ({ input }) => Option.exists(input, ({ roles, when, unless }) => roles.includes('code') && (when.length === 0 || invocation.has(...when)) && !invocation.has(...unless)));
        const fromInput = readsCode && Option.isNone(Option.filter(executed, ({ word }) => !word.spells('-')));
        const heredoc = invocation.stdin._tag === 'Text' && fromInput ? [invocation.stdin.text] : [];
        const inline = Option.map(Option.all([...codeOf(invocation).map(({ text }) => text), ...heredoc]), (pieces) => pieces.join('\n').trim());
        const file = Option.flatMap(Option.all([Option.flatMap(executed, ({ word }) => word.text), invocation.place._tag === 'Local' ? invocation.place.cwd.text : Option.none<string>()]), ([given, cwd]) =>
            Option.liftPredicate(path.resolve(cwd, given), (resolved) => path.dirname(resolved) === cwd && Option.contains(inline, '') && !writesBefore(invocations, invocation, resolved)),
        );
        const source = yield* file.pipe(
            Effect.fromOption,
            Effect.flatMap((resolved) => fs.readFileString(resolved)),
            Effect.option,
        );
        const written = Option.getOrElse(inline, () => '');
        const command = Array.findFirst(idioms, ([pattern, restate]) => Option.flatMap(Option.fromNullishOr(pattern.exec(Option.match(source, { onNone: () => written, onSome: String.trim }))), ({ groups = {} }) => restate(groups, invocation)));
        return Match.value({ command: Option.getOrUndefined(command), inPlace: inPlace === true && invocation.has('-i'), json: jsonParse.test(written), effects: effects?.test(written.replaceAll(stringLiteral, '""')) === true }).pipe(
            Match.when({ command: Match.defined }, ({ command: words }) => rewrite(invocation, words)),
            Match.when({ inPlace: true }, () => [{ deny: `${name} -i edits files in place where the hook cannot judge the change.`, advice: 'Edit them with sd where one pattern covers every change, else with your file edit tool.' }]),
            Match.when({ json: true }, () => [{ deny: `${name} inline code parses JSON.`, advice: 'Read it with jq.' }]),
            Match.when({ effects: true }, () => [{ deny: `${name} inline code runs commands or deletes files where the hook cannot judge them.`, advice: 'Run each as its own shell command.' }]),
            Match.orElse(() => []),
        );
    });
};

// --- [RULES] ---------------------------------------------------------------------------

const programs: Table = {
    'stdin.empty': {
        action: 'Shell',
        decide: (script, { invocations }) => {
            const alone = invocations.filter(({ source }) => source === script.source).length === 1;
            return invocations
                .filter((invocation) => readsInput(invocation) && invocation.stdin._tag === 'Inherited')
                .flatMap((invocation) =>
                    Match.value({ span: Option.getOrUndefined(invocation.span), stmt: Option.getOrUndefined(invocation.node), local: invocation.source === script.source, nested: Option.isSome(invocation.stage) || invocation.within.length > 0, alone }).pipe(
                        Match.when({ span: Match.defined, local: true, nested: true }, ({ span }): readonly Verdict[] => [{ edit: (editor) => [editor.replace(span, [':'])] }, { note: `${invocation.label} read standard input, which this call leaves empty, so it became : and the rest of the call ran.`, once: 'call' }]),
                        Match.when({ stmt: Match.defined, local: true, nested: false, alone: false }, ({ stmt }): readonly Verdict[] => [{ edit: (editor) => [editor.remove(stmt)] }, { note: `${invocation.label} read standard input, which this call leaves empty, so the call ran without it.`, once: 'call' }]),
                        Match.orElse((): readonly Verdict[] => [{ deny: `${invocation.label} reads standard input, which this call leaves empty.`, advice: 'Name the file it should read, or pipe or redirect its input.' }]),
                    ),
                );
        },
    },
    ...Record.fromEntries(Record.collect(languages, (program, { aliases }) => [`code.${program}`, { program, aliases, decide: judgeCode(program) }] as const)),
    'sed.suffix': {
        program: 'sed',
        decide: (sed) =>
            sed.flags.filter(takesScript).flatMap(({ spelled, values }) => {
                const value = Option.flatMap(Array.head(values), ({ node }) => node);
                const expression = Option.flatMap(Array.head(values), ({ text }) => text);
                return Option.toArray(Option.all([spelled.node, expression])).map(([word, given]) => ({
                    edit: (editor) => [Option.match(value, { onNone: () => editor.replace(word, ['-i', '', given]), onSome: () => editor.after(word, ['']) })],
                }));
            }),
    },
    'ls.options': {
        program: 'ls',
        decide: (ls) =>
            Option.toArray(Option.flatMap(Array.head(ls.operands), ({ word }) => word.node)).flatMap((first) => {
                const flags = ls.flags.flatMap(({ spelled }) => Option.toArray(Option.all([spelled.node, spelled.text])));
                const late = Array.dedupeWith(
                    flags.filter(([word]) => word.start > first.start),
                    ([left], [right]) => left === right,
                );
                return Array.match(late, {
                    onEmpty: () => [],
                    onNonEmpty: (options) => [
                        {
                            edit: (editor) => [
                                editor.before(
                                    first,
                                    Array.map(options, ([, text]) => text),
                                ),
                                ...options.map(([word]) => editor.remove(word)),
                            ],
                        },
                    ],
                });
            }),
    },
    'cat.visible': { program: 'cat', rename: { '-A': '-vet' } },
    'curl.fail': { program: 'curl', ensure: ['--fail-with-body'], unless: ['--fail', '--write-out'] },
    'screencapture.clipboard': { program: 'screencapture', when: ['-c'], deny: "replaces the owner's clipboard", advice: 'Pass a file.' },
    'screencapture.interactive': { program: 'screencapture', drop: ['-i', '-W', '-U', '-P', '-M'] },
    'pnpm.patch': { program: 'pnpm', path: ['patch'], deny: 'copies a package into the tree', advice: patchAdvice },
    'pnpm.patch-commit': { program: 'pnpm', path: ['patch-commit'], deny: 'writes a patched package into the tree', advice: patchAdvice },
    'sdef.launch': {
        program: 'sdef',
        decide: Effect.fnUntraced(function* (sdef, { cwd }) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const read = (plist: string, key: string) => Effect.map(execute(cwd)(['plutil', '-extract', key, 'raw', '-o', '-', plist]), ({ stdout, code }) => Option.liftPredicate(stdout.trim(), () => code === 0));
            const launching = yield* Effect.filter(
                Array.getSomes(sdef.operands.map(({ word }) => word.text)).map((given) => path.resolve(cwd, given)),
                Effect.fnUntraced(function* (bundle) {
                    const plist = path.join(bundle, 'Contents', 'Info.plist');
                    const [present, kind, file, dynamic] = yield* Effect.all([Effect.orElseSucceed(fs.exists(plist), () => false), read(plist, 'CFBundlePackageType'), read(plist, 'OSAScriptingDefinition'), read(plist, 'OSAScriptingDefinitionIsDynamic')]);
                    return present && !Option.exists(kind, (type) => type !== 'APPL') && (Option.isNone(file) || Option.contains(dynamic, 'true'));
                }),
            );
            return launching.map((bundle) => ({
                deny: `sdef ${bundle} launches the application, as its Info.plist names no static OSAScriptingDefinition.`,
                advice: 'Read the .scriptSuite and .scriptTerminology pair or the aete .rsrc file under Contents/Resources, or the command list execute_script returns for a running instance.',
            }));
        }),
    },
    'sd.dashed': {
        program: 'sd',
        decide: (sd) =>
            sd.flags
                .filter(({ flag }) => !Option.exists(sd.usage, (usage) => Option.isSome(usage.flag(flag.spellings[0]))))
                .flatMap(({ spelled }) => Option.toArray(Option.all([spelled.node, spelled.text])))
                .map(([word, text]) =>
                    Match.value({ multiline: text === '-U' || text === '--multiline', option: /^--?[A-Za-z][\w-]*$/u.test(text) }).pipe(
                        Match.when({ multiline: true }, () => (sd.has('--across') ? { drop: [text] } : { rename: { [text]: '-A' } })),
                        Match.when({ option: true }, () => ({ deny: `sd ${text} is no sd option.`, advice: 'Pass -- before a pattern that starts with -.' })),
                        Match.orElse((): Verdict => ({ edit: (editor) => [editor.before(word, ['--'])] })),
                    ),
                ),
    },
    'sd.empty': { program: 'sd', decide: (sd) => (Option.contains(sd.operand(), '') ? [{ ensure: ['--preview'] }] : []) },
    'sd.same': {
        program: 'sd',
        decide: (sd) => {
            const [find, replace] = [sd.operand(), sd.operand(1)];
            const plain = sd.has('--fixed-strings') || Option.exists(find, (text) => !/[\\.+*?()|[\]{}^$]/u.test(text));
            return Option.isSome(find) && Equal.equals(find, replace) && plain && !sd.has('--flags', '--preview') && sd.operands.length > 2 ? rewrite(sd, [':']) : [];
        },
    },
    'sd.across': {
        program: 'sd',
        decide: (sd) => (Option.exists(sd.operand(), (find) => find.includes('\n') || (!sd.has('--fixed-strings') && /(?<!\\)(?:\\\\)*\\n/u.test(find))) ? [{ ensure: ['--across'] }] : []),
    },
    'sd.unmatched': {
        program: 'sd',
        decide: Effect.fnUntraced(function* (sd, { invocations, cwd }) {
            const fs = yield* FileSystem.FileSystem;
            const letters = Array.fromIterable(
                Option.getOrElse(
                    Option.flatMap(Array.head(sd.values('--flags')), ({ text }) => text),
                    () => '',
                ),
            );
            const flags = Option.all(letters.map((letter) => Record.get(rgFlags, letter)));
            const paths = Option.getOrElse(Option.all(sd.arguments.effects.writes.map(({ text }) => text)), Array.empty<string>);
            const present = yield* Effect.forEach(paths, (path) => Effect.orElseSucceed(fs.exists(path), () => false));
            const settled = Array.isReadonlyArrayNonEmpty(paths) && present.every(Boolean) && !paths.some((path) => writesBefore(invocations, sd, path));
            const options = [...(sd.has('--fixed-strings') ? ['--fixed-strings'] : []), ...(sd.has('--across') ? ['--multiline'] : [])];
            const matches = yield* Option.match(
                Option.filter(Option.all([sd.operand(), flags]), () => settled),
                {
                    onNone: () => Effect.succeed(true),
                    onSome: ([find, given]) =>
                        sd.has('--fixed-strings') && Array.isReadonlyArrayEmpty(given)
                            ? Effect.map(
                                  Effect.forEach(paths, (path) => Effect.orElseSucceed(fs.readFileString(path), () => find)),
                                  (texts) => texts.some((text) => text.includes(find)),
                              )
                            : Effect.map(execute(cwd)(['rg', '--no-config', '--quiet', '--text', ...options, ...given, '--regexp', find, '--', ...paths]), ({ code }) => code !== 1),
                },
            );
            return matches ? [] : [{ deny: `sd pattern matches nothing in ${paths.join(' ')}.`, advice: 'Read the current text, then match what it holds.' }];
        }),
    },
    'jq.file': {
        program: 'jq',
        decide: (jq, { invocations }) => {
            const cat = Array.findFirst(invocations, (other) => other.feeds(jq) && other.runs('cat') && other.flags.length === 0 && other.operands.length === 1 && Option.exists(other.stage, ({ index, count }) => index === 0 && count === 2) && Option.exists(other.node, ({ redirs }) => redirs.length === 0));
            const reads = jq.operands.some(({ roles }) => roles.includes('read')) || jq.rest.length > 0 || jq.within.length > 0;
            const moved = Option.all([Option.flatMap(cat, (found) => found.operand()), jq.span, Option.flatMap(cat, ({ node }) => node)]);
            return Option.toArray(Option.filter(moved, () => !reads)).map(([file, [, last], stmt]) => ({ edit: (editor) => [editor.remove(stmt), editor.after(last, [file])] }));
        },
    },
    'duckdb.stdin': {
        program: 'duckdb',
        decide: (duckdb) => (duckdb.words.some((word) => word.test(/\/dev\/stdin/u)) ? [{ ensure: ['-markdown'], unless: ['-ascii', '-box', '-column', '-csv', '-html', '-json', '-jsonlines', '-line', '-list', '-quote', '-table'] }] : []),
    },
    'hyperfine.shell': {
        program: 'hyperfine',
        decide: (hyperfine) => {
            const shells = hyperfine.flags.filter(({ flag, values }) => flag.spellings.includes('--shell') && values.some((value) => value.test(shellWrapper)));
            const stray = hyperfine.has('-S', '-N') && hyperfine.operands.length > 1 ? hyperfine.operands.filter(({ word }) => word.test(shellWrapper)).flatMap(({ word }) => Option.toArray(word.node)) : [];
            const scripted = hyperfine.operands.some(({ word }) => word.test(shellSyntax));
            return [
                ...(scripted ? [{ ensure: Array.of('-S'), unless: ['-N', '--shell'] }] : []),
                ...shells.flatMap(({ spelled, values }) =>
                    Option.toArray(spelled.node).map((word): Verdict => ({
                        edit: (editor) => [
                            Option.match(
                                Option.flatMap(Array.head(values), ({ node }) => node),
                                { onNone: () => editor.replace(word, ['--shell=default']), onSome: (value) => editor.replace(value, ['default']) },
                            ),
                        ],
                    })),
                ),
                ...stray.map((word): Verdict => ({ edit: (editor) => [editor.remove(word)] })),
            ];
        },
    },
    'hyperfine.silenced': {
        program: 'hyperfine',
        decide: (hyperfine, { script }) => {
            const quiet = Option.toArray(hyperfine.node)
                .flatMap(({ redirs }) => redirs)
                .filter(({ op, n, word }) => {
                    const target = Option.flatMap(Option.fromNullishOr(word), (given) => script.expanded(given));
                    return ((op === RedirOperator.RdrAll || op === RedirOperator.RdrOut) && Option.contains(target, '/dev/null')) || (op === RedirOperator.DplOut && n?.value === '2' && Option.contains(target, '1'));
                });
            return (quiet.length > 1 || quiet.some(({ op }) => op === RedirOperator.RdrAll)) && !hyperfine.has('--style') ? [{ ensure: Array.make('--style', 'none') }, { edit: (editor) => quiet.map((redirect) => editor.remove(redirect)) }] : [];
        },
    },
    'hyperfine.input': {
        program: 'hyperfine',
        decide: Effect.fnUntraced(function* (hyperfine, { script, shell }) {
            const inputs = hyperfine.flags.filter(({ flag }) => flag.spellings.includes('--input'));
            const candidates = inputs.flatMap(({ spelled, values }) =>
                Option.match(
                    Option.flatMap(Array.head(values), ({ node }) => node),
                    { onNone: () => Option.toArray(spelled.node).map((word) => ({ word, joined: true })), onSome: (word) => [{ word, joined: false }] },
                ),
            );
            const moved = Array.findFirst(candidates, ({ word, joined }) => {
                const part = word.parts.at(-1);
                return part?.type === 'ProcSubst' && (word.parts.length === 1 || (joined && word.parts.length === 2 && word.parts[0]?.type === 'Lit')) ? Option.some({ word, part, joined }) : Option.none();
            });
            const written = yield* moved.pipe(
                Effect.fromOption,
                Effect.flatMap(({ part, joined }) => Effect.all([shell.script(script)`hyperfine_input=$(mktemp) && cat ${part} > "$hyperfine_input"`, joined ? shell.script(script.dialect)`--input "$hyperfine_input"` : shell.script(script.dialect)`"$hyperfine_input"`])),
                Effect.option,
            );
            const words = inputs.flatMap(({ spelled, values }) => [spelled, ...values]).flatMap(({ node }) => Option.toArray(node));
            const substituted = words.some(({ parts }) => parts.some(({ type }) => type === 'ProcSubst'));
            return Match.value({ written: Option.getOrUndefined(Option.all([written, moved, hyperfine.node])), substituted }).pipe(
                Match.when({ written: Match.defined }, ({ written: [[prepared, input], { word }, stmt] }): readonly Verdict[] => [{ edit: (editor) => [editor.precede(prepared, '&&', stmt), editor.replace(word, input)] }]),
                Match.when({ substituted: true }, (): readonly Verdict[] => [{ deny: 'hyperfine --input <(…) is read by the first run alone.', advice: 'Write the input to a file, then pass that file to --input.' }]),
                Match.orElse(() => []),
            );
        }),
    },
    'base64.input': {
        program: 'base64',
        decide: (base64) => {
            const file = Option.flatMap(
                Array.findFirst(base64.operands, ({ roles }) => roles.includes('read')),
                ({ word }) => word.node,
            );
            const named = Option.exists(base64.usage, (usage) => Option.exists(usage.flag('-i'), ({ spellings }) => spellings.includes('--input')));
            return Option.toArray(Option.filter(file, () => named && !base64.has('-i'))).map((word) => ({ edit: (editor) => [editor.before(word, ['-i'])] }));
        },
    },
    'nproc.absent': { program: 'nproc', decide: (nproc, { script }) => Effect.map(isInstalled(nproc, script), (installed) => (installed ? [] : rewrite(nproc, ['getconf', '_NPROCESSORS_ONLN']))) },
    'shuf.absent': {
        program: 'shuf',
        decide: Effect.fnUntraced(function* (shuf, { script, shell }) {
            const installed = yield* isInstalled(shuf, script);
            const count = Option.flatMap(Array.head(shuf.values('-n')), ({ text }) => text);
            const head = yield* Effect.transposeOption(Option.map(count, (lines) => shell.script(script.dialect)`head -n ${lines}`));
            return installed || !shuf.only('-n') || shuf.has('-n') !== Option.isSome(count)
                ? []
                : [{ drop: ['-n'] }, { edit: (editor) => [...Option.toArray(shuf.head).map((word) => editor.replace(word, ['sort', '-R'])), ...Option.toArray(Option.all([head, shuf.node])).map(([taken, stmt]) => editor.follow(stmt, '|', taken))] }];
        }),
    },
    'temporary.system': {
        action: 'Shell',
        decide: (script, { invocations }) => {
            const written = invocations.flatMap(({ writes }) => Array.getSomes(writes.map(({ text }) => text))).filter((path) => systemTemporary.test(path));
            const words = invocations.flatMap((invocation) => invocation.words.flatMap(({ node, text }) => Option.toArray(Option.all([node, text]))));
            const targets = invocations
                .flatMap(({ node }) => Option.toArray(node))
                .flatMap(({ redirs }) => redirs.flatMap(({ word }) => Array.fromNullishOr(word)))
                .flatMap((word) => Option.toArray(Option.map(script.expanded(word), (text) => [word, text] as const)));
            const moved = Array.dedupeWith([...words, ...targets], ([left], [right]) => left === right).filter(([, text]) => systemTemporary.test(text) && written.some((path) => covers(path, text) || covers(text, path)));
            const folder = Option.map(Record.get(script.dialect.environment, 'TMPDIR'), (given) => given.replace(/\/+$/u, ''));
            return Option.toArray(Option.filter(folder, () => Array.isReadonlyArrayNonEmpty(moved))).flatMap((given) => [
                { edit: (editor) => moved.map(([word, text]) => editor.replace(word, [text.replace(systemTemporary, given)])) },
                ...Array.dedupe(moved.map(([, text]) => text)).map((text) => ({ note: `${text} became ${text.replace(systemTemporary, given)}, as /tmp is shared by every user; later calls use that path.`, once: 'call' as const })),
            ]);
        },
    },
    'pipe.status': {
        action: 'Shell',
        decide: Effect.fnUntraced(function* (script, { invocations, shell }) {
            const listed = yield* script.invocations;
            const set = script.dialect.options.pipefail || listed.some((invocation) => invocation.runs('set') && invocation.words.some((word) => word.spells('pipefail')));
            const hidden = listed.some((invocation) => invocation.source === script.source && Option.exists(invocation.stage, ({ index, count }) => index < count - 1) && invocation.leaves.some((leaf) => leaf.runs(...statuses)) && !invocations.some((next) => invocation.feeds(next) && closes(next)));
            const first = Option.filter(Array.head(script.syntax.stmts), () => hidden && !set);
            const pipefail = yield* Effect.transposeOption(Option.map(first, () => shell.script(script.dialect)`set -o pipefail`));
            return Option.toArray(Option.all([first, pipefail])).map(([stmt, command]) => ({ edit: (editor) => [editor.precede(command, ';', stmt)] }));
        }),
    },
    'zsh.special': {
        action: 'Shell',
        decide: ({ tree, dialect }) => {
            const assigned = tree.links.keys().flatMap((node) => ((node.type === 'Assign' || node.type === 'WordIter') && node.name !== null ? [node.name.value] : []));
            return Array.dedupe(assigned.filter((name) => /^\p{Ll}/u.test(name)).toArray()).flatMap((name) => {
                const kind = Option.filter(Option.fromUndefinedOr(dialect.parameters.get(name)), (given) => given.has('special') && !given.isDisjointFrom(altering));
                return Option.toArray(kind).map((given) => ({ deny: `zsh parameter ${name} is ${[...given].join('-')}, so assigning it changes the shell itself.`, advice: 'Rename the variable.' }));
            });
        },
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { programs };
