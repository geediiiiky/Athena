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
    /// <summary>
    /// Interaction logic for GitResetOptions.xaml
    /// </summary>
    public partial class GitResetOptions : Window
    {
        private MainWindow mainWindow;

        public GitResetOptions(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
        }

        private void OnlyReset_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Are you sure you want to reset all modified files?", "reset modified files", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No))
            {
                mainWindow.ExecuteCommand("ResetGit.bat");
            }
        }

        private void ResetAndCleanUntracked_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Are you sure you want to reset all modified files and remove untracked files (files ignored by git WILL NOT be deleted, ie. build caches)?", "reset and clean", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No))
            {
                mainWindow.ExecuteCommand("ResetAndCleanUntracked.bat");
            }
        }

        private void ResetAndCleanAll_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("This will clean your whole repo, including deleting the cached files. Are your sure?", "Reset and clean all", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    mainWindow.ExecuteCommand("ResetAndCleanAll.bat");
                    break;
            }
        }
    }
}
