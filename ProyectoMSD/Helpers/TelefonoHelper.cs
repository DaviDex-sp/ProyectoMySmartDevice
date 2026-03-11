namespace ProyectoMSD.Helpers
{
    public static class TelefonoHelper
    {
        public static string ObtenerPrefijoConBandera(string? prefijo)
        {
            if (string.IsNullOrWhiteSpace(prefijo)) return string.Empty;

            return prefijo switch
            {
                "+57" => "🇨🇴 +57",
                "+1" => "🇺🇸 +1",
                "+52" => "🇲🇽 +52",
                "+34" => "🇪🇸 +34",
                "+54" => "🇦🇷 +54",
                "+56" => "🇨🇱 +56",
                "+51" => "🇵🇪 +51",
                "+58" => "🇻🇪 +58",
                "+55" => "🇧🇷 +55",
                "+593" => "🇪🇨 +593",
                "+507" => "🇵🇦 +507",
                _ => prefijo
            };
        }
    }
}
