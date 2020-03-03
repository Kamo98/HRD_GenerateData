using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HRD_GenerateData
{
	class GenPersonalCard
	{
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
		}

		public void execute ()
		{
			StreamWriter sw = new StreamWriter("personalCards.txt");
			Random rand = new Random();

			for (int i = 0; i < firstNameMan.Length; i++)
				for (int j = 0; j < lastNameMan.Length; j++)
					for (int k = 0; k < patronymicMan.Length; k++)
					{
						string strComIns = "('" + lastNameMan[j] + " "+  firstNameMan[i] + " " + patronymicMan[k] + "', ";

						get_persinal_data(rand, ref strComIns);

						strComIns += "'М')\n";
						sw.Write(strComIns);
					}

			for (int i = 0; i < firstNameWoman.Length; i++)
				for (int j = 0; j < lastNameWoman.Length; j++)
					for (int k = 0; k < patronymicWoman.Length; k++)
					{
						string strComIns = "('" + lastNameWoman[j] + " " + firstNameWoman[i] + " " + patronymicWoman[k] + "', ";
						
						get_persinal_data(rand, ref strComIns);
						strComIns += "'Ж')\n";
						sw.Write(strComIns);
					}

			sw.Close();

		}


		private void get_persinal_data(Random rand, ref string strComIns)
		{
			int monthBirth = rand.Next(1, 13);
			int dayBirth = rand.Next(1, daysInMonth[monthBirth - 1]);
			int yearBirth = rand.Next(minYear, maxYear);

			strComIns += "'" + dayBirth + "." + monthBirth + "." + yearBirth + "', ";
			strComIns += "'Характеристика', ";

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
			int dayPasp = rand.Next(1, daysInMonth[monthBirth - 1]);
			int yearPasp = yearBirth + 20;
			strComIns += "'" + dayPasp + "." + monthPasp + "." + yearPasp + "', ";

			strComIns += "'ГУ МВД по Алтайскому краю',";

			int monthRegistr = rand.Next(1, 13);
			int dayRegistr = rand.Next(1, daysInMonth[monthBirth - 1]);
			int yearRegistr = yearBirth + 21;

			strComIns += "'" + dayRegistr + "." + monthRegistr + "." + yearRegistr + "', ";

			//Адреса
			string addressRegistr = "";
			strComIns += "'" + addressRegistr + "', ";

			string addressFact = "";
			strComIns += "'" + addressFact + "', ";

			string phone = "+7" + rand.Next(11111, 99999) + rand.Next(11111, 99999);
			strComIns += "'" + phone + "', ";

			string placeOfBirth = "";
			strComIns += "'" + placeOfBirth + "', ";

			int monthCreate = rand.Next(1, 13);
			int dayCreate = rand.Next(1, daysInMonth[monthBirth - 1]);
			int yearCreate = yearBirth + 23;
			strComIns += "'" + dayCreate + "." + monthCreate + "." + yearCreate + "', ";


		}
	}
}
