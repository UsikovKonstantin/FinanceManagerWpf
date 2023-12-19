﻿using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace Finance_Manager;

/// <summary>
/// Состояние базы данных.
/// </summary>
public enum DatabaseState
{
	Correct,
	Incorrect,
	Missing
}

/// <summary>
/// Класс для проверки наличия и правильности формата базы данных и выполнения запросов. 
/// </summary>
public class Connector
{
	#region Поля
	/// <summary>
	/// Строка соединения.
	/// </summary>
	private readonly string connectionString;

	/// <summary>
	/// Состояние базы данных.
	/// </summary>
    public DatabaseState State { get; private set; }
	#endregion

	#region Конструктор
	/// <summary>
	/// Конструктор коннектора по параметрам для подключения к базе данных.
	/// </summary>
	/// <param name="host"> хост </param>
	/// <param name="port"> порт </param>
	/// <param name="database"> название базы данных </param>
	/// <param name="username"> имя пользователя </param>
	/// <param name="password"> пароль </param>
	public Connector(string host, int port, string database, string username, string password)
	{
		// Создаем строку соединения
        NpgsqlConnectionStringBuilder connectionStringBuilder = new NpgsqlConnectionStringBuilder()
        {
            Host = host,
            Port = port,
            Database = database,
            Username = username,
            Password = password
        };
		connectionString = connectionStringBuilder.ToString();

		// Проверяем подключение к базе данных
		try
		{
			using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
			{
				connection.Open();	
			}
		}
		catch (Exception)
		{
			State = DatabaseState.Missing;
			return;
		}

		// Проверяем правильность таблиц
		if (!CheckDatabase())
		{
			State = DatabaseState.Incorrect;
			return;
		}

        State = DatabaseState.Correct;
    }
	#endregion

	#region Методы для проверки таблиц
	/// <summary>
	/// Удаляет все таблицы из базы данных и создает новые таблицы.
	/// </summary>
	public void CreateTables()
    {
		using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
		{
			connection.Open();

            // Удаляем схему
			string queryDropSchema = "DROP SCHEMA public CASCADE";
			ExecuteAction(connection, queryDropSchema);

            // Создаем новую схему
			string queryCreateDSchema = "CREATE SCHEMA public";
			ExecuteAction(connection, queryCreateDSchema);

            // Создаем таблицу 'person' 
			string queryCreatePersonTable = "CREATE TABLE person (" +
                "person_id INT NOT NULL GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY, " +
                "person_name VARCHAR(50) NOT NULL UNIQUE )";
			ExecuteAction(connection, queryCreatePersonTable);

			// Создаем таблицу 'category'
			string queryCreateCategoryTable = "CREATE TABLE category (" +
				"category_id INT NOT NULL GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY, " +
				"category_name VARCHAR(50) NOT NULL UNIQUE )";
			ExecuteAction(connection, queryCreateCategoryTable);

			// Создаем таблицу 'transfer'
			string queryCreateTransferTable = "CREATE TABLE transfer (" +
                "transfer_id INT NOT NULL GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY, " +
                "person_id INT NOT NULL REFERENCES person (person_id) ON DELETE CASCADE, " +
                "category_id INT NOT NULL REFERENCES category (category_id) ON DELETE CASCADE, " +
                "description VARCHAR(255) NOT NULL, " +
                "amount FLOAT NOT NULL, " +
                "done_at DATE NOT NULL )";
			ExecuteAction(connection, queryCreateTransferTable);

			// Добавляем данные в таблицу 'category'
			string queryInsertCategory1 = "INSERT INTO category (category_name) VALUES ('Перевод между пользователями')";
			ExecuteAction(connection, queryInsertCategory1);
			string queryInsertCategory2 = "INSERT INTO category (category_name) VALUES ('Единовременная транзакция')";
			ExecuteAction(connection, queryInsertCategory2);
		}
		State = DatabaseState.Correct;
	}

	/// <summary>
	/// Проверяет формат таблиц.
	/// </summary>
	/// <returns> true - если таблицы соответствуют формату, иначе - false </returns>
	public bool CheckDatabase()
	{
		using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
		{
			connection.Open();

			// Проверяем наличие необходимых таблиц в базе данных
			string[] tablesToCheck = { "person", "category", "transfer" };
			foreach (string table in tablesToCheck)
			{
				NpgsqlParameter[] parameters = new NpgsqlParameter[]
				{
					new NpgsqlParameter("tableName", NpgsqlDbType.Varchar) { Value = table }
				};
				object? result = ExecuteScalar(connection, $"SELECT table_name FROM information_schema.tables WHERE table_name = @tableName", parameters);
				if (result == null || result == DBNull.Value)
					return false;
			}

			// Проверяем формат всех столбцов в таблицах
			bool res = CheckColumn(connection, "person", "person_id", "integer", true, false) &&
					   CheckColumn(connection, "person", "person_name", "character varying", false, false) &&
					   CheckColumn(connection, "category", "category_id", "integer", true, false) &&
					   CheckColumn(connection, "category", "category_name", "character varying", false, false) &&
					   CheckColumn(connection, "transfer", "transfer_id", "integer", true, false) &&
					   CheckColumn(connection, "transfer", "person_id", "integer", false, false) &&
					   CheckColumn(connection, "transfer", "category_id", "integer", false, false) &&
					   CheckColumn(connection, "transfer", "description", "character varying", false, false) &&
					   CheckColumn(connection, "transfer", "amount", "double precision", false, false) &&
					   CheckColumn(connection, "transfer", "done_at", "date", false, false);
			return res;
		}
	}

	/// <summary>
	/// Проверяет, что столбец соответствует формату.
	/// </summary>
	/// <param name="connection"> соединение </param>
	/// <param name="tableName"> название таблицы </param>
	/// <param name="columnName"> название столбца </param>
	/// <param name="columnType"> тип столбца </param>
	/// <param name="isPrimaryKey"> является ли столбец первичным ключом </param>
	/// <param name="nullable"> допускает ли столбец значения null </param>
	/// <returns> true - если столбец соответствует формату, иначе - false </returns>
	private bool CheckColumn(NpgsqlConnection connection, string tableName, string columnName, string columnType, bool isPrimaryKey, bool nullable)
	{
		string sqlQuery = "SELECT data_type, is_identity, is_nullable FROM information_schema.columns WHERE table_name = @tableName AND column_name = @columnName";
		NpgsqlParameter[] parameters = new NpgsqlParameter[]
		{
		new NpgsqlParameter("tableName", NpgsqlDbType.Varchar) { Value = tableName },
		new NpgsqlParameter("columnName", NpgsqlDbType.Varchar) { Value = columnName },
		};

		using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
		{
			command.Parameters.AddRange(parameters);
			using (NpgsqlDataReader reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					return (string)reader["data_type"] == columnType &&
							(string)reader["is_identity"] == (isPrimaryKey ? "YES" : "NO") &&
							(string)reader["is_nullable"] == (nullable ? "YES" : "NO");
				}
				return false;
			}
		}
	}
	#endregion

	#region Методы для выполнения запросов
	/// <summary>
	/// Выполняет sql запрос, который ничего не возвращает.
	/// </summary>
	/// <param name="connection"> соединение </param>
	/// <param name="sqlQuery"> запрос </param>
	/// <param name="parameters"> параметры </param>
	public void ExecuteAction(NpgsqlConnection connection, string sqlQuery, NpgsqlParameter[]? parameters = null)
	{
		using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
		{
			if (parameters != null)
			{
				command.Parameters.AddRange(parameters);
			}

			command.ExecuteNonQuery();
		}
	}

	/// <summary>
	/// Выполняет sql запрос, который ничего не возвращает.
	/// </summary>
	/// <param name="sqlQuery"> запрос </param>
	/// <param name="parameters"> параметры </param>
	public void ExecuteAction(string sqlQuery, NpgsqlParameter[]? parameters = null)
	{
		using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
		{
			connection.Open();

			ExecuteAction(connection, sqlQuery, parameters);
		}
	}

	/// <summary>
	/// Выполняет sql запрос, который возвращает единственное значение.
	/// </summary>
	/// <param name="connection"> соединение </param>
	/// <param name="sqlQuery"> запрос </param>
	/// <param name="parameters"> параметры </param>
	/// <returns> результат запроса </returns>
	public object? ExecuteScalar(NpgsqlConnection connection, string sqlQuery, NpgsqlParameter[]? parameters = null)
	{
		using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
		{
			if (parameters != null)
			{
				command.Parameters.AddRange(parameters);
			}

			return command.ExecuteScalar();
		}
	}

	/// <summary>
	/// Выполняет sql запрос и возврашает результат в виде таблицы.
	/// </summary>
	/// <param name="query"> запрос </param>
	/// <param name="parameters"> параметры </param>
	/// <returns> таблица </returns>
	public DataTable ExecuteTable(string query, NpgsqlParameter[]? parameters = null)
    {
		DataTable table = new DataTable();
		using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
		{
			connection.Open();

			using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
			{
				if (parameters != null)
				{
					command.Parameters.AddRange(parameters);
				}

				using (NpgsqlDataReader reader = command.ExecuteReader())
				{
					for (var i = 0; i <= reader.FieldCount - 1; i++)
					{
						table.Columns.Add(reader.GetName(i));
					}
					while (reader.Read())
					{
						DataRow row = table.NewRow();
						for (var i = 0; i < reader.FieldCount; i++)
						{
							row[i] = reader.GetValue(i);
						}
						table.Rows.Add(row);
					}
				}
			}
		}
		return table;
	}
	#endregion
}