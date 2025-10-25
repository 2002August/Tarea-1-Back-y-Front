# UMG Proyecto — API .NET + Frontend Node/EJS

Estructura:
```
umg-proyecto/
├─ backend-dotnet/     # API REST (.NET 8 + EF Core + MySQL)
└─ frontend-node/      # Frontend (Express + EJS)
```

## Requisitos
- .NET 8 SDK
- Node.js LTS
- MySQL Server + Workbench

## Base de datos (ejecuta en MySQL)
CREATE DATABASE universidad;
USE universidad;
CREATE TABLE tipos_sangre (
  id_tipo_sangre INT AUTO_INCREMENT PRIMARY KEY,
  sangre VARCHAR(10) NOT NULL
);

CREATE TABLE estudiantes (
  id_estudiante INT AUTO_INCREMENT PRIMARY KEY,
  carne VARCHAR(10) NOT NULL,
  nombres VARCHAR(100) NOT NULL,
  apellidos VARCHAR(100) NOT NULL,
  direccion VARCHAR(150),
  telefono VARCHAR(20),
  correo_electronico VARCHAR(100),
  id_tipo_sangre INT,
  fecha_nacimiento DATE,
  FOREIGN KEY (id_tipo_sangre) REFERENCES tipos_sangre(id_tipo_sangre)
);

## Backend
```
cd backend-dotnet
dotnet restore
# abre appsettings.json y coloca tu password de MySQL
dotnet run
```
## Frontend
```
cd frontend-node
cp .env.sample .env  # ajusta API_BASE si tu backend usa otro puerto
npm install
npm start
```
Abre: `http://localhost:3001/estudiantes`
