import { ArrowUpRight, Minus, Plus } from 'lucide-react';
import type { ReactElement, ReactNode } from 'react';
import { Button } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

function ZoomControls({ zoomOut, fit, zoomIn, href, linkLabel, children }: { zoomOut: () => void; fit: () => void; zoomIn: () => void; href: string; linkLabel: string; children?: ReactNode }): ReactElement {
    return (
        <div className="flex flex-wrap items-center gap-2 pt-1 pb-2 landscape-short:col-start-2 landscape-short:row-start-1 landscape-short:py-0 [&>button]:min-w-11">
            <Button aria-label="Zoom out" className="button button-outline" onPress={zoomOut}>
                <Minus className="size-4" strokeLinecap="butt" />
            </Button>
            <Button className="button button-outline" onPress={fit}>
                Fit
            </Button>
            <Button aria-label="Zoom in" className="button button-outline" onPress={zoomIn}>
                <Plus className="size-4" strokeLinecap="butt" />
            </Button>
            {children}
            <a className="ml-auto gap-1 text-[13px] text-link" href={href} rel="noreferrer" target="_blank">
                {linkLabel}
                <ArrowUpRight className="size-6" strokeLinecap="butt" strokeLinejoin="miter" />
            </a>
        </div>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ZoomControls };
