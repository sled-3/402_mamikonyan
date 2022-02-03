using Microsoft.ML;
using Microsoft.ML.Transforms.Onnx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YOLOv4.DataStructures;

namespace ObjectSearch
{
    internal static class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            var cancelTokenSource = new CancellationTokenSource();
            var token = cancelTokenSource.Token;
            List<string> files;
            using (OpenFileDialog ofd = new OpenFileDialog()
            {
                FileName = "Выбор папки",
                Filter = "Файлы изображений|*.bmp;*.png;*.jpg",
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                Title = "Выбор папки изображений"
            })
            {
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;
                var directory = new DirectoryInfo(Path.GetDirectoryName(ofd.FileName));
                var masks = new[] { "*.bmp", "*.png", "*.jpg" };
                files = masks.SelectMany(directory.EnumerateFiles)
                    .Select(x => Path.Combine(directory.FullName, x.Name))
                    .ToList();
            }
            if (files.Any())
            {
                var t = Task.Run(
                    () =>
                    {
                        Parallel.ForEach(
                            files,
                            async f =>
                            {
                                if (token.IsCancellationRequested)
                                    return;
                                var list = await ProcessingFileAsync(f);
                                var imageName = Path.GetFileName(f);

                                foreach (var r in list)
                                {
                                    Console.WriteLine(
                                        $"{imageName}- класс: { r.Label} ( {r.BBox[0]};{r.BBox[1]}|{r.BBox[2]};{r.BBox[3]};)");
                                }
                            });
                    },
                    token);
                try
                {
                    Console.WriteLine("Введите Y для отмены операции или любой другой символ для ее продолжения:");
                    var s = Console.ReadKey(true);
                    if (s.Key == ConsoleKey.Y)
                    {
                        cancelTokenSource.Cancel();
                    }
                    else
                    {
                        t.Wait();
                        Console.WriteLine($"Обработано { files.Count} файла.");
                    }
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine("Операция прервана");
                }
                catch (AggregateException e)
                {
                    Console.WriteLine("Exception messages:");
                    foreach (var ie in e.InnerExceptions)
                        Console.WriteLine($"   {ie.GetType().Name}: {ie.Message}");

                    Console.WriteLine("\nTask status: {0}", t.Status);
                }
                finally
                {
                    cancelTokenSource.Dispose();
                }
            }
            else
                Console.WriteLine("Файлы с изображениями в выбраной папке отсуствуют");
            Console.WriteLine("Для продолжения нажмите любую клавишу ...");
            Console.ReadKey();
        }

        private static async Task<List<YoloV4Result>> ProcessingFileAsync(string file)
        {
            MLContext mlContext = new MLContext();
            var res = new List<YoloV4Result>();
            Microsoft.ML.Data.EstimatorChain<OnnxTransformer> pipeline = Helper.GetPipeline(mlContext);
            var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));
            var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);
            using (var bitmap = new Bitmap(Image.FromFile(file)))
            {
                var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                IReadOnlyList<YoloV4Result> results =await predict.GetResultsAsync(Helper.ClassesNames, 0.3f, 0.7f);
                res.AddRange(results);
            }
            return res;
        }
    }
}