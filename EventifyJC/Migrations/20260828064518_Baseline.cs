using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventifyJC.Migrations
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categoria",
                columns: table => new
                {
                    id_categoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    link = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Categori__CD54BC5A1661DB65", x => x.id_categoria);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    telefono = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    activo = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Usuario__4E3E04AD3BC2BC29", x => x.id_usuario);
                });

            migrationBuilder.CreateTable(
                name: "Auditoria",
                columns: table => new
                {
                    id_auditoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tabla = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    operacion = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    id_registro = table.Column<int>(type: "int", nullable: true),
                    usuario_accion = table.Column<int>(type: "int", nullable: true),
                    fecha_hora = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    datos_anteriores = table.Column<string>(type: "text", nullable: true),
                    datos_nuevos = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Auditori__9644A3CE819D4831", x => x.id_auditoria);
                    table.ForeignKey(
                        name: "FK__Auditoria__usuar__00200768",
                        column: x => x.usuario_accion,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "Proveedor",
                columns: table => new
                {
                    id_proveedor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    nombre_comercial = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    razon_social = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    nit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    representante = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    telefono_negocio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    direccion = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    ciudad = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    nivel_cobertura = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ciudades_cubiertas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    categorias = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    tarifas = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    capacidad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    anticipacion_requerida = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    logo_url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    galeria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    videos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    whatsapp = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    instagram = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    facebook = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    sitio_web = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    lat = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    lng = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    verificado = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    calificacion_promedio = table.Column<decimal>(type: "decimal(3,2)", nullable: true, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Proveedo__8D3DFE2851B91C9B", x => x.id_proveedor);
                    table.ForeignKey(
                        name: "FK__Proveedor__id_us__06CD04F7",
                        column: x => x.id_usuario,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudCotizacion",
                columns: table => new
                {
                    id_solicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_organizador = table.Column<int>(type: "int", nullable: false),
                    fecha_solicitud = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    fecha_evento = table.Column<DateOnly>(type: "date", nullable: false),
                    descripcion_evento = table.Column<string>(type: "text", nullable: true),
                    presupuesto_estimado = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    estado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "pendiente")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Solicitu__5C0C31F366BD744E", x => x.id_solicitud);
                    table.ForeignKey(
                        name: "FK__Solicitud__id_or__0A9D95DB",
                        column: x => x.id_organizador,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "Servicio",
                columns: table => new
                {
                    id_servicio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_proveedor = table.Column<int>(type: "int", nullable: false),
                    id_categoria = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    precio_base = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    disponibilidad = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "disponible"),
                    imagen_url = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Servicio__6FD07FDC480C7129", x => x.id_servicio);
                    table.ForeignKey(
                        name: "FK__Servicio__id_cat__08B54D69",
                        column: x => x.id_categoria,
                        principalTable: "Categoria",
                        principalColumn: "id_categoria");
                    table.ForeignKey(
                        name: "FK__Servicio__id_pro__09A971A2",
                        column: x => x.id_proveedor,
                        principalTable: "Proveedor",
                        principalColumn: "id_proveedor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cotizacion",
                columns: table => new
                {
                    id_cotizacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_solicitud = table.Column<int>(type: "int", nullable: false),
                    id_proveedor = table.Column<int>(type: "int", nullable: false),
                    id_servicio = table.Column<int>(type: "int", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    mensaje = table.Column<string>(type: "text", nullable: true),
                    fecha_envio = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    estado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "enviada")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Cotizaci__9713D174C4B4F357", x => x.id_cotizacion);
                    table.ForeignKey(
                        name: "FK__Cotizacio__id_pr__02FC7413",
                        column: x => x.id_proveedor,
                        principalTable: "Proveedor",
                        principalColumn: "id_proveedor");
                    table.ForeignKey(
                        name: "FK__Cotizacio__id_se__03F0984C",
                        column: x => x.id_servicio,
                        principalTable: "Servicio",
                        principalColumn: "id_servicio");
                    table.ForeignKey(
                        name: "FK__Cotizacio__id_so__04E4BC85",
                        column: x => x.id_solicitud,
                        principalTable: "SolicitudCotizacion",
                        principalColumn: "id_solicitud");
                });

            migrationBuilder.CreateTable(
                name: "Reserva",
                columns: table => new
                {
                    id_reserva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cotizacion = table.Column<int>(type: "int", nullable: false),
                    estado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "confirmada"),
                    fecha_confirmacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    monto_final = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    comision_eventify = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Reserva__423CBE5DF13B458E", x => x.id_reserva);
                    table.ForeignKey(
                        name: "FK__Reserva__id_coti__07C12930",
                        column: x => x.id_cotizacion,
                        principalTable: "Cotizacion",
                        principalColumn: "id_cotizacion");
                });

            migrationBuilder.CreateTable(
                name: "Calificacion",
                columns: table => new
                {
                    id_calificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_reserva = table.Column<int>(type: "int", nullable: false),
                    id_organizador = table.Column<int>(type: "int", nullable: false),
                    puntuacion = table.Column<int>(type: "int", nullable: false),
                    comentario = table.Column<string>(type: "text", nullable: true),
                    fecha = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Califica__38CEF35C7AB3ADB0", x => x.id_calificacion);
                    table.ForeignKey(
                        name: "FK__Calificac__id_or__01142BA1",
                        column: x => x.id_organizador,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario");
                    table.ForeignKey(
                        name: "FK__Calificac__id_re__02084FDA",
                        column: x => x.id_reserva,
                        principalTable: "Reserva",
                        principalColumn: "id_reserva");
                });

            migrationBuilder.CreateTable(
                name: "Pago",
                columns: table => new
                {
                    id_pago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_reserva = table.Column<int>(type: "int", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    metodo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    estado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "pendiente"),
                    fecha_pago = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    id_transaccion_externa = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Pago__0941B074C02E7041", x => x.id_pago);
                    table.ForeignKey(
                        name: "FK__Pago__id_reserva__05D8E0BE",
                        column: x => x.id_reserva,
                        principalTable: "Reserva",
                        principalColumn: "id_reserva");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_usuario_accion",
                table: "Auditoria",
                column: "usuario_accion");

            migrationBuilder.CreateIndex(
                name: "IX_Calificacion_id_organizador",
                table: "Calificacion",
                column: "id_organizador");

            migrationBuilder.CreateIndex(
                name: "IX_Calificacion_id_reserva",
                table: "Calificacion",
                column: "id_reserva");

            migrationBuilder.CreateIndex(
                name: "UQ__Categori__72AFBCC6561E5108",
                table: "Categoria",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_id_proveedor",
                table: "Cotizacion",
                column: "id_proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_id_servicio",
                table: "Cotizacion",
                column: "id_servicio");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_id_solicitud",
                table: "Cotizacion",
                column: "id_solicitud");

            migrationBuilder.CreateIndex(
                name: "IX_Pago_id_reserva",
                table: "Pago",
                column: "id_reserva");

            migrationBuilder.CreateIndex(
                name: "UQ__Proveedo__4E3E04ACF240E99F",
                table: "Proveedor",
                column: "id_usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Reserva__9713D1750D54EDA2",
                table: "Reserva",
                column: "id_cotizacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servicio_id_categoria",
                table: "Servicio",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Servicio_id_proveedor",
                table: "Servicio",
                column: "id_proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudCotizacion_id_organizador",
                table: "SolicitudCotizacion",
                column: "id_organizador");

            migrationBuilder.CreateIndex(
                name: "UQ__Usuario__AB6E6164ADA3B92C",
                table: "Usuario",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditoria");

            migrationBuilder.DropTable(
                name: "Calificacion");

            migrationBuilder.DropTable(
                name: "Pago");

            migrationBuilder.DropTable(
                name: "Reserva");

            migrationBuilder.DropTable(
                name: "Cotizacion");

            migrationBuilder.DropTable(
                name: "Servicio");

            migrationBuilder.DropTable(
                name: "SolicitudCotizacion");

            migrationBuilder.DropTable(
                name: "Categoria");

            migrationBuilder.DropTable(
                name: "Proveedor");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}
