using Rhino.Arches.Core;
using Rhino.Arches.Interaction.Constants;
using Rhino.Arches.Interaction.Faactory;
using Rhino.Arches.Interaction.Visualization;
using Rhino.Display;
using Rhino.Geometry;


namespace Rhino.Arches.Interaction.Types
{

    public static partial class ArchInteractionTypes
    {

        static ClickResult RunFoilCountLoop(DrawTwoPointResult tResult,
    string promptMessage, string option, string optionValue,
    Action<DrawEventArgs, DrawTwoPointResult, ClickResult> onOptionRun = null)
        {

            var cResult = ClickFactory.GetPoint(
                        promptMessage,
   (gp) =>
   {
       gp.AcceptNumber(true, true);
       gp.AddOption(option, optionValue);
       gp.SetBasePoint(tResult.StartPt, true);
   },
   (e, dynamicPt) =>
   {
       onOptionRun?.Invoke(e, tResult, null);
   });

            if (cResult is null)
                return null;

            while (cResult.Number is not double.NaN)
            {
                // ask again
                cResult = ClickFactory.GetPoint(
               promptMessage,
               (gp) =>
               {
                   //gp.AcceptNothing(true);
                   gp.AcceptNumber(true, true);

                   // gp.SetBasePoint(dpTwoResult.StartPt, true);
               },
               (e, dynamicPt) =>
               {

                   onOptionRun?.Invoke(e,
                       tResult,
                       cResult);
               });

                if (cResult is null)
                    return null;

            }

            return cResult;

        }



        public static RhinoArchBase Get_StartEndOptionArch(Point3d? startPt, string promptMessage, string option,
            string optionValue, Func<DrawTwoPointResult, ClickResult,bool, RhinoArchBase> buildArch)
        {
            bool showPointed = false;
            void onEndPointCallback(DrawEventArgs e, DrawTwoPointResult result)
            {

                var arch = buildArch(result, null, showPointed);
                arch?.Visualize(e);
                e.VisualizeArchEndPoints(result.StartPt, result.EndPt);

            }

            void onFoilCountCallback(DrawEventArgs e, DrawTwoPointResult result, ClickResult clickResult)
            {
               
                var arch = buildArch(result, clickResult, showPointed);
                arch?.Visualize(e);
                e.VisualizeArchEndPoints(result.StartPt, result.EndPt);

            }

            var startPoint = Point3d.Unset;
            bool isBothSides = false;
            bool isFlip = false;

            bool startPointWasProvided = false;

            if (startPt == null)
            {
                var result = RunStartLoop(ref isBothSides, null, null);

                if (result is null)
                {
                    return null;
                }

                startPoint = result.Value;
            }
            else
            {
                startPoint = startPt.Value;
                startPointWasProvided = true;
            }

            var pD = RunEndLoop_FoilArch(startPoint, startPointWasProvided, isTwoPointArch: true,
                ref isBothSides, ref isFlip, ref showPointed,
                onEndPointCallback: onEndPointCallback);

            if (pD is null)
            { return null; }



            var clickResult = RunFoilCountLoop(pD, promptMessage, option, optionValue, onFoilCountCallback);


            return buildArch?.Invoke(pD, clickResult, showPointed);


        }

    }
}
