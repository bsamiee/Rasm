import { useAtomSet } from '@effect/atom-react';
import { sessionPath } from '@rasm/hosting/authentication/sites.ts';
import { Field } from '@rasm/ui/field.tsx';
import { Optic, Struct } from 'effect';
import { AsyncResult } from 'effect/reactivity';
import { MediaFigure } from '../media/media-figure.tsx';
import type { AssetCollection } from '../model/asset.ts';
import type { Portfolio } from '../model/portfolio.ts';
import { PlacementFields } from './placement-fields.tsx';
import { draft, hero } from './state.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Identity = ({ assets, portfolio, emailId, onChooseHero }: { assets: typeof AssetCollection.Type; portfolio: typeof Portfolio.Type; emailId: string; onChooseHero: () => void }) => {
    const setDraft = useAtomSet(draft);
    const update = (fields: Partial<typeof Portfolio.Type>) => setDraft(AsyncResult.map(Struct.evolve({ portfolio: (current: typeof Portfolio.Type) => ({ ...current, ...fields }) })));
    return (
        <>
            <Field label="Display name" onChange={(name) => update({ name })} placeholder="Your name or studio" value={portfolio.name} />
            <Field label="Introduction" multiline={true} onChange={(introduction) => update({ introduction })} placeholder="Optional short introduction" value={portfolio.introduction} />
            <Field description="Enter a complete email address, or leave this blank." id={emailId} label="Public contact email" onChange={(email) => update({ email })} placeholder="Optional" type="email" value={portfolio.email} />
            <h3 className="text-heading tracking-display mt-2">Hero placement</h3>
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
                    <PlacementFields assets={assets} onChange={(placement) => setDraft(AsyncResult.map(Optic.replace(hero, placement)))} value={portfolio.hero} />
                    <button className="button button-outline" onClick={() => setDraft(AsyncResult.map(Optic.replace(hero, undefined)))} type="button">
                        Clear hero
                    </button>
                </>
            )}
            <a className="mt-8.75 text-sm underline" href={sessionPath('signout', '/')} target="_top">
                Sign out
            </a>
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Identity };
