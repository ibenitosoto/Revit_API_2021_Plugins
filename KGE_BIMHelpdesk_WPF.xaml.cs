﻿using System;
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
    /// Interaction logic for KGE_BIMHelpdesk.xaml
    /// </summary>
    public partial class KGE_BIMHelpdesk_WPF : Window
    {
        public ExternalCommandData cd;
        public Autodesk.Revit.ApplicationServices.Application app;
        public Document doc;
        public UIDocument uidoc;
        public UIApplication uiapp;

        public static List<Element> views;

        //public KGE_BIMHelpdesk_WPF(Autodesk.Revit.ApplicationServices.Application application, Document document)
        public KGE_BIMHelpdesk_WPF(ExternalCommandData commandData)
        {
            cd = commandData;
            uiapp = commandData.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            app = uiapp.Application;
            doc = uidoc.Document;
            
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
            //Email.SendMessage(GenerateID(), shortText, longText);
            SystemMail.CreateTestMessage(GenerateID(), shortText, longText);
        }

        public string GenerateID()
        {
            return Guid.NewGuid().ToString("N");
        }

        private void dropdownOpenModels_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (Document document in app.Documents)
            {
                dropdownOpenModels.Items.Add(document);
            }
        }


        private void dropdownOpenModels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dropdownViews.Items.Clear();

            Document selectedDocument = (Document)dropdownOpenModels.SelectedItem;

            views = new FilteredElementCollector(selectedDocument).OfClass(typeof(View)).ToList();

            foreach (View view in views)
            {
                dropdownViews.Items.Add(view);
            }
        }

        private void buttonSelectElement_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Element pickedObject = KGE_Scripts.PickObject(cd);
            buttonSelectElement.Content = pickedObject.Category.Name + " element with ID: " + pickedObject.Id;
            this.Show();
        }
    }
}
