export const meta = {
  name: 'reduce',
  description: 'Reduce one folder in any language with no capability growth: the deepen workflow with args.mode reduce',
  whenToUse: 'args.language typescript, python, rust, swift or dotnet; args.path the folder from the repository root, or args.key under libs/<language>/; args.base the commit it starts from.',
}

// --- [COMPOSITION] ---------------------------------------------------------------------

return await workflow('deepen', { ...args, mode: 'reduce' })
