namespace EasyGames.Helpers
{
    public static class TierHelper
    {
        public static string FromProfit(decimal profit) =>
            profit >= 2000m ? "Platinum" :
            profit >= 500m ? "Gold" :
            profit >= 100m ? "Silver" : "Bronze";
    }
}
