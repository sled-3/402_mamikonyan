using ObjectStorage.ViewModel;
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

namespace ObjectStorage.View
{
    /// <summary>
    /// Логика взаимодействия для StorageTableControl1.xaml
    /// </summary>
    public partial class StorageTableControl : UserControl
    {
        public StorageTableControl()
        {
            InitializeComponent();
            DataContext=new StorageTableControlViewModel();
        }


        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Visibility = Visibility.Visible;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Visibility = System.Windows.Visibility.Collapsed;
        }

    }
}
