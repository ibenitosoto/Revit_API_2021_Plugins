using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
using API_2021_Plugins;
using TextBox = System.Windows.Controls.TextBox;

namespace API_2021_Plugins
{
    /// <summary>
    /// Interaction logic for KGE_CreateModels_WPF.xaml
    /// </summary>
    public partial class KGE_CreateModels_WPF : Window
    {   
        public ExternalCommandData cd;
        public Autodesk.Revit.ApplicationServices.Application app;
        public Document doc;
        public UIDocument uidoc;
        public UIApplication uiapp;

        public KGE_CreateModels_WPF(ExternalCommandData commandData)
        {
            cd = commandData;
            uiapp = commandData.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            app = uiapp.Application;
            doc = uidoc.Document;

            InitializeComponent();
        }

        private void buttonExecute_Copy_Click(object sender, RoutedEventArgs e)
        {
            string modelNamesTextBox = textBoxModelNames.Text;
            string[] modelNamesList = modelNamesTextBox.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            //foreach (string modelName in modelNamesList)
            //{
            //    if (modelName.Contains("/n"))
            //    {
            //        modelName.Replace("/n", "");
            //    }
            //}

            KGE_CreateModels.CreateModels(textBoxRevitTemplate.Text, textBoxWindowsFolder.Text, modelNamesList);
        }

        private void textBoxRevitTemplate_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.FontWeight = FontWeights.Regular;
            tb.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFA0A6C6");
        }

        private void textBoxWindowsFolder_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.FontWeight = FontWeights.Regular;
            tb.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFA0A6C6");
        }

        private void textBoxModelNames_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.FontWeight = FontWeights.Regular;
            tb.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFA0A6C6");

        }
        private void textBoxRevitTemplate_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= textBoxRevitTemplate_GotFocus;
        }

        private void textBoxWindowsFolder_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= textBoxWindowsFolder_GotFocus;
        }
        private void textBoxModelNames_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= textBoxModelNames_GotFocus;
        }

        private void textBoxRevitTemplate_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxRevitTemplate.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF0E1D79");
            textBoxRevitTemplate.FontWeight = FontWeights.Bold;
        }
        private void textBoxWindowsFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxWindowsFolder.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF0E1D79");
            textBoxWindowsFolder.FontWeight = FontWeights.Bold;
        }
        private void textBoxModelNames_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBoxModelNames.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF0E1D79");
            textBoxModelNames.FontWeight = FontWeights.Bold;
        }

   
    }
}
