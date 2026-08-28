
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EventifyJC.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auditorium> Auditoria { get; set; }

    public virtual DbSet<Calificacion> Calificacions { get; set; }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Cotizacion> Cotizacions { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<SolicitudCotizacion> SolicitudCotizacions { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=localhost\\SQLEXPRESS;Initial Catalog=EventifyJC;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.HasKey(e => e.IdAuditoria).HasName("PK__Auditori__9644A3CE819D4831");

            entity.Property(e => e.IdAuditoria).HasColumnName("id_auditoria");
            entity.Property(e => e.DatosAnteriores)
                .HasColumnType("text")
                .HasColumnName("datos_anteriores");
            entity.Property(e => e.DatosNuevos)
                .HasColumnType("text")
                .HasColumnName("datos_nuevos");
            entity.Property(e => e.FechaHora)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_hora");
            entity.Property(e => e.IdRegistro).HasColumnName("id_registro");
            entity.Property(e => e.Operacion)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("operacion");
            entity.Property(e => e.Tabla)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tabla");
            entity.Property(e => e.UsuarioAccion).HasColumnName("usuario_accion");

            entity.HasOne(d => d.UsuarioAccionNavigation).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.UsuarioAccion)
                .HasConstraintName("FK__Auditoria__usuar__00200768");
        });

        modelBuilder.Entity<Calificacion>(entity =>
        {
            entity.HasKey(e => e.IdCalificacion).HasName("PK__Califica__38CEF35C7AB3ADB0");

            entity.ToTable("Calificacion");

            entity.Property(e => e.IdCalificacion).HasColumnName("id_calificacion");
            entity.Property(e => e.Comentario)
                .HasColumnType("text")
                .HasColumnName("comentario");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdOrganizador).HasColumnName("id_organizador");
            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.Puntuacion).HasColumnName("puntuacion");

            entity.HasOne(d => d.IdOrganizadorNavigation).WithMany(p => p.Calificacions)
                .HasForeignKey(d => d.IdOrganizador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Calificac__id_or__01142BA1");

            entity.HasOne(d => d.IdReservaNavigation).WithMany(p => p.Calificacions)
                .HasForeignKey(d => d.IdReserva)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Calificac__id_re__02084FDA");
        });

        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__Categori__CD54BC5A1661DB65");

            entity.HasIndex(e => e.Nombre, "UQ__Categori__72AFBCC6561E5108").IsUnique();

            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Link)                
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("link");
        });

        modelBuilder.Entity<Cotizacion>(entity =>
        {
            entity.HasKey(e => e.IdCotizacion).HasName("PK__Cotizaci__9713D174C4B4F357");

            entity.ToTable("Cotizacion");

            entity.Property(e => e.IdCotizacion).HasColumnName("id_cotizacion");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("enviada")
                .HasColumnName("estado");
            entity.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_envio");
            entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");
            entity.Property(e => e.IdServicio).HasColumnName("id_servicio");
            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.Mensaje)
                .HasColumnType("text")
                .HasColumnName("mensaje");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("monto");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Cotizacions)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cotizacio__id_pr__02FC7413");

            entity.HasOne(d => d.IdServicioNavigation).WithMany(p => p.Cotizacions)
                .HasForeignKey(d => d.IdServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cotizacio__id_se__03F0984C");

            entity.HasOne(d => d.IdSolicitudNavigation).WithMany(p => p.Cotizacions)
                .HasForeignKey(d => d.IdSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cotizacio__id_so__04E4BC85");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.IdPago).HasName("PK__Pago__0941B074C02E7041");

            entity.ToTable("Pago");

            entity.Property(e => e.IdPago).HasColumnName("id_pago");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.FechaPago)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_pago");
            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.IdTransaccionExterna)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("id_transaccion_externa");
            entity.Property(e => e.Metodo)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("metodo");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("monto");

            entity.HasOne(d => d.IdReservaNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdReserva)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pago__id_reserva__05D8E0BE");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor).HasName("PK__Proveedo__8D3DFE2851B91C9B");

            entity.ToTable("Proveedor");

            entity.HasIndex(e => e.IdUsuario, "UQ__Proveedo__4E3E04ACF240E99F").IsUnique();

            entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.Property(e => e.NombreComercial)
                .HasMaxLength(400)
                .HasColumnName("nombre_comercial");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(400)
                .HasColumnName("razon_social");
            entity.Property(e => e.Nit)
                .HasMaxLength(100)
                .HasColumnName("nit");
            entity.Property(e => e.Representante)
                .HasMaxLength(400)
                .HasColumnName("representante");
            entity.Property(e => e.TelefonoNegocio)
                .HasMaxLength(100)
                .HasColumnName("telefono_negocio");
            entity.Property(e => e.NivelCobertura)
                .HasMaxLength(100)
                .HasColumnName("nivel_cobertura");
            entity.Property(e => e.CiudadesCubiertas)
                .HasMaxLength(1000)
                .HasColumnName("ciudades_cubiertas");
            entity.Property(e => e.Categorias)
                .HasMaxLength(1000)
                .HasColumnName("categorias");
            entity.Property(e => e.Tarifas)
                .HasMaxLength(400)
                .HasColumnName("tarifas");
            entity.Property(e => e.Capacidad)
                .HasMaxLength(200)
                .HasColumnName("capacidad");
            entity.Property(e => e.AnticipacionRequerida)
                .HasMaxLength(200)
                .HasColumnName("anticipacion_requerida");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(1000)
                .HasColumnName("logo_url");
            entity.Property(e => e.Galeria)
                .HasColumnName("galeria");
            entity.Property(e => e.Videos)
                .HasColumnName("videos");
            entity.Property(e => e.Whatsapp)
                .HasMaxLength(100)
                .HasColumnName("whatsapp");
            entity.Property(e => e.Instagram)
                .HasMaxLength(200)
                .HasColumnName("instagram");
            entity.Property(e => e.Facebook)
                .HasMaxLength(400)
                .HasColumnName("facebook");
            entity.Property(e => e.SitioWeb)
                .HasMaxLength(400)
                .HasColumnName("sitio_web");

            entity.Property(e => e.CalificacionPromedio)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("calificacion_promedio");
            entity.Property(e => e.Ciudad)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ciudad");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.Lat)
                .HasColumnType("decimal(10, 8)")
                .HasColumnName("lat");
            entity.Property(e => e.Lng)
                .HasColumnType("decimal(11, 8)")
                .HasColumnName("lng");
            entity.Property(e => e.Verificado)
                .HasDefaultValue(false)
                .HasColumnName("verificado");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Proveedor)
                .HasForeignKey<Proveedor>(d => d.IdUsuario)
                .HasConstraintName("FK__Proveedor__id_us__06CD04F7");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.IdReserva).HasName("PK__Reserva__423CBE5DF13B458E");

            entity.ToTable("Reserva");

            entity.HasIndex(e => e.IdCotizacion, "UQ__Reserva__9713D1750D54EDA2").IsUnique();

            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.ComisionEventify)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("comision_eventify");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("confirmada")
                .HasColumnName("estado");
            entity.Property(e => e.FechaConfirmacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_confirmacion");
            entity.Property(e => e.IdCotizacion).HasColumnName("id_cotizacion");
            entity.Property(e => e.MontoFinal)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("monto_final");

            entity.HasOne(d => d.IdCotizacionNavigation).WithOne(p => p.Reserva)
                .HasForeignKey<Reserva>(d => d.IdCotizacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reserva__id_coti__07C12930");
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.IdServicio).HasName("PK__Servicio__6FD07FDC480C7129");

            entity.ToTable("Servicio");

            entity.Property(e => e.IdServicio).HasColumnName("id_servicio");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Disponibilidad)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("disponible")
                .HasColumnName("disponibilidad");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("imagen_url");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PrecioBase)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("precio_base");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Servicios)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Servicio__id_cat__08B54D69");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Servicios)
                .HasForeignKey(d => d.IdProveedor)
                .HasConstraintName("FK__Servicio__id_pro__09A971A2");
        });

        modelBuilder.Entity<SolicitudCotizacion>(entity =>
        {
            entity.HasKey(e => e.IdSolicitud).HasName("PK__Solicitu__5C0C31F366BD744E");

            entity.ToTable("SolicitudCotizacion");

            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.DescripcionEvento)
                .HasColumnType("text")
                .HasColumnName("descripcion_evento");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.FechaEvento).HasColumnName("fecha_evento");
            entity.Property(e => e.FechaSolicitud)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.IdOrganizador).HasColumnName("id_organizador");
            entity.Property(e => e.PresupuestoEstimado)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("presupuesto_estimado");

            entity.HasOne(d => d.IdOrganizadorNavigation).WithMany(p => p.SolicitudCotizacions)
                .HasForeignKey(d => d.IdOrganizador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Solicitud__id_or__0A9D95DB");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__4E3E04AD3BC2BC29");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.Email, "UQ__Usuario__AB6E6164ADA3B92C").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("rol");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
