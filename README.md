# UMG Proyecto — API .NET + Frontend Node/EJS

Estructura:
```
umg-proyecto/
├─ backend-dotnet/     # API REST (.NET 8 + EF Core + MySQL)
└─ frontend-node/      # Frontend (Express + EJS) que consume el API
```

## Requisitos
- .NET 8 SDK
- Node.js LTS
- MySQL Server + Workbench

## Base de datos (ejecuta en MySQL)
```sql
CREATE DATABASE umg CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE umg;
CREATE TABLE tipos_sangre (
  id_tipo_sangre INT AUTO_INCREMENT PRIMARY KEY,
  sangre VARCHAR(5) NOT NULL
);
CREATE TABLE estudiantes (
  id_estudiante INT AUTO_INCREMENT PRIMARY KEY,
  carne VARCHAR(4) NOT NULL UNIQUE,
  nombres VARCHAR(100) NOT NULL,
  apellidos VARCHAR(100) NOT NULL,
  direccion VARCHAR(200),
  telefono VARCHAR(20),
  correo_electronico VARCHAR(150),
  id_tipo_sangre INT NOT NULL,
  fecha_nacimiento DATE,
  CONSTRAINT fk_estudiantes_tipos_sangre
    FOREIGN KEY (id_tipo_sangre) REFERENCES tipos_sangre(id_tipo_sangre)
    ON UPDATE CASCADE ON DELETE RESTRICT
);
INSERT INTO tipos_sangre (sangre) VALUES ('A+'),('A-'),('B+'),('B-'),('AB+'),('AB-'),('O+'),('O-');
```
**Regex de Carné**: `^E(00[1-9]|0[1-9][0-9]|[1-9][0-9]{2})$`

## Backend
```
cd backend-dotnet
dotnet restore
# abre appsettings.json y coloca tu password de MySQL
dotnet run
```
Swagger: navega a `/swagger` en el puerto que indique la consola.

## Frontend
```
cd frontend-node
cp .env.sample .env  # ajusta API_BASE si tu backend usa otro puerto
npm install
npm start
```
Abre: `http://localhost:3000/estudiantes`
