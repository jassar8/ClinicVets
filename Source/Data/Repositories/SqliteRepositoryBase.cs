using Microsoft.Data.Sqlite;

namespace ClinicVetsAvalonia.Data.Repositories
{
    internal abstract class SqliteRepositoryBase
    {
        protected static SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(DatabaseSettings.ConnectionString);
            connection.Open();
            return connection;
        }

        protected static void EnsureColumnExists(
            SqliteConnection connection,
            string tableName,
            string columnName,
            string columnDefinition)
        {
            bool columnExists = false;

            using var infoCommand = new SqliteCommand($"PRAGMA table_info({tableName});", connection);
            using (var reader = infoCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.GetString(1) == columnName)
                    {
                        columnExists = true;
                        break;
                    }
                }
            }

            if (columnExists)
                return;

            using var alterCommand = new SqliteCommand(
                $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};",
                connection);
            alterCommand.ExecuteNonQuery();
        }
    }
}
