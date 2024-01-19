using SportRadar.Api.Model;

namespace SportRadar.App.Extension
{
    public static class IScoreBoardItemExtension
    {
        public static string ToString(this IScoreBoardItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return $"{item.HomeTeamName} {item.HomeTeamGoalTotal} - {item.AwayTeamName} {item.AwayTeamGoalTotal}";
        }
    }
}
