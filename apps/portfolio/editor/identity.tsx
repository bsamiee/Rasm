import type { ReactNode } from 'react';
import { MediaFigure } from '../media/figure.tsx';
import type { Portfolio } from '../model/document.ts';
import type { Placement } from '../model/placement.ts';
import { Field } from './controls.tsx';
import { emailId } from './focus.ts';
import { PlacementFields } from './placement.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

function Identity({ portfolio, onChange, onHero, onChooseHero }: { portfolio: typeof Portfolio.Type; onChange: (portfolio: typeof Portfolio.Type) => void; onHero: (hero: typeof Placement.Type | undefined) => void; onChooseHero: () => void }): ReactNode {
    return (
        <>
            <Field label="Display name" onChange={(name): void => onChange({ ...portfolio, name })} placeholder="Your name or studio" value={portfolio.name} />
            <Field label="Introduction" multiline={true} onChange={(introduction): void => onChange({ ...portfolio, introduction })} placeholder="Optional short introduction" value={portfolio.introduction} />
            <Field description="Enter a complete email address, or leave this blank." id={emailId} label="Public contact email" onChange={(email): void => onChange({ ...portfolio, email })} placeholder="Optional" type="email" value={portfolio.email} />
            <h3 className="mt-2 text-[26px] tracking-[-0.035em]">Hero placement</h3>
            {portfolio.hero ? (
                <>
                    <div className="flex justify-center [--media-height:320px]">
                        <MediaFigure active={true} placement={portfolio.hero} presentation="full" priority={false} />
                    </div>
                    <PlacementFields onChange={onHero} value={portfolio.hero} />
                    <button className="button button-outline" onClick={(): void => onHero(undefined)} type="button">
                        Clear hero
                    </button>
                </>
            ) : (
                <div className="actions">
                    <p className="note">Choose an existing file or add a new hero. No extra copy is created.</p>
                    <button className="button button-outline" onClick={onChooseHero} type="button">
                        Choose hero file
                    </button>
                </div>
            )}
            <a className="mt-[35px] text-sm underline" href="/signout-with-chatgpt?return_to=%2F" target="_top">
                Sign out
            </a>
        </>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Identity };
