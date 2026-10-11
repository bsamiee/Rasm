import { ArrowUpRight, Minus, Plus } from 'lucide-react';
import type { ReactElement, ReactNode } from 'react';
import { Button, Toolbar } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

function ZoomControls({ zoomOut, fit, zoomIn, href, linkLabel, children }: { zoomOut: () => void; fit: ReactNode; zoomIn: () => void; href: string; linkLabel: string; children?: ReactNode }): ReactElement {
    return (
        <div className="flex max-h-[calc(var(--visual-viewport-height)*0.4)] shrink-0 flex-wrap items-center gap-x-2 gap-y-1 overflow-y-auto border-line border-y py-2 landscape-short:col-span-full landscape-short:row-start-2 landscape-short:py-1">
            <Toolbar aria-label="Zoom controls" className="order-first flex w-fit max-w-full items-center [&>button]:min-w-11 [&>button]:selected:bg-transparent [&>button]:px-2 [&>button]:selected:text-foreground [&>button]:selected:decoration-accent-text">
                <Button aria-label="Zoom out" className="button button-ghost" onPress={zoomOut}>
                    <Minus className="size-4" />
                </Button>
                {fit}
                <Button aria-label="Zoom in" className="button button-ghost" onPress={zoomIn}>
                    <Plus className="size-4" />
                </Button>
            </Toolbar>
            {children}
            <a className="-order-1 ml-auto min-h-11 gap-1 text-[13px] text-link" href={href} rel="noreferrer" target="_blank">
                {linkLabel}
                <ArrowUpRight className="size-4" />
            </a>
        </div>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ZoomControls };
