import { Atom } from 'effect/reactivity';

// --- [SERVICES] ------------------------------------------------------------------------

const openDialogs = Atom.make(new Set<string>());
const dialogOpen = Atom.family((id: string) =>
    Atom.writable(
        (get) => get(openDialogs).has(id),
        (context, open: boolean) => {
            const current = context.get(openDialogs);
            context.set(openDialogs, open ? current.union(new Set([id])) : current.difference(new Set([id])));
        },
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { dialogOpen, openDialogs };
