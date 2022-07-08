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
using OutlookAddIn;
using Outlook = Microsoft.Office.Interop.Outlook;
using MailKit;

namespace API_2021_Plugins
{
    /// <summary>
    /// Interaction logic for KGE_BIMHelpdesk.xaml
    /// </summary>
    public partial class KGE_BIMHelpdesk_WPF : Window
    {
        public KGE_BIMHelpdesk_WPF(Document doc)
        {
            InitializeComponent();
        }

     
        public void textBoxShort_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= textBoxShort_GotFocus;
        }

        private void textBoxLong_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= textBoxLong_GotFocus;
        }

        private void buttonSubmit_Click(object sender, RoutedEventArgs e)
        {
            string shortText = textBoxShort.Text;
            string longText = textBoxLong.Text;
      
            //Mail.SendEmailFromAccount(OutlookAddIn.Mail.GetApplicationObject(), shortText, longText, destination, smtpAddress);
            Email.SendMessage(shortText, longText);
        }
    }
}
