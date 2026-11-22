import { useAtomSet } from '@effect/atom-react';
import { Number } from 'effect';
import { Atom } from 'effect/reactivity';
import { type ReactNode, useEffect } from 'react';

// --- [SERVICES] ------------------------------------------------------------------------

const mounted = Atom.make(0);
const openDialogs = Atom.readable((get) => get(mounted));

// --- [COMPOSITION] ---------------------------------------------------------------------

const Counted = ({ children }: { readonly children: ReactNode }) => {
    const adjust = useAtomSet(mounted);
    useEffect(() => {
        adjust(Number.increment);
        return () => adjust(Number.decrement);
    }, [adjust]);
    return children;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Counted, openDialogs };
