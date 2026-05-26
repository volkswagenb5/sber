namespace sberbank.Model
{
    public class BankProduct
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string ProductType { get; set; }
        public string Description { get; set; }
        public decimal Rate { get; set; }
        public decimal ServiceCost { get; set; }

        public string DisplayCost
        {
            get { return ServiceCost.ToString("N2") + " руб."; }
        }

        public string DisplayRate
        {
            get { return Rate.ToString("N2") + " %"; }
        }
    }
}
