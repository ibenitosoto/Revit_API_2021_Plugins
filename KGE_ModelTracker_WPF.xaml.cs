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

namespace API_2021_Plugins
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class KGE_ModelTracker_WPF : Window
    {
        public Document document { get; set; }
        public KGE_ModelTracker_WPF(Document doc)
        {
            document = doc;
            InitializeComponent();
            UpdateLengthCounters();
        }

        public void UpdateLengthCounters()
        {
            pipesTextBlock.Text = string.Format("{0:N2}", API_2021_Plugins.ExternalDBApplication.pipeTotalLength);
            traysTextBlock.Text = string.Format("{0:N2}", API_2021_Plugins.ExternalDBApplication.trayTotalLength);
            ductsTextBlock.Text = string.Format("{0:N2}", API_2021_Plugins.ExternalDBApplication.ductTotalLength);
        }
    }
    
}


