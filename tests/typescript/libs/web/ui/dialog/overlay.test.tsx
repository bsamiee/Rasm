import { RegistryContext } from '@effect/atom-react';
import { ModalOverlay } from '@rasm/ui/dialog/overlay';
import { AtomRegistry } from 'effect/reactivity';
import { type ReactNode, StrictMode } from 'react';
import { Button, Dialog, DialogTrigger, Modal } from 'react-aria-components';
import { expect, it } from 'vitest';
import { userEvent } from 'vitest/browser';
import { render } from 'vitest-browser-react';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _registered = async (tree: ReactNode) => {
    const registry = AtomRegistry.make();
    const scoped = (node: ReactNode) => <RegistryContext.Provider value={registry}>{node}</RegistryContext.Provider>;
    const screen = await render(scoped(tree));
    return { screen, rerender: (node: ReactNode) => screen.rerender(scoped(node)), open: () => registry.get(ModalOverlay.open) };
};
const _controlled = (label: string, isOpen: boolean) => (
    <ModalOverlay isOpen={isOpen}>
        <Modal>
            <Dialog aria-label={label} />
        </Modal>
    </ModalOverlay>
);
const _triggered = (label: string, content?: ReactNode) => (
    <DialogTrigger>
        <Button>{`Open ${label}`}</Button>
        <ModalOverlay isDismissable={true}>
            <Modal>
                <Dialog aria-label={label}>{content}</Dialog>
            </Modal>
        </ModalOverlay>
    </DialogTrigger>
);

// --- [COMPOSITION] ---------------------------------------------------------------------

it('counts each controlled dialog independently as its open state changes', async () => {
    const dialogs = (first: boolean) => (
        <>
            {_controlled('First', first)}
            {_controlled('Second', true)}
        </>
    );
    const { rerender, open } = await _registered(dialogs(true));
    await expect.poll(open).toBe(2);
    await rerender(dialogs(false));
    await expect.poll(open).toBe(1);
    await rerender(dialogs(true));
    await expect.poll(open).toBe(2);
});
it('counts trigger-opened dialog until Escape dismisses it', async () => {
    const { screen, open } = await _registered(_triggered('Editor'));
    expect(open()).toBe(0);
    await screen.getByRole('button', { name: 'Open Editor' }).click();
    await expect.element(screen.getByRole('dialog', { name: 'Editor' })).toBeInTheDocument();
    await expect.poll(open).toBe(1);
    await userEvent.keyboard('{Escape}');
    await expect.element(screen.getByRole('dialog', { name: 'Editor' })).not.toBeInTheDocument();
    await expect.poll(open).toBe(0);
});
it('counts nested dialog opened from inside another and releases it on dismissal', async () => {
    const { screen, open } = await _registered(_triggered('Editor', _triggered('Confirm')));
    await screen.getByRole('button', { name: 'Open Editor' }).click();
    await screen.getByRole('button', { name: 'Open Confirm' }).click();
    await expect.element(screen.getByRole('dialog', { name: 'Confirm' })).toBeInTheDocument();
    await expect.poll(open).toBe(2);
    await userEvent.keyboard('{Escape}');
    await expect.element(screen.getByRole('dialog', { name: 'Confirm' })).not.toBeInTheDocument();
    await expect.poll(open).toBe(1);
});
it('counts once under strict mode effect replay and releases every count on unmount', async () => {
    const { screen, open } = await _registered(<StrictMode>{_controlled('Strict', true)}</StrictMode>);
    await expect.poll(open).toBe(1);
    await screen.unmount();
    expect(open()).toBe(0);
});
