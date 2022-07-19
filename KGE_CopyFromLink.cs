using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

namespace API_2021_Plugins
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class KGE_CopyFromLink : IExternalCommand
    { 
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)

        {
            Document hostDoc = commandData.Application.ActiveUIDocument.Document;

            //Get the link

            FilteredElementCollector links =

            new FilteredElementCollector(hostDoc)

            .OfClass(typeof(RevitLinkInstance));

            Document linkedDoc = links.Cast<RevitLinkInstance>().FirstOrDefault().GetLinkDocument();



            ////Get familys in link

            //FilteredElementCollector linkedFamCollector = new FilteredElementCollector(linkedDoc);

            //ICollection<ElementId> ids = linkedFamCollector

            //.OfClass(typeof(FamilyInstance))

            //.OfCategory(BuiltInCategory.OST_GenericModel)

            //.ToElementIds();

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


            //Get pipes in link

            FilteredElementCollector linkedElemCollector = new FilteredElementCollector(linkedDoc);

            ICollection<ElementId> ids = linkedElemCollector
            .WherePasses(allElementsFilter)
            .WhereElementIsNotElementType()
            .ToElementIds();




            if (ids.Count == 0)

            {
                TaskDialog.Show("Copy Paste", "The link does not contain the specified elements.");
            }

            else

            {

                Transaction targetTrans = new Transaction(hostDoc);

                CopyPasteOptions copyOptions = new CopyPasteOptions();

                copyOptions.SetDuplicateTypeNamesHandler(new CopyUseDestination());

                targetTrans.Start("Copy and paste linked elements");



                ElementTransformUtils.CopyElements(linkedDoc, ids, hostDoc, null, copyOptions);

                hostDoc.Regenerate();

                targetTrans.Commit();

            }

            return Result.Succeeded;

        }



        public class CopyUseDestination : IDuplicateTypeNamesHandler

        {

            public DuplicateTypeAction OnDuplicateTypeNamesFound(DuplicateTypeNamesHandlerArgs args)

            {
                return DuplicateTypeAction.UseDestinationTypes;
            }

        }



    }

}
    

