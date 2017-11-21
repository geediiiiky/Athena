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
using System.Windows.Shapes;

namespace Athena
{
    public delegate void ParameterModifiedDel(List<int> selectedHashes);

    /// <summary>
    /// Interaction logic for ParameterWindow.xaml
    /// </summary>
    public partial class ParameterWindow : Window
    {
        private RunParametersConfig sharedConfig;
        private RunParametersConfig userConfig;
        public event ParameterModifiedDel ParameterModified;

        public ParameterWindow(RunParametersConfig shared, RunParametersConfig user, List<int> selectedHashes)
        {
            sharedConfig = shared;
            userConfig = user;

            InitializeComponent();
            foreach (var entry in sharedConfig.Parameters)
            {
                var hash = entry.Param.GetHashCode();
                AddEntry(entry.instance.ToString(), entry.Param, hash != 0 && selectedHashes.Contains(hash), false);
            }

            foreach (var entry in userConfig.Parameters)
            {
                var hash = entry.Param.GetHashCode();
                AddEntry(entry.instance.ToString(), entry.Param, hash!= 0 && selectedHashes.Contains(hash), true);
            }
        }

        private void AddEntry(string instanceText, string text, bool selected, bool canBeModified)
        {
            var newEntry = new DockPanel();

            var select = new CheckBox();
            select.VerticalAlignment = VerticalAlignment.Center;
            select.IsChecked = selected;
            select.Checked += Select_Checked;
            select.Unchecked += Select_Unchecked;
            select.Width = 50;
            newEntry.Children.Add(select);

            var removeButton = new Button();
            removeButton.Content = " - ";
            removeButton.VerticalAlignment = VerticalAlignment.Center;
            removeButton.Click += RemoveButton_Click;
            DockPanel.SetDock(removeButton, Dock.Right);
            newEntry.Children.Add(removeButton);
            if (!canBeModified)
            {
                removeButton.Visibility = Visibility.Hidden;
            }

            var instance = new TextBox();
            instance.Tag = "InstanceBox";
            instance.VerticalAlignment = VerticalAlignment.Center;
            instance.Margin = new Thickness(0, 0, 30, 0);
            instance.Text = instanceText;
            instance.IsReadOnly = !canBeModified;
            instance.Width = 20;
            newEntry.Children.Add(instance);

            var textBox = new TextBox();
            textBox.Tag = "ParameterBox";
            textBox.VerticalAlignment = VerticalAlignment.Center;
            textBox.Margin = new Thickness(5, 0, 30, 0);
            textBox.Text = text;
            textBox.IsReadOnly = !canBeModified;
            newEntry.Children.Add(textBox);

            newEntry.Opacity = selected ? 1.0 : 0.5;
            ParametersContainer.Children.Add(newEntry);
        }

        private void Select_Unchecked(object sender, RoutedEventArgs e)
        {
            var selectButton = sender as CheckBox;
            if (selectButton != null)
            {
                var selected = selectButton.Parent as DockPanel;
                selected.Opacity = 0.5;
            }
        }

        private void AddParam_Click(object sender, RoutedEventArgs e)
        {
            AddEntry("", "", false, true);
        }

        private void Select_Checked(object sender, RoutedEventArgs e)
        {
            var selectButton = sender as CheckBox;
            if (selectButton != null)
            {
                var selected = selectButton.Parent as DockPanel;
                selected.Opacity = 1.0;

                //foreach (var child in ParametersContainer.Children)
                //{
                //    var unselected = child as DockPanel;
                //    if (unselected != null && unselected != selected)
                //    {
                //        unselected.Opacity = 0.5;
                //        foreach (var element in unselected.Children)
                //        {
                //            var checkbox = element as CheckBox;
                //            if (checkbox != null)
                //            {
                //                checkbox.IsChecked = false;
                //                break;
                //            }
                //        }
                //    }
                //}
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var panel = button.Parent as UIElement;
                ParametersContainer.Children.Remove(panel);
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Apply_Click(sender, e);
            this.Close();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            userConfig.Parameters.Clear();
            List<int> selectedHashes = new List<int>();

            foreach (var child in ParametersContainer.Children)
            {
                var panel = child as DockPanel;
                if (panel != null && panel.IsEnabled)
                {
                    string text = "";
                    int inst = -1;
                    bool isSelected = false;
                    bool shouldBeSaved = false;
                    foreach (var element in panel.Children)
                    {
                        var textbox = element as TextBox;
                        if (textbox != null)
                        {
                            if ("ParameterBox".Equals(textbox.Tag))
                            {
                                text = textbox.Text;
                            }
                            else if ("InstanceBox".Equals(textbox.Tag))
                            {
                                Int32.TryParse(textbox.Text, out inst);
                            }
                            shouldBeSaved = !textbox.IsReadOnly;
                            continue;
                        }
                        var checkbox = element as CheckBox;
                        if (checkbox != null)
                        {
                            isSelected = checkbox.IsChecked == true;
                        }
                    }

                    if (shouldBeSaved)
                    {
                        userConfig.Parameters.Add(new RunParametersConfig.RunParameter
                        {
                            instance = inst,
                            Param = text
                        });
                    }
                    if (isSelected)
                    {
                        selectedHashes.Add(text.GetHashCode());
                    }
                }
            }

            ParameterModified(selectedHashes);
        }
    }
}

