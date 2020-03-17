using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRD_GenerateData
{
	class GenOrders
	{
		Connection connect;
		List<int> idPersonals = new List<int>();
		List<DateTime> createDates = new List<DateTime>();
		List<int> positionsId = new List<int>();

		Random rand = new Random();

		int limitPersinal = 128;
		int offsetPersonal = 0;

		public GenOrders(Connection conn)
		{
			connect = conn;
			get_personal();
			get_positions();
		}


		public void execute()
		{
			int countPers = createDates.Count;
			DateTime curDate = createDates[0];
			for (int i = 0; i < countPers;)
			{
				List<int> curPers = new List<int>();
				while (i < countPers && createDates[i] == curDate)
				{
					curPers.Add(i);
					i++;
				}

				//Создать приказы для сотрудников, принятых на дату curDate
				create_order(curDate, curPers);


				if (i < countPers)
					curDate = createDates[i];
			}
		}


		private void create_order (DateTime date, List<int> pers)
		{
			GenHandbooks gh = new GenHandbooks(connect);

			string strComIns = "insert into \"Order\" (\"nomer\", \"data_order\", \"pk_type_order\") " +
					"values ('" + rand.Next(1000000, 10000000) + "', '" +
					date.Year + "-" + date.Month + "-" + date.Day + "', '" + 
					gh.get_id_type_order(GenHandbooks.recruitment) + //Тип приказа: приём
					"') RETURNING \"pk_order\"";

			NpgsqlCommand command = new NpgsqlCommand(strComIns, connect.get_connect());

			NpgsqlDataReader reader = command.ExecuteReader();
			int idOrder = -1;
			foreach (DbDataRecord rec in reader)
				idOrder = rec.GetInt32(0);
			reader.Close();
			

			//Создать строки приказа для каждого сотрудника
			foreach (int p in pers)
			{
				string numberWork = rand.Next(1000, 9999).ToString() + "-" + rand.Next(1000, 9999).ToString();

				strComIns = "insert into \"String_order\" " +
					"(\"pk_order\", \"Move_date\", \"Number_work_doc\", \"Work_doc_date\",  \"Reason\") " +
					"values ('" + idOrder + "', '" +
					date.Year + "-" + date.Month + "-" + date.Day + "', '" +
					numberWork + "', '" +
					date.Year + "-" + date.Month + "-" + date.Day + 
					"', '') RETURNING \"pk_string_order\"";

				command = new NpgsqlCommand(strComIns, connect.get_connect());
				NpgsqlDataReader reader2 = command.ExecuteReader();
				int idCurString = -1;
				foreach (DbDataRecord rec in reader2)
					idCurString = rec.GetInt32(0);
				reader2.Close();

				strComIns = "insert into \"PeriodPosition\" " +
					"(\"DataFrom\", \"pk_position\", \"pk_personal_card\",  \"pk_move_order\") " +
					"values ('" +
					date.Year + "-" + date.Month + "-" + date.Day + "', '" +
					positionsId[rand.Next(0, positionsId.Count)] + "', '" +		//Случайная должность
					idPersonals[p] + "', '" +
					idCurString + "')";

				command = new NpgsqlCommand(strComIns, connect.get_connect());
				int count = command.ExecuteNonQuery();
			}
		}


		public void clear()
		{
			string strForCom = "delete from \"PeriodPosition\"";
			NpgsqlCommand command = new NpgsqlCommand(strForCom, connect.get_connect());
			command.ExecuteNonQuery();

			strForCom = "delete from \"String_order\"";
			command = new NpgsqlCommand(strForCom, connect.get_connect());
			command.ExecuteNonQuery();

			strForCom = "delete from \"Order\"";
			command = new NpgsqlCommand(strForCom,connect.get_connect());
			command.ExecuteNonQuery();


		}

		//Получение id сотрудников и дат создания их карточек
		private void get_personal()
		{
			string strCom = "select \"pk_personal_card\", \"Creation_date\" from \"PersonalCard\" order by \"Creation_date\"" +
				" limit " + limitPersinal + " offset " + offsetPersonal;
			NpgsqlCommand command = new NpgsqlCommand(strCom, connect.get_connect());

			NpgsqlDataReader reader = command.ExecuteReader();
			if (reader.HasRows)
				foreach (DbDataRecord rec in reader)
				{
					idPersonals.Add(rec.GetInt32(0));
					createDates.Add((DateTime)rec.GetValue(1));
					//Console.Write((GenTimeTracking.curDate - (DateTime)rec.GetValue(1)).TotalDays / 30 + "\n");
				}
			reader.Close();
		}


		//Получение id должностей
		private void get_positions()
		{
			string strCom = "select \"pk_position\" from \"Position\"";
			NpgsqlCommand command = new NpgsqlCommand(strCom, connect.get_connect());

			NpgsqlDataReader reader = command.ExecuteReader();
			if (reader.HasRows)
				foreach (DbDataRecord rec in reader)
					positionsId.Add(rec.GetInt32(0));
			reader.Close();
		}
	}
}
