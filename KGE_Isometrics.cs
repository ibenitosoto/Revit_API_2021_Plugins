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
    public class KGE_Isometrics : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UI Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;


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
            IList<Element> allElements = new List<Element>();
            allElements = new FilteredElementCollector(doc).WherePasses(allElementsFilter).WhereElementIsNotElementType().ToElements();

            TaskDialog.Show("Welcome", "Welcome to the KGE Isometrics plugin");
            TaskDialog.Show("Quantifications", $"{pipes.Count} pipes in the model\n" +
                                                $"{fittings.Count} fittings in the model\n" +
                                                $"{valves.Count} valves in the model\n");




            var lineNumberGroups = from e in allElements
                                   //group e by e.GetParameters("EZ_LINE NO").ToString();
                                   group e by e.LookupParameter("EZ_LINE NO").AsString();


            ////Create 3D view to be used later as a template for all assemblies
            //using (Transaction transaction = new Transaction(doc))
            //{
            //    transaction.Start("Create View Template");
            //    View3D newView = View3D.CreateIsometric()
            //    transaction.Commit();

                int lineNumberCounter = 0;
                //View view3dTemplate = null;
                //ElementId view3dTemplateId = null;

                foreach (var lineNumberGroup in lineNumberGroups)
                {
                    //TaskDialog.Show("Line Number Groups", $"Line number {lineNumberGroup.Key} found\n" +
                    //    $"Number of elements with that line number: {lineNumberGroup.Count()}");

                    List<ElementId> elementIDs = new List<ElementId>();

                    foreach (Element e in lineNumberGroup)
                    {
                        //TaskDialog.Show("Category", $"Category of element is {e.Category.Name}");

                        //Get Id of current element
                        ElementId elementId = e.Id;

                        //Add Id to previously created list of element Ids
                        elementIDs.Add(elementId);

                        //Get insulation associated with each element
                        IList<Element> insulationList = new List<Element>();
                    
                        //Get id of insulation associated with this element
                        ICollection<ElementId> insulationId = InsulationLiningBase.GetInsulationIds(doc, elementId);

                        //Certain elements such as valves might not have insulation
                        if (insulationId.Count > 0)
                        {
                            //Get the insulation element with previously found Id
                            //Element insulation = doc.GetElement(insulationId.FirstOrDefault());

                            //Add the insulation to the line number group
                            elementIDs.Add(insulationId.FirstOrDefault());
                        }

                        else
                        {
                            break;
                        }
                    }


                    using (Transaction transaction = new Transaction(doc))
                    {
                        ElementId categoryId = lineNumberGroup.First().Category.Id;
                        transaction.Start("Create Assembly Instance");
                        AssemblyInstance assemblyInstance = AssemblyInstance.Create(doc, elementIDs, categoryId);
                        transaction.Commit(); // need to commit the transaction to complete the creation of the assembly instance so it can be accessed in the code below

                        string assemblyName = lineNumberGroup.Key.ToString();

                        if (transaction.GetStatus() == TransactionStatus.Committed)
                        {
                            lineNumberCounter++;
                            transaction.Start("Set Assembly Name");
                            if (assemblyName.Length > 0)
                            {
                                assemblyInstance.AssemblyTypeName = assemblyName;
                            }

                            else
                            {
                                assemblyInstance.AssemblyTypeName = "naming_error";
                            }
                        
                            transaction.Commit();

                        }

                        else
                        {
                            break;
                        }



                        if (assemblyInstance.AllowsAssemblyViewCreation()) // check to see if views can be created for this assembly
                        {

                            if (transaction.GetStatus() == TransactionStatus.Committed)
                            {

                                transaction.Start("View Creation");
                                ElementId titleBlockId = new FilteredElementCollector(doc)
                                     .OfClass(typeof(FamilySymbol))
                                     .OfCategory(BuiltInCategory.OST_TitleBlocks)
                                     .Cast<FamilySymbol>()
                                     .FirstOrDefault().Id;

       
                                ViewSheet viewSheet = AssemblyViewUtils.CreateSheet(doc, assemblyInstance.Id, titleBlockId);
                                View3D view3d = AssemblyViewUtils.Create3DOrthographic(doc, assemblyInstance.Id);
                                view3d.Name = $"{assemblyName}";
                                view3d.DetailLevel = ViewDetailLevel.Coarse;
                                view3d.Scale = 7;

                                ////if its the first assembly created and 3D view can be duplicated
                                //if (lineNumberCounter == 1 && view3d.CanViewBeDuplicated(ViewDuplicateOption.Duplicate))
                                //{
                                //    view3dTemplateId = view3d.Duplicate(ViewDuplicateOption.Duplicate);
                                //    view3dTemplate = doc.GetElement(view3dTemplateId) as View3D;
                                //    view3dTemplate.Name = "IsometricsTemplate";
                                //}


                                //if (view3dTemplateId != null)
                                //{
                                //    view3d.ViewTemplateId = view3dTemplateId;
                                //}

                                //setting the display style to wireframe so pipes,fittings and valves are visible under insulations
                                view3d.DisplayStyle = DisplayStyle.Wireframe;

                                //assign a different detail level to insulations so they show in Coarse 3D Views
                                //getting the pipework insulations category
                                Category insulationCategory = Category.GetCategory(doc, BuiltInCategory.OST_PipeInsulations);
                            
                                //Overriding the detail level of pipe insulations in each view
                                OverrideGraphicSettings insulationOverride = new OverrideGraphicSettings();
                                insulationOverride.SetDetailLevel(ViewDetailLevel.Fine);

                                //Overriding the pattern of insulation lines to show as dashed
                                LinePatternElement dashedPattern = LinePatternElement.GetLinePatternElementByName(doc, "Dash");
                                insulationOverride.SetProjectionLinePatternId(dashedPattern.Id);   //this changes the view VV settings but doesnt work

                                //Applying category overrides to the 3D view
                                view3d.SetCategoryOverrides(insulationCategory.Id, insulationOverride);

  

                                //OverrideGraphicSettings dashedPatternOverride = new OverrideGraphicSettings();
                                //dashedPatternOverride.SetProjectionLinePatternId(dashedPattern.Id);
                                


                                //lock the view to be able to add dimensions
                                if (view3d.CanBeLocked())
                                {
                                    view3d.SaveOrientationAndLock();
                                }

                                else
                                {
                                    break;
                                }


                                //List<Element> elementsInView = new FilteredElementCollector(doc, viewSheet.Id).ToList();
                                //ViewSchedule viewSchedule = AssemblyViewUtils.CreateSingleCategorySchedule(doc, assemblyInstance.Id);

                                //ViewSection elevationTop = AssemblyViewUtils.CreateDetailSection(doc, assemblyInstance.Id, AssemblyDetailViewOrientation.ElevationTop);
                                //ViewSection elevationLeft = AssemblyViewUtils.CreateDetailSection(doc, assemblyInstance.Id, AssemblyDetailViewOrientation.ElevationLeft);
                                //ViewSection elevationRight = AssemblyViewUtils.CreateDetailSection(doc, assemblyInstance.Id, AssemblyDetailViewOrientation.ElevationRight);
                                //ViewSection elevationFront = AssemblyViewUtils.CreateDetailSection(doc, assemblyInstance.Id, AssemblyDetailViewOrientation.ElevationFront);
                                //ViewSection detailSectionA = AssemblyViewUtils.CreateDetailSection(doc, assemblyInstance.Id, AssemblyDetailViewOrientation.DetailSectionA);
                                //ViewSection detailSectionB = AssemblyViewUtils.CreateDetailSection(doc, assemblyInstance.Id, AssemblyDetailViewOrientation.DetailSectionB);
                                //ViewSection detailSectionH = AssemblyViewUtils.CreateDetailSection(doc, assemblyInstance.Id, AssemblyDetailViewOrientation.HorizontalDetail);
                                //ViewSchedule materialTakeoff = AssemblyViewUtils.CreateMaterialTakeoff(doc, assemblyInstance.Id);
                                ViewSchedule partList = AssemblyViewUtils.CreatePartList(doc, assemblyInstance.Id);
                                ScheduleDefinition partSchedule = partList.Definition;
                                partSchedule.IsItemized = false;

                                ICollection<ElementId> schedulableFields = partSchedule.GetValidCategoriesForEmbeddedSchedule();

                                //WORK IN PROGRESS
                                //List<string> schedulableFieldNames = new List<string>();
                                //foreach (ElementId elementId in schedulableFields)
                                //{
                                //    schedulableFieldNames.Add(elementId);
                                //}

                                Viewport.Create(doc, viewSheet.Id, view3d.Id, new XYZ(1, 1, 0));
                                //Viewport.Create(doc, viewSheet.Id, elevationTop.Id, new XYZ(2, 2, 0));
                                //Viewport.Create(doc, viewSheet.Id, elevationLeft.Id, new XYZ(1, 1.7, 0));
                                //Viewport.Create(doc, viewSheet.Id, elevationRight.Id, new XYZ(2.5, 2, 0));
                                //Viewport.Create(doc, viewSheet.Id, elevationFront.Id, new XYZ(2, 1, 0));
                                //Viewport.Create(doc, viewSheet.Id, detailSectionA.Id, new XYZ(1.5, 1.25, 0));
                                //Viewport.Create(doc, viewSheet.Id, detailSectionB.Id, new XYZ(0.5, 1.5, 0));
                                //Viewport.Create(doc, viewSheet.Id, detailSectionH.Id, new XYZ(1.5, 2, 0));
                                //ScheduleSheetInstance.Create(doc, viewSheet.Id, materialTakeoff.Id, new XYZ(2, 2.5, 0));
                                ScheduleSheetInstance.Create(doc, viewSheet.Id, partList.Id, new XYZ(2.5, 2.5, 0));

                                transaction.Commit();


                            }



             

                        }

                



                    }//end of assembly creation transaction

                }//end of foreach loop through all line numbers

            return Result.Succeeded;

        }
    }
}
