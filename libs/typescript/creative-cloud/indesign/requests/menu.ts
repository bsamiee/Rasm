import { HostError } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Option } from 'effect';
import { type Invoke, Menu } from '#/contract';
import { Model } from '#/dom/model';
import { commit } from '#/dom/transactions';

// --- [OPERATIONS] ----------------------------------------------------------------------

const menu = ({ area }: Menu) =>
    Model.use(({ app }) =>
        Model.read(Menu.Result, 'menu actions', () => {
            const every = app.menuActions.everyItem();
            return Array.zipWith(Array.zip(every.id, every.name), every.area, ([id, name], within) => ({ id, name, area: within }));
        }),
    ).pipe(Effect.flatMap(Unmatched.matching('menu area', area, (row) => [row.area])));
const invoke = (payload: Invoke) =>
    menu(Menu.make({ area: Option.none() })).pipe(
        Effect.flatMap(Unmatched.single('menu action', Option.some(String(payload.action)), ({ id, name, area }) => [`${id} ${name} (${area})`, String(id), name])),
        Effect.flatMap((chosen) =>
            commit(
                payload.label,
                Model.use(({ app }) => Effect.as(Effect.try({ try: () => app.menuActions.itemByID(chosen.id).invoke(), catch: HostError.refused(chosen.name) }), chosen)),
            ),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { invoke, menu };
