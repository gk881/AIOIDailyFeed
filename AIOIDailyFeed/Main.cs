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
                List<string> oldRegList = new List<string>();
                List<string> newRegList = new List<string>(); 


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
                    Log.WriteLine("Error. No source file found. ");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine("Error. Could not read excel file: " + ex.Message);
            }
        }

        public static void Write(DataTable dt, int records, List<string> newRegList)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AioiVehicleDetails"].ToString();
            int rowIndex = 0;
            int updateCount = 0;


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
                                    cmd.Parameters.AddWithValue("@policyNum", dt.Rows[rowIndex]["Policy_Number"].ToString().TrimEnd());
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

                                    
                                    if (!updateDate.Equals(null))
                                    {
                                        int result = newRegList.IndexOf(dt.Rows[rowIndex]["Registration_Number"].ToString().TrimEnd());
                                        if (result != -1)
                                        //if (updateDate.Value > DateTime.Now.AddDays(-1) && updateDate.Value < DateTime.Now || records == 0)
                                        {
                                            cmd.CommandText = "INSERT INTO dbo.TabAIOIVehicleDetails(recordType_char,policyNum_char,foreignRegInd_char,registrationNumber_char," +
                                                "vehicleDerivative_char,namedDriver_char,tradePlate_char,coverStartDate_date,coverEndDate_date,insertDate_date,updateDate_date," +
                                                "activeFlag_char,midFileSeqNo_int,midSentInd_char,midSentDate_date,midStatus_char,midErrorCodes_char,vehicleMake_char,vehicleModel_char," +
                                                "insertedBy_char,updatedBy_char,dateCreated_date)" +
                                                "VALUES(@recordType,@policyNum,@foreignReg,@registration,@vehDerivative,@driver,@tradePlate,@onDate,@offDate,@insertDate,@updateDate,@activeFlag," +
                                                "@midFileNo,@midSent,@midSentDate,@midStatus,@midError,@vehicleMake,@vehicleModel,@insertedBy,@updatedBy,@dateCreated)";

                                            cmd.ExecuteNonQuery();
                                            updateCount++;
                                            Log.WriteLine("Policy: " + dt.Rows[rowIndex]["Policy_Number"].ToString().TrimEnd() + ", Registration: " + dt.Rows[rowIndex]["Registration_Number"].ToString().TrimEnd() + " Saved.");
                                        }
                                    }
                                    else
                                    {
                                        Log.WriteLine("ERROR: Policy: " + dt.Rows[rowIndex]["Policy_Number"].ToString().TrimEnd() + ", Registration: " + dt.Rows[rowIndex]["Registration_Number"].ToString().TrimEnd() + " not saved due to NULL update date.");
                                    }

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

        }
    } 
