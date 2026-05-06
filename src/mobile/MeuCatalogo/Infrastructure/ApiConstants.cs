namespace MeuCatalogo.Infrastructure;

public static class ApiConstants
{
    public static string BaseUrl =>
        "http://catalogo-api.sanyz.com.br/api/v1";
//
// #if RELEASE
//         "http://catalogo-api.sanyz.com.br/api/v1";
// #else
//     #if ANDROID
//             "http://10.0.2.2:5000/api/v1";
//     #elif WINDOWS
//             "http://localhost:5000/api/v1";
//     #elif IOS
//             "http://localhost:5000/api/v1";
//     #else
//             "http://localhost:5000/api/v1";
//     #endif
// #endif
}
