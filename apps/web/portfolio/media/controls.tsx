import { ArrowUpRight, Minus, Plus } from 'lucide-react';
import type { ReactNode } from 'react';
import { Button, Toolbar } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ZoomControls = ({ zoomOut, fit, zoomIn, href, linkLabel, children }: { zoomOut: () => void; fit: ReactNode; zoomIn: () => void; href: string; linkLabel: string; children?: ReactNode }) => (
    <div className="viewer-controls">
        <Toolbar aria-label="Zoom controls" className="viewer-toolbar order-first *:px-2">
            <Button aria-label="Zoom out" className="button button-ghost" onPress={zoomOut}>
                <Minus className="size-4" />
            </Button>
            {fit}
            <Button aria-label="Zoom in" className="button button-ghost" onPress={zoomIn}>
                <Plus className="size-4" />
            </Button>
        </Toolbar>
        {children}
        <a className="-order-1 ml-auto min-h-11 gap-1 text-caption text-link" href={href} rel="noreferrer" target="_blank">
            {linkLabel}
            <ArrowUpRight className="size-4" />
        </a>
    </div>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ZoomControls };
