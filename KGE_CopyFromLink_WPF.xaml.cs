using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using API_2021_Plugins;
using TextBox = System.Windows.Controls.TextBox;

namespace API_2021_Plugins
{
    /// <summary>
    /// Interaction logic for KGE_CopyFromLink_WPF.xaml
    /// </summary>
    public partial class KGE_CopyFromLink_WPF : Window
    {
        public ExternalCommandData cd;
        public Autodesk.Revit.ApplicationServices.Application app;
        public Document doc;
        public UIDocument uidoc;
        public UIApplication uiapp;

        public Document selectedLink;
        public ElementMulticategoryFilter allElementsFilter;


        public KGE_CopyFromLink_WPF(ExternalCommandData commandData)
        {
            cd = commandData;
            uiapp = commandData.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            app = uiapp.Application;
            doc = uidoc.Document;

            InitializeComponent();
        }


        private void dropdownLinksLoaded_Loaded(object sender, RoutedEventArgs e)
        {
            dropdownLinksLoaded.ItemsSource = KGE_CopyFromLink.GetLoadedLinks(doc);
        }

        private void dropdownLinksLoaded_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedLink = dropdownLinksLoaded.SelectedItem as Document;
        }

        private void buttonGenericModels_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_GenericModel);
        }

        private void buttonMechanicalEquipment_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_MechanicalEquipment);
        }

        private void buttonPlumbinglFixtures_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_PlumbingFixtures);
        }

        private void buttonPipes_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_PipeCurves);
        }

        private void buttonPipeFittings_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_PipeFitting);
        }

        private void buttonPipeAccesories_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_PipeAccessory);
        }

        private void buttonDucts_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_DuctCurves);
        }

        private void buttonDuctFittings_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_DuctFitting);
        }

        private void buttonDuctAccesories_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_DuctAccessory);
        }

        private void buttonFlexDucts_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_FlexDuctCurves);
        }

        private void buttonAirTerminals_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_DuctTerminal);
        }

        private void buttonSprinklers_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_Sprinklers);
        }

        private void buttonElectricalEquipment_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_ElectricalEquipment);
        }

        private void buttonElectricalFixtures_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_ElectricalFixtures);
        }

        private void buttonTrays_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_CableTray);
        }

        private void buttonTrayFittings_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_CableTrayFitting);
        }

        private void buttonConduits_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_Conduit);
        }

        private void buttonConduitFittings_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_ConduitFitting);
        }

        private void buttonLightingDevices_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_LightingDevices);
        }

        private void buttonLightingFixtures_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_LightingFixtures);
        }

        private void buttonFireAlarmDevices_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_FireAlarmDevices);
        }

        private void buttonDataDevices_Checked(object sender, RoutedEventArgs e)
        {
            KGE_CopyFromLink.allCategories.Add(BuiltInCategory.OST_DataDevices);
        }

        private void buttonExecute_Click(object sender, RoutedEventArgs e)
        {
            //Creating the multicategory filter with selected categories
            KGE_CopyFromLink.allCategoriesFilter = new ElementMulticategoryFilter(KGE_CopyFromLink.allCategories);


            //Get element IDs for those elements to be copied
            ICollection<ElementId> ids = KGE_CopyFromLink.GetElementIds(selectedLink);

            //Finally copy those elements
            KGE_CopyFromLink.CopyElements(ids, doc, selectedLink);
        }


    }
}

