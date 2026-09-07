// Path rows over Read, Edit, Write, and NotebookEdit for binary denies, secret content denies, skill context once per session, and per-edit context

// --- [IMPORTS] -------------------------------------------------------------------------

import type { ToolCallInput } from 'claude-code';
import { type Decision, deny, type Rule, rewrite } from '../composition/decision.ts';
import { fromNullable, fromPredicate, getOrElse, liftPredicate, map, toArray } from '../composition/option.ts';
import { isString } from '../host/store.ts';
import { basename, extension, under } from '../text/path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type PathTool = 'Read' | 'Edit' | 'Write' | 'NotebookEdit';

// The tool variants as the declarations state them, NotebookEdit names its file under notebook_path and its text under new_source
type PathEvent = Extract<ToolCallInput, { readonly tool: PathTool }>;

type Field = 'file_path' | 'notebook_path' | 'content' | 'new_string' | 'new_source' | 'old_string';

// Context lines injected once per session under their key, the skill name or the tool they name
interface Once {
    readonly key: string;
    readonly line: string;
}

// Guidance rows apply under the CLAUDE.md chain and the memory directory alone, the facts the session row holds
interface PathRow {
    readonly match: (path: string, text: string) => boolean;
    readonly tools: readonly PathTool[];
    readonly deny?: (path: string) => string;
    readonly once?: readonly Once[];
    readonly each?: (path: string, text: string, old: string, base: number) => readonly string[];
    readonly guidance?: true;
}

// The file is the edited file's text before the call, '' when unread, an Edit's lines number from the file
interface PathFacts {
    readonly seen: ReadonlySet<string>;
    readonly guidance: readonly string[];
    readonly file: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _WIDTH = 150;
const BINLOG_DENY = '.binlog files are binary, call mcp__binlog__binlog_overview on the file';
const OP_DENY = 'Secrets come from Doppler alone, an op:// reference never lands in a file, use doppler secrets';
// The probe directory every agent of the session shares under the scratchpad, the tail keeps the rest of the path
const PROBE = /\/scratchpad\/probe(?<tail>\/|$)/u;
const _ALL: readonly PathTool[] = ['Read', 'Edit', 'Write'];
const _WRITES: readonly PathTool[] = ['Edit', 'Write'];
// Every tool that lands text in a file, the content rows read the text whatever the file
const _CONTENT_WRITES: readonly PathTool[] = ['Edit', 'Write', 'NotebookEdit'];
const _OP = /op:\/\/|\bop\s+(?:read|inject|item|run)\b/u;
const _TARGET = /<Target\b/u;
const _PACKAGE_REFERENCE = /PackageReference/u;
const _VERSION_ATTRIBUTE = /Version=/u;
const _TOOL_VERSION = /[=]\s*"\d[^"]*"/u;
const _PIN = /[=]=\d|"\s*:\s*"[~^]?\d/u;
const _ENTRY = /^(?:- |\| |\d+\. )/u;
const _SELF_REFERENCE = /this file|see above|see below|this section/giu;
const _LINE = /\r?\n/u;
// One dependency row per manifest, counted in the old and the new text to tell an added row from a dropped one
const _DEPENDENCY_ROW: Readonly<Partial<Record<string, RegExp>>> = {
    'Directory.Packages.props': /<PackageVersion\s/gu,
    'package.json': /^\s*"[^"]+":\s*"(?:catalog:|workspace:|[~^]?\d)/gmu,
    'pnpm-workspace.yaml': /^\s{2,}[\w@/.-]+:\s*\S/gmu,
    'pyproject.toml': /^\s*"[\w.[\],=<>!~ -]+"\s*,?\s*$/gmu,
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const _skill = (name: string): Once => ({ key: name, line: `Load the ${name} skill` });

// The deny of a write under the shared probe directory, the agent-labelled directory spliced from the path, main for the main agent
const probeDeny = (path: string): string =>
    `${path} sits in the probe/ directory every agent of the session shares, write under ${path.replace(PROBE, '/scratchpad/<label>-probe$<tail>')}, the label your brief states (main for the main agent)`;

const _count = (text: string, pattern: RegExp): number => [...text.matchAll(pattern)].length;

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

const _dependencyLines = (path: string, text: string, old: string): readonly string[] =>
    toArray(fromNullable(_DEPENDENCY_ROW[basename(path)])).flatMap((row) => [
        ...toArray(
            liftPredicate<string>(() => _count(text, row) > _count(old, row))('Record the dependency in the owning README.md dependency list'),
        ),
        ...toArray(
            liftPredicate<string>(() => _count(text, row) < _count(old, row))(
                'Add the missing dependency record to its manifest or README.md dependency list and keep the dropped row',
            ),
        ),
    ]);

// The file line the written text starts at: the line breaks before old in the file, 0 for a Write, an unread file, or an absent old
const _baseLine = (file: string, old: string): number =>
    getOrElse(() => 0)(map((at: number) => file.slice(0, at).split(_LINE).length - 1)(liftPredicate<number>((at) => at >= 0)(file.indexOf(old))));

// Entries at or over the width and self-references, one line each, numbered from the file line the text starts at
const _markdownLines = (text: string, base: number): readonly string[] =>
    text
        .split(_LINE)
        .flatMap((line, index) => [
            ...toArray(
                liftPredicate<string>(() => _ENTRY.test(line) && line.length >= _WIDTH)(
                    `Entry at line ${base + index + 1} is ${line.length} columns, the limit is ${_WIDTH}`,
                ),
            ),
            ...[...line.matchAll(_SELF_REFERENCE)].map((hit) => `Self-reference at line ${base + index + 1}: ${hit[0]}`),
        ]);

// --- [ROWS] ----------------------------------------------------------------------------

const PATHS = [
    {
        match: (path): boolean => extension(path) === '.binlog',
        tools: _ALL,
        deny: (): string => BINLOG_DENY,
    },
    {
        match: (path, text): boolean => _OP.test(text) && !under(path, '.claude/skills'),
        tools: _CONTENT_WRITES,
        deny: (): string => OP_DENY,
    },
    {
        // RegExp.test reads its receiver, the arrow binds PROBE to it
        // ast-grep-ignore: no-forwarding-arrow
        match: (path): boolean => PROBE.test(path),
        tools: _CONTENT_WRITES,
        deny: probeDeny,
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
            ['eng', 'infra', 'tools', '.github'].some((directory) => under(path, directory)) || ['mise.toml', 'nx.json'].includes(basename(path)),
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
        match: (path): boolean => _DEPENDENCY_ROW[basename(path)] !== undefined,
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
    {
        match: (path): boolean => extension(path) === '.md',
        tools: _WRITES,
        each: (_file, text, _replaced, base): readonly string[] => _markdownLines(text, base),
        guidance: true,
    },
] as const satisfies readonly PathRow[];

// --- [RULES] ---------------------------------------------------------------------------

const pathHits = (e: PathEvent): readonly PathRow[] => PATHS.filter((row: PathRow) => row.tools.includes(e.tool) && row.match(_path(e), _text(e)));

// The keys the adapter stamps as injected once the call ran
const pathSkills = (e: PathEvent): readonly string[] => [...new Set(pathHits(e).flatMap((row) => (row.once ?? []).map((once) => once.key)))];

const _underGuidance = (path: string, guidance: readonly string[]): boolean => guidance.some((directory) => path.startsWith(`${directory}/`));

const _applies =
    (e: PathEvent, facts: PathFacts): ((row: PathRow) => boolean) =>
    (row: PathRow): boolean =>
        row.guidance === undefined || _underGuidance(_path(e), facts.guidance);

const _context = (e: PathEvent, facts: PathFacts): readonly string[] => [
    ...new Set(
        pathHits(e)
            .filter(_applies(e, facts))
            .flatMap((row) => [
                ...(row.once ?? []).filter((once) => !facts.seen.has(once.key)).map((once) => once.line),
                ...(row.each ?? ((): readonly string[] => []))(_path(e), _text(e), _old(e), _baseLine(facts.file, _old(e))),
            ]),
    ),
];

// The first deny ends the rule, otherwise every once line not yet seen and every each line join the context
const pathRule =
    <E extends PathEvent>(facts: PathFacts): Rule<E, unknown, string> =>
    (e: E): Decision<E, unknown, string> =>
        fromNullable(pathHits(e).find((row) => row.deny !== undefined)?.deny).match<Decision<E, unknown, string>>({
            some: (reason) => deny(reason(_path(e))),
            none: () => rewrite(e, _context(e, facts)),
        });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Once, PathEvent, PathFacts, PathRow, PathTool };
export { BINLOG_DENY, OP_DENY, PATHS, PROBE, pathRule, pathSkills, probeDeny };
