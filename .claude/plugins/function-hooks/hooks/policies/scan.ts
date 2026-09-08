// Scan rows over an edited path, each a command and its lines

// --- [IMPORTS] -------------------------------------------------------------------------

import type { FsEntry, OpValueOf } from 'claude-code';
import {
    flatMap,
    fromBoolean,
    fromNullable,
    fromPredicate,
    getOrElse,
    liftPredicate,
    map,
    type Option,
    optional,
    struct,
    toArray,
} from '../composition/option.ts';
import { decodeJson, decodeScan, type Entry, isNullableString, isNumber, isString, keys, type Scan, stampOf, suffix } from '../host/store.ts';
import { first, lines } from '../text/lines.ts';
import { basename, extension, under } from '../text/path.ts';
import { MS_PER_DAY } from './findings.ts';

// --- [TYPES] ---------------------------------------------------------------------------

// What $.process.run resolves with, ProcessRunResult in the declarations under a name they do not export
type Run = OpValueOf['process.run'];

// The rule and rewrite ids with their language, the facts the util row's filter reads
type RuleId = Pick<RuleFile, 'id' | 'language'>;

interface ScanFacts {
    readonly rules: readonly RuleId[];
}

// One command per row over the paths it matches, the lines are the context the model reads after the edit
interface ScanRow {
    readonly match: (path: string, facts: ScanFacts) => boolean;
    readonly argv: (path: string, facts: ScanFacts) => readonly string[];
    readonly lines: (run: Run, path: string, facts: ScanFacts) => readonly string[];
    readonly timeoutMs?: number;
}

// One hit of ast-grep scan --json=compact, the fields the lines and the store row read
interface Hit {
    readonly ruleId: string;
    readonly file: string;
    readonly line: number;
    readonly note: string;
    readonly replacement: Option<string>;
}

interface _Range {
    readonly start: { readonly line: number };
}

interface _CompactHit {
    readonly ruleId: string;
    readonly file: string;
    readonly message: string;
    readonly note?: string | null;
    readonly range: _Range;
    readonly replacement?: string;
}

// One rule file under tools/ast-grep/rules/<language>/<package>/<id>.yml with the modification time $.fs.stat reads
interface RuleFile {
    readonly id: string;
    readonly language: string;
    readonly package: string;
    readonly mtimeMs: number;
}

// One package directory of the rule or rewrite tree, tools/ast-grep/<kind>/<language>/<package>/<id>.yml
interface Package {
    readonly language: string;
    readonly package: string;
    readonly ids: readonly string[];
}

// The scan/ entries that decode, name a listed rule, and sit inside the window
interface _Live {
    readonly key: string;
    readonly ruleId: string;
    readonly file: string;
    readonly at: number;
}

interface _Count {
    readonly rule: RuleFile;
    readonly hits: number;
    readonly last: string;
}

interface Family {
    readonly language: string;
    readonly package: string;
    readonly rules: number;
}

interface Utils {
    readonly language: string;
    readonly count: number;
}

interface Rewrites {
    readonly language: string;
    readonly package: string;
    readonly ids: readonly string[];
}

// The live facts of the rule tree the skill prompt opens with, version is the run of ast-grep --version
interface Facts {
    readonly version: Run;
    readonly grammar: boolean;
    readonly families: readonly Family[];
    readonly utils: readonly Utils[];
    readonly rewrites: readonly Rewrites[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _WINDOW_DAYS = 30;
// The window the telemetry block reports and the prune keeps
const SCAN_WINDOW_MS = _WINDOW_DAYS * MS_PER_DAY;
const GRAMMAR_PATH = '.cache/ast-grep/xml.so';
const _TELEMETRY_HEADING = `Rule hits from edit-time scans in the last ${_WINDOW_DAYS} days:`;
const _EVERY_RULE_FIRED = 'Every rule landed before the window fired at least once';
const _GRAMMAR_TARGET = 'pnpm exec nx run rasm:grammar';
// The extensions the rule families' languages read, tsx from languageGlobs, python from its built-in glob, xml from customLanguages
const _FAMILY_EXTENSIONS: readonly string[] = [
    '.ts',
    '.tsx',
    '.mts',
    '.cts',
    '.py',
    '.sh',
    '.bash',
    '.yml',
    '.yaml',
    '.json',
    '.csproj',
    '.props',
    '.targets',
    '.slnx',
    '.nuspec',
    '.pubxml',
    '.proj',
];
const _FAMILY_NAMES: readonly string[] = ['NuGet.config'];
// The rule tree, one directory per kind, the rows and the skill prompt's tree facts read it
const TREE = {
    rules: 'tools/ast-grep/rules',
    rewrites: 'tools/ast-grep/rewrites',
    tests: 'tools/ast-grep/tests',
    utils: 'tools/ast-grep/utils',
} as const;
const _TREE_DIRECTORIES: readonly string[] = [TREE.rules, TREE.rewrites, TREE.tests];
const _TASK_GRAPH_NAMES: readonly string[] = ['nx.json', 'package.json', 'project.json'];
const _TASK_GRAPH_DIRECTORY = 'tools/nx';
// The stem of a test or snapshot file is its rule id
const _TEST_SUFFIX = /-(?:test|snapshot)\.yml$/u;
const _YML = /\.yml$/u;
// The verdict lines of ast-grep test, the set rule-checks.sh keeps and the test result line after it
const _VERDICT = /^(?:test result:|FAIL|SKIP|Configuration not found|Error:|╰▻)/u;
const _GRAMMAR_ABORT = /custom language library/u;
// The language directory of a util, tools/ast-grep/utils/<language>/<file>.yml, the rules its test filter names
const _UTIL_LANGUAGE = /tools\/ast-grep\/utils\/(?<language>[^/]+)\//u;
// The test filter of a util whose language lists no rule id, a regex no id matches, ast-grep reports the id as not found
const _NO_RULE_FILTER = '^$';

// --- [OPERATIONS] ----------------------------------------------------------------------

// The rule id of a tree file, the test or snapshot suffix stripped under the test tree alone and the extension elsewhere, because a rule id
// under rules/ or rewrites/ can itself end in -test
const stem = (path: string): string =>
    fromBoolean(under(path, TREE.tests)).match<string>({
        some: () => basename(path).replace(_TEST_SUFFIX, ''),
        none: () => basename(path).replace(_YML, ''),
    });

const _isTree = (path: string): boolean => extension(path) === '.yml' && _TREE_DIRECTORIES.some((directory) => under(path, directory));

const _isUtil = (path: string): boolean => extension(path) === '.yml' && under(path, TREE.utils);

// The paths whose rows read facts.rules, the util row filters the test run to the ids of the util's language
const needsRuleIds: (path: string) => boolean = _isUtil;

const _utilLanguage = (path: string): Option<string> => fromNullable(_UTIL_LANGUAGE.exec(path)?.groups?.language);

// The ids of the rules under the util's language, none for a path outside the util tree
const _utilIds = (path: string, facts: ScanFacts): readonly string[] =>
    toArray(_utilLanguage(path)).flatMap((language) => facts.rules.filter((rule) => rule.language === language).map((rule) => rule.id));

// The ids as one anchored alternation, the no-rule filter when the language lists none
const _utilFilter = (path: string, facts: ScanFacts): string =>
    getOrElse(() => _NO_RULE_FILTER)(
        map((ids: readonly string[]) => `^(${ids.join('|')})$`)(liftPredicate<readonly string[]>((ids) => ids.length > 0)(_utilIds(path, facts))),
    );

// --- [TREE] ----------------------------------------------------------------------------

const dirs = (entries: readonly FsEntry[]): readonly string[] => entries.filter((entry) => entry.kind === 'dir').map((entry) => entry.name);

// The rule ids of a package directory, the stems of its .yml files under the directory that decides the suffix
const ymlIds = (dir: string, entries: readonly FsEntry[]): readonly string[] =>
    entries.filter((entry) => entry.kind === 'file' && extension(entry.name) === '.yml').map((entry) => stem(`${dir}/${entry.name}`));

// The package directories of one tree root with their ids, through the listing the hook body builds over $.fs.listDir
const packages = async (root: string, list: (dir: string) => Promise<readonly FsEntry[]>): Promise<readonly Package[]> => {
    const languages = dirs(await list(root));
    const pairs = await Promise.all(
        languages.map(async (language) => dirs(await list(`${root}/${language}`)).map((name) => ({ language, package: name }))),
    );
    return Promise.all(
        pairs.flat().map(async (pair): Promise<Package> => {
            const dir = `${root}/${pair.language}/${pair.package}`;
            return { ...pair, ids: ymlIds(dir, await list(dir)) };
        }),
    );
};

const ruleIds = (groups: readonly Package[]): readonly RuleId[] =>
    groups.flatMap((group) => group.ids.map((id): RuleId => ({ id, language: group.language })));

const rulePath = (group: Package, id: string): string => `${TREE.rules}/${group.language}/${group.package}/${id}.yml`;

const ruleFile = (group: Package, id: string, mtimeMs: number): RuleFile => ({ id, language: group.language, package: group.package, mtimeMs });

const family = (group: Package): Family => ({ language: group.language, package: group.package, rules: group.ids.length });

const _isRange: (value: unknown) => value is _Range = struct({ start: struct({ line: isNumber }) });

const _isCompactHit: (value: unknown) => value is _CompactHit = struct({
    ruleId: isString,
    file: isString,
    message: isString,
    note: optional(isNullableString),
    range: _isRange,
    replacement: optional(isString),
});

const _hit = (compact: _CompactHit): Hit => ({
    ruleId: compact.ruleId,
    file: compact.file,
    line: compact.range.start.line + 1,
    note: compact.note ?? compact.message,
    replacement: fromPredicate(isString)(compact.replacement),
});

// The JSON array a command printed, a malformed or non-array stdout reads as empty
const _array = (stdout: string): readonly unknown[] =>
    getOrElse((): readonly unknown[] => [])(flatMap(fromPredicate(Array.isArray))(decodeJson(stdout)));

// The hits of a scan's stdout, one per element that holds the compact fields
const scanHits = (run: Run): readonly Hit[] => _array(run.stdout).flatMap((value) => toArray(map(_hit)(fromPredicate(_isCompactHit)(value))));

const _quote = (value: string): string => `'${value.replaceAll("'", "'\"'\"'")}'`;

const _hitLine = (hit: Hit): string =>
    `${hit.file}:${hit.line} ${hit.ruleId}: ${hit.note}${getOrElse(() => '')(
        map(() => `, fix: nx run rasm:rewrite -- --filter=${_quote(`^${hit.ruleId}$`)} --error=${_quote(hit.ruleId)} ${_quote(hit.file)}`)(
            hit.replacement,
        ),
    )}`;

// The run of a child that did not start, the rejection of $.process.run read as an exit -1 with the error as stderr
const abort = (error: unknown): Run => ({ exitCode: -1, stdout: '', stderr: String(error) });

// The verdict lines of a test run under a prefix, stdout then stderr, the Error line of an unknown id among them
const _testLines = (run: Run, prefix: string): readonly string[] =>
    [...lines(run.stdout), ...lines(run.stderr)].filter((line) => _VERDICT.test(line)).map((line) => `${prefix}${line}`);

const _affected = (run: Run): readonly string[] => _array(run.stdout).filter(isString);

// The one line of a pairing run that printed nothing, no finding on exit 0 and the exit code otherwise
const _pairingSilent = (run: Run): string =>
    fromBoolean(run.exitCode === 0).match<string>({
        some: () => 'rule-checks.sh pairing: no finding',
        none: () => `rule-checks.sh pairing exited ${run.exitCode}`,
    });

// --- [TELEMETRY] -----------------------------------------------------------------------

// The hit time of a scan/ key, the id under the namespace read through stampOf
const _hitStamp = (storeKey: string): Option<number> => stampOf(suffix('scan')(storeKey));

const _known = (rules: readonly RuleFile[]): ((row: Scan) => Option<Scan>) =>
    liftPredicate<Scan>((row) => rules.some((rule) => rule.id === row.ruleId));

const _inWindow = (now: number): ((at: number) => Option<number>) => liftPredicate<number>((at) => now - at <= SCAN_WINDOW_MS);

const _liveHit = (entry: Entry, rules: readonly RuleFile[], now: number): Option<_Live> =>
    flatMap((row: Scan) =>
        map((at: number): _Live => ({ key: entry.key, ruleId: row.ruleId, file: row.file, at }))(flatMap(_inWindow(now))(_hitStamp(entry.key))),
    )(flatMap(_known(rules))(decodeScan(entry.value)));

// The live hits newest first, the first hit of a rule is its last file
const _live = (entries: readonly Entry[], rules: readonly RuleFile[], now: number): readonly _Live[] =>
    entries.flatMap((entry) => toArray(_liveHit(entry, rules, now))).toSorted((left, right) => right.at - left.at);

// Whether a scan/ key's hit stamp lies past the window, a key with no stamp is past it as stale reads it
const _expired =
    (now: number): ((storeKey: string) => boolean) =>
    (storeKey: string): boolean =>
        _hitStamp(storeKey).match<boolean>({ some: (at) => now - at > SCAN_WINDOW_MS, none: () => true });

// The scan/ keys past the window, the prune session.start runs with no rule listing
const expiredScans = (all: readonly string[], now: number): readonly string[] => keys('scan')(all).filter(_expired(now));

// The scan/ keys to delete, every entry that is not a live hit
const stale = (entries: readonly Entry[], rules: readonly RuleFile[], now: number): readonly string[] => {
    const live: ReadonlySet<string> = new Set(_live(entries, rules, now).map((hit) => hit.key));
    return entries.map((entry) => entry.key).filter((storeKey) => !live.has(storeKey));
};

// One count per rule with a live hit, by count descending then id
const _counts = (live: readonly _Live[], rules: readonly RuleFile[]): readonly _Count[] =>
    rules
        .flatMap((rule) => {
            const own = live.filter((hit) => hit.ruleId === rule.id);
            return toArray(map((newest: _Live): _Count => ({ rule, hits: own.length, last: newest.file }))(fromNullable(own[0])));
        })
        .toSorted((left, right) => right.hits - left.hits || left.rule.id.localeCompare(right.rule.id));

const _rulePath = (rule: RuleFile): string => `${rule.language}/${rule.package}/${rule.id}`;

// Rules older than the window that no live hit names, the candidates for a dead rule
const _silent = (live: readonly _Live[], rules: readonly RuleFile[], now: number): readonly string[] =>
    rules
        .filter((rule) => now - rule.mtimeMs > SCAN_WINDOW_MS && !live.some((hit) => hit.ruleId === rule.id))
        .map((rule) => rule.id)
        .toSorted((left, right) => left.localeCompare(right));

// The block appended after the skill text, none without a rule file
const telemetryBlock = (entries: readonly Entry[], rules: readonly RuleFile[], now: number): Option<string> =>
    map((listed: readonly RuleFile[]): string => {
        const live = _live(entries, listed, now);
        return [
            _TELEMETRY_HEADING,
            ..._counts(live, listed).map((count) => `- ${_rulePath(count.rule)}: ${count.hits} hits, last ${count.last}`),
            getOrElse(() => _EVERY_RULE_FIRED)(
                map((ids: readonly string[]) => `Rules with no hit in the window, landed before it: ${ids.join(', ')}`)(
                    liftPredicate<readonly string[]>((ids) => ids.length > 0)(_silent(live, listed, now)),
                ),
            ),
        ].join('\n');
    })(liftPredicate<readonly RuleFile[]>((listed) => listed.length > 0)(rules));

// --- [FACTS] ---------------------------------------------------------------------------

const _byLanguage = <T extends { readonly language: string }>(left: T, right: T): number => left.language.localeCompare(right.language);

const _byPackage = <T extends { readonly language: string; readonly package: string }>(left: T, right: T): number =>
    _byLanguage(left, right) || left.package.localeCompare(right.package);

const _rewriteIds = (rewrites: readonly Rewrites[]): string =>
    rewrites
        .toSorted(_byPackage)
        .flatMap((group) => group.ids.toSorted((left, right) => left.localeCompare(right)).map((id) => `${group.language}/${group.package} ${id}`))
        .join(', ');

// The block prepended before the skill text, one fact per line
const factsBlock = (facts: Facts): string =>
    [
        getOrElse(() => `ast-grep --version exited ${facts.version.exitCode}: ${first(facts.version.stderr)}`)(
            liftPredicate<string>(() => facts.version.exitCode === 0)(`${first(facts.version.stdout)} under sgconfig.yml`),
        ),
        fromBoolean(facts.grammar).match<string>({
            some: () => `Custom grammar ${GRAMMAR_PATH} present`,
            none: () => `Custom grammar ${GRAMMAR_PATH} absent, every scan aborts until ${_GRAMMAR_TARGET} writes it`,
        }),
        `Rules per family: ${facts.families
            .toSorted(_byPackage)
            .map((group) => `${group.language}/${group.package} ${group.rules}`)
            .join(', ')}`,
        `Utils per language: ${facts.utils
            .toSorted(_byLanguage)
            .map((utils) => `${utils.language} ${utils.count}`)
            .join(', ')}`,
        `Rewrites: ${getOrElse(() => 'none')(liftPredicate<string>((ids) => ids !== '')(_rewriteIds(facts.rewrites)))}`,
    ].join('\n');

// --- [ROWS] ----------------------------------------------------------------------------

const SCAN = [
    {
        match: (path): boolean =>
            !(_isTree(path) || _isUtil(path)) && (_FAMILY_EXTENSIONS.includes(extension(path)) || _FAMILY_NAMES.includes(basename(path))),
        argv: (path): readonly string[] => ['ast-grep', 'scan', '--json=compact', path],
        // Successful scans can report warnings on exit 0 or errors on exit 1
        lines: (run, path): readonly string[] =>
            fromBoolean(run.exitCode === 0 || run.exitCode === 1).match<readonly string[]>({
                some: () =>
                    getOrElse(() => [`ast-grep scan: no hit in ${path}`])(
                        map((hits: readonly Hit[]) => hits.map(_hitLine))(liftPredicate<readonly Hit[]>((hits) => hits.length > 0)(scanHits(run))),
                    ),
                none: () => [
                    `ast-grep scan exited ${run.exitCode}: ${first(run.stderr)}${getOrElse(() => '')(
                        liftPredicate<string>(() => _GRAMMAR_ABORT.test(run.stderr))(`, run ${_GRAMMAR_TARGET}`),
                    )}`,
                ],
            }),
    },
    {
        match: _isTree,
        argv: (path): readonly string[] => ['ast-grep', 'test', '--include-off', '--filter', `^${stem(path)}$`],
        lines: (run, path): readonly string[] => _testLines(run, `ast-grep test ${stem(path)}: `),
    },
    // A util names no test of its own, the rules of its language are the tests its edit can break
    {
        match: _isUtil,
        argv: (path, facts): readonly string[] => ['ast-grep', 'test', '--include-off', '--filter', _utilFilter(path, facts)],
        lines: (run): readonly string[] => _testLines(run, 'ast-grep test: '),
    },
    // The pairing check reads the whole tree in under a second, every edit under it runs the one form
    {
        match: (path): boolean => _isTree(path) || _isUtil(path),
        argv: (): readonly string[] => ['.claude/skills/ast-grep/scripts/rule-checks.sh', 'pairing'],
        lines: (run): readonly string[] =>
            getOrElse((): readonly string[] => [_pairingSilent(run)])(
                liftPredicate<readonly string[]>((found) => found.length > 0)([...lines(run.stdout), ...lines(run.stderr)]),
            ),
    },
    // Paths outside the working directory keep their absolute form and belong to no project, nx refuses an absolute --files value
    {
        match: (path): boolean => !path.startsWith('/') && (_TASK_GRAPH_NAMES.includes(basename(path)) || under(path, _TASK_GRAPH_DIRECTORY)),
        argv: (path): readonly string[] => ['pnpm', 'exec', 'nx', 'show', 'projects', '--affected', '--json', `--files=${path}`],
        lines: (run): readonly string[] =>
            getOrElse((): readonly string[] => [`nx show projects exited ${run.exitCode}: ${first(run.stderr)}`])(
                map((names: readonly string[]) => [`Affected projects (${names.length}): ${names.join(', ')}`])(
                    liftPredicate<readonly string[]>(() => run.exitCode === 0)(_affected(run)),
                ),
            ),
    },
] as const satisfies readonly ScanRow[];

// --- [RULES] ---------------------------------------------------------------------------

// Every row the path matches, a tools/nx TypeScript file takes the family scan and the affected projects
const scanRows = (path: string, facts: ScanFacts): readonly ScanRow[] => SCAN.filter((row: ScanRow) => row.match(path, facts));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Facts, Family, Hit, Package, Rewrites, RuleFile, RuleId, Run, ScanFacts, ScanRow, Utils };
export {
    abort,
    dirs,
    expiredScans,
    factsBlock,
    family,
    GRAMMAR_PATH,
    needsRuleIds,
    packages,
    ruleFile,
    ruleIds,
    rulePath,
    SCAN,
    SCAN_WINDOW_MS,
    scanHits,
    scanRows,
    stale,
    stem,
    TREE,
    telemetryBlock,
    ymlIds,
};
