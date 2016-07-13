
Last Revision: 2016-07-12  - STM
********************************

With respect to setting up the back end services, I think everything we minimally need, we have in the following.

Prerequisites:

	1)	Setup DB:    
		a.	..\scrap-runner\DatabaseScripts\01-INSTALLATION_CHECKLIST.txt

	2)	Setup C#:
		a.	Tutorial Prerequisite for how to self-sign a cert and apply it to an IP address and port number:  http://camvm-tcs-1:81/repository/download/BradyGroupDevelopment_BwfDocumentationBlessed/.lastSuccessful/htmloutput/prerequisites.html
		b.	Scrap Runner Wiki for how to self-sign a cert and apply it to a full hostname and port number:  https://github.com/BradyMetals/scrap-runner/wiki 

How to Deploy and Configure C# 

	1) Manual build within VS by
		Selecting Brady.ScrapRunner.Server solution
		Selecting Release / Any CPU
		Clean and rebuild solution
		zip up the output directory, for example 
		...\scrap-runner\src\Brady.ScrapRunner.Server\Brady.ScrapRunner.Host\output --> output-20160629.zip

	2) Move zip file to host of interest. Here we are assuming we are deploying to jacvs-dev12 host and the ScrapDev2 instance.
		For now, at least, unzip the output-20160629.zip folder to C:\ScrapDev2\Scrap\ScrapServices\output\

	3) (First time only) Edit the renamed App.configs to suit:
		Brady.ScrapRunner.Host.exe.config
		Brady.ScrapRunner.Host.vshost.exe.config  (Optional, this one should eventually go away on prod builds)

	    Keep backup copies for next deployment (in C:\ScrapDev2\Scrap\ScrapServices\ as sibling to the output\ directory)

	4) (First time only) Install as a service.  Should only need to do this 1 time.  (See Uninstall/Reinstall below if necesary):
	    C:> cd C:\ScrapDev2\Scrap\ScrapServices\output\
	    C:> Brady.ScrapRunner.host.exe install ^
		-instance:bwfSrvcScrapDev2 ^
		-displayname:bwfSrvcScrapDev2 ^
		-description "BFW Scrap Run Service for Dev2" ^
	        --manual

		OR reiew below details, edit to suite, then cut and paste:
		Brady.ScrapRunner.host.exe install -instance:bwfSrvcScrapDev2 -displayname:bwfSrvcScrapDev2 -description "BFW Scrap Run Service for Dev2" --manual

	5) To get the connections going against SQLServer (may not need all this, but this generally seems to work):

		Security Logins

			Added login for host:   BRADYPLC\JACVS-DEV12$			(NOTE trailing $ symbol!)
				(securables) JACVS-DEV12 server was granted connect
				(status) connection granted, login enabled

			Adjusted Login for: NT AUTHORITY\SYSTEM
				(server roles) sysadmin 				(??? that might be too much permission!)
				(user mapping) into ScrapDev2, grant db_owner role

	6) For now, we'll self sign the cert so that we have (for example)

		https://jacvs-dev12.bradyplc.com:7777
        	        Service display name:  bwfSrvcScrapDev2 (Instance: bwfSrvcScrapDev2)    
			Service is manual at the moment
	                Deployed to:  C:\ScrapDev2\Scrap\ScrapServices\...
	                Database:  Source=JACVS-DEV12;Initial Catalog=ScrapDev2

	7) Uninstall/Reinstall

		7a) Usually you can just upgrade by replacing the output diectory and the corercet config files.

			1) Stop service:  bwfSrvcScrapDev2 (Instance: bwfSrvcScrapDev2)
			2) cd C:\ScrapDev2\Scrap\ScrapServices\Brady.ScrapRunner.Host.exe.config into C:\ScrapDev2\Scrap\ScrapServices\output\
			3) For now copy in the "vshost" equivalent file too.  Eventually it goes away.
			4) Restart the service:  bwfSrvcScrapDev2 (Instance: bwfSrvcScrapDev2)

		7b) Should you ever need to uninsall and reinstall the service.  Assume we have staged folders output/ and output-new/

			1) Stop service:  bwfSrvcScrapDev2 (Instance: bwfSrvcScrapDev2)
			2) cd C:\ScrapDev2\Scrap\ScrapServices\output\
			3) Brady.ScrapRunner.host.exe uninstall -instance:bwfSrvcScrapDev2 
			4) cd ..
			5) ren output output-old
			6) ren output-new output
    			<<<< Repeat Step 4 >>>


