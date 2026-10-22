import { Button } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const MediaLoading = () => (
    <span className="text-muted pointer-events-none absolute inset-0 grid place-items-center text-xs" role="status">
        Loading…
    </span>
);
const MediaFailure = ({ message, href, linkLabel, onRetry }: { message: string; href: string; linkLabel: string; onRetry?: () => void }) => (
    <div className="flex flex-col items-center gap-3 p-5.5 text-center text-sm" role="status">
        <p>{message}</p>
        {onRetry && (
            <Button className="button button-outline" onPress={onRetry}>
                Try again
            </Button>
        )}
        <a className="text-link" href={href} rel="noreferrer" target="_blank">
            {linkLabel}
        </a>
    </div>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { MediaFailure, MediaLoading };
