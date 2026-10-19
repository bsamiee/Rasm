import type { Document } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Effect, Option, Predicate, Schema, Struct } from 'effect';
import { Host } from './host.ts';
import { Classes } from './objects/classes.ts';
import { Reference, read, Specifier } from './objects/specifiers.ts';
import { Mutation } from './objects/transactions.ts';

// --- [MODELS] --------------------------------------------------------------------------

class Session extends Schema.TaggedClass<Session, { readonly brand: unique symbol }>()('Session', {
    version: Schema.String,
    status: Schema.TaggedUnion({
        Modal: {},
        Ready: {
            tool: Schema.String,
            panels: Schema.Array(Schema.String),
            documents: Schema.Array(Reference.pipe(Schema.fieldsAssign({ modified: Schema.Boolean, saved: Schema.Boolean, path: Schema.OptionFromOptionalKey(Schema.String), windows: Schema.Array(Specifier) }))),
            active: Schema.OptionFromOptionalKey(
                Schema.Struct({
                    document: Specifier,
                    window: Specifier,
                    selection: Schema.Array(
                        Reference.pipe(
                            Schema.fieldsAssign({
                                contents: Schema.OptionFromOptionalKey(Schema.String),
                                story: Schema.OptionFromOptionalKey(Specifier),
                                overrides: Schema.OptionFromOptionalKey(Schema.Struct(Struct.Record(['paragraph', 'character'], Schema.Boolean))),
                            }),
                        ),
                    ),
                    overset: Schema.Array(Specifier),
                    undo: Schema.Struct({ mode: Schema.String, name: Schema.String, history: Schema.Array(Schema.String) }),
                }),
            ),
        },
    }),
}) {
    static readonly current = Effect.fn('current')(function* () {
        const [{ indesign }, { reference }] = yield* Effect.all([Host, Classes]);
        const { app, StyleType } = indesign;
        const modal = app.modalState;
        const documents = modal ? [] : app.documents.everyItem().getElements();
        const paths = yield* Effect.forEach(documents, (document) => (document.saved ? Effect.asSome(Effect.promise(() => document.fullName)) : Effect.succeedNone));
        const opened = Option.filter(Option.liftThrowable(() => app.activeDocument)(), () => !modal);
        const text = (item: object, name: string): string | undefined => Option.getOrUndefined(Schema.decodeUnknownOption(Schema.String)(Reflect.get(item, name)));
        const panels = Array.flatMap(
            Array.makeBy(app.panels.length, (index) => app.panels.item(index)),
            (panel) => Option.getOrElse(Option.liftThrowable(() => (panel.visible ? [panel.name] : []))(), () => []),
        );
        const selection = () =>
            Array.map(app.selection, (item) => ({
                ...reference(item),
                contents: text(item, 'contents'),
                story: 'parentStory' in item ? item.parentStory : undefined,
                overrides: 'textHasOverrides' in item ? Option.getOrUndefined(Option.liftThrowable(() => ({ paragraph: item.textHasOverrides(StyleType.PARAGRAPH_STYLE_TYPE), character: item.textHasOverrides(StyleType.CHARACTER_STYLE_TYPE) }))()) : undefined,
            }));
        const overset = (document: Document): readonly unknown[] => Array.flatMap(Array.zip(document.stories.everyItem().overflows, document.stories.everyItem().id), ([overflows, id]) => (overflows === true ? [document.stories.itemByID(id)] : []));
        const live = Option.map(opened, (document) => ({
            document,
            window: app.activeWindow,
            selection: selection(),
            overset: overset(document),
            undo: { mode: app.activeScriptUndoMode, name: document.undoName, history: document.undoHistory },
        }));
        const listed = Array.zipWith(documents, paths, (document, path) => ({ ...reference(document), modified: document.modified, saved: document.saved, path: Option.getOrUndefined(Option.map(path, Struct.get('nativePath'))), windows: document.windows.everyItem().getElements() }));
        return yield* read(Session, () => ({ _tag: 'Session', version: app.version, status: modal ? { _tag: 'Modal' } : { _tag: 'Ready', tool: app.toolBoxTools.currentTool, panels, documents: listed, active: Option.getOrUndefined(live) } }));
    });
}
class MenuAction extends Schema.Class<MenuAction, { readonly brand: unique symbol }>('MenuAction')({ id: Schema.Int, name: Schema.String, area: Schema.String }) {
    static readonly listed = Effect.fn('listed')(function* (area: Option.Option<string>) {
        const { indesign } = yield* Host;
        const rows = yield* read(Schema.Array(MenuAction), () => {
            const every = indesign.app.menuActions.everyItem();
            return Array.zipWith(Array.zip(every.id, every.name), every.area, ([id, name], within) => ({ id, name, area: within }));
        });
        const chosen = Option.match(area, { onNone: () => rows, onSome: (wanted) => rows.filter((action) => action.area === wanted) });
        return yield* Option.isSome(area) && Array.isReadonlyArrayEmpty(chosen) ? Effect.fail(Unmatched.make({ subject: 'menu area', query: area.value, candidates: Array.dedupe(Array.map(rows, Struct.get('area'))) })) : Effect.succeed(chosen);
    });
}
class Invoke extends Mutation.extend<Invoke>('Invoke')({ action: Schema.Union([Schema.Int, Schema.String]).annotate({ description: 'Menu action id or name from the menu resource' }) }, { description: 'Invokes one menu action as the user would, without dialogs, in one undo step' }) {
    static readonly Result = Mutation.Result(MenuAction.fields);

    override get label(): string {
        return `Invoke ${this.action}`;
    }

    run() {
        const { action } = this;
        return this.commit(
            Invoke.Result,
            Effect.gen(function* () {
                const { indesign } = yield* Host;
                const rows = yield* MenuAction.listed(Option.none());
                const chosen = yield* Array.match(
                    rows.filter((row) => (Predicate.isNumber(action) ? row.id === action : row.name === action)),
                    {
                        onEmpty: () => Effect.fail(Unmatched.make({ subject: 'menu action', query: String(action), candidates: Array.map(rows, Struct.get('name')) })),
                        onNonEmpty: ([only, ...rest]) => (rest.length === 0 ? Effect.succeed(only) : Effect.fail(Unmatched.make({ subject: 'single menu action', query: String(action), candidates: [only, ...rest].map(({ id, area }) => `${id} (${area})`) }))),
                    },
                );
                yield* Effect.try({ try: () => indesign.app.menuActions.itemByID(chosen.id).invoke(), catch: (cause) => new Cause.UnknownError(cause, chosen.name) });
                return chosen;
            }),
        );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Invoke, MenuAction, Session };
