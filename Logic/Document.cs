using System.Drawing;
using System.Net.Mime;

namespace IFEditedDoc.Logic;

public class Document
{
  public IFormFile image { get; set; }
  public List<Evidence> evidences { get; set; }
  public List<byte[]> Suspects { get; set; }

  public Document(IFormFile _image)
  {
    image = _image;
    evidences = new List<Evidence>();
    Suspects = new List<byte[]>();
  }
}