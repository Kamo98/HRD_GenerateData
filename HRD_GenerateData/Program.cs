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
			//Connection connect = Connection.get_instance(login, pass);
			Connection connect = null;

			//connect.close_connection();

			GenPersonalCard gpc = new GenPersonalCard(connect);
			gpc.execute();
		}
	}
}
