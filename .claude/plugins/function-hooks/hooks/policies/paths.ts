// Path rows over Read, Edit, Write, and NotebookEdit: a binary deny, skill lines once per session, and per-edit lines

// --- [IMPORTS] -------------------------------------------------------------------------

import type { ToolCallInput } from 'claude-code';
import { type Decision, deny, rewrite } from '../composition/decision.ts';
import { basename, extension, under } from '../text/path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type PathTool = 'Read' | 'Edit' | 'Write' | 'NotebookEdit';

// The tool variants as the declarations state them, NotebookEdit names its file under notebook_path and its text under new_source
type PathEvent = Extract<ToolCallInput, { readonly tool: PathTool }>;

// A context line the session reads once under its key, or on every call without one
interface OnceLine {
    readonly key?: string;
    readonly line: string;
}

// A dependency manifest: the row pattern with its name group, and the sibling records a dropped name can remain in
interface Manifest {
    readonly row: RegExp;
    readonly siblings: readonly string[];
}

interface PathRow {
    readonly tools: readonly PathTool[];
    readonly match: (path: string, text: string, old: string) => boolean;
    readonly deny?: (path: string) => string;
    readonly once?: readonly OnceLine[];
    readonly lines?: (path: string, text: string, old: string) => readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const BINLOG_DENY = 'Use mcp__binlog__binlog_overview on the .binlog';
const OP_LINE = 'Read secrets through doppler secrets, not an op:// reference';
const _ALL: readonly PathTool[] = ['Read', 'Edit', 'Write'];
const _WRITES: readonly PathTool[] = ['Edit', 'Write'];
const _CONTENT_WRITES: readonly PathTool[] = ['Edit', 'Write', 'NotebookEdit'];
const _MSBUILD: readonly string[] = ['.csproj', '.props', '.targets'];
const _OP = /op:\/\/|\bop\s+(?:read|inject|item|run)\b/gu;
const _TARGET = /<Target\b/u;
const _PACKAGE_REFERENCE = /PackageReference/u;
const _VERSION_ATTRIBUTE = /Version=/u;
const _TOOL_VERSION = /[=]\s*"\d[^"]*"/u;
const _PIN = /[=]=\d|"\s*:\s*"[~^]?\d/u;
const _TYPESCRIPT_SIBLINGS: readonly string[] = ['package.json', 'pnpm-workspace.yaml'];
// One manifest per dependency record, its rows named in the old and the new text to tell an added row from a dropped one
const _MANIFESTS: Readonly<Partial<Record<string, Manifest>>> = {
    'Directory.Packages.props': { row: /<PackageVersion\s+Include="(?<name>[^"]+)"/gu, siblings: ['*.csproj', '*.props', '*.targets'] },
    'package.json': { row: /^\s*"(?<name>[^"]+)":\s*"(?:catalog:|workspace:|[~^]?\d)/gmu, siblings: _TYPESCRIPT_SIBLINGS },
    'pnpm-workspace.yaml': { row: /^\s{2,}'?(?<name>[\w@/.-]+)'?:\s*\S/gmu, siblings: _TYPESCRIPT_SIBLINGS },
    'pyproject.toml': { row: /^\s*"(?<name>[\w.-]+)[\w.[\],=<>!~ -]*"\s*,?\s*$/gmu, siblings: ['pyproject.toml', 'uv.lock'] },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const _skill = (name: string): OnceLine => ({ key: name, line: `Load the ${name} skill` });

const _names = (text: string, row: RegExp): ReadonlySet<string> => new Set([...text.matchAll(row)].flatMap((hit) => hit.groups?.['name'] ?? []));

// The distinct names in text and not in other, the added rows as (new, old) and the dropped rows as (old, new)
const _only = (text: string, other: string, row: RegExp): readonly string[] => {
    const known = _names(other, row);
    return [..._names(text, row)].filter((name) => !known.has(name));
};

const _path = (e: PathEvent): string => (e.tool === 'NotebookEdit' ? e.notebook_path : e.file_path);

// The new content of a Write, the replacement of an Edit, the cell source of a NotebookEdit, and nothing for a Read
const _text = (e: PathEvent): string => {
    switch (e.tool) {
        case 'Write':
            return e.content;
        case 'Edit':
            return e.new_string;
        case 'NotebookEdit':
            return e.new_source;
        default:
            return '';
    }
};

const _old = (e: PathEvent): string => (e.tool === 'Edit' ? e.old_string : '');

const _references = (text: string): number => [...text.matchAll(_OP)].length;

// Added rows take a README record, and dropped rows name the sibling records the name can remain in
const _manifestLines = (path: string, text: string, old: string): readonly string[] => {
    const manifest = _MANIFESTS[basename(path)];
    if (manifest === undefined) {
        return [];
    }
    const added = _only(text, old, manifest.row);
    const dropped = _only(old, text, manifest.row);
    return [
        ...(added.length > 0 ? [`Record ${added.join(', ')} in the owning README.md dependency list`] : []),
        ...(dropped.length > 0
            ? [
                  `Dropped ${dropped.join(', ')} from ${basename(path)}, restore a name ${manifest.siblings.join(', ')} or the README.md dependency list still holds, else drop it there too`,
              ]
            : []),
    ];
};

// --- [ROWS] ----------------------------------------------------------------------------

const PATHS = [
    { tools: _ALL, match: (path): boolean => extension(path) === '.binlog', deny: (): string => BINLOG_DENY },
    // A reference the write adds outside the harness tree, which documents and tests the form
    {
        tools: _CONTENT_WRITES,
        match: (path, text, old): boolean => _references(text) > _references(old) && !under(path, '.claude'),
        lines: (): readonly string[] => [OP_LINE],
    },
    { tools: _ALL, match: (path): boolean => extension(path) === '.cs', once: [_skill('dotnet-roslyn-codelens'), _skill('dotnet-coding')] },
    {
        tools: _ALL,
        match: (path): boolean => _MSBUILD.includes(extension(path)),
        once: [_skill('dotnet-msbuild-evaluation'), _skill('dotnet-msbuild-antipatterns')],
    },
    {
        tools: _ALL,
        match: (path, text): boolean => _MSBUILD.includes(extension(path)) && _TARGET.test(text),
        once: [_skill('dotnet-msbuild-execution')],
    },
    {
        tools: _ALL,
        match: (path, text): boolean =>
            ['Directory.Packages.props', 'NuGet.config'].includes(basename(path)) || extension(path) === '.slnx' || _PACKAGE_REFERENCE.test(text),
        once: [_skill('dotnet-msbuild-packaging')],
    },
    {
        tools: _ALL,
        match: (path): boolean =>
            ['eng', 'infra', 'tools', '.github'].some((directory) => under(path, directory)) ||
            ['mise.toml', 'mise.unix.toml', '.miserc.toml', 'nx.json'].includes(basename(path)),
        once: [_skill('manage-repo')],
    },
    { tools: _ALL, match: (path): boolean => under(path, 'infra'), once: [_skill('pulumi')] },
    {
        tools: _ALL,
        match: (path): boolean => under(path, '.claude'),
        once: [{ key: 'claudeCodeDocs', line: 'Use mcp__claudeCodeDocs__search_claude_code_docs for the harness docs' }],
    },
    { tools: _ALL, match: (path): boolean => basename(path).startsWith('.env') || basename(path) === 'doppler.yaml', once: [_skill('secrets')] },
    {
        tools: _WRITES,
        match: (path, text): boolean => basename(path) === 'Directory.Packages.props' && _VERSION_ATTRIBUTE.test(text),
        lines: (): readonly string[] => ['Verify the row with mcp__nuget__get_package_context'],
    },
    { tools: _WRITES, match: (path): boolean => _MANIFESTS[basename(path)] !== undefined, lines: _manifestLines },
    {
        tools: _WRITES,
        match: (path, text): boolean => basename(path) === 'mise.toml' && _TOOL_VERSION.test(text),
        lines: (): readonly string[] => ['State the reason for the pin in a comment on its row'],
    },
    {
        tools: _WRITES,
        match: (path, text): boolean => ['pyproject.toml', 'package.json'].includes(basename(path)) && _PIN.test(text),
        lines: (): readonly string[] => ['Spell the row unpinned, the lock file alone pins versions'],
    },
] as const satisfies readonly PathRow[];

// --- [RULES] ---------------------------------------------------------------------------

const _hits = (e: PathEvent): readonly PathRow[] =>
    PATHS.filter((row: PathRow) => row.tools.includes(e.tool) && row.match(_path(e), _text(e), _old(e)));

// The once lines the call raises, the adapter stamps their keys as injected once the call ran
const pathOnce = (e: PathEvent): readonly OnceLine[] => _hits(e).flatMap((row) => row.once ?? []);

// The first deny, else the pass with every unseen once line and every per-edit line
const pathRule =
    (seen: ReadonlySet<string>): (<E extends PathEvent>(e: E) => Decision<E>) =>
    <E extends PathEvent>(e: E): Decision<E> => {
        const hits = _hits(e);
        const [reason] = hits.flatMap((row) => (row.deny === undefined ? [] : [row.deny(_path(e))]));
        if (reason !== undefined) {
            return deny(reason);
        }
        const once = pathOnce(e).filter((line) => line.key === undefined || !seen.has(line.key));
        return rewrite(e, [...new Set([...once.map((line) => line.line), ...hits.flatMap((row) => row.lines?.(_path(e), _text(e), _old(e)) ?? [])])]);
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export type { OnceLine, PathEvent, PathRow, PathTool };
export { BINLOG_DENY, OP_LINE, pathOnce, pathRule };
