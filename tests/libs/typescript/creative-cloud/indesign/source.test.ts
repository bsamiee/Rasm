import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, describe, it } from '@effect/vitest';
import { Description, type FacetContainer, type FacetControl } from '@rasm/applications/source';
import { Typeset } from '@rasm/indesign/typesetting';
import { Preflight } from '@rasm/indesign/views';
import { Declaration } from '@rasm/object-model/model';
import { readTypeScript } from '@rasm/object-model/node';
import { run } from '@rasm/process';
import base from '@rasm/uxp/tsconfig.base.json' with { type: 'json' };
import { pluginConfig } from '@rasm/uxp/vite';
import { Array, Cause, Context, Effect, FileSystem, Formatter, Layer, Option, Path, Schema, Struct } from 'effect';
import { build, mergeConfig } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = { manifestVersion: 5, id: 'app.rasm.test.source', name: 'Source', version: '0.1.0', host: { app: 'ID' }, entrypoints: [], requiredPermissions: {} };
const AREA = 'File Menu';
const TYPE = { id: 'TYPE_TOOL', name: 'Type Tool' } as const;
const RECTANGLE = 'RECTANGLE_TOOL';
const UNLISTED = 'NO_SUCH_TOOL';
const PLACED = 'placed text\n';
const BUNDLE_PATH = /bundle path="(?<path>[^"]+)"/u;
const TIMEOUT = 120_000;
const PROFILE = '[Basic]';
const OVERSET = 'Overset text in a frame too small to hold it. '.repeat(40);
const FIGURES = '2026';
const CHOSEN = { tnum: 1, lnum: 1, sups: 1 } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);
const Answer = Schema.fromJsonString(
    Schema.Union([
        Schema.Struct({ described: Description }),
        Schema.Struct({ operated: Schema.Boolean }),
        Schema.Struct({ heard: Schema.Struct({ both: Schema.Boolean, after: Schema.Boolean, listeners: Schema.Int }) }),
        Schema.Struct({ preflighted: Preflight.Result }),
        Schema.Struct({ typeset: Typeset.Result }),
        Schema.Struct({ failed: Schema.String }),
    ]),
);
const Framed = Schema.fromJsonString(Schema.Struct({ document: Schema.Int, frame: Schema.String }));
const Styled = Schema.fromJsonString(Schema.Struct({ figures: Schema.String, position: Schema.String }));
const Read = Schema.fromJsonString(
    Schema.Struct({
        tool: Schema.String,
        name: Schema.String,
        tools: Schema.Array(Schema.String),
        panels: Schema.Array(Schema.Tuple([Schema.String, Schema.Boolean])),
        areas: Schema.Array(Schema.String),
        actions: Schema.Array(Schema.Struct({ name: Schema.String, enabled: Schema.Boolean, checked: Schema.Boolean })),
        holding: Schema.Array(Schema.String),
    }),
);

// --- [SERVICES] ------------------------------------------------------------------------

class Host extends Context.Service<Host, { readonly application: string; readonly bundle: string; readonly placed: string }>()('@rasm/test-indesign/source.test/Host') {
    static readonly layer = (application: string) =>
        Effect.gen(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const folder = yield* fs.makeTempDirectoryScoped();
            const out = path.join(folder, 'out');
            const open = yield* readTypeScript(Option.none(), path.join(import.meta.dirname, 'node_modules'));
            const source = yield* open({ root: 'indesign-uxp-types/flat', types: ['indesign-uxp-types'], lib: base.compilerOptions.lib, preamble: '', enumeration: Option.some('Enumeration') });
            const [manifest, entry, declared] = yield* Effect.all([Schema.encodeEffect(Json)(MANIFEST), Schema.encodeEffect(Json)(fileURLToPath(import.meta.resolve('./source-host.ts'))), Effect.flatMap(source.model, Schema.encodeEffect(Schema.fromJsonString(Declaration.Model)))]);
            yield* Effect.all(
                [
                    fs.writeFileString(path.join(folder, 'manifest.json'), manifest),
                    fs.writeFileString(path.join(folder, 'declared.json'), declared),
                    fs.writeFileString(path.join(folder, 'main.ts'), `import declarations from './declared.json?raw';\nimport { serve } from ${entry};\nexport const run = serve(declarations);\n`),
                    fs.writeFileString(path.join(folder, 'placed.txt'), PLACED),
                ],
                { discard: true },
            );
            yield* Effect.tryPromise({ try: () => build(mergeConfig(pluginConfig(), { configFile: false, root: folder, logLevel: 'silent', build: { outDir: out } })), catch: (cause) => new Cause.UnknownError(cause, Formatter.format(cause)) });
            return { application, bundle: path.join(out, 'main.js'), placed: path.join(folder, 'placed.txt') };
        }).pipe(Layer.effect(Host));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const scripted = Effect.fnUntraced(function* (body: string, ...given: readonly string[]) {
    const { application } = yield* Host;
    const target = yield* Schema.encodeEffect(Json)(application);
    const source = `const { script } = require('uxp'); try { script.setResult((() => { ${body} })()); } catch (error) { script.setResult(JSON.stringify({ failed: String(error) })); }`;
    const { stdout } = yield* run(['osascript', '-e', 'on run argv', '-e', `tell application ${target} to do script (item 1 of argv) language uxpscript with arguments (rest of argv)`, '-e', 'end run', source, ...given]);
    return stdout.trim();
});
const facet = Effect.fnUntraced(function* (request: object) {
    const [{ bundle }, encoded] = yield* Effect.all([Host, Schema.encodeEffect(Json)(request)]);
    const answered = yield* scripted("const [file, request] = script.args; const module = { exports: {} }; new Function('require', 'module', 'exports', require('fs').readFileSync(file, { encoding: 'utf-8' }))(require, module, module.exports); return module.exports.run(request);", bundle, encoded);
    return yield* Schema.decodeEffect(Answer)(answered);
});
const dom = Effect.flatMap(
    scripted(
        `const { app, UITools } = require('indesign');
const [area] = script.args;
const every = app.menuActions.everyItem();
const panels = [];
for (let index = 0; index < app.panels.length; index += 1) { const panel = app.panels.item(index); if (panel !== undefined && panel.name !== '') { panels.push([panel.name, panel.visible]); } }
const [ids, areas] = [every.id, every.area];
const actions = ids.filter((id, index) => areas[index] === area).map((id) => app.menuActions.itemByID(id)).map((action) => ({ name: action.name, enabled: action.enabled, checked: action.checked }));
const holding = app.documents.length === 1 && app.documents.item(0).placeGuns.loaded ? app.documents.item(0).placeGuns.pageItems.everyItem().getElements().map((item) => item.toSpecifier()) : [];
return JSON.stringify({ tool: String(app.toolBoxTools.currentTool), name: app.toolBoxTools.currentToolName, tools: Object.getOwnPropertyNames(Object.getPrototypeOf(UITools)).filter((key) => String(UITools[key]) === key && !UITools.NONE.equals(UITools[key])), panels, areas: [...new Set(areas)], actions, holding });`,
        AREA,
    ),
    Schema.decodeEffect(Read),
);
const described = (answer: typeof Answer.Type) => ('described' in answer ? Effect.succeed(answer.described) : Effect.fail(new Cause.UnknownError(answer, `facet answered ${Formatter.format(answer)}`)));
const operated = (answer: typeof Answer.Type) => ('operated' in answer ? Effect.succeed(answer.operated) : Effect.fail(new Cause.UnknownError(answer, `facet answered ${Formatter.format(answer)}`)));
const preflighted = (answer: typeof Answer.Type) => ('preflighted' in answer ? Effect.succeed(answer.preflighted) : Effect.fail(new Cause.UnknownError(answer, `facet answered ${Formatter.format(answer)}`)));
const typeset = (answer: typeof Answer.Type) => ('typeset' in answer ? Effect.succeed(answer.typeset) : Effect.fail(new Cause.UnknownError(answer, `facet answered ${Formatter.format(answer)}`)));
const controls = ({ containers }: Description, kind: FacetContainer['kind']): readonly FacetControl[] => Array.flatMap(containers, (container) => (container.kind === kind ? container.controls : []));
const Held = Schema.fromJsonString(Schema.Struct({ document: Schema.Int, tool: Schema.String }));
const loaded = Effect.acquireRelease(
    Effect.flatMap(Host, ({ placed }) =>
        Effect.flatMap(scripted("const { app } = require('indesign'); const tool = String(app.toolBoxTools.currentTool); const document = app.documents.add(true); document.placeGuns.loadPlaceGun(script.args[0]); return JSON.stringify({ document: document.id, tool });", placed), Schema.decodeEffect(Held)),
    ),
    ({ document, tool }) => Effect.orDie(scripted("const { app, SaveOptions, UITools } = require('indesign'); const [document, tool] = script.args; app.documents.itemByID(Number(document)).close(SaveOptions.NO); app.toolBoxTools.currentTool = UITools[tool]; return 'closed';", `${document}`, tool)),
);
const labelled =
    (text: string) =>
    ({ label }: Pick<FacetControl, 'label'>) =>
        label?.text === text;
const named =
    (text: string) =>
    ({ label }: { readonly label: string }) =>
        label === text;
const hiddenToggle = ({ value }: FacetControl) => value?._tag === 'bool' && !value.value;
const panelRow = ({ label, value }: FacetControl) => [label?.text, value];
const actionRow = ({ label, enabled, mark }: FacetControl) => ({ name: label?.text, enabled, checked: mark !== undefined });
const areaName = ({ label }: FacetControl) => label?.text;
const domPanel = ([name, visible]: readonly [string, boolean]) => [name, { _tag: 'bool', value: visible }];
const describes = Effect.fnUntraced(function* () {
    yield* loaded;
    const [top, read] = yield* Effect.all([Effect.flatMap(facet({ describe: {} }), described), dom]);
    const [choice] = controls(top, 'toolbar');
    const area = yield* Effect.fromOption(Array.findFirst(controls(top, 'menuBar'), labelled(AREA)), () => new Cause.NoSuchElementError(`${AREA} menu area`));
    const level = yield* Effect.flatMap(facet({ describe: { within: area.locator } }), described);
    assert.isTrue(read.holding.length === 1 && read.tool === 'PLACE_CURSOR_TOOL', `setup loads one item into the place gun, read ${Formatter.format(read)}`);
    assert.deepStrictEqual(
        {
            tool: choice?.value,
            tools: Array.map(choice?.options ?? [], Struct.get('label')).toSorted(),
            panels: Array.map(controls(top, 'panel'), panelRow),
            areas: Array.map(controls(top, 'menuBar'), areaName),
            actions: Array.map(controls(level, 'menu'), actionRow),
            pointer: top.pointer,
        },
        { tool: { _tag: 'text', value: read.name }, tools: Array.map(read.tools, (tool) => (tool === read.tool ? read.name : tool)).toSorted(), panels: Array.map(read.panels, domPanel), areas: read.areas, actions: read.actions, pointer: { tool: read.name, holding: read.holding } },
    );
}, Effect.scoped);
const operates = Effect.fnUntraced(function* () {
    const [top, before] = yield* Effect.all([Effect.flatMap(facet({ describe: {} }), described), dom]);
    const options = controls(top, 'toolbar')[0]?.options ?? [];
    const [chosen, hidden] = yield* Effect.all([Effect.fromOption(Array.findFirst(options, named(TYPE.id)), () => new Cause.NoSuchElementError(`${TYPE.id} option`)), Effect.fromOption(Array.findFirst(controls(top, 'panel'), hiddenToggle), () => new Cause.NoSuchElementError('hidden panel'))]);
    assert.notStrictEqual(before.tool, TYPE.id, 'setup starts from another tool');
    yield* Effect.addFinalizer(() => Effect.orDie(Effect.all([facet({ operate: { locator: hidden.locator, to: false } }), scripted("const { app, UITools } = require('indesign'); app.toolBoxTools.currentTool = UITools[script.args[0]]; return 'restored';", before.tool)])));
    yield* Effect.all([Effect.flatMap(facet({ operate: { locator: chosen.locator } }), operated), Effect.flatMap(facet({ operate: { locator: hidden.locator, to: true } }), operated)]);
    const [after, read] = yield* Effect.all([Effect.flatMap(facet({ describe: {} }), described), dom]);
    const panel = hidden.label?.text ?? '';
    assert.deepStrictEqual(
        {
            tool: read.tool,
            name: read.name,
            toolRead: controls(after, 'toolbar')[0]?.value,
            panel: Option.getOrUndefined(Array.findFirst(read.panels, ([name, visible]) => (name === panel ? Option.some(visible) : Option.none()))),
            panelRead: Option.getOrUndefined(Option.map(Array.findFirst(controls(after, 'panel'), labelled(panel)), Struct.get('value'))),
        },
        { tool: TYPE.id, name: TYPE.name, toolRead: { _tag: 'text', value: TYPE.name }, panel: true, panelRead: { _tag: 'bool', value: true } },
    );
}, Effect.scoped);
const refuses = Effect.fnUntraced(function* () {
    const [top, before] = yield* Effect.all([Effect.flatMap(facet({ describe: {} }), described), dom]);
    const [choice] = controls(top, 'toolbar');
    const option = yield* Effect.fromOption(Array.head(choice?.options ?? []), () => new Cause.NoSuchElementError('tool option'));
    const refused = yield* facet({ operate: { locator: [...Array.initNonEmpty(option.locator), ['tool', UNLISTED]] } });
    const after = yield* dom;
    assert.isTrue('failed' in refused && refused.failed.includes(`No UITools member matching ${UNLISTED}`), `an unlisted tool fails Unmatched over the UITools members, answered ${Formatter.format(refused)}`);
    assert.strictEqual(after.tool, before.tool, 'a refused tool leaves the current tool as it was');
});
const names = Effect.fnUntraced(function* () {
    const [top, before] = yield* Effect.all([Effect.flatMap(facet({ describe: {} }), described), dom]);
    const [choice] = controls(top, 'toolbar');
    const locator = yield* Effect.fromOption(Option.map(Option.fromUndefinedOr(choice), Struct.get('locator')), () => new Cause.NoSuchElementError('tool choice'));
    assert.notStrictEqual(before.tool, TYPE.id, 'setup starts from another tool');
    yield* Effect.addFinalizer(() => Effect.orDie(scripted("const { app, UITools } = require('indesign'); app.toolBoxTools.currentTool = UITools[script.args[0]]; return 'restored';", before.tool)));
    yield* Effect.flatMap(facet({ operate: { locator, to: TYPE.name } }), operated);
    assert.deepStrictEqual(Struct.pick(yield* dom, ['tool', 'name']), { tool: TYPE.id, name: TYPE.name });
}, Effect.scoped);
const nests = Effect.fnUntraced(function* () {
    const id = yield* Effect.acquireRelease(scripted("const { app } = require('indesign'); const document = app.documents.add(false); document.pages.item(0).textFrames.add({ geometricBounds: [0, 0, 20, 20], contents: script.args[0] }); return String(document.id);", OVERSET), (held) =>
        Effect.orDie(scripted("const { app, SaveOptions } = require('indesign'); app.documents.itemByID(Number(script.args[0])).close(SaveOptions.NO); return 'closed';", held)),
    );
    const [checked] = yield* Effect.flatMap(facet({ preflight: { document: `/document[@id=${id}]`, profile: PROFILE } }), preflighted);
    const categories = checked?.aggregatedResults ?? [];
    const overset = Array.findFirst(Array.flatMap(categories, Struct.get('rows')), ({ label }) => label.startsWith('Overset text'));
    assert.deepStrictEqual(
        { categoriesHoldRules: Array.every(categories, ({ rows }) => Array.isReadonlyArrayNonEmpty(rows)), oversetIssues: Option.map(overset, ({ rows }) => Array.map(rows, ({ rows: below }) => below.length)) },
        { categoriesHoldRules: true, oversetIssues: Option.some([0]) },
        `${PROFILE} rows nest issue under rule under category, answered ${Formatter.format(categories)}`,
    );
}, Effect.scoped);
const features = Effect.fnUntraced(function* () {
    const { document, frame } = yield* Effect.acquireRelease(
        Effect.flatMap(
            scripted("const { app } = require('indesign'); const document = app.documents.add(false); const frame = document.pages.item(0).textFrames.add({ geometricBounds: [0, 0, 50, 100], contents: script.args[0] }); return JSON.stringify({ document: document.id, frame: frame.toSpecifier() });", FIGURES),
            Schema.decodeEffect(Framed),
        ),
        ({ document: held }) => Effect.orDie(scripted("const { app, SaveOptions } = require('indesign'); app.documents.itemByID(Number(script.args[0])).close(SaveOptions.NO); return 'closed';", `${held}`)),
    );
    const settings = yield* Effect.flatMap(facet({ typeset: { target: frame, settings: { features: CHOSEN } } }), typeset);
    const read = yield* Effect.flatMap(
        scripted("const { app } = require('indesign'); const text = app.documents.itemByID(Number(script.args[0])).textFrames.item(0).texts.item(0); return JSON.stringify({ figures: String(text.otfFigureStyle), position: String(text.position) });", `${document}`),
        Schema.decodeEffect(Styled),
    );
    assert.deepStrictEqual({ read, chosen: Struct.pick(settings.features, ['tnum', 'lnum', 'sups']) }, { read: { figures: 'TABULAR_LINING', position: 'OT_SUPERSCRIPT' }, chosen: CHOSEN });
}, Effect.scoped);
const hears = Effect.fnUntraced(function* () {
    const before = yield* dom;
    yield* Effect.addFinalizer(() => Effect.orDie(scripted("const { app, UITools } = require('indesign'); app.toolBoxTools.currentTool = UITools[script.args[0]]; return 'restored';", before.tool)));
    const answered = yield* facet({ listen: { plugin: MANIFEST.id, tools: [TYPE.id, RECTANGLE] } });
    assert.deepStrictEqual(answered, { heard: { both: true, after: true, listeners: 1 } });
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const application = Option.flatMap(Option.liftThrowable(() => execFileSync('lsappinfo', ['info', '-only', 'bundlepath', execFileSync('lsappinfo', ['find', 'bundleid=com.adobe.InDesign'], { encoding: 'utf-8' }).trim().split(/\s+/u)[0] ?? ''], { encoding: 'utf-8' }))(), (text) =>
    Option.fromUndefinedOr(BUNDLE_PATH.exec(text)?.groups?.['path']),
);

describe.skipIf(Option.isNone(application))('InDesign facets against the running InDesign', () => {
    it.layer(Host.layer(Option.getOrElse(application, () => '')).pipe(Layer.provideMerge(layer)), { timeout: TIMEOUT })((methods) => {
        methods.effect('describe states the current tool by its name, every tool of the UITools enumeration but NONE as an option, each named panel with its visibility, every menu area, one area level, and the loaded place gun as the DOM reads them', describes, TIMEOUT);
        methods.effect('operate selects a tool by its option keyed by the UITools member and shows a hidden panel, the DOM and a later describe each reading the change', operates, TIMEOUT);
        methods.effect('operate on a tool the UITools enumeration lacks fails Unmatched over its members and leaves the current tool', refuses, TIMEOUT);
        methods.effect('operate on the tool choice takes a tool by the name the host shows for it', names, TIMEOUT);
        methods.effect('preflight of a document holding one overset frame nests its one issue under the Overset text rule under its category', nests, TIMEOUT);
        methods.effect('typeset with features tnum, lnum, and sups sets otfFigureStyle to TABULAR_LINING and position to OT_SUPERSCRIPT, and reads those three features back', features, TIMEOUT);
        methods.effect('two subscribers to one host event each hear it through one host listener, and the one whose scope stays open hears the next after the other closed', hears, TIMEOUT);
    });
});
