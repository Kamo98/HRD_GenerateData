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
		//Dictionary<int, int> pers2Unit = new Dictionary<int, int>();			//id подразделения
		List<DateTime> createDates = new List<DateTime>();
		//List<int> idUnits = new List<int>();
		Dictionary<int, CharactPers> charactPers = new Dictionary<int, CharactPers>();


		//Для выборки шифров явки/неявки и ключей
		Dictionary<string, int> markTT2key = new Dictionary<string, int>();

		//Для ограничения выборки сотрудников
		int limitPersinal = 128;
		int offsetPersonal = 0;

		public GenTimeTracking(Connection conn)
		{
			connect = conn;
			get_personal();
			get_mark_tt();
			//get_id_units();
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

					int id = rec.GetInt32(0);
					charactPers.Add(id, curCharact);

					idPersonals.Add(id);
					createDates.Add((DateTime)rec.GetValue(1));
					//Console.Write((curDate - (DateTime)rec.GetValue(1)).TotalDays / 30 + "\n");
				}
			reader.Close();

			//foreach (int idP in idPersonals)
			//{
			//	strCom = "select u.\"pk_unit\" from \"PeriodPosition\" p, \"Position\" d, \"Unit\" u " +
			//	"where p.\"pk_personal_card\" = '" + idP + "' AND p.\"DateTo\" IS NULL AND p.\"pk_position\" = d.\"pk_position\" AND d.\"pk_unit\" = u.\"pk_unit\"";

			//	command = new NpgsqlCommand(strCom, connect.get_connect());
			//	reader = command.ExecuteReader();
			//	if (reader.HasRows)
			//		foreach (DbDataRecord rec in reader)
			//		{
			//			pers2Unit.Add(idP, rec.GetInt32(0));
			//		}
			//	reader.Close();
			//}

			
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
		//private void get_id_units()
		//{
		//	string strCom = "select \"pk_unit\" from \"Unit\"";
		//	NpgsqlCommand command = new NpgsqlCommand(strCom, connect.get_connect());

		//	NpgsqlDataReader reader = command.ExecuteReader();
		//	if (reader.HasRows)
		//		foreach (DbDataRecord rec in reader)
		//			idUnits.Add(rec.GetInt32(0));
		//	reader.Close();
		//}


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


			DateTime endDate = new DateTime(2018, 8, 30);

			//Сгенерировать табели начиная с самого первого принятого сотрудника и до curDate
			for (; date <= endDate; date = next_month(date))
			{
				DateTime dateFrom = new DateTime(date.Year, date.Month, 1);
				DateTime dateTo = next_month(date) - new TimeSpan(1, 0, 0, 0);

				Dictionary<int, int> unit2idTT = new Dictionary<int, int>();    //Для хранения id шапок текущего месяца


				Dictionary<int, Dictionary<int, KeyValuePair<DateTime, DateTime>>> persAndUnits = get_pers_period(dateFrom, dateTo);


				//Пройти по всем сотрудникам, работавшим в этот период
				foreach (KeyValuePair<int, Dictionary<int, KeyValuePair<DateTime, DateTime>>> pr in persAndUnits)
				{
					int pers = pr.Key;
					//Пройти по всем подразделениям, в которых они работил в этот период
					foreach (KeyValuePair<int, KeyValuePair<DateTime, DateTime>> pr2 in pr.Value)
					{
						dateFrom = new DateTime(date.Year, date.Month, 1);
						dateTo = next_month(date) - new TimeSpan(1, 0, 0, 0);

						int unit = pr2.Key;

						//Проверим, есть ли уже шапка для этого подразделения
						if (!unit2idTT.ContainsKey(unit))
						{
							//Если нет, то создадим
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
							unit2idTT.Add(unit, idTT);     //Сопоставить подразделение с шапкой
						}

						//Создадть строку для сотрудника в табеле

						string queryIns1 = "insert into \"StringTimeTracking\" " +
						"(\"pk_personal_card\", " +
						"\"pk_time_tracking\"" +
						") values ('" +
						pers + "', '" +
						unit2idTT[unit] +
						"') RETURNING \"pk_string_time_tracking\"";

						int idString = -1;
						if (writeToDb)
						{
							NpgsqlCommand command = new NpgsqlCommand(queryIns1, connect.get_connect());
							NpgsqlDataReader reader = command.ExecuteReader();
							foreach (DbDataRecord rec in reader)
								idString = rec.GetInt32(0);
							reader.Close();
						}

						if (writeToFile)
							sw.Write("\t" + queryIns1 + "\n");


						DateTime dF_Fact = pr2.Value.Key;
						DateTime dT_Fact = pr2.Value.Value;
						if (dF_Fact > dateFrom)
							dateFrom = dF_Fact;
						if (dT_Fact < dateTo)
							dateTo = dT_Fact - new TimeSpan(1, 0, 0, 0);

						
						gen_str_pers(idString, pers, dateFrom, dateTo, writeToDb, writeToFile, sw);
					}
				}
			}
		}


		public Dictionary<int, Dictionary<int, KeyValuePair<DateTime, DateTime>>> get_pers_period (DateTime dateFrom, DateTime dateTo)
		{
			//units = new HashSet<int>();		//Список подразделений (без повторов)
			string from = dateFrom.Year + "-" + dateFrom.Month + "-" + dateFrom.Day;
			string to = dateTo.Year + "-" + dateTo.Month + "-" + dateTo.Day;

			Dictionary<int, Dictionary<int, KeyValuePair<DateTime, DateTime>>> persAndUnit 
				= new Dictionary<int, Dictionary<int, KeyValuePair<DateTime, DateTime>>>();

			//Выбираем сотрудников работающих в период с dateFrom по dateTo
			string strCom = "select p.\"pk_personal_card\", u.\"pk_unit\", p.\"DataFrom\", p.\"DateTo\"" +
				" from \"PeriodPosition\" p, \"Position\" d, \"Unit\" u " +
				" where p.\"pk_position\" = d.\"pk_position\" and d.\"pk_unit\" = u.\"pk_unit\" and " +
			"('" + from + "' <= p.\"DateTo\" or p.\"DateTo\" is null) and '" + to + "' >= p.\"DataFrom\"";	

			NpgsqlCommand command = new NpgsqlCommand(strCom, connect.get_connect());
			NpgsqlDataReader reader = command.ExecuteReader();
			if (reader.HasRows)
				foreach (DbDataRecord rec in reader)
				{
					int pers = rec.GetInt32(0);

					if (charactPers.ContainsKey(pers))
					{
						DateTime dF = rec.GetDateTime(2);

						DateTime dT = new DateTime(3000, 1, 1);
						if (!rec.IsDBNull(3))
							dT = rec.GetDateTime(3);

						if (!persAndUnit.ContainsKey(pers))
							persAndUnit.Add(pers, new Dictionary<int, KeyValuePair<DateTime, DateTime>>());
						persAndUnit[pers].Add(rec.GetInt32(1), new KeyValuePair<DateTime, DateTime>(dF, dT));

					}

					//units.Add(rec.GetInt32(1));
				}
			reader.Close();



			return persAndUnit;
		}

		//private List<int> work


		private void gen_str_pers(int idString, int idPers, DateTime dateFrom, DateTime dateTo, bool writeToBd, bool writeToFile, StreamWriter sw)
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
					switch (charactPers[idPers])
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
