// --- [CONSTANTS] -----------------------------------------------------------------------

globalThis.JOB_TYPE = {
    label: 'Rhino Render Queue',
    settings: [{ key: 'copy', required: true, subtype: 'file_path', type: 'string' }],
};

// --- [COMPOSITION] ---------------------------------------------------------------------

globalThis.compileJob = (job) => {
    const task = author.Task(job.name, 'rhino');
    task.addCommand(
        author.Command('exec', {
            args: [job.settings.copy, ...job.settings.arguments],
            exe: '{rhino}',
        }),
    );
    job.addTask(task);
};
