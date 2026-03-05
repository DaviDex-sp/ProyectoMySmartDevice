using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoMSD.Modelos
{
    public class Notificacion
    {
        public int Id { get; set; }

        public int IdUsuarios { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; } = null!;

        [Required]
        public string Mensaje { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = null!;

        public bool Leida { get; set; }

        public DateTime FechaCreacion { get; set; }

        public string? RutaRedireccion { get; set; }

        [ForeignKey("IdUsuarios")]
        public virtual Usuario Usuario { get; set; } = null!;
    }
}
