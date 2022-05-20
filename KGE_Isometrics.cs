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
        public static int lineNumberCounter = 0;
        public static int assemblyCounter = 0;
        public static int dimensionCounter = 0;
        public static int pipeTagsCounter = 0;
        public static List<IndependentTag> pipeTagsList = new List<IndependentTag>();
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
            ElementCategoryFilter fittingsFilter = new ElementCategoryFilter(BuiltInCategory.OST_PipeFitting);
            ElementCategoryFilter valvesFilter = new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory);

            //Name of the line number parameter
            string lineNumberParameterName = "Line Number";

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

            //pipe tags checker
            ElementCategoryFilter pipeTagsFilter0 = new ElementCategoryFilter(BuiltInCategory.OST_PipeTags);
            IList<Element> pipeTagsCreated0 = new FilteredElementCollector(doc).WherePasses(pipeTagsFilter0).WhereElementIsNotElementType().ToElements();


            TaskDialog.Show("Welcome", "Welcome to the KGE Isometrics plugin");
            TaskDialog.Show("Quantifications", $"{pipes.Count} pipes in the model\n" +
                                                $"{fittings.Count} fittings in the model\n" +
                                                $"{valves.Count} valves in the model\n" +
                                                $"{pipeTagsCreated0.Count} pipe tags in the model\n");
            

            #region Line number initial check

            //lists of elements splitting the ones that have line number from the ones who don't
            List<Element> emptyLineNumberElements = new List<Element>();
            List<Element> filledLineNumberElements = new List<Element>();

            //storing ids and names for all the ones with empty line numbers
            List<string> emptyLineNumberList = new List<string>();

            //looping through all elements to proceed with the splitting
            foreach (Element element in allElements)
            {
                try
                {
                    string elementLineNumber = element.LookupParameter(lineNumberParameterName).AsString();

                    if (elementLineNumber == "" || elementLineNumber == null)
                    {
                        emptyLineNumberElements.Add(element);
                        emptyLineNumberList.Add(string.Format("{0,20}{1,30}{2,50}\n",
                                                element.Id.ToString(),
                                                element.Category.Name,
                                                element.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString()));
                    }

                    else
                    {
                        filledLineNumberElements.Add(element);
                    }
                }
                catch (Exception)
                {
                    TaskDialog.Show("Line Number Parameter Error", "Parameter - Line Number - must be added to the project and filled");
                }
                

      
            }

            

            bool CanContinue = true;

            //uncomment this code to forbid execution until all line numbers are filled
            //if (emptyLineNumberElements.Count > 0)
            //{
            //    CanContinue = false;
            //}

            


            //TaskDialog.Show("Line Number Check", $"________________________LINE NUMBER CHECK________________________\n\n" +
            //                                        $"Elements with line numbers filled: {filledLineNumberElements.Count}\n\n" +
            //                                        $"Elements with line numbers empty: {emptyLineNumberElements.Count}\n\n" +
            //                                        $"Line numbers have to be filled for the following elements:\n\n" +
            //                                        string.Join("", emptyLineNumberList));
            


            
            

            #endregion

            if (CanContinue)
            {
                TaskDialog.Show("Can Continue", "All elements have a line number assigned. The program will execute now.");
                #region line number grouping and automatic assembly & sheet creation and tagging process


                var lineNumberGroups = from e in filledLineNumberElements
                                           //from e in allElements
                                           //group e by e.GetParameters(lineNumberParameterName).ToString();
                                       group e by e.LookupParameter(lineNumberParameterName).AsString();


            ////Create 3D view to be used later as a template for all assemblies
            //using (Transaction transaction = new Transaction(doc))
            //{
            //    transaction.Start("Create View Template");
            //    View3D newView = View3D.CreateIsometric()
            //    transaction.Commit();

  
                //View view3dTemplate = null;
                //ElementId view3dTemplateId = null;

                foreach (var lineNumberGroup in lineNumberGroups)
                {
                    //TaskDialog.Show("Line Number Groups", $"Line number {lineNumberGroup.Key} found\n" +
                    //    $"Number of elements with that line number: {lineNumberGroup.Count()}");

                    List<ElementId> elementIDs = new List<ElementId>();
                    List<Element> pipesInView = new List<Element>();

                    foreach (Element e in lineNumberGroup)
                    {
                        //TaskDialog.Show("Category", $"Category of element is {e.Category.Name}");
                        string catName = e.Category.Name;

                        if (e.Category.Name == "Pipes")
                        {
                            pipesInView.Add(e);
                        }
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

                    }


                    using (Transaction transaction = new Transaction(doc))
                    {
                        ElementId categoryId = lineNumberGroup.First().Category.Id;
                        transaction.Start("Create Assembly Instance");
                        AssemblyInstance assemblyInstance = AssemblyInstance.Create(doc, elementIDs, categoryId);
                        transaction.Commit(); // need to commit the transaction to complete the creation of the assembly instance so it can be accessed in the code below
                        assemblyCounter++;

                        string assemblyName = lineNumberGroup.Key.ToString();

                        //try
                        //{
                        //    string assemblyName = lineNumberGroup.Key.ToString();
                        //}
                        //catch (Exception)
                        //{
                        //}
                        

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

                                

                                //automated dimensions, tags
                                foreach (Pipe pipe in pipesInView)
                                {
                                    if (pipe != null)
                                    {
                                        LocationCurve pipeLocationCurve = pipe.Location as LocationCurve;
    
                                        Options op = app.Create.NewGeometryOptions();
                                        op.ComputeReferences = true;
                                        op.View = view3d;
                                        op.IncludeNonVisibleObjects = true;

                                        Reference ref1 = null;
                                        Reference ref2 = null;
                                        ReferenceArray references = new ReferenceArray();

                                        XYZ pipeEndPoint1 = null;
                                        XYZ pipeEndPoint2 = null;

                                        pipeEndPoint1 = pipeLocationCurve.Curve.GetEndPoint(0);
                                        pipeEndPoint2 = pipeLocationCurve.Curve.GetEndPoint(1);

                                        //CreateIndependentTag(doc, view3d, pipe, pipeLocationCurve);

                                        // define tag mode and tag orientation for new tag
                                        var tagMode = TagMode.TM_ADDBY_CATEGORY;
                                        var tagOrientation = TagOrientation.Horizontal;

                                        // Add the tag to the middle of the pipe
                                        //var pipeStart = pipeLocationCurve.Curve.GetEndPoint(0);
                                        //var pipeEnd = pipeLocationCurve.Curve.GetEndPoint(1);

                                        var pipeMid = pipeLocationCurve.Curve.Evaluate(0.5, true);
                                        Reference pipeRef = new Reference(pipe);

                                        //THIS DOC.REGENERATE IS KEY OR PIPE TAGS WONT EVER REALLY BE CREATED
                                        doc.Regenerate();

                                        IndependentTag newTag = IndependentTag.Create(doc, view3d.Id, pipeRef, false, tagMode, tagOrientation, pipeMid);
                                        

                                        //if (newTag == null)
                                        //{
                                        //    throw new Exception("Create IndependentTag Failed.");
                                        //}
                                        //else
                                        //{
                                        //    newTag.HasLeader = true;
                                        //    ////  set leader mode free
                                        //    //// otherwise leader end point move with elbow point
                                        //    newTag.LeaderEndCondition = LeaderEndCondition.Free;
                                        //    var elbowPnt = pipeMid + new XYZ(5.0, 5.0, 0.0);
                                        //    newTag.LeaderElbow = elbowPnt;
                                        //    var headerPnt = pipeMid + new XYZ(10.0, 10.0, 0.0);
                                        //    newTag.TagHeadPosition = headerPnt;

                                        //    pipeTagsCounter++;
                                        //    pipeTagsList.Add(newTag);
                                        //    //AddToNewPipeTagsList(pipeTagsList, newTag);
                                        //}
                            

                                        foreach (var geoObj in pipe.get_Geometry(op))
                                        {
                                            Curve c = geoObj as Curve;
                                            if (c != null)
                                            {
                                                ref1 = c.GetEndPointReference(0);
                                                ref2 = c.GetEndPointReference(1);
                                                references.Append(ref1);
                                                references.Append(ref2);
                                            }
                                        }

                                        // Get points in model coordinates not family coordinates
                                        XYZ coord1 = pipeEndPoint1;//.GlobalPoint;
                                        XYZ coord2 = pipeEndPoint2;//.GlobalPoint;

                                        Line line = null;
                                        Plane geomPlane = null;
                                        if (Equals4DigitPrecision(coord1.X, coord2.X) && Equals4DigitPrecision(coord1.Y, coord2.Y))
                                        {
                                            try
                                            {
                                                // process as vertical
                                                line = Line.CreateBound(new XYZ(coord1.X, coord1.Y, coord1.Z), new XYZ(coord1.X, coord1.Y, coord2.Z));
                                                //line = Line.CreateBound(new XYZ(coord1.X + 1, coord1.Y, coord1.Z), new XYZ(coord1.X + 1, coord1.Y, coord2.Z));
                                                geomPlane = Plane.CreateByNormalAndOrigin(XYZ.BasisX, line.Evaluate(0.5, true));
                                            }
                                            catch (Exception)
                                            {
                                                //TaskDialog.Show("Exception Caught", "Exception: " + ex);
                                            }
                                        }

                                        else
                                        {
                                        // process as horizontal
                                            try
                                            {
                                                line = Line.CreateBound(new XYZ(coord1.X, coord1.Y, coord1.Z), new XYZ(coord2.X, coord2.Y, coord1.Z));
                                                //line = Line.CreateBound(new XYZ(coord1.X, coord1.Y + 1, coord1.Z), new XYZ(coord2.X, coord2.Y + 1, coord1.Z));
                                                geomPlane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, line.Evaluate(0.5, true));
                                            }
                                            catch (Exception)
                                            {
                                                //TaskDialog.Show("Exception Caught", "Exception: " + ex);
                                            }
                                        }

                                        SketchPlane plane = null;
                                        if (geomPlane != null)
                                            {
                                                plane = SketchPlane.Create(doc, geomPlane);
                                                view3d.SketchPlane = plane;
                                            }
                                        

                                        // Sketch plane cannot be set on Assembly 3D View, so find a default 3D View and set it there instead
                                        //var defaultView = new FilteredElementCollector(doc).OfClass(typeof(View3D)).Where(x => x.Name.Contains("{3D}")).FirstOrDefault();
                                        //if (defaultView != null)
                                        //{
                                        //    View v = defaultView as View;
                                        //    v.SketchPlane = plane;
                                        //}

                                        
                                        if (line != null && references != null)
                                        {
                                            Dimension newDim = doc.Create.NewDimension(view3d, line, references);
                                            dimensionCounter++;
                                        }
                                    }

                                }


                                int pipeTagListCount = pipeTagsList.Count();
                                

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
                                partList.Name = "Bill of materials";
                                ScheduleDefinition partSchedule = partList.Definition;
                                partSchedule.IsItemized = false;
                                

                                //get number of fields added by default to the assembly schedule
                                int fieldQuantity = partSchedule.GetFieldCount();

                                //three fields are added by default to the schedule, being the 3rd one "Count", which is not schedulable
                                //we'll have to add only the first 2 ones (category and type)

                                for (int i = 0; i < (fieldQuantity -1); i++)
                                {
                                    ScheduleFieldId fieldId = partSchedule.GetFieldId(i);
                                    ScheduleField field = partSchedule.GetField(fieldId);
                                    ScheduleSortGroupField sortGroupField = new ScheduleSortGroupField(field.FieldId);
                                    partSchedule.AddSortGroupField(sortGroupField);
                                }

     

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
                                ScheduleSheetInstance.Create(doc, viewSheet.Id, partList.Id, new XYZ(2.01, 1.95, 0));

                                transaction.Commit();


                            }//end of if transaction is committed



             

                        }//end of if assembly allows view creation

                



                    }//end of assembly creation transaction

                }//end of foreach loop through all line numbers


                TaskDialog.Show("Quantifications", $" {lineNumberCounter} line numbers detected \n" +
                                                $" {assemblyCounter} assemblies created \n" +
                                                $" {dimensionCounter} dimensions created for {pipes.Count} pipes in the model \n" +
                                                $" {pipeTagsCounter} pipe tags created for {pipes.Count} pipes in the model \n");



                ElementCategoryFilter pipeTagsFilter = new ElementCategoryFilter(BuiltInCategory.OST_PipeTags);
                IList<Element> pipeTagsCreated = new FilteredElementCollector(doc).WherePasses(pipeTagsFilter).WhereElementIsNotElementType().ToElements();
                TaskDialog.Show("Pipe Tags Created", $"{pipeTagsList.Count()} - {pipeTagsCreated.Count()} pipe tags in the model\n");


                #endregion
            }//end of CanContinue if statement

            else
            {
                TaskDialog.Show("Shutdown", "Please fill all empty line numbers before running the program again");
            }
            return Result.Succeeded;
            
        }//end of Execute method



        private static bool Equals4DigitPrecision(double left, double right)
        {
            return Math.Abs(left - right) < 0.0001;
        }

        //public void AddToNewPipeTagsList(List<IndependentTag> tagList, IndependentTag tag)
        //{
        //    tagList.Add(tag);
        //}

        //public IndependentTag CreateIndependentTag(Document document, View view, Pipe pipe, LocationCurve pipeLocationCurve)
        //{

        //    // define tag mode and tag orientation for new tag
        //    var tagMode = TagMode.TM_ADDBY_CATEGORY;
        //    var tagOrientation = TagOrientation.Horizontal;

        //    // Add the tag to the middle of the pipe
        //    //var pipeStart = pipeLocationCurve.Curve.GetEndPoint(0);
        //    //var pipeEnd = pipeLocationCurve.Curve.GetEndPoint(1);
    
        //    var pipeMid = pipeLocationCurve.Curve.Evaluate(0.5, true);
        //    var pipeRef = new Reference(pipe);

        //    var newTag = IndependentTag.Create(document, view.Id, pipeRef, true, tagMode, tagOrientation, pipeMid);

        //    if (newTag == null)
        //    {
        //        throw new Exception("Create IndependentTag Failed.");
        //    }
        //    else
        //    {
        //        pipeTagsCounter++;
        //    }
        //    //// set leader mode free
        //    //// otherwise leader end point move with elbow point
        //    newTag.LeaderEndCondition = LeaderEndCondition.Free;
        //    var elbowPnt = pipeMid + new XYZ(5.0, 5.0, 0.0);
        //    newTag.LeaderElbow = elbowPnt;
        //    var headerPnt = pipeMid + new XYZ(10.0, 10.0, 0.0);
        //    newTag.TagHeadPosition = headerPnt;

        //    return newTag;
        //}
    }//end of External command class
}
