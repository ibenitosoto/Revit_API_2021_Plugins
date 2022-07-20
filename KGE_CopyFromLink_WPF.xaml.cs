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

namespace API_2021_Plugins
{
    /// <summary>
    /// Interaction logic for KGE_CopyFromLink_WPF.xaml
    /// </summary>
    public partial class KGE_CopyFromLink_WPF : Window
    {
        public KGE_CopyFromLink_WPF()
        {
            InitializeComponent();
        }

        private void dropdownOpenModels_Loaded(object sender, RoutedEventArgs e)
        {
            //collect all loaded links inside the active document
        }
    }
}
