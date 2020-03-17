using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRD_GenerateData
{
	class GenTimeTracking
	{
		public static DateTime curDate = new DateTime(2019, 12, 20);

		Connection connect;
		List<int> idPersonals = new List<int>();
		List<DateTime> createDates = new List<DateTime>();

		//Для выборки шифров явки/неявки и ключей
		Dictionary<string, int> markTT2key = new Dictionary<string, int>();

		int limitPersinal = 10;
		int offsetPersonal = 0;

		public GenTimeTracking(Connection conn)
		{
			connect = conn;
			get_personal();
			get_mark_tt();
		}

		//Получение id сотрудников и дат создания их карточек
		private void get_personal()
		{
			string strCom = "select \"pk_personal_card\", \"Creation_date\" from \"PersonalCard\" " +
				" limit " + limitPersinal + " offset " + offsetPersonal;
			NpgsqlCommand command = new NpgsqlCommand(strCom, connect.get_connect());

			NpgsqlDataReader reader = command.ExecuteReader();
			if (reader.HasRows)
				foreach (DbDataRecord rec in reader)
				{
					idPersonals.Add(rec.GetInt32(0));
					createDates.Add((DateTime)rec.GetValue(1));
					Console.Write((curDate - (DateTime)rec.GetValue(1)).TotalDays / 30 + "\n");
				}
			reader.Close();
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


		public void execute()
		{
			Random rand = new Random();

			string queryIns = "insert into \"TimeTracking\" " +
			"\"nomer\", " +
			"\"date_sostav\", " +
			"\"from\"," +
			" \"to\"," +
			" \"pk_unit\"," +
			" values (";

			string number = rand.Next(10000000, 100000000).ToString();


		}

		//private List<int> work


		private void gen_str_top_pers()
		{

		}
		private void gen_str_good_pers()
		{

		}

		private void gen_str_bad_pers()
		{

		}

	}
}
