import { KeyStep } from '@rasm/applications/contract';
import type { Declaration } from '@rasm/applications/source';
import { readVersionAndDescriptor } from 'ag-psd/dist/descriptor.js';
import { createReader, readSignature, readUint16 } from 'ag-psd/dist/psdReader.js';
import { Array, Cache, Context, Duration, Effect, Exit, FileSystem, Function, Glob, HashMap, Layer, Match, Option, Path, Predicate, Record, Result, Schema, SchemaGetter, SchemaIssue, Stream } from 'effect';
import { Xml } from 'effect/encoding';
import type { Application } from '#/application';

// --- [TYPES] ---------------------------------------------------------------------------

type Row = readonly [key: string, tool: HashMap.HashMap.Value<Declaration['tools']>];
type Entry = string | { readonly [name: string]: Entry };
interface Place {
    readonly application: Application;
    readonly bundle: string;
    readonly install: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MODIFIERS: Record.ReadonlyRecord<string, KeyStep['modifiers'][number]> = { Cmd: 'command', Shift: 'shift', Opt: 'option', Ctrl: 'control', Control: 'control' };
const KEYS: Record.ReadonlyRecord<string, KeyStep['key']> = {
    'Up Arrow': 'up',
    'Down Arrow': 'down',
    'Left Arrow': 'left',
    'Right Arrow': 'right',
    'Page Up': 'pageUp',
    'Page Down': 'pageDown',
    Return_Mac: 'return',
    Enter_Mac: 'enter',
    Delete_Mac: 'delete',
    Del_Mac: 'forwardDelete',
    'Num 0': 0x52,
    'Num 1': 0x53,
    'Num 2': 0x54,
    'Num 3': 0x55,
    'Num 4': 0x56,
    'Num 5': 0x57,
    'Num 6': 0x58,
    'Num 7': 0x59,
    'Num 8': 0x5b,
    'Num 9': 0x5c,
    'Num +': 0x45,
    'Num -': 0x4e,
    'Num /': 0x4b,
};
const SHORTCUT = new RegExp(`^(?<held>(?:(?:${Record.keys(MODIFIERS).map(RegExp.escape).join('|')})\\+)*)(?:#Keyboard_)?(?<key>.+)$`, 'u');
const ILLUSTRATOR = { modifiers: { shift: 32, command: 64, option: 128 }, tab: 9, functions: 13 } as const;
const INDESIGN = { context: 'DefaultContext', set: 'Default.indk' } as const;
const CATALOG = { locale: 'en', technique: 'tool_technique', pages: 'https://helpx.adobe.com/', hosts: { PHXS: 'PHSP' } as Record.ReadonlyRecord<string, string> } as const;
const ESCAPED = /\\(?<escaped>.)/gu;
const TOKEN = /\/(?<name>(?:\\.|[^\s\\/{}[\]<>])*)|(?<open>\{)|(?<close>\})|\[\s*\d+(?<bytes>[\da-f\s]*)\]|<\s*\d+(?<bytes>[\da-f\s]*)>|\((?<text>(?:\\.|[^\\)])*)\)|(?<word>[^\s/{}[\]()<>]+)/giu;

// --- [MODELS] --------------------------------------------------------------------------

const named = Schema.is(KeyStep.fields.key.members[0]);
const refused = (expected: string, input: unknown) => new SchemaIssue.InvalidValue({ expected }, input);
const Shortcut = Schema.String.pipe(
    Schema.decodeTo(KeyStep, {
        decode: SchemaGetter.transformEffect((text: string) => {
            const { held = '', key = '' } = SHORTCUT.exec(text)?.groups ?? {};
            const resolved = [...key].length === 1 ? Option.some(key.toLowerCase()) : Option.orElse(Option.liftPredicate(key.toLowerCase(), named), () => Record.get(KEYS, key));
            const modifiers = Array.filterMap(held.split('+'), (name) => Result.fromOption(Record.get(MODIFIERS, name), Function.constVoid));
            return Effect.fromOption(Option.map(resolved, (step) => ({ key: step, modifiers }))).pipe(Effect.mapError(() => refused('a key the shortcut set names', text)));
        }),
        encode: SchemaGetter.forbiddenEncoding,
    }),
).annotate({ description: 'Shortcut text of a shortcut set, each held modifier joined by + before the key' });
const CharId = Schema.Int.pipe(
    Schema.decodeTo(Schema.String, {
        decode: SchemaGetter.transform((code: number) => `$${String.fromCodePoint(...Array.map([24, 16, 8, 0], (shift) => (code >>> shift) & 0xff))}`),
        encode: SchemaGetter.forbiddenEncoding,
    }),
).annotate({ description: 'Four-character tool code as getActiveTool().key spells it' });
const PhotoshopTools = Schema.Array(Schema.Struct({ attributes: Schema.Struct({ name: Schema.String, key: Schema.FiniteFromString.pipe(Schema.decodeTo(CharId)) }), children: Schema.Array(Shortcut) }));
const Toolbar = Schema.Uint8Array.pipe(
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transformEffect((bytes: Uint8Array) =>
            Effect.try({
                try: () => {
                    const reader = createReader(bytes.buffer, bytes.byteOffset, bytes.byteLength);
                    const signature = readSignature(reader);
                    readUint16(reader);
                    const descriptor: unknown = readVersionAndDescriptor(reader);
                    return { signature, descriptor };
                },
                catch: () => refused('a Photoshop settings descriptor', bytes),
            }),
        ),
        encode: SchemaGetter.forbiddenEncoding,
    }),
    Schema.decodeTo(Schema.Struct({ signature: Schema.Literal('8BPF'), descriptor: Schema.Struct({ tlst: Schema.Array(Schema.Array(Schema.Struct({ toolKey: CharId }))) }) })),
);
const Catalog = Schema.fromJsonString(
    Schema.Struct({
        meta: Schema.Struct({ host: Schema.String.annotate({ description: 'UXP host appCode the catalog serves' }) }),
        items: Schema.Array(
            Schema.Struct({
                name: Schema.String,
                itemId: Schema.String,
                context: Schema.Struct({ trigger: Schema.optionalKey(Schema.String) }),
                cmHelpXDetail: Schema.optionalKey(Schema.Array(Schema.Struct({ id: Schema.String.annotate({ description: 'helpx.adobe.com page path' }) }))),
            }),
        ),
    }),
);
const Dictionary = Schema.String.pipe(
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transformEffect((text: string) => Effect.try({ try: () => entries(text.matchAll(TOKEN)), catch: () => refused('an Illustrator dictionary', text) })),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);
const IllustratorKey = Schema.Struct({ Modifiers: Schema.FiniteFromString.pipe(Schema.decodeTo(Schema.Int)), Key: Schema.FiniteFromString.pipe(Schema.decodeTo(Schema.Int)) }).pipe(
    Schema.decodeTo(Schema.Struct({ shortcut: Schema.optionalKey(KeyStep) }), {
        decode: SchemaGetter.transformEffect(({ Modifiers, Key }: { readonly Modifiers: number; readonly Key: number }) => {
            const held = Record.keys(Record.filter(ILLUSTRATOR.modifiers, (bit) => (Modifiers & bit) !== 0));
            const unread = Modifiers & ~Array.reduce(Record.values(ILLUSTRATOR.modifiers), 0, (mask, bit) => mask | bit);
            const stated = Match.value(Key).pipe(
                Match.when(ILLUSTRATOR.tab, () => Option.some<KeyStep['key']>('tab')),
                Match.when(
                    (code) => code >= 0x20 && code < 0x7f,
                    (code) => Option.some(String.fromCodePoint(code).toLowerCase()),
                ),
                Match.orElse((code) => Option.liftPredicate(`f${code - ILLUSTRATOR.functions}`, named)),
            );
            return Match.value({ Key, unread }).pipe(
                Match.when({ unread: (bits) => bits !== 0 }, () => Effect.fail(refused('Shift, Command, and Option modifier bits', Modifiers))),
                Match.when({ Key: 0 }, () => Effect.succeed({})),
                Match.orElse(() => Effect.mapError(Effect.fromOption(Option.map(stated, (step) => ({ shortcut: { key: step, modifiers: held } }))), () => refused('Tab, a function key, or a printable character code', Key))),
            );
        }),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);
const IllustratorSet = Dictionary.pipe(Schema.decodeTo(Schema.Struct({ Tools: Schema.Record(Schema.String, IllustratorKey) })));
const IllustratorPrefs = Dictionary.pipe(
    Schema.decodeTo(
        Schema.Struct({
            plugin: Schema.Struct({
                KBSCShortcutFile: Schema.optionalKey(Schema.Struct({ IsPreset: Schema.Union([Schema.Literal('1').transform(true), Schema.Literal('0').transform(false)]).annotate({ description: 'The set is a preset of the build, else a set the user saved in the settings folder' }), FileName: Schema.StringFromHex })),
                AIMCPServer: Schema.optionalKey(Schema.Struct({ AuthToken: Schema.StringFromHex.annotate({ description: 'Bearer token the MCP server of the build takes, written once the server is enabled' }) })),
            }),
        }),
    ),
);
const RunConfig = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Unknown)).annotate({ description: 'Illustrator .runConfig, keyed by the application version that wrote it' });
const IndesignShortcuts = Schema.Array(
    Schema.Struct({
        'action-id': Schema.Struct({
            attributes: Schema.Struct({
                value: Schema.TemplateLiteralParser([Schema.FiniteFromString, ' + ', Schema.FiniteFromString]).pipe(Schema.decodeTo(Schema.String, { decode: SchemaGetter.transform(([area, , offset]: readonly [number, ' + ', number]) => `${area + offset}`), encode: SchemaGetter.forbiddenEncoding })),
            }),
        }),
        context: Schema.Struct({ children: Schema.Tuple([Schema.String]) }),
        string: Schema.Struct({ children: Schema.Tuple([Shortcut]) }),
    }),
);

// --- [SERVICES] ------------------------------------------------------------------------

class Preferences extends Context.Service<Preferences, string>()('@rasm/creative-cloud-desktop/tools/Preferences') {
    static readonly illustrator = Effect.fnUntraced(function* (application: Application) {
        const [path, files] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
        const configs = yield* Effect.filter(yield* settings(application, '*/.runConfig'), (file) => Effect.map(Effect.flatMap(files.readFileString(file), Schema.decodeEffect(RunConfig)), Record.has(application.version)));
        return yield* Effect.forEach(configs, (file) => Effect.map(Effect.flatMap(files.readFileString(path.join(path.dirname(file), 'Adobe Illustrator Prefs')), Schema.decodeEffect(IllustratorPrefs)), ({ plugin }) => ({ folder: path.dirname(file), plugin })));
    });
}
class Tools extends Context.Service<Tools, (application: Application) => Effect.Effect<Declaration, Effect.Error<ReturnType<typeof read>>>>()('@rasm/creative-cloud-desktop/tools') {
    static readonly layer = Layer.effect(
        Tools,
        Effect.map(
            Cache.makeWith((application: Application) => read(application), { capacity: Number.POSITIVE_INFINITY, timeToLive: (exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero) }),
            (cache) => (application: Application) => Cache.get(cache, application),
        ),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const entries = (tokens: Iterator<RegExpExecArray, undefined>): Readonly<Record<string, Entry>> => Object.fromEntries(Iterator.from({ next: () => pair(tokens) }));
const pair = (tokens: Iterator<RegExpExecArray, undefined>): IteratorResult<readonly [string, Entry], undefined> => {
    const head = tokens.next().value;
    const name = head?.groups?.['name'];
    const value = name === undefined ? undefined : tokens.next().value?.groups;
    if (head === undefined || head.groups?.['close'] !== undefined) {
        return { done: true, value: undefined };
    }
    if (name === undefined || value === undefined) {
        throw new SyntaxError(`${head[0]} stands where a /name and its value belong`);
    }
    return { done: false, value: [name.replaceAll(ESCAPED, '$<escaped>'), value['open'] === undefined ? (value['bytes']?.replaceAll(/\s/gu, '') ?? (value['text'] ?? value['name'])?.replaceAll(ESCAPED, '$<escaped>') ?? value['word'] ?? '') : entries(tokens)] };
};
const found = Effect.fnUntraced(function* (root: string, pattern: string) {
    const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return Array.map(yield* files.glob(pattern, { root, dot: true }), (each) => path.join(root, each));
});
const settings = Effect.fnUntraced(function* (application: Application, pattern: string) {
    const [path, preferences] = yield* Effect.all([Path.Path, Preferences]);
    const bundle = yield* path.fromFileUrl(application.url);
    const files = yield* found(preferences, `${Glob.escape(path.basename(bundle, '.app'))} *Settings/${pattern}`);
    return files.filter((file) => /\bBeta\b/u.test(path.relative(preferences, file).split(path.sep, 1).join('')) === (application.channel === 'beta'));
});
const elements = (file: string, at: Array.NonEmptyReadonlyArray<string>) => Stream.unwrap(FileSystem.FileSystem.useSync((files) => files.stream(file))).pipe(Stream.pipeThroughChannel(Xml.decode()), Stream.pipeThroughChannel(Xml.elements(at)), Stream.runCollect);
const photoshop = Effect.fnUntraced(function* ({ application }: Place) {
    const files = yield* FileSystem.FileSystem;
    const [sets, toolbars] = yield* Effect.all([settings(application, 'Keyboard Shortcuts.psp'), settings(application, 'Toolbar Customization.psp')]);
    const listed = yield* Effect.forEach(sets, (file) => elements(file, ['photoshop-keyboard-shortcuts', 'tool']));
    const tools = yield* Schema.decodeUnknownEffect(PhotoshopTools)(Array.filter(Array.flatten(listed), ({ attributes }) => Record.has(attributes, 'key')));
    const groups = yield* Effect.forEach(toolbars, (file) => Effect.flatMap(files.readFile(file), Schema.decodeEffect(Toolbar)));
    return {
        rows: Array.map(tools, ({ attributes: { name, key }, children }): Row => [key, { label: name, ...Record.getSomes({ shortcut: Array.head(children) }) }]),
        toolbar: Array.flatMap(groups, ({ descriptor }) =>
            Array.map(
                descriptor.tlst,
                Array.map(({ toolKey }) => toolKey),
            ),
        ),
    };
});
const illustrator = Effect.fnUntraced(function* ({ application, install }: Place) {
    const [path, files] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const chosen = Array.flatMap(yield* Preferences.illustrator(application), ({ folder, plugin }) =>
        Option.toArray(Option.map(Option.fromUndefinedOr(plugin.KBSCShortcutFile), ({ IsPreset, FileName }) => path.join(IsPreset ? path.join(install, 'Presets.localized', path.basename(folder), 'Keyboard Shortcuts') : folder, `${FileName}.kys`))),
    );
    const sets = yield* Effect.filter(chosen, (file) => files.exists(file));
    const decoded = yield* Effect.forEach(sets, (file) => Effect.flatMap(files.readFileString(file), Schema.decodeEffect(IllustratorSet)));
    return Array.flatMap(decoded, ({ Tools: listed }): readonly Row[] => Record.toEntries(listed));
});
const indesign = Effect.fnUntraced(function* ({ install }: Place) {
    const path = yield* Path.Path;
    const sets = yield* found(path.join(install, 'Presets', 'InDesign Shortcut Sets'), `*/${INDESIGN.set}`);
    const listed = yield* Effect.forEach(sets, (file) => elements(file, ['shortcut-set', 'shortcut']));
    const fields = Array.map(Array.flatten(listed), ({ children }) => Record.fromEntries(Array.filterMap(children, (child) => (Predicate.isString(child) ? Result.failVoid : Result.succeed([child.name, child] as const)))));
    const decoded = yield* Schema.decodeUnknownEffect(IndesignShortcuts)(fields);
    return Array.filterMap(decoded, ({ 'action-id': { attributes }, context, string }) => (context.children[0] === INDESIGN.context ? Result.succeed<Row>([attributes.value, { shortcut: string.children[0] }]) : Result.failVoid));
});
const catalogs = Effect.fnUntraced(function* ({ application, bundle }: Place) {
    const [path, files] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const listed = yield* found(path.join(bundle, 'Contents', 'Required', 'UXP'), `**/com.adobe.unifiedpanel/locale/${CATALOG.locale}/*/curatedContent.json`);
    const decoded = yield* Effect.forEach(listed, (file) => Effect.flatMap(files.readFileString(file), Schema.decodeEffect(Catalog)));
    const served = Array.filter(decoded, ({ meta: { host } }) => Option.getOrElse(Record.get(CATALOG.hosts, host), () => host) === application.product);
    return Array.filterMap(
        Array.flatMap(served, ({ items }) => items),
        ({ name, itemId, context, cmHelpXDetail = [] }) => (context.trigger === CATALOG.technique ? Result.succeed<Row>([itemId, { label: name, ...Record.getSomes({ help: Option.map(Array.head(cmHelpXDetail), ({ id }) => new URL(`${id}.html`, CATALOG.pages).href) }) }]) : Result.failVoid),
    );
});
const read = Effect.fnUntraced(function* (application: Application) {
    const path = yield* Path.Path;
    const bundle = yield* path.fromFileUrl(application.url);
    const place = { application, bundle, install: path.dirname(bundle) };
    const [{ rows, toolbar }, drawn, laid, catalogued] = yield* Effect.all([photoshop(place), illustrator(place), indesign(place), catalogs(place)], { concurrency: 'unbounded' });
    const declaration: Declaration = { tools: Array.reduce([...rows, ...drawn, ...laid, ...catalogued], HashMap.empty<string, Row[1]>(), (held, [key, tool]) => HashMap.set(held, key, { ...tool, ...Option.getOrUndefined(HashMap.get(held, key)) })), toolbar };
    return declaration;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Preferences, Tools };
