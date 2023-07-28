# AIOIDailyFeed
Import process to upload AIOI vehicle data
-Runs on ISPROC1 at 10am Daily 
-Aioi send an excel sheet to us via FTP daily which is then pulled into target folder 
-Int records function was initially used on an older build, can be removed now if we have time
-Finds file and converts into Datatable 
-Pulls all existing records from SQL01 > TPADATA > AIOIVEHICLEDETAILS into another datatable 
-These are then used to populate 2 lists and compared against each other with a vehicle class containing Reg and Pol Expiry date only  
-List1 is all the vehicles that we have records for but have dropped off the new Excel file 
-List2 is all the vehicles that are on the new file that we don't have in our records
-If List1 vehicles have a cover end date in the future, this field is updated to be today's date at midnight - effectively ending the vehicle's cover 
-List2 vehicles are inserted into AIOIVEHICLEDETAILS 
-The target file is then archived in N\Daily files 

