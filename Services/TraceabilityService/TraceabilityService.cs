using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectingDotsAPI.Services.traceabilityService
{
    public class TraceabilityService(ConnectingDotsDbContext db) : ITraceabilityService
    {
        private readonly ConnectingDotsDbContext db = db;

        public async Task<Traceability> Save(TraceabilityModel.TraceabilityRequest request, int userId)
        {
            var value = db.Traceabilities.Where(x => x.Id == request.Id
            || (!string.IsNullOrEmpty(request.Guid) && x.Guid == Guid.Parse(request.Guid))).FirstOrDefault() ?? new Traceability()
            {
                Guid = Guid.NewGuid(),
                CreatedOn = DateTime.Now,
            };
            value.UpdatedOn = DateTime.Now;
            value.Name = request.Name;
            value.Description = request.Description;
            value.ManagerUserId = userId;
            if (!string.IsNullOrEmpty(request.SupplierId))
                value.SupplierId = db.Users.Where(x => x.Guid == Guid.Parse(request.SupplierId)).Select(x => x.Id).FirstOrDefault();
            if (!string.IsNullOrEmpty(request.ParentNodeId))
                value.ParentNodeId = db.Traceabilities.Where(x => x.Guid == Guid.Parse(request.ParentNodeId)).Select(x => x.Id).FirstOrDefault();
            if (!string.IsNullOrEmpty(request.ProductId))
                value.ProductId = db.Products.Where(x => x.Guid == Guid.Parse(request.ProductId)).Select(x => x.Id).FirstOrDefault();
            value.TypeId = request.TypeId;
            if (value.Id == 0)
            {
                db.Traceabilities.Add(value);
            }
            await db.SaveChangesAsync();
            return new Traceability { Id = value.Id, Guid = value.Guid };
        }
        public async Task<Traceability> Delete(TraceabilityModel.TraceabilityRequest request)
        {
            var value = db.Traceabilities.Where(x => x.Id == request.Id
           || (!string.IsNullOrEmpty(request.Guid) && x.Guid == Guid.Parse(request.Guid))).FirstOrDefault() ?? throw new Exception("NOT_FOUND");
            value.Deleted = true;

            await db.SaveChangesAsync();
            return new Traceability { Id = value.Id };
        }

        public TraceabilityModel.TraceabilityDetails Details(TraceabilityModel.TraceabilityRequest request)
        {
            return db.Traceabilities.Where(x => x.Id == request.Id
            || (!string.IsNullOrEmpty(request.Guid) && x.Guid == Guid.Parse(request.Guid)))

                .Select(traceability =>
            new TraceabilityModel.TraceabilityDetails
            {
                Guid = traceability.Guid,
                ManagerUser = new { traceability.ManagerUser.FirstName, traceability.ManagerUser.LastName, Id =traceability.ManagerUser.Guid },
                Type = new { traceability.Type.Name, traceability.Type.Id, traceability.Type.SystemKeyword },
                Description = traceability.Description,
                Name = traceability.Name,
                ParentNode = traceability.ParentNode != null ? new {  traceability.ParentNode.Name, Id = traceability.ParentNode.Guid } : null,
                Product = traceability.Product != null ? new { traceability.Product.Name, Id = traceability.Product.Guid, traceability.Product.ShortDescription } : null,
                Supplier = traceability.Supplier != null ? new { traceability.Supplier.FirstName, traceability.Supplier.LastName, Id = traceability.Guid } : null,
            }).FirstOrDefault() ?? throw new Exception("NOT_FOUND");
        }
    }
}
