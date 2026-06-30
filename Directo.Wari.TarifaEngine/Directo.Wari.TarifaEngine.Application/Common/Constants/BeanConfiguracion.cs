namespace Directo.Wari.TarifaEngine.Application.Common.Constants
{
    public static class BeanConfiguracion
    {
        public static class VUELO
        {
            public const int VUELO_TIPO_NACIONAL = 1;
            public const int VUELO_TIPO_INTERNACIONAL = 2;

            public const int VUELO_ESTADO_SALIDA = 1;
            public const int VUELO_ESTADO_LLEGADA = 2;
        }

        public static class HTTP_RESPONSE
        {
            public const int HTTP_OK_MSG = 1;
            public const int HTTP_OK_NOMSG = 2;
            public const int HTTP_ALGUN_ERROR = 0;
            public const int HTTP_ERROR_MSG = -1;
            public const int HTTP_ERROR_NOMSG = -2;
            public const int HTTP_ERROR_SERVER = -3;
        }

        public static class PRESUPUESTO
        {
            public const int PRESUPUESTO_ENTERO = 1;
        }
    }
}
