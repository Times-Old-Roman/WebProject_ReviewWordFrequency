namespace DB2WebProject.Models
{
	public class ProductSummary
	{
		public string link { get; set; } = "";
		public string productName { get; set; } = "";
		public string summary { get; set; } = "";
		public string userComment { get; set; } = "";

		public ProductSummary(string link, string productName, string summary, string userComment)
		{
			this.link = link;
			this.productName = productName;
			this.summary = summary;
			this.userComment = userComment;
		}
	}
}
