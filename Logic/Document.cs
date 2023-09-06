using System.Drawing;
using System.Net.Mime;

namespace IFEditedDoc.Logic;

public class Document
{
  public IFormFile image { get; set; }
  public List<Evidence> evidences { get; set; }
  public Bitmap Suspects { get; set; }

  public Document(IFormFile _image)
  {
    image = _image;
    evidences = new List<Evidence>();
    try
    {
      using (var stream = _image.OpenReadStream())
      {
        Suspects = new Bitmap(stream);
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Не удалось сгенерировать документ вывода: "+ex.Message);
    }
  }