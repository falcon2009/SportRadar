using SportRadar.Api.Model;

namespace SportRadar.Api.Service.V1
{
    public interface IScoreBoardDataProvider
    {
        IEnumerable<IScoreBoardItem> GetActiveMatchSummary();
    }
}
