namespace IFEditedDoc.Logic;

public class Evidence
{
  public bool ifEdited;
  public string message;

  public Evidence(bool _ifedited, string mes)
  {
    ifEdited = _ifedited;
    message = mes;
  }
}