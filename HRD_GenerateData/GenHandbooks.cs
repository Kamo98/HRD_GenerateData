using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HRD_GenerateData
{
	class GenHandbooks
	{

		Connection connect;

		private string[] orders = new string[]
		{
			"Приём",
			"Перевод",
			"Увольнение"
		};
		
		public GenHandbooks(Connection conn)
		{
			connect = conn;
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
