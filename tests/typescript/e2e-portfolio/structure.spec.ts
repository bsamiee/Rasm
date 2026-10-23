import { expect, test } from '#/fixtures';

// --- [COMPOSITION] ---------------------------------------------------------------------

test('editor dialog holds actions, status, and tabs', async ({ owner }) => {
    await expect(owner.getByRole('dialog', { name: 'Portfolio editor.' })).toMatchAriaSnapshot(`
      - dialog "Portfolio editor.":
        - heading "Portfolio editor." [level=2]
        - button "Close portfolio editor"
        - button "Save draft"
        - button "Preview draft"
        - button "Publish"
        - status: Private draft
        - group:
          - tablist "Portfolio editing":
            - tab "Projects and studies" [selected]
            - tab "Files"
            - tab "Identity and hero"
          - tabpanel "Projects and studies":
            - button "New project"
            - button "New study"
            - grid "Projects and studies"
    `);
});
test('entry removal asks for confirmation, Keep returns focus to entry', async ({ owner }, { testId }) => {
    const editor = owner.getByRole('dialog', { name: 'Portfolio editor.' });
    const title = `Harbor Library ${testId}`;
    const entry = editor.getByRole('grid', { name: 'Projects and studies' }).getByRole('row', { name: title });
    await editor.getByRole('button', { name: 'New project' }).click();
    await editor.getByRole('textbox', { name: 'Title' }).fill(title);
    await editor.getByRole('button', { name: 'Back to entries' }).click();
    await entry.getByRole('button', { name: 'Remove entry' }).click();
    await expect(owner.getByRole('alertdialog', { name: 'Remove entry?' })).toMatchAriaSnapshot(`
      - alertdialog "Remove entry?":
        - heading "Remove entry?" [level=2]
        - paragraph: Source files stay in library. Published work changes on next publish
        - button "Keep"
        - button "Remove entry"
    `);
    await expect(owner.getByRole('alertdialog').getByRole('button', { name: 'Keep' })).toBeFocused();
    await owner.keyboard.press('Enter');
    await expect(owner.getByRole('alertdialog')).toBeHidden();
    await expect(entry).toBeFocused();
});
