using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProyectoMSD.Modelos;

/// <summary>
/// Modelo para registrar la actividad de los usuarios en el sistema.
/// Cada entrada representa un evento (Login, Logout, PageView) de un usuario.
/// </summary>
public partial class RegistroAcceso
{
    public int Id { get; set; }

    /// <summary>FK al usuario que realizó la acción</summary>
    [Required]
    public int IdUsuarios { get; set; }

    /// <summary>Fecha y hora exacta del evento</summary>
    [Required]
    public DateTime FechaAcceso { get; set; }

    /// <summary>Tipo de evento: Login, Logout, PageView</summary>
    [Required]
    [StringLength(50)]
    public string TipoAccion { get; set; } = null!;

    /// <summary>Dirección IP del cliente (IPv4 o IPv6)</summary>
    [StringLength(45)]
    public string? DireccionIp { get; set; }

    /// <summary>User-Agent del navegador del usuario</summary>
    [StringLength(500)]
    public string? Navegador { get; set; }

    /// <summary>Ruta de la página visitada (ej: /Dispositivos/Index)</summary>
    [StringLength(250)]
    public string? PaginaVisitada { get; set; }

    /// <summary>Duración de la sesión en segundos (calculado al logout)</summary>
    public int? DuracionSesion { get; set; }

    /// <summary>Navegación al usuario asociado</summary>
    [ValidateNever]
    public virtual Usuario IdUsuariosNavigation { get; set; } = null!;
}
