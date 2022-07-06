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
    public class KGE_Numbering : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UI Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            UIApplication uiApp = commandData.Application;
            //Document doc = uiApp.ActiveUIDocument.Document;
            Application app = uiApp.Application;

            //Create Filters
            ElementCategoryFilter pipesFilter = new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves);

            //Name of the numbering parameter
            string numberingParameterName = "Identifier";

            //Collect pipes
            IList<Element> pipes = new FilteredElementCollector(doc).WherePasses(pipesFilter).WhereElementIsNotElementType().ToElements();

            //list with elements belonging to the same system as the picked one
            //List<Element> allElementsInSystem = new List<Element>();

            try
            {
                //pick object
                Reference pickedObj = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                //element Id
                ElementId pickedElementId = pickedObj.ElementId;

                //element
                Element pickedElement = doc.GetElement(pickedElementId);

                if (pickedElement != null)
                {
                    TaskDialog.Show("Picked Object", $"Element selected: {pickedElement.Category.Name} with ID number {pickedElementId}");

                    string systemAbb = pickedElement.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString();

                    var allElementsInSystem = from pipe in pipes
                                              where pipe.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString() == systemAbb
                                              select pipe;
                }
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
  


































        }//end of Result Execute method
    }
}
