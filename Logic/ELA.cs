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
        CvInvoke.Imwrite("temp.jpg", image, new KeyValuePair<ImwriteFlags, int>(ImwriteFlags.JpegQuality, 95));
        using (var resavedImage = CvInvoke.Imread("temp.jpg", ImreadModes.AnyColor))
        {
          using (var elaImage = new Mat())
          {
            CvInvoke.AbsDiff(image, resavedImage, elaImage);

            double minVal, maxVal;
            int[] minIdx = new int[2];
            int[] maxIdx = new int[2];

            CvInvoke.MinMaxIdx(elaImage, out minVal, out maxVal, minIdx, maxIdx);

            double maxDifference = maxVal;
            Console.WriteLine($"Максимальная разница: {maxDifference}");

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