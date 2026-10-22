import { Unmatched } from '@rasm/search/unmatched.ts';
import { Cause, Effect, Option, Schema } from 'effect';
import { Model } from '../dom/model.ts';
import { Mutation } from '../dom/transactions.ts';
import { Menu } from './menu.ts';

// --- [MODELS] --------------------------------------------------------------------------

class Invoke extends Mutation.extend<Invoke>('Invoke')(
    { action: Schema.Union([Schema.Int, Schema.String]).annotate({ description: 'Menu action id or name' }) },
    { description: 'Invokes one menu action as its menu item does, in one undo step: an action whose command opens a dialog holds the call until the dialog is answered' },
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

export { Invoke };
