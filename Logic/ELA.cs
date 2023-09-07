using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace IFEditedDoc.Logic;

public class ELA : IClue
{
  public double maxDifferenceThreshold = 10;
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
    string message = "ELA is OK";
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
            // Конвертируем входное изображение в формат RGB (если оно не RGB)
            if (image.NumberOfChannels == 4)
            {
              CvInvoke.CvtColor(image, image, ColorConversion.Bgra2Bgr);
            }
            CvInvoke.AbsDiff(image, resavedImage, elaImage);

            // Конвертируем ELA изображение в оттенки серого (grayscale)
            CvInvoke.CvtColor(elaImage, elaImage, ColorConversion.Bgr2Gray);

            double minVal = 0, maxVal = 0;
            System.Drawing.Point minLoc = new System.Drawing.Point(), maxLoc = new System.Drawing.Point();

            CvInvoke.MinMaxLoc(elaImage, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

            double maxDifference = maxVal;
            Console.WriteLine($"Максимальная разница: {maxDifference}");
            if (maxDifference > maxDifferenceThreshold)
            {
              CvInvoke.Normalize(elaImage, elaImage, 0, 120, NormType.MinMax, DepthType.Cv8U);
              CvInvoke.PutText(elaImage, "ELA", new System.Drawing.Point(10, 30), FontFace.HersheyPlain, 1,
                new MCvScalar(255, 255, 255));
              byte[] to_model;
              using (MemoryStream ms = new MemoryStream())
              {
                elaImage.ToBitmap().Save(ms, ImageFormat.Jpeg);
                to_model = ms.ToArray();
              }
              doc.Suspects.Add(to_model);
              check = true;
              message = "ELA: Take a closer look at the picture elements you see in the ELA image. Maybe it's a sign of photoshop";
            }
            File.Delete("temp.jpg");
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