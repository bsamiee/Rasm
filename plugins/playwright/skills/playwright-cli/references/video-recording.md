# [VIDEO_RECORDING]

Recordings capture the page between `video-start` and `video-stop` as a VP9 WebM file.

## [01]-[RECORDING]

`video-start` records every open page to a file path, without one to `$PLAYWRIGHT_MCP_OUTPUT_DIR/video-<timestamp>.webm`, `video-stop` prints one file per page, a second page as `<name>-1.webm`:
- `--size=<width>x<height>` sets the frame size, the page scales to fit and gray fills the rest, without it the frame fits 800x800 at the viewport's aspect
- `--fps=<rate>` sets the frame rate, 25 without it
- `--cursor` draws a cursor that moves to each action point and paces actions by 800ms
- `video-chapter <title>` blocks while the current tab shows a chapter card with `--description` for `--duration` milliseconds, 2000 without it

```bash
playwright cli open
playwright cli video-start $PLAYWRIGHT_MCP_OUTPUT_DIR/demo.webm --size=1280x800 --fps=60 --cursor
playwright cli video-chapter "Getting Started" --description="Opening the homepage" --duration=2000
playwright cli goto https://example.com/login
playwright cli snapshot
playwright cli click e1
playwright cli video-chapter "Filling Form" --description="Entering test data" --duration=2000
playwright cli fill e2 "test input"
playwright cli video-stop
playwright cli close
```

## [02]-[ACTIONS]

`video-show-actions` decorates each later action with a title naming the action and its locator, a cursor, and when styled a point marker and a target highlight, until `video-hide-actions`:
- `--duration=<ms>` paces actions and fades each decoration over that time, 500 without it
- `--position` places the title at `top-left`, `top`, `top-right`, `bottom-left`, `bottom`, or `bottom-right`, `top-right` without it
- `--point-style` sizes and colors the zero-sized marker centered on the action point
- `--highlight-style` styles the box over the target element, `outline` keeps the box at the element's bounds
- `--title-style` styles the title, `display: none` keeps the cursor without a title

```bash
playwright cli open https://example.com
playwright cli video-start $PLAYWRIGHT_MCP_OUTPUT_DIR/demo.webm
playwright cli video-show-actions --duration=800 --position=top-right \
    --point-style="width: 20px; height: 20px; border-radius: 50%; background: rgba(255,0,0,.7)" \
    --highlight-style="outline: 2px solid #333; background: rgba(0,128,255,.15)" \
    --title-style="font-size: 16px"
playwright cli click e1
playwright cli video-hide-actions
playwright cli video-stop
playwright cli close
```

## [03]-[SCRIPTS]

Scripted recordings through `run-code --filename` pace typing with `pressSequentially`, wait between steps, and annotate the page through `page.screencast`:
- `start({ path, size, fps })` begins the recording, `stop()` writes the file
- `showActions({ cursor, duration, position, style: { point, highlight, title } })` takes the `video-show-actions` options, `cursor: 'none'` drops the cursor, `hideActions()` ends them
- `showChapter(title, { description, duration })` blocks while a chapter card shows for its duration, 2000ms without one
- `showOverlay(html, { duration })` draws HTML over the page for its duration, without one until `dispose()` on the returned value
- `hideOverlays()` and `showOverlays()` hide and restore every overlay
- Overlays take no pointer events, actions run through them

```bash
playwright cli open
playwright cli run-code --filename=<script>.js
playwright cli close
```

```js
async page => {
    await page.screencast.start({ path: '<dir>/demo.webm', size: { width: 1280, height: 800 }, fps: 60 });
    await page.screencast.showActions({
        duration: 800,
        style: {
            point: 'width: 20px; height: 20px; border-radius: 50%; background: rgba(255, 0, 0, .7)',
            title: 'display: none',
        },
    });
    await page.goto('https://demo.playwright.dev/todomvc');

    await page.screencast.showChapter('Adding Todo Items', {
        description: 'We will add several items to the todo list.',
        duration: 2000,
    });
    await page.getByRole('textbox', { name: 'What needs to be done?' }).pressSequentially('Walk the dog', { delay: 60 });
    await page.getByRole('textbox', { name: 'What needs to be done?' }).press('Enter');
    await page.waitForTimeout(1000);

    const annotation = await page.screencast.showOverlay(`
        <div style="position: absolute; top: 8px; right: 8px;
            padding: 6px 12px; background: rgba(0,0,0,0.7);
            border-radius: 8px; font-size: 13px; color: white;">
            Item added
        </div>
    `);
    await page.getByRole('textbox', { name: 'What needs to be done?' }).pressSequentially('Buy groceries', { delay: 60 });
    await page.getByRole('textbox', { name: 'What needs to be done?' }).press('Enter');
    await page.waitForTimeout(1500);
    await annotation.dispose();

    const bounds = await page.getByText('Walk the dog').boundingBox();
    await page.screencast.showOverlay(`
        <div style="position: absolute;
            top: ${bounds.y}px; left: ${bounds.x}px;
            width: ${bounds.width}px; height: ${bounds.height}px;
            outline: 1px solid red;">
        </div>
        <div style="position: absolute;
            top: ${bounds.y + bounds.height + 5}px;
            left: ${bounds.x + bounds.width / 2}px;
            transform: translateX(-50%);
            padding: 6px; background: #808080;
            border-radius: 10px; font-size: 14px; color: white;">The first item</div>
    `, { duration: 2000 });

    await page.screencast.hideActions();
    await page.screencast.stop();
}
```

Use pr-attachments.md for pull request and issue attachments.
