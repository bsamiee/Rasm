
using Arches;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Rhino.Arches.Interaction.Faactory
{

    public record CustomOption(string PromptMessage,string Option,string OptionValue);
    public class ClickResult
    {
        public GetPoint Gp;
        public Point3d? Point = null;
        public int OptionIndex = -1;
        public double Number = double.NaN;
        public string OptionName = string.Empty;

        public ClickResult(GetPoint _gp)
        {
            Gp = _gp;
        }
    }

    public record DrawTwoPointResult(Point3d StartPt, Point3d EndPt, bool BothSides, bool IsFlipClicked,
        string SelectedOptionName);

    public record DrawThreePointResult(Point3d StartPt, Point3d EndPt, Point3d ApexPoint,
        bool BothSides, int OptionIndexSelected);

    public record ApexCallbackResult(double MinimumRisePossible, double MaximumRisePossible);
    public class ClickFactory
    {

        public static ClickResult GetPoint(string message,
            Action<GetPoint> addOptions = null,
                Action<DrawEventArgs, Point3d> onDynamicDraw = null)
        {
            var gp = new GetPoint();
            addOptions?.Invoke(gp);
            gp.SetCommandPrompt(message);

            gp.DynamicDraw += (sender, e) =>
            {
                // Then your own dynamic drawing overlay
                onDynamicDraw?.Invoke(e, e.CurrentPoint);
            };

            if (GetResult.Cancel == gp.Get())
                return null;

            return new ClickResult(gp)
            {
                Point = gp.Result() == Input.GetResult.Point ? gp.Point() : null,
                OptionIndex = gp.Result() == Input.GetResult.Option ? gp.Option().Index : -1,
                Number = gp.Result() == Input.GetResult.Number ? gp.Number() : double.NaN,
                OptionName = gp.Result() == Input.GetResult.Option ? gp.Option().EnglishName : null
            };
        }

        public static ArchBase RunCommand(Func<ClickResult> onCommandStart,
    List<Func<ArchBase>> archBuilders)
        {

            var commandStartResult = onCommandStart.Invoke();

            if (commandStartResult == null)
                return null;

            var archBuilder = archBuilders[0];
            if (commandStartResult.Point is null)
            {
                //check for options 
                if (commandStartResult.OptionIndex > 0)
                {
                    archBuilder = archBuilders[commandStartResult.OptionIndex - 1];
                    var arch = archBuilder.Invoke();
                    arch?.DrawProfile();
                    return arch;
                }
                return null;
            }
            else if (commandStartResult.Point is not null)
            {
                var arch = archBuilder.Invoke();
                arch?.DrawProfile();
                return arch;
            }

            return null;
        }
    }
}