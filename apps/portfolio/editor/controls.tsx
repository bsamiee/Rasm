import { Array, Match, Option, SchemaIssue } from 'effect';
import { constVoid } from 'effect/Function';
import type { Atom } from 'effect/reactivity';
import { ChevronDown } from 'lucide-react';
import type { ReactNode } from 'react';
import { Button, Dialog, DialogTrigger, Heading, Input, Label, ListBox, ListBoxItem, Menu, MenuItem, MenuTrigger, Modal, ModalOverlay, Popover, Select, SelectValue, Slider, SliderOutput, SliderThumb, SliderTrack, Text, TextArea, TextField, type TextFieldProps } from 'react-aria-components';
import type { deleteRequest, draft, saveRequest } from './services.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type RequestFailure = Atom.Failure<typeof saveRequest>['error'] | Atom.Failure<typeof draft> | Atom.Failure<typeof deleteRequest>;
type FieldProps = Omit<TextFieldProps, 'children' | 'className'> & { readonly label: string; readonly placeholder?: string; readonly description?: string; readonly multiline?: boolean };
interface Choice {
    readonly id: string;
    readonly label: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const popover = 'max-h-[50vh] max-w-[min(480px,calc(100vw-32px))] overflow-auto border border-control-line bg-background';
const option = 'min-h-11 cursor-pointer px-4 py-2.5 outline-none focused:bg-foreground focused:text-background';

// --- [COMPOSITION] ---------------------------------------------------------------------

function Field({ label, placeholder, description, multiline, ...props }: FieldProps): ReactNode {
    return (
        <TextField {...props} className="field">
            <Label>{label}</Label>
            {multiline ? <TextArea className="control min-h-[90px] resize-y" placeholder={placeholder} /> : <Input className="control" {...(placeholder === undefined ? {} : { placeholder })} />}
            {description && (
                <Text className="note mt-[13px]" slot="description">
                    {description}
                </Text>
            )}
        </TextField>
    );
}
function SelectField<T extends Choice>({ label, value, options, onChange }: { label: string; value: T['id']; options: readonly T[]; onChange: (choice: T) => void }): ReactNode {
    return (
        <Select
            className="field"
            onChange={(key): void =>
                Option.match(
                    Array.findFirst(options, (item) => item.id === key),
                    { onNone: constVoid, onSome: onChange },
                )
            }
            value={value}
        >
            <Label>{label}</Label>
            <Button className="control flex items-center justify-between gap-2 text-left">
                <SelectValue className="min-w-0 truncate placeholder-shown:text-muted" />
                <ChevronDown className="size-4 shrink-0" />
            </Button>
            <Popover className={popover}>
                <ListBox className="outline-none" items={options}>
                    {(item): ReactNode => (
                        <ListBoxItem className={option} id={item.id} textValue={item.label}>
                            {item.label}
                        </ListBoxItem>
                    )}
                </ListBox>
            </Popover>
        </Select>
    );
}
function SliderField({ label, value, onChange }: { label: string; value: number; onChange: (value: number) => void }): ReactNode {
    return (
        <Slider className="field" maxValue={100} minValue={0} onChange={onChange} value={value}>
            <div className="flex justify-between">
                <Label>{label}</Label>
                <SliderOutput />
            </div>
            <SliderTrack className="relative h-11 w-full before:absolute before:top-1/2 before:h-0.5 before:w-full before:-translate-y-1/2 before:bg-control-line before:content-['']">
                <SliderThumb className="top-1/2 size-4 rounded-full bg-accent-text dragging:bg-accent" />
            </SliderTrack>
        </Slider>
    );
}
function MenuButton<T extends Choice>({ label, options, onAction, children }: { label: string; options: readonly T[]; onAction: (choice: T) => void; children: ReactNode }): ReactNode {
    return (
        <MenuTrigger>
            <Button className="button button-outline">{children}</Button>
            <Popover className={popover}>
                <Menu aria-label={label} className="outline-none" items={options}>
                    {(item): ReactNode => (
                        <MenuItem className={option} id={item.id} onAction={(): void => onAction(item)} textValue={item.label}>
                            {item.label}
                        </MenuItem>
                    )}
                </Menu>
            </Popover>
        </MenuTrigger>
    );
}
function ConfirmDialog({ label, description, onConfirm, disabled }: { label: string; description: string; onConfirm: () => void; disabled: boolean }): ReactNode {
    return (
        <DialogTrigger>
            <Button className="button button-ghost" isDisabled={disabled}>
                {label}
            </Button>
            <ModalOverlay className="fixed inset-0 z-[60] bg-foreground/55">
                <Modal>
                    <Dialog className="fixed top-1/2 left-1/2 z-[61] w-[min(480px,calc(100%-32px))] -translate-x-1/2 -translate-y-1/2 bg-background p-7 outline-none" role="alertdialog">
                        <Heading className="text-[26px] tracking-[-0.03em]" slot="title">
                            {label}?
                        </Heading>
                        <Text className="hint mt-3.5 mb-6" elementType="p" slot="description">
                            {description}
                        </Text>
                        <div className="flex justify-end gap-2.5">
                            <Button autoFocus={true} className="button button-outline" slot="close">
                                Keep
                            </Button>
                            <Button className="button" onPress={onConfirm} slot="close">
                                Remove
                            </Button>
                        </div>
                    </Dialog>
                </Modal>
            </ModalOverlay>
        </DialogTrigger>
    );
}
function RequestAlert({ error, onField }: { error: RequestFailure; onField?: (path: readonly PropertyKey[]) => void }): ReactNode {
    return (
        <div className="border-accent-text border-l-[3px] bg-surface p-4 text-sm" role="alert">
            {Match.value(error).pipe(
                Match.tag('SchemaError', ({ issue }) => (
                    <>
                        <p>Review these fields:</p>
                        <ul>
                            {SchemaIssue.makeFormatterStandardSchemaV1()(issue).issues.map((item) => {
                                const path = item.path?.map((part) => (typeof part === 'object' ? part.key : part)) ?? [];
                                const text = `${path.map(String).join(' › ')}: ${item.message}`;
                                return (
                                    <li key={text}>
                                        {onField ? (
                                            <button className="block text-left text-accent-text underline" onClick={(): void => onField(path)} type="button">
                                                {text}
                                            </button>
                                        ) : (
                                            item.message
                                        )}
                                    </li>
                                );
                            })}
                        </ul>
                    </>
                )),
                Match.tag('Forbidden', () => <p>Sign in with the owner account to edit.</p>),
                Match.tag('Conflict', () => <p>A file is still used by saved or published work. Remove its placements and save before trying again.</p>),
                Match.tag('ServiceUnavailable', () => <p>The service is unavailable. Your changes remain here.</p>),
                Match.orElse(() => <p>The request could not finish. Your changes remain here; try again.</p>),
            )}
        </div>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ConfirmDialog, Field, MenuButton, RequestAlert, SelectField, SliderField };
