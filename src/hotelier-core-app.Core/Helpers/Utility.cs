using hotelier_core_app.Core.Helpers.Interface;
using hotelier_core_app.Core.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace hotelier_core_app.Core.Helpers
{
    public class Utility : IUtility
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;

        public Utility(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

        }

        #region public
        /// <summary>
        /// Convert an object to a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrBytes"></param>
        /// <returns></returns>
        public byte[] ObjectToByteArray<T>(T obj)
        {
            var jsonString = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(jsonString);
        }
        /// <summary>
        /// Convert a byte array to an object of the Type specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrBytes"></param>
        /// <returns></returns>
        public T ByteArrayToObject<T>(byte[] arrBytes)
        {
            string jsonString = Encoding.UTF8.GetString(arrBytes);
            T? obj = JsonConvert.DeserializeObject<T>(jsonString);
            return obj != null ? obj : throw new InvalidOperationException("Deserialization resulted in a null object.");
        }
        /// <summary>
        /// Asynchronously make a http call to an endpoint
        /// </summary>
        /// <param name="request"></param>
        /// <param name="baseAddress"></param>
        /// <param name="requestUri"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage?> MakeHttpRequest(object request, string baseAddress, string requestUri, HttpMethod method, Dictionary<string, string>? headers = null, bool logRequest = false, bool logResponse = false)
        {
            HttpResponseMessage? responseMessage = null;
            var apiRequestLog = new ApiRequestLog { RequestUrl = $"{baseAddress}{requestUri}", HttpMethod = method.Method };

            try
            {
                _httpClient = _httpClientFactory.CreateClient();
                _httpClient.BaseAddress = new Uri(baseAddress);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string data = JsonConvert.SerializeObject(request);
                apiRequestLog.Request = logRequest ? data : string.Empty;
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                if (method == HttpMethod.Post)
                {
                    HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    responseMessage = await _httpClient.PostAsync(requestUri, content);
                }
                else if (method == HttpMethod.Get)
                {
                    responseMessage = await _httpClient.GetAsync(requestUri);
                }
                else if (method == HttpMethod.Put)
                {
                    HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    responseMessage = await _httpClient.PutAsync(requestUri, content);
                }
                else if (method == HttpMethod.Delete)
                {
                    responseMessage = await _httpClient.DeleteAsync(requestUri);
                }

                if (responseMessage != null)
                {
                    apiRequestLog.Response = logResponse ? await responseMessage.Content.ReadAsStringAsync() : string.Empty;
                    apiRequestLog.HttpResponseStatusCode = responseMessage.StatusCode.ToString();
                }
                else
                {
                    apiRequestLog.Response = string.Empty;
                    apiRequestLog.HttpResponseStatusCode = "No Response";
                }
            }
            catch (Exception ex)
            {
                apiRequestLog.ExceptionMessage = ex.InnerException?.Message ?? ex.Message;
                throw;
            }
            finally
            {
                if (logRequest || logResponse) Log.Warning(JsonConvert.SerializeObject(apiRequestLog));
            }
            return responseMessage;
        }

        public async Task<HttpResponseMessage?> MakeHttpFormRequest(object request, string baseAddress, string requestUri, HttpMethod method, Dictionary<string, string> headers = null)
        {
            try
            {
                using HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(baseAddress);
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                if (request != null)
                {
                    MultipartFormDataContent content = new MultipartFormDataContent();
                    foreach (PropertyInfo property in request.GetType().GetProperties())
                    {
                        var value = property.GetValue(request, null);

                        if (value is IFormFile formFile)
                        {
                            SetFormFileData(formFile, content, property);
                        }
                        else if (value is List<IFormFile> fileList)
                        {
                            foreach (var file in fileList)
                            {
                                SetFormFileData(file, content, property);
                            }
                        }
                        else
                        {
                            if (value != null && !IsJsonSerializable(value))
                            {
                                content.Add(new StringContent(JsonConvert.SerializeObject(value)), property.Name);
                            }
                            else
                            {
                                if (value != null)
                                    content.Add(new StringContent(value.ToString() ?? string.Empty), property.Name);
                            }
                        }
                    }
                    if (method == HttpMethod.Post)
                        return await client.PostAsync(requestUri, content);
                    if (method == HttpMethod.Get)
                        return await client.GetAsync(requestUri);
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IFormFile ConvertToFormFile(MemoryStream stream, string fileName, string contentType)
        {
            return new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType,
            };
        }

        public async Task<HttpResponseMessage?> MakeHttpFormRequestWithMultipleFiles(List<KeyValuePair<string, object>> request, string baseAddress, string requestUri, HttpMethod method, Dictionary<string, string> headers = null)
        {
            try
            {
                using HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(baseAddress);
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                if (request != null)
                {
                    MultipartFormDataContent content = new MultipartFormDataContent();
                    foreach (var item in request)
                    {
                        var value = item.Value;

                        if (value is FormFile file)
                        {
                            byte[] fileBytes;
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }
                            var imageContent = new ByteArrayContent(fileBytes);
                            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

                            content.Add(imageContent, item.Key, file.FileName);
                        }
                        else
                        {
                            content.Add(new StringContent(JsonConvert.SerializeObject(value)), item.Key);
                        }
                    }
                    if (method == HttpMethod.Post)
                        return await client.PostAsync(requestUri, content);
                    if (method == HttpMethod.Get)
                        return await client.GetAsync(requestUri);
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Create a SHA256 version of the string passed
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Sha256Hash(string data)
        {
            //From String to byte array
            byte[] sourceBytes = Encoding.UTF8.GetBytes(data);
            byte[] hashBytes = SHA256.HashData(sourceBytes);
            string stringHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            return stringHash;
        }

        public List<T> Paginate<T>(IEnumerable<T> items, int pageNumber, int pageSize) where T : class
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            return items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }
        #endregion

        #region private
        private static void SetFormFileData(IFormFile file, MultipartFormDataContent content, PropertyInfo property)
        {
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                fileBytes = ms.ToArray();
            }
            var imageContent = new ByteArrayContent(fileBytes);
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

            content.Add(imageContent, property.Name, file.FileName);
        }

        private static bool IsJsonSerializable(object value)
        {
            try
            {
                JsonConvert.SerializeObject(value);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
        #endregion
    }
}
