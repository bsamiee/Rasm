import { ModalDialog } from '@rasm/ui/modal-dialog';
import { expect, it, vi } from 'vitest';
import { render } from 'vitest-browser-react';

// --- [COMPOSITION] ---------------------------------------------------------------------

it('confirms through an alert dialog titled by its label, focusing cancel first and acting once on confirmation alone', async () => {
    const onConfirm = vi.fn<() => void>();
    const screen = await render(<ModalDialog.Confirm cancel="Keep" description="Deletion is permanent" label="Delete item" onConfirm={onConfirm} />);
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

it('acts after its dialog closed, keeping focus where the confirmed action moves it', async () => {
    const screen = await render(
        <>
            <ModalDialog.Confirm cancel="Keep" description="Deletion is permanent" label="Delete item" onConfirm={() => screen.getByRole('button', { name: 'New item' }).element().focus()} />
            <button type="button">New item</button>
        </>,
    );
    await screen.getByRole('button', { name: 'Delete item' }).click();
    await screen.getByRole('alertdialog').getByRole('button', { name: 'Delete item' }).click();
    await expect.element(screen.getByRole('alertdialog')).not.toBeInTheDocument();
    await expect.element(screen.getByRole('button', { name: 'New item' })).toHaveFocus();
});
