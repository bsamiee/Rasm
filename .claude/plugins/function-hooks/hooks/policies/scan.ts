// Scan rows over an edited path, each a command and the lines its run yields, with the rule-hit telemetry the ast-grep skill reads

// --- [IMPORTS] -------------------------------------------------------------------------

import type { FsEntry, OpValueOf } from 'claude-code';
import { decodeJson, type Entry, isNumber, isScan, isString, keys, optional, stampOf, struct, suffix } from '../host/store.ts';
import { first, lines } from '../text/lines.ts';
import { basename, extension, under } from '../text/path.ts';
import { MS_PER_DAY } from './findings.ts';

// --- [TYPES] ---------------------------------------------------------------------------

// What $.process.run resolves with, ProcessRunResult in the declarations under a name they do not export
type Run = OpValueOf['process.run'];

// The directory listing the hook body builds over $.fs.listDir
type Listing = (dir: string) => Promise<readonly FsEntry[]>;

// One command per row over the paths it matches, the lines are the context the model reads after the edit
interface ScanRow {
    readonly match: (path: string) => boolean;
    readonly argv: (path: string, ruleIds: readonly string[]) => readonly string[];
    readonly lines: (run: Run, path: string) => readonly string[];
}

// One hit of ast-grep scan --json=compact, the fields the lines and the store row read
interface Hit {
    readonly ruleId: string;
    readonly file: string;
    readonly line: number;
    readonly note: string;
    readonly replacement: string | undefined;
}

// One package directory of the rule or rewrite tree, tools/ast-grep/<kind>/<language>/<package>/<id>.yml
interface Package {
    readonly language: string;
    readonly package: string;
    readonly ids: readonly string[];
}

// A package directory before its ids are listed
interface PackageDir extends Omit<Package, 'ids'> {
    readonly dir: string;
}

// The live facts of the rule tree the skill prompt opens with, version is the run of ast-grep --version
interface Facts {
    readonly version: Run;
    readonly grammar: boolean;
    readonly rules: readonly Package[];
    readonly rewrites: readonly Package[];
    readonly utils: readonly Package[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _WINDOW_DAYS = 30;
// The window the telemetry block reports and the prune keeps
const SCAN_WINDOW_MS = _WINDOW_DAYS * MS_PER_DAY;
const GRAMMAR_PATH = '.cache/ast-grep/xml.so';
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
// The rule tree, one directory per kind
const TREE = {
    rules: 'tools/ast-grep/rules',
    rewrites: 'tools/ast-grep/rewrites',
    tests: 'tools/ast-grep/tests',
    utils: 'tools/ast-grep/utils',
} as const;
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

const _isCompactHit = struct({
    ruleId: isString,
    file: isString,
    message: isString,
    note: optional((value: unknown): value is string | null => value === null || isString(value)),
    range: struct({ start: struct({ line: isNumber }) }),
    replacement: optional(isString),
});

// --- [TREE] ----------------------------------------------------------------------------

// The rule id of a tree file, the test or snapshot suffix stripped under the test tree alone and the extension elsewhere, because a rule id
// under rules/ or rewrites/ can itself end in -test
const stem = (path: string): string => basename(path).replace(under(path, TREE.tests) ? _TEST_SUFFIX : _YML, '');

const _isTree = (path: string): boolean =>
    extension(path) === '.yml' && [TREE.rules, TREE.rewrites, TREE.tests].some((directory) => under(path, directory));

const _isUtil = (path: string): boolean => extension(path) === '.yml' && under(path, TREE.utils);

// The language of a util path, the adapter lists that language's rules for the util row's filter
const utilLanguage = (path: string): string | undefined => (_isUtil(path) ? _UTIL_LANGUAGE.exec(path)?.groups?.language : undefined);

const _dirs = (entries: readonly FsEntry[]): readonly string[] => entries.filter((entry) => entry.kind === 'dir').map((entry) => entry.name);

// The rule ids of a package directory, the stems of its .yml files under the directory that decides the suffix
const _ids = (dir: string, entries: readonly FsEntry[]): readonly string[] =>
    entries.filter((entry) => entry.kind === 'file' && extension(entry.name) === '.yml').map((entry) => stem(`${dir}/${entry.name}`));

// The package directories of one tree root, tools/ast-grep/<kind>/<language>/<package>, and under utils the language directory alone
const _packageDirs = async (root: string, list: Listing, languages: readonly string[]): Promise<readonly PackageDir[]> => {
    if (root === TREE.utils) {
        return languages.map((name) => ({ language: name, package: name, dir: `${root}/${name}` }));
    }
    const listed = await Promise.all(
        languages.map(async (name) =>
            _dirs(await list(`${root}/${name}`)).map((dir) => ({ language: name, package: dir, dir: `${root}/${name}/${dir}` })),
        ),
    );
    return listed.flat();
};

// The packages of one tree root with their rule ids, through the listing the hook body builds over $.fs.listDir
const packages = async (root: string, list: Listing, language?: string): Promise<readonly Package[]> => {
    const dirs = await _packageDirs(root, list, language === undefined ? _dirs(await list(root)) : [language]);
    return Promise.all(dirs.map(async ({ dir, ...group }): Promise<Package> => ({ ...group, ids: _ids(dir, await list(dir)) })));
};

// --- [HITS] ----------------------------------------------------------------------------

// The JSON array a command printed, a malformed or non-array stdout reads as empty
const _array = (stdout: string): readonly unknown[] => {
    const value = decodeJson(stdout);
    return Array.isArray(value) ? value : [];
};

// The hits of a scan's stdout, one per element that holds the compact fields
const scanHits = (run: Run): readonly Hit[] =>
    _array(run.stdout).flatMap((value) =>
        _isCompactHit(value)
            ? [
                  {
                      ruleId: value.ruleId,
                      file: value.file,
                      line: value.range.start.line + 1,
                      note: value.note ?? value.message,
                      replacement: value.replacement,
                  },
              ]
            : [],
    );

const _quote = (value: string): string => `'${value.replaceAll("'", "'\"'\"'")}'`;

const _hitLine = (hit: Hit): string =>
    `${hit.file}:${hit.line} ${hit.ruleId}: ${hit.note}${
        hit.replacement === undefined
            ? ''
            : `, fix: nx run rasm:rewrite -- --filter=${_quote(`^${hit.ruleId}$`)} --error=${_quote(hit.ruleId)} ${_quote(hit.file)}`
    }`;

// The run of a child that did not start, the rejection of $.process.run read as an exit -1 with the error as stderr
const abort = (error: unknown): Run => ({ exitCode: -1, stdout: '', stderr: String(error) });

// The verdict lines of a test run under a prefix, stdout then stderr, the Error line of an unknown id among them
const _testLines = (run: Run, prefix: string): readonly string[] =>
    [...lines(run.stdout), ...lines(run.stderr)].filter((line) => _VERDICT.test(line)).map((line) => `${prefix}${line}`);

// --- [TELEMETRY] -----------------------------------------------------------------------

// The hit time of a scan/ key, the id under the namespace read through stampOf
const _hitStamp = (storeKey: string): number | undefined => stampOf(suffix('scan')(storeKey));

// The scan/ keys past the window, a key with no stamp is past it
const expiredScans = (all: readonly string[], now: number): readonly string[] =>
    keys('scan')(all).filter((storeKey) => {
        const at = _hitStamp(storeKey);
        return at === undefined || now - at > SCAN_WINDOW_MS;
    });

// The block appended after the skill text, undefined without a rule id
const telemetryBlock = (entries: readonly Entry[], ruleIds: readonly string[], now: number): string | undefined => {
    if (ruleIds.length === 0) {
        return undefined;
    }
    const live = entries
        .flatMap((entry) => {
            const at = _hitStamp(entry.key);
            return isScan(entry.value) && ruleIds.includes(entry.value.ruleId) && at !== undefined && now - at <= SCAN_WINDOW_MS
                ? [{ ...entry.value, at }]
                : [];
        })
        .toSorted((left, right) => right.at - left.at);
    const counts = ruleIds
        .flatMap((ruleId) => {
            const own = live.filter((hit) => hit.ruleId === ruleId);
            const [newest] = own;
            return newest === undefined ? [] : [{ ruleId, hits: own.length, last: newest.file }];
        })
        .toSorted((left, right) => right.hits - left.hits || left.ruleId.localeCompare(right.ruleId));
    const silent = ruleIds.filter((ruleId) => !live.some((hit) => hit.ruleId === ruleId)).toSorted((left, right) => left.localeCompare(right));
    return [
        `Rule hits from edit-time scans in the last ${_WINDOW_DAYS} days:`,
        ...counts.map((count) => `- ${count.ruleId}: ${count.hits} hits, last ${count.last}`),
        silent.length > 0 ? `Rules with no hit in the window: ${silent.join(', ')}` : 'Every rule fired at least once in the window',
    ].join('\n');
};

// --- [FACTS] ---------------------------------------------------------------------------

const _byPackage = (left: Package, right: Package): number =>
    left.language.localeCompare(right.language) || left.package.localeCompare(right.package);

const _perPackage = (groups: readonly Package[], count: (group: Package) => string): string =>
    groups
        .toSorted(_byPackage)
        .map((group) => `${group.language}/${group.package} ${count(group)}`)
        .join(', ');

// The block prepended before the skill text, one fact per line
const factsBlock = (facts: Facts): string =>
    [
        facts.version.exitCode === 0
            ? `${first(facts.version.stdout)} under sgconfig.yml`
            : `ast-grep --version exited ${facts.version.exitCode}: ${first(facts.version.stderr)}`,
        facts.grammar
            ? `Custom grammar ${GRAMMAR_PATH} present`
            : `Custom grammar ${GRAMMAR_PATH} absent, every scan aborts until ${_GRAMMAR_TARGET} writes it`,
        `Rules per family: ${_perPackage(facts.rules, (group) => String(group.ids.length))}`,
        `Utils per language: ${facts.utils
            .toSorted(_byPackage)
            .map((group) => `${group.language} ${group.ids.length}`)
            .join(', ')}`,
        `Rewrites: ${facts.rewrites.length > 0 ? _perPackage(facts.rewrites, (group) => group.ids.toSorted().join(' ')) : 'none'}`,
    ].join('\n');

// --- [ROWS] ----------------------------------------------------------------------------

const SCAN = [
    {
        match: (path): boolean =>
            !(_isTree(path) || _isUtil(path)) && (_FAMILY_EXTENSIONS.includes(extension(path)) || _FAMILY_NAMES.includes(basename(path))),
        argv: (path): readonly string[] => ['ast-grep', 'scan', '--json=compact', path],
        // Successful scans can report warnings on exit 0 or errors on exit 1
        lines: (run, path): readonly string[] => {
            if (run.exitCode !== 0 && run.exitCode !== 1) {
                return [
                    `ast-grep scan exited ${run.exitCode}: ${first(run.stderr)}${_GRAMMAR_ABORT.test(run.stderr) ? `, run ${_GRAMMAR_TARGET}` : ''}`,
                ];
            }
            const hits = scanHits(run);
            return hits.length > 0 ? hits.map(_hitLine) : [`ast-grep scan: no hit in ${path}`];
        },
    },
    {
        match: _isTree,
        argv: (path): readonly string[] => ['ast-grep', 'test', '--include-off', '--filter', `^${stem(path)}$`],
        lines: (run, path): readonly string[] => _testLines(run, `ast-grep test ${stem(path)}: `),
    },
    // A util names no test of its own, the rules of its language are the tests its edit can break, and ^$ matches no id when it lists none
    {
        match: _isUtil,
        argv: (_path, ruleIds): readonly string[] => [
            'ast-grep',
            'test',
            '--include-off',
            '--filter',
            ruleIds.length > 0 ? `^(${ruleIds.join('|')})$` : '^$',
        ],
        lines: (run): readonly string[] => _testLines(run, 'ast-grep test: '),
    },
    // The pairing check reads the whole tree in under a second, every edit under it runs the one form
    {
        match: (path): boolean => _isTree(path) || _isUtil(path),
        argv: (): readonly string[] => ['.claude/skills/ast-grep/scripts/rule-checks.sh', 'pairing'],
        lines: (run): readonly string[] => {
            const printed = [...lines(run.stdout), ...lines(run.stderr)];
            if (printed.length > 0) {
                return printed;
            }
            return [run.exitCode === 0 ? 'rule-checks.sh pairing: no finding' : `rule-checks.sh pairing exited ${run.exitCode}`];
        },
    },
    // Paths outside the working directory keep their absolute form and belong to no project, nx refuses an absolute --files value
    {
        match: (path): boolean => !path.startsWith('/') && (_TASK_GRAPH_NAMES.includes(basename(path)) || under(path, _TASK_GRAPH_DIRECTORY)),
        argv: (path): readonly string[] => ['pnpm', 'exec', 'nx', 'show', 'projects', '--affected', '--json', `--files=${path}`],
        lines: (run): readonly string[] => {
            const names = _array(run.stdout).filter(isString);
            return run.exitCode === 0
                ? [`Affected projects (${names.length}): ${names.join(', ')}`]
                : [`nx show projects exited ${run.exitCode}: ${first(run.stderr)}`];
        },
    },
] as const satisfies readonly ScanRow[];

// Every row the path matches, a tools/nx TypeScript file takes the family scan and the affected projects
const scanRows = (path: string): readonly ScanRow[] => SCAN.filter((row: ScanRow) => row.match(path));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Facts, Hit, Package, Run, ScanRow };
export {
    abort,
    expiredScans,
    factsBlock,
    GRAMMAR_PATH,
    packages,
    SCAN,
    SCAN_WINDOW_MS,
    scanHits,
    scanRows,
    stem,
    TREE,
    telemetryBlock,
    utilLanguage,
};
