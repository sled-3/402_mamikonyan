using System.ComponentModel;
using System.Windows.Media;

namespace ObjectStorage.Model
{
    public class ImageModel: INotifyPropertyChanged
    {
        public int Id { get; set; }
        ImageSource imageSource;
        public ImageSource ImageSource
        {
            get => imageSource;
            set
            {
                if (imageSource == value)
                {
                    return;
                }

                imageSource = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageSource)));
            }
        }
        string imagePath;
        public string ImagePath
        {
            get => imagePath;
            set
            {
                if (imagePath == value)
                {
                    return;
                }

                imagePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImagePath)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
