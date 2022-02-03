using System;

namespace ObjectStorage.ViewModel
{
    public abstract class ViewObjectViewModelBase : ViewModelBase
    {
        public abstract string Name { get; }
        public abstract string Icon { get; }
    }
}
