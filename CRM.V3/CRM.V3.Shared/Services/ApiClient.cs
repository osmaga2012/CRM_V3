using CRM.Dtos.Response;
using CRM.V3.Shared.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CRM.V3.Shared.Services
{
    /// <summary>
    /// Converter para manejar strings que vienen del servidor como números (long)
    /// Ejemplo: "2132" -> 2132L
    /// </summary>
    public class StringToLongConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (long.TryParse(stringValue, out var result))
                {
                    return result;
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64();
            }
            
            return 0; // Valor por defecto si no se puede convertir
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    /// <summary>
    /// Converter para manejar strings que vienen del servidor como números (int)
    /// Ejemplo: "123" -> 123
    /// </summary>
    public class StringToIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (int.TryParse(stringValue, out var result))
                {
                    return result;
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }
            
            return 0;
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    /// <summary>
    /// Converter para manejar strings que vienen del servidor como números (decimal)
    /// Ejemplo: "123.45" -> 123.45M
    /// </summary>
    public class StringToDecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (decimal.TryParse(stringValue, out var result))
                {
                    return result;
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }
            
            return 0M;
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    public class ApiClient<TDto> : IApiClient<TDto> where TDto : class
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory httpClientFactory;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = 
            { 
                new StringToLongConverter(),
                new StringToIntConverter(),
                new StringToDecimalConverter()
            }
        };

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

                return await response.Content.ReadFromJsonAsync<IEnumerable<TDto>>(_jsonOptions) ?? Enumerable.Empty<TDto>();
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
            var response = await _httpClient.GetAsync($"{endpoint}/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TDto>(_jsonOptions);
        }

        public async Task<ResponseDto> CreateAsync(string endpoint, TDto dto)
        {
            try
            {
                //var test = dto.ToString();                

                var response = await _httpClient.PostAsJsonAsync(endpoint, dto);
                response.EnsureSuccessStatusCode(); // Lanza excepción si el código de estado HTTP no es de éxito
                var ret =  await response.Content.ReadFromJsonAsync<ResponseDto>(_jsonOptions);

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

            return await response.Content.ReadFromJsonAsync<ResponseDto>(_jsonOptions);

        }

        public async Task<ResponseDto> UpdateAsync(string endpoint, TDto dto, object id)
        {
            var response = await _httpClient.PutAsJsonAsync($"{endpoint}/{id}", dto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ResponseDto>(_jsonOptions); 
            
        }

        public async Task<ResponseDto> DeleteAsync(string endpoint, object id)
        {
            var response = await _httpClient.DeleteAsync($"{endpoint}/{id}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ResponseDto>(_jsonOptions);
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

            return await response.Content.ReadFromJsonAsync<ResponseDto>(_jsonOptions) ?? new ResponseDto { Success = true };
        }

    }
}
