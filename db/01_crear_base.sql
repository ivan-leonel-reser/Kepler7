-- Kepler-7: Protocolo Fenix
-- Crea la base y las tablas si no existen. Se puede correr las veces que haga falta.
-- El juego ejecuta este mismo script al arrancar; a mano:
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i db\01_crear_base.sql

IF DB_ID(N'Kepler7') IS NULL
    CREATE DATABASE Kepler7;
GO

USE Kepler7;
GO

-- Una sola partida guardada: la fila 1.
IF OBJECT_ID(N'dbo.Progreso', N'U') IS NULL
CREATE TABLE dbo.Progreso (
    Id INT NOT NULL
        CONSTRAINT PK_Progreso PRIMARY KEY
        CONSTRAINT CK_Progreso_UnaSola CHECK (Id = 1),
    Xp INT NOT NULL,
    Creditos INT NOT NULL,
    OleadasSuperadas INT NOT NULL,
    Actualizado DATETIME2 NOT NULL
        CONSTRAINT DF_Progreso_Actualizado DEFAULT SYSDATETIME()
);
GO

-- Las insignias de esa partida, una fila por patron dominado.
IF OBJECT_ID(N'dbo.Insignia', N'U') IS NULL
CREATE TABLE dbo.Insignia (
    Patron NVARCHAR(40) NOT NULL
        CONSTRAINT PK_Insignia PRIMARY KEY,
    Obtenida DATETIME2 NOT NULL
        CONSTRAINT DF_Insignia_Obtenida DEFAULT SYSDATETIME()
);
GO

-- Registro de la Boveda: los intentos de acceso que el proxy rechazo. No se borra con la partida.
IF OBJECT_ID(N'dbo.IntentoAcceso', N'U') IS NULL
CREATE TABLE dbo.IntentoAcceso (
    Id INT IDENTITY(1, 1) NOT NULL
        CONSTRAINT PK_IntentoAcceso PRIMARY KEY,
    Fecha DATETIME2 NOT NULL,
    Usuario NVARCHAR(60) NOT NULL,
    Rol NVARCHAR(60) NOT NULL,
    Operacion NVARCHAR(20) NOT NULL,
    IdArchivo NVARCHAR(80) NOT NULL
);
GO

-- Historial: una fila por partida terminada, ganada o perdida. Tampoco se borra con la partida.
IF OBJECT_ID(N'dbo.PartidaJugada', N'U') IS NULL
CREATE TABLE dbo.PartidaJugada (
    Id INT IDENTITY(1, 1) NOT NULL
        CONSTRAINT PK_PartidaJugada PRIMARY KEY,
    Fecha DATETIME2 NOT NULL,
    EstacionSalvada BIT NOT NULL,
    OleadasSuperadas INT NOT NULL,
    Nivel INT NOT NULL,
    Xp INT NOT NULL,
    Insignias INT NOT NULL
);
GO
