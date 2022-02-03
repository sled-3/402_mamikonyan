using ObjectStorage.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectStorage.ViewModel
{
    public class StorageTableControlViewModel : GallaryControlViewModel
    {
        public StorageTableControlViewModel()
        {

        }
        public override string Name
        {
            get { return ObjectStorageHelper.StorageTable; }
        }
        public override string Icon
        {
            get { return ObjectStorageHelper.StorageTableIcon; }
        }
    }
}
