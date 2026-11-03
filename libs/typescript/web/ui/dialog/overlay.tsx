import { RegistryContext } from '@effect/atom-react';
import { Atom } from 'effect/reactivity';
import { type ReactNode, use, useEffect } from 'react';
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
            const registry = use(RegistryContext);
            useEffect(() => {
                registry.update(mounted, (count) => count + 1);
                return () => registry.update(mounted, (count) => count - 1);
            }, [registry]);
            return children;
        },
    },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ModalOverlay };
