using Amazon.Runtime.Internal.Util;
using Azure.Core;
using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services.CacheService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Newtonsoft.Json;
using static ConnectingDotsAPI.Models.UserModel;

namespace ConnectingDotsAPI.Services.SettingsService
{
    public class SettingsService(ConnectingDotsDbContext db, ICacheService cacheService) : ISettingsService
    {
        private readonly ConnectingDotsDbContext db = db;
        private readonly ICacheService cacheService = cacheService;
        #region Configuration/Settings
        public bool FindConfigurationBool(string name)
        {
            _ = bool.TryParse(FindConfiguration(name), out bool result);
            return result;
        }
        public string? FindConfiguration(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            name = name.Trim().ToLower();

            var value = cacheService.GetValue(name);
            if (value != null)
            {
                return value;
            }

            try
            {
                value = db.Settings.FirstOrDefault(x => x.Name == name)?.Value;
            }
            catch (Exception)
            {
                // Log the exception
                // logger.LogError(ex, "Error retrieving setting from the database");
                return null;
            }

            if (!string.IsNullOrEmpty(value))
            {
                cacheService.SetValue(name, value, 24 * 60 * 60); // Cache for 24 hours
            }

            return value;
        }
        public void SaveConfiguration(SettingsModel.ConfigurationRequest request)
        {

            request.Name = request.Name.Trim().ToLower();
            if (string.IsNullOrEmpty(request.Name)) throw new Exception("Invalid name");
            var setting = db.Settings.FirstOrDefault(s => s.Name == request.Name) ?? new Setting();
            setting.Value = request.Value;
            if (setting.Id == 0 && request.Name != null)
            {
                setting.Name = request.Name;
                db.Settings.Add(setting);
            }
            db.SaveChanges();
            cacheService.Delete(request.Name);
            cacheService.Delete("all-settings");

        }
        public async Task<List<SettingsModel.SettingsDetails>> GetAllSettings()
        {
            var key = "all-settings";
            var cachedValue = cacheService.GetValue(key);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonConvert.DeserializeObject<List<SettingsModel.SettingsDetails>>(cachedValue) ?? [];
            }
            var settings = await db.Settings.Select(x => new SettingsModel.SettingsDetails
            {
                Id = x.Id,
                Name = x.Name,
                Value = x.Value
            }).ToListAsync();
            cacheService.SetValue(key, JsonConvert.SerializeObject(settings));
            return settings;
        }
        #endregion

        #region Pages,Roles
        public async Task<List<SettingsModel.SitePageDetails>> GetPagesInRoles(List<int> RoleId)
        {
            return await (from pr in db.PagesInRoles
                          join p in db.SitePages on pr.PageId equals p.Id
                          where RoleId.Contains(pr.RoleId)
                          select new SettingsModel.SitePageDetails
                          {
                              SystemName = p.SystemName,
                              Id = p.Id,
                              Name = p.Name,
                          }).Distinct().ToListAsync();
        }
        public async Task<List<SettingsModel.SitePageDetails>> GetSitePages(int userId)
        {
            User user = db.Users
                .Include(u => u.Roles).ThenInclude(x => x.PagesInRoles)
                .FirstOrDefault(u => u.Id == userId) ?? throw new Exception("USER NOT FOUND");
            
            bool isSuperAdmin = user != null && user.Roles.Any(r => r.SystemName.Equals("superadmin", StringComparison.OrdinalIgnoreCase));
           
                

            List<int> pageIds = new();
            foreach (var role in user.Roles)
            {
                foreach (var item in role.PagesInRoles)
                {
                    pageIds.Add(item.PageId);
                }
                
            }
            return await db.SitePages.Where(x => x.Active == true && (isSuperAdmin || pageIds.Contains(x.Id))).OrderBy(x => x.Name).Select(x => new SettingsModel.SitePageDetails
            {
                SystemName = x.SystemName,
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();
        }
        public async Task AddPageInRole(SettingsModel.PagesInRolesRequest request)
        {
            db.PagesInRoles.RemoveRange(db.PagesInRoles.Where(x => x.RoleId == request.RoleId));

            request.PagesId.ToList().ForEach(x =>
            {
                db.PagesInRoles.Add(new PagesInRole
                {
                    RoleId = request.RoleId,
                    Active = true,
                    DateChanged = DateTime.Now,
                    DateCreated = DateTime.Now,
                    PageId = x
                });
            });

            await db.SaveChangesAsync();

        }
        #endregion


        #region Countries/State
        public async Task<List<SettingsModel.CountryDetails>> GetCountries()
        {
            var key = "country";
            var cachedValue = cacheService.GetValue(key);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonConvert.DeserializeObject<List<SettingsModel.CountryDetails>>(cachedValue) ?? [];
            }
            var countries = await db.Countries.Include(x => x.StateProvinces).Select(x =>
            new SettingsModel.CountryDetails
            {
                AllowsBilling = x.AllowsBilling,
                AllowsShipping = x.AllowsShipping,
                DisplayOrder = x.DisplayOrder,
                Id = x.Id,
                Name = x.Name,
                NumericIsoCode = x.NumericIsoCode,
                Published = x.Published,
                ThreeLetterIsoCode = x.ThreeLetterIsoCode,
                TwoLetterIsoCode = x.TwoLetterIsoCode,
                NumberOfStates = x.StateProvinces.Count
            }).OrderBy(c => c.DisplayOrder).ToListAsync();

            cacheService.SetValue(key, JsonConvert.SerializeObject(countries));
            return countries;
        }
        public SettingsModel.CountryDetails? GetCountryDetails(int id)
        {
            var key = $"country_{id}";
            var cachedValue = cacheService.GetValue(key);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonConvert.DeserializeObject<SettingsModel.CountryDetails>(cachedValue);
            }
            return db.Countries.Where(x => x.Id == id).Include(x => x.StateProvinces).Select(x =>
            new SettingsModel.CountryDetails
            {
                AllowsBilling = x.AllowsBilling,
                AllowsShipping = x.AllowsShipping,
                DisplayOrder = x.DisplayOrder,
                Id = x.Id,
                Name = x.Name,
                NumericIsoCode = x.NumericIsoCode,
                Published = x.Published,
                ThreeLetterIsoCode = x.ThreeLetterIsoCode,
                TwoLetterIsoCode = x.TwoLetterIsoCode,
                States = x.StateProvinces.Select(s => new { s.Id, s.Name, Cities = s.Cities.Select(c => new { c.Id, c.Name }).ToList() }).ToList()
            }).FirstOrDefault();
        }
        public async Task<Country> SaveCountry(SettingsModel.CountryRequest request)
        {
            var country = db.Countries.FirstOrDefault(c => c.Id == request.Id) ?? new Country();
            country.Published = request.Published;
            country.ThreeLetterIsoCode = request.ThreeLetterIsoCode;
            country.TwoLetterIsoCode = request.TwoLetterIsoCode;
            country.Published = request.Published;
            country.AllowsShipping = request.AllowsShipping;
            country.AllowsBilling = request.AllowsBilling;
            country.DisplayOrder = request.DisplayOrder;
            country.Name = request.Name;
            country.NumericIsoCode = request.NumericIsoCode;
            if (country.Id == 0)
                db.Countries.Add(country);
            await db.SaveChangesAsync();
            cacheService.Delete("country");
            cacheService.Delete($"country_{request.Id}");
            return new Country { Id = country.Id };

        }
        public async Task DeleteCountry(int id)
        {
            var country = db.Countries.Where(c => c.Id == id).Include(x => x.StateProvinces).FirstOrDefault() ?? throw new Exception("NOT FOUND");
            country.StateProvinces.Clear();
            db.Countries.Remove(country);
            cacheService.Delete("country");
            cacheService.Delete($"country_{id}");
            await db.SaveChangesAsync();
        }

        public async Task<StateProvince> SaveState(SettingsModel.StateRequest request)
        {
            var val = db.StateProvinces.FirstOrDefault(c => c.Id == request.Id) ?? new StateProvince();
            val.Published = request.Published;
            val.Name = request.Name;
            val.DisplayOrder = request.DisplayOrder;
            val.Abbreviation = request.Abbreviation;
            val.CountryId = request.CountryId;
            if (val.Id == 0)
                db.StateProvinces.Add(val);
            await db.SaveChangesAsync();
            //deleting cahce for country in case of new state addition
            cacheService.Delete($"country_{request.CountryId}");

            return new StateProvince { Id = val.Id };
        }
        public async Task<List<StateProvince>> GetStateProvinces(int countryId)
        {
            return await db.StateProvinces.Where(c => c.CountryId == countryId).OrderBy(c => c.DisplayOrder).ToListAsync();
        }
        public StateProvince? GetStateProvince(int id)
        {
            return db.StateProvinces.FirstOrDefault(x => x.Id == id);
        }
        public async Task<StateProvince> DeleteState(int id)
        {
            var val = db.StateProvinces.Where(c => c.Id == id).FirstOrDefault() ?? throw new Exception("State NOT FOUND");
            db.StateProvinces.Remove(val);
            await db.SaveChangesAsync();
            return new StateProvince { Id = val.Id };
        }

        public async Task<City> SaveCity(SettingsModel.CityRequest request)
        {
            var val = db.Cities.FirstOrDefault(c => c.Id == request.Id) ?? new City();
            val.Published = request.Published;
            val.Name = request.Name;
            val.DisplayOrder = request.DisplayOrder;
            val.StateId = request.StateId;
            val.CountryId = request.CountryId;
            if (val.Id == 0)
                db.Cities.Add(val);
            await db.SaveChangesAsync();
            cacheService.Delete($"country_{request.CountryId}");

            return new City { Id = val.Id };
        }
        public City? GetCity(int id)
        {
            return db.Cities.FirstOrDefault(x => x.Id == id);
        }
        public async Task<City> DeleteCity(int id)
        {
            var val = db.Cities.Where(c => c.Id == id).FirstOrDefault() ?? throw new Exception("City NOT FOUND");
            db.Cities.Remove(val);
            await db.SaveChangesAsync();
            return new City { Id = val.Id };

        }
        #endregion

        #region Email Template
        public async Task<int> SaveEmailTemplate(SettingsModel.EmailTemplateRequest request)
        {
            var template = db.EmailTemplates.FirstOrDefault(et => et.Id == request.Id) ?? new EmailTemplate();
            template.PreviewFirst = request.PreviewFirst;
            template.SystemName = request.SystemName;
            template.Active = request.Active;
            template.Subject = request.Subject;
            template.DateChanged = DateTime.UtcNow;
            template.Name = request.Name;
            template.TemplateHtml = request.TemplateHtml;
            template.SendTo = request.SendTo;
            if (template.Id == 0)
            {
                template.DateCreated = DateTime.UtcNow;
                db.EmailTemplates.Add(template);
            }
            await db.SaveChangesAsync();
            return template.Id;
        }
        public async Task<List<EmailTemplate>> GetEmailTemplate()
        {
            return await db.EmailTemplates.ToListAsync();
        }
        public EmailTemplate? GetEmailTemplateDetails(int id)
        {
            return db.EmailTemplates.FirstOrDefault(et => et.Id == id);
        }
        public void DeleteEmailTemplate(int id)
        {
            db.EmailTemplates.RemoveRange(db.EmailTemplates.Where(x => x.Id == id));
        }
        #endregion


    }
}
