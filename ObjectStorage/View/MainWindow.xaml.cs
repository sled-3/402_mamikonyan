using ObjectStorage.Helpers;
using ObjectStorage.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ObjectStorage.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();

            ListBoxStorageObjects.SelectedIndex = 0;
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CommandBinding_CanExecute_1(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MiniMize_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }
        public void Drag_Window(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ListBoxObjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxObjects.SelectedIndex != -1)
            {
                pnlcontent.Content = ListBoxObjects.SelectedItem;
                ListBoxStorageObjects.SelectedIndex = -1;
            }
        }

        private void ListBoxStorageObjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxStorageObjects.SelectedIndex != -1)
            {
                pnlcontent.Content = ListBoxStorageObjects.SelectedItem;
                ListBoxObjects.SelectedIndex = -1;
            }
        }
        protected override void OnClosed(System.EventArgs e)
        {
            ObjectStorageHelper.growlNotifications.Close();
            base.OnClosed(e);
        }
    }
}
