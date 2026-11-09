import { Choice, Field, MenuButton } from '@rasm/ui';
import { Match, Option, Record, String, Struct } from 'effect';
import type { ReactNode } from 'react';
import { choiceLayout, descriptionLabel, joinDetails, sheetLabel } from '#/media/labels';
import { Asset, type Assets } from '#/model/asset';
import { Placement } from '#/model/placement';

// --- [CONSTANTS] -----------------------------------------------------------------------

const imageRoles = Record.collect({ drawing: 'Drawing (full sheet)', photograph: 'Photograph' } satisfies Record.ReadonlyRecord<Extract<typeof Placement.Type, { readonly kind: 'image' }>['role'], string>, (id, label) => ({ id, label }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const present = <K extends 'caption' | 'alt' | 'description'>(key: K, input: string) => Option.getOrUndefined(Option.map(Option.liftPredicate(input, String.isNonEmpty), (text) => Record.singleton(key, text)));

// --- [COMPOSITION] ---------------------------------------------------------------------

const PlacementFields = ({ value, assets, onChange, children }: { readonly value: typeof Placement.Type; readonly assets: typeof Assets.Service; readonly onChange: (value: typeof Placement.Type) => void; readonly children?: ReactNode }) => (
    <div className="border-line flex flex-col gap-3.25 border-t pt-4.5">
        <span className="text-caption wrap-anywhere">{value.asset.name}</span>
        <Field
            label="Caption"
            onChange={(caption) => {
                const { caption: _, ...rest } = value;
                onChange({ ...rest, ...present('caption', caption) });
            }}
            value={value.caption ?? ''}
        />
        {Match.value(value).pipe(
            Match.discriminatorsExhaustive('kind')({
                pdf: (sheet) => (
                    <MenuButton aria-label="Sheet" onAction={({ page }) => onChange({ ...sheet, page })} options={sheet.asset.sheets.map((page) => ({ id: page.number.toString(), label: sheetLabel(page, sheet.asset.pages.length), page }))}>
                        {sheetLabel(sheet.page, sheet.asset.pages.length)}
                    </MenuButton>
                ),
                image: (image) => {
                    const shared = Struct.pick(image, ['kind', 'asset', 'caption', 'alt', 'description']);
                    return (
                        <>
                            <Choice {...choiceLayout} label="Image use" onChange={({ id }) => onChange(id === 'photograph' ? { ...shared, role: id, framing: { mode: 'contain' } } : { ...shared, role: id })} options={imageRoles} value={image.role} />
                            {Match.value(image).pipe(
                                Match.discriminatorsExhaustive('role')({
                                    drawing: () => undefined,
                                    photograph: ({ framing, ...photograph }) => {
                                        const framings = [
                                            { id: 'contain', label: 'Fit whole image', framing: { mode: 'contain' } },
                                            { id: 'cover', label: 'Crop to frame', framing: { mode: 'cover', x: 50, y: 50 } },
                                        ] as const;
                                        return (
                                            <>
                                                <Choice {...choiceLayout} label="Framing" onChange={(option) => onChange({ ...photograph, framing: option.framing })} options={framings} value={framing.mode} />
                                                {framing.mode === 'cover' && (
                                                    <div className="field-pair">
                                                        {Record.collect({ x: 'Horizontal', y: 'Vertical' } as const, (axis, direction) => (
                                                            <Field.Slider key={axis} label={`${direction} focal point`} maxValue={Placement.focus.maximum} minValue={Placement.focus.minimum} onChange={(position) => onChange({ ...photograph, framing: { ...framing, [axis]: position } })} value={framing[axis]} />
                                                        ))}
                                                    </div>
                                                )}
                                            </>
                                        );
                                    },
                                }),
                            )}
                        </>
                    );
                },
                video: (film) => (
                    <div className="flex flex-col gap-3">
                        <MenuButton
                            aria-label="Video poster"
                            onAction={({ asset }) => onChange({ ...film, poster: asset })}
                            options={Object.values(assets)
                                .filter(Asset.guards.image)
                                .map((asset) => ({ id: asset.id, label: asset.name, asset }))}
                        >
                            {film.poster === undefined ? 'Choose poster image' : joinDetails(['Poster', film.poster.name])}
                        </MenuButton>
                        {film.poster && (
                            <button className="button button-ghost self-start" onClick={() => onChange(Struct.omit(film, ['poster']))} type="button">
                                Clear poster
                            </button>
                        )}
                        <p className="hint">Poster shows before playback. Videos with speech or meaningful audio need burned-in captions. Transcripts do not replace captions</p>
                    </div>
                ),
            }),
        )}
        <Field
            label="Accessible description"
            onChange={(alt) => {
                const { alt: _, ...rest } = value;
                onChange({ ...rest, ...present('alt', alt) });
            }}
            placeholder="What image or sheet conveys"
            value={value.alt ?? ''}
        />
        <Field
            label={descriptionLabel(value)}
            multiline={true}
            onChange={(description) => {
                const { description: _, ...rest } = value;
                onChange({ ...rest, ...present('description', description) });
            }}
            value={value.description ?? ''}
        />
        {children}
    </div>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { PlacementFields };
