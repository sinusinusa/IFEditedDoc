﻿using IFEditedDoc.Logic;
using Microsoft.AspNetCore.Mvc;

namespace IFEditedDoc.Controllers;

public class IndexController : Controller
{
  [HttpPost]
  public async Task<IActionResult> UploadFile(IFormFile uploadedFile)
  {
    if (uploadedFile != null && uploadedFile.Length > 0)
    {
 
      Document toCheck = new Document(uploadedFile);
      List<IClue> checkers = new List<IClue>
      {
        new Meta(),
        new ELA(),
        new Stitch()
      };
      foreach (var check in checkers)
      {
        toCheck.evidences.Add(check.getCheck(toCheck));
      }
      return View("UploadResult", toCheck);
    }
    else
    {
      return RedirectToAction("");
    }
  }
  // GET
  public IActionResult Index()
  {
    return View();
  }
}