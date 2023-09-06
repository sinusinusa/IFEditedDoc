namespace IFEditedDoc.Logic;
using ExifLib;
using System.Drawing;

public class Meta : IClue
{
  //Класс реализует проверку EXIF файла, мета-данных
  public Evidence getCheck(Document doc)
  {
    bool check = false;
    string message = "Software editions weren't found in meta";
    try
    {
      using (var stream = doc.image.OpenReadStream())
      {
        using (ExifReader reader = new ExifReader(stream))
        {
          string photoshopInfo;
          if (reader.GetTagValue(ExifTags.Software, out photoshopInfo))
          {
            check = photoshopInfo.Contains("Adobe Photoshop");
            message = "Was edited in " + photoshopInfo;
          }
        }
      }
      
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Ошибка при чтении метаданных: {ex.Message}");
    }
    return new Evidence(check, message);


  }
}