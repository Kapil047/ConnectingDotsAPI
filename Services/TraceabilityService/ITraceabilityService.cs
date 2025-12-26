using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;

namespace ConnectingDotsAPI.Services.traceabilityService
{
    public interface ITraceabilityService
    {
        Task<Traceability> Delete(TraceabilityModel.TraceabilityRequest request);
        TraceabilityModel.TraceabilityDetails Details(TraceabilityModel.TraceabilityRequest request);
        Task<Traceability> Save(TraceabilityModel.TraceabilityRequest request, int userId);
    }
}