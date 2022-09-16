using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace KnowledgeSpace.BackendServer.Helpers
{
    public class ApiResponse
    {
        public int StatusCode { get; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; }

        public ApiResponse(int statusCode, string massage = null)
        {
            StatusCode = statusCode;
            Message = massage ?? GetDefaultMassageForStatusCode(statusCode);
        }

        private static string GetDefaultMassageForStatusCode(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return "Resource not found";
                case 500:
                    return "An unhandled error occured";
                default:
                    return null;
            }
        }
    }
}
