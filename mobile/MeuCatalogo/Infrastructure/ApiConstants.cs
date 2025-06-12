namespace MeuCatalogo.Infrastructure;

public static class ApiConstants
{
    public static string BaseUrl =>
#if RELEASE
        "http://catalogo-api.sanyz.com.br/api";
#else
    #if ANDROID
            "http://10.0.2.2:5000/api";
    #elif WINDOWS
            "http://localhost:5000";
    #elif IOS
            "http://localhost:5000";
    #else
            "http://localhost:5000";
    #endif
#endif
}
