# TMS_
Implementation of TMS in .NET 6 using SQL Server.

Link to TMS-ReactJS:
https://github.com/MuneelHaider/TMS-ReactJS/

Requirements:
1. Visual Studio 2022 with C# and .NET Modules installed.
2. NodeJS installed on your systems.
3. SQL Server and SQL Server Management Studio.

How to run:
1. Open SQL Server Management Studio.
2. Create a new database with the name 'TMS_'.
3. Please don't do anything else with SQL Server and close it. (Make sure your SQL Server services are active in the background)
4. Open Visual Studio 2022, and clone my repo. I have also provided appsettings.json since it uses a local host and server so there are no database exposure threats.
5. Open appsettings.json, and replace the server name with your server name. We have already created the database with the same name so you don't need to change the rest.
   
   ![image](https://github.com/user-attachments/assets/a27bef19-dda2-4d0f-a0b8-ad023b4a3698)

   
6. You are good to go, run the program using the debugger.
7. I have configured the program to register an admin upon starting if none exists.

   Credentials:

   Username: Muneel

   Password: 123

8. To use the application, please check out the other repo '[TMS-ReactJS](https://github.com/MuneelHaider/TMS-ReactJS)' which contains the frontend webapp.
