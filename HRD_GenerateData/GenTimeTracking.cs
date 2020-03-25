using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRD_GenerateData
{
	enum CharactPers
	{
		Bad,
		Good,
		Sick
	}

	class GenTimeTracking
	{
		public static DateTime curDate = new DateTime(2019, 12, 20);

		Connection connect;
		List<int> idPersonals = new List<int>();        //id сотрудника 
		List<int> pers2Unit = new List<int>();			//id подразделения
		List<DateTime> createDates = new List<DateTime>();
		List<int> idUnits = new List<int>();
		List<CharactPers> charactPers = new List<CharactPers>();


		//Для выборки шифров явки/неявки и ключей
		Dictionary<string, int> markTT2key = new Dictionary<string, int>();

		//Для ограничения выборки сотрудников
		int limitPersinal = 10;
		int offsetPersonal = 0;

		public GenTimeTracking(Connection conn)
		{
			connect = conn;
			get_personal();
			get_mark_tt();
			get_id_units();
		}

		//Получение id сотрудников и дат создания их карточек, а также подразделений, в которых они работают
		private void get_personal()
		{
			string strCom = "select \"pk_personal_card\", \"Creation_date\" from \"PersonalCard\" " + "order by \"Creation_date\"" +
				" limit " + limitPersinal + " offset " + offsetPersonal;
			NpgsqlCommand command = new NpgsqlCommand(strCom, connect.get_connect());

			Random rand = new Random();
			NpgsqlDataReader reader = command.ExecuteReader();
			if (reader.HasRows)
				foreach (DbDataRecord rec in reader)
				{
					CharactPers curCharact = CharactPers.Good;
					int r = rand.Next(1, 11);
					if (r <= 3)
						curCharact = CharactPers.Bad;
					else if (r >= 9)
						curCharact = CharactPers.Sick;

					charactPers.Add(curCharact);

					idPersonals.Add(rec.GetInt32(0));
					createDates.Add((DateTime)rec.GetValue(1));
					//Console.Write((curDate - (DateTime)rec.GetValue(1)).TotalDays / 30 + "\n");
				}
			reader.Close();

			foreach (int idP in idPersonals)
			{
				strCom = "select u.\"pk_unit\" from \"PeriodPosition\" p, \"Position\" d, \"Unit\" u " +
				"where p.\"pk_personal_card\" = '" + idP + "' AND p.\"DateTo\" IS NULL AND p.\"pk_position\" = d.\"pk_position\" AND d.\"pk_unit\" = u.\"pk_unit\"";

				command = new NpgsqlCommand(strCom, connect.get_connect());
				reader = command.ExecuteReader();
				if (reader.HasRows)
					foreach (DbDataRecord rec in reader)
					{
						pers2Unit.Add(rec.GetInt32(0));
					}
				reader.Close();
			}

			
		}

		//Получение id и шифров отметок о явке/неявке
		private void get_mark_tt()
		{
			string strCom = "select \"pk_mark_time_tracking\", \"ShortName\" from \"MarkTimeTracking\"";
			NpgsqlCommand command = new NpgsqlCommand(strCom, connect.get_connect());

			NpgsqlDataReader reader = command.ExecuteReader();
			if (reader.HasRows)
				foreach (DbDataRecord rec in reader)
					markTT2key[rec.GetString(1)] = rec.GetInt32(0);
			reader.Close();
		}

		//получить список id всех подразделений
		private void get_id_units()
		{
			string strCom = "select \"pk_unit\" from \"Unit\"";
			NpgsqlCommand command = new NpgsqlCommand(strCom, connect.get_connect());

			NpgsqlDataReader reader = command.ExecuteReader();
			if (reader.HasRows)
				foreach (DbDataRecord rec in reader)
					idUnits.Add(rec.GetInt32(0));
			reader.Close();
		}


		public void clear()
		{
			string strForCom;
			NpgsqlCommand command;			

			strForCom = "delete from \"Fact\"";
			command = new NpgsqlCommand(strForCom, connect.get_connect());
			command.ExecuteNonQuery();

			strForCom = "delete from \"StringTimeTracking\"";
			command = new NpgsqlCommand(strForCom, connect.get_connect());
			command.ExecuteNonQuery();

			strForCom = "delete from \"TimeTracking\"";
			command = new NpgsqlCommand(strForCom, connect.get_connect());
			command.ExecuteNonQuery();
		}

		private DateTime next_month(DateTime date)
		{
			DateTime dateRes;
			if (date.Month == 12)
				dateRes = new DateTime(date.Year + 1, 1, 1);          //Первый день месяца
			else
				dateRes = new DateTime(date.Year, date.Month + 1, 1); //Первый день месяца
			return dateRes;
		}

		public void execute()
		{
			bool writeToDb = true;
			bool writeToFile = true;
			StreamWriter sw = new StreamWriter("timeTracking.txt");

			Random rand = new Random();
			int countPers = createDates.Count;
			DateTime date = new DateTime(createDates[0].Year, createDates[0].Month, 1);
			List<KeyValuePair<int, bool>> persCurWork = new List<KeyValuePair<int, bool>>();        //Сотрудники который на дату date уже работают
			HashSet<int> curUnits = new HashSet<int>();			//id подразделений, шапки табеля для которых сейчас необходимы

			int indPers = 0;


			//Сгенерировать шапки табелей начиная с самого первого принятого сотрудника и до curDate
			for (; date <= curDate; date = next_month(date))
			{
				DateTime dateFrom = new DateTime(date.Year, date.Month, 1);
				DateTime dateTo = next_month(date) - new TimeSpan(1, 0, 0, 0);


				Dictionary<int, int> unit2idTT = new Dictionary<int, int>();    //Для хранения id шапок текущего месяца

				//Добавляем в общий список сотрудников, которые начали работу в текущем месяце
				while (indPers < countPers && createDates[indPers] < dateTo)
				{
					persCurWork.Add(new KeyValuePair<int, bool>(indPers, false));
					curUnits.Add(pers2Unit[indPers]);
					indPers++;
				}

				//Для каждого подразделения создаётся отдельная шапка табеля
				foreach (int unit in curUnits)
				{
					string queryIns = "insert into \"TimeTracking\" " +
					"(\"nomer\", " +
					"\"date_sostav\", " +
					"\"from\"," +
					" \"to\"," +
					" \"pk_unit\")" +
					" values ('" +
					rand.Next(10000000, 100000000).ToString() + "', '" +
					dateTo.Year + "-" + dateTo.Month + "-" + dateTo.Day + "', '" +
					dateFrom.Year + "-" + dateFrom.Month + "-" + dateFrom.Day + "', '" +
					dateTo.Year + "-" + dateTo.Month + "-" + dateTo.Day + "', '" +
					unit + "') RETURNING \"pk_time_tracking\"";

					int idTT = -1;
					if (writeToDb)
					{
						NpgsqlCommand command = new NpgsqlCommand(queryIns, connect.get_connect());
						NpgsqlDataReader reader = command.ExecuteReader();
						foreach (DbDataRecord rec in reader)
							idTT = rec.GetInt32(0);
						reader.Close();
					}
					unit2idTT[unit] = idTT;     //Сопоставить подразделение с шапкой

					if (writeToFile)
						sw.Write(queryIns + "\n");
				}

				

				
				//Пройтись по списку сотрудников, работающих на данный момент и создать для них строки в табеле
				for (int i = 0; i < persCurWork.Count; i++)
				{
					//pers.Value == false, значит нужно заполнять не с начала месяца, а с даты приёма сотрудника
					KeyValuePair<int, bool> pers = persCurWork[i];
					int p = pers.Key;

					string queryIns = "insert into \"StringTimeTracking\" " +
					"(\"pk_personal_card\", " +
					"\"pk_time_tracking\"" +
					") values ('" + 
					idPersonals[p] + "', '" +
					unit2idTT[pers2Unit[p]] +
					"') RETURNING \"pk_string_time_tracking\"";

					int idString = -1;
					if (writeToDb)
					{
						NpgsqlCommand command = new NpgsqlCommand(queryIns, connect.get_connect());
						NpgsqlDataReader reader = command.ExecuteReader();
						foreach (DbDataRecord rec in reader)
							idString = rec.GetInt32(0);
						reader.Close();
					}

					if (writeToFile)
						sw.Write("\t" + queryIns + "\n");

					gen_str_pers(idString, p, pers.Value == true ? dateFrom : createDates[p], dateTo, writeToDb, writeToFile, sw);
					persCurWork[i] = new KeyValuePair<int, bool>(persCurWork[i].Key, true);
				}

			}
		}

		//private List<int> work


		private void gen_str_pers(int idString, int indPers, DateTime dateFrom, DateTime dateTo, bool writeToBd, bool writeToFile, StreamWriter sw)
		{
			Random rand = new Random();

			TimeSpan oneDay = new TimeSpan(1, 0, 0, 0);
			for (DateTime d = dateFrom; d <= dateTo; d += oneDay)
			{
				int mark = markTT2key["Я"];
				int countHours = 8;

				if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
				{
					mark = markTT2key["В"];
					countHours = 0;
				}
				else
				{
					int k = rand.Next(1, 11);
					switch (charactPers[indPers])
					{
						case CharactPers.Bad:
							if (k == 3)
							{
								mark = markTT2key["Б"];
								countHours = 0;
							}
							else if (k <= 2)
							{
								mark = markTT2key["ПР"];
								countHours = 0;
							} else if (k >= 9)
							{
								mark = markTT2key["НН"];
								countHours = 0;
							} else if (k >= 7)
							{
								mark = markTT2key["Я"];
								countHours = rand.Next(5, 8);
							}
							break;
						case CharactPers.Good:
							if (k == 3)
							{
								mark = markTT2key["Б"];
								countHours = 0;
							}
							else if (k >= 9)
							{
								mark = markTT2key["Я"];
								countHours = rand.Next(7, 10);
							} else if (k <= 2)
							{
								mark = markTT2key["Н"];
								countHours = rand.Next(3, 8);
							}
							break;
						case CharactPers.Sick:
							if (k == 1)
							{
								mark = markTT2key["ПР"];
								countHours = 0;
							}
							else if (k >= 7)
							{
								mark = markTT2key["Б"];
								countHours = 0;
							}							
							break;
					}
				}

				string queryIns = "insert into \"Fact\" (" +
						"\"pk_string_time_tracking\", " +
						"\"pk_mark_time_tracking\", " +
						"\"data\", " +
						"\"count_of_hours\"" +
						") values ('" +
						idString + "', '" +
						mark + "', '" +
						d.Year + "-" + d.Month + "-" + d.Day + "', '" +
						countHours + "')";

				if (writeToBd)
				{
					NpgsqlCommand command = new NpgsqlCommand(queryIns, connect.get_connect());
					if (command.ExecuteNonQuery() == 0)
						Console.Write("ERROR!\n");
				}
				if (writeToFile)
					sw.Write("\t\t" + queryIns + "\n");
					
			}
		}

	}
}
