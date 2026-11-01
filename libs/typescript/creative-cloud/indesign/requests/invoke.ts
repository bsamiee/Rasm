import { Unmatched } from '@rasm/search/unmatched';
import { Cause, Effect, Option, Schema } from 'effect';
import { Model } from '#/dom/model';
import { Mutation } from '#/dom/transactions';
import { Menu } from '#/requests/menu';

// --- [MODELS] --------------------------------------------------------------------------

class Invoke extends Mutation.extend<Invoke>('Invoke')({ action: Schema.Union([Schema.Int, Schema.String]).annotate({ description: 'Menu action id or name' }) }, { description: 'Invokes menu action in one undo step, a dialog it opens waits for an answer' }) {
    override get label(): string {
        return `Invoke ${this.action}`;
    }

    run() {
        return Menu.make({ area: Option.none() })
            .run()
            .pipe(
                Effect.flatMap(Unmatched.single('menu action', Option.some(String(this.action)), ({ id, name, area }) => [`${id} ${name} (${area})`, String(id), name])),
                Effect.flatMap((chosen) => this.commit(Model.use(({ app }) => Effect.as(Effect.try({ try: () => app.menuActions.itemByID(chosen.id).invoke(), catch: (cause) => new Cause.UnknownError(cause, chosen.name) }), chosen)))),
            );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Invoke };
