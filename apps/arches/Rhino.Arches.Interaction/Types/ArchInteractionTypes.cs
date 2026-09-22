
using Rhino.Arches.Core.Extensions;
using Rhino.Arches.Interaction.Constants;
using Rhino.Arches.Interaction.Faactory;
using Rhino.Display;
using Rhino.Geometry;

namespace Rhino.Arches.Interaction.Types
{
    public static partial class ArchInteractionTypes
    {

        static Point3d? RunStartLoop(
ref bool isBothSides, Action<DrawEventArgs,
Point3d> onStartPointCallback = null,
Func<ClickResult, ClickResult> onStartOptionSelected = null)
        {
            bool UseBothSide = false;
            ClickResult startResult =
                startResult = ClickFactory.GetPoint(
                                  Prompts.GetFirstEndPointPrompt,
                                  (gp) =>
                                  {
                                      gp.AddOption(Options.BothSides);
                                  },
                                  onStartPointCallback);

            if (startResult is null)
                return null;
            if (startResult.OptionIndex == 1)
                isBothSides = !isBothSides;
            while (startResult is not null && startResult.Point is null)
            {
                UseBothSide = isBothSides;
                startResult =   ClickFactory.GetPoint(
                                Prompts.GetFirstEndPointPrompt,
                                (gp) =>
                                {
                                    if (UseBothSide)
                                        gp.AddOption(Options.ByEndPoints);
                                    else
                                        gp.AddOption(Options.ByEndPoints);
                                },
                                onStartPointCallback);

                if (startResult.OptionIndex > 1)
                    startResult = onStartOptionSelected?.Invoke(startResult);
                else if (startResult.OptionIndex == 1)
                {
                    isBothSides = !isBothSides;
                }
            }

            if (startResult is null || startResult.Point is null)
                return null;


            return startResult.Point.Value;
        }

         static DrawTwoPointResult RunEndLoop(Point3d startPoint,
      bool startPointWasProvided,
      bool isTwoPointArch,
      ref bool isBothSides,
      ref bool isFlip,
      Action<DrawEventArgs, DrawTwoPointResult> onEndPointCallback = null)
        {
            var newStart = startPoint;
            bool useBothSide = isBothSides;
            bool useFlip = isFlip;
            var endResult = ClickFactory.GetPoint(
            Prompts.GetSecondEndPointPrompt,
        (gp) =>
        {
            gp.SetBasePoint(startPoint, true);
            if (startPointWasProvided)
                gp.AddOption(Options.BothSides);

            if (isTwoPointArch)
                gp.AddOption(Options.Flip);

        },
        (e, dynamicPt) =>
        {

            if (useBothSide)
            {
                var center = startPoint;
                var ep = dynamicPt;
                var dir = ep - center;
                dir.Unitize();
                if (dir.Length > 0)
                {
                    newStart = center - dir * center.DistanceTo(ep);
                }
            }

            onEndPointCallback?.Invoke(e, new DrawTwoPointResult(newStart, dynamicPt,
                useBothSide, useFlip, string.Empty));
        });

            if (endResult == null)
                return null;

            if (startPointWasProvided)
            {
                if (endResult.OptionIndex == 1)
                    isBothSides = !isBothSides;

                isFlip = endResult.OptionIndex == 2;
            }
            else
                isFlip = endResult.OptionIndex == 1;

            //only if endresult is not null && custom option is not selected
            while (endResult is not null &&
                endResult.OptionIndex != -1) // both sides
            {
                useBothSide = isBothSides;
                useFlip = isFlip;

                endResult = ClickFactory.GetPoint(
                 Prompts.GetSecondEndPointPrompt,
                     (gp) =>
                     {
                         gp.SetBasePoint(startPoint, true);
                         if (isTwoPointArch)
                         {
                             gp.AddOption(Options.Flip);
                         }

                     },
                     (e, dynamicPt) =>
                     {
                         if (useBothSide)
                         {
                             var center = startPoint;
                             var ep = dynamicPt;
                             var dir = ep - center;
                             dir.Unitize();
                             if (dir.Length > 0)
                             {
                                 newStart = center - dir * center.DistanceTo(ep);
                             }
                         }

                         onEndPointCallback?.Invoke(e, new DrawTwoPointResult(newStart, dynamicPt,
                             useBothSide, useFlip, endResult.OptionName));
                     });

                if (endResult is null)
                    return null;
                if (endResult.OptionIndex == 1)
                {
                    isFlip = !isFlip;
                }
            }

            if (endResult is null || endResult.Point is null)
                return null;

            var pD = new DrawTwoPointResult(newStart, endResult.Point.Value, isBothSides, isFlip,
                endResult.OptionName);
            return pD;
        }


        static DrawTwoPointResult RunEndLoop_FoilArch(Point3d startPoint,
bool startPointWasProvided,
bool isTwoPointArch,
ref bool isBothSides,
ref bool isFlip,
ref bool showPointed,
Action<DrawEventArgs, DrawTwoPointResult> onEndPointCallback = null)
        {
            var newStart = startPoint;
            bool useBothSide = isBothSides;
            bool useFlip = isFlip;
            bool isPointed = showPointed;
            var endResult = ClickFactory.GetPoint(
            Prompts.GetSecondEndPointPrompt,
        (gp) =>
        {
            gp.SetBasePoint(startPoint, true);
            if (startPointWasProvided)
                gp.AddOption(Options.BothSides);

            if (isTwoPointArch)
                gp.AddOption(Options.Flip);


            gp.AddOption(Options.Pointed);

        },
        (e, dynamicPt) =>
        {

            if (useBothSide)
            {
                var center = startPoint;
                var ep = dynamicPt;
                var dir = ep - center;
                dir.Unitize();
                if (dir.Length > 0)
                {
                    newStart = center - dir * center.DistanceTo(ep);
                }
            }

            onEndPointCallback?
            .Invoke(e,
                new DrawTwoPointResult(newStart, dynamicPt,
                useBothSide, useFlip, string.Empty));
        });

            if (endResult == null)
                return null;

            if (startPointWasProvided)
            {
                if (endResult.OptionIndex == 1)
                    isBothSides = !isBothSides;

                isFlip = endResult.OptionIndex == 2;
            }
            else
                isFlip = endResult.OptionIndex == 1;

            //only if endresult is not null && custom option is not selected
            while (endResult is not null &&
                endResult.OptionIndex != -1) // both sides
            {
                useBothSide = isBothSides;
                useFlip = isFlip;
                if(endResult.OptionIndex == 2 || endResult.OptionIndex == 3)
                {
                    isPointed = !isPointed;
                    showPointed = isPointed;
                }

                endResult = ClickFactory.GetPoint(
                 Prompts.GetSecondEndPointPrompt,
                     (gp) =>
                     {
                         gp.SetBasePoint(startPoint, true);
                         if (isTwoPointArch)
                         {
                             gp.AddOption(Options.Flip);
                         }

                         if(isPointed)
                         {
                             gp.AddOption(Options.Rounded);
                         }else
                         {
                             gp.AddOption(Options.Pointed);
                         }

                     },
                     (e, dynamicPt) =>
                     {
                         if (useBothSide)
                         {
                             var center = startPoint;
                             var ep = dynamicPt;
                             var dir = ep - center;
                             dir.Unitize();
                             if (dir.Length > 0)
                             {
                                 newStart = center - dir * center.DistanceTo(ep);
                             }
                         }

                         onEndPointCallback?.Invoke(e, new DrawTwoPointResult(newStart, dynamicPt,
                             useBothSide, useFlip, endResult.OptionName));
                     });

                if (endResult is null)
                    return null;
                if (endResult.OptionIndex == 1)
                {
                    isFlip = !isFlip;
                }
            }

            if (endResult is null || endResult.Point is null)
                return null;

            var pD = new DrawTwoPointResult(newStart, endResult.Point.Value, isBothSides, isFlip,
                endResult.OptionName);
            return pD;
        }



        static DrawThreePointResult RunApexLoop(DrawTwoPointResult dpTwoResult,
   Func<DrawEventArgs, DrawThreePointResult, ApexCallbackResult> onApexPointCallback = null)
        {
            double inputArchHeight = double.NaN;
            ApexCallbackResult apexCallbackResult = null;
            Point3d apexPoint = Point3d.Unset;
            var apexResult = ClickFactory.GetPoint(
               Prompts.GetApexPoint,
               (gp) =>
               {
                   gp.AddOption(Options.ByArchHeight);
               },
               (e, dynamicPt) =>
               {
                   apexPoint = dynamicPt;
                   apexCallbackResult = onApexPointCallback?.Invoke(e,
                     new DrawThreePointResult(dpTwoResult.StartPt, dpTwoResult.EndPt,
                     apexPoint, dpTwoResult.BothSides, -1));
               });

            if (apexResult is null)
                return null;

            //var mid = dpTwoResult.EndPt - dpTwoResult.StartPt / 2;
            while (apexResult.OptionIndex == 1 || apexResult.Number is not double.NaN)
            {
                // ask again
                apexResult = ClickFactory.GetPoint(
               Prompts.GetArchHeightPrompt,
               (gp) =>
               {
                   gp.AcceptNothing(true);
                   gp.AcceptNumber(true, true);
               },
               (e, dynamicPt) =>
               {

                   if (inputArchHeight is not double.NaN)
                   {
                       var normal = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.GetConstructionPlane().Plane.Normal;
                       var perpDir = ArcGeometry.GetPerpendicular(dpTwoResult.StartPt, dpTwoResult.EndPt, normal);
                       var mid = (dpTwoResult.StartPt + dpTwoResult.EndPt) / 2;
                       apexPoint = mid + perpDir * inputArchHeight;
                   }

                   apexCallbackResult = onApexPointCallback?.Invoke(e,
                                        new DrawThreePointResult(dpTwoResult.StartPt, dpTwoResult.EndPt,
                                        apexPoint, dpTwoResult.BothSides, -1));
               });

                if (apexResult is null)
                    return null;

                if (apexResult.Number is not double.NaN)
                {
                    inputArchHeight = double.NaN;
                    if (Math.Abs(apexResult.Number) > apexCallbackResult.MaximumRisePossible)
                    {
                        RhinoApp.WriteLine($"Arch height should be less than <{Math.Round(apexCallbackResult.MaximumRisePossible, 2)}>");
                    }
                    else if (Math.Abs(apexResult.Number) < apexCallbackResult.MinimumRisePossible)
                    {
                        RhinoApp.WriteLine($"Arch height should be more than <{Math.Round(apexCallbackResult.MinimumRisePossible, 2)}>");
                    }
                    else
                    {
                        inputArchHeight = apexResult.Number;
                    }

                }

            }

            if (apexResult == null)
                return null;


            return new DrawThreePointResult(dpTwoResult.StartPt, dpTwoResult.EndPt,
                 apexPoint, dpTwoResult.BothSides,
                 apexResult.OptionIndex);
        }


    }
}
