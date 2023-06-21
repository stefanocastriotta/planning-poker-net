namespace PlanningPoker.Web.Models
{
    public class EstimateValueModel
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public int Order { get; set; }

        public string Label { get; set; }

        public int Value { get; set; }
    }
}
