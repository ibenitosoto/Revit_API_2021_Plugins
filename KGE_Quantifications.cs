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
    public class KGE_Quantifications : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UI Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            //Create Filtered Element Collector
            FilteredElementCollector collector1 = new FilteredElementCollector(doc);
            FilteredElementCollector collector2 = new FilteredElementCollector(doc);
            FilteredElementCollector collector3 = new FilteredElementCollector(doc);

            //Create Category Filters
            //ElementCategoryFilter pipesFilter = new ElementCategoryFilter(BuiltInCategory.OST_PipeSegments);
            //ElementCategoryFilter ductsFilter = new ElementCategoryFilter(BuiltInCategory.OST_DuctCurves);
            //ElementCategoryFilter traysFilter = new ElementCategoryFilter(BuiltInCategory.OST_CableTray);

            ElementClassFilter pipesFilter = new ElementClassFilter(typeof(Pipe));
            ElementClassFilter ductsFilter = new ElementClassFilter(typeof(Duct));
            ElementClassFilter traysFilter = new ElementClassFilter(typeof(CableTray));

            //Apply Filters
            IList<Element> pipes = collector1.WherePasses(pipesFilter).WhereElementIsNotElementType().ToElements();
            IList<Element> ducts = collector2.WherePasses(ductsFilter).WhereElementIsNotElementType().ToElements();
            IList<Element> trays = collector3.WherePasses(traysFilter).WhereElementIsNotElementType().ToElements(); 

            double pipeLength;
            double pipeTotalLengths = 0;

            foreach (Element element in pipes)
            {
                pipeLength = element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                pipeTotalLengths += pipeLength;
            }

            double ductLength;
            double ductTotalLengths = 0;

            foreach (Element element in ducts)
            {
                ductLength = element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                ductTotalLengths += ductLength;
            }

            TaskDialog.Show("Quantifications", $"{pipes.Count} pipes in the model\n" +
                                                $"{ducts.Count} ducts in the model\n" +
                                                $"{trays.Count} trays in the model\n" +
                                                $"Total length of pipes {pipeTotalLengths} m\n" +
                                                $"Total length of ducts {ductTotalLengths} m");

            return Result.Succeeded;

        }
    }
}