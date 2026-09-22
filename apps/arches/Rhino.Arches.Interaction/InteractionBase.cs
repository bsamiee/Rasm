using Arches;
using Rhino.Arches.Core;
using Rhino.Arches.Interaction.Constants;
using Rhino.Arches.Interaction.Faactory;
using Rhino.Arches.Interaction.Types;
using Rhino.Geometry;
using Rhino.Input.Custom;


namespace Rhino.Arches.Interaction
{

    public record DrawBySpan(Point3d Start, Point3d End, Vector3d Normal);
    public record DrawBySpan_FoilCount(Point3d Start, Point3d End, Vector3d Normal,bool IsPointed, int FoilCount);

    public record DrawStaticFoilArchBySpan(Point3d Start, Point3d End, Vector3d Normal, bool IsPointed);

    public record DrawBySpanApex(Point3d Start, Point3d End, Point3d Apex, Vector3d Normal);

    public abstract class InteractionBase
    {
        protected Point3d? _startPoint = null;

        public ArchBase StartCommand(string startMessage,
            Action<GetPoint> AddOptions, List<Func<ArchBase>> functions)
        {
            var arch = ClickFactory.RunCommand(() =>
            {

                var result = ClickFactory.GetPoint(
                startMessage, AddOptions, null);

                if (result is null)
                    return null;

                _startPoint = result.Point;

                return result;
            }, functions);
            return arch;
        }

        public RhinoArchBase DrawByStartAndEnd(Func<DrawBySpan, RhinoArchBase> buildArch)
        {
            try
            {
                var arch = ArchInteractionTypes.Get_StartEndArch(_startPoint, (twoPointResult) =>
                {

                    int multiplier = twoPointResult.IsFlipClicked ? -1 : 1;
                    var normal = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.GetConstructionPlane().Plane.Normal * multiplier;

                    return buildArch(new DrawBySpan(twoPointResult.StartPt, twoPointResult.EndPt, normal));

                });

                return arch;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public RhinoArchBase DrawByStartEndApex(Func<DrawBySpanApex, RhinoArchBase> buildArch)
        {
            try
            {
               // var normal = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.GetConstructionPlane().Plane.Normal;
                var arch = ArchInteractionTypes.Get_StartEndApexArch(startPt: _startPoint,
                buildArch: (result) =>
                {
                    var normal = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.GetConstructionPlane().Plane.Normal;
                    return buildArch(new DrawBySpanApex(result.StartPt, result.EndPt, result.ApexPoint, normal));

                });

                return arch;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public RhinoArchBase DrawStaticFoilByStartAndEnd(Func<DrawStaticFoilArchBySpan, RhinoArchBase> buildArch)
        {
            try
            {
                var arch = ArchInteractionTypes.Get_StartEndArch(_startPoint, (twoPointResult, isPointed) =>
                {

                    int multiplier = twoPointResult.IsFlipClicked ? -1 : 1;
                    var normal = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.GetConstructionPlane().Plane.Normal * multiplier;


                    return buildArch(new DrawStaticFoilArchBySpan(twoPointResult.StartPt, twoPointResult.EndPt, normal, isPointed));

                });

                return arch;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public RhinoArchBase DrawByStartEndFoilCount(Func<DrawBySpan_FoilCount, RhinoArchBase> buildArch)
        {
            try
            {
                int foilCount = 5;
                var arch = ArchInteractionTypes.Get_StartEndOptionArch(_startPoint,
                    promptMessage:Prompts.GetFoilCountPrompt,
                    Options.FoilCount,foilCount.ToString(),
                    (twoPointResult, optionResult,isPointed) =>
                {

                    if (optionResult is not null && optionResult.Number is not double.NaN)
                    {
                        foilCount = (int)optionResult.Number;
                    }

                    int multiplier = twoPointResult.IsFlipClicked ? -1 : 1;
                    var normal = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.GetConstructionPlane().Plane.Normal * multiplier;

                    //var isPointed = twoPointResult.SelectedOptionName == Options.Pointed;

                    var a = buildArch.Invoke(new DrawBySpan_FoilCount(twoPointResult.StartPt,
                        twoPointResult.EndPt, normal,isPointed, foilCount));

                    
                    return a;

                });

                return arch;
            }
            catch (Exception ex)
            {

                return null;
            }
        }
    }
}
