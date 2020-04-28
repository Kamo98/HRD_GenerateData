using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRD_GenerateData
{
	class Program
	{
		static void Main(string[] args)
		{
			//string login = "postgres";
			//string pass = "Ntcnbhjdfybt_01";


			string login = "admin1";
			string pass = "admin1";
			//string login = "accounting1";
			//string pass = "accounting1";
			//string login = "reception1";
			//string pass = "reception1";


			Connection connect = Connection.get_instance(login, pass);
			//Connection connect = null;

			string strForCom = "update \"PersonalCard\" SET \"Work_kind\" = 'Основная', \"Index_fact\" = '123456', \"Index_real\" = '123456'";
			NpgsqlCommand command = new NpgsqlCommand(strForCom, connect.get_connect());
			command.ExecuteNonQuery();


			//GenOrders gord = new GenOrders(connect);
			//gord.clear();

			//GenTimeTracking gtt = new GenTimeTracking(connect);
			//gtt.clear();

			//GenPersonalCard gpc = new GenPersonalCard(connect);
			//gpc.clear();
			//gpc.execute();

			

			//gord.execute();


			//gtt.execute();

			//HashSet<int> units;
			//Dictionary<int, List<int>> persAndUnits = gtt.get_pers_period(new DateTime(2019, 1, 22), new DateTime(2019, 7, 17), out units);

			//StreamWriter sw = new StreamWriter("out.txt");
			//foreach (KeyValuePair<int, List<int>> pr in persAndUnits)
			//{
			//	sw.Write(pr.Key + ":\t");
			//	foreach (int i in pr.Value)
			//		sw.Write(i + ",  ");
			//	sw.Write("\n");
			//}
			//sw.Close();





			//GenHandbooks gh = new GenHandbooks(connect);
			//gh.addMarkTimeTracking();

			//(new Test(connect)).ecec_read();
			//(new Test(connect)).get_all_tables();
			//(new Test(connect)).correct_db();



			connect.close_connection();
		}
	}
}
