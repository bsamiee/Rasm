import { Button } from 'react-aria-components';
import { assetUrl } from '#/model/api';
import type { Asset } from '#/model/asset';

// --- [COMPOSITION] ---------------------------------------------------------------------

const MediaLoading = () => (
    <span className="text-muted pointer-events-none absolute inset-0 grid place-items-center text-xs" role="status">
        Loading…
    </span>
);
const MediaFailure = ({ asset, message, onRetry }: { asset: typeof Asset.Type; message: string; onRetry?: () => void }) => (
    <div className="flex flex-col items-center gap-3 p-5.5 text-center text-sm" role="status">
        <p>{message}</p>
        {onRetry && (
            <Button className="button button-outline" onPress={onRetry}>
                Try again
            </Button>
        )}
        <a className="text-link" href={assetUrl(asset)} rel="noreferrer" target="_blank">
            {{ image: 'Open original image', video: 'Open original video', pdf: 'Open original PDF' }[asset.kind]}
        </a>
    </div>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { MediaFailure, MediaLoading };
