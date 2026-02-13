using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoMSD.Modelos;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Almacenan> Almacenans { get; set; }

    public virtual DbSet<Configuracione> Configuraciones { get; set; }

    public virtual DbSet<Dispositivo> Dispositivos { get; set; }

    public virtual DbSet<Espacio> Espacios { get; set; }

    public virtual DbSet<Propiedade> Propiedades { get; set; }

    public virtual DbSet<Soporte> Soportes { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }


    //no mames bro respeta

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    { }
   // => optionsBuilder.UseMySql("server=127.0.0.1;port=3306;database=ProjectMySD;uid=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.32-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Almacenan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("almacenan");

            entity.HasIndex(e => e.IdDispositivos, "ID_Dispositivos");

            entity.HasIndex(e => e.IdEspacios, "ID_Espacios");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.IdDispositivos)
                .HasColumnType("int(11)")
                .HasColumnName("ID_Dispositivos");
            entity.Property(e => e.IdEspacios)
                .HasColumnType("int(11)")
                .HasColumnName("ID_Espacios");

            entity.HasOne(d => d.IdDispositivosNavigation).WithMany(p => p.Almacenans)
                .HasForeignKey(d => d.IdDispositivos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("almacenan_ibfk_2");

            entity.HasOne(d => d.IdEspaciosNavigation).WithMany(p => p.Almacenans)
                .HasForeignKey(d => d.IdEspacios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("almacenan_ibfk_1");
        });

        modelBuilder.Entity<Configuracione>(entity =>
        {
            entity.HasKey(e => e.Codigo).HasName("PRIMARY");

            entity.ToTable("configuraciones");

            entity.HasIndex(e => e.IdDispositivos, "ID_Dispositivos");

            entity.HasIndex(e => e.IdUsuarios, "ID_Usuarios");

            entity.Property(e => e.Codigo).HasColumnType("int(11)");
            entity.Property(e => e.Adaptador).HasMaxLength(250);
            entity.Property(e => e.Alertas).HasMaxLength(250);
            entity.Property(e => e.IdDispositivos)
                .HasColumnType("int(11)")
                .HasColumnName("ID_Dispositivos");
            entity.Property(e => e.IdUsuarios)
                .HasColumnType("int(11)")
                .HasColumnName("ID_Usuarios");
            entity.Property(e => e.Idioma).HasMaxLength(250);

            entity.HasOne(d => d.IdDispositivosNavigation).WithMany(p => p.Configuraciones)
                .HasForeignKey(d => d.IdDispositivos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("configuraciones_ibfk_1");

            entity.HasOne(d => d.IdUsuariosNavigation).WithMany(p => p.Configuraciones)
                .HasForeignKey(d => d.IdUsuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("configuraciones_ibfk_2");
        });

        modelBuilder.Entity<Dispositivo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("dispositivos");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.Estado).HasMaxLength(250);
            entity.Property(e => e.Marca).HasMaxLength(250);
            entity.Property(e => e.Nombre).HasMaxLength(250);
            entity.Property(e => e.Tipo).HasMaxLength(250);
            entity.Property(e => e.Usos).HasMaxLength(250);
        });

        modelBuilder.Entity<Espacio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("espacios");

            entity.HasIndex(e => e.IdPropiedades, "ID_Propiedades");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.IdPropiedades)
                .HasColumnType("int(11)")
                .HasColumnName("ID_Propiedades");
            entity.Property(e => e.Nombre).HasMaxLength(250);
            entity.Property(e => e.Permisos).HasMaxLength(250);
            entity.Property(e => e.Señal).HasColumnType("int(11)");
            entity.Property(e => e.Ubicacion).HasMaxLength(250);

            entity.HasOne(d => d.IdPropiedadesNavigation).WithMany(p => p.Espacios)
                .HasForeignKey(d => d.IdPropiedades)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("espacios_ibfk_1");
        });

        modelBuilder.Entity<Propiedade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("propiedades");

            entity.HasIndex(e => e.IdUsuarios, "ID_USUARIOS");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.IdUsuarios)
                .HasColumnType("int(11)")
                .HasColumnName("ID_USUARIOS");
            entity.Property(e => e.Pisos).HasColumnType("int(11)");
            entity.Property(e => e.Tipo).HasMaxLength(250);

            entity.HasOne(d => d.IdUsuariosNavigation).WithMany(p => p.Propiedades)
                .HasForeignKey(d => d.IdUsuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("propiedades_ibfk_1");
        });

        modelBuilder.Entity<Soporte>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("soportes");

            entity.HasIndex(e => e.IdUsuarios, "ID_USUARIOS");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.IdUsuarios)
                .HasColumnType("int(11)")
                .HasColumnName("ID_USUARIOS");
            entity.Property(e => e.Respuesta).HasMaxLength(250);
            entity.Property(e => e.Tipo).HasMaxLength(250);

            entity.HasOne(d => d.IdUsuariosNavigation).WithMany(p => p.Soportes)
                .HasForeignKey(d => d.IdUsuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("soportes_ibfk_1");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("ID");
            entity.Property(e => e.Acesso).HasMaxLength(250);
            entity.Property(e => e.Clave).HasMaxLength(250);
            entity.Property(e => e.Correo).HasMaxLength(250);
            entity.Property(e => e.Documento).HasColumnType("bigint(20)");
            entity.Property(e => e.Nombre).HasMaxLength(250);
            entity.Property(e => e.Permisos).HasMaxLength(250);
            entity.Property(e => e.Rol).HasMaxLength(250);
            entity.Property(e => e.Rut).HasMaxLength(250);
            entity.Property(e => e.Telefono).HasColumnType("int(11)");
            entity.Property(e => e.Ubicacion).HasMaxLength(250);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}