using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProyectoMSD.Modelos;

public partial class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "La Clave es nesesaria")]
    public string Clave { get; set; } = null!;

    [Required(ErrorMessage = "Se necesita un correo")]
    public string Correo { get; set; } = null!;

    [Required(ErrorMessage = "Especifica un Rol")]
    public string Rol { get; set; } = null!;

    [Required(ErrorMessage = "Agrega un telefono")]
    public int Telefono { get; set; }

    [Required(ErrorMessage = "Requiere una ubicacion")]
    public string Ubicacion { get; set; } = null!;

    [Required(ErrorMessage = "Los permisos son obligatorios")]
    public string Permisos { get; set; } = null!;

    [Required(ErrorMessage = "El documento es Obligatorio")]
    public long? Documento { get; set; }

    public string? Rut { get; set; }

    [Required(ErrorMessage = "El estado de acceso es obligatorio")]
    public string? Acesso { get; set; }

    public virtual ICollection<Configuracione> Configuraciones { get; set; } = new List<Configuracione>();

    public virtual ICollection<Propiedade> Propiedades { get; set; } = new List<Propiedade>();

    public virtual ICollection<Soporte> Soportes { get; set; } = new List<Soporte>();

    public virtual ICollection<RegistroAcceso> RegistroAccesos { get; set; } = new List<RegistroAcceso>();
}
