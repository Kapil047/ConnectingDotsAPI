using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Services.CacheService;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ConnectingDotsAPI.Services.HelperService
{
    public class HelperService : IHelperService
    {
        private readonly ConnectingDotsDbContext db;
        private readonly ICacheService cacheService;
        public HelperService(ConnectingDotsDbContext db, ICacheService cacheService)
        {
            this.db = db;
            ValidateReferenceTypesAndCodes().Wait();
            this.cacheService = cacheService;
        }
        #region UrlRecord
        public async Task UpdateUrlRecord(int entityId, string entityName, string slug)
        {
            var urlRecord = db.UrlRecords.FirstOrDefault(u => u.EntityName == entityName && u.EntityId == entityId && u.Slug == slug);
            if (urlRecord != null)
                urlRecord.IsActive = true;
            else
            {
                db.UrlRecords.Add(new UrlRecord
                {
                    EntityId = entityId,
                    EntityName = entityName.ToLower(),
                    IsActive = true,
                    Slug = slug
                });
            }
            await db.SaveChangesAsync();

        }
        public async Task DeleteUrlRecord(int entityId, string entityName)
        {
            db.UrlRecords.RemoveRange(db.UrlRecords.Where(u => u.EntityName.ToLower() == entityName.ToLower() && u.EntityId == entityId));
            await db.SaveChangesAsync();
        }
        public async Task DeactivateUrlRecord(int entityId, string entityName)
        {
            await db.UrlRecords.Where(u => u.EntityName == entityName && u.EntityId == entityId)
                .ForEachAsync(urlRecord => urlRecord.IsActive = false);
            await db.SaveChangesAsync();
        }
        public int? GetEntityId(string entityName, string slug)
        {
            slug = slug.ToLower().Trim();
            entityName = entityName.ToLower().Trim();
            return db.UrlRecords.FirstOrDefault(u => u.EntityName == entityName && u.Slug.ToLower() == slug)?.EntityId;
        }

        #endregion


        #region Reference Type and Code
        public int? FindByReferenceType(string type)
        {
            return db.ReferenceTypes.Where(x=>x.SystemKeyword.ToLower() == type.ToLower()).Select(x=>x.Id).FirstOrDefault();
        }
        public int? FindByReferenceCodeAndType(string type, string code)
        {
            return (from rt in db.ReferenceTypes
                    join rc in db.ReferenceCodes on rt.Id equals rc.ReferenceTypeId
                    where rt.SystemKeyword.ToLower() == type.ToLower() && rc.SystemKeyword.ToLower() == code.ToLower()
                    select rc.Id).FirstOrDefault();
        }
        public async Task<ReferenceCode> DeleteReferenceCode(string code)
        {
            var referenceCode = db.ReferenceCodes.Where(x => x.SystemKeyword.ToLower() == code.ToLower().Trim()).FirstOrDefault() ?? throw new Exception("NOT FOUND");
            db.ReferenceCodes.Remove(referenceCode);
            await db.SaveChangesAsync();
            return new ReferenceCode { Id = referenceCode.Id };
        }
        public ReferenceCode GetReferenceCodeDetails(string type, string code)
        {
               return (from rt in db.ReferenceTypes
                             join rc in db.ReferenceCodes on rt.Id equals rc.ReferenceTypeId
                       where rt.SystemKeyword.ToLower() == type.ToLower() && rc.SystemKeyword.ToLower() == code.ToLower()
                             select rc).FirstOrDefault() ?? throw new Exception("NOT FOUND");
        }
        public List<ReferenceCode> GetReferenceCodes(string type)
        {

            var cachedValue = cacheService.GetValue(type);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonConvert.DeserializeObject<List<ReferenceCode>>(cachedValue) ?? [];
            }

            var values = db.ReferenceCodes.Where(x => x.ReferenceType.SystemKeyword.ToLower() == type.Trim().ToLower()
            && !x.ReferenceCodeId.HasValue)
                .Include(x => x.ReferenceCodeNavigation)
                .Include(x => x.InverseReferenceCodeNavigation)
                .ToList();



            //var values = await (from rt in db.ReferenceTypes
            //                    join rc in db.ReferenceCodes on rt.Id equals rc.ReferenceTypeId
            //                    where rt.SystemKeyword == type
            //                    select rc).ToListAsync();
            //cacheService.SetValue(type, JsonConvert.SerializeObject(values), 24 * 60 * 60);

            return values;
        }

        public async Task<ReferenceCode> SaveReferenceCode(int referenceTypeId, string systemKeyword, string name)
        {
            systemKeyword = systemKeyword.Trim().Replace(' ', '_');
            var lowerName = name.Trim().ToLower();

            // Check for duplicates
            var existingReferenceCode = await db.ReferenceCodes
                .FirstOrDefaultAsync(rc => rc.ReferenceTypeId == referenceTypeId
                                           && (rc.SystemKeyword == systemKeyword || rc.Name.ToLower() == lowerName));

            if (existingReferenceCode != null)
            {
                // Handle the case where the duplicate exists (optional)
                // For example, you can throw an exception or log a message
                throw new InvalidOperationException("A reference code with the same system keyword or name already exists.");
            }

            var referenceCode = new ReferenceCode
            {
                ReferenceTypeId = referenceTypeId,
                Enabled = true,
                Name = name,
                SystemKeyword = systemKeyword
            };
            db.ReferenceCodes.Add(referenceCode);

            await db.SaveChangesAsync();

            return new ReferenceCode { Id = referenceCode.Id};
        }


        public async Task<int> SaveReferenceType(string code)
        {
            code = code.Trim();
            var lowerCode = code.ToLower();
            var systemKeyword = code.Replace(' ', '_');

            // Check for duplicates
            var existingReferenceType = await db.ReferenceTypes
                .FirstOrDefaultAsync(rt => rt.Name == lowerCode || rt.SystemKeyword == systemKeyword);

            if (existingReferenceType != null)
            {
                // Return the Id of the existing ReferenceType if it already exists
                return existingReferenceType.Id;
            }

            var referenceType = new ReferenceType
            {
                Enabled = true,
                Name = lowerCode,
                SystemKeyword = systemKeyword
            };

            db.ReferenceTypes.Add(referenceType);
            await db.SaveChangesAsync();
            return referenceType.Id;
        }
        #endregion


        private async Task ValidateReferenceTypesAndCodes()
        {

            var downloadType = "download.type";
            var referenceCodes = new List<(string Code, string Text)>
    {
        ("product.image", "Product Image"),
        ("product.attachment", "Product Attachment"),
        ("category.image", "Category Image"),
        ("category.attachment", "Category Attachment")
    };

            var referenceType = await db.ReferenceTypes.FirstOrDefaultAsync(x => x.SystemKeyword == downloadType);
            if (referenceType == null)
            {
                var rtid = await SaveReferenceType(downloadType);

                foreach (var (code, text) in referenceCodes)
                {
                    if (!await db.ReferenceCodes.AnyAsync(x => x.ReferenceTypeId == rtid && x.SystemKeyword == code))
                    {
                        await SaveReferenceCode(rtid, code, text);
                    }
                }
            }
            else
            {
                var rtid = referenceType.Id;
                foreach (var (code, text) in referenceCodes)
                {
                    if (!await db.ReferenceCodes.AnyAsync(x => x.ReferenceTypeId == rtid && x.SystemKeyword == code))
                    {
                        await SaveReferenceCode(rtid, code, text);
                    }
                }
            }

        }
    }
}
