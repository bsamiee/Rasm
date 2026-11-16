import { ArrowUpRight, Minus, Plus } from 'lucide-react';
import type { ReactNode } from 'react';
import { Button, Toolbar } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ZoomControls = ({ className, href, fit, zoomIn, zoomOut, children }: { readonly className?: string | undefined; readonly href: string; readonly fit: ReactNode; readonly zoomIn: () => void; readonly zoomOut: () => void; readonly children?: ReactNode }) => (
    <div className={className}>
        <Toolbar aria-label="Zoom">
            <Button aria-label="Zoom out" onPress={zoomOut}>
                <Minus />
            </Button>
            {fit}
            <Button aria-label="Zoom in" onPress={zoomIn}>
                <Plus />
            </Button>
        </Toolbar>
        <a href={href} rel="noreferrer" target="_blank">
            Original
            <ArrowUpRight />
        </a>
        {children}
    </div>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ZoomControls };
