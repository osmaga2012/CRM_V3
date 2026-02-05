using CRM.Dtos.Response;
using CRM.V3.Shared.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CRM.V3.Shared.Services
{
    public class ApiClient<TDto> : IApiClient<TDto> where TDto : class
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory httpClientFactory;

        public ApiClient(HttpClient httpClient,IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
            //_httpClient = httpClientFactory.CreateClient("ApiCRM");
            _httpClient = httpClientFactory.CreateClient("ApiClient");

        }

        public async Task<IEnumerable<TDto>> GetAllAsync(string endpoint,
            Dictionary<string, string>? queryParams = null, params string[] includes)
        {
            try
            {
                string requestUrl = endpoint;
                var allQueryParams = new Dictionary<string, string>();

                // Añade los queryParams existentes
                if (queryParams != null)
                {
                    foreach (var kvp in queryParams)
                    {
                        allQueryParams[kvp.Key] = kvp.Value;
                    }
                }

                // Añade el parámetro de includes si existe
                if (includes != null && includes.Any())
                {
                    // Convierte la lista de includes en una cadena separada por comas
                    // Por ejemplo: "Barco,Empresa"
                    allQueryParams["includes"] = string.Join(",", includes);
                }

                // Si se proporcionan parámetros (incluyendo los nuevos 'includes'), los añade a la URL como query string
                if (allQueryParams.Any())
                {
                    var queryString = string.Join("&", allQueryParams.Select(kvp =>
                        $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"
                    ));
                    requestUrl = $"{requestUrl}?{queryString}";
                }

                var response = await _httpClient.GetAsync(requestUrl);
                if (response.StatusCode == HttpStatusCode.NoContent ||
                    response.Content.Headers.ContentLength == 0)
                {
                    return Enumerable.Empty<TDto>();
                }

                return await response.Content.ReadFromJsonAsync<IEnumerable<TDto>>() ?? Enumerable.Empty<TDto>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching data from {endpoint}: {ex.Message}");
                throw;
            }


            //return await _httpClient.GetFromJsonAsync<IEnumerable<TDto>>(endpoint) ?? Enumerable.Empty<TDto>();
        }

        public async Task<TDto?> GetByIdAsync(string endpoint, object id)
        {
            return await _httpClient.GetFromJsonAsync<TDto>($"{endpoint}/{id}");
        }

        public async Task<ResponseDto> CreateAsync(string endpoint, TDto dto)
        {
            try
            {
                //var test = dto.ToString();                

                var response = await _httpClient.PostAsJsonAsync(endpoint, dto);
                response.EnsureSuccessStatusCode(); // Lanza excepción si el código de estado HTTP no es de éxito
                var ret =  await response.Content.ReadFromJsonAsync<ResponseDto>();

                return ret;

                }
            catch (Exception ex)
            {

                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }


        }
        public async Task<ResponseDto> UpdateAsync(string endpoint, TDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{endpoint}", dto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ResponseDto>();

        }

        public async Task<ResponseDto> UpdateAsync(string endpoint, TDto dto, object id)
        {
            var response = await _httpClient.PutAsJsonAsync($"{endpoint}/{id}", dto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ResponseDto>(); 
            
        }

        public async Task<ResponseDto> DeleteAsync(string endpoint, object id)
        {
            var response = await _httpClient.DeleteAsync($"{endpoint}/{id}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ResponseDto>();
        }

        public async Task<ResponseDto> UploadFileAsync(string endpoint, IBrowserFile file, long maxAllowedSize = 20 * 1024 * 1024, string formFieldName = "file")
        {
            ArgumentNullException.ThrowIfNull(file);

            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize));
            var mediaType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            content.Add(fileContent, formFieldName, file.Name);

            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ResponseDto>() ?? new ResponseDto { Success = true };
        }

    }
}
