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
			StreamWriter sw = new StreamWriter("out.txt");

			//Запрос должностей
			//string strCom = "select * from \"Unit\" u, \"Position\" p where p.\"pk_unit\" = '12' and u.\"pk_unit\" = p.\"pk_unit\"";

			//string strCom = "select \"pk_personal_card\", \"Creation_date\" from \"PersonalCard\"";

			///

			//string strCom = "select \"pk_personal_card\", \"DataFrom\", \"DateTo\", \"pk_move_order\", t.\"Name\", \"pk_fire_order_string\", d.\"Name\", d.\"pk_unit\", u.\"Name\" " +
			//	" from \"PeriodPosition\" p, \"Position\" d, \"String_order\" s, \"Order\" o, \"TypeOrder\" t, \"Unit\" u" +
			//	" where d.\"pk_position\" = p.\"pk_position\" and p.\"pk_move_order\" = s.\"pk_string_order\" and o.\"pk_order\" = s.\"pk_order\" " +
			//	" and t.\"pk_type_order\" = o.\"pk_type_order\" and d.\"pk_unit\" = u.\"pk_unit\" and \"pk_personal_card\" = '1342'";

			string strCom = "select c.\"surname\", c.\"name\", c.\"otchestvo\", \"DataFrom\", \"DateTo\",  t.\"Name\", d.\"Name\", u.\"Name\" " +
				" from \"PeriodPosition\" p, \"Position\" d, \"String_order\" s, \"Order\" o, \"TypeOrder\" t, \"Unit\" u, \"PersonalCard\" c" +
				" where c.\"pk_personal_card\" = p.\"pk_personal_card\" and d.\"pk_position\" = p.\"pk_position\" and p.\"pk_move_order\" = s.\"pk_string_order\" and o.\"pk_order\" = s.\"pk_order\" " +
				" and t.\"pk_type_order\" = o.\"pk_type_order\" and d.\"pk_unit\" = u.\"pk_unit\" and p.\"pk_personal_card\" = '1342'";


			//string strCom = "select * from \"pg_user\"";

			//string strCom = "select c.\"pk_personal_card\", c.\"surname\", c.\"name\", c.\"otchestvo\", p.\"pk_fire_order_string\", o.\"data_order\"" +
			//	" from \"PeriodPosition\" p, \"PersonalCard\" c, \"String_order\" s, \"Order\" o" +
			//	" where p.\"pk_personal_card\" = c.\"pk_personal_card\" and p.\"pk_fire_order_string\" = s.\"pk_string_order\" " +
			//	" and o.\"pk_order\" = s.\"pk_order\" and p.\"pk_fire_order_string\" IS NOT NULL";

			//string strCom = "select \"Creation_date\" from \"PersonalCard\" group by \"Creation_date\" order by \"Creation_date\"";
			//string strCom = "select \"nomer\", \"data_order\", \"Name\" from \"Order\" o, \"TypeOrder\" t where o.\"pk_type_order\" = t.\"pk_type_order\"";
			//string strCom = "select * from \"String_order\" where \"pk_order\"=100";
			//string strCom = "select pg_get_serial_sequence('Order', 'pk_order');";
			//string strCom = "select pg_get_serial_sequence(PersonalCard, pk_personal_card)";
			//string strCom = "select currval(PersonalCard_pk_personal_card_seq)";

			//Вывод строк приказа
			//string strCom = "select o.\"nomer\", s.\"Move_date\", s.\"Number_work_doc\", d.\"Name\" from \"String_order\" s, \"Order\" o, \"PeriodPosition\" p, \"Position\" d where p.\"pk_move_order\" = s.\"pk_string_order\" AND s.\"pk_order\" = o.\"pk_order\" AND d.\"pk_position\" = p.\"pk_position\"";
			//string strCom = "select * from \"pg_shadow\"";

			//string strCom = "select \"role_name\" from \"information_schema\".\"applicable_roles\" where \"grantee\" = 'accounting1'";

			//string strCom = "select * from \"TypeOrder\"";

			//string strCom = "select count(o.\"pk_order\")" +
			//	" from \"Order\" o, \"TypeOrder\" t" +
			//	" where o.\"pk_type_order\" = t.\"pk_type_order\" and o.\"pk_type_order\" = '1'" +
			//	"  and o.\"data_order\" >= '2018-1-1' and o.\"data_order\" <= '2018-1-31'";



			NpgsqlCommand command = new NpgsqlCommand(strCom,connect.get_connect());


			NpgsqlDataReader reader = command.ExecuteReader();

			if (reader.HasRows)
			{
				foreach (DbDataRecord rec in reader)
				{
					object[] obj = new object[rec.FieldCount];
					rec.GetValues(obj);

					foreach (object o in obj)
					{
						Console.Write(o.ToString().Trim() + "   ");
						sw.Write(o.ToString().Trim() + "   ");
					}
					Console.Write("\n");
					sw.Write("\n");
				}
			}
			reader.Close();
			sw.Close();
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


			//string strForCom = "CREATE ROLE accounting IN ROLE postgres";
			//string strForCom = "CREATE USER accounting1 WITH PASSWORD 'accounting1' IN ROLE accounting";
			//string strForCom = "ALTER ROLE accounting RENAME TO accounting_old";
			string strForCom = "GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO \"reception\"";


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
