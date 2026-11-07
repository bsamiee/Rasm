import type { Tree } from '@nx/devkit';
import { Effect, Record, Schema } from 'effect';
import { TextFormat } from '#/eng/typescript/text-format';
import { decodeTree, patched, syncGenerator } from '#/tools/nx/sync/tree';

// --- [CONSTANTS] -----------------------------------------------------------------------

const OWNER = 'plugins/argus/.lsp.json';
const EDITOR = '.vscode/settings.json';
const PREFIX = 'rust-analyzer';
const SECTIONS = ['check', 'rustc'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Section = Schema.Record(Schema.String, Schema.Json);
const LanguageServers = Schema.fromJsonString(Schema.Struct({ rust: Schema.Struct({ initializationOptions: Schema.Record(Schema.Literals(SECTIONS), Section) }) }));
const EditorSettings = TextFormat.Json5.pipe(Schema.decodeTo(Section));

// --- [OPERATIONS] ----------------------------------------------------------------------

const shared = (section: (typeof SECTIONS)[number]) => (key: string) => `${PREFIX}.${section}.${key}`;
const derived = (key: string) => key !== shared('check')('command') && SECTIONS.some((section) => key.startsWith(shared(section)('')));
const rows = Effect.fnUntraced(function* (tree: Tree) {
    const [
        {
            rust: { initializationOptions },
        },
        current,
    ] = yield* Effect.all([decodeTree(tree, LanguageServers, OWNER), decodeTree(tree, EditorSettings, EDITOR)]);
    const stale = Record.map(
        Record.filter(current, (_, key) => derived(key)),
        () => undefined,
    );
    return [yield* patched(tree, EDITOR, current, { ...stale, ...Record.mapKeys(Record.remove(initializationOptions.check, 'command'), shared('check')), ...Record.mapKeys(initializationOptions.rustc, shared('rustc')) })];
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const rustAnalyzer = syncGenerator(`${EDITOR} ${PREFIX} rows differ from ${OWNER}`, rows);

// --- [EXPORTS] -------------------------------------------------------------------------

export { rustAnalyzer };
