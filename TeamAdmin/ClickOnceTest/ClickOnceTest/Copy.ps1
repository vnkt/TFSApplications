Param(
	[string]$directory = $env:BUILD_SOURCESDIRECTORY
	#This is to create a variable that picks the environment variable for the working directory for the build agent
)
$append = "\ClickOnceTest\ClickOnceTest\bin\Debug\app.publish\*"
#This is to create the variable that represents the organization structure for where the clickonce files go
#ClickOnceTest is my solution name and hence this path

$finaldirectory =  $directory + $append
#Getting the file location of where the ClickOnce files are present on the build server

$dest = "\\vnkt-server\ClickOnce"
#Variable for the UNC publish paht

Copy-Item $finaldirectory $dest -recurse -Force
#Recursive copy