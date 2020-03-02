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

			Test test = new Test(connect);
			test.execute();

			connect.close_connection();
		}
	}
}
