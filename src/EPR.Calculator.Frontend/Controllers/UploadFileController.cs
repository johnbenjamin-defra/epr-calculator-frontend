﻿using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadFileController : Controller
    {
        private const long MaxFileSize = 50 * 1024; // 50 KB

        public IActionResult Index()
        {
            return View(ViewNames.UploadFileIndex);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            try
            {
                if (ValidateCSV(fileUpload).ErrorMessage is not null)
                {                   
                        ViewBag.Errors = JsonConvert.DeserializeObject<ErrorViewModel>(TempData["Default_Parameter_Upload_Errors"].ToString());
                        return View(ViewNames.UploadFileIndex);                   
                }

                var schemeTemplateParameterValues = await PrepareDataForUpload(fileUpload);

                ViewData["schemeTemplateParameterValues"] = schemeTemplateParameterValues.ToArray();

                return View(ViewNames.UploadFileRefresh);
            }
            catch(Exception ex)
            {
                return RedirectToAction("Index", "StandardError");
            }
        }

        public async Task<IActionResult> Upload()
        {
            try
            {
                if (TempData["FilePath"] != null)
                {
                    using var stream = System.IO.File.OpenRead(TempData["FilePath"].ToString());
                    var fileUpload = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

                    if (ValidateCSV(fileUpload).ErrorMessage is not null)
                    {
                        if (TempData["Default_Parameter_Upload_Errors"] != null)
                        {
                            ViewBag.Errors = JsonConvert.DeserializeObject<ErrorViewModel>(TempData["Default_Parameter_Upload_Errors"].ToString());
                            return View(ViewNames.UploadFileIndex);
                        }
                    }

                    var schemeTemplateParameterValues = await PrepareDataForUpload(fileUpload);

                    ViewData["schemeTemplateParameterValues"] = schemeTemplateParameterValues.ToArray();

                    return View(ViewNames.UploadFileRefresh);
                }

                // Code will reach this point if the uploaded file is not available
                return RedirectToAction("Index", "StandardError");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "StandardError");
            }
        }

        public IActionResult DownloadCsvTemplate()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), StaticHelpers.Path);
                return PhysicalFile(filePath, "text/csv", "SchemeParameterTemplate.v0.1.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occured while processing request" + ex.Message);
            }
        }

        private async Task<List<SchemeParameterTemplateValue>> PrepareDataForUpload(IFormFile fileUpload)
        {
            try
            {
                var schemeTemplateParameterValues = new List<SchemeParameterTemplateValue>();

                using var memoryStream = new MemoryStream(new byte[fileUpload.Length]);
                await fileUpload.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using (var reader = new StreamReader(memoryStream))
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        PrepareHeaderForMatch = header => Regex.Replace(header.ToString(), @"\s", string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
                        ShouldSkipRecord = args => args.Row.Parser.Record.All(string.IsNullOrWhiteSpace) || args.Row.GetField(0).Contains("upload version")
                    };
                    using (var csv = new CsvReader(reader, config))
                    {
                        csv.Read();
                        while (csv.Read())
                        {
                            schemeTemplateParameterValues.Add(
                                new SchemeParameterTemplateValue() { ParameterUniqueReferenceId = csv.GetField(0), ParameterValue = csv.GetField(5) });
                        }
                    }
                }

                return schemeTemplateParameterValues;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private ErrorViewModel ValidateCSV(IFormFile fileUpload)
        {
            ErrorViewModel validationErrors = CSVHelper.ValidateCSV(fileUpload);

            if (validationErrors.ErrorMessage != null)
            {
                TempData["Default_Parameter_Upload_Errors"] = JsonConvert.SerializeObject(validationErrors);
            }

            return validationErrors;
        }
    }
}
