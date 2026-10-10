; Instalador de Kepler-7: Protocolo Fénix (Inno Setup 6).
; Antes de compilarlo hay que publicar el juego en la carpeta "publicar" de la raíz:
;   dotnet publish src/Kepler7.Game -c Release -r win-x64 --self-contained false -o publicar
;   ISCC instalador\Kepler7.iss

#define Nombre "Kepler-7: Protocolo Fénix"
#define NombreCorto "Kepler-7"
#define Version "1.0.0"
#define Ejecutable "Kepler7.Game.exe"
#define Publicado "..\publicar"
#define Manual "Manual de instalacion.pdf"
#define HayManual FileExists(AddBackslash(SourcePath) + Publicado + "\" + Manual)

[Setup]
AppId={{935C0B50-1594-4D98-8ABB-C466A3AF198E}
AppName={#Nombre}
AppVersion={#Version}
AppPublisher=Grupo 9 - UAI
DefaultDirName={autopf}\Kepler7
DefaultGroupName={#NombreCorto}
DisableProgramGroupPage=yes
DisableWelcomePage=no
; Se instala para el usuario actual: no pide permisos de administrador.
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayName={#Nombre}
UninstallDisplayIcon={app}\{#Ejecutable}
OutputDir=salida
OutputBaseFilename=Kepler7-Setup-{#Version}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "escritorio"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#Publicado}\*"; DestDir: "{app}"; Excludes: "*.pdb,appsettings.json"; Flags: ignoreversion recursesubdirs createallsubdirs
; Si en una instalación anterior cambiaron la cadena de conexión, no se pisa.
Source: "{#Publicado}\appsettings.json"; DestDir: "{app}"; Flags: onlyifdoesntexist
; El juego crea la base solo; el script va igual por si hay que correrlo a mano.
Source: "..\db\01_crear_base.sql"; DestDir: "{app}\db"; Flags: ignoreversion

[Icons]
Name: "{group}\{#NombreCorto}"; Filename: "{app}\{#Ejecutable}"
#if HayManual
Name: "{group}\Manual de instalación"; Filename: "{app}\{#Manual}"
#endif
Name: "{group}\Desinstalar {#NombreCorto}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#NombreCorto}"; Filename: "{app}\{#Ejecutable}"; Tasks: escritorio

[Run]
Filename: "{app}\{#Ejecutable}"; Description: "{cm:LaunchProgram,{#NombreCorto}}"; Flags: nowait postinstall skipifsilent; Check: TieneRuntime

[Code]
const
  PaginaRuntime = 'https://dotnet.microsoft.com/es-es/download/dotnet/8.0';

// El runtime de escritorio queda en Archivos de programa\dotnet\shared, una carpeta por versión.
function TieneRuntime: Boolean;
var
  Busqueda: TFindRec;
begin
  Result := FindFirst(ExpandConstant('{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App\8.*'), Busqueda);
  if Result then
    FindClose(Busqueda);
end;

function InitializeSetup: Boolean;
var
  Codigo: Integer;
begin
  Result := True;
  if not TieneRuntime then
    if SuppressibleMsgBox('Para jugar hace falta el .NET 8 Desktop Runtime (x64) y no está instalado en este equipo.' + #13#10#13#10 +
        '¿Abrir la página de descarga de Microsoft? La instalación de Kepler-7 sigue igual.',
        mbConfirmation, MB_YESNO, IDNO) = IDYES then
      ShellExec('open', PaginaRuntime, '', '', SW_SHOWNORMAL, ewNoWait, Codigo);
end;
