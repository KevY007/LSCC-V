@echo off

REM Current directory
set "CURRENT_DIR=%CD%"

REM Check if the "MSBuildExe" environment variable is set
if "%MSBuildExe%" == "" (
    echo "MSBuildExe" environment variable is not set.
    pause
    exit /b 1
)

REM Build the solution
"%MSBuildExe%" "%CURRENT_DIR%\LSCC.sln"

REM URL to make the GET request
set "BASE_URL=https://panel.gm.miami/api/client/servers/696a9fa2"
set "GET_UPLOAD_URL_ROUTE=%BASE_URL%/files/upload"
set "SERVER_RESTART_ROUTE=%BASE_URL%/power"
set "PANEL_KEY=ptlc_Oe07ZqnQKzEjkkidSj1Se26GMiT7SGrgp6XhZJFdJLM"

REM Make the initial curl request and store the response in a temporary file
curl "%GET_UPLOAD_URL_ROUTE%" -H "Authorization: Bearer %PANEL_KEY%" -H "Content-Type: application/json" -H "Accept: Application/vnd.pterodactyl.v1+json" -s -o get_url_output.txt

REM Use jq to extract attributes.url directly from the JSON response
"%CURRENT_DIR%\jq-win64.exe" ".attributes.url" get_url_output.txt > extracted_url_output.txt

REM Read the extracted URL from the file into a variable
set /p UPLOAD_ROUTE=<extracted_url_output.txt

REM Remove the double quotes from the variable
set "UPLOAD_ROUTE=%UPLOAD_ROUTE:"=%"

REM Check if the extracted URL is empty
if not defined UPLOAD_ROUTE (
    echo attributes.url not found in the JSON response.
    pause
)

REM Upload the files
curl "%UPLOAD_ROUTE%&directory=/dotnet/resources/RNC" -F "files=@%CURRENT_DIR%\LSCC\bin\Debug\netcoreapp3.1\RNC.dll" -F "files=@%CURRENT_DIR%\LSCC\bin\Debug\netcoreapp3.1\RNC.pdb"

REM Delete the temporary output files
del get_url_output.txt
del extracted_url_output.txt

REM Restart the server
curl "%SERVER_RESTART_ROUTE%" -H "Authorization: Bearer %PANEL_KEY%" -F "signal=restart"

REM Pause to prevent the window from closing immediately for debugging purposes
REM pause