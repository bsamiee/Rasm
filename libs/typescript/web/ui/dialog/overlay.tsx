import { useAtomSet } from '@effect/atom-react';
import { type ReactNode, useEffect } from 'react';
import { composeRenderProps, type ModalOverlayProps, ModalOverlay as Overlay } from 'react-aria-components';
import { openDialogs } from '#/dialog/open';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Open = ({ children }: { readonly children: ReactNode }) => {
    const count = useAtomSet(openDialogs);
    useEffect(() => {
        count((open) => open + 1);
        return () => count((open) => open - 1);
    }, [count]);
    return children;
};
const ModalOverlay = ({ children, ...props }: ModalOverlayProps) => (
    <Overlay {...props}>
        {composeRenderProps(children, (content) => (
            <Open>{content}</Open>
        ))}
    </Overlay>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ModalOverlay };
