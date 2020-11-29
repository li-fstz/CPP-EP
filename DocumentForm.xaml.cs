using System;
using System.Windows.Controls;
using ScintillaNET.WPF;
using System.IO;
using System.ComponentModel;
using ScintillaNET;
using System.Globalization;
using System.Windows;
using System.Diagnostics;
using Xceed.Wpf.AvalonDock.Layout;

namespace CPP_EP
{
    /// <summary>
    /// Interaction logic for DocumentForm.xaml
    /// </summary>
    public partial class DocumentForm : LayoutDocument
    {

        public DocumentForm()
        {
            InitializeComponent ();
            Title = "";
            Scintilla.SavePointLeft += Scintilla_SavePointLeft;
        }

        private void Scintilla_SavePointLeft(object sender, EventArgs e)
        {
            AddOrRemoveAsteric();
        }

        public ScintillaWPF Scintilla
        {
            get { return scintilla; }
        }

        private string _filePath;

        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; }
        }

        private void AddOrRemoveAsteric()
        {
            if (scintilla.Modified)
            {
                if (!Title.EndsWith(" *", StringComparison.InvariantCulture))
                    Title += " *";
            }
            else
            {
                if (Title.EndsWith(" *", StringComparison.InvariantCulture))
                    Title = Title.Substring(0, Title.Length - 2);
            }
        }

    }
}