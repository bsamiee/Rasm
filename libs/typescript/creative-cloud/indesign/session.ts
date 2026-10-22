import { Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Cause, Effect, Option, pipe, Record, Schema, SchemaGetter, Struct } from 'effect';
import Builder from 'fast-xml-builder';
import { XMLParser } from 'fast-xml-parser';
import { DocumentView } from './documents.ts';
import { Model } from './model.ts';
import { active, Reference, read, Specifier } from './references.ts';
import { Overrides, TextView } from './text.ts';
import { Mutation } from './transactions.ts';

// --- [MODELS] --------------------------------------------------------------------------

class Session extends Schema.TaggedClass<Session, { readonly brand: unique symbol }>()('Session', {
    version: Schema.String,
    tool: Schema.String,
    panels: Schema.Array(Schema.String),
    documents: Schema.Array(DocumentView.Row),
    active: Schema.OptionFromOptionalKey(
        Schema.Struct({
            document: Specifier,
            window: Specifier,
            selection: Schema.Array(Reference.mapFields(Struct.assign({ contents: Schema.OptionFromOptionalKey(Schema.Json), parentStory: Schema.OptionFromOptionalKey(Specifier), overrides: Schema.OptionFromOptionalKey(Overrides) }))),
            overset: Schema.Array(Specifier),
            undo: Schema.Struct({ mode: Schema.String, name: Schema.String, history: Schema.Array(Schema.String) }),
        }),
    ),
}) {
    static readonly current = Effect.gen(function* () {
        const { app } = yield* Model;
        const rows = yield* Effect.forEach(app.documents.everyItem().getElements(), DocumentView.row);
        const opened = yield* Effect.option(active);
        const selection = yield* Effect.forEach(Option.match(opened, { onNone: Array.empty, onSome: () => app.selection }), (item) =>
            pipe(
                Option.liftPredicate(item, (each) => 'textHasOverrides' in each),
                Option.map(TextView.overrides),
                Effect.transposeOption,
                Effect.map((overrides) => ({
                    ...Reference.of(item),
                    contents: Option.fromUndefinedOr('contents' in item ? item.contents : undefined),
                    parentStory: 'parentStory' in item ? Option.some(item.parentStory.toSpecifier()) : Option.none(),
                    overrides,
                })),
            ),
        );
        const panels = Array.getSomes(
            Array.makeBy(app.panels.length, (index) =>
                Option.map(
                    Option.filter(Option.fromUndefinedOr(app.panels.item(index)), ({ visible }) => visible),
                    Struct.get('name'),
                ),
            ),
        );
        return yield* read(Session, 'session', () => ({
            _tag: 'Session',
            version: app.version,
            tool: app.toolBoxTools.currentTool,
            panels,
            documents: rows,
            active: Option.map(opened, (document) => ({
                document: document.toSpecifier(),
                window: app.activeWindow.toSpecifier(),
                selection,
                overset: DocumentView.overset(document),
                undo: { mode: app.activeScriptUndoMode, name: document.undoName, history: document.undoHistory },
            })),
        }));
    });
}
const Binding = Schema.Struct({
    context: Schema.String.annotate({ description: 'Context the shortcut works in, as shortcut set files spell it: DefaultContext, TextContext, TableContext' }),
    string: Schema.String.annotate({ description: 'Keys the shortcut presses, as shortcut set files spell them: Cmd+N, Shift+Cmd+S, Opt+#Keyboard_F12' }),
});
const ShortcutSet = Schema.String.pipe(
    Schema.decodeTo(
        Schema.Struct({
            '?xml': Schema.Record(Schema.String, Schema.String),
            'shortcut-set': Schema.Struct({
                ...Struct.Record(['@_platform', '@_file-version', '@_application-version', '@_build-number'], Schema.optionalKey(Schema.String)),
                shortcut: Schema.Array(
                    Schema.Struct({
                        'action-id': Schema.Struct({
                            '@_value': Schema.TemplateLiteralParser(['0x', Schema.String, ' + ', Schema.Int]).pipe(
                                Schema.decodeTo(Schema.Int, { decode: SchemaGetter.transform(([, base, , offset]) => Number.parseInt(base, 16) + offset), encode: SchemaGetter.transform((id: number) => ['0x', '0', ' + ', id] as const) }),
                            ),
                            '@_name': Schema.optionalKey(Schema.String),
                        }),
                        ...Binding.fields,
                    }),
                ).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
            }),
        }),
        {
            decode: SchemaGetter.transform((text: string) => new XMLParser({ ignoreAttributes: false, parseTagValue: false, isArray: (name) => name === 'shortcut' }).parse(text)),
            encode: SchemaGetter.transform((set) => new Builder({ ignoreAttributes: false, format: true, indentBy: '  ' }).build(set)),
        },
    ),
).annotate({ description: 'Shortcut set file (.indk) InDesign applies by name' });
class Menu extends Schema.Class<Menu, { readonly brand: unique symbol }>('Menu')(
    { area: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Menu area, in any case' })) },
    { description: 'Menu actions by id, name, and area with the keyboard shortcuts the active shortcut set binds, every area or one' },
) {
    static readonly Action = Schema.Struct({ id: Schema.Int, name: Schema.String, area: Schema.String, shortcuts: Schema.Array(Binding) });
    static readonly Result = Schema.Array(Menu.Action);
    static readonly located = Effect.fnUntraced(function* (name: string) {
        const [{ folders }, { fs }] = yield* Effect.all([Model, Uxp]);
        const { settings, presets, locale } = yield* folders;
        const folder = `${settings}/InDesign Shortcut Sets`;
        const path = `${folder}/${name}.indk`;
        const loaded = (origin: 'factory' | 'user', at: string) =>
            pipe(
                Effect.try({ try: () => fs.readFileSync(`file:${at}`, { encoding: 'utf-8' }), catch: (cause) => new Cause.UnknownError(cause, at) }),
                Effect.map((text) => ({ origin, text })),
                Effect.asSome,
                Effect.catchIf(
                    ({ cause }) => Schema.is(Schema.Struct({ code: Schema.Literal(-2) }))(cause),
                    () => Effect.succeedNone,
                ),
            );
        const found = yield* Effect.filterOrElse(loaded('factory', `${presets}/InDesign Shortcut Sets/${locale}/${name}.indk`), Option.isSome, () => loaded('user', path));
        return { folder, path, found: yield* Effect.transposeOption(Option.map(found, ({ origin, text }) => Effect.map(Schema.decodeEffect(ShortcutSet)(text), (set) => ({ origin, set })))) };
    });
    static readonly active = Effect.gen(function* () {
        const { app } = yield* Model;
        const name = app.generalPreferences.keyboardShortcutSet;
        const { found } = yield* Menu.located(name);
        return yield* Effect.fromOption(Option.map(found, Struct.get('set')), () => Unmatched.make({ subject: 'shortcut set file', query: Option.some(name), candidates: [] }));
    });
    static readonly chosen = (action: number | string) => Effect.flatMap(Menu.make({ area: Option.none() }).run(), (rows) => Unmatched.single('menu action', Option.some(String(action)))(rows, ({ id, name, area }) => [`${id} ${name} (${area})`, String(id), name]));

    run() {
        const { area } = this;
        return Effect.flatMap(Effect.all([Model, Menu.active]), ([{ app }, set]) => {
            const bound = Array.groupBy(set['shortcut-set'].shortcut, (entry) => String(entry['action-id']['@_value']));
            return read(Menu.Result, 'menu actions', () => {
                const every = app.menuActions.everyItem();
                return Array.zipWith(Array.zip(every.id, every.name), every.area, ([id, name], within) => ({ id, name, area: within, shortcuts: Option.getOrElse(Record.get(bound, String(id)), Array.empty) }));
            });
        }).pipe(Effect.flatMap((rows) => Unmatched.matching('menu area', area)(rows, (row) => [row.area])));
    }
}
class Invoke extends Mutation.extend<Invoke>('Invoke')(
    { action: Schema.Union([Schema.Int, Schema.String]).annotate({ description: 'Menu action id or name from the menu resource' }) },
    { description: 'Invokes one menu action as its menu item does, in one undo step: an action whose command opens a dialog holds the call until the dialog is answered' },
) {
    override get label(): string {
        return `Invoke ${this.action}`;
    }

    run() {
        return Effect.flatMap(Menu.chosen(this.action), (chosen) => this.commit(Model.use(({ app }) => Effect.as(Effect.try({ try: () => app.menuActions.itemByID(chosen.id).invoke(), catch: (cause) => new Cause.UnknownError(cause, chosen.name) }), chosen))));
    }
}
class Shortcut extends Schema.Class<Shortcut, { readonly brand: unique symbol }>('Shortcut')(
    {
        action: Invoke.fields.action,
        shortcuts: Schema.Array(Binding).annotate({ description: 'Every shortcut the action holds afterwards, none to unbind it' }),
        set: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'User shortcut set to write and apply; the active set when absent' })),
    },
    {
        description:
            "Binds one menu action's keyboard shortcuts as desired state: writes the named user shortcut set, seeded from the active set when the user holds none of that name, and applies it. An action losing its shortcut to this one loses that binding alone, every other binding stays, and a factory set is refused, since InDesign applies it from its install folder over any user set of that name",
    },
) {
    run() {
        const { action, shortcuts, set } = this;
        return Effect.gen(function* () {
            const [chosen, { app, ScriptLanguage }, { fs }] = yield* Effect.all([Menu.chosen(action), Model, Uxp]);
            const name = Option.getOrElse(set, () => app.generalPreferences.keyboardShortcutSet);
            const { folder, path, found } = yield* Menu.located(name);
            const source = yield* Option.match(found, {
                onNone: () => Menu.active,
                onSome: ({ origin, set: held }) => (origin === 'user' ? Effect.succeed(held) : Effect.fail(new Cause.IllegalArgumentError(`${name} is a factory shortcut set, applied from the install folder over any user set of that name; name a user set`))),
            });
            const kept = source['shortcut-set'].shortcut.filter((entry) => entry['action-id']['@_value'] !== chosen.id && !shortcuts.some(({ context, string }) => entry.context === context && entry.string === string));
            const text = yield* Schema.encodeEffect(ShortcutSet)({ ...source, 'shortcut-set': { ...source['shortcut-set'], shortcut: [...kept, ...Array.map(shortcuts, (binding) => ({ 'action-id': { '@_value': chosen.id }, ...binding }))] } });
            yield* Effect.try({
                try: () => {
                    app.doScript('Folder(arguments[0]).create()', ScriptLanguage.JAVASCRIPT, [folder]);
                    fs.writeFileSync(`file:${path}`, text, { encoding: 'utf-8' });
                    app.applyShortcutSet(name);
                },
                catch: (cause) => new Cause.UnknownError(cause, `shortcut set ${name}`),
            });
            return { ...chosen, shortcuts };
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Invoke, Menu, Session, Shortcut };
