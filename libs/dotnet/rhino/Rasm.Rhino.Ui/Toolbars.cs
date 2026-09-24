using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Answers))]

namespace Rasm.Rhino.Ui;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ToolbarOp {
    public sealed record Close(ToolbarFile File, bool Prompt) : ToolbarOp;

    public sealed record Save(ToolbarFile File) : ToolbarOp;

    public sealed record SaveAs(ToolbarFile File, string Path) : ToolbarOp;

    public sealed record GroupVisible(ToolbarFile File, Guid Group, bool Visible) : ToolbarOp;

    public sealed record SidebarIsVisible(bool Visible) : ToolbarOp;

    public sealed record MruSidebarIsVisible(bool Visible) : ToolbarOp;

    public sealed record BitmapSize(Size Size) : ToolbarOp;

    public sealed record TabSize(Size Size) : ToolbarOp;
}

public sealed record ToolbarGroupState(Guid Id, Option<string> Name, bool Visible, bool Docked);

public sealed record ToolbarFileState(Guid Id, string Name, string Path, Seq<ToolbarGroupState> Groups, Seq<(Guid Id, string Name)> Toolbars);

public sealed record ToolbarFiles(Seq<ToolbarFileState> Files, bool SidebarIsVisible, bool MruSidebarIsVisible, Size BitmapSize, Size TabSize);

public sealed record MenuItemState(Option<bool> Enabled, Option<bool> Checked, Option<bool> RadioChecked, Option<LocalizeStringPair> Text);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Toolbars {
    // --- [FILES]
    public static IO<ToolbarFile> OpenFile(string path) =>
        IO.lift(() => Missing.Unless(RhinoApp.ToolbarFiles.Open(path), nameof(ToolbarFileCollection.Open)));

    public static IO<ToolbarFile> FindByPath(string path) =>
        IO.lift(() => Missing.Unless(RhinoApp.ToolbarFiles.FindByPath(path), nameof(ToolbarFileCollection.FindByPath)));

    public static IO<ToolbarFile> FindByName(string name, bool ignoreCase) =>
        IO.lift(() => Missing.Unless(RhinoApp.ToolbarFiles.FindByName(name, ignoreCase), nameof(ToolbarFileCollection.FindByName)));

    public static IO<ToolbarFile> Resolve(ComponentRef reference) =>
        TableOps.Find(
                reference,
                static id => Files().Find(file => file.Id == id),
                static index => IndexOutOfRange.Unless(index, RhinoApp.ToolbarFiles.Count, nameof(ToolbarFileCollection)).Map(_ => Optional(RhinoApp.ToolbarFiles[index])),
                static name => Optional(RhinoApp.ToolbarFiles.FindByName(name, ignoreCase: false)))
            .Bind(static found => IO.lift(found.ToFin(new Missing(nameof(ToolbarFileCollection)))));

    public static IO<Unit> Apply(ToolbarOp op) =>
        op.Switch(
            close: static close => IO.lift(() => Refused.Unless(close.File.Close(close.Prompt), nameof(ToolbarFile.Close))),
            save: static save => IO.lift(() => Refused.Unless(save.File.Save(), nameof(ToolbarFile.Save))),
            saveAs: static saveAs => IO.lift(() => Refused.Unless(saveAs.File.SaveAs(saveAs.Path), nameof(ToolbarFile.SaveAs))),
            groupVisible: static visible =>
                from groups in IO.lift(() => Groups(visible.File))
                from found in IO.lift(() => groups.Find(candidate => candidate.Id == visible.Group).ToFin(new Missing(nameof(ToolbarFile.GetGroup))))
                from shown in IO.lift(() => { found.Visible = visible.Visible; })
                select shown,
            sidebarIsVisible: static sidebar => IO.lift(() => { ToolbarFileCollection.SidebarIsVisible = sidebar.Visible; }),
            mruSidebarIsVisible: static mru => IO.lift(() => { ToolbarFileCollection.MruSidebarIsVisible = mru.Visible; }),
            bitmapSize: static size => IO.lift(() => { Toolbar.BitmapSize = size.Size; }),
            tabSize: static size => IO.lift(() => { Toolbar.TabSize = size.Size; }));

    public static IO<ToolbarFiles> Read() =>
        IO.lift(static () => Files()
            .TraverseM(Snapshot)
            .As()
            .Map(static files => new ToolbarFiles(files, ToolbarFileCollection.SidebarIsVisible, ToolbarFileCollection.MruSidebarIsVisible, Toolbar.BitmapSize, Toolbar.TabSize)));

    private static Seq<ToolbarFile> Files() =>
        Answers.Present(RhinoApp.ToolbarFiles);

    private static Fin<ToolbarFileState> Snapshot(ToolbarFile file) =>
        from groups in Groups(file)
        from toolbars in toSeq(Range(0, file.ToolbarCount)).TraverseM(index => Missing.Unless(file.GetToolbar(index), nameof(ToolbarFile.GetToolbar))).As()
        select new ToolbarFileState(
            file.Id,
            file.Name,
            file.Path,
            groups.Map(static row => ToolbarMapper.ToState(row)).Strict(),
            toolbars.Map(static toolbar => (toolbar.Id, toolbar.Name)).Strict());

    private static Fin<Seq<ToolbarGroup>> Groups(ToolbarFile file) =>
        toSeq(Range(0, file.GroupCount)).TraverseM(index => Missing.Unless(file.GetGroup(index), nameof(ToolbarFile.GetGroup))).As();

    // --- [MENUS]
    public static IO<Unit> RegisterMenuItem(Guid file, Guid menu, Guid item, Func<MenuItemState> sync) =>
        IO.lift(() => Refused.Unless(RuiUpdateUi.RegisterMenuItem(file, menu, item, (_, ui) => Written(ui, sync())), nameof(RuiUpdateUi.RegisterMenuItem)));

    private static void Written(RuiUpdateUi ui, MenuItemState state) {
        _ = state.Enabled.Iter(enabled => ui.Enabled = enabled);
        _ = state.Checked.Iter(check => ui.Checked = check);
        _ = state.RadioChecked.Iter(radio => ui.RadioChecked = radio);
        _ = state.Text.Iter(text => ui.Text = text.Local);
    }
}

[Mapper]
internal static partial class ToolbarMapper {
    [MapProperty(nameof(ToolbarGroup.IsDocked), nameof(ToolbarGroupState.Docked))]
    internal static partial ToolbarGroupState ToState(ToolbarGroup group);
}
