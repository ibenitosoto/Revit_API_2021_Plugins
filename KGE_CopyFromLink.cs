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
        public static IList<BuiltInCategory> allCategories = new List<BuiltInCategory>();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)

        {
            Document hostDoc = commandData.Application.ActiveUIDocument.Document;

            //Launch WPF interface
            Show_KGE_CopyFromLink_WPF(commandData, ref message, elements);


            ////Get the link
            //FilteredElementCollector links =
            //new FilteredElementCollector(hostDoc)
            //.OfClass(typeof(RevitLinkInstance));
            //Document linkedDoc = links.Cast<RevitLinkInstance>().FirstOrDefault().GetLinkDocument();


            ////Get families in link
            //FilteredElementCollector linkedFamCollector = new FilteredElementCollector(linkedDoc);
            //ICollection<ElementId> ids = linkedFamCollector
            //.OfClass(typeof(FamilyInstance))
            //.OfCategory(BuiltInCategory.OST_GenericModel)
            //.ToElementIds();


            ////Multicategory filter
            //allCategories.Add(BuiltInCategory.OST_GenericModel);
            //allCategories.Add(BuiltInCategory.OST_MechanicalEquipment);
            //allCategories.Add(BuiltInCategory.OST_PlumbingFixtures);
            //allCategories.Add(BuiltInCategory.OST_PipeCurves);
            //allCategories.Add(BuiltInCategory.OST_PipeFitting);
            //allCategories.Add(BuiltInCategory.OST_PipeAccessory);
            //allCategories.Add(BuiltInCategory.OST_DuctCurves);
            //allCategories.Add(BuiltInCategory.OST_DuctFitting);
            //allCategories.Add(BuiltInCategory.OST_DuctAccessory);
            //allCategories.Add(BuiltInCategory.OST_FlexDuctCurves);
            //allCategories.Add(BuiltInCategory.OST_DuctTerminal);
            //allCategories.Add(BuiltInCategory.OST_Sprinklers);
            //allCategories.Add(BuiltInCategory.OST_ElectricalEquipment);
            //allCategories.Add(BuiltInCategory.OST_ElectricalFixtures);
            //allCategories.Add(BuiltInCategory.OST_CableTray);
            //allCategories.Add(BuiltInCategory.OST_CableTrayFitting);
            //allCategories.Add(BuiltInCategory.OST_Conduit);
            //allCategories.Add(BuiltInCategory.OST_ConduitFitting);
            //allCategories.Add(BuiltInCategory.OST_LightingDevices);
            //allCategories.Add(BuiltInCategory.OST_LightingFixtures);
            //allCategories.Add(BuiltInCategory.OST_FireAlarmDevices);
            //allCategories.Add(BuiltInCategory.OST_DataDevices);
  

            return Result.Succeeded;

        }


        public static List<Document> GetLoadedLinks(Document doc)
        {
            List<Document> rvtLinkInstancesList = new List<Document>(); 

            using (FilteredElementCollector rvtLinks = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).OfClass(typeof(RevitLinkType)))
            {
                if (rvtLinks.ToElements().Count > 0)
                {
                    foreach (RevitLinkType rvtLink in rvtLinks.ToElements())
                    {
                        if (rvtLink.GetLinkedFileStatus() == LinkedFileStatus.Loaded)
                        {
                            RevitLinkInstance link = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).OfClass(typeof(RevitLinkInstance)).Where(x => x.GetTypeId() == rvtLink.Id).First() as RevitLinkInstance;
                            rvtLinkInstancesList.Add(link.Document);
                        }
                    }
                }
                else
                {
                    return null;
                }

                return rvtLinkInstancesList;
            }
        }


        public void Show_KGE_CopyFromLink_WPF(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get WPF Interface
            KGE_CopyFromLink_WPF copyFromLinkForm = new KGE_CopyFromLink_WPF(commandData);
            copyFromLinkForm.ShowDialog();
        }

        public static ICollection<ElementId> GetElementIds(Document selectedLink, ElementMulticategoryFilter allElementsFilter)
        {
            FilteredElementCollector linkedElemCollector = new FilteredElementCollector(selectedLink);
            ICollection<ElementId> ids = linkedElemCollector
            .WherePasses(allElementsFilter)
            .WhereElementIsNotElementType()
            .ToElementIds();

            return ids;
        }

        public static void CopyElements(ICollection<ElementId> ids, Document hostDoc, Document linkedDoc)
        {
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
    

