using GizaPowerIndustryWebsite.Models.Metals;

namespace GizaPowerIndustryWebsite.Services
{
    public interface IMetalsService
    {
        Task<MetalsWidgetDto> GetRatesAsync(CancellationToken cancellationToken = default);
    }
}
