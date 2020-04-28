using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Npgsql;

namespace HRD_GenerateData
{
	class GenPersonalCard
	{
		int countPeople;

		private string[] firstNameMan = new string[]
		{
			"Сергей", "Михаил", "Николай", "Трофим", "Тимофей",
			"Алексей", "Александр", "Никита", "Владислав", "Виктор",
			"Иван", "Пётр", "Дмитрий", "Павел", "Фёдор"
		};

		private string[] lastNameMan = new string[]
		{
			"Сергеев", "Михайлов", "Николаев", "Трофимов", "Тимофеев",
			"Алексеев", "Краснов", "Борисов", "Чернов", "Менделеев",
			"Иванов", "Пётров", "Остапкин", "Павлов", "Фёдоров"
		};

		private string[] patronymicMan = new string[]
		{
			"Сергеевич", "Михайлович", "Николаевич", "Трофимович", "Тимофеевич",
			"Алексеевич", "Александрович", "Никитич", "Владиславович", "Викторович",
			"Иванович", "Петрович", "Дмитриевич", "Павлович", "Фёдорович"
		};


		private string[] firstNameWoman = new string[]
		{
			"Алла", "Ксения", "Ирина", "Екатерина", "Евгения",
			"Виолетта", "Кристина", "Валентина", "Тамара", "Варвара",
			"Оксана", "Мария", "Дарья", "Анастасия", "Алина"
		};

		private string[] lastNameWoman = new string[]
		{
			"Сергеева", "Михайлова", "Николаева", "Трофимова", "Тимофеева",
			"Алексеева", "Краснова", "Борисова", "Чернова", "Менделеева",
			"Иванова", "Пётрова", "Остапкина", "Павлова", "Фёдорова"
		};

		private string[] patronymicWoman = new string[]
		{
			"Сергеевна", "Михайловна", "Николаевна", "Трофимовна", "Тимофеевна",
			"Алексеевна", "Александровна", "Борисовна", "Владиславовна", "Викторовна",
			"Ивановна", "Петровна", "Дмитриевна", "Павловна", "Фёдоровна"
		};

		private const int minYear = 1970;
		private const int maxYear = 1995;
		private int[] daysInMonth = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

		Connection connect;
		public GenPersonalCard(Connection conn)
		{
			connect = conn;
			countPeople = 4;
		}

		

		public void execute ()
		{
			StreamWriter sw = new StreamWriter("personalCards.txt");
			StreamWriter swDate = new StreamWriter("datesCreating.txt");
			Random rand = new Random();

			bool writeToDb = true;
			bool writeToFile = true;
			bool writeLog = true;

			string queryIns = "insert into \"PersonalCard\" " +
						"(\"surname\"," +
						" \"name\"," +
						" \"otchestvo\", " +
						"\"pk_marital_status\"," +
						" \"pk_character_work\"," +
						" \"pk_military_rank\"," +
						" \"pk_category_military\"," +
						" \"pk_stock_category\", " +
						"\"birthday\"," +
						" \"Characteristic\"," +
						" \"INN\"," +
						" \"SSN\"," +
						" \"Labor_contract\"," +
						" \"Serial_number\", " +
						"\"Passport_date\"," +
						" \"Vidan\"," +
						" \"Home_date\", " +
						"\"Propiska\"," +
						" \"Fact_address\", " +
						"\"Phone\"," +
						"\"Birth_place\"," +
						" \"Creation_date\", " +
						" \"Military_profile\", " +
						" \"Military_code\", " +
						" \"Military_name\", " +
						" \"Military_status\", " +
						" \"Military_cancel\", " +
						" \"Gender\") " +
						" values (";



			const int maxDeltaDays = 30 * 24;
			int deltaDays = maxDeltaDays / ((countPeople * countPeople * countPeople));
			DateTime dateCreating = GenTimeTracking.curDate.Subtract(new TimeSpan(maxDeltaDays, 0, 0, 0, 0));


			const int maxPersInOrder = 6;
			int curCount = rand.Next(1, maxPersInOrder+1);
			int countPers = 0;

			for (int i = 0; i < countPeople; i++)
				for (int j = 0; j < countPeople; j++)
					for (int k = 0; k < countPeople; k++)
					{
						countPers++;

						if (countPers % curCount == 0)
						{
							curCount = rand.Next(1, maxPersInOrder + 1);
							dateCreating += new TimeSpan(deltaDays, 0, 0, 0, 0);
						}

						string strComIns = queryIns +
						"'" + lastNameMan[j] + "', " +
						"'" + firstNameMan[i] + "', " +
						"'" + patronymicMan[k] + "', ";

						get_persinal_data(rand, ref strComIns, dateCreating);

						strComIns += "'М') RETURNING \"pk_personal_card\"";

						if (writeToFile)
							sw.Write(strComIns + "\n");

						if (writeToDb)
						{
							int pk_personal_card = 1;

							NpgsqlCommand command = new NpgsqlCommand(strComIns, connect.get_connect());

							NpgsqlDataReader reader2 = command.ExecuteReader();
							if (reader2.HasRows) // если есть данные
							{
								while (reader2.Read()) // построчно считываем данные
								{
									object pk = reader2.GetValue(0);
									pk_personal_card = Convert.ToInt32(pk);

								}
							}
							reader2.Close();

							set_cards(rand, pk_personal_card);
							//int count = command.ExecuteNonQuery();
							//if (writeLog)
							//	if (count == 1)
							//		Console.Out.Write("Строка вставлена\n");
							//	else
							//		Console.Out.Write("Строка НЕ вставлена\n");
						}
						
					}

			for (int i = 0; i < countPeople; i++)
				for (int j = 0; j < countPeople; j++)
					for (int k = 0; k < countPeople; k++)
					{
						countPers++;

						if (countPers % curCount == 0)
						{
							curCount = rand.Next(1, maxPersInOrder+1);
							dateCreating += new TimeSpan(deltaDays, 0, 0, 0, 0);
						}

						string strComIns = queryIns +
						"'" + lastNameMan[j] + "', " +
						"'" + firstNameMan[i] + "', " +
						"'" + patronymicMan[k] + "', ";

						get_persinal_data(rand, ref strComIns, dateCreating);

						strComIns += "'Ж') RETURNING \"pk_personal_card\"";

						if (writeToFile)
							sw.Write(strComIns + "\n");

						if (writeToDb)
						{
							int pk_personal_card = 1;

							NpgsqlCommand command = new NpgsqlCommand(strComIns, connect.get_connect());

							NpgsqlDataReader reader2 = command.ExecuteReader();
							if (reader2.HasRows) // если есть данные
							{
								while (reader2.Read()) // построчно считываем данные
								{
									object pk = reader2.GetValue(0);
									pk_personal_card = Convert.ToInt32(pk);

								}
							}
							reader2.Close();

							set_cards(rand, pk_personal_card);

							//NpgsqlCommand command = new NpgsqlCommand(strComIns, connect.get_connect());
							//int count = command.ExecuteNonQuery();
							//if (writeLog)
							//	if (count == 1)
							//		Console.Out.Write("Строка вставлена\n");
							//	else
							//		Console.Out.Write("Строка НЕ вставлена\n");
						}
					}
			sw.Close();

		}

		public void clear()
		{
			string strForCom = "delete from \"card-citizenship\"";

			NpgsqlCommand command = new NpgsqlCommand(
				strForCom,
				connect.get_connect());

			command.ExecuteNonQuery();

			strForCom = "delete from \"lang-card\"";

			command = new NpgsqlCommand(
				strForCom,
				connect.get_connect());

			command.ExecuteNonQuery();

			strForCom = "delete from \"card-education\"";

			command = new NpgsqlCommand(
				strForCom,
				connect.get_connect());

			command.ExecuteNonQuery();

			strForCom = "delete from \"Characteristic\"";

			command = new NpgsqlCommand(
				strForCom,
				connect.get_connect());

			command.ExecuteNonQuery();
			

			strForCom = "delete from \"PersonalCard\"";

			command = new NpgsqlCommand(
				strForCom,
				connect.get_connect());

			command.ExecuteNonQuery();
		}

		private void set_cards(Random rand, int id_pers)
		{
			//Гражданство
			//Добавим гражданство в карточку гражданства
			string SqlExpression101 = "INSERT INTO \"card-citizenship\" (\"pk_sitizenship\",\"pk_personal_card\") " +
			"VALUES (@pk_citizenship,@pk_personal_card)";


			NpgsqlCommand command = new NpgsqlCommand(SqlExpression101, connect.get_connect());
			// создаем параметры и добавляем их к команде
			NpgsqlParameter Param1 = new NpgsqlParameter("@pk_citizenship", 294);
			command.Parameters.Add(Param1);
			NpgsqlParameter Param2 = new NpgsqlParameter("@pk_personal_card", id_pers);
			command.Parameters.Add(Param2);
			int number = command.ExecuteNonQuery();



			//Языки
			int kLang = rand.Next(1, 3);


			string SqlExpression2 = "INSERT INTO \"lang-card\" (\"pk_language\",\"pk_personal_card\",\"pk_degree_language\") " +
								"VALUES (@pk_language,@pk_personal_card,@pk_degree_language)";

			command = new NpgsqlCommand(SqlExpression2, connect.get_connect());
			// создаем параметры и добавляем их к команде
			Param1 = new NpgsqlParameter("@pk_language", 199);
			command.Parameters.Add(Param1);
			Param2 = new NpgsqlParameter("@pk_personal_card", id_pers);
			command.Parameters.Add(Param2);
			NpgsqlParameter Param3 = new NpgsqlParameter("@pk_degree_language", 3);
			command.Parameters.Add(Param3);
			command.ExecuteNonQuery();

			if (kLang >= 1)
			{
				command = new NpgsqlCommand(SqlExpression2, connect.get_connect());
				// создаем параметры и добавляем их к команде
				Param1 = new NpgsqlParameter("@pk_language", 171);
				command.Parameters.Add(Param1);
				Param2 = new NpgsqlParameter("@pk_personal_card", id_pers);
				command.Parameters.Add(Param2);
				Param3 = new NpgsqlParameter("@pk_degree_language", 2);
				command.Parameters.Add(Param3);
				command.ExecuteNonQuery();
			}

			if (kLang == 2)
			{
				command = new NpgsqlCommand(SqlExpression2, connect.get_connect());
				// создаем параметры и добавляем их к команде
				Param1 = new NpgsqlParameter("@pk_language", 258);
				command.Parameters.Add(Param1);
				Param2 = new NpgsqlParameter("@pk_personal_card", id_pers);
				command.Parameters.Add(Param2);
				Param3 = new NpgsqlParameter("@pk_degree_language", 1);
				command.Parameters.Add(Param3);
				command.ExecuteNonQuery();
			}

			//Образование

			string SqlExpression3 = "INSERT INTO \"card-education\" (\"pk_education\",\"pk_personal_card\",\"pk_specialty\"," +
							   "\"pk_nstitution\",\"document_name\",\"serial_number\",\"Year\") " +
							   "VALUES (@pk_education,@pk_personal_card,@pk_specialty,@pk_nstitution,@document_name,@serial_number,@Year)";

			command = new NpgsqlCommand(SqlExpression3, connect.get_connect());
			// создаем параметры и добавляем их к команде
			Param1 = new NpgsqlParameter("@pk_education", 3);
			command.Parameters.Add(Param1);
			Param2 = new NpgsqlParameter("@pk_personal_card", id_pers);
			command.Parameters.Add(Param2);
			Param3 = new NpgsqlParameter("@pk_specialty", rand.Next(2, 617));
			command.Parameters.Add(Param3);
			NpgsqlParameter Param4 = new NpgsqlParameter("@pk_nstitution", rand.Next(2, 47));
			command.Parameters.Add(Param4);
			NpgsqlParameter Param5 = new NpgsqlParameter("@document_name", "Диплом");
			command.Parameters.Add(Param5);
			NpgsqlParameter Param6 = new NpgsqlParameter("@serial_number", "1234-5678");
			command.Parameters.Add(Param6);
			NpgsqlParameter Param7 = new NpgsqlParameter("@Year", 2000);
			command.Parameters.Add(Param7);
			command.ExecuteNonQuery();


			//Хар-ка

			string SqlExpression4 = "INSERT INTO \"Characteristic\" (\"date\",\"characteristic\",\"fileReference\"," +
								"\"pk_personal_card\") " +
								"VALUES (@date,@characteristic,@fileReference,@pk_personal_card)";

			command = new NpgsqlCommand(SqlExpression4, connect.get_connect());
			// создаем параметры и добавляем их к команде
			Param1 = new NpgsqlParameter("@date", new DateTime(2018, 5, 12));
			command.Parameters.Add(Param1);
			Param2 = new NpgsqlParameter("@characteristic", "Характеристика сотрудника");
			command.Parameters.Add(Param2);
			Param3 = new NpgsqlParameter("@fileReference", "C:\\KADRY_CHARACTERISTIC");
			command.Parameters.Add(Param3);
			Param4 = new NpgsqlParameter("@pk_personal_card", id_pers);
			command.Parameters.Add(Param4);
			command.ExecuteNonQuery();

			


		}

		private void get_persinal_data(Random rand, ref string strComIns, DateTime dateCreating)
		{
			int maritalStatus = rand.Next(1, 7);
			int militaryRank = rand.Next(1, 13);
			int categoryMilitary = rand.Next(4, 13);
			int stockCategory = rand.Next(1, 4);
			strComIns += "'" + maritalStatus + "', " + "'" + 2 + "', " + 
				"'" + militaryRank + "', " + "'" + categoryMilitary + "', " + "'" + stockCategory + "', "; 

			int monthBirth = rand.Next(1, 13);
			int dayBirth = rand.Next(4, daysInMonth[monthBirth - 1]);
			int yearBirth = rand.Next(minYear, maxYear);

			strComIns += "'" + yearBirth + "-" + monthBirth + "-" + (dayBirth - 2) + "', ";
			strComIns += "'', ";

			//ИНН
			string inn = rand.Next(111111, 999999).ToString() + rand.Next(111111, 999999).ToString();
			strComIns += "'" + inn + "', ";

			//СНИЛС
			string snils = rand.Next(100, 999).ToString() + "-" + rand.Next(100, 999).ToString() + "-" + rand.Next(100, 999).ToString() + " " + rand.Next(10, 99).ToString();
			strComIns += "'" + snils + "', ";

			//Номер трудового договора
			string numberWork = rand.Next(1000, 9999).ToString() + "-" + rand.Next(1000, 9999).ToString();
			strComIns += "'" + numberWork + "', ";

			string seriaNumPasp = rand.Next(1000, 9999).ToString() + " " + rand.Next(100000, 999999).ToString();
			strComIns += "'" + seriaNumPasp + "', ";

			int monthPasp = rand.Next(1, 13);
			int dayPasp = rand.Next(4, daysInMonth[monthBirth - 1]);
			int yearPasp = yearBirth + 20;
			strComIns += "'" + yearPasp + "-" + monthPasp + "-" + (dayPasp - 2) + "', ";

			strComIns += "'ГУ МВД по Алтайскому краю', ";

			int monthRegistr = rand.Next(1, 13);
			int dayRegistr = rand.Next(4, daysInMonth[monthBirth - 1]);
			int yearRegistr = yearBirth + 21;

			strComIns += "'" + yearRegistr + "-" + monthRegistr + "-" + (dayRegistr - 2) + "', ";

			//Адреса
			string addressRegistr = "Алтайский край, г. Барнаул, ул Ленина, д. 66, кв 13";
			strComIns += "'" + addressRegistr + "', ";

			string addressFact = addressRegistr;
			strComIns += "'" + addressFact + "', ";

			string phone = "+7" + rand.Next(11111, 99999) + rand.Next(11111, 99999);
			strComIns += "'" + phone + "', ";

			string placeOfBirth = "г. Барнаул";
			strComIns += "'" + placeOfBirth + "', ";
			

			//DateTime dateCreate = GenTimeTracking.curDate.Subtract(new TimeSpan(rand.Next(60, 30*24), 0,0,0,0));

			//int monthCreate = rand.Next(1, 13);
			//int dayCreate = rand.Next(4, daysInMonth[monthBirth - 1]);
			//int yearCreate = yearBirth + 23;
			strComIns += "'" + dateCreating.Year + "-" + dateCreating.Month + "-" + dateCreating.Day + "', ";

			strComIns += "'Профиль', ";
			strComIns += "'Кодовое обозначение ВУС', ";
			strComIns += "'Военный комиссариат по Советскому и Алтайскому районам', ";
			strComIns += "'Состоит', ";
			strComIns += "'-', ";
		}
	}
}
