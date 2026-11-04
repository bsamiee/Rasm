import { useAtomSet } from '@effect/atom-react';
import { Number } from 'effect';
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
            const adjust = useAtomSet(mounted);
            useEffect(() => {
                adjust(Number.increment);
                return () => adjust(Number.decrement);
            }, [adjust]);
            return children;
        },
    },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Backdrop };
