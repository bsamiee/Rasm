import { Unmatched } from '@rasm/search';
import { Array, Cause, Effect, Option, pipe, Schema, Struct } from 'effect';
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
class Menu extends Schema.Class<Menu, { readonly brand: unique symbol }>('Menu')({ area: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Menu area, in any case' })) }, { description: 'Menu actions by id, name, and area, every area or one' }) {
    static readonly Action = Schema.Struct({ id: Schema.Int, name: Schema.String, area: Schema.String });
    static readonly Result = Schema.Array(Menu.Action);

    run() {
        const { area } = this;
        return Model.use(({ app }) =>
            read(Menu.Result, 'menu actions', () => {
                const every = app.menuActions.everyItem();
                return Array.zipWith(Array.zip(every.id, every.name), every.area, ([id, name], within) => ({ id, name, area: within }));
            }),
        ).pipe(Effect.flatMap((rows) => Unmatched.matching('menu area', area)(rows, (row) => [row.area])));
    }
}
class Invoke extends Mutation.extend<Invoke>('Invoke')(
    { action: Schema.Union([Schema.Int, Schema.String]).annotate({ description: 'Menu action id or name from the menu resource' }) },
    {
        description: 'Invokes one menu action under never-interact, in one undo step: a command runs as its menu item runs it, and an action whose command opens a dialog returns at once having opened no dialog and changed nothing',
    },
) {
    override get label(): string {
        return `Invoke ${this.action}`;
    }

    run() {
        const { action } = this;
        return Effect.flatMap(
            Effect.flatMap(Menu.make({ area: Option.none() }).run(), (rows) => Unmatched.single('menu action', Option.some(String(action)))(rows, ({ id, name, area }) => [`${id} ${name} (${area})`, String(id), name])),
            (chosen) => this.commit(Model.use(({ app }) => Effect.as(Effect.try({ try: () => app.menuActions.itemByID(chosen.id).invoke(), catch: (cause) => new Cause.UnknownError(cause, chosen.name) }), chosen))),
        );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Invoke, Menu, Session };
