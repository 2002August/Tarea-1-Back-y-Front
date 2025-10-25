namespace Backend.Models
{
    public class Estudiante
    {
        public int Id_Estudiante { get; set; }           // PK (id_estudiante)
        public string Carne { get; set; } = "";           // UNIQUE (carne)  E001..E999
        public string Nombres { get; set; } = "";         // (nombres)
        public string Apellidos { get; set; } = "";       // (apellidos)
        public string? Direccion { get; set; }            // (direccion)
        public string? Telefono { get; set; }             // (telefono)
        public string? Correo_Electronico { get; set; }   // (correo_electronico)
        public DateTime? Fecha_Nacimiento { get; set; }   // (fecha_nacimiento)
        public int Id_Tipo_Sangre { get; set; }           // FK (id_tipo_sangre)
        public TipoSangre? TipoSangre { get; set; }       // nav
    }
}
