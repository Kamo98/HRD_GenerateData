using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
			//NpgsqlCommand command = new NpgsqlCommand(
			//	"insert into \"CategoryMilitary\" (\"Name\", \"Code\") values ('Б-1', 'Б-1')", 
			//	connect.get_connect());

			//int count = command.ExecuteNonQuery();

			//if (count == 1)
			//	Console.Out.Write("Строка вставлена\n");
			//else
			//	Console.Out.Write("Строка НЕ вставлена\n");
		}


		public void ecec_read()
		{

			//Запрос должностей
			//string strCom = "select * from \"Unit\" u, \"Position\" p where p.\"pk_unit\" = 2 and u.\"pk_unit\" = p.\"pk_unit\"";

			//string strCom = "select \"pk_personal_card\", \"birthday\" from \"PersonalCard\"";
			//string strCom = "select \"Creation_date\" from \"PersonalCard\" group by \"Creation_date\" order by \"Creation_date\"";
			//string strCom = "select * from \"Position\"";
			//string strCom = "select pg_get_serial_sequence('Order', 'pk_order');";
			//string strCom = "select pg_get_serial_sequence(PersonalCard, pk_personal_card)";
			//string strCom = "select currval(PersonalCard_pk_personal_card_seq)";

			//Вывод строк приказа
			string strCom = "select o.\"nomer\", s.\"Move_date\", s.\"Number_work_doc\", d.\"Name\" from \"String_order\" s, \"Order\" o, \"PeriodPosition\" p, \"Position\" d where p.\"pk_move_order\" = s.\"pk_string_order\" AND s.\"pk_order\" = o.\"pk_order\" AND d.\"pk_position\" = p.\"pk_position\"";
			
			NpgsqlCommand command = new NpgsqlCommand(strCom,connect.get_connect());


			NpgsqlDataReader reader = command.ExecuteReader();

			if (reader.HasRows)
			{
				foreach (DbDataRecord rec in reader)
				{
					object[] obj = new object[rec.FieldCount];
					rec.GetValues(obj);

					foreach (object o in obj)
						Console.Write(o.ToString().Trim() + "   ");
					Console.Write("\n");
				}
			}
			reader.Close();
		}

		public void get_all_tables()
		{

			NpgsqlCommand command = new NpgsqlCommand(
				"select table_name, column_name from information_schema.columns where table_schema = 'public'",
				connect.get_connect());

			NpgsqlDataReader reader = command.ExecuteReader();

			StreamWriter sw = new StreamWriter("tables.txt");

			if (reader.HasRows)
			{
				foreach (DbDataRecord rec in reader)
				{
					object[] obj = new object[rec.FieldCount];
					rec.GetValues(obj);

					foreach (object o in obj)
						sw.Write(o.ToString() + "\t");
					sw.Write("\n");
				}
			}

			sw.Close();
			reader.Close();
		}

		public void correct_db ()
		{
			//string strForCom = "CREATE TABLE \"Characteristic\"(" +
			//"\"pk_characteristic\" Serial NOT NULL," +
			//"\"date\" Date NOT NULL," +
			//"\"characteristic\" Character(10000)," +
			//"\"fileReference\" Character varying(255)," +
			//"\"pk_personal_card\" Integer)";

			//string strForCom = "CREATE INDEX \"IX_Relationship121\" ON \"Characteristic\" (\"pk_personal_card\")";
			//string strForCom = "ALTER TABLE \"PersonalCard\" ALTER COLUMN \"Serial_number\" TYPE Character varying(12);";
			//string strForCom = "ALTER TABLE \"Characteristic\" ADD CONSTRAINT \"Relationship121\" FOREIGN KEY (\"pk_personal_card\") REFERENCES \"PersonalCard\" (\"pk_personal_card\") ON DELETE RESTRICT ON UPDATE RESTRICT";
			//string strForCom = "ALTER TABLE \"Characteristic\" ADD CONSTRAINT \"PK_Characteristic\" PRIMARY KEY (\"pk_characteristic\")";
			string strForCom = "";
			//string strForCom = "";

			NpgsqlCommand command = new NpgsqlCommand(
				strForCom,
				connect.get_connect());

			int count = command.ExecuteNonQuery();
			if (count == 1)
				Console.Out.Write("Успешно\n");
			else
				Console.Out.Write("Ошибка\n");
		}
	}
}
