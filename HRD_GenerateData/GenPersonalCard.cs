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

		public void clear ()
		{
			string strForCom = "delete from \"PersonalCard\"";

			NpgsqlCommand command = new NpgsqlCommand(
				strForCom,
				connect.get_connect());

			command.ExecuteNonQuery();
		}

		public void execute ()
		{
			StreamWriter sw = new StreamWriter("personalCards.txt");
			StreamWriter swDate = new StreamWriter("datesCreating.txt");
			Random rand = new Random();

			bool writeToDb = true;
			bool writeToFile = true;
			bool writeLog = true;

			string queryIns = "insert into \"PersonalCard\"" +
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
						"\"Gender\")" +
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

						strComIns += "'М')";

						if (writeToFile)
							sw.Write(strComIns + "\n");

						if (writeToDb)
						{
							NpgsqlCommand command = new NpgsqlCommand(strComIns, connect.get_connect());
							int count = command.ExecuteNonQuery();
							if (writeLog)
								if (count == 1)
									Console.Out.Write("Строка вставлена\n");
								else
									Console.Out.Write("Строка НЕ вставлена\n");
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

						strComIns += "'Ж')";

						if (writeToFile)
							sw.Write(strComIns + "\n");

						if (writeToDb)
						{
							NpgsqlCommand command = new NpgsqlCommand(strComIns, connect.get_connect());
							int count = command.ExecuteNonQuery();
							if (writeLog)
								if (count == 1)
									Console.Out.Write("Строка вставлена\n");
								else
									Console.Out.Write("Строка НЕ вставлена\n");
						}
					}
			sw.Close();

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

			string placeOfBirth = addressRegistr;
			strComIns += "'" + placeOfBirth + "', ";
			

			//DateTime dateCreate = GenTimeTracking.curDate.Subtract(new TimeSpan(rand.Next(60, 30*24), 0,0,0,0));

			//int monthCreate = rand.Next(1, 13);
			//int dayCreate = rand.Next(4, daysInMonth[monthBirth - 1]);
			//int yearCreate = yearBirth + 23;
			strComIns += "'" + dateCreating.Year + "-" + dateCreating.Month + "-" + dateCreating.Day + "', ";


		}
	}
}
