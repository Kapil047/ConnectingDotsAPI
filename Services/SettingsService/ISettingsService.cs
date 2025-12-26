using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;

namespace ConnectingDotsAPI.Services.SettingsService
{
    public interface ISettingsService
    {
        Task AddPageInRole(SettingsModel.PagesInRolesRequest request);
        Task DeleteCountry(int id);
        void DeleteEmailTemplate(int id);
        string? FindConfiguration(string name);
        bool FindConfigurationBool(string name);
        Task<List<SettingsModel.SettingsDetails>> GetAllSettings();
        Task<List<SettingsModel.CountryDetails>> GetCountries();
        SettingsModel.CountryDetails? GetCountryDetails(int id);
        Task<List<EmailTemplate>> GetEmailTemplate();
        EmailTemplate? GetEmailTemplateDetails(int id);
        Task<List<SettingsModel.SitePageDetails>> GetPagesInRoles(List<int> RoleId);
        Task<List<SettingsModel.SitePageDetails>> GetSitePages(int userId);
        StateProvince? GetStateProvince(int id);
        City? GetCity(int id);
        Task<List<StateProvince>> GetStateProvinces(int countryId);
        void SaveConfiguration(SettingsModel.ConfigurationRequest request);
        Task<Country> SaveCountry(SettingsModel.CountryRequest request);
        Task<int> SaveEmailTemplate(SettingsModel.EmailTemplateRequest request);
        Task<StateProvince> SaveState(SettingsModel.StateRequest request);
        Task<City> SaveCity(SettingsModel.CityRequest request);
        Task<City> DeleteCity(int id);
        Task<StateProvince> DeleteState(int id);
    }
}