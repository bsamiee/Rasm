import { useAtomSet } from '@effect/atom-react';
import { Atom } from 'effect/reactivity';
import { type ReactNode, useEffect } from 'react';
import { composeRenderProps, type ModalOverlayProps, ModalOverlay as Overlay } from 'react-aria-components';

// --- [SERVICES] ------------------------------------------------------------------------

const mounted = Atom.make(0);

// --- [COMPOSITION] ---------------------------------------------------------------------

const Counted = ({ children }: { readonly children: ReactNode }) => {
    const setMounted = useAtomSet(mounted);
    useEffect(() => {
        setMounted((count) => count + 1);
        return () => setMounted((count) => count - 1);
    }, [setMounted]);
    return children;
};
const ModalOverlay = Object.assign(
    ({ children, ...props }: ModalOverlayProps) => (
        <Overlay {...props}>
            {composeRenderProps(children, (content) => (
                <Counted>{content}</Counted>
            ))}
        </Overlay>
    ),
    { open: Atom.readable((get) => get(mounted)) },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ModalOverlay };
