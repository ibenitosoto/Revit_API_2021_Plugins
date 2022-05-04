using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Plumbing;

namespace API_2021_Plugins
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class KGE_LinesFromPipes : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UI Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            UIApplication uiApp = commandData.Application;
            // Document doc = uiApp.ActiveUIDocument.Document;
            Application app = uiApp.Application;


            //Create Filters
            ElementCategoryFilter pipesFilter = new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves);
            ElementCategoryFilter fittingsFilter = new ElementCategoryFilter(BuiltInCategory.OST_PipeFitting);
            ElementCategoryFilter valvesFilter = new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory);

            //Multicategory filter
            //Initialize list inheriting from IList and adding all 3 categories
            IList<BuiltInCategory> allCategories = new List<BuiltInCategory>();
            allCategories.Add(BuiltInCategory.OST_PipeCurves);
            allCategories.Add(BuiltInCategory.OST_PipeFitting);
            allCategories.Add(BuiltInCategory.OST_PipeAccessory);

            //Creating the multicategory filter
            ElementMulticategoryFilter allElementsFilter = new ElementMulticategoryFilter(allCategories);

            //Collect elements separately
            IList<Element> pipes = new FilteredElementCollector(doc).WherePasses(pipesFilter).WhereElementIsNotElementType().ToElements();
            IList<Element> fittings = new FilteredElementCollector(doc).WherePasses(fittingsFilter).WhereElementIsNotElementType().ToElements();
            IList<Element> valves = new FilteredElementCollector(doc).WherePasses(valvesFilter).WhereElementIsNotElementType().ToElements();

            //Collect all elements together
            //IList<Element> allElements = new List<Element>();
            //allElements = new FilteredElementCollector(doc).WherePasses(allElementsFilter).WhereElementIsNotElementType().ToElements();

            TaskDialog.Show("Welcome", "Welcome to the KGE LineFromPipes plugin");
            TaskDialog.Show("Quantifications", $"{pipes.Count} pipes in the model\n" +
                                                $"{fittings.Count} fittings in the model\n" +
                                                $"{valves.Count} valves in the model\n");

            ICollection<ElementId> pipeIds = new List<ElementId>();

            View3D view3d = new FilteredElementCollector(doc).OfClass(typeof(View3D)).Cast<View3D>().FirstOrDefault();

            using (Transaction transaction = new Transaction(doc))
            {
                transaction.Start("Create Lines From Pipes");

                //var elementsInView = new FilteredElementCollector(doc, doc.ActiveView.Id).Cast<Element>().ToList();
                //List<ElementId> elementsInViewIDs = new List<ElementId> { };

                //foreach (Element elem in elementsInView)
                //{
                //    elementsInViewIDs.Add(elem.Id);
                //}

                foreach (Pipe pipe in pipes)
                {
                    ConnectorSet pipeConnectorSet = pipe.ConnectorManager.Connectors;
                    List<Connector> pipeConnectorList = new List<Connector>();

                    foreach (Connector connector in pipeConnectorSet)
                    {
                        pipeConnectorList.Add(connector);
                    }

                    pipeIds.Add(pipe.Id);

                    if (pipe != null)
                    {
                        LocationCurve LC = pipe.Location as LocationCurve;

                        Options op = app.Create.NewGeometryOptions();
                        op.ComputeReferences = true;
                        op.View = view3d;
                        op.IncludeNonVisibleObjects = true;

                        //Reference ref1 = null;
                        //Reference ref2 = null;
                        //ReferenceArray references = new ReferenceArray();

                        XYZ R1 = null;
                        XYZ R2 = null;

                        R1 = LC.Curve.GetEndPoint(0);
                        R2 = LC.Curve.GetEndPoint(1);

                        Curve curve = LC.Curve;





                        //// Get points in model coordinates not family coordinates
                        //XYZ coord1 = R1;//.GlobalPoint;
                        //XYZ coord2 = R2;//.GlobalPoint;

                        //Line line = null;
                        //Plane geomPlane = null;
                        //Plane plane = Plane.CreateByNormalAndOrigin(pipeConnectorList[0].CoordinateSystem.BasisY, pipeConnectorList[0].CoordinateSystem.Origin);

                        //if (Equals4DigitPrecision(coord1.X, coord2.X) && Equals4DigitPrecision(coord1.Y, coord2.Y))
                        //{
                        //    try
                        //    {
                        //        // process as vertical
                        //        line = Line.CreateBound(new XYZ(coord1.X, coord1.Y, coord1.Z), new XYZ(coord1.X, coord1.Y, coord2.Z));
                        //        geomPlane = Plane.CreateByNormalAndOrigin(XYZ.BasisX, line.Evaluate(0.5, true));
                        //    }
                        //    catch //(Exception ex)
                        //    {
                        //        //TaskDialog.Show("Exception Caught", "Exception: " + ex);
                        //    }
                        //}

                        //else
                        //{
                        //    // process as horizontal
                        //    try
                        //    {
                        //        line = Line.CreateBound(new XYZ(coord1.X, coord1.Y, coord1.Z), new XYZ(coord2.X, coord2.Y, coord1.Z));
                        //        geomPlane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, line.Evaluate(0.5, true));
                        //    }
                        //    catch //(Exception ex)
                        //    {
                        //        //TaskDialog.Show("Exception Caught", "Exception: " + ex);
                        //    }
                        //}

                        //SketchPlane sketchPlane = null;
                        //if (geomPlane != null)
                        //{
                        //    sketchPlane = SketchPlane.Create(doc, geomPlane);
                        //    view3d.SketchPlane = sketchPlane;
                        //}

                        Plane plane = Plane.CreateByNormalAndOrigin(pipeConnectorList[0].CoordinateSystem.BasisY, pipeConnectorList[0].CoordinateSystem.Origin);
                        SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                        ModelCurve pipeLine = doc.Create.NewModelCurve(curve, sketchPlane);


                    }

                   
                }



                doc.Delete(pipeIds);

                //try
                //{
                //    doc.Delete(elementsInViewIDs);
                //}
                //catch //(Exception)
                //{

                //    //throw;
                //}
                

                transaction.Commit();
            }




            return Result.Succeeded;
            
        }//end of Result Execute method



        private static bool Equals4DigitPrecision(double left, double right)
        {
            return Math.Abs(left - right) < 0.0001;
        }


    }//end of External command class
}
