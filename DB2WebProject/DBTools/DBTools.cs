using Npgsql;

namespace DB2WebProject.DBTools
{
	public class DBConnection
	{
		private string connectionString = "Host=localhost;Port=5432;Database=reviewSummaryProjectDB;" +
			"User ID=postgres;Password=220702;Include Error Detail=true";
		private NpgsqlConnection connectionInstance;
		private static readonly DBConnection dBConnection = new DBConnection();

		private DBConnection()
		{
			connectionInstance = new NpgsqlConnection(connectionString);
			connectionInstance.Open();
		}

		~DBConnection() => connectionInstance.Close();

		public static DBConnection getConnection() => dBConnection;

		public NpgsqlDataReader Select(string sqlCommand)
		{
			NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connectionInstance);
			NpgsqlDataReader reader = command.ExecuteReader();
			return reader;
		}

		public void Insert(string sqlCommand)
		{
			NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connectionInstance);
			command.ExecuteNonQuery();
		}
	}
}
