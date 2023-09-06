using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;


namespace IFEditedDoc.Logic;

public class Stitch : IClue
{
  public double colorDifferenceThreshold = 62;
  //Исследуем изображение на наличие фрагментов из других изображений
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
        int numRegions = 5; // Задайте количество частей
        int rows = image.Rows / numRegions;
        int cols = image.Cols / numRegions;

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
            Console.WriteLine($"Средний цвет: {(meanColor.V0+ meanColor.V1 + meanColor.V2)/3}");
          }
        }
        for (int i = 0; i < regionColors.Length; i++)
        {
          for (int j = i+1; j < regionColors.Length; j++)
          {
            double colorDifference = CalculateColorDifference(regionColors[i], regionColors[j]);
            if (colorDifference > colorDifferenceThreshold)
            {
              message += $"Regions {i + 1} and {j + 1} vary greatly.\n";
              check = true;
            }
          }
        }
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
    double diff = Math.Sqrt(Math.Pow(color1.V0 - color2.V0, 2) + Math.Pow(color1.V1 - color2.V1, 2) + Math.Pow(color1.V2 - color2.V2, 2));
    return diff;
  }
}