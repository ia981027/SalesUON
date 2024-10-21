using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using SalesUON.Models;
using SalesUON.Pages;
using System.Globalization;
using System.IO;
using SalesUON.Data;
using MongoDB.Driver;
using System.Text.Json;

namespace UONSales.Controllers
{
    [ApiController]
    [Route("api/csv")]
    public class CsvController : ControllerBase
    {
        private readonly IMongoCollection<Sale> _collection; // MongoDB collection


        public CsvController(MyDbContext context, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDBConnection");
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("UON");

            _collection = database.GetCollection<Sale>("Sales");

        }

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

                _collection.InsertMany(records);


                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("sales")]
        public IActionResult GetSales(int page = 1, int pageSize = 50, DateTime? saleDate = null, string make = null, string model = null)
        {
            try
            {
                // Calculate skip for pagination
                var skip = (page - 1) * pageSize;

                // Start building the filter
                var filterBuilder = Builders<Sale>.Filter;
                var filter = filterBuilder.Empty; // Start with an empty filter

                // Add filters based on provided parameters
                if (saleDate.HasValue)
                {
                    filter &= filterBuilder.Eq(s => s.SaleDate, saleDate.Value);
                }
                if (!string.IsNullOrEmpty(make))
                {
                    filter &= filterBuilder.Eq(s => s.Make, make);
                }
                if (!string.IsNullOrEmpty(model))
                {
                    filter &= filterBuilder.Eq(s => s.Model, model);
                }

                // Use Find with the filter and Project to get the desired data
                var sales = _collection.Find(filter)
                                      .Skip(skip)
                                      .Limit(pageSize)
                                      .Project<Sale>(Builders<Sale>.Projection.Exclude("_id"))
                                      .ToList();

                // Create a response object with pagination information
                var response = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalItems = _collection.CountDocuments(filter), // Count with the filter
                    items = sales
                };

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                return Ok(json);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpDelete("sales/{vin}")]
        public IActionResult DeleteSale(string vin)
        {
            Console.WriteLine($"Deleting sale with VIN: {vin}");
            var filter = Builders<Sale>.Filter.Eq(s => s.VIN, vin);
            Console.WriteLine($"Filter: {filter}");
            var saleToDelete = _collection.Find(filter).FirstOrDefault();
            Console.WriteLine($"Deleting: {filter} and {saleToDelete}");

            if (saleToDelete == null)
            {
                return NotFound();
            }

            _collection.DeleteOne(filter);

            return NoContent();
        }

        [HttpPost("sales")]
        public IActionResult AddSale(Sale newSale)
        {
            try
            {
                _collection.InsertOne(newSale);
                return Ok(newSale);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("sales/summary/month")]
        public IActionResult GetSalesSummaryByMonth()
        {
            try
            {
                var summary = _collection.Aggregate()
                    .Group(s => new { s.SaleDate.Year, s.SaleDate.Month },
                           g => new SalesSummaryByMonth
                           {
                               Year = g.Key.Year,
                               Month = g.Key.Month,
                               TotalSales = g.Count()
                           })
                    .ToList();

                var json = JsonSerializer.Serialize(summary, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                return Ok(json);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("sales/summary/year")]
        public IActionResult GetSalesSummaryByYear()
        {
            try
            {
                var summary = _collection.Aggregate()
                    .Group(s => s.SaleDate.Year,
                           g => new SalesSummaryByYear
                           {
                               Year = g.Key,
                               TotalSales = g.Count()
                           })
                    .ToList();

                var json = JsonSerializer.Serialize(summary, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                return Ok(json);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("sales/summary/make")]
        public IActionResult GetSalesSummaryByMake()
        {
            try
            {
                var summary = _collection.Aggregate()
                    .Group(s => s.Make,
                           g => new SalesSummaryByMake
                           {
                               Make = g.Key,
                               TotalSales = g.Count()
                           })
                    .ToList();

                var json = JsonSerializer.Serialize(summary, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                return Ok(json);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("sales/summary/model")]
        public IActionResult GetSalesSummaryByModel()
        {
            try
            {
                var summary = _collection.Aggregate()
                    .Group(s => s.Model,
                           g => new SalesSummaryByModel
                           {
                               Model = g.Key,
                               TotalSales = g.Count()
                           })
                    .ToList();

                var json = JsonSerializer.Serialize(summary, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                return Ok(json);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPut("sales/{vin}")]
        public IActionResult UpdateSale(string vin, Sale updatedSale)
        {
            try
            {
                var filter = Builders<Sale>.Filter.Eq(s => s.VIN, vin);
                var update = Builders<Sale>.Update
                    .Set(s => s.Make, updatedSale.Make)
                    .Set(s => s.Model, updatedSale.Model)
                    .Set(s => s.SaleDate, updatedSale.SaleDate);

                var result = _collection.UpdateOne(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
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

                // If parsing fails, return MinValue
                return DateTime.MinValue;
            });
    }
}
