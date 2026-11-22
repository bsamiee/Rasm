import { Button } from 'react-aria-components';
import { assetUrl } from '#/model/api.ts';
import type { Asset } from '#/model/asset.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const MediaLoading = () => <output className="pointer-events-none absolute inset-0 grid place-items-center text-xs text-muted">Loading…</output>;
const MediaFailure = ({ asset, message, onRetry }: { asset: typeof Asset.Type; message: string; onRetry?: () => void }) => (
    <output className="flex flex-col items-center gap-3 p-5.5 text-center text-sm">
        <span>{message}</span>
        {onRetry && (
            <Button className="button button-outline" onPress={onRetry}>
                Try again
            </Button>
        )}
        <a className="text-link" href={assetUrl(asset)} rel="noreferrer" target="_blank">
            {{ image: 'Open original image', video: 'Open original video', pdf: 'Open original PDF' }[asset.kind]}
        </a>
    </output>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { MediaFailure, MediaLoading };
