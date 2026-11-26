import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, describe, it } from '@effect/vitest';
import { Description } from '@rasm/applications/source';
import { run } from '@rasm/process';
import { pluginConfig } from '@rasm/uxp/vite';
import { Array, Cause, Context, Effect, FileSystem, Formatter, Layer, Option, Path, Schema, Struct } from 'effect';
import { build, mergeConfig } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = { manifestVersion: 5, id: 'app.rasm.test.source', name: 'Source', version: '0.1.0', host: { app: 'ID' }, entrypoints: [], requiredPermissions: {} };
const AREA = 'File Menu';
const TOOLS = 'Tools';
const TYPE = 'Type Tool';
const PLACED = 'placed text\n';
const BUNDLE_PATH = /bundle path="(?<path>[^"]+)"/u;
const TIMEOUT = 120_000;

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);
const Answer = Schema.fromJsonString(Schema.Union([Schema.Struct({ described: Description }), Schema.Struct({ operated: Schema.Boolean }), Schema.Struct({ failed: Schema.String })]));
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
            const [manifest, entry] = yield* Effect.all([Schema.encodeEffect(Json)(MANIFEST), Schema.encodeEffect(Json)(fileURLToPath(import.meta.resolve('./source-host.ts')))]);
            yield* Effect.all([fs.writeFileString(path.join(folder, 'manifest.json'), manifest), fs.writeFileString(path.join(folder, 'main.ts'), `export { run } from ${entry};\n`), fs.writeFileString(path.join(folder, 'placed.txt'), PLACED)], { discard: true });
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
        `const { app } = require('indesign');
const [area, toolbox] = script.args;
const every = app.menuActions.everyItem();
const panels = [];
for (let index = 0; index < app.panels.length; index += 1) { const panel = app.panels.item(index); if (panel !== undefined && panel.name !== '') { panels.push([panel.name, panel.visible]); } }
const [ids, areas, names] = [every.id, every.area, every.name];
const actions = ids.filter((id, index) => areas[index] === area).map((id) => app.menuActions.itemByID(id)).map((action) => ({ name: action.name, enabled: action.enabled, checked: action.checked }));
const holding = app.documents.length === 1 && app.documents.item(0).placeGuns.loaded ? app.documents.item(0).placeGuns.pageItems.everyItem().getElements().map((item) => item.toSpecifier()) : [];
return JSON.stringify({ tool: String(app.toolBoxTools.currentTool), name: app.toolBoxTools.currentToolName, tools: names.filter((name, index) => areas[index] === toolbox), panels, areas: [...new Set(areas)], actions, holding });`,
        AREA,
        TOOLS,
    ),
    Schema.decodeEffect(Read),
);
const described = (answer: typeof Answer.Type) => ('described' in answer ? Effect.succeed(answer.described) : Effect.fail(new Cause.UnknownError(answer, `facet answered ${Formatter.format(answer)}`)));
const operated = (answer: typeof Answer.Type) => ('operated' in answer ? Effect.succeed(answer.operated) : Effect.fail(new Cause.UnknownError(answer, `facet answered ${Formatter.format(answer)}`)));
const controls = ({ containers }: Description, kind: (typeof Description.Container.Type)['kind']): readonly (typeof Description.Control.Type)[] => Array.flatMap(containers, (container) => (container.kind === kind ? container.controls : []));
const Held = Schema.fromJsonString(Schema.Struct({ document: Schema.Int, tool: Schema.String }));
const loaded = Effect.acquireRelease(
    Effect.flatMap(Host, ({ placed }) =>
        Effect.flatMap(scripted("const { app } = require('indesign'); const tool = String(app.toolBoxTools.currentTool); const document = app.documents.add(true); document.placeGuns.loadPlaceGun(script.args[0]); return JSON.stringify({ document: document.id, tool });", placed), Schema.decodeEffect(Held)),
    ),
    ({ document, tool }) => Effect.orDie(scripted("const { app, SaveOptions, UITools } = require('indesign'); const [document, tool] = script.args; app.documents.itemByID(Number(document)).close(SaveOptions.NO); app.toolBoxTools.currentTool = UITools[tool]; return 'closed';", `${document}`, tool)),
);
const labelled =
    (text: string) =>
    ({ label }: Pick<typeof Description.Control.Type, 'label'>) =>
        label?.text === text;
const named =
    (text: string) =>
    ({ label }: { readonly label: string }) =>
        label === text;
const hiddenToggle = ({ value }: typeof Description.Control.Type) => value !== undefined && 'bool' in value && !value.bool.value;
const panelRow = ({ label, value }: typeof Description.Control.Type) => [label?.text, value];
const actionRow = ({ label, enabled, mark }: typeof Description.Control.Type) => ({ name: label?.text, enabled, checked: mark !== undefined });
const areaName = ({ label }: typeof Description.Control.Type) => label?.text;
const domPanel = ([name, visible]: readonly [string, boolean]) => [name, { bool: { value: visible } }];
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
        { tool: { text: { value: read.name } }, tools: read.tools.toSorted(), panels: Array.map(read.panels, domPanel), areas: read.areas, actions: read.actions, pointer: { tool: read.name, holding: read.holding } },
    );
}, Effect.scoped);
const operates = Effect.fnUntraced(function* () {
    const [top, before] = yield* Effect.all([Effect.flatMap(facet({ describe: {} }), described), dom]);
    const options = controls(top, 'toolbar')[0]?.options ?? [];
    const [chosen, hidden] = yield* Effect.all([Effect.fromOption(Array.findFirst(options, named(TYPE)), () => new Cause.NoSuchElementError(`${TYPE} option`)), Effect.fromOption(Array.findFirst(controls(top, 'panel'), hiddenToggle), () => new Cause.NoSuchElementError('hidden panel'))]);
    assert.notStrictEqual(before.name, TYPE, 'setup starts from another tool');
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
        { tool: 'TYPE_TOOL', name: TYPE, toolRead: { text: { value: TYPE } }, panel: true, panelRead: { bool: { value: true } } },
    );
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const application = Option.flatMap(Option.liftThrowable(() => execFileSync('lsappinfo', ['info', '-only', 'bundlepath', execFileSync('lsappinfo', ['find', 'bundleid=com.adobe.InDesign'], { encoding: 'utf-8' }).trim().split(/\s+/u)[0] ?? ''], { encoding: 'utf-8' }))(), (text) =>
    Option.fromUndefinedOr(BUNDLE_PATH.exec(text)?.groups?.['path']),
);

describe.skipIf(Option.isNone(application))('InDesign facets against the running InDesign', () => {
    it.layer(Host.layer(Option.getOrElse(application, () => '')).pipe(Layer.provideMerge(layer)), { timeout: TIMEOUT })((methods) => {
        methods.effect('describe states the current tool name among the toolbox actions by name, each named panel with its visibility, every menu area, one area level, and the loaded place gun as the DOM reads them', describes, TIMEOUT);
        methods.effect('operate selects a tool by its toolbox option and shows a hidden panel, the DOM and a later describe each reading the change by name', operates, TIMEOUT);
    });
});
