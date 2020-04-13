using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRD_GenerateData
{
	class GenOrders
	{
		Connection connect;
		List<int> idPersonals = new List<int>();
		List<bool> isDismiss = new List<bool>();
		List<bool> fluidity = new List<bool>();		//Наличие текучести сорудника
		List<DateTime> createDates = new List<DateTime>();
		List<int> positionsId = new List<int>();

		Random rand = new Random();

		int limitPersinal = 128;
		int offsetPersonal = 0;

		public GenOrders(Connection conn)
		{
			connect = conn;
			get_personal();
			get_positions();
		}


		public void execute()
		{
			int countPers = createDates.Count;
			DateTime curDate = createDates[0];
			for (int i = 0; i < countPers;)
			{
				List<int> curPers = new List<int>();
				while (i < countPers && createDates[i] == curDate)
				{
					curPers.Add(i);
					i++;
				}

				//Создать приказы для сотрудников, принятых на дату curDate
				create_orders(curDate, curPers);


				if (i < countPers)
					curDate = createDates[i];
			}
		}


		private void create_orders (DateTime date, List<int> pers)
		{
			//List<int> moveWorkPers = new List<int>();		//Список для тех, кого надо переводить
			//Проверяем, нужен ли перевод этих сотрудников
			bool hasMoveWork = false;
			foreach(int p in pers)
				if (fluidity[p])
				{
					hasMoveWork = true;
					break;
				}

			string strComIns;
			NpgsqlCommand command;
			NpgsqlDataReader reader;
			GenHandbooks gh = new GenHandbooks(connect);

			//если нужен решаем, сколько будет переводов, определяем даты и создаём шапки приказов
			List<KeyValuePair<int, DateTime>> idOrdersMove = new List<KeyValuePair<int, DateTime>>();       //Список id и дат шапок на перевод
			int countDeltaDay = 0;

			if (hasMoveWork)
			{
				int countMove = rand.Next(1, 4);        //От 1 до 3
				countDeltaDay = (GenTimeTracking.curDate - date).Days / (countMove + 2);		//Разница в днях межу переводами

				for (int i = 0; i < countMove; i++)
				{
					DateTime dateMove = date + new TimeSpan(countDeltaDay * (i+1), 0, 0, 0);

					strComIns = "insert into \"Order\" (\"nomer\", \"data_order\", \"pk_type_order\") " +
					"values ('" + rand.Next(1000000, 10000000) + "', '" +
					dateMove.Year + "-" + dateMove.Month + "-" + dateMove.Day + "', '" +
					gh.get_id_type_order(GenHandbooks.moveWork) + //Тип приказа: перевод
					"') RETURNING \"pk_order\"";

					command = new NpgsqlCommand(strComIns, connect.get_connect());

					reader = command.ExecuteReader();
					foreach (DbDataRecord rec in reader)
						idOrdersMove.Add(new KeyValuePair<int, DateTime>(rec.GetInt32(0), dateMove));
					reader.Close();					
				}
			}


			//Прверяем, нужен ли приказ об увольнении
			bool hasDismiss = false;
			foreach (int p in pers)
				if (isDismiss[p])
				{
					hasDismiss = true;
					break;
				}

			int idOrderDismiss = -1;
			DateTime dateDismiss = date + new TimeSpan((GenTimeTracking.curDate - date).Days / 10 * 8, 0, 0, 0);

			if (hasDismiss)
			{
				

				if (hasMoveWork)
					dateDismiss = idOrdersMove[idOrdersMove.Count - 1].Value + new TimeSpan(countDeltaDay, 0, 0, 0);

				strComIns = "insert into \"Order\" (\"nomer\", \"data_order\", \"pk_type_order\") " +
					"values ('" + rand.Next(1000000, 10000000) + "', '" +
					dateDismiss.Year + "-" + dateDismiss.Month + "-" + dateDismiss.Day + "', '" +
					gh.get_id_type_order(GenHandbooks.dismissal) +		//Тип приказа: увольнение
					"') RETURNING \"pk_order\"";

				command = new NpgsqlCommand(strComIns, connect.get_connect());

				reader = command.ExecuteReader();
				foreach (DbDataRecord rec in reader)
					idOrderDismiss = rec.GetInt32(0);
				reader.Close();
			}


			//Добавить шапку приказа на приём сотрудников
			strComIns = "insert into \"Order\" (\"nomer\", \"data_order\", \"pk_type_order\") " +
					"values ('" + rand.Next(1000000, 10000000) + "', '" +
					date.Year + "-" + date.Month + "-" + date.Day + "', '" + 
					gh.get_id_type_order(GenHandbooks.recruitment) + //Тип приказа: приём
					"') RETURNING \"pk_order\"";

			command = new NpgsqlCommand(strComIns, connect.get_connect());

			reader = command.ExecuteReader();
			int idOrderPriem = -1;
			foreach (DbDataRecord rec in reader)
				idOrderPriem = rec.GetInt32(0);
			reader.Close();

			


			//Создать строки приказа для каждого сотрудника
			foreach (int p in pers)
			{

				//Вставка строк приказов для сотрудника

				//На приём
				string numberWork = rand.Next(1000, 9999).ToString() + "-" + rand.Next(1000, 9999).ToString();

				strComIns = "insert into \"String_order\" " +
					"(\"pk_order\", \"Number_work_doc\", \"Work_doc_date\",  \"Reason\") " +
					"values ('" + idOrderPriem + "', '" +
					numberWork + "', '" +
					date.Year + "-" + date.Month + "-" + date.Day + 
					"', 'Основание') RETURNING \"pk_string_order\"";

				command = new NpgsqlCommand(strComIns, connect.get_connect());
				NpgsqlDataReader reader2 = command.ExecuteReader();
				int idStrRecruitment = -1;
				foreach (DbDataRecord rec in reader2)
					idStrRecruitment = rec.GetInt32(0);
				reader2.Close();


				List<int> idMoveStrings = new List<int>();

				//На переводы
				if (fluidity[p] == true)
				{
					foreach (KeyValuePair<int, DateTime> idOrd in idOrdersMove)
					{
						numberWork = rand.Next(1000, 9999).ToString() + "-" + rand.Next(1000, 9999).ToString();

						strComIns = "insert into \"String_order\" " +
						"(\"pk_order\", \"Number_work_doc\", \"Work_doc_date\",  \"Reason\") " +
						"values ('" + idOrd.Key + "', '" +
						numberWork + "', '" +
						idOrd.Value.Year + "-" + idOrd.Value.Month + "-" + idOrd.Value.Day +
						"', 'Основание для перевода сотрудника') RETURNING \"pk_string_order\"";

						command = new NpgsqlCommand(strComIns, connect.get_connect());
						reader2 = command.ExecuteReader();
						foreach (DbDataRecord rec in reader2)
							idMoveStrings.Add(rec.GetInt32(0));
						reader2.Close();
					}
				}


				int idDismissString = -1;
				//На увольнение
				if (isDismiss[p])
				{
					numberWork = rand.Next(1000, 9999).ToString() + "-" + rand.Next(1000, 9999).ToString();

					strComIns = "insert into \"String_order\" " +
					"(\"pk_order\", \"Number_work_doc\", \"Work_doc_date\",  \"Reason\") " +
					"values ('" + idOrderDismiss + "', '" +
					numberWork + "', '" +
					dateDismiss.Year + "-" + dateDismiss.Month + "-" + dateDismiss.Day +
					"', 'Основание для увольнения сотрудника') RETURNING \"pk_string_order\"";

					command = new NpgsqlCommand(strComIns, connect.get_connect());
					reader2 = command.ExecuteReader();
					foreach (DbDataRecord rec in reader2)
						idDismissString = rec.GetInt32(0);
					reader2.Close();
				}


				//Вставка периодов должостей для сотрудника

				if (idMoveStrings.Count == 0)           //Только приём
				{
					if (isDismiss[p])
					{
						strComIns = "insert into \"PeriodPosition\" " +
							"(\"DataFrom\", \"DateTo\", \"pk_position\", \"pk_personal_card\",  \"pk_move_order\", \"pk_fire_order_string\") " +
							"values ('" +
							date.Year + "-" + date.Month + "-" + date.Day + "', '" +
							dateDismiss.Year + "-" + dateDismiss.Month + "-" + dateDismiss.Day + "', '" +
							positionsId[rand.Next(0, positionsId.Count)] + "', '" +     //Случайная должность
							idPersonals[p] + "', '" +
							idStrRecruitment + "', '" + 
							idDismissString + "')";

						command = new NpgsqlCommand(strComIns, connect.get_connect());
						int count = command.ExecuteNonQuery();
					} else
					{
						strComIns = "insert into \"PeriodPosition\" " +
							"(\"DataFrom\", \"pk_position\", \"pk_personal_card\",  \"pk_move_order\") " +
							"values ('" +
							date.Year + "-" + date.Month + "-" + date.Day + "', '" +
							positionsId[rand.Next(0, positionsId.Count)] + "', '" +     //Случайная должность
							idPersonals[p] + "', '" +
							idStrRecruitment + "')";

						command = new NpgsqlCommand(strComIns, connect.get_connect());
						int count = command.ExecuteNonQuery();
					}

				}
				else
				{           //Нужны переводы
					int positionId = positionsId[rand.Next(0, positionsId.Count)];   //Случайная должность
					
					//Приём
					strComIns = "insert into \"PeriodPosition\" " +
						"(\"DataFrom\", \"DateTo\", \"pk_position\", \"pk_personal_card\",  \"pk_move_order\") " +
						"values ('" +
						date.Year + "-" + date.Month + "-" + date.Day + "', '" +
						idOrdersMove[0].Value.Year + "-" + idOrdersMove[0].Value.Month + "-" + idOrdersMove[0].Value.Day + "', '" +
						positionId + "', '" + 
						idPersonals[p] + "', '" +
						idStrRecruitment + "')";

					command = new NpgsqlCommand(strComIns, connect.get_connect());
					int count = command.ExecuteNonQuery();

					int newPositionId;
					//Переводы
					for (int i = 0; i < idMoveStrings.Count - 1; i++)
					{
						newPositionId = positionsId[rand.Next(0, positionsId.Count)];
						while (newPositionId == positionId)
							newPositionId = positionsId[rand.Next(0, positionsId.Count)];
						positionId = newPositionId;

						strComIns = "insert into \"PeriodPosition\" " +
						"(\"DataFrom\", \"DateTo\", \"pk_position\", \"pk_personal_card\",  \"pk_move_order\") " +
						"values ('" +
						idOrdersMove[i].Value.Year + "-" + idOrdersMove[i].Value.Month + "-" + idOrdersMove[i].Value.Day + "', '" +
						idOrdersMove[i + 1].Value.Year + "-" + idOrdersMove[i + 1].Value.Month + "-" + idOrdersMove[i + 1].Value.Day + "', '" +
						positionId + "', '" +
						idPersonals[p] + "', '" +
						idMoveStrings[i] + "')";

						command = new NpgsqlCommand(strComIns, connect.get_connect());
						count = command.ExecuteNonQuery();
					}


					//Последний перевод
					newPositionId = positionsId[rand.Next(0, positionsId.Count)];
					while (newPositionId == positionId)
						newPositionId = positionsId[rand.Next(0, positionsId.Count)];
					positionId = newPositionId;

					//Нужно ли увольнение
					if (isDismiss[p])
					{
						strComIns = "insert into \"PeriodPosition\" " +
						"(\"DataFrom\", \"DateTo\", \"pk_position\", \"pk_personal_card\",  \"pk_move_order\", \"pk_fire_order_string\") " +
						"values ('" +
						idOrdersMove[idOrdersMove.Count - 1].Value.Year + "-" + idOrdersMove[idOrdersMove.Count - 1].Value.Month + "-" + idOrdersMove[idOrdersMove.Count - 1].Value.Day + "', '" +
						dateDismiss.Year + "-" + dateDismiss.Month + "-" + dateDismiss.Day + "', '" +
						positionId + "', '" +
						idPersonals[p] + "', '" +
						idMoveStrings[idMoveStrings.Count - 1] + "', '" + 
						idDismissString + "')";

						command = new NpgsqlCommand(strComIns, connect.get_connect());
						count = command.ExecuteNonQuery();

					} else
					{
						
						strComIns = "insert into \"PeriodPosition\" " +
						"(\"DataFrom\", \"pk_position\", \"pk_personal_card\",  \"pk_move_order\") " +
						"values ('" +
						idOrdersMove[idOrdersMove.Count-1].Value.Year + "-" + idOrdersMove[idOrdersMove.Count - 1].Value.Month + "-" + idOrdersMove[idOrdersMove.Count - 1].Value.Day + "', '" +
						positionId + "', '" +
						idPersonals[p] + "', '" +
						idMoveStrings[idMoveStrings.Count - 1] + "')";

						command = new NpgsqlCommand(strComIns, connect.get_connect());
						count = command.ExecuteNonQuery();
					}
				}
			

			}
		}



		public void clear()
		{
			string strForCom = "delete from \"PeriodPosition\"";
			NpgsqlCommand command = new NpgsqlCommand(strForCom, connect.get_connect());
			command.ExecuteNonQuery();

			strForCom = "delete from \"String_order\"";
			command = new NpgsqlCommand(strForCom, connect.get_connect());
			command.ExecuteNonQuery();

			strForCom = "delete from \"Order\"";
			command = new NpgsqlCommand(strForCom,connect.get_connect());
			command.ExecuteNonQuery();


		}

		//Получение id сотрудников и дат создания их карточек
		private void get_personal()
		{
			string strCom = "select \"pk_personal_card\", \"Creation_date\" from \"PersonalCard\" order by \"Creation_date\"" +
				" limit " + limitPersinal + " offset " + offsetPersonal;
			NpgsqlCommand command = new NpgsqlCommand(strCom, connect.get_connect());

			NpgsqlDataReader reader = command.ExecuteReader();
			if (reader.HasRows)
				foreach (DbDataRecord rec in reader)
				{
					idPersonals.Add(rec.GetInt32(0));
					DateTime date = (DateTime)rec.GetValue(1);
					createDates.Add(date);
					//Console.Write((GenTimeTracking.curDate - (DateTime)rec.GetValue(1)).TotalDays / 30 + "\n");


					int r = rand.Next(1, 11);

					//Решаем, будут ли сотрудника часто переводить 
					if ((GenTimeTracking.curDate - date).Days > 150)        //Для переводов сотрудник должен проработать хотя бы 150 дней
						if (r <= 5)
							fluidity.Add(true);
						else
							fluidity.Add(false);
					else
						fluidity.Add(false);

					//Решаем будет ли сотрудник уволен
					if (r <= 2 || r == 9)
						isDismiss.Add(true);
					else
						isDismiss.Add(false);




				}
			reader.Close();
		}


		//Получение id должностей
		private void get_positions()
		{
			string strCom = "select \"pk_position\" from \"Position\"";
			NpgsqlCommand command = new NpgsqlCommand(strCom, connect.get_connect());

			NpgsqlDataReader reader = command.ExecuteReader();
			if (reader.HasRows)
				foreach (DbDataRecord rec in reader)
					positionsId.Add(rec.GetInt32(0));
			reader.Close();
		}
	}
}
