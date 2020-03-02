using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRD_GenerateData
{
	class Test
	{
		Connection connect;
		public Test (Connection conn)
		{
			connect = conn;
		}

		public void execute()
		{
			NpgsqlCommand command = new NpgsqlCommand(
				"insert into \"CategoryMilitary\" (\"Name\", \"Code\") values ('Б-1', 'Б-1')", 
				connect.get_connect());

			int count = command.ExecuteNonQuery();

			if (count == 1)
				Console.Out.Write("Строка вставлена\n");
			else
				Console.Out.Write("Строка НЕ вставлена\n");
		}
	}
}
