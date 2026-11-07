// --- [CONSTANTS] -----------------------------------------------------------------------

globalThis.JOB_TYPE = {
    description: 'Render frame chunks with task dependencies',
    label: 'Rasm Render',
    settings: [{ description: 'Installed extension command for frame rendering', key: 'render_command', required: true, type: 'string', visible: 'web' }],
};

// --- [COMPOSITION] ---------------------------------------------------------------------

globalThis.compileJob = (job) => {
    const tasks = job.settings.jobs.map((entry, part) =>
        entry.requests.map((request, chunk) => {
            const task = author.Task(`render-${part}-${chunk}`, 'blender');
            task.addCommand(
                author.Command(
                    'blender-render',
                    {
                        args: ['--command', job.settings.render_command, request.argument, job.settings[entry.output_root]],
                        argsBefore: ['--background'],
                        blendfile: entry.blendfile,
                        exe: '{blender}',
                        exeArgs: '{blenderArgs}',
                    },
                    request.steps,
                ),
            );
            return task;
        }),
    );
    job.settings.jobs.forEach((entry, part) => {
        const prerequisites = entry.prerequisites.flatMap((prerequisite) => tasks[prerequisite]);
        tasks[part].forEach((task) => {
            prerequisites.forEach(task.addDependency);
            job.addTask(task);
        });
    });
};
