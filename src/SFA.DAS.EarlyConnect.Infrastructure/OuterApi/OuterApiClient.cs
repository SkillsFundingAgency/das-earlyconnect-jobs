using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace SFA.DAS.EarlyConnect.Infrastructure.OuterApi
{
    public class OuterApiClient : IOuterApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly OuterApiConfiguration _config;

        public OuterApiClient(
            HttpClient httpClient,
            IOptions<OuterApiConfiguration> config)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        }

        public async Task<ApiResponse<TResponse>> Get<TResponse>(IGetApiRequest request)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);

            AddHeaders(httpRequestMessage);

            var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var errorContent = "";
            var responseBody = (TResponse)default;

            if (IsNot200RangeResponseCode(response.StatusCode))
            {
                errorContent = json;
            }
            else if (string.IsNullOrWhiteSpace(json))
            {
                // 204 No Content from a potential returned null
                // Will throw if attempts to deserialise but didn't
                // feel right making it part of the error if branch
                // even if there is no content.
            }
            else
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                options.Converters.Add(new JsonStringEnumConverter());
                responseBody = JsonSerializer.Deserialize<TResponse>(json, options);
            }

            var getWithResponseCode = new ApiResponse<TResponse>(responseBody, response.StatusCode, errorContent);

            return getWithResponseCode;
        }

        public async Task<ApiResponse<TResponse>> Post<TResponse>(IPostApiRequest request, bool includeResponse = true)
        {
            var stringContent = request.Data != null ? new StringContent(JsonSerializer.Serialize(request.Data), Encoding.UTF8, "application/json") : null;

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, request.PostUrl);

            requestMessage.Content = stringContent;

            AddHeaders(requestMessage);

            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var errorContent = "";
            var responseBody = (TResponse)default;

            if (IsNot200RangeResponseCode(response.StatusCode))
            {
                errorContent = json;
                HandleException(response, json);
            }
            else if (includeResponse)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                responseBody = JsonSerializer.Deserialize<TResponse>(json, options);
            }

            var postWithResponseCode = new ApiResponse<TResponse>(responseBody, response.StatusCode, errorContent);

            return postWithResponseCode;
        }

        private void AddHeaders(HttpRequestMessage httpRequestMessage)
        {
            httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", _config.Key);
            httpRequestMessage.Headers.Add("X-Version", "1");
        }

        private static bool IsNot200RangeResponseCode(HttpStatusCode statusCode)
        {
            return !((int)statusCode >= 200 && (int)statusCode <= 299);
        }

        public virtual string HandleException(HttpResponseMessage response, string json)
        {
            return json;
        }
    }

    public interface IPostApiRequest : IBaseApiRequest
    {
        [JsonIgnore]
        string PostUrl { get; }
        object Data { get; set; }
    }
    public interface IGetApiRequest : IBaseApiRequest
    {
        [JsonIgnore]
        string GetUrl { get; }
    }
    public interface IBaseApiRequest
    {
        [JsonIgnore]
        string Version => "1.0";
    }
}