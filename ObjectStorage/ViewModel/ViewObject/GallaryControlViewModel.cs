using ObjectStorage.Helpers;
using ObjectStorage.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ObjectStorage.ViewModel
{
    public class GallaryControlViewModel : ViewObjectViewModelBase
    {
        MyDbContext context=new MyDbContext();
        public GallaryControlViewModel()
        {
            fillImages();
        }
        #region Comand
        private ICommand saveNameCommand;
        public ICommand SaveNameCommand
        {
            get
            {
                if (saveNameCommand == null)
                {
                    saveNameCommand = new RelayCommand(new Action<object>(saveNewName));
                }
                return saveNameCommand;
            }
            set
            {
                saveNameCommand = value;
                RaisedPropertyChanged("SaveNameCommand");
            }
        }

        private void saveNewName(object obj)
        {
            if (SelectedImage != null && obj is TextBox box)
            {
                var foto = context.Fotos.Find(SelectedImage.Id);
                if (foto != null)
                {
                    foto.Name = box.Text;
                    context.SaveChanges();
                    SelectedImage.ImagePath = foto.Name;
                    box.Text = "";
                }
            }
        }

        private ICommand deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new RelayCommand(new Action<object>(deleteImages));
                }
                return deleteCommand;
            }
            set
            {
                deleteCommand = value;
                RaisedPropertyChanged("DeleteCommand");
            }
        }
        private void deleteImages(object obj)
        {
            if (SelectedImage == null) return;
            var foto = context.Fotos.Find(SelectedImage.Id);
            if (foto != null)
            {
                context.Fotos.Remove(foto);
                context.SaveChanges();
                SelectedObject = null;
                SelectedImage = null;
                fillImages();
            }
        }

        #endregion

        private void fillImages()
        {
            Images.Clear();
            context.Fotos.ToList().ForEach(x =>
            {
                var image = new ImageModel
                {
                    Id = x.Id,
                    ImageSource = x.Image.ToWpfBitmap(),
                    ImagePath = x.Name
                };
                Images.Add(image);
            });
        }

        public override string Name
        {
            get { return ObjectStorageHelper.Gallary; }
        }
        public override string Icon
        {
            get { return ObjectStorageHelper.GallaryIcon; }
        }
        ObservableCollection<ImageModel> images = new ObservableCollection<ImageModel>();
        public ObservableCollection<ImageModel> Images
        {
            get => images;
            set
            {
                if (images == value)
                {
                    return;
                }

                images = value;
                RaisedPropertyChanged(nameof(Images));
            }
        }

        ObservableCollection<YoloObject> yoloObjects = new ObservableCollection<YoloObject>();
        public ObservableCollection<YoloObject> YoloObjects
        {
            get => yoloObjects;
            set
            {
                if (yoloObjects == value)
                {
                    return;
                }

                yoloObjects = value;
                RaisedPropertyChanged(nameof(YoloObjects));
            }
        }
        YoloObject selectedObject;
        public YoloObject SelectedObject
        {
            get => selectedObject;
            set
            {
                if (selectedObject == value)
                {
                    return;
                }

                selectedObject = value;
                RaisedPropertyChanged(nameof(SelectedObject));
            }
        }


        ImageModel selectedImage;
        public ImageModel SelectedImage
        {
            get => selectedImage;
            set
            {
                if (selectedImage == value)
                {
                    return;
                }
                selectedImage = value;
                fillYoloObjects();
                RaisedPropertyChanged(nameof(SelectedImage));
            }
        }

        private void fillYoloObjects()
        {
            YoloObjects.Clear();
            if (SelectedImage == null) return;
            var foto = context.Fotos.ToList().FirstOrDefault(x => x.Id == selectedImage.Id);
            if (foto != null)
                foto.YoloObject.ToList().ForEach(x => YoloObjects.Add(x));
        }
    }
}
