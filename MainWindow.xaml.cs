using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CPP_EP
{
    public partial class MainWindow : Window
    {
        private List<MenuItem> menuItems;
        private Dictionary<MenuItem, List<string>> labFiles;
        public MainWindow()
        {
            InitializeComponent();
            menuItems = new List<MenuItem>() { menuItem1, menuItem2, menuItem3, menuItem4, menuItem5, menuItem6, menuItem7, menuItem8 };
            labFiles = new Dictionary<MenuItem, List<string>>
            {
                [menuItem1] = new List<string>() { "lab1.c", "src\\rule.c", "src\\voidtable.c" },
                [menuItem2] = new List<string>() { "lab2.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c" },
                [menuItem3] = new List<string>() { "lab3.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c", "src\\follow.c" },
                [menuItem4] = new List<string>() { "lab4.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c", "src\\follow.c", "src\\parsingtable.c" },
                [menuItem5] = new List<string>() { "lab5.c", "src\\rule.c", "src\\pickupleftfactor.c" },
                [menuItem6] = new List<string>() { "lab6.c", "src\\rule.c", "src\\removeleftrecursion1.c" },
                [menuItem7] = new List<string>() { "lab7.c", "src\\rule.c", "src\\removeleftrecursion2.c" },
                [menuItem8] = new List<string>() { "lab8.c", "src\\rule.c", "src\\voidtable.c", "src\\first.c", "src\\follow.c", "src\\parsingtable.c", "src\\parser.c" }
            };
        }

        private void OpenLabFiles(MenuItem item)
        {
            tabControl.Items.Clear();
            foreach (var file in labFiles[item])
            {
                tabControl.Items.Add(new FileTab("C:\\Users\\User\\CPP-Labs\\" + file));
            }
            tabControl.SelectedIndex = 0;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ((TabItem)tabControl.Items[0]).Visibility = Visibility.Collapsed;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ((TabItem)tabControl.Items[0]).Visibility = Visibility.Visible;
        }

        private void ItemLab_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in menuItems)
            {
                if (sender == item)
                {
                    item.IsChecked = true;
                    OpenLabFiles(item);
                    
                }
                else
                {
                    item.IsChecked = false;
                }
            }
        }

        
    }
}
