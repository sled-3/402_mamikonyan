using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ObjectStorage.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private ObservableCollection<ViewObjectViewModelBase> objectMenu;
        private ObservableCollection<ViewObjectViewModelBase> storageMenu;
        ObservableCollection<ViewObjectViewModelBase> tempObjectMenu =
            new ObservableCollection<ViewObjectViewModelBase>();
        ObservableCollection<ViewObjectViewModelBase> tempStorageMenu =
            new ObservableCollection<ViewObjectViewModelBase>();

        public ObservableCollection<ViewObjectViewModelBase> ObjectMenu
        {
            get { return this.objectMenu; }
            set
            {
                objectMenu = value;
                RaisedPropertyChanged("ObjectMenu");
            }
        }
        public ObservableCollection<ViewObjectViewModelBase> StorageMenu
        {
            get { return this.storageMenu; }
            set
            {
                storageMenu = value;
                RaisedPropertyChanged("StorageMenu");
            }
        }

        public MainWindowViewModel()
        {
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            #region Storage Menu
            tempStorageMenu.Add(new StorageTableControlViewModel());
            tempStorageMenu.Add(new GallaryControlViewModel());

            #endregion

            /////////////////////////////////////////////////////////////////////
            #region Object Menu

            tempObjectMenu.Add(new SearchControlViewModel());
            //if (IsAdmin)
            //{
            //    tempInventoryMenu.Add(new AddInvertoryViewModel());
            //    tempInventoryMenu.Add(new UpdateInvertoryViewModel());
            //}
            //tempInventoryMenu.Add(new InvertiryHistoryViewModel());
            //tempInventoryMenu.Add(new SellInvertoryWriteOffViewModel());
            #endregion
            //////////////////////////////////////////////////////////////////////
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ObjectMenu = tempObjectMenu;
            StorageMenu = tempStorageMenu;
        }

    }
}
