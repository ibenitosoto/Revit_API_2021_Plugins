using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Plumbing;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.IO;

namespace API_2021_Plugins
{
    class ExternalApplication : IExternalApplication
    {
        //public static ExternalApplication instance = null;

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            //instance = this;

            //instances of images to be used in the app
            //important to have this inside the OnStartup method or they won't be loaded
            //set ribbon icon images as Embedded Resource and Copy if newer and plugin images as Resource and Copy Always
            BitmapSource kirbyIcon = GetEmbeddedImage("API_2021_Plugins.images.kirbyIconK.png");
            BitmapSource kirbyLogo = GetEmbeddedImage("API_2021_Plugins.images.kirbyLogoK.png");
            BitmapSource kirbyFullLogo = GetEmbeddedImage("API_2021_Plugins.images.kirbyFullLogo.png");

            //Create Ribbon Tab
            application.CreateRibbonTab("KGE BIM");

            string path = Assembly.GetExecutingAssembly().Location;

            PushButtonData button1 = new PushButtonData("button1", "Quantifications", path, "API_2021_Plugins.KGE_Quantifications");
            PushButtonData button2 = new PushButtonData("button2", "Isometrics", path, "API_2021_Plugins.KGE_Isometrics");
            PushButtonData button3 = new PushButtonData("button3", "Lines From Pipes", path, "API_2021_Plugins.KGE_LinesFromPipes");
            PushButtonData button4 = new PushButtonData("button4", "Model Tracker", path, "API_2021_Plugins.KGE_ModelTracker");
            PushButtonData button5 = new PushButtonData("button5", "Numbering", path, "API_2021_Plugins.KGE_Numbering");
            PushButtonData button6 = new PushButtonData("button6", "BIM Helpdesk", path, "API_2021_Plugins.KGE_BIMHelpdesk");

            RibbonPanel panel = application.CreateRibbonPanel("KGE BIM", "Kirby Group Engineering Revit Plugins");

            //add button image
            //Uri imagePath = new Uri(@"C:\Users\isoto\OneDrive - Kirby Engineering & Construction\RevitPlugins\API_2021_Plugins\images\kirbyIconK.png");
            //BitmapImage icon = new BitmapImage(imagePath);
            
            PushButton pushButton1 = panel.AddItem(button1) as PushButton;
            PushButton pushButton2 = panel.AddItem(button2) as PushButton;
            PushButton pushButton3 = panel.AddItem(button3) as PushButton;
            PushButton pushButton4 = panel.AddItem(button4) as PushButton;
            PushButton pushButton5 = panel.AddItem(button5) as PushButton;
            PushButton pushButton6 = panel.AddItem(button6) as PushButton;

            
            pushButton1.LargeImage = kirbyIcon;
            pushButton2.LargeImage = kirbyIcon;
            pushButton3.LargeImage = kirbyIcon;
            pushButton4.LargeImage = kirbyIcon;
            pushButton5.LargeImage = kirbyIcon;
            pushButton6.LargeImage = kirbyIcon;

            return Result.Succeeded;
        }

        public void Show_KGE_ModelTracker_WPF(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UI Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            //Get WPF Interface
            KGE_ModelTracker_WPF modelTrackerWPF = new KGE_ModelTracker_WPF(doc);
            modelTrackerWPF.ShowDialog();
        }

        static BitmapSource GetEmbeddedImage(string name)
        {
            try
            {
                Assembly a = Assembly.GetExecutingAssembly();
                Stream s = a.GetManifestResourceStream(name);
                return BitmapFrame.Create(s);
            }
            catch
            {
                return null;
            }
        }


    }
}
