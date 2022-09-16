namespace KnowledgeSpace.BackendServer.Helpers
{
    public class ApiNotFoundRespone : ApiResponse
    {
        public ApiNotFoundRespone(string message)
            : base(404 , message)
        {
        }
    }
}
