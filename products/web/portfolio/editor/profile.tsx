import { useAtomSet } from '@effect/atom-react';
import { Identity } from '@rasm/hosting/identity';
import { Field } from '@rasm/ui';
import { Optic } from 'effect';
import type { Atom } from 'effect/reactivity';
import type { Ref } from 'react';
import { PlacementFields } from '#/editor/placement-fields.tsx';
import { draft, entered, hero, portfolio } from '#/editor/state';
import { MediaFigure } from '#/media/media-figure.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Profile = ({ hero: heading, onChooseHero, ...state }: Pick<Atom.Success<typeof draft>, 'assets' | 'portfolio'> & { readonly hero: Ref<HTMLHeadingElement>; readonly onChooseHero: () => void }) => {
    const setDraft = useAtomSet(draft);
    const field = (name: 'name' | 'introduction' | 'email') => ({ name, value: state.portfolio[name] ?? '', onChange: (input: string) => setDraft(Optic.replace(portfolio.optionalKey(name), entered(input))) });
    return (
        <>
            <Field {...field('name')} label="Display name" placeholder="Name or studio" />
            <Field {...field('introduction')} label="Introduction" multiline placeholder="Optional" />
            <Field {...field('email')} label="Public contact email" placeholder="Optional" type="email" />
            <h3 ref={heading} tabIndex={-1}>
                Hero
            </h3>
            {state.portfolio.hero === undefined ? (
                <div className="actions">
                    <p className="hint">Choose library file or upload new one</p>
                    <button className="button button-outline" onClick={onChooseHero} type="button">
                        Choose hero file
                    </button>
                </div>
            ) : (
                <>
                    <div className="profile-hero">
                        <MediaFigure active placement={state.portfolio.hero} presentation="full" priority={false} renderMedia />
                    </div>
                    <PlacementFields assets={state.assets} onChange={(placement) => setDraft(Optic.replace(hero, placement))} value={state.portfolio.hero}>
                        <button className="button button-outline" onClick={() => setDraft(Optic.replace(hero, undefined))} type="button">
                            Clear hero
                        </button>
                    </PlacementFields>
                </>
            )}
            <a className="profile-signout" href={Identity.href('signout', '/')} target="_top">
                Sign out
            </a>
        </>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Profile };
