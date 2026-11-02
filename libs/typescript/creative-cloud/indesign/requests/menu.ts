import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Option, Schema } from 'effect';
import { Model } from '#/dom/model';
import { Mutation } from '#/dom/transactions';
import { read } from '#/dom/value';
import { InDesignError } from '#/error';

// --- [MODELS] --------------------------------------------------------------------------

class Menu extends Schema.Class<Menu, { readonly brand: unique symbol }>('Menu')({ area: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Area in any case' })) }) {
    static readonly Action = Schema.Struct({ id: Schema.Int, name: Schema.String, area: Schema.String });
    static readonly Result = Schema.Array(Menu.Action);

    run() {
        const { area } = this;
        return Model.use(({ app }) =>
            read(Menu.Result, 'menu actions', () => {
                const every = app.menuActions.everyItem();
                return Array.zipWith(Array.zip(every.id, every.name), every.area, ([id, name], within) => ({ id, name, area: within }));
            }),
        ).pipe(Effect.flatMap(Unmatched.matching('menu area', area, (row) => [row.area])));
    }
}
class Invoke extends Mutation.extend<Invoke>('Invoke')({ action: Schema.Union([Schema.Int, Schema.String]).annotate({ description: 'Menu action id or name' }) }) {
    override get label(): string {
        return `Invoke ${this.action}`;
    }

    run() {
        return Menu.make({ area: Option.none() })
            .run()
            .pipe(
                Effect.flatMap(Unmatched.single('menu action', Option.some(String(this.action)), ({ id, name, area }) => [`${id} ${name} (${area})`, String(id), name])),
                Effect.flatMap((chosen) => this.commit(Model.use(({ app }) => Effect.as(Effect.try({ try: () => app.menuActions.itemByID(chosen.id).invoke(), catch: InDesignError.refused(chosen.name) }), chosen)))),
            );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Invoke, Menu };
