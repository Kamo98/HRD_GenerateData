using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.Common;

namespace HRD_GenerateData
{
	class GenHandbooks
	{

		Connection connect;

		public static string recruitment = "Приём";
		public static string dismissal = "Увольнение";
		public static string moveWork = "Перевод";


		private string[] orders = new string[]
		{
			recruitment,
			dismissal,
			moveWork
		};
		
		public GenHandbooks(Connection conn)
		{
			connect = conn;
		}


		public int get_id_type_order(string type)
		{
			string strCom = "select \"pk_type_order\" from \"TypeOrder\" where \"Name\" = '" + type + "'";

			NpgsqlCommand command = new NpgsqlCommand(strCom, connect.get_connect());
			NpgsqlDataReader reader = command.ExecuteReader();

			int id = 0;
			foreach (DbDataRecord rec in reader)
				id = rec.GetInt32(0);

			reader.Close();

			return id;
		}

		public void addMarkTimeTracking ()
		{
			StreamReader sr = new StreamReader("markTimeTracking.txt");
			string line = "";
			while ((line = sr.ReadLine()) != "#")
			{
				string[] arr = line.Split('|');

				string strComIns = "insert into \"MarkTimeTracking\" (\"Name\", \"ShortName\") " +
					"values ('" + arr[0].Trim() + "', '" + arr[1].Trim() + "')";

				NpgsqlCommand command = new NpgsqlCommand(strComIns, connect.get_connect());

				int count = command.ExecuteNonQuery();
				if (count == 1)
					Console.Out.Write("Строка вставлена\n");
				else
					Console.Out.Write("Строка НЕ вставлена\n");
			}
		}


		public void addTypeOrders()
		{
			foreach (string ord in orders)
			{
				string strComIns = "insert into \"TypeOrder\" (\"Name\") values ('" + ord + "')";

				NpgsqlCommand command = new NpgsqlCommand(strComIns, connect.get_connect());

				int count = command.ExecuteNonQuery();
				if (count == 1)
					Console.Out.Write("Строка вставлена\n");
				else
					Console.Out.Write("Строка НЕ вставлена\n");
			}
		}
	}
}
