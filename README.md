# ZEISS MachineStream  
# Developed in .NET Core 2.2  
  
Smart maintenance solution for large industrial clients using different ZEISS products such as microscopes and measurement machines. It is planned to allow an operator to monitor these assets remotely in near real-time.  
  
The working example of the project is deployed @https://zeissmachinestream.azurewebsites.net  
  
For the API Documentation @https://zeissmachinestream.azurewebsites.net/swagger/index.html  
  
I would suggest to open the project using VS 2017 since I create the folder tree directly from the IDE  
and is not mantained in the same way using other editors/IDE, however there are no problems using VS Code or others.  

The Web Application contains the Dockerfile to build the image for a Linux Docker container,  
the base image used to deploy the container is microsoft/dotnet:2.2-aspnetcore-runtime  
  
To run the project locally:  
- Download .NET Core SDK 2.2 (I've used the 2.2.106) from https://dotnet.microsoft.com/download/dotnet-core/2.2  
	To run locally without Docker:  
	- Restore the NuGet packages (ZEISSMachineStream and MachineStreamCore)  
		dotnet restore  
	- Run the solution (ZEISSMachineStream)  
		dotnet run  

	To run on Docker:  
	- Build the Docker image  
		docker build --tag=zeissmachinestream  
	- Run the image  
		docker run -p 4001:80 zeissmachinestream  
  
The DB used is an Azure CosmosDB with MongoDB API  
The backgroud service listeing for the WebSocket is connected to ws://machinestream.herokuapp.com/ws  

There are Unit, Integration and Functional test, to run the tests:  
- From VS:
	DropDown menu "Test" -> "Run" -> "All test"  
- Using CLI:  
	Navigate to the folder of the solution "dotnet test"  