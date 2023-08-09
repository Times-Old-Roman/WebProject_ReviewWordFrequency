namespace DB2WebProject.Models
{
	public class ProfileViewModel
	{
		public string username { get; private set; } = "";
		public List<ProductSummary> productSummaries { get; set; } = new List<ProductSummary>();

		public ProfileViewModel(string username, List<ProductSummary> productSummaries)
		{
			this.username = username;
			this.productSummaries = productSummaries;
		}
	}
}
