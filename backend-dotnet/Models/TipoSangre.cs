namespace Backend.Models
{
    public class TipoSangre
    {
        public int Id_Tipo_Sangre { get; set; }           // PK (id_tipo_sangre)
        public string Sangre { get; set; } = "";           // (sangre)
        public ICollection<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();
    }
}
