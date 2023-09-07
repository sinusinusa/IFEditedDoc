
using SixLabors.ImageSharp.Metadata.Profiles.Icc;

namespace IFEditedDoc.Logic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using System.Drawing;
using Image = Image;

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
        using (Image image = Image.Load(stream))
        {
          if (doc.image.ContentType == "image/jpeg" || doc.image.ContentType == "image/jpg")
          {
            if (image.Metadata.ExifProfile != null)
            {
              var softwareTag = image.Metadata.ExifProfile.Values
                .FirstOrDefault(value => value.Tag == ExifTag.Software);

              if (softwareTag != null)
              {
                check = true; 
                message = "Meta: Image was edited using software: " + softwareTag.ToString();
              }
            }
          }
          if (doc.image.ContentType == "image/png")
          {
            if (image.Metadata.IccProfile != null)
            {
              var softwareTag = image.Metadata.IccProfile.Entries
                .FirstOrDefault(value => value.TagSignature == IccProfileTag.ProfileDescription);
              if (softwareTag != null)
              {
                check = true;
                message = "Meta: Image was edited using software";
              }
            }
          }

        }
      }
      
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Ошибка при чтении метаданных: {ex.Message}");
      message = "Meta: It's not supported file or there are no meta tags";
    }
    return new Evidence(check, message);


  }
}