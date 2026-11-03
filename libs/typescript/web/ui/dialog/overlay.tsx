import { useAtomSet } from '@effect/atom-react';
import { Atom } from 'effect/reactivity';
import { type ReactNode, useEffect } from 'react';
import { composeRenderProps, type ModalOverlayProps, ModalOverlay as Overlay } from 'react-aria-components';

// --- [SERVICES] ------------------------------------------------------------------------

const mounted = Atom.make(0);

// --- [COMPOSITION] ---------------------------------------------------------------------

const ModalOverlay = Object.assign(
    ({ children, ...props }: ModalOverlayProps) => (
        <Overlay {...props}>
            {composeRenderProps(children, (content) => (
                <ModalOverlay.Counted>{content}</ModalOverlay.Counted>
            ))}
        </Overlay>
    ),
    {
        open: Atom.readable((get) => get(mounted)),
        Counted: ({ children }: { readonly children: ReactNode }) => {
            const count = useAtomSet(mounted);
            useEffect(() => {
                count((open) => open + 1);
                return () => count((open) => open - 1);
            }, [count]);
            return children;
        },
    },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ModalOverlay };
