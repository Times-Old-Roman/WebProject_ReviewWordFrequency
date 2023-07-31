using DB2WebProject.DBTools;
using Npgsql;

namespace DB2WebProject.Models
{
	public class IndexViewModel
	{
		public bool loggedIn { get; set; } = false;
		public List<string> websites { get; set; }
		public Dictionary<string, int> wordsToFreq { get; set; }

		private DBConnection connection;

		public IndexViewModel(bool loggedIn) 
		{
			this.loggedIn = loggedIn;
			this.wordsToFreq = new Dictionary<string, int>();
			websites = new List<string>();

			connection = DBConnection.getConnection();
			NpgsqlDataReader reader = connection.Select("SELECT name FROM users.website");
			while(reader.Read())
				websites.Add(reader.GetString(0));
			reader.Close();
		}
	}
}
