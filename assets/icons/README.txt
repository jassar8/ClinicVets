Optional application icon for WinForms:

1. Add a file named app.ico in this folder.
2. In src/ClinicVets.Desktop/ClinicVets.Desktop.csproj add:
   <PropertyGroup>
     <ApplicationIcon>..\..\assets\icons\app.ico</ApplicationIcon>
   </PropertyGroup>

The desktop app runs correctly without a custom icon (Windows default is used).
