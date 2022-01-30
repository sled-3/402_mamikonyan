using Microsoft.ML;
using Microsoft.ML.Transforms.Onnx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfObjectSearch.Model;
using WpObjectSearch;
using YOLOv4.DataStructures;
using static WpfObjectSearch.BitmapConversion;

namespace WpfObjectSearch.ViewModel
{
    public class MainViewModel:ViewModelBase
    {
        static CancellationTokenSource cancelTokenSource;
        CancellationToken token;

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
            if(cancelTokenSource!=null)
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

        private void processingImages(object obj)
        {
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            var t = Task.Run(
                () =>
                {
                    Parallel.ForEach(
                        Images,
                        async f =>
                        {
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                f.ImageSource = await ProcessingBitmapAsync(f.ImageSource, token);
                            }
                            catch (OperationCanceledException ex)
                            {
                                if (cancelTokenSource != null)
                                    MessageBox.Show("Операция прервана");
                            }
                            catch (AggregateException e)
                            {
                                StringBuilder builder=new StringBuilder();
                                builder.AppendLine("Сообщение об ошибках:");
                                foreach (var ie in e.InnerExceptions)
                                    builder.AppendLine($"   {ie.GetType().Name}: {ie.Message}");
                                MessageBox.Show(builder.ToString());
                            }
                            finally
                            {
                                if (cancelTokenSource != null)
                                {
                                    cancelTokenSource.Dispose();
                                    cancelTokenSource = null;
                                }
                            }
                        });
                },
                token);
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
            Images.Add(new ImageModel { Id = GetNextImageModelId, ImageSource = source,ImagePath=imageWithPath });
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
        private async Task<ImageSource> ProcessingBitmapAsync(ImageSource source, CancellationToken token)
        {
            MLContext mlContext = new MLContext();
            Microsoft.ML.Data.EstimatorChain<OnnxTransformer> pipeline = Helper.GetPipeline(mlContext);
            var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));
            var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);
            Bitmap bitmap=((BitmapSource)source).ToWinFormsBitmap();
            var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
            IReadOnlyList<YoloV4Result> results =await predict.GetResultsAsync(Helper.ClassesNames, 0.3f, 0.7f,token);
            using (var g = Graphics.FromImage(bitmap))
            {
                foreach (var res in results)
                {
                    // draw predictions
                    var x1 = res.BBox[0];
                    var y1 = res.BBox[1];
                    var x2 = res.BBox[2];
                    var y2 = res.BBox[3];
                    g.DrawRectangle(Pens.Red, x1, y1, x2 - x1, y2 - y1);
                    using (var brushes = new SolidBrush(System.Drawing.Color.FromArgb(50, System.Drawing.Color.Red)))
                    {
                        g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                    }

                    g.DrawString(res.Label + " " + res.Confidence.ToString("0.00"),
                                 new Font("Arial", 12), System.Drawing.Brushes.Blue, new PointF(x1, y1));
                }
            }
            return bitmap.ToWpfBitmap();
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
        ObservableCollection<ImageModel> images=new ObservableCollection<ImageModel>();
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
        public int GetNextImageModelId => (Images.Any()? Images.Max(x => x.Id): 0) + 1;
    }
}
