using System;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace IdentityOAuthSpaExtensions.Wrappers
{
    internal static class ReflectionHelpers
    {
        public static void SetHttpContext(this IAuthenticationHandler authenticationHandler,
            HttpContext httpContext)
        {
            var type = GetAuthenticationHandlerBaseType(authenticationHandler);

            var property = type
                .GetProperty("Context", BindingFlags.NonPublic | BindingFlags.Instance);
            property.SetValue(authenticationHandler, httpContext,
                BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null,
                new object[] { },
                CultureInfo.CurrentCulture);
        }

        public static void SetOptions(this IAuthenticationHandler authenticationHandler,
            AuthenticationSchemeOptions authenticationSchemeOptions)
        {
            var type = GetAuthenticationHandlerBaseType(authenticationHandler);

            var property = type
                .GetProperty("Options",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            property.SetValue(authenticationHandler, authenticationSchemeOptions,
                BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null,
                new object[] { },
                CultureInfo.CurrentCulture);
        }

        public static RemoteAuthenticationOptions GetOptions(
            this IAuthenticationHandler authenticationHandler)
        {
            var type = GetAuthenticationHandlerBaseType(authenticationHandler);

            var property = type
                .GetProperty("OptionsMonitor",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            IOptionsMonitor<RemoteAuthenticationOptions> optionsMonitor = property.GetValue(
                authenticationHandler,
                BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance, null,
                new object[] { },
                CultureInfo.CurrentCulture) as IOptionsMonitor<RemoteAuthenticationOptions>;

            return optionsMonitor.CurrentValue;
        }

        private static readonly MethodInfo CloneMethod =
            typeof(Object).GetMethod("MemberwiseClone",
                BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(String)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }

        public static T MemberwiseClone<T>(this T originalObject)
        {
            if (originalObject == null) return default(T);
            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect)) return originalObject;
            if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return default(T);
            var cloneObject = CloneMethod.Invoke(originalObject, null);
            return (T) cloneObject;
        }

        private static Type GetAuthenticationHandlerBaseType(
            IAuthenticationHandler authenticationHandler)
        {
            const string baseType = "Microsoft.AspNetCore.Authentication.AuthenticationHandler";
            var type = authenticationHandler.GetType();
            while (type != null &&
                   !type.FullName.StartsWith(baseType))
                type = type.BaseType;

            if (type == null)
            {
                throw new InvalidOperationException(
                    $"Type {authenticationHandler.GetType()} is not supported. We only support descendants of {baseType}<>");
            }

            return type;
        }
    }
}