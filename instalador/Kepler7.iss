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
  PaginaLocalDB = 'https://learn.microsoft.com/es-es/sql/database-engine/configure-windows/sql-server-express-localdb';
  ClaveLocalDB = 'SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions';
  ClaveInstancias = 'SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL';

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

// LocalDB anota cada versión instalada como una subclave: alcanza con que haya una.
function TieneLocalDB: Boolean;
var
  Versiones: TArrayOfString;
begin
  Result := RegGetSubkeyNames(HKLM64, ClaveLocalDB, Versiones) and (GetArrayLength(Versiones) > 0);
end;

// Un SQL Server común (Express o superior) figura con el nombre de su instancia.
function TieneOtroSqlServer: Boolean;
var
  Instancias: TArrayOfString;
begin
  Result := (RegGetValueNames(HKLM64, ClaveInstancias, Instancias) and (GetArrayLength(Instancias) > 0))
    or (RegGetValueNames(HKLM32, ClaveInstancias, Instancias) and (GetArrayLength(Instancias) > 0));
end;

procedure AbrirPaginaLocalDB(Sender: TObject);
var
  Codigo: Integer;
begin
  ShellExec('open', PaginaLocalDB, '', '', SW_SHOWNORMAL, ewNoWait, Codigo);
end;

// El juego viene configurado para LocalDB. Si no está, se instala igual, pero se avisa
// en una página propia que sin SQL Server el progreso no se guarda y cómo se resuelve.
procedure InitializeWizard;
var
  Pagina: TOutputMsgWizardPage;
  Boton: TNewButton;
  Texto: String;
begin
  if TieneLocalDB then
    Exit;

  if TieneOtroSqlServer then
    Texto := 'Kepler-7 viene configurado para SQL Server Express LocalDB, que en este equipo no está instalado. Sí hay otro SQL Server.' + #13#10#13#10 +
      'El juego se instala y se puede jugar igual. Hasta que se conecte a un servidor, la partida queda en memoria y se pierde al cerrarlo; la pantalla de inicio lo avisa.' + #13#10#13#10 +
      'Para que el progreso se conserve hay dos caminos: instalar LocalDB, o indicar el servidor que ya existe en el archivo appsettings.json de la carpeta del juego. Los dos están explicados en la sección 6 del manual de instalación.'
  else
    Texto := 'Kepler-7 guarda el progreso en SQL Server y en este equipo no hay ninguno instalado.' + #13#10#13#10 +
      'El juego se instala y se puede jugar igual. Mientras falte, la partida queda en memoria y se pierde al cerrarlo; la pantalla de inicio lo avisa.' + #13#10#13#10 +
      'Para que el progreso se conserve hace falta SQL Server Express LocalDB, gratuito y distribuido por Microsoft. Se puede instalar ahora o más adelante: la próxima vez que se abra, el juego lo detecta y crea la base solo. Los pasos están en la sección 6 del manual de instalación.';

  Pagina := CreateOutputMsgPage(wpWelcome, 'Base de datos',
    'No se encontró SQL Server Express LocalDB en este equipo', Texto);

  Boton := TNewButton.Create(Pagina);
  Boton.Parent := Pagina.Surface;
  Boton.Caption := 'Abrir la página de Microsoft sobre LocalDB';
  Boton.Width := ScaleX(270);
  Boton.Height := ScaleY(25);
  Boton.Top := Pagina.SurfaceHeight - Boton.Height;
  Boton.OnClick := @AbrirPaginaLocalDB;
end;
