import { sessionPath } from '@rasm/hosting/sites.ts';
import { MediaFigure } from '../media/figure.tsx';
import type { AssetCollection } from '../model/asset.ts';
import type { Portfolio } from '../model/document.ts';
import type { Placement } from '../model/placement.ts';
import { Field } from './controls.tsx';
import { PlacementFields } from './placement.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Identity = ({
    assets,
    portfolio,
    emailId,
    onChange,
    onHero,
    onChooseHero,
}: {
    assets: typeof AssetCollection.Type;
    portfolio: typeof Portfolio.Type;
    emailId: string;
    onChange: (update: (portfolio: typeof Portfolio.Type) => typeof Portfolio.Type) => void;
    onHero: (hero: typeof Placement.Type | undefined) => void;
    onChooseHero: () => void;
}) => (
    <>
        <Field label="Display name" onChange={(name) => onChange((current) => ({ ...current, name }))} placeholder="Your name or studio" value={portfolio.name} />
        <Field label="Introduction" multiline={true} onChange={(introduction) => onChange((current) => ({ ...current, introduction }))} placeholder="Optional short introduction" value={portfolio.introduction} />
        <Field description="Enter a complete email address, or leave this blank." id={emailId} label="Public contact email" onChange={(email) => onChange((current) => ({ ...current, email }))} placeholder="Optional" type="email" value={portfolio.email} />
        <h3 className="mt-2 text-heading tracking-display">Hero placement</h3>
        {portfolio.hero === undefined ? (
            <div className="actions">
                <p className="hint">Choose an existing file or add a new hero. No extra copy is created.</p>
                <button className="button button-outline" onClick={onChooseHero} type="button">
                    Choose hero file
                </button>
            </div>
        ) : (
            <>
                <div className="media-h-80 flex justify-center">
                    <MediaFigure active={true} placement={portfolio.hero} presentation="full" priority={false} renderMedia={true} />
                </div>
                <PlacementFields assets={assets} onChange={onHero} value={portfolio.hero} />
                <button className="button button-outline" onClick={() => onHero(undefined)} type="button">
                    Clear hero
                </button>
            </>
        )}
        <a className="mt-8.75 text-sm underline" href={sessionPath('signout', '/')} target="_top">
            Sign out
        </a>
    </>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Identity };
