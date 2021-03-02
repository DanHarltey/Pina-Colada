dotnet restore ..\ || goto :error
dotnet build ..\PinaColada.sln --configuration Release --nologo --no-restore || goto :error
dotnet test ..\PinaColada.sln --configuration Release --nologo --no-build --no-restore --verbosity normal || goto :error

goto :EOF

:error
echo Failed with error #%errorlevel%.
exit /b %errorlevel%