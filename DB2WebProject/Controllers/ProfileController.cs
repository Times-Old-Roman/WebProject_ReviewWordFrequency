using DB2WebProject.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace DB2WebProject.Controllers
{
    public class ProfileController : Controller
    {
		public static ProfileViewModel model = null;
        public IActionResult Profile(int? id)
        {
			if(model == null)
			{
				List<ProductSummary> entries = new List<ProductSummary>();
				NpgsqlDataReader reader = DBTools.DBConnection.getConnection().Select(
					$"SELECT link, product_name, summary, user_comment" +
					$" FROM users.product_summary WHERE username = '{(string)TempData["username"]}'");
				while (reader.Read()) 
					entries.Add(new ProductSummary(reader.GetString(0), reader.GetString(1),
						reader.GetString(2), reader.GetString(3)));
				reader.Close();

				model = new ProfileViewModel((string)TempData["username"], entries);
			}
			if(id != null)
			{
				TempData["expandedItem"] = id;
			}
            return View(model);
        }
    }
}
