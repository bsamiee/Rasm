import { HostError } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Option, Schema } from 'effect';
import { Model } from '#/dom/model';
import { commit } from '#/dom/transactions';
import { read } from '#/dom/value';

// --- [MODELS] --------------------------------------------------------------------------

class Menu extends Schema.Class<Menu, { readonly brand: unique symbol }>('Menu')({ area: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Area in any case' })) }) {
    static readonly Action = Schema.Struct({ id: Schema.Int, name: Schema.String, area: Schema.String });
    static readonly Result = Schema.Array(Menu.Action);
    static readonly handle = ({ area }: Menu) =>
        Model.use(({ app }) =>
            read(Menu.Result, 'menu actions', () => {
                const every = app.menuActions.everyItem();
                return Array.zipWith(Array.zip(every.id, every.name), every.area, ([id, name], within) => ({ id, name, area: within }));
            }),
        ).pipe(Effect.flatMap(Unmatched.matching('menu area', area, (row) => [row.area])));
}
class Invoke extends Schema.Class<Invoke, { readonly brand: unique symbol }>('Invoke')({ action: Schema.Union([Schema.Int, Schema.String]).annotate({ description: 'Menu action id or name' }) }) {
    static readonly Result = Menu.Action;
    static readonly handle = (payload: Invoke) =>
        Menu.handle(Menu.make({ area: Option.none() })).pipe(
            Effect.flatMap(Unmatched.single('menu action', Option.some(String(payload.action)), ({ id, name, area }) => [`${id} ${name} (${area})`, String(id), name])),
            Effect.flatMap((chosen) =>
                commit(
                    payload.label,
                    Model.use(({ app }) => Effect.as(Effect.try({ try: () => app.menuActions.itemByID(chosen.id).invoke(), catch: HostError.refused(chosen.name) }), chosen)),
                ),
            ),
        );

    get label(): string {
        return `Invoke ${this.action}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Invoke, Menu };
