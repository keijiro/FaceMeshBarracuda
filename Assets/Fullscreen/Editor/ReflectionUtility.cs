using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace FullscreenEditor {
    /// <summary>Class containing method extensions for getting private and internal members.</summary>
    public static class ReflectionUtility {

        private static Assembly[] cachedAssemblies;

        public const BindingFlags FULL_BINDING = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>Find a type by its name.</summary>
        public static Type FindClass(string name) {
            // return typeof(Editor).Assembly.GetType(name, false, true);
            var result = FindTypeInAssembly(name, typeof(Editor).Assembly);

            if (result != null)
                return result;

            if (cachedAssemblies == null)
                cachedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (var i = 0; i < cachedAssemblies.Length; i++) {
                result = FindTypeInAssembly(name, cachedAssemblies[i]);

                if (result != null)
                    return result;
            }

            return result;
        }

        private static Type FindTypeInAssembly(string name, Assembly assembly) {
            return assembly == null ?
                null :
                assembly.GetType(name, false, true);
        }

        /// <summary>Find a field of a type by its name.</summary>
        public static FieldInfo FindField(this Type type, string fieldName, bool throwNotFound = true) {
            if (type == null)
                throw new ArgumentNullException("type");

            var field = type.GetField(fieldName, FULL_BINDING);

            if (field == null && throwNotFound)
                throw new MissingFieldException(type.FullName, fieldName);

            return field;
        }

        /// <summary>Find a property of a type by its name.</summary>
        public static PropertyInfo FindProperty(this Type type, string propertyName, bool throwNotFound = true) {
            if (type == null)
                throw new ArgumentNullException("type");

            var prop = type.GetProperty(propertyName, FULL_BINDING);

            if (prop == null && throwNotFound)
                throw new MissingMemberException(type.FullName, propertyName);

            return prop;
        }

        /// <summary>Find a method of a type by its name.</summary>
        public static MethodInfo FindMethod(this Type type, string methodName, Type[] args = null, bool throwNotFound = true) {
            if (type == null)
                throw new ArgumentNullException("type");

            MethodInfo method;

            if (args == null) {
                method = type.GetMethod(methodName, FULL_BINDING);
                // method = type.GetMethods(FULL_BINDING)
                //     .Where(m => m.Name == methodName)
                //     .FirstOrDefault();
            } else {
                method = type.GetMethod(methodName, FULL_BINDING, null, args, null);

                // There are very specific cases where the above method may not bind properly
                // e.g. when the method declares an enum and the arg type is an int, so we ignore the args 
                // and hope that there are no ambiguity of methods
                if (method == null) {
                    method = FindMethod(type, methodName, null, throwNotFound);

                    if (method != null && method.GetParameters().Length != args.Length)
                        method = null;
                }
            }

            if (method == null && throwNotFound)
                throw new MissingMethodException(type.FullName, methodName);

            return method;
        }

        /// <summary>Get the value of the static field.</summary>
        public static T GetFieldValue<T>(this Type type, string fieldName) { return (T)type.FindField(fieldName).GetValue(null); }

        /// <summary>Get the value of the instance field.</summary>
        public static T GetFieldValue<T>(this object obj, string fieldName) { return (T)obj.GetType().FindField(fieldName).GetValue(obj); }

        /// <summary>Set the value of the static field.</summary>
        public static void SetFieldValue(this Type type, string fieldName, object value) { type.FindField(fieldName).SetValue(null, value); }

        /// <summary>Set the value of the instance field.</summary>
        public static void SetFieldValue(this object obj, string fieldName, object value) { obj.GetType().FindField(fieldName).SetValue(obj, value); }

        /// <summary>Get the value of the static property.</summary>
        public static T GetPropertyValue<T>(this Type type, string propertyName) { return (T)type.FindProperty(propertyName).GetValue(null, null); }

        /// <summary>Get the value of the instance property.</summary>
        public static T GetPropertyValue<T>(this object obj, string propertyName) { return (T)obj.GetType().FindProperty(propertyName).GetValue(obj, null); }

        /// <summary>Set the value of the static property.</summary>
        public static void SetPropertyValue(this Type type, string propertyName, object value) { type.FindProperty(propertyName).SetValue(null, value, null); }

        /// <summary>Set the value of the instance property.</summary>
        public static void SetPropertyValue(this object obj, string propertyName, object value) { obj.GetType().FindProperty(propertyName).SetValue(obj, value, null); }

        /// <summary>Invoke a static method on the type and return the result.</summary>
        public static T InvokeMethod<T>(this Type type, string methodName, params object[] args) { return (T)type.FindMethod(methodName, args.Select(a => a.GetType()).ToArray()).Invoke(null, args); }

        /// <summary>Invoke a method on the object instance and return the result.</summary>
        public static T InvokeMethod<T>(this object obj, string methodName, params object[] args) { return (T)obj.GetType().FindMethod(methodName, args.Select(a => a.GetType()).ToArray()).Invoke(obj, args); }

        /// <summary>Invoke a static method on the type.</summary>
        public static void InvokeMethod(this Type type, string methodName, params object[] args) { type.FindMethod(methodName, args.Select(a => a.GetType()).ToArray()).Invoke(null, args); }

        /// <summary>Invoke a method on the object instance.</summary>
        public static void InvokeMethod(this object obj, string methodName, params object[] args) { obj.GetType().FindMethod(methodName, args.Select(a => a.GetType()).ToArray()).Invoke(obj, args); }

        /// <summary>Returns wheter the given type is the same as another one.</summary>
        /// <param name="toCheck">Type that will be checked.</param>
        /// <param name="type">Type to check against.</param>
        /// <param name="orInherited">Returns true if the checked type is inherited from the type argument.</param>
        public static bool IsOfType(this Type toCheck, Type type, bool orInherited = true) {
            return type == toCheck || (orInherited && type.IsAssignableFrom(toCheck));
        }

        /// <summary>Returns wheter the given instance is of a given type.</summary>
        /// <param name="obj">The instance to check.</param>
        /// <param name="type">Type to check against.</param>
        /// <param name="orInherited">Returns true if the instance is inherited from the type argument.</param>
        public static bool IsOfType<T>(this T obj, Type type, bool orInherited = true) {
            return obj.GetType().IsOfType(type, orInherited);
        }

        /// <summary>Throws an exception if the instance is not of the given type.</summary>
        /// <param name="obj">The instance to check.</param>
        /// <param name="type">Type to check against.</param>
        /// <param name="orInherited">Do not throw if the instance is inherited from the type argument.</param>
        public static void EnsureOfType<T>(this T obj, Type type, bool orInherited = true) {
            if (!obj.IsOfType(type, orInherited))
                throw new InvalidCastException(
                    string.Format("Object {0} must be of type {1}{2}",
                        obj.GetType().FullName,
                        type.FullName,
                        orInherited? " or inherited from it": ""
                    )
                );
        }

        /// <summary>Returns whether the type defines the static field.</summary>
        public static bool HasField(this Type type, string fieldName) {
            return type.FindField(fieldName, false) != null;
        }

        /// <summary>Returns whether the type defines the static property.</summary>
        public static bool HasProperty(this Type type, string propertyName) {
            return type.FindProperty(propertyName, false) != null;
        }

        /// <summary>Returns whether the type defines the static method.</summary>
        public static bool HasMethod(this Type type, string methodName, Type[] args = null) {
            return type.FindMethod(methodName, args, false) != null;
        }

        /// <summary>Returns whether the object type defines the instance field.</summary>
        public static bool HasField(this object obj, string fieldName) {
            return obj.GetType().HasField(fieldName);
        }

        /// <summary>Returns whether the object type defines the instance property.</summary>
        public static bool HasProperty(this object obj, string propertyName) {
            return obj.GetType().HasProperty(propertyName);
        }

        /// <summary>Returns whether the object type defines the instance method.</summary>
        public static bool HasMethod(this object obj, string methodName, Type[] args = null) {
            return obj.GetType().HasMethod(methodName, args);
        }

    }
}
