import { useAtomSet } from '@effect/atom-react';
import { Atom } from 'effect/reactivity';
import { type ReactNode, useEffect } from 'react';
import { composeRenderProps, ModalOverlay, type ModalOverlayProps } from 'react-aria-components';

// --- [SERVICES] ------------------------------------------------------------------------

const mounted = Atom.make(0);

// --- [COMPOSITION] ---------------------------------------------------------------------

const Backdrop = Object.assign(
    ({ children, ...props }: ModalOverlayProps) => (
        <ModalOverlay {...props}>
            {composeRenderProps(children, (content) => (
                <Backdrop.Counted>{content}</Backdrop.Counted>
            ))}
        </ModalOverlay>
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

export { Backdrop };
