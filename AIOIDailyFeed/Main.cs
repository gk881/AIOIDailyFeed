using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Data;
using System.Data.SqlTypes;
using ExcelDataReader;
using System.Data.SqlClient;

namespace AIOIDailyFeed
{
    internal class Main
    {
        public static void ReadIn(int records)
        {
            try
            {
                bool runSuccess = false;
                var conf = new ExcelReaderConfiguration { Password = "BrokerAIOI£!" };
                List<Vehicle> oldRegList = new List<Vehicle>();
                List<Vehicle> newRegList = new List<Vehicle>();


                foreach (string file in Directory.GetFiles(ConfigurationSettings.AppSettings["Path"].ToString()))
                {
                    DataTableCollection tableCollection;
                    DataTable dt = new DataTable();
                    DataTable oldReg = new DataTable();
                    DataSet result = new DataSet();

                    using (var stream = File.Open(file, FileMode.Open, FileAccess.Read))
                    {
                        if (file.Contains(".xlsx") || file.Contains(".csv"))
                        {
                            runSuccess = true;

                            if (file.Contains(".xlsx"))
                            {

                                using (var reader = ExcelReaderFactory.CreateReader(stream, conf))
                                {
                                    result = reader.AsDataSet(new ExcelDataSetConfiguration()
                                    {
                                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                                    });
                                }
                            }
                            else if (file.Contains(".csv"))
                            {
                                using (var reader = ExcelReaderFactory.CreateCsvReader(stream, conf))
                                {
                                    result = reader.AsDataSet(new ExcelDataSetConfiguration()
                                    {
                                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                                    });

                                }
                            }

                            tableCollection = result.Tables;
                            dt = tableCollection[0];
                            oldReg = OldRegistrations();

                            oldRegList = Differences.Regs(dt, oldReg, false);
                            newRegList = Differences.Regs(dt, oldReg, true);

                            ClearOldPolicy.Remove(oldRegList);

                            Write(dt, records, newRegList);
                            stream.Close();
                            CleanUp(file);
                        }
                    }
                }
                if (!runSuccess)
                {
                    string msg = "Error. No source file found."; 
                    Log.WriteLine(msg);

                    AddErrorLog(msg); 
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine("Error. Could not read excel file: " + ex.Message);
                AddErrorLog(ex.Message); 
            }
        }

        private static void AddErrorLog(string msg)
        {
            {
                try
                {

                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ErrorLog"].ConnectionString))
                    {

                            using (var cmd = new SqlCommand("up_ins_tabProcessLog", con)
                            {
                                CommandType = CommandType.StoredProcedure
                            })
                            {
                                cmd.Parameters.Add("@Routine", SqlDbType.VarChar, 250).Value = msg;
                                cmd.Parameters.Add("@Step", SqlDbType.VarChar, 250).Value = msg;
                                cmd.Parameters.Add("@Outcome", SqlDbType.VarChar, 250).Value = msg;
                                cmd.Parameters.Add("@Description", SqlDbType.VarChar).Value = msg;

                                con.Open();
                                cmd.ExecuteNonQuery();
 }
                        }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void Write(DataTable dt, int records, List<Vehicle> newRegList)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AioiVehicleDetails"].ToString();
            int rowIndex = 0;
            int updateCount = 0; 
            List<string> aioiPolicyList = new List<string>();
            aioiPolicyList = GetAioiPolicyList();



            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();

                    using (connection)
                    {
                        using (connection)
                        {
                            using (SqlCommand cmd = connection.CreateCommand())
                            {
                                cmd.CommandType = CommandType.Text;

                                while (rowIndex < dt.Rows.Count)
                                {
                                    cmd.Parameters.Clear();
                                    DateTime? onDate = null;
                                    DateTime? offDate = null;
                                    DateTime? insertDate = null;
                                    DateTime? updateDate = null;
                                    DateTime? midSentDate = null;
                                    int? midFileNo = null;


                                    cmd.Parameters.AddWithValue("@recordType", dt.Rows[rowIndex]["Record_Type"].ToString().TrimEnd());
                                    //cmd.Parameters.AddWithValue("@policyNum", dt.Rows[rowIndex]["Policy_Number"].ToString().TrimEnd());
                                    cmd.Parameters.AddWithValue("@foreignReg", dt.Rows[rowIndex]["Foreign_Reg_Ind"].ToString().TrimEnd());
                                    cmd.Parameters.AddWithValue("@registration", dt.Rows[rowIndex]["Registration_Number"].ToString().TrimEnd());
                                    cmd.Parameters.AddWithValue("@vehDerivative", dt.Rows[rowIndex]["Vehicle_Derivative"].ToString().TrimEnd());
                                    cmd.Parameters.AddWithValue("@driver", dt.Rows[rowIndex]["Named_Driver"].ToString().TrimEnd());
                                    cmd.Parameters.AddWithValue("@tradePlate", dt.Rows[rowIndex]["Trade_Plate_Ind"].ToString().TrimEnd());
                                    cmd.Parameters.AddWithValue("@activeFlag", dt.Rows[rowIndex]["Active_Flag"].ToString().TrimEnd());
                                    cmd.Parameters.AddWithValue("@midSent", dt.Rows[rowIndex]["MID_Sent_Ind"].ToString().TrimEnd());
                                    cmd.Parameters.AddWithValue("@midStatus", dt.Rows[rowIndex]["MID_Status"].ToString().TrimEnd());
                                    cmd.Parameters.AddWithValue("@midError", dt.Rows[rowIndex]["MID_Error_Codes"].ToString().TrimEnd());
                                    cmd.Parameters.AddWithValue("@vehicleMake", dt.Rows[rowIndex]["Vehicle_Make"].ToString().TrimEnd());
                                    cmd.Parameters.AddWithValue("@vehicleModel", dt.Rows[rowIndex]["Vehicle_Model"].ToString().TrimEnd());
                                    cmd.Parameters.AddWithValue("@insertedBy", dt.Rows[rowIndex]["Inserted_By"].ToString().TrimEnd());
                                    cmd.Parameters.AddWithValue("@updatedBy", dt.Rows[rowIndex]["Updated_By"].ToString().TrimEnd());
                                    cmd.Parameters.AddWithValue("@dateCreated", DateTime.Now);

                                    if (dt.Rows[rowIndex]["MID_File_Seq_No"].ToString().Length > 0 && !dt.Rows[rowIndex]["MID_File_Seq_No"].ToString().Equals("NULL"))
                                    {
                                        midFileNo = int.Parse(dt.Rows[rowIndex]["MID_File_Seq_No"].ToString());
                                        cmd.Parameters.AddWithValue("@midFileNo", midFileNo);
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("midFileNo", midFileNo).Value = DBNull.Value;
                                    }

                                    if (dt.Rows[rowIndex]["Cover_Start_Date"].ToString().Length > 0 && !dt.Rows[rowIndex]["Cover_Start_Date"].ToString().Equals("NULL"))
                                    {
                                        onDate = ConvertedDate(dt.Rows[rowIndex]["Cover_Start_Date"].ToString());
                                        cmd.Parameters.AddWithValue("@onDate", onDate);
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@onDate", onDate).Value = DBNull.Value;
                                    }

                                    if (dt.Rows[rowIndex]["Cover_End_Date"].ToString().Length > 0 && !dt.Rows[rowIndex]["Cover_End_Date"].ToString().Equals("NULL"))
                                    {
                                        offDate = ConvertedDate(dt.Rows[rowIndex]["Cover_End_Date"].ToString());
                                        cmd.Parameters.AddWithValue("@offDate", offDate);
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@offDate", offDate).Value = DBNull.Value;
                                    }

                                    if (dt.Rows[rowIndex]["Insert_Date"].ToString().Length > 0 && !dt.Rows[rowIndex]["Insert_Date"].ToString().Equals("NULL"))
                                    {
                                        insertDate = ConvertedDate(dt.Rows[rowIndex]["Insert_Date"].ToString());
                                        cmd.Parameters.AddWithValue("@insertDate", insertDate);
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@insertDate", insertDate).Value = DBNull.Value;
                                    }

                                    if (dt.Rows[rowIndex]["Update_date"].ToString().Length > 0 && !dt.Rows[rowIndex]["Update_date"].ToString().Equals("NULL"))
                                    {
                                        updateDate = ConvertedDate(dt.Rows[rowIndex]["Update_date"].ToString());
                                        cmd.Parameters.AddWithValue("@updateDate", updateDate);
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@updateDate", updateDate).Value = DBNull.Value;
                                    }

                                    if (dt.Rows[rowIndex]["Mid_Sent_Date"].ToString().Length > 0 && !dt.Rows[rowIndex]["Mid_Sent_Date"].ToString().Equals("NULL"))
                                    {
                                        midSentDate = ConvertedDate(dt.Rows[rowIndex]["Mid_Sent_Date"].ToString());
                                        cmd.Parameters.AddWithValue("@midSentDate", midSentDate);
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@midSentDate", midSentDate).Value = DBNull.Value;
                                    }

                                    //if (dt.Rows[rowIndex]["Policy_Number"].ToString().TrimEnd().Equals("F43063") || dt.Rows[rowIndex]["Policy_Number"].ToString().TrimEnd().Equals("F43075") ||
                                    //    dt.Rows[rowIndex]["Policy_Number"].ToString().TrimEnd().Equals("F43038") || dt.Rows[rowIndex]["Policy_Number"].ToString().TrimEnd().Equals("F42435R"))
                                    //{

                                        string polRefToCheck = dt.Rows[rowIndex]["Policy_Number"].ToString().TrimEnd();                                        
                                        string checkedPolRef = LinkedPolicy(polRefToCheck,aioiPolicyList);

                                        cmd.Parameters.AddWithValue("@policyNum", checkedPolRef);


                                    int result = newRegList.IndexOf(Differences.ConvertToVehicleNew(dt.Rows[rowIndex]));



                                    if (result != -1)
                                    {
                                        cmd.CommandText = "INSERT INTO dbo.TabAIOIVehicleDetails(recordType_char,policyNum_char,foreignRegInd_char,registrationNumber_char," +
                                            "vehicleDerivative_char,namedDriver_char,tradePlate_char,coverStartDate_date,coverEndDate_date,insertDate_date,updateDate_date," +
                                            "activeFlag_char,midFileSeqNo_int,midSentInd_char,midSentDate_date,midStatus_char,midErrorCodes_char,vehicleMake_char,vehicleModel_char," +
                                            "insertedBy_char,updatedBy_char,dateCreated_date)" +
                                            "VALUES(@recordType,@policyNum,@foreignReg,@registration,@vehDerivative,@driver,@tradePlate,@onDate,@offDate,@insertDate,@updateDate,@activeFlag," +
                                            "@midFileNo,@midSent,@midSentDate,@midStatus,@midError,@vehicleMake,@vehicleModel,@insertedBy,@updatedBy,@dateCreated)";

                                        cmd.ExecuteNonQuery();
                                        updateCount++;
                                        Log.WriteLine("Policy: " + checkedPolRef + ", Registration: " + dt.Rows[rowIndex]["Registration_Number"].ToString().TrimEnd() + " Saved.");
                                    }
                                    else
                                    {
                                        Log.WriteLine("NOT ADDED: " + checkedPolRef + ", Registration: " + dt.Rows[rowIndex]["Registration_Number"].ToString().TrimEnd() +" Because result = 0"); 
                                    }


                                    //}

                                    rowIndex++;
                                }
                            }
                        }

                        Log.WriteLine("Uploaded " + updateCount + " new records");
                    }
                }
            }
        }

        //Convert any dates to sql DateTime format - PREVIOUS 2 VERSIONS OF AIOI TEST DATA WAS NOT IN SQL DATE FORMAT SO NEEDED CONVERTING
        private static DateTime ConvertedDate(string oldDate)
        {

            DateTime convertedDate;

            if (DateTime.TryParse(oldDate, out convertedDate))
            {
                return convertedDate;
            }
            else
            {
                return convertedDate;
            }

        }

        private static DataTable OldRegistrations()
        {
            var oldReg = new DataTable();

            string connectionString = ConfigurationManager.ConnectionStrings["AioiVehicleDetails"].ToString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM tabAIOIVehicleDetails;";
                    cmd.ExecuteNonQuery();

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(oldReg);
                    da.Dispose();
                }

                return oldReg;

            }
        }
        public static void CleanUp(string file)
        {
            var newFileDestination = Path.Combine(ConfigurationManager.AppSettings["ArchPath"] + Path.GetFileName(file));

            File.Move(file, newFileDestination);
            Log.WriteLine("File archived to " + newFileDestination);

        }

     

        private static string LinkedPolicy(string polRefToCheck, List<string> aioiPolicyList)
        {
            //Check to see if there are any associated policies that the vehicle should be linked to 
            
            string checkedPolRef = "";


            if (polRefToCheck.Length == 11)
            { 
                polRefToCheck = polRefToCheck.Substring(0, 6);
            }


            bool exists = aioiPolicyList.Any(s => s.Contains(polRefToCheck));

            //if we don't have that policy, do we have it without the letter on the end? 
            if (!exists)
            {
                var mappedPolFound = aioiPolicyList.Where(s => s == polRefToCheck[0..^1]);

                //if  we still don't have it, do we have it with a different letter on the end? 
                if (mappedPolFound.Count() < 1)
                {
                    var postfixFound = aioiPolicyList.Where(s => s.Remove(s.Length - 1).Equals(polRefToCheck.Remove(polRefToCheck.Length - 1)));

                    if (postfixFound.Count() < 1)
                    {
                        Log.WriteLine(polRefToCheck + "cannot be found");
                    }
                    else
                    {
                        checkedPolRef = postfixFound.First();

                    }
                }
                else
                {
                    checkedPolRef = polRefToCheck.Remove(polRefToCheck.Length - 1);
                }

            }
            else
            {
                checkedPolRef = polRefToCheck; 
            }


            //These policy refs need to be changed to Motor Ichiban ref MU00000
            if (polRefToCheck.Equals("F43063") || polRefToCheck.Equals("F43075") || polRefToCheck.Equals("F43038"))
            {
                checkedPolRef = "MU00000";
            }


            return checkedPolRef;
        }

        private static List<string> GetAioiPolicyList()
        {
            var dt = new DataTable();

            string connectionString = ConfigurationManager.ConnectionStrings["AioiVehicleDetails"].ToString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT DISTINCT (PolicyNumber_char) FROM dbo.tabAIOIPolicy;";
                    cmd.ExecuteNonQuery();

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    da.Dispose();
                }

                List<string> aioiPolicyList = dt.Rows.OfType<DataRow>().Select(dr => dr.Field<string>("policyNumber_char")).ToList();

                return aioiPolicyList;
            }
        }
    }
}
