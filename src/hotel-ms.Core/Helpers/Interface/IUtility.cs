using Microsoft.AspNetCore.Http;

namespace hotelier_core_app.Core.Helpers.Interface
{
    public interface IUtility : IAutoDependencyCore
    {
        byte[] ObjectToByteArray<T>(T obj);

        T ByteArrayToObject<T>(byte[] arrBytes);

        Task<HttpResponseMessage?> MakeHttpRequest(object request, string baseAddress, string requestUri, HttpMethod method, Dictionary<string, string> headers = null, bool logRequest = false, bool logResponse = false);

        Task<HttpResponseMessage?> MakeHttpFormRequest(object request, string baseAddress, string requestUri, HttpMethod method, Dictionary<string, string> headers = null);

        string Sha256Hash(string data);
        Task<HttpResponseMessage?> MakeHttpFormRequestWithMultipleFiles(List<KeyValuePair<string, object>> request, string baseAddress, string requestUri, HttpMethod method, Dictionary<string, string> headers = null);

        IFormFile ConvertToFormFile(MemoryStream stream, string fileName, string contentType);

        List<T> Paginate<T>(IEnumerable<T> items, int pageNumber, int pageSize) where T : class;
    }
}
