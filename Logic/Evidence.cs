namespace IFEditedDoc.Logic;

public class Evidence
{
  public bool ifEdited { get; }
  public string message { get; }

  public Evidence(bool _ifedited, string mes)
  {
    ifEdited = _ifedited;
    message = mes;
  }
}