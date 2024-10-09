using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using SalesUON.Models;
using System.Globalization;
using System.IO;

namespace YourProjectName.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsvController : ControllerBase
    {
        [HttpPost("upload")]
        public IActionResult UploadCsvFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided.");

            if (!file.ContentType.Equals("text/csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid file type. Please upload a CSV file.");

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    MissingFieldFound = null
                };
                using var csv = new CsvReader(reader, config);
                csv.Context.RegisterClassMap<SaleDateMap>();
                var records = csv.GetRecords<Sale>().ToList();

                // TODO Save to Datebase

                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

public class SaleDateMap : ClassMap<Sale>
{
    public SaleDateMap()
    {
        Map(m => m.Make);
        Map(m => m.Model);
        Map(m => m.VIN);
        Map(m => m.SaleDate)
            .Convert(args =>
            {
                // Try to parse the date using the specified format
                if (DateTime.TryParseExact(args.Row.GetField("SaleDate"), "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    return date;

                // If parsing fails, you can handle it differently (e.g., return a default value, throw an exception, or log the error)
                return DateTime.MinValue; // Or handle the error as needed
            });
    }
}