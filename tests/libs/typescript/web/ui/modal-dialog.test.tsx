import { RegistryContext } from '@effect/atom-react';
import { ModalDialog } from '@rasm/ui';
import { AtomRegistry } from 'effect/reactivity';
import { type ReactNode, StrictMode } from 'react';
import { Button, DialogTrigger } from 'react-aria-components';
import { expect, it, vi } from 'vitest';
import { render } from 'vitest-browser-react';
import { userEvent } from 'vitest/browser';

// --- [OPERATIONS] ----------------------------------------------------------------------

const registered = async (tree: ReactNode) => {
    const registry = AtomRegistry.make();
    const scoped = (node: ReactNode) => <RegistryContext.Provider value={registry}>{node}</RegistryContext.Provider>;
    const screen = await render(scoped(tree));
    return { screen, rerender: (node: ReactNode) => screen.rerender(scoped(node)), open: () => registry.get(ModalDialog.open) };
};
const controlled = (label: string, isOpen: boolean) => <ModalDialog dialog={{ 'aria-label': label }} isOpen={isOpen} />;
const dialogs = (first: boolean) => (
    <>
        {controlled('First', first)}
        {controlled('Second', true)}
    </>
);
const triggered = (label: string, content?: ReactNode) => (
    <DialogTrigger>
        <Button>{`Open ${label}`}</Button>
        <ModalDialog dialog={{ 'aria-label': label }} isDismissable>
            {content}
        </ModalDialog>
    </DialogTrigger>
);

// --- [COMPOSITION] ---------------------------------------------------------------------

it('counts each controlled dialog independently as its open state changes', async () => {
    const { rerender, open } = await registered(dialogs(true));
    await expect.poll(open).toBe(2);
    await rerender(dialogs(false));
    await expect.poll(open).toBe(1);
    await rerender(dialogs(true));
    await expect.poll(open).toBe(2);
});

it('counts trigger-opened dialog until Escape dismisses it', async () => {
    const { screen, open } = await registered(triggered('Editor'));
    expect(open()).toBe(0);
    await screen.getByRole('button', { name: 'Open Editor' }).click();
    await expect.element(screen.getByRole('dialog', { name: 'Editor' })).toBeInTheDocument();
    await expect.poll(open).toBe(1);
    await userEvent.keyboard('{Escape}');
    await expect.element(screen.getByRole('dialog', { name: 'Editor' })).not.toBeInTheDocument();
    await expect.poll(open).toBe(0);
});

it('counts nested dialog opened from inside another and releases it on dismissal', async () => {
    const { screen, open } = await registered(triggered('Editor', triggered('Confirm')));
    await screen.getByRole('button', { name: 'Open Editor' }).click();
    await screen.getByRole('button', { name: 'Open Confirm' }).click();
    await expect.element(screen.getByRole('dialog', { name: 'Confirm' })).toBeInTheDocument();
    await expect.poll(open).toBe(2);
    await userEvent.keyboard('{Escape}');
    await expect.element(screen.getByRole('dialog', { name: 'Confirm' })).not.toBeInTheDocument();
    await expect.poll(open).toBe(1);
});

it('counts once under strict mode effect replay and releases every count on unmount', async () => {
    const { screen, open } = await registered(<StrictMode>{controlled('Strict', true)}</StrictMode>);
    await expect.poll(open).toBe(1);
    await screen.unmount();
    expect(open()).toBe(0);
});

it('confirms through an alert dialog titled by its label, focusing cancel first and acting once on confirmation alone', async () => {
    const onConfirm = vi.fn<() => void>();
    const { screen } = await registered(<ModalDialog.Confirm cancel="Keep" description="Deletion is permanent" label="Delete item" onConfirm={onConfirm} />);
    const trigger = screen.getByRole('button', { name: 'Delete item' });
    await trigger.click();
    const alert = screen.getByRole('alertdialog', { name: 'Delete item' });
    await expect.element(alert).toHaveAccessibleDescription('Deletion is permanent');
    await expect.element(alert.getByRole('button', { name: 'Keep' })).toHaveFocus();
    await alert.getByRole('button', { name: 'Keep' }).click();
    await expect.element(alert).not.toBeInTheDocument();
    await trigger.click();
    await screen.getByRole('alertdialog').getByRole('button', { name: 'Delete item' }).click();
    await expect.element(screen.getByRole('alertdialog')).not.toBeInTheDocument();
    expect(onConfirm).toHaveBeenCalledOnce();
});
