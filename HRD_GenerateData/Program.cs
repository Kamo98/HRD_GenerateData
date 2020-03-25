using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRD_GenerateData
{
	class Program
	{
		static void Main(string[] args)
		{
			string login = "postgres";
			string pass = "Ntcnbhjdfybt_01";
			Connection connect = Connection.get_instance(login, pass);
			//Connection connect = null;

			//GenPersonalCard gpc = new GenPersonalCard(connect);
			//gpc.clear();
			//gpc.execute();

			GenTimeTracking gtt = new GenTimeTracking(connect);
			gtt.clear();
			gtt.execute();

			//GenOrders gord = new GenOrders(connect);
			//gord.clear();
			//gord.execute();

			//GenHandbooks gh = new GenHandbooks(connect);
			//gh.addMarkTimeTracking();

			(new Test(connect)).ecec_read();
			//(new Test(connect)).get_all_tables();
			//(new Test(connect)).correct_db();



			connect.close_connection();

		}
	}
}
