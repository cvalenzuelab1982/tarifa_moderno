using MassTransit.Middleware;

namespace Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants
{
    public static class SPName
    {
        public static class Cliente
        {
            public static string APP_VALIDAR_SERVICIOS_POR_CALIFICAR = "APP_VALIDAR_SERVICIOS_POR_CALIFICAR";
            public static string Presupuesto_Saldo = "Presupuesto_Saldo";
        }

        public static class Parametros
        {
            public static string PARAMETRO_LISTAR_TODO = "ParametroListarTodo";
        }

        public static class Tarifa
        {
            public static string X1_VERIFICAR_COBERTURA = "X1_VERIFICAR_COBERTURA";
            public static string X2_VERIFICAR_COBERTURA = "X2_VERIFICAR_COBERTURA";
            public static string VALIDAR_ZONAS = "ValidarZonas";
            public static string X1_VERIFICAR_PUNTO_ZONA = "X1_VERIFICAR_PUNTO_ZONA";
            public static string X1_OBTENER_TARIFA_ZONA_PARTICULAR = "X1_OBTENER_TARIFA_ZONA_PARTICULAR";
            public static string X1_WS_OBTENER_PARAMETROS_FORMULA = "X1_WS_OBTENER_PARAMETROS_FORMULA";
            public static string EX1_WS_VERIFICAR_PUNTO_ZONA_AEROPUERTO = "EX1_WS_VERIFICAR_PUNTO_ZONA_AEROPUERTO";
            public static string X1_WS_OBTENER_CONSTANTE_ZONA_EMPRESA = "X1_WS_OBTENER_CONSTANTE_ZONA_EMPRESA";
            public static string X1_VERIFICAR_MULTIPLE_PUNTO_ZONA = "X1_VERIFICAR_MULTIPLE_PUNTO_ZONA";
            public static string X1_WS_OBTENER_PARAMETROS_FORMULA_ISOCOUNTRY = "X1_WS_OBTENER_PARAMETROS_FORMULA_ISOCOUNTRY";
            public static string X1_WS_OBTENER_CONSTANTE_ZONA = "X1_WS_OBTENER_CONSTANTE_ZONA";
            public static string EX1_OBTENER_DESCUENTO_MONTO = "EX1_OBTENER_DESCUENTO_MONTO";
            public static string OBTENER_MONTO_INCREMENTO_EMPRESA_X1 = "OBTENER_MONTO_INCREMENTO_EMPRESA_X1";
            public static string EX3_WS_TarifaByFormaCalculo = "EX3_WS_TarifaByFormaCalculo";
            public static string EX1_TarifaByTipoServicio = "EX1_TarifaByTipoServicio";
        }

        public static class Zona
        {
            public static string ZN_VERIFICAR_IDZONA = "ZN_VERIFICAR_IDZONA";
            public static string EX1_ZonaByPosicionEmpresa = "EX1_ZonaByPosicionEmpresa";
            public static string ZN_VERIFICAR_ZONA_PELIGROSA = "ZN_VERIFICAR_ZONA_PELIGROSA";
            public static string NX_LISTAR_PEAJES_X_ZONAS = "NX_LISTAR_PEAJES_X_ZONAS";
            public static string EX1_ZonaListar = "EX1_ZonaListar";
        }

        public static class DescargarMaestro
        {
            public static string EX5_GenericaDispositivoTipoServicioCobertura = "EX5_GenericaDispositivoTipoServicioCobertura";
            public static string EX5_GenericaDispositivoTipoPagoClienteCobertura = "EX5_GenericaDispositivoTipoPagoClienteCobertura";
        }

        public static class Servicio
        {
            public static string X1_WS_TodosPrimerDestino = "X1_WS_TodosPrimerDestino";
            public static string X2_WS_FORMA_CALCULO_TIPO_SERVICIO = "X2_WS_FORMA_CALCULO_TIPO_SERVICIO";
            public static string OBTENER_TIEMPO_POR_POR_ZONA = "OBTENER_TIEMPO_POR_POR_ZONA";
            public static string OBTENER_TIEMPO_POR_ZONA_ALO_X2 = "OBTENER_TIEMPO_POR_ZONA_ALO_X2";
        }

        public static class Empresa
        {
            public static string EX1_USA_CONSTANTE_ZONA = "EX1_USA_CONSTANTE_ZONA";
            public static string Validar_Dia_X1 = "Validar_Dia_X1";
            public static string APP_VALIDAR_CONDUCTOR_CERCANO = "APP_VALIDAR_CONDUCTOR_CERCANO";
        }

        public static class RecargoEspecial
        {
            public static string EX2_VALIDAR_HORA_PUNTA_AEROPUERTO = "EX2_VALIDAR_HORA_PUNTA_AEROPUERTO";
            public static string EX2_OBTENER_RECARGO_ESPECIAL_DIRECTO = "EX2_OBTENER_RECARGO_ESPECIAL_DIRECTO";
        }

        public static class RecargoReserva
        {
            public static string X2_VALIDAR_HORA_PUNTA_MODO_RESERVA = "X2_VALIDAR_HORA_PUNTA_MODO_RESERVA";
        }

        public static class HoraPunta
        {
            public static string X3_VALIDAR_HORA_PUNTA = "X3_VALIDAR_HORA_PUNTA";
        }

        public static class  Promociones
        {
            public static string X9_PromocionVigencia = "X9_PromocionVigencia";
            public static string X10_PromocionVigenciaId = "X10_PromocionVigenciaId";
            public static string SP_GET_ZONAS_PARAMETRIZADOS_PROMOCION_x1 = "SP_GET_ZONAS_PARAMETRIZADOS_PROMOCION_x1";
            public static string API_VALIDATE_PROMOCION_X2_Prueba = "API_VALIDATE_PROMOCION_X2_Prueba";
        }

        public static class Compania
        {
            public static string EX1_WS_RECUPERAR_COMPANIA = "EX1_WS_RECUPERAR_COMPANIA";
        }

        public static class Plaza
        {
            public static string TARIFA_HORARIO_PLAZA = "TARIFA_HORARIO_PLAZA";
            public static string X1_WS_VERIFICAR_PUNTO_ZONA_PLAZA = "X1_WS_VERIFICAR_PUNTO_ZONA_PLAZA";
        }

        public static class Conductor
        {
            public static string EX1_USA_CONSTANTE_ZONA = "EX1_USA_CONSTANTE_ZONA";
        }
    }
}
