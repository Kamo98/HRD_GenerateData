﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRD_GenerateData
{
	class GenDepartAndPos
	{
		private string[] departments = new string[]
		{
			"Акушерское отделение",
			"Гинекологическое отделение",
			"Детская консультация",
			"Инфекционное отделение",
			"Кадошкинское поликлиническое отделение",
			"Кардиологическое отделение",
			"Неврологическое отделение",
			"Отделение переливания крови",
			"Отделение скорой медицинской помощи",
			"Паллиативное отделение",
			"Педиатрическое (детское) отделение",
			"Терапевтическое отделение",
			"Травматологическое отделение",
			"Хирургическое отделение"
		};

		Connection connect;
		public GenDepartAndPos(Connection conn)
		{
			connect = conn;
		}

		public void addDepartments()
		{

			foreach (string dep in departments) {
				string strComIns = "insert into \"Unit\" (\"Name\") values ('" + dep + "')";

				NpgsqlCommand command = new NpgsqlCommand(strComIns, connect.get_connect());

				int count = command.ExecuteNonQuery();
				if (count == 1)
					Console.Out.Write("Строка вставлена\n");
				else
					Console.Out.Write("Строка НЕ вставлена\n");
			}
		}

		public void addPositions()
		{

		}

	}
}
