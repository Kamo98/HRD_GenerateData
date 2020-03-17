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
		Connection connect;
		List<int> idPersonals = new List<int>();
		List<DateTime> createDates = new List<DateTime>();

		int limitPersinal = 10;
		int offsetPersonal = 0;

		public GenTimeTracking(Connection conn)
		{
			connect = conn;
			get_personal();
		}

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
				}
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
