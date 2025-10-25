using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class UmgContext : DbContext
    {
        public UmgContext(DbContextOptions<UmgContext> options) : base(options) { }
        public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
        public DbSet<TipoSangre> TiposSangre => Set<TipoSangre>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // Estudiantes
            mb.Entity<Estudiante>(e =>
            {
                e.ToTable("estudiantes");
                e.HasKey(x => x.Id_Estudiante);

                e.Property(x => x.Id_Estudiante).HasColumnName("id_estudiante");
                e.Property(x => x.Carne).HasColumnName("carne");
                e.Property(x => x.Nombres).HasColumnName("nombres");
                e.Property(x => x.Apellidos).HasColumnName("apellidos");
                e.Property(x => x.Direccion).HasColumnName("direccion");
                e.Property(x => x.Telefono).HasColumnName("telefono");
                e.Property(x => x.Correo_Electronico).HasColumnName("correo_electronico");
                e.Property(x => x.Id_Tipo_Sangre).HasColumnName("id_tipo_sangre");
                e.Property(x => x.Fecha_Nacimiento).HasColumnName("fecha_nacimiento");

                e.HasIndex(x => x.Carne).IsUnique();

                e.HasOne(x => x.TipoSangre)
                 .WithMany(t => t.Estudiantes)
                 .HasForeignKey(x => x.Id_Tipo_Sangre)
                 .HasConstraintName("fk_estudiantes_tipos_sangre");
            });

            // Tipos de sangre
            mb.Entity<TipoSangre>(t =>
            {
                t.ToTable("tipos_sangre");
                t.HasKey(x => x.Id_Tipo_Sangre);
                t.Property(x => x.Id_Tipo_Sangre).HasColumnName("id_tipo_sangre");
                t.Property(x => x.Sangre).HasColumnName("sangre");
            });
        }
    }
}
