import { Option, Schema } from 'effect';
import { Atom } from 'effect/reactivity';
import type { Bootstrap } from '#/model/api';
import { Portfolio, type PortfolioData } from '#/model/portfolio';

// --- [CONSTANTS] -----------------------------------------------------------------------

const edit = { name: 'edit', value: 'true' } as const;

// --- [SERVICES] ------------------------------------------------------------------------

const bootstrap = Atom.make<typeof Bootstrap.Type>({ published: { portfolio: Portfolio.make({}), assets: {} }, session: 'signedOut' });
const editing = Atom.searchParam(edit.name, { schema: Schema.Literal(edit.value) }).pipe(Atom.withServerValue(() => Option.none()));
const requested = Atom.make((get) => get(bootstrap).session === 'allowed' && Option.isSome(get(editing))).pipe(Atom.withServerValue(() => false));
const editor = Atom.writable<'idle' | 'open' | 'closed', 'open' | 'closed'>(
    (get) => (get(requested) ? 'open' : 'idle'),
    (context, view) => context.setSelf(view),
).pipe(Atom.withServerValue(() => 'idle'));
const revised = Atom.make(Option.none<typeof PortfolioData.Type>());
const preview = Atom.make(Option.none<typeof PortfolioData.Type>());
const published = Atom.make((get) => Option.getOrElse(get(revised), () => get(bootstrap).published));
const shown = Atom.make((get) => Option.getOrElse(get(preview), () => get(published)).portfolio);

// --- [EXPORTS] -------------------------------------------------------------------------

export { bootstrap, edit, editor, preview, published, requested, revised, shown };
