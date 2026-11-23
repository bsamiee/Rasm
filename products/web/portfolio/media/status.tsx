import { Button } from 'react-aria-components';
import { assetUrl } from '#/model/api';
import { Asset } from '#/model/asset';

// --- [COMPOSITION] ---------------------------------------------------------------------

const MediaLoading = () => <output className="media-loading">Loading…</output>;
const MediaFailure = ({ asset, message, onRetry }: { readonly asset: typeof Asset.Type; readonly message: string; readonly onRetry?: () => void }) => (
    <output className="media-failure">
        <span>{message}</span>
        {onRetry && (
            <Button className="button button-outline" onPress={onRetry}>
                Try again
            </Button>
        )}
        <a className="text-link" href={assetUrl(asset)} rel="noreferrer" target="_blank">
            {Asset.match(asset, { image: () => 'Open original image', video: () => 'Open original video', pdf: () => 'Open original PDF' })}
        </a>
    </output>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { MediaFailure, MediaLoading };
