@echo off
setlocal enabledelayedexpansion

:: Set the source folder (current directory if not specified)
set "source_folder=."
:: Set the output file name
set "output_file=combined_cs_files.md"

:: Check if source folder exists
if not exist "%source_folder%" (
    echo Error: Source folder "%source_folder%" does not exist.
    exit /b 1
)

:: Clear the output file if it exists
if exist "%output_file%" del "%output_file%"

:: Process all .cs files in the source folder
for %%f in ("%source_folder%\*.cs") do (
    echo Processing: %%~nxf
    
    :: Write the filename as a markdown heading
    echo ## %%~nxf >> "%output_file%"
    echo. >> "%output_file%"
    
    :: Write the markdown code block opening
    echo ```csharp >> "%output_file%"
    
    :: Copy the file contents
    type "%%f" >> "%output_file%"
    
    :: Write the markdown code block closing
    echo. >> "%output_file%"
    echo ``` >> "%output_file%"
    echo. >> "%output_file%"
    echo. >> "%output_file%"
)

echo.
echo Done! Combined file created: %output_file%
echo Total .cs files processed.