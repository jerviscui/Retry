@set curdir=%cd%
:: echo %curdir%

:: build
@echo 1. build
@cd %curdir%\src

@cd ./RetryCore
dotnet clean >nul
dotnet build --configuration Release >nul

:: pack
@echo.
@echo 2. pack
@cd %curdir%\nupkgs
@del /s /q /f *.nupkg >nul 2>nul
@cd %curdir%\src

@cd ./RetryCore
dotnet pack -c Release -o %curdir%\nupkgs --no-build >nul

:: publish
@echo.
@echo 3. publish
@cd %curdir%\nupkgs
@for %%i in (*.nupkg) do @(
    :: echo %%i
    dotnet nuget push %%i  --skip-duplicate  -s https://api.nuget.org/v3/index.json  --no-service-endpoint
)

@pause