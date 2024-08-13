namespace Improvement_Bot.db
{
    public class SummaryReport
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int UncompletedOrders { get; set; }
        public int OverdueOrders { get; set; }
    }
}

