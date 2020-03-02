using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRD_GenerateData
{
	class Connection
	{
		private NpgsqlConnection npgSqlConnection;
		private static Connection connect = null;

		private Connection (string login, string pass)
		{
			string connectionString = "Server = hrd.cx7kyl76gv42.us-east-2.rds.amazonaws.com; DataBase = HRD; Integrated Security = false; User Id = " + login + "; password = " + pass;

			//Создание соединения с БД
			npgSqlConnection = new NpgsqlConnection(connectionString);

			try
			{
				npgSqlConnection.Open();		//Открываем соединение
			}
			catch (NpgsqlException e)
			{
				npgSqlConnection = null;
				Console.Out.Write("Подключение НЕ выполнено\n" + e.Message + "\n");
			}
		}
		

		public NpgsqlConnection get_connect()
		{
			return npgSqlConnection;
		}

		public static Connection get_instance(string login, string pass)
		{
			if (connect == null)
				connect = new Connection(login, pass);
			return connect;
			
		}

		public void close_connection ()
		{
			npgSqlConnection.Close();
		}
		
	}
}
