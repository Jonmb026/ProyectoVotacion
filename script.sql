-- Creación de la base de datos
CREATE DATABASE ProyectoVotacion;
GO

USE ProyectoVotacion;
GO


CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Cedula NVARCHAR(50) NOT NULL UNIQUE,
    Nombre NVARCHAR(100) NOT NULL,
    PrimerApellido NVARCHAR(100) NOT NULL,
    SegundoApellido NVARCHAR(100) NOT NULL,
    FechaNacimiento DATE NOT NULL,
    Provincia NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(100) NOT NULL,
    Rol NVARCHAR(50) NOT NULL -- 'Admin' o 'Votante'
);
GO

CREATE TABLE Candidatos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    PrimerApellido NVARCHAR(100) NOT NULL,
    SegundoApellido NVARCHAR(100) NOT NULL,
    Cedula NVARCHAR(50) NOT NULL UNIQUE,
    Cargo NVARCHAR(50) NOT NULL, -- 'Presidente', 'Alcalde', 'Diputado'
    Provincia NVARCHAR(100) NOT NULL,
    Partido NVARCHAR(100) NOT NULL,
    Eslogan NVARCHAR(255)
);
GO

CREATE TABLE Votos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    CandidatoId INT NOT NULL,
    FechaVoto DATETIME NOT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    FOREIGN KEY (CandidatoId) REFERENCES Candidatos(Id),
    UNIQUE (UsuarioId, CandidatoId) -- Un usuario solo puede votar una vez por un candidato
);
GO

-- Creación de índices para optimizar las consultas
CREATE INDEX IDX_Usuarios_Cedula ON Usuarios(Cedula);
CREATE INDEX IDX_Candidatos_Cedula ON Candidatos(Cedula);
CREATE INDEX IDX_Votos_UsuarioId_CandidatoId ON Votos(UsuarioId, CandidatoId);
GO

