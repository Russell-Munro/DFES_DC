# Universal Data Connector

#Development Installation

SDK Installer 
- ~~2.1 https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.1.802-windows-x64-installer~~
- 9.0 https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-9.0.300-windows-x64-installer

#Hosting Installation

Runtime installer 
- ~~2.1 https://dotnet.microsoft.com/download/thank-you/dotnet-runtime-2.1.13-windows-hosting-bundle-installer~~ 
- 9.0 https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-9.0.5-windows-hosting-bundle-installer

## local developer setup

1. Establish the VolunteerHub siteFinity project in your local environment. 
git clone https://bitbucket.org/Equilibrium/volunteerhub.dfes.wa.gov.au/src/master/
2. Create a new web site in IIS, point it to the SitefinityWebApp folder with host name of volunteerhub-dev.equ.com.au
3. Establish UniversalDataConnector project in your local environment. 
got clone https://bitbucket.org/Equilibrium/universaldataconnector
4. Launch the DataConnectorUI project and make note of the url and port number. 
4. Check the sitefinity site is using the name AdminUIUrl value that you noted in previous step. 
https://volunteerhub-dev.equ.com.au/Sitefinity/Administration/Settings/Advanced : DataConnector > AdminUIUrl  
3. Login to the site finity Open > Admin Data Connector Management 


## Sitefinity Upgrade - development plan

When updating any project in the solution, please use [Sitefinity CLI](https://equilibrium.atlassian.net/wiki/spaces/EQ/pages/1598816408/Sitefinity+Upgrade+By+CLI)

1. Firstly upgrade the UniversalDataConnector project to ensure the UDC.SitefinityContextPlugin is upgraded, _it must be deployed to Teamcity before proceeding_.
    - The Sitefinity CMS project has already integrated (added reference by Teamcity NuGet Package Source) with a custom UniversalDataConnector project
    - Depending on the replace [Version] with the actual version to upgrade to eg 11.2.6934 > 13.3.7628
1. At the Sitefinity CMS project, manually update the version number of `UniversalDataConnector` assemble in the `packages.config` file e.g.`<package id="UDC.SitefinityContextPlugin" version="13.3.7628" targetFramework="net48" />`
1. Run Sitefinity CLI upgrade command for the CMS project
1. Build and check compile errors
1. Check and confirm the correct config which was modified by the CLI
1. Before starting the website do the following
    1. Remove Bootstrap and Bootstrap4 from both SitefinityWebApp and RandomSiteControlsMVC
    1. Reset RazorGeneratorMVCStart.cs, SampleTemplate.cshtml, `App_Readme MVC/\*/readme.txt` in all projects (from Visual Studio)


===========
Sitefinity Upgrades (optional - nuget package upgrade directly)
===========
# Change the version numbers of the assembly build outputs and if a major version the UDC.SitefinityIntegrator\Platform.cs version.	
# Depending on the replace [Version] with the actual version to upgrade to eg 11.2.6934, Note since 13 OpenAccess has it's own version
#		- Install-Package Telerik.Sitefinity.Core -Version [Version] -ProjectName UDC.SitefinityContextPlugin
#		- Install-Package Telerik.Sitefinity.Feather -Version [Version] -ProjectName UDC.SitefinityContextPlugin
#		- Install-Package Telerik.Sitefinity.OpenAccess -Version [Version] -ProjectName UDC.SitefinityContextPlugin
#	Delete ResourcePackages from Visual Studio
#	Duplicate UDC.Nuget/UDC.SitefinityContextPlugin.11.2.6934.nuspec and rename to UDC.SitefinityContextPlugin.[Version].nuspec
#	Build the solution (Ensure to use Release)
#	Edit UDC.SitefinityContextPlugin.[Version].nuspec
#	Package with .\nuget.exe pack .\UDC.SitefinityContextPlugin.[Version].nuspec
#	Force add the new nuget package as it is git ignored.

===========
Setting up environments
===========
Run these in an Administrative Command Prompt
setx ASPNETCORE_ENVIRONMENT "DEV" /m
setx ASPNETCORE_URLS "http://localhost:5000;" /m (or whatever port you were going with for the UI)

Install the certificate and get the thumbprint
To use the netssh you need to bind it:
netsh http add sslcert ipport=0.0.0.0:8080 certhash=53f496c6dcbef4d7b2b9ba0f11ed7c2e366b9015 appid="{86551017-05CD-4F04-92A0-BD888C9C60A5}"