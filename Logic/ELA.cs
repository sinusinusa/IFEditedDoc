using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace IFEditedDoc.Logic;

public class ELA : IClue
{
  public double maxDifferenceThreshold = 15;
  private Mat? formFileToMat(IFormFile _image)
  {
    try
    {
      using (var stream = _image.OpenReadStream())
      {
        var bitmap = new Bitmap(stream);
        Mat mat = bitmap.ToMat();
        return mat;
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Не удается преобразовать изображение: " + ex.Message);
      return null;
    }
  }
  public Evidence getCheck(Document doc)
  {
    bool check = false;
    string message = "ELA is not failed";
    try
    {
      using (var image = formFileToMat(doc.image))
      {
        // Сохраняем изображение с сжатием во временный файл
        CvInvoke.Imwrite("temp.jpg", image, new KeyValuePair<ImwriteFlags, int>(ImwriteFlags.JpegQuality, 90));
        using (var resavedImage = CvInvoke.Imread("temp.jpg", ImreadModes.AnyColor))
        {
          using (var elaImage = new Mat())
          {
            CvInvoke.AbsDiff(image, resavedImage, elaImage);

            // Конвертируем ELA изображение в оттенки серого (grayscale)
            CvInvoke.CvtColor(elaImage, elaImage, ColorConversion.Bgr2Gray);

            double minVal = 0, maxVal = 0;
            Point minLoc = new Point(), maxLoc = new Point();

            CvInvoke.MinMaxLoc(elaImage, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

            double maxDifference = maxVal;
            Console.WriteLine($"Максимальная разница: {maxDifference}");
            // Усиливаем контраст ELA изображения
            CvInvoke.Normalize(elaImage, elaImage, 0, 255, NormType.MinMax, DepthType.Cv8U);
            CvInvoke.Imshow("ELA", elaImage);
            CvInvoke.WaitKey(0);
          }
        }
      }
      
    }
    catch (Exception ex)
    {
      Console.WriteLine("Не удается обработать изображение: " + ex.Message);
    }
    return new Evidence(check, message);
  }
}