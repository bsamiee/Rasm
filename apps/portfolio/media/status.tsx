import type { ReactElement } from 'react';
import { Button } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

function MediaLoading(): ReactElement {
    return (
        <span className="pointer-events-none absolute inset-0 grid place-items-center text-muted text-xs" role="status">
            Loading…
        </span>
    );
}
function MediaFailure({ message, href, linkLabel, onRetry, className }: { message: string; href: string; linkLabel: string; onRetry: () => void; className?: string }): ReactElement {
    return (
        <div className={`flex flex-col items-center gap-3 p-[22px] text-center text-sm ${className ?? ''}`} role="status">
            <p>{message}</p>
            <Button className="button button-outline" onPress={onRetry}>
                Try again
            </Button>
            <a className="text-link" href={href} rel="noreferrer" target="_blank">
                {linkLabel}
            </a>
        </div>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { MediaFailure, MediaLoading };
