import { Unmatched } from '@rasm/search/unmatched.ts';
import { Array, Effect, Schema } from 'effect';
import { read } from '../dom/encoding.ts';
import { Model } from '../dom/model.ts';

// --- [MODELS] --------------------------------------------------------------------------

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

// --- [EXPORTS] -------------------------------------------------------------------------

export { Menu };
