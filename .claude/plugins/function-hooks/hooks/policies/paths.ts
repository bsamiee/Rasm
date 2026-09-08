// Path rows over Read, Edit, Write, and NotebookEdit for binary denies, secret content denies, skill context once per session, and per-edit context

// --- [IMPORTS] -------------------------------------------------------------------------

import type { BuiltinToolResults, ToolCallInput } from 'claude-code';
import { answer, type Decision, deny, fold, type Rule, rewrite } from '../composition/decision.ts';
import { fromBoolean, fromNullable, fromPredicate, getOrElse, liftPredicate, map, type Option, toArray } from '../composition/option.ts';
import { isString } from '../host/store.ts';
import { first, lines } from '../text/lines.ts';
import { basename, extension, relative, under } from '../text/path.ts';
import type { Run } from './scan.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type PathTool = 'Read' | 'Edit' | 'Write' | 'NotebookEdit';

// The tool variants as the declarations state them, NotebookEdit names its file under notebook_path and its text under new_source
type PathEvent = Extract<ToolCallInput, { readonly tool: PathTool }>;

type Field = 'file_path' | 'notebook_path' | 'content' | 'new_string' | 'new_source' | 'old_string';

// The Write result as the declarations state it, the shape core validates a hook's answer against
type WriteResult = BuiltinToolResults['Write'];

// An answered Write the adapter lands through a child in the engine's place, the argv with the text on its stdin and the result answered
interface Landing {
    readonly argv: readonly string[];
    readonly stdin: string;
    readonly result: WriteResult;
}

// Context lines injected once per session under their key, the skill name or the tool they name
interface Once {
    readonly key: string;
    readonly line: string;
}

// The rg search over the sibling records of a dropped dependency row, one per name the edit drops
interface Search {
    readonly name: string;
    readonly argv: readonly string[];
}

// A search with the child's answer, the holder paths on stdout relative to the working directory
interface Searched extends Search {
    readonly run: Run;
}

// A dependency record: the row pattern with its name group, the sibling record globs, and the regex a name is held by in them
interface DependencyRecord {
    readonly row: RegExp;
    readonly globs: readonly string[];
    readonly held: (name: string) => string;
}

interface PathRow {
    readonly match: (path: string, text: string, old: string) => boolean;
    readonly tools: readonly PathTool[];
    readonly deny?: (path: string) => string;
    readonly answer?: (path: string, text: string) => WriteResult;
    readonly once?: readonly Once[];
    readonly each?: (path: string, text: string, old: string, facts: PathFacts) => readonly string[];
}

// The searches are the dropped rows' runs
interface PathFacts {
    readonly seen: ReadonlySet<string>;
    readonly searches: readonly Searched[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const BINLOG_DENY = '.binlog files are binary, call mcp__binlog__binlog_overview on the file';
const OP_DENY = 'Secrets come from Doppler alone, an op:// reference never lands in a file, use doppler secrets';
const _ALL: readonly PathTool[] = ['Read', 'Edit', 'Write'];
const _WRITES: readonly PathTool[] = ['Edit', 'Write'];
const _WRITE: readonly PathTool[] = ['Write'];
// The directory subagents write records under, the engine refuses a subagent Write of these names beneath next and passes every other
const _SCRATCH = '.claude/scratch';
const _REPORT_NAMES: readonly string[] = ['report.md', 'summary.md', 'findings.md'];
// The child that lands a record where $.fs.writeFile refuses a .claude path, the previous text printed after the update line, then the
// directory made and the text read from stdin, one child for the engine's update answer
const _LAND = 'if [ -e "$1" ]; then echo update; cat -- "$1"; else echo create; fi && mkdir -p "$(dirname "$1")" && cat > "$1"';
const _UPDATE = 'update\n';
// Every tool that lands text in a file, the content rows read the text whatever the file
const _CONTENT_WRITES: readonly PathTool[] = ['Edit', 'Write', 'NotebookEdit'];
const _OP = /op:\/\/|\bop\s+(?:read|inject|item|run)\b/gu;
const _TARGET = /<Target\b/u;
const _PACKAGE_REFERENCE = /PackageReference/u;
const _VERSION_ATTRIBUTE = /Version=/u;
const _TOOL_VERSION = /[=]\s*"\d[^"]*"/u;
const _PIN = /[=]=\d|"\s*:\s*"[~^]?\d/u;
const _README = 'README.md';
// The rg exits of a finished search, 0 with holders and 1 with none, any other exit is a failed child
const _SEARCH_EXITS: readonly number[] = [0, 1];
const _REGEX_SPECIAL = /[\\^$.*+?()[\]{}|]/gu;
// The separators a PEP 503 normalization folds, uv.lock spells a name normalized
const _NAME_JOIN = /[-_.]+/gu;
const _TYPESCRIPT_GLOBS: readonly string[] = ['package.json', 'pnpm-workspace.yaml'];
const _escape = (name: string): string => name.replace(_REGEX_SPECIAL, '\\$&');
const _typescriptHeld = (name: string): string => `^\\s+'?${_escape(name)}'?:|^\\s*"${_escape(name)}":\\s*"`;
// One dependency record per manifest, its rows named in the old and the new text to tell an added row from a dropped one
const _RECORDS: Readonly<Partial<Record<string, DependencyRecord>>> = {
    'Directory.Packages.props': {
        row: /<PackageVersion\s+Include="(?<name>[^"]+)"/gu,
        globs: ['*.csproj', '*.props', '*.targets'],
        held: (name) => `PackageReference\\s+Include="${_escape(name)}"`,
    },
    'package.json': { row: /^\s*"(?<name>[^"]+)":\s*"(?:catalog:|workspace:|[~^]?\d)/gmu, globs: _TYPESCRIPT_GLOBS, held: _typescriptHeld },
    'pnpm-workspace.yaml': { row: /^\s{2,}'?(?<name>[\w@/.-]+)'?:\s*\S/gmu, globs: _TYPESCRIPT_GLOBS, held: _typescriptHeld },
    'pyproject.toml': {
        row: /^\s*"(?<name>[\w.-]+)[\w.[\],=<>!~ -]*"\s*,?\s*$/gmu,
        globs: ['pyproject.toml', 'uv.lock'],
        held: (name) => `^\\s*"${name.replace(_NAME_JOIN, '[-_.]+')}[^\\w.-]|^name = "${name.replace(_NAME_JOIN, '[-_.]+')}"`,
    },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const _skill = (name: string): Once => ({ key: name, line: `Load the ${name} skill` });

const _names = (text: string, row: RegExp): readonly string[] => [...text.matchAll(row)].flatMap((hit) => toArray(fromNullable(hit.groups?.name)));

// The distinct names in text and not in other, the added rows as (new, old) and the dropped rows as (old, new)
const _only = (text: string, other: string, row: RegExp): readonly string[] => {
    const known = new Set(_names(other, row));
    return [...new Set(_names(text, row))].filter((name) => !known.has(name));
};

// String fields of any variant, empty where the variant lacks them
const _field = (e: PathEvent, name: Field): string => {
    const fields: Readonly<Record<string, unknown>> = e;
    return getOrElse(() => '')(fromPredicate(isString)(fields[name]));
};

// The file a row reads, file_path on Read, Edit, and Write, and notebook_path on NotebookEdit
const _path = (e: PathEvent): string => _field(e, 'file_path') || _field(e, 'notebook_path');

// The text a row reads, the new content of a Write, the replacement of an Edit, the cell source of a NotebookEdit, and nothing for a Read
const _text = (e: PathEvent): string => _field(e, 'content') || _field(e, 'new_string') || _field(e, 'new_source');

const _old = (e: PathEvent): string => _field(e, 'old_string');

const _references = (text: string): number => [...text.matchAll(_OP)].length;

// The record answered as a created file with no patch, the fields BuiltinToolResults.Write requires
const _writeResult = (path: string, text: string): WriteResult => ({
    type: 'create',
    filePath: path,
    content: text,
    structuredPatch: [],
    originalFile: null,
});

// The record as the child landed it, an update over the previous text after the update line, else the created record as answered
const landed = (result: WriteResult, stdout: string): WriteResult =>
    fromBoolean(stdout.startsWith(_UPDATE)).match<WriteResult>({
        some: () => ({ ...result, type: 'update', originalFile: stdout.slice(_UPDATE.length) }),
        none: () => result,
    });

// The lines of a dropped name's search: a failed child, or the holders that keep the drop from being a removal
const _droppedLines = (name: string, searches: readonly Searched[]): readonly string[] =>
    searches
        .filter((search) => search.name === name)
        .flatMap((search) => [
            ...toArray(
                liftPredicate<string>(() => !_SEARCH_EXITS.includes(search.run.exitCode))(
                    `The record search for ${name} failed, rg exited ${search.run.exitCode}: ${first(search.run.stderr)}`,
                ),
            ),
            ...toArray(
                liftPredicate<string>(() => lines(search.run.stdout).length > 0)(
                    `${name} remains in ${lines(search.run.stdout).join(', ')}, keep the dropped row and add the missing dependency record, or remove it there too`,
                ),
            ),
        ]);

const _dependencyLines = (path: string, text: string, old: string, facts: PathFacts): readonly string[] =>
    toArray(fromNullable(_RECORDS[basename(path)])).flatMap((record) => [
        ...toArray(
            liftPredicate<string>(() => _only(text, old, record.row).length > 0)('Record the dependency in the owning README.md dependency list'),
        ),
        ..._only(old, text, record.row).flatMap((name) => _droppedLines(name, facts.searches)),
    ]);

// One rg search per dropped row over the sibling records of its language and every README.md, the edited file excluded by the last glob
const recordSearches = (e: PathEvent, cwd: string): readonly Search[] =>
    toArray(fromNullable(_RECORDS[basename(_path(e))])).flatMap((record) =>
        _only(_old(e), _text(e), record.row).map((name) => ({
            name,
            argv: [
                'rg',
                '--files-with-matches',
                '--ignore-case',
                '--hidden',
                ...[...record.globs, _README].flatMap((glob) => ['--glob', glob]),
                '--glob',
                `!/${relative(cwd, _path(e))}`,
                '--regexp',
                `${record.held(name)}|\`${_escape(name)}\``,
            ],
        })),
    );

// --- [ROWS] ----------------------------------------------------------------------------

const PATHS = [
    {
        match: (path): boolean => extension(path) === '.binlog',
        tools: _ALL,
        deny: (): string => BINLOG_DENY,
    },
    {
        // A reference the write adds, the harness tree documents and tests the rule
        match: (path, text, old): boolean => _references(text) > _references(old) && !under(path, '.claude'),
        tools: _CONTENT_WRITES,
        deny: (): string => OP_DENY,
    },
    {
        match: (path): boolean => under(path, _SCRATCH) && _REPORT_NAMES.includes(basename(path).toLowerCase()),
        tools: _WRITE,
        answer: _writeResult,
    },
    { match: (path): boolean => extension(path) === '.cs', tools: _ALL, once: [_skill('dotnet-roslyn-codelens'), _skill('dotnet-coding')] },
    {
        match: (path): boolean => ['.csproj', '.props', '.targets'].includes(extension(path)),
        tools: _ALL,
        once: [_skill('dotnet-msbuild-evaluation'), _skill('dotnet-msbuild-antipatterns')],
    },
    {
        match: (path, text): boolean => ['.csproj', '.props', '.targets'].includes(extension(path)) && _TARGET.test(text),
        tools: _ALL,
        once: [_skill('dotnet-msbuild-execution')],
    },
    {
        match: (path, text): boolean =>
            ['Directory.Packages.props', 'NuGet.config'].includes(basename(path)) || extension(path) === '.slnx' || _PACKAGE_REFERENCE.test(text),
        tools: _ALL,
        once: [_skill('dotnet-msbuild-packaging')],
    },
    {
        match: (path): boolean =>
            ['eng', 'infra', 'tools', '.github'].some((directory) => under(path, directory)) ||
            ['mise.toml', 'mise.unix.toml', '.miserc.toml', 'nx.json'].includes(basename(path)),
        tools: _ALL,
        once: [_skill('manage-repo')],
    },
    { match: (path): boolean => under(path, 'infra'), tools: _ALL, once: [_skill('pulumi')] },
    {
        match: (path): boolean => under(path, '.claude'),
        tools: _ALL,
        once: [{ key: 'claudeCodeDocs', line: 'Search the docs with mcp__claudeCodeDocs__search_claude_code_docs' }],
    },
    {
        match: (path): boolean => basename(path).startsWith('.env') || basename(path) === 'doppler.yaml',
        tools: _ALL,
        once: [_skill('secrets')],
    },
    {
        match: (path, text): boolean => basename(path) === 'Directory.Packages.props' && _VERSION_ATTRIBUTE.test(text),
        tools: _WRITES,
        each: (): readonly string[] => ['Verify the row with mcp__nuget__get_package_context'],
    },
    {
        match: (path): boolean => _RECORDS[basename(path)] !== undefined,
        tools: _WRITES,
        each: _dependencyLines,
    },
    {
        match: (path, text): boolean => basename(path) === 'mise.toml' && _TOOL_VERSION.test(text),
        tools: _WRITES,
        each: (): readonly string[] => ['Pinned tool versions take their reason in a comment on the row'],
    },
    {
        match: (path, text): boolean => ['pyproject.toml', 'package.json'].includes(basename(path)) && _PIN.test(text),
        tools: _WRITES,
        each: (): readonly string[] => ['The lock file alone pins versions, spell the row unpinned'],
    },
] as const satisfies readonly PathRow[];

// --- [RULES] ---------------------------------------------------------------------------

const pathHits = (e: PathEvent): readonly PathRow[] =>
    PATHS.filter((row: PathRow) => row.tools.includes(e.tool) && row.match(_path(e), _text(e), _old(e)));

// The keys the adapter stamps as injected once the call ran
const pathSkills = (e: PathEvent): readonly string[] => [...new Set(pathHits(e).flatMap((row) => (row.once ?? []).map((once) => once.key)))];

const _context = (e: PathEvent, facts: PathFacts): readonly string[] => [
    ...new Set(
        pathHits(e).flatMap((row) => [
            ...(row.once ?? []).filter((once) => !facts.seen.has(once.key)).map((once) => once.line),
            ...(row.each ?? ((): readonly string[] => []))(_path(e), _text(e), _old(e), facts),
        ]),
    ),
];

// The result of the first row that answers the call, none when no row does
const _answer = (e: PathEvent): Option<WriteResult> =>
    map((result: NonNullable<PathRow['answer']>) => result(_path(e), _text(e)))(
        fromNullable(pathHits(e).find((row) => row.answer !== undefined)?.answer),
    );

// The child of an answered Write, sh with the path as its one argument and the text on stdin, run by the adapter under the session environment
const landing = (e: PathEvent): Option<Landing> =>
    map((result: WriteResult): Landing => ({ argv: ['sh', '-c', _LAND, 'sh', result.filePath], stdin: result.content, result }))(_answer(e));

// The first deny ends the rule
const _refuse = <E extends PathEvent>(e: E): Decision<E, unknown, string> =>
    fromNullable(pathHits(e).find((row) => row.deny !== undefined)?.deny).match<Decision<E, unknown, string>>({
        some: (reason) => deny(reason(_path(e))),
        none: () => rewrite(e, []),
    });

// The first answer ends the rule
const _land = <E extends PathEvent>(e: E): Decision<E, unknown, string> =>
    _answer(e).match<Decision<E, unknown, string>>({ some: answer, none: () => rewrite(e, []) });

// Every once line not yet seen and every each line join the context
const _pass =
    <E extends PathEvent>(facts: PathFacts): Rule<E, unknown, string> =>
    (e: E): Decision<E, unknown, string> =>
        rewrite(e, _context(e, facts));

// The deny, then the answer, then the pass
const pathRule = <E extends PathEvent>(facts: PathFacts): Rule<E, unknown, string> => fold<E, unknown, string>([_refuse, _land, _pass(facts)]);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Landing, Once, PathEvent, PathFacts, PathRow, PathTool, Search, Searched, WriteResult };
export { BINLOG_DENY, landed, landing, OP_DENY, PATHS, pathRule, pathSkills, recordSearches };
