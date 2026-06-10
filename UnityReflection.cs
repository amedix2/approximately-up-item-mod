using System;
using System.Reflection;

namespace ApproximatelyUpMod
{
    internal static class UnityReflection
    {
        internal struct RectData
        {
            internal readonly float X;
            internal readonly float Y;
            internal readonly float Width;
            internal readonly float Height;

            internal RectData(float x, float y, float width, float height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }
        }

        private const BindingFlags StaticMembers = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static Type _inputType;
        private static Type _keyCodeType;
        private static Type _timeType;
        private static Type _cursorType;
        private static Type _cursorLockModeType;
        private static Type _rectType;
        private static Type _vector2Type;
        private static Type _guiType;
        private static Type _guiLayoutType;
        private static Type _guiLayoutOptionType;

        internal static double RealtimeSinceStartup
        {
            get
            {
                object value = GetStaticProperty(TimeType, "realtimeSinceStartupAsDouble");
                if (value is double)
                {
                    return (double)value;
                }

                value = GetStaticProperty(TimeType, "realtimeSinceStartup");
                if (value is float)
                {
                    return (float)value;
                }

                return 0.0;
            }
        }

        internal static bool GetKeyDown(int keyCodeValue)
        {
            Type inputType = InputType;
            Type keyCodeType = KeyCodeType;
            if (inputType == null || keyCodeType == null)
            {
                return false;
            }

            MethodInfo method = inputType.GetMethod("GetKeyDown", StaticMembers, null, new[] { keyCodeType }, null);
            if (method == null)
            {
                return false;
            }

            object keyCode = Enum.ToObject(keyCodeType, keyCodeValue);
            object result = method.Invoke(null, new[] { keyCode });
            return result is bool && (bool)result;
        }

        internal static object GetCursorLockState()
        {
            return GetStaticProperty(CursorType, "lockState");
        }

        internal static void SetCursorVisible(bool visible)
        {
            SetStaticProperty(CursorType, "visible", visible);
        }

        internal static void SetCursorLockStateNone()
        {
            Type lockType = CursorLockModeType;
            if (lockType == null)
            {
                return;
            }

            SetCursorLockState(Enum.ToObject(lockType, 0));
        }

        internal static void SetCursorLockState(object lockState)
        {
            if (lockState != null)
            {
                SetStaticProperty(CursorType, "lockState", lockState);
            }
        }

        internal static object CreateVector2(float x, float y)
        {
            Type type = Vector2Type;
            if (type == null)
            {
                return null;
            }

            return Activator.CreateInstance(type, new object[] { x, y });
        }

        internal static void GuiBox(RectData rect, string text)
        {
            InvokeStatic(GuiType, "Box", new[] { RectType, typeof(string) }, new[] { CreateRect(rect), text });
        }

        internal static void BeginArea(RectData rect)
        {
            InvokeStatic(GUILayoutType, "BeginArea", new[] { RectType }, new[] { CreateRect(rect) });
        }

        internal static void EndArea()
        {
            InvokeStatic(GUILayoutType, "EndArea", Type.EmptyTypes, new object[0]);
        }

        internal static void Label(string text)
        {
            InvokeStatic(GUILayoutType, "Label", new[] { typeof(string), GUILayoutOptionArrayType }, new object[] { text, EmptyOptions() });
        }

        internal static bool Button(string text)
        {
            object result = InvokeStatic(GUILayoutType, "Button", new[] { typeof(string), GUILayoutOptionArrayType }, new object[] { text, EmptyOptions() });
            return result is bool && (bool)result;
        }

        internal static bool Button(string text, float width)
        {
            object result = InvokeStatic(GUILayoutType, "Button", new[] { typeof(string), GUILayoutOptionArrayType }, new object[] { text, Options(Width(width)) });
            return result is bool && (bool)result;
        }

        internal static void BeginHorizontal()
        {
            InvokeStatic(GUILayoutType, "BeginHorizontal", new[] { GUILayoutOptionArrayType }, new object[] { EmptyOptions() });
        }

        internal static void EndHorizontal()
        {
            InvokeStatic(GUILayoutType, "EndHorizontal", Type.EmptyTypes, new object[0]);
        }

        internal static void Space(float pixels)
        {
            InvokeStatic(GUILayoutType, "Space", new[] { typeof(float) }, new object[] { pixels });
        }

        internal static object BeginScrollView(object scrollPosition, float height)
        {
            object result = InvokeStatic(GUILayoutType, "BeginScrollView", new[] { Vector2Type, GUILayoutOptionArrayType }, new[] { scrollPosition ?? CreateVector2(0f, 0f), Options(Height(height)) });
            return result ?? scrollPosition;
        }

        internal static void EndScrollView()
        {
            InvokeStatic(GUILayoutType, "EndScrollView", Type.EmptyTypes, new object[0]);
        }

        private static object CreateRect(RectData rect)
        {
            Type type = RectType;
            if (type == null)
            {
                return null;
            }

            return Activator.CreateInstance(type, new object[] { rect.X, rect.Y, rect.Width, rect.Height });
        }

        private static object Width(float value)
        {
            return InvokeStatic(GUILayoutType, "Width", new[] { typeof(float) }, new object[] { value });
        }

        private static object Height(float value)
        {
            return InvokeStatic(GUILayoutType, "Height", new[] { typeof(float) }, new object[] { value });
        }

        private static Array EmptyOptions()
        {
            return Array.CreateInstance(GUILayoutOptionType, 0);
        }

        private static Array Options(object option)
        {
            Array options = Array.CreateInstance(GUILayoutOptionType, option == null ? 0 : 1);
            if (option != null)
            {
                options.SetValue(option, 0);
            }

            return options;
        }

        private static Type GUILayoutOptionArrayType
        {
            get { return GUILayoutOptionType == null ? null : GUILayoutOptionType.MakeArrayType(); }
        }

        private static Type InputType
        {
            get { return _inputType ?? (_inputType = FindUnityType("UnityEngine.Input")); }
        }

        private static Type KeyCodeType
        {
            get { return _keyCodeType ?? (_keyCodeType = FindUnityType("UnityEngine.KeyCode")); }
        }

        private static Type TimeType
        {
            get { return _timeType ?? (_timeType = FindUnityType("UnityEngine.Time")); }
        }

        private static Type CursorType
        {
            get { return _cursorType ?? (_cursorType = FindUnityType("UnityEngine.Cursor")); }
        }

        private static Type CursorLockModeType
        {
            get { return _cursorLockModeType ?? (_cursorLockModeType = FindUnityType("UnityEngine.CursorLockMode")); }
        }

        private static Type RectType
        {
            get { return _rectType ?? (_rectType = FindUnityType("UnityEngine.Rect")); }
        }

        private static Type Vector2Type
        {
            get { return _vector2Type ?? (_vector2Type = FindUnityType("UnityEngine.Vector2")); }
        }

        private static Type GuiType
        {
            get { return _guiType ?? (_guiType = FindUnityType("UnityEngine.GUI")); }
        }

        private static Type GUILayoutType
        {
            get { return _guiLayoutType ?? (_guiLayoutType = FindUnityType("UnityEngine.GUILayout")); }
        }

        private static Type GUILayoutOptionType
        {
            get { return _guiLayoutOptionType ?? (_guiLayoutOptionType = FindUnityType("UnityEngine.GUILayoutOption")); }
        }

        private static Type FindUnityType(string fullName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type type = assemblies[i].GetType(fullName, false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static object GetStaticProperty(Type type, string name)
        {
            if (type == null)
            {
                return null;
            }

            PropertyInfo property = type.GetProperty(name, StaticMembers);
            return property == null ? null : property.GetValue(null, null);
        }

        private static void SetStaticProperty(Type type, string name, object value)
        {
            if (type == null)
            {
                return;
            }

            PropertyInfo property = type.GetProperty(name, StaticMembers);
            if (property != null && property.CanWrite)
            {
                property.SetValue(null, value, null);
            }
        }

        private static object InvokeStatic(Type type, string methodName, Type[] parameterTypes, object[] args)
        {
            if (type == null || parameterTypes == null)
            {
                return null;
            }

            MethodInfo method = type.GetMethod(methodName, StaticMembers, null, parameterTypes, null);
            return method == null ? null : method.Invoke(null, args);
        }
    }
}
