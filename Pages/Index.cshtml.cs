using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using SalesUON.Data;
using SalesUON.Models;
using System.Text.Json;
namespace SalesUON.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly HttpClient _http;

        public Sale? EditingSale { get; set; }
        public DateTime? SaleDate { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; } = "asc";
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; } = 2;
        public int PageSize = 50;
        public void OnGetShowEditForm(string vin)
        {
            if (Sales != null)
            {
                EditingSale = Sales.FirstOrDefault(s => s.VIN == vin);
            }
            else
            {

                EditingSale = new Sale();
            }
        }

        public void OnGetHideEditForm()
        {
            EditingSale = null;
        }
        public async Task<IActionResult> OnPostAddSaleAsync()
        {
            var newSale = new Sale
            {
                Make = Request.Form["Make"],
                Model = Request.Form["Model"],
                VIN = Request.Form["VIN"],
                SaleDate = DateTime.Parse(Request.Form["SaleDate"])
            };

            var response = await _http.PostAsJsonAsync("/api/csv/sales", newSale);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("./Index", new { page = CurrentPage });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Error adding sale.");
                return Page();
            }
        }
        public async Task<IActionResult> OnPostDeleteSaleAsync(string vin)
        {
            var response = await _http.DeleteAsync($"/api/csv/sales/{vin}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("./Index", new { page = CurrentPage });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Error deleting sale.");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostSaveSaleAsync()
        {
            // 1. Get the updated data from the form
            var updatedSale = new Sale
            {
                Make = Request.Form["Make"],
                Model = Request.Form["Model"],
                VIN = Request.Form["VIN"],
                SaleDate = DateTime.Parse(Request.Form["SaleDate"])
            };

            // 2. Send a PUT request to your API to update the sale
            var response = await _http.PutAsJsonAsync($"/api/csv/sales/{updatedSale.VIN}", updatedSale);

            // 3. If the update is successful, refresh the sales data
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("./Index", new { page = CurrentPage });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Error updating sale.");
                return Page();
            }
        }

        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _http = clientFactory.CreateClient("SalesAPI");
        }

        public List<Sale>? Sales { get; set; }

        public async Task<IActionResult> OnGetAsync(int currentPage = 1, int pageSize = 50, DateTime? saleDate = null, string make = null, string model = null)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            Console.WriteLine($"page: {currentPage}, pageSize: {pageSize}, saleDate: {saleDate.ToString()}, make: {make}, model: {model}");
            // Construct the API URL with filter parameters
            var url = $"/api/csv/sales?page={CurrentPage}&pageSize={PageSize}";
            Console.WriteLine($"url: {url}");


            // Add the filter parameters to the URL (if they are present)
            if (saleDate.HasValue)
            {
                url += $"&saleDate={saleDate.Value.ToString("yyyy-MM-dd")}";
                Console.WriteLine($"Add saleDate to url: {url}");
            }
            if (!string.IsNullOrEmpty(make))
            {
                url += $"&make={Uri.EscapeDataString(make)}";
                Console.WriteLine($"Add make to url: {url}");

            }
            if (!string.IsNullOrEmpty(model))
            {
                url += $"&model={Uri.EscapeDataString(model)}";
                Console.WriteLine($"Add model to url: {url}");

            }

            try
            {
                // Fetch sales data from the API
                var response = await _http.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the entire response to get pagination data
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

                    // Assign the items to the Sales property
                    Sales = apiResponse.items;

                    // Assign total items and calculate total pages
                    TotalItems = apiResponse.totalItems;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

                    // Set the current page
                    CurrentPage = apiResponse.currentPage;

                    // Update the model state to reflect the current page number
                    ModelState.AddModelError(string.Empty, $"Page: {CurrentPage}");

                    return Page(); // Return the page with updated data
                }
                else
                {
                    // Handle API request error
                    ModelState.AddModelError(string.Empty, "Error retrieving sales data. Please try again later.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                // Handle any other potential errors (e.g., network issues)
                _logger.LogError(ex, "Error fetching sales data.");
                if (ex is HttpRequestException || ex is JsonException)
                {
                    return RedirectToPage("/Error"); // Or RedirectToPage("/Index");
                }
                ModelState.AddModelError(string.Empty, "Error retrieving sales data. Please try again later.");
                return Page();
            }
        }
    }
}

public class ApiResponse
{
    public int currentPage { get; set; }
    public int pageSize { get; set; }
    public int totalItems { get; set; }
    public List<Sale> items { get; set; }
}

