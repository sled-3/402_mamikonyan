using Microsoft.ML;
using Microsoft.ML.Transforms.Onnx;
using ObjectStorage.Helpers;
using ObjectStorage.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YOLOv4.DataStructures;

namespace ObjectStorage.ViewModel
{
    public class SearchControlViewModel : ViewObjectViewModelBase
    {
        static CancellationTokenSource cancelTokenSource;
        CancellationToken token;
        //MyDbContext context=new MyDbContext();
        public SearchControlViewModel()
        {
        }
        #region Comand
        private ICommand getPathCommand;
        public ICommand GetPathCommand
        {
            get
            {
                if (getPathCommand == null)
                {
                    getPathCommand = new RelayCommand(new Action<object>(getPathOfImages));
                }
                return getPathCommand;
            }
            set
            {
                getPathCommand = value;
                RaisedPropertyChanged("GetPathCommand");
            }
        }
        private ICommand cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                {
                    cancelCommand = new RelayCommand(new Action<object>(cancelProcessing));
                }
                return cancelCommand;
            }
            set
            {
                getPathCommand = value;
                RaisedPropertyChanged("CancelCommand");
            }
        }

        private void cancelProcessing(object obj)
        {
            if (cancelTokenSource != null)
                cancelTokenSource.Cancel();
        }

        private ICommand processingCommand;
        public ICommand ProcessingCommand
        {
            get
            {
                if (processingCommand == null)
                {
                    processingCommand = new RelayCommand(new Action<object>(processingImages));
                }
                return processingCommand;
            }
            set
            {
                processingCommand = value;
                RaisedPropertyChanged("ProcessingCommand");
            }
        }
        private async void processingImages(object obj)
        {
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            int count = 0;
            if (Images.Any())
            {
                try
                {
                    count = await ParallelІSaveImageAsync(Images.ToList(), token);
                    ObjectStorageHelper.SimpleAlert("Добавление изображений", $"Добавлено {count} новых изображений");
                    //MessageBox.Show($"Добавлено {count} новых изображений");
                }
                catch (Exception ex)
                {
                    //;
                }
            }
        }


        private ICommand clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (clearCommand == null)
                {
                    clearCommand = new RelayCommand(new Action<object>(clearImages));
                }
                return clearCommand;
            }
            set
            {
                clearCommand = value;
                RaisedPropertyChanged("ClearCommand");
            }
        }

        private void clearImages(object obj) => Images.Clear();

        private ICommand loadhCommand;
        public ICommand LoadCommand
        {
            get
            {
                if (loadhCommand == null)
                {
                    loadhCommand = new RelayCommand(new Action<object>(loadImages));
                }
                return loadhCommand;
            }
            set
            {
                loadhCommand = value;
                RaisedPropertyChanged("LoadCommand");
            }
        }

        private void loadImages(object obj)
        {
            if (string.IsNullOrEmpty(Path)) return;
            try
            {
                DirectoryInfo folder = new DirectoryInfo(Path);
                if (folder.Exists)
                {
                    foreach (var fileinfo in folder.GetFiles())
                    {
                        if (".jpg|.jpeg|.png".Contains(fileinfo.Extension.ToLower()))
                            addImage(fileinfo.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void addImage(string imageWithPath)
        {
            Bitmap bitmap = new Bitmap(System.Drawing.Image.FromFile(imageWithPath));
            var source = bitmap.ToWpfBitmap();
            Images.Add(new ImageModel { Id = GetNextImageModelId, ImageSource = source, ImagePath = imageWithPath });
        }

        private void getPathOfImages(object obj)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                FileName = "Выбор папки",
                Filter = "Файлы изображений|*.bmp;*.png;*.jpg",
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                Title = "Выбор папки изображений"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var directory = new DirectoryInfo(System.IO.Path.GetDirectoryName(ofd.FileName));
            Path = directory.FullName;
        }
        #endregion
        private async Task<int> ProcessingBitmapAsync(ImageSource source, CancellationToken token)
        {
            MLContext mlContext = new MLContext();
            Microsoft.ML.Data.EstimatorChain<OnnxTransformer> pipeline = Helper.GetPipeline(mlContext);
            var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));
            var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);
            Bitmap bitmap = ((BitmapSource)source).ToWinFormsBitmap();
            var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
            IReadOnlyList<YoloV4Result> results = await predict.GetResultsAsync(Helper.ClassesNames, 0.3f, 0.7f, token);
            var context = new MyDbContext();
            var objs = context.YoloObjects;
            var fotos = context.Fotos;
            bool inDb = true;
            List<YoloObject> objects = new List<YoloObject>();
            int ret = 0;
            foreach (var res in results)
            {
                var obj = new YoloObject
                {
                    X1 = res.BBox[0],
                    Y1 = res.BBox[1],
                    X2 = res.BBox[2],
                    Y2 = res.BBox[3],
                    ClassName = res.Label,
                    Confidence = res.Confidence,
                };
                Rectangle rectangle = new Rectangle((int)obj.X1, (int)obj.Y1, (int)(obj.X2 - obj.X1), (int)(obj.Y2 - obj.Y1));
                var image = bitmap.CropImage(rectangle);
                obj.Image = image.ImageToByte();
                obj.Hash = image.GetImageHash();
                if (inDb)
                    inDb = objs.FirstOrDefault(x => x.Hash.Equals(obj.Hash)) != null;
                objects.Add(obj);
            }
            if (inDb)
            {
                var hash = bitmap.GetImageHash();
                inDb = fotos.FirstOrDefault(x => x.Hash.Equals(hash)) != null;
            }
            if (!inDb)
            {
                var foto = new Foto()
                {
                    Image = bitmap.ImageToByte(),
                    Hash = bitmap.GetImageHash(),
                    Name = $"image_{DateTime.Now.Ticks.GetHashCode():x}"
                };
                fotos.Add(foto);
                context.SaveChanges();
                foreach (var obj in objects)
                {
                    obj.FotoId = foto.Id;
                    objs.Add(obj);
                }
                await context.SaveChangesAsync().ConfigureAwait(false);
                ret++;
            }
            return ret;
        }
        private async Task<int> ParallelІSaveImageAsync(List<ImageModel> images, CancellationToken token)
        {
            int count = 0;
            await Task.Run(() =>
            {
                try
                {
                    Parallel.ForEach(images, (image) =>
                   {
                       token.ThrowIfCancellationRequested();
                       if (ProcessingBitmap(image.ImageSource, token) > 0)
                           count++;
                   });
                }
                catch (Exception ex)
                {
                    ;
                }
            });
            return count;
        }

        private int ProcessingBitmap(ImageSource source, CancellationToken token)
        {
            MLContext mlContext = new MLContext();
            Microsoft.ML.Data.EstimatorChain<OnnxTransformer> pipeline = Helper.GetPipeline(mlContext);
            var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));
            var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);
            Bitmap bitmap = ((BitmapSource)source).ToWinFormsBitmap();
            var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
            IReadOnlyList<YoloV4Result> results = predict.GetResults(Helper.ClassesNames, 0.3f, 0.7f);
            var context = new MyDbContext();
            var objs = context.YoloObjects;
            var fotos = context.Fotos;
            bool inDb = true;
            List<YoloObject> objects = new List<YoloObject>();
            int ret = 0;
            foreach (var res in results)
            {
                var obj = new YoloObject
                {
                    X1 = res.BBox[0],
                    Y1 = res.BBox[1],
                    X2 = res.BBox[2],
                    Y2 = res.BBox[3],
                    ClassName = res.Label,
                    Confidence = res.Confidence,
                };
                Rectangle rectangle = new Rectangle((int)obj.X1, (int)obj.Y1, (int)(obj.X2 - obj.X1), (int)(obj.Y2 - obj.Y1));
                var image = bitmap.CropImage(rectangle);
                obj.Image = image.ImageToByte();
                obj.Hash = image.GetImageHash();
                if (inDb)
                    inDb = objs.FirstOrDefault(x => x.Hash.Equals(obj.Hash)) != null;
                objects.Add(obj);
            }
            if (inDb)
            {
                var hash = bitmap.GetImageHash();
                inDb = fotos.FirstOrDefault(x => x.Hash.Equals(hash)) != null;
            }
            if (!inDb)
            {
                var foto = new Foto()
                {
                    Image = bitmap.ImageToByte(),
                    Hash = bitmap.GetImageHash(),
                    Name = $"image_{DateTime.Now.Ticks.GetHashCode():x}"
                };
                fotos.Add(foto);
                context.SaveChanges();
                foreach (var obj in objects)
                {
                    obj.FotoId = foto.Id;
                    objs.Add(obj);
                }
                context.SaveChanges();
                ret++;
            }
            return ret;
        }

        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                RaisedPropertyChanged(nameof(Path));
            }
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
        public int GetNextImageModelId => (Images.Any() ? Images.Max(x => x.Id) : 0) + 1;

        public override string Name
        {
            get { return ObjectStorageHelper.Objects; }
        }
        public override string Icon
        {
            get { return ObjectStorageHelper.ObjectsIcon; }
        }
    }
}
