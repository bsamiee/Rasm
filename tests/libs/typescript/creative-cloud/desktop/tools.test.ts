import { fileURLToPath, pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, describe, it } from '@effect/vitest';
import { Description } from '@rasm/applications/source';
import { Application } from '@rasm/creative-cloud-desktop/application';
import { Preferences, Tools } from '@rasm/creative-cloud-desktop/tools';
import { Array, Effect, FileSystem, HashMap, Layer, Option, Path, Stream, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FIXTURES = fileURLToPath(new URL('fixtures/', import.meta.url));
const PHOTOSHOP = Application.make({
    name: 'Adobe Photoshop (Beta)',
    product: 'PHSP',
    channel: 'beta',
    version: '28.2.0',
    build: Option.some('28.2.0.3749'),
    identifier: 'com.adobe.Photoshop',
    url: pathToFileURL(`${FIXTURES}Applications/Adobe Photoshop (Beta)/Adobe Photoshop (Beta).app/`),
});
const ILLUSTRATOR = Application.make({
    name: 'Adobe Illustrator (Beta)',
    product: 'ILST',
    channel: 'beta',
    version: '31.1.0',
    build: Option.some('31.1.0.127'),
    identifier: 'com.adobe.illustratorBeta',
    url: pathToFileURL(`${FIXTURES}Applications/Adobe Illustrator (Beta)/Adobe Illustrator.app/`),
});
const INDESIGN = Application.make({
    name: 'Adobe InDesign 2027 (Beta)',
    product: 'IDSN',
    channel: 'beta',
    version: '22.1.0.19',
    build: Option.some('22.1.0.019'),
    identifier: 'com.adobe.InDesign',
    url: pathToFileURL(`${FIXTURES}Applications/Adobe InDesign 2027 (Beta)/Adobe InDesign 2027 (Beta).app/`),
});
const SETTINGS = 'Adobe Photoshop (Beta) Settings';

// --- [OPERATIONS] ----------------------------------------------------------------------

const reader = (preferences: string) => Effect.provide(Tools, Tools.layer.pipe(Layer.provide(Layer.succeed(Preferences, preferences))));
const declared = (application: Application) => Effect.flatMap(reader(`${FIXTURES}Preferences`), (read) => read(application)).pipe(Effect.provide(layer));
const folded = Effect.fnUntraced(function* (application: Application, current: { readonly id: string; readonly label: string }, listed: Option.Option<readonly { readonly id: string; readonly label: string; readonly help?: string }[]> = Option.none()) {
    const [described, tools] = yield* Effect.all([Description.tools({ current: Effect.succeed(current), ...Option.getOrUndefined(Option.map(listed, (each) => ({ listed: Effect.succeed(each) }))), select: () => Effect.void, changes: Stream.empty }).describe({}), declared(application)]);
    const folding = Description.fold(described, tools);
    const choice = Array.findFirst(Array.flatMap(folding.containers, Struct.get('controls')), ({ kind }) => kind === 'choice');
    const options = Array.map(
        Option.getOrElse(
            Option.flatMap(choice, (control) => Option.fromUndefinedOr(control.options)),
            Array.empty,
        ),
        ({ locator, ...facts }) => [Array.lastNonEmpty(locator)[1], facts] as const,
    );
    return { described, folding, tools, value: Option.getOrUndefined(Option.flatMap(choice, ({ value }) => Option.fromUndefinedOr(value))), options, pointer: folding.pointer };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('Photoshop', () => {
    it.effect.each([
        { tool: '$lmvr', expected: { label: 'Move Tool', shortcut: { key: 'v', modifiers: [] } } },
        { tool: '$slbr', expected: { label: 'Selection Brush Tool', shortcut: { key: 'l', modifiers: [] } } },
        { tool: '$pntb', expected: { label: 'Brush Tool', shortcut: { key: 'b', modifiers: [] } } },
        { tool: '$srmt', expected: { label: 'Single Row Marquee Tool' } },
        { tool: 'paintbrushTool', expected: { label: 'Brush Tool', help: 'https://helpx.adobe.com/photoshop/using/tool-techniques/brush-tool.html' } },
        { tool: 'rulerTool', expected: { label: 'Ruler Tool' } },
    ])('states $tool from its active shortcut set and curated catalog', ({ tool, expected }) =>
        Effect.map(declared(PHOTOSHOP), ({ tools }) => {
            assert.deepStrictEqual(Option.getOrUndefined(HashMap.get(tools, tool)), expected);
        }),
    );
    it.effect.each([
        { tool: '$SLre', source: 'a tool of the Select and Mask taskspace' },
        { tool: 'Adobe Brush Tool', source: "Illustrator's catalog shipped in the same bundle" },
        { tool: '', source: 'a catalog item outside the tool techniques' },
    ])('leaves out $source', ({ tool }) =>
        Effect.map(declared(PHOTOSHOP), ({ tools }) => {
            assert.isFalse(HashMap.has(tools, tool));
        }),
    );
    it.effect('groups the toolbar slots of its toolbar file by charID', () =>
        Effect.map(declared(PHOTOSHOP), ({ toolbar }) => {
            assert.strictEqual(toolbar.length, 22);
            assert.deepStrictEqual(toolbar.slice(0, 3), [
                ['$arwT', '$ArtT'],
                ['$rgmt', '$elmt', '$srmt', '$scmt'],
                ['$slbr', '$laso', '$pgon', '$mlas'],
            ]);
        }),
    );
    it.effect('keeps the facts of a build once read, and reads another build again', () =>
        Effect.gen(function* () {
            const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const preferences = yield* files.makeTempDirectoryScoped();
            yield* files.copy(path.join(FIXTURES, 'Preferences', SETTINGS), path.join(preferences, SETTINGS));
            const read = yield* reader(preferences);
            yield* read(PHOTOSHOP);
            yield* files.remove(path.join(preferences, SETTINGS, 'Keyboard Shortcuts.psp'));
            const [held, rebuilt] = yield* Effect.all([read(PHOTOSHOP), read(Application.make({ ...PHOTOSHOP, build: Option.some('28.2.0.3750') }))]);
            assert.isTrue(HashMap.has(held.tools, '$pntb'));
            assert.isFalse(HashMap.has(rebuilt.tools, '$pntb'));
            assert.isTrue(HashMap.has(rebuilt.tools, 'paintbrushTool'));
        }).pipe(Effect.scoped, Effect.provide(layer)),
    );
});

describe('Illustrator', () => {
    it.effect.each([
        { tool: 'Adobe Brush Tool', expected: { label: 'Paintbrush Tool', shortcut: { key: 'b', modifiers: [] }, help: 'https://helpx.adobe.com/illustrator/using/tool-techniques/paintbrush-tool.html' } },
        { tool: 'Adobe Select Tool', expected: { shortcut: { key: 'v', modifiers: [] } } },
        { tool: 'Adobe Width Tool', expected: { shortcut: { key: 'w', modifiers: ['shift'] } } },
        { tool: 'Adobe Toggle Drawing Mode', expected: { shortcut: { key: 'd', modifiers: ['shift'] } } },
        { tool: 'Adobe Direct Object Select Tool', expected: {} },
        { tool: 'Adobe Symbol Sprayer Tool', expected: { label: 'Symbol Sprayer Tool', help: 'https://helpx.adobe.com/illustrator/using/tool-techniques/symbol-sprayer-tool.html' } },
    ] as const)('states $tool from the preset its preferences name and its curated catalog', ({ tool, expected }) =>
        Effect.map(declared(ILLUSTRATOR), ({ tools }) => {
            assert.deepStrictEqual(Option.getOrUndefined(HashMap.get(tools, tool)), expected);
        }),
    );
    it.effect('leaves out menu commands and states no toolbar', () =>
        Effect.map(declared(ILLUSTRATOR), ({ tools, toolbar }) => {
            assert.isFalse(HashMap.has(tools, 'cut2'));
            assert.deepStrictEqual(toolbar, []);
        }),
    );
});

describe('InDesign', () => {
    it.effect.each([
        { tool: '24350', action: 'kPageToolActionID 0x5F00 + 30', expected: { shortcut: { key: 'p', modifiers: ['shift'] } } },
        { tool: '22786', action: 'kGoBackActionID 0x5900 + 2', expected: { shortcut: { key: 'pageUp', modifiers: ['command'] } } },
        { tool: '11024', action: 'kClearFillStrokeToolActionID 0x2b00 + 16', expected: { shortcut: { key: '/', modifiers: [] } } },
        { tool: '272', action: 'kClearActionID 0x100 + 16', expected: { shortcut: { key: 'forwardDelete', modifiers: [] } } },
        { tool: '24331', action: 'kShowFrameEdgeActionID 0x5f00 + 11', expected: { shortcut: { key: 'h', modifiers: ['control', 'command'] } } },
        { tool: '118822', action: 'kPointerToolActionID 0x1d000 + 38', expected: { shortcut: { key: 'v', modifiers: [] } } },
    ] as const)('states $action from the Default set in the default context', ({ tool, expected }) =>
        Effect.map(declared(INDESIGN), ({ tools }) => {
            assert.deepStrictEqual(Option.getOrUndefined(HashMap.get(tools, tool)), expected);
        }),
    );
    it.effect('leaves out an action bound in the text context alone', () =>
        Effect.map(declared(INDESIGN), ({ tools }) => {
            assert.isFalse(HashMap.has(tools, '42248'));
        }),
    );
});

describe('Illustrator preferences', () => {
    it.effect('reads the MCP server token and the shortcut set the preferences of an Illustrator build name', () =>
        Effect.map(Preferences.illustrator(ILLUSTRATOR).pipe(Effect.provideService(Preferences, `${FIXTURES}Preferences`), Effect.provide(layer)), (read) => {
            assert.deepStrictEqual(Array.map(read, Struct.get('plugin')), [{ AIMCPServer: { AuthToken: 'ilst_test' }, KBSCShortcutFile: { IsPreset: true, FileName: 'Illustrator Defaults' } }]);
        }),
    );
    it.effect('reads none for another application of the version an Illustrator settings folder ran', () =>
        Effect.map(
            Preferences.illustrator(Application.make({ ...ILLUSTRATOR, name: 'Adobe After Effects (Beta)', product: 'AEFT', identifier: 'com.adobe.AfterEffects', url: pathToFileURL(`${FIXTURES}Applications/Adobe After Effects (Beta)/Adobe After Effects (Beta).app/`) })).pipe(
                Effect.provideService(Preferences, `${FIXTURES}Preferences`),
                Effect.provide(layer),
            ),
            (read) => {
                assert.deepStrictEqual(read, []);
            },
        ),
    );
    it.effect('reads none for a build no settings folder ran', () =>
        Effect.map(Preferences.illustrator(Application.make({ ...ILLUSTRATOR, version: '31.2.0' })).pipe(Effect.provideService(Preferences, `${FIXTURES}Preferences`), Effect.provide(layer)), (read) => {
            assert.deepStrictEqual(read, []);
        }),
    );
});

describe('Tools folded into a toolbar description', () => {
    it.effect('lists each Photoshop toolbar slot once with the name and shortcut of its active set, the current tool keyed by its charID', () =>
        Effect.map(folded(PHOTOSHOP, { id: '$pntb', label: 'Brush Tool' }), ({ tools, value, options, pointer }) => {
            assert.deepStrictEqual(pointer, { tool: 'Brush Tool', shortcut: { key: 'b', modifiers: [] } });
            assert.deepStrictEqual(value, { _tag: 'text', value: '$pntb' });
            assert.deepStrictEqual(options.slice(0, 7), [
                ['$arwT', { label: '$arwT' }],
                ['$ArtT', { label: '$ArtT' }],
                ['$rgmt', { label: '$rgmt' }],
                ['$elmt', { label: '$elmt' }],
                ['$srmt', { label: 'Single Row Marquee Tool' }],
                ['$scmt', { label: '$scmt' }],
                ['$slbr', { label: 'Selection Brush Tool', shortcut: { key: 'l', modifiers: [] } }],
            ]);
            assert.strictEqual(options.length, new Set(tools.toolbar.flat()).size);
        }),
    );
    it.effect('names the current Illustrator tool by its catalog name, shortcut, and help page, listing no tool', () =>
        Effect.map(folded(ILLUSTRATOR, { id: 'Adobe Brush Tool', label: 'Adobe Brush Tool' }), ({ value, options, pointer }) => {
            assert.deepStrictEqual(pointer, { tool: 'Paintbrush Tool', shortcut: { key: 'b', modifiers: [] }, help: 'https://helpx.adobe.com/illustrator/using/tool-techniques/paintbrush-tool.html' });
            assert.deepStrictEqual(value, { _tag: 'text', value: 'Adobe Brush Tool' });
            assert.deepStrictEqual(options, []);
        }),
    );
    it.effect('keeps the label and help a host lists, filling a label equal to its key and each unstated shortcut', () =>
        Effect.map(
            folded(
                PHOTOSHOP,
                { id: '$slbr', label: '$slbr' },
                Option.some([
                    { id: '$pntb', label: 'Pinsel', help: 'Malt mit der Vordergrundfarbe' },
                    { id: '$slbr', label: '$slbr' },
                ]),
            ),
            ({ value, options, pointer }) => {
                assert.deepStrictEqual(options.slice(0, 2), [
                    ['$pntb', { label: 'Pinsel', help: 'Malt mit der Vordergrundfarbe', shortcut: { key: 'b', modifiers: [] } }],
                    ['$slbr', { label: 'Selection Brush Tool', shortcut: { key: 'l', modifiers: [] } }],
                ]);
                assert.deepStrictEqual(value, { _tag: 'text', value: 'Selection Brush Tool' });
                assert.deepStrictEqual(pointer, { tool: 'Selection Brush Tool', shortcut: { key: 'l', modifiers: [] } });
            },
        ),
    );
    it.effect('leaves a description whole where the declarations key none of its tools', () =>
        Effect.map(
            folded(
                INDESIGN,
                { id: 'SELECTION_TOOL', label: 'Selection Tool' },
                Option.some([
                    { id: 'SELECTION_TOOL', label: 'SELECTION_TOOL' },
                    { id: 'PEN_TOOL', label: 'PEN_TOOL' },
                ]),
            ),
            ({ described, folding }) => {
                assert.deepStrictEqual(folding, described);
            },
        ),
    );
});
