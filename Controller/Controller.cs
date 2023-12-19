using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace Finance_Manager;

/// <summary>
/// Класс действий, производимых над базой данных.
/// </summary>
public class Controller 
{
	#region Поля
	/// <summary>
	/// Объект для выполнения запросов к базе данных.
	/// </summary>
	public Connector Connector { get; }

	/// <summary>
	/// Пользователи
	/// </summary>
    public DataTable Persons { get; private set; } = new DataTable();

	/// <summary>
	/// Категории
	/// </summary>
	public DataTable Categories { get; private set; } = new DataTable();

	/// <summary>
	/// Переводы
	/// </summary>
	public DataTable Transfers { get; private set; } = new DataTable();
	#endregion

	#region Конструктор
	/// <summary>
	/// Конструктор контроллера по параметрам для подключения к базе данных.
	/// </summary>
	/// <param name="host"> хост </param>
	/// <param name="port"> порт </param>
	/// <param name="database"> название базы данных </param>
	/// <param name="username"> имя пользователя </param>
	/// <param name="password"> пароль </param>
	public Controller(string host, int port, string database, string username, string password) 
    {
        Connector = new Connector(host, port, database, username, password);
	}
	#endregion

	#region Получить все данные 
	/// <summary>
	/// Получить всех пользователей.
	/// </summary>
	/// <returns> таблица пользователей </returns>
	public DataTable GetPersons() 
    {
		Persons = Connector.ExecuteTable(
			"SELECT p.person_id AS \"ID пользователя\", " +
					"p.person_name AS \"Имя пользователя\", " +
					"CASE " +
						"WHEN SUM(t.amount) IS NULL THEN 0 " +
						"ELSE SUM(t.amount) " +
					"END AS \"Сумма всех переводов\" " +
			"FROM person p " +
			"LEFT JOIN transfer t " +
				"ON t.person_id = p.person_id " +
			"GROUP BY p.person_id");

		return Persons;
    }

	/// <summary>
	/// Получить все переводы.
	/// </summary>
	/// <returns> таблица переводов </returns>
    public DataTable GetTransfers() 
    {
		Transfers = Connector.ExecuteTable(
			"SELECT t.transfer_id AS \"ID перевода\", " +
					"p.person_name AS \"Имя пользователя\", " +
					"t.description AS \"Описание перевода\", " +
					"t.amount AS \"Сумма перевода\", " +
					"TO_CHAR(t.done_at, 'DD-MM-YYYY') AS \"Дата перевода\" " +
			"FROM transfer t " +
			"LEFT JOIN person p " +
				"ON p.person_id = t.person_id");

		return Transfers;
    }

	/// <summary>
	/// Получить все категории.
	/// </summary>
	/// <returns> таблица категорий </returns>
	public DataTable GetCategories() 
    {
		Categories = Connector.ExecuteTable("SELECT * FROM category");
		return Categories;

	}
	#endregion

	#region Получить переводы
	/// <summary>
	/// Получить переводы пользователя.
	/// </summary>
	/// <param name="personId"> id пользователя </param>
	/// <returns> таблица переводов </returns>
	public DataTable GetTransfersByPerson(int personId) 
    {
		NpgsqlParameter[] parameters = new NpgsqlParameter[]
		{
			new NpgsqlParameter("person_id", NpgsqlDbType.Integer) { Value = personId }
		};

		return Connector.ExecuteTable(
			"SELECT t.transfer_id AS \"ID перевода\", " +
					"p.person_name AS \"Имя пользователя\", " +
					"t.description AS \"Описание перевода\", " +
					"t.amount AS \"Сумма перевода\", " +
					"TO_CHAR(t.done_at, 'DD-MM-YYYY') AS \"Дата перевода\" " +
			"FROM transfer t " +
			"LEFT JOIN person p " +
				"ON p.person_id = t.person_id " +
            "WHERE p.person_id = @person_id", parameters);
    }

	/// <summary>
	/// Получить переводы за период.
	/// </summary>
	/// <param name="dateFrom"> дата начала </param>
	/// <param name="dateTo"> дата окончания </param>
	/// <returns> таблица переводов </returns>
	public DataTable GetTransfersPerPeriod(DateTime dateFrom, DateTime dateTo)
	{
		NpgsqlParameter[] parameters = new NpgsqlParameter[]
		{
			new NpgsqlParameter("dateFrom", NpgsqlDbType.Date) { Value = dateFrom },
			new NpgsqlParameter("dateTo", NpgsqlDbType.Date) { Value = dateTo }
		};

		return Connector.ExecuteTable(
			"SELECT t.transfer_id AS \"ID перевода\", " +
					"p.person_name AS \"Имя пользователя\", " +
					"t.description AS \"Описание перевода\", " +
					"t.amount AS \"Сумма перевода\", " +
					"TO_CHAR(t.done_at, 'DD-MM-YYYY') AS \"Дата перевода\" " +
			"FROM transfer t " +
			"LEFT JOIN person p " +
				"ON p.person_id = t.person_id " +
			"WHERE t.done_at >= @dateFrom AND t.done_at <= @dateTo", parameters);
	}

	/// <summary>
	/// Получить переводы по категории.
	/// </summary>
	/// <param name="categoryId"> id категории </param>
	/// <returns> таблица переводов </returns>
	public DataTable GetTransfersPerCategory(int categoryId)
	{
		NpgsqlParameter[] parameters = new NpgsqlParameter[]
		{
			new NpgsqlParameter("category_id", NpgsqlDbType.Integer) { Value = categoryId }
		};

		return Connector.ExecuteTable(
			"SELECT t.transfer_id AS \"ID перевода\", " +
					"p.person_name AS \"Имя пользователя\", " +
					"t.description AS \"Описание перевода\", " +
					"t.amount AS \"Сумма перевода\", " +
					"TO_CHAR(t.done_at, 'DD-MM-YYYY') AS \"Дата перевода\" " +
			"FROM transfer t " +
			"LEFT JOIN person p " +
				"ON p.person_id = t.person_id " +
			"LEFT JOIN category c " +
				"ON c.category_id = t.category_id " +
			"WHERE c.category_id = @category_id", parameters);
	}

	/// <summary>
	/// Получить переводы пользователя по категории.
	/// </summary>
	/// <param name="personId"> id пользователя </param>
	/// <param name="categoryId"> id категории </param>
	/// <returns> таблица переводов </returns>
	public DataTable GetTransfersPerPersonAndCategory(int personId, int categoryId)
	{
		NpgsqlParameter[] parameters = new NpgsqlParameter[]
		{
			new NpgsqlParameter("person_id", NpgsqlDbType.Integer) { Value = personId },
			new NpgsqlParameter("category_id", NpgsqlDbType.Integer) { Value = categoryId }
		};

		return Connector.ExecuteTable(
			"SELECT t.transfer_id AS \"ID перевода\", " +
					"p.person_name AS \"Имя пользователя\", " +
					"t.description AS \"Описание перевода\", " +
					"t.amount AS \"Сумма перевода\", " +
					"TO_CHAR(t.done_at, 'DD-MM-YYYY') AS \"Дата перевода\" " +
			"FROM transfer t " +
			"LEFT JOIN person p " +
				"ON p.person_id = t.person_id " +
			"LEFT JOIN category c " +
				"ON c.category_id = t.category_id " +
			"WHERE p.person_id = @person_id AND c.category_id = @category_id", parameters);
	}

	/// <summary>
	/// Получить переводы пользователя за период.
	/// </summary>
	/// <param name="dateFrom"> дата начала </param>
	/// <param name="dateTo"> дата окончания </param>
	/// <param name="id"> идентификатор </param>
	/// <returns> таблица переводов </returns>
	public DataTable GetTransferByPersonPerPeriod(DateTime dateFrom, DateTime dateTo, int id) 
    {
		NpgsqlParameter[] parameters = new NpgsqlParameter[]
		{
			new NpgsqlParameter("dateFrom", NpgsqlDbType.Date) { Value = dateFrom },
			new NpgsqlParameter("dateTo", NpgsqlDbType.Date) { Value = dateTo },
			new NpgsqlParameter("person_id", NpgsqlDbType.Integer) { Value = id }
		};

		return Connector.ExecuteTable(
			"SELECT t.transfer_id AS \"ID перевода\", " +
					"p.person_name AS \"Имя пользователя\", " +
					"t.description AS \"Описание перевода\", " +
					"t.amount AS \"Сумма перевода\", " +
					"TO_CHAR(t.done_at, 'DD-MM-YYYY') AS \"Дата перевода\" " +
			"FROM transfer t " +
			"LEFT JOIN person p " +
				"ON p.person_id = t.person_id " +
			"WHERE t.done_at >= @dateFrom AND t.done_at <= @dateTo AND p.person_id = @person_id", parameters);
	}

	/// <summary>
	/// Получить переводы по категории и дате.
	/// </summary>
	/// <param name="dateFrom"> дата начала </param>
	/// <param name="dateTo"> дата окончания </param>
	/// <param name="categoryId"> id категории </param>
	/// <returns> таблица переводов </returns>
	public DataTable GetTransfersPerCategoryAndPeriod(DateTime dateFrom, DateTime dateTo, int categoryId)
	{
		NpgsqlParameter[] parameters = new NpgsqlParameter[]
		{
			new NpgsqlParameter("dateFrom", NpgsqlDbType.Date) { Value = dateFrom },
			new NpgsqlParameter("dateTo", NpgsqlDbType.Date) { Value = dateTo },
			new NpgsqlParameter("category_id", NpgsqlDbType.Integer) { Value = categoryId }
		};

		return Connector.ExecuteTable(
			"SELECT t.transfer_id AS \"ID перевода\", " +
					"p.person_name AS \"Имя пользователя\", " +
					"t.description AS \"Описание перевода\", " +
					"t.amount AS \"Сумма перевода\", " +
					"TO_CHAR(t.done_at, 'DD-MM-YYYY') AS \"Дата перевода\" " +
			"FROM transfer t " +
			"LEFT JOIN person p " +
				"ON p.person_id = t.person_id " +
			"LEFT JOIN category c " +
				"ON c.category_id = t.category_id " +
			"WHERE t.done_at >= @dateFrom AND t.done_at <= @dateTo AND c.category_id = @category_id", parameters);
	}

	/// <summary>
	/// Получить переводы пользователя по дате и категории.
	/// </summary>
	/// <param name="dateFrom"> дата начала </param>
	/// <param name="dateTo"> дата окончания </param>
	/// <param name="personId"> id пользователя </param>
	/// <param name="categoryId"> id категории </param>
	/// <returns> таблица переводов </returns>
	public DataTable GetTransfersPerCategoryAndPeriodAndPerson(DateTime dateFrom, DateTime dateTo, int personId, int categoryId)
	{
		NpgsqlParameter[] parameters = new NpgsqlParameter[]
		{
			new NpgsqlParameter("dateFrom", NpgsqlDbType.Date) { Value = dateFrom },
			new NpgsqlParameter("dateTo", NpgsqlDbType.Date) { Value = dateTo },
			new NpgsqlParameter("person_id", NpgsqlDbType.Integer) { Value = personId },
			new NpgsqlParameter("category_id", NpgsqlDbType.Integer) { Value = categoryId }
		};

		return Connector.ExecuteTable(
			"SELECT t.transfer_id AS \"ID перевода\", " +
					"p.person_name AS \"Имя пользователя\", " +
					"t.description AS \"Описание перевода\", " +
					"t.amount AS \"Сумма перевода\", " +
					"TO_CHAR(t.done_at, 'DD-MM-YYYY') AS \"Дата перевода\" " +
			"FROM transfer t " +
			"LEFT JOIN person p " +
				"ON p.person_id = t.person_id " +
			"LEFT JOIN category c " +
				"ON c.category_id = t.category_id " +
			"WHERE t.done_at >= @dateFrom AND t.done_at <= @dateTo AND p.person_id = @person_id AND c.category_id = @category_id", parameters);
	}

	/// <summary>
	/// Получить переводы, сгруппированные по категории.
	/// </summary>
	/// <returns> таблица переводов </returns>
	public DataTable GetTransfersGroupByCategory() 
    {
        return Connector.ExecuteTable(
			"SELECT c.category_name AS \"Категория перевода\", " +
					"CASE " +
						"WHEN SUM(t.amount) IS NULL THEN 0 " +
						"ELSE SUM(t.amount) " +
					"END AS \"Сумма всех переводов по категории\" " +
			"FROM transfer t " +
			"LEFT JOIN person p " +
				"ON p.person_id = t.person_id " +
			"LEFT JOIN category c " +
				"ON c.category_id = t.category_id " +
			"GROUP BY c.category_name");
    }
	#endregion

	#region Добавить данные
	/// <summary>
	/// Добавить пользователя.
	/// </summary>
	/// <param name="personName"> имя пользователя </param>
	public void AddPerson(string personName) 
	{
		NpgsqlParameter[] parameters = new NpgsqlParameter[]
		{
			new NpgsqlParameter("person_name", NpgsqlDbType.Varchar) { Value = personName }
		};

		Connector.ExecuteAction($"INSERT INTO person (person_name) VALUES (@person_name)", parameters);
    }

	/// <summary>
	/// Добавить категорию.
	/// </summary>
	/// <param name="categoryName"> название категории </param>
	public void AddCategory(string categoryName)
	{
		NpgsqlParameter[] parameters = new NpgsqlParameter[]
		{
			new NpgsqlParameter("category_name", NpgsqlDbType.Varchar) { Value = categoryName }
		};

		Connector.ExecuteAction($"INSERT INTO category (category_name) VALUES (@category_name)", parameters);
	}

	/// <summary>
	/// Добавить перевод.
	/// </summary>
	/// <param name="personId"> id пользователя </param>
	/// <param name="categoryId"> id категории </param>
	/// <param name="description"> описание </param>
	/// <param name="amount"> сумма </param>
	/// <param name="date"> дата </param>
	public void AddTransfer(int personId, int categoryId, string description, double amount, DateTime date) 
	{
		NpgsqlParameter[] parameters = new NpgsqlParameter[]
		{
			new NpgsqlParameter("person_id", NpgsqlDbType.Integer) { Value = personId },
			new NpgsqlParameter("category_id", NpgsqlDbType.Integer) { Value = categoryId },
			new NpgsqlParameter("description", NpgsqlDbType.Varchar) { Value = description },
			new NpgsqlParameter("amount", NpgsqlDbType.Double) { Value = amount },
			new NpgsqlParameter("date", NpgsqlDbType.Date) { Value = date }
		};

		Connector.ExecuteAction($"INSERT INTO transfer (person_id, category_id, description, amount, done_at) " +
								$"VALUES (@person_id, @category_id, @description, @amount, @date)", parameters);
    }
	#endregion

	#region Удалить данные
	/// <summary>
	/// Удалить пользователей.
	/// </summary>
	/// <param name="personIds"> id пользователей </param>
	public void RemovePersons(IEnumerable<int> personIds) 
	{
		foreach (int personId in personIds)
		{
			NpgsqlParameter[] parameters = new NpgsqlParameter[]
			{
				new NpgsqlParameter("person_id", NpgsqlDbType.Integer) { Value = personId }
			};
			Connector.ExecuteAction($"DELETE FROM person WHERE person_id = @person_id", parameters);
		}
    }

	/// <summary>
	/// Удалить переводы.
	/// </summary>
	/// <param name="transferIds"> id переводов </param>
	public void RemoveTransfers(IEnumerable<int> transferIds) 
	{
		foreach (int transferId in transferIds)
		{
			NpgsqlParameter[] parameters = new NpgsqlParameter[]
			{
				new NpgsqlParameter("transfer_id", NpgsqlDbType.Integer) { Value = transferId }
			};
			Connector.ExecuteAction($"DELETE FROM transfer WHERE transfer_id = @transfer_id", parameters);
		}
	}

	/// <summary>
	/// Удалить категорию.
	/// </summary>
	/// <param name="categoryName"> название категории </param>
	public void RemoveCategory(string categoryName)
	{
		NpgsqlParameter[] parameters = new NpgsqlParameter[]
		{
			new NpgsqlParameter("category_name", NpgsqlDbType.Varchar) { Value = categoryName }
		};
		Connector.ExecuteAction($"DELETE FROM category WHERE category_name = @category_name", parameters);
	}
	#endregion

	#region Создать таблицы
	/// <summary>
	/// Создать таблицы.
	/// </summary>
	public void CreateTables() 
	{
        Connector.CreateTables();
    }
	#endregion
}
