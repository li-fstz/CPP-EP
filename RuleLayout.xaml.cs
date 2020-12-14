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
using System.Collections.Generic;
using CPP_EP.Lab;
using System.Windows.Documents;

namespace CPP_EP
{
    /// <summary>
    /// Interaction logic for DocumentForm.xaml
    /// </summary>
    public partial class RuleLayout: LayoutAnchorable {

        public RuleLayout() {
            InitializeComponent ();
        }
        public void Draw(List<Rule> rules) {
            TextBlock tb = new TextBlock();
            tb.Margin = new Thickness (10);
            foreach (var rule in rules) {
                tb.Inlines.Add (rule.Name + " => ");
                bool f = true;
                foreach (var select in rule.Selects) {
                    if (f) {
                        f = false;
                    } else {
                        tb.Inlines.Add ("|");
                    }
                    foreach (var symbol in select.Symbols) {
                        tb.Inlines.Add (symbol.Name);
                    }
                }
                tb.Inlines.Add (new LineBreak ());
            }
            Content = tb;
        }
    }
}