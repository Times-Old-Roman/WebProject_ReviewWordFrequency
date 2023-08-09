using DB2WebProject.DBTools;
using Npgsql;

namespace DB2WebProject.Models
{
	public class IndexViewModel
	{
		public bool loggedIn { get; set; } = false;
		public string username { get; set; } = "";
		public Uri uriToProduct { get; set; } = null;
		public List<string> websites { get; set; } = new List<string>();
		public Dictionary<string, int> wordsToFreq { get; set; } = new Dictionary<string, int>();

		private DBConnection connection;

		public IndexViewModel(bool loggedIn) 
		{
			this.loggedIn = loggedIn;

			connection = DBConnection.getConnection();
			NpgsqlDataReader reader = connection.Select("SELECT name FROM users.website");
			while(reader.Read())
				websites.Add(reader.GetString(0));
			reader.Close();
		}
	}
}
