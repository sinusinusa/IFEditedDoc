using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;


namespace IFEditedDoc.Logic;

public class Stitch : IClue
{
  public double colorDifferenceThreshold = 82; // Задайте пороговое значение
  public int numRegions = 10; // Задайте количество частей
  //Исследуем изображение на наличие подозрительных частей
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
      Console.WriteLine("Не удается преобразовать изображение: "+ ex.Message);
      return null;
    }
  }

  public Evidence getCheck(Document doc)
  {
    bool check = false;
    string message = "";
    try
    {
      using (var image = formFileToMat(doc.image))
      {
        
        int rows = image.Rows / numRegions;
        int cols = image.Cols / numRegions;
        Rectangle[] regionRectangles = new Rectangle[numRegions * numRegions];
        MCvScalar[] regionColors = new MCvScalar[numRegions * numRegions];

        for (int i = 0; i < numRegions; i++)
        {
          for (int j = 0; j < numRegions; j++)
          {

              Rectangle roi = new Rectangle(j * cols, i * rows, cols, rows);
              Mat region = new Mat(image, roi);

              // Рассчет среднего значения цвета
              MCvScalar meanColor = CvInvoke.Mean(region);

              regionColors[i * numRegions + j] = meanColor;
              

              Console.WriteLine($"Регион {i * numRegions + j + 1}:");
              Console.WriteLine($"Средний цвет (BGR): B={meanColor.V0}, G={meanColor.V1}, R={meanColor.V2}");
              Console.WriteLine($"Средний цвет: {(meanColor.V0 + meanColor.V1 + meanColor.V2) / 3}");
              if (meanColor.V0 <= 245 && meanColor.V1 <= 245)
              {
                regionRectangles[i * numRegions + j] = roi;
              }

          }
        }

        Mat sus = formFileToMat(doc.image);

        for (int i = 0; i < regionColors.Length; i++)
        {
          for (int j = 0; j < regionColors.Length; j++)
          {
            if (regionRectangles[i].Height != 0 && regionRectangles[j].Height != 0)
            {
              double colorDifference = CalculateColorDifference(regionColors[i], regionColors[j]);
              if (colorDifference > colorDifferenceThreshold)
              {
                check = true;
                CvInvoke.Rectangle(sus, regionRectangles[i], new MCvScalar(0, 0, 255, 10), 1);
              }
            }
          }
        }

        if (check)
        {
          message = "Stitch: Look closely at the parts of the picture that are highlighted in red squares on the Stitch image. Maybe it's a sign of photoshop.";
        }
        CvInvoke.PutText(sus, "Stitch", new Point(10, 30), FontFace.HersheyPlain, 1, new MCvScalar(0, 0, 0));
        byte[] to_model;
        using (MemoryStream ms = new MemoryStream())
        {
          sus.ToBitmap().Save(ms, ImageFormat.Jpeg);
          to_model = ms.ToArray();
        }
        doc.Suspects.Add(to_model);
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Не удается обработать изображение: " + ex.Message);
    }

    if (check == false)
    {
      message = "This image probably has no quality differences or it can't be analyzed";
    }
    return new Evidence(check, message);
  }
  static double CalculateColorDifference(MCvScalar color1, MCvScalar color2)
  {
    // Создаем объекты Mat для хранения цветов
    Mat matColor1 = new Mat(1, 1, DepthType.Cv8U, 3);
    Mat matColor2 = new Mat(1, 1, DepthType.Cv8U, 3);

    // Устанавливаем значения цветов
    matColor1.SetTo(color1);
    matColor2.SetTo(color2);

    // Создаем объекты для хранения результата преобразования
    Mat hsvColor1 = new Mat();
    Mat hsvColor2 = new Mat();

    // Преобразование цветов из BGR в HSV
    CvInvoke.CvtColor(matColor1, hsvColor1, ColorConversion.Bgr2Hsv);
    CvInvoke.CvtColor(matColor2, hsvColor2, ColorConversion.Bgr2Hsv);

    // Извлекаем каналы Hue, Saturation и Value
    Mat hue1 = new Mat();
    Mat hue2 = new Mat();
    CvInvoke.ExtractChannel(hsvColor1, hue1, 0); // Канал оттенка
    CvInvoke.ExtractChannel(hsvColor2, hue2, 0);

    Mat sat1 = new Mat();
    Mat sat2 = new Mat();
    CvInvoke.ExtractChannel(hsvColor1, sat1, 1); // Канал насыщенности
    CvInvoke.ExtractChannel(hsvColor2, sat2, 1);

    Mat val1 = new Mat();
    Mat val2 = new Mat();
    CvInvoke.ExtractChannel(hsvColor1, val1, 2); // Канал яркости
    CvInvoke.ExtractChannel(hsvColor2, val2, 2);

    // Рассчитываем различия в оттенке (Hue), насыщенности (Saturation) и яркости (Value)
    Mat hueDifference = new Mat();
    CvInvoke.AbsDiff(hue1, hue2, hueDifference);

    Mat saturationDifference = new Mat();
    CvInvoke.AbsDiff(sat1, sat2, saturationDifference);

    Mat valueDifference = new Mat();
    CvInvoke.AbsDiff(val1, val2, valueDifference);

    // Вычисляем общую разницу на основе различий в оттенке, насыщенности и яркости
    double totalDifference = CvInvoke.Sum(hueDifference).V0 + CvInvoke.Sum(saturationDifference).V0 + CvInvoke.Sum(valueDifference).V0;

    return totalDifference;
  }
}