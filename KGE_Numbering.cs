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

            try
            {
                //pick object
                Reference pickedObj = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                //element Id
                ElementId pickedElementId = pickedObj.ElementId;

                //element
                Element pickedElement = doc.GetElement(elementId);

                if (pickedObj != null)
                {
                    TaskDialog.Show("Picked Object", "Element selected: " + pickedObj.ElementId.ToString() + pickedObj;
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }


  


































        }//end of Result Execute method
    }
}
