# backend-dotnet

API REST en .NET 8 (Minimal API) + EF Core + MySQL.

## Pasos rápidos
1) Ajusta `appsettings.json` con tu clave de MySQL.
2) Ejecuta:
```bash
dotnet restore
dotnet run
```
3) Prueba en `/swagger` la API.

## Tablas esperadas en MySQL
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
**Validación Carné**: `^E(00[1-9]|0[1-9][0-9]|[1-9][0-9]{2})$` (E001..E999).
