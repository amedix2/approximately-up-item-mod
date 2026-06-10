using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ApproximatelyUpMod
{
    public partial class ItemListController
    {
        private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static Type _coreType;
        private static Type _uiManagerType;

        internal void ApplyMaterialsAmountFromUi(string rawValue)
        {
            int requestedAmount;
            if (!TryParseMaterialsAmount(rawValue, out requestedAmount))
            {
                ModLog.Warn("Set item amount aborted: invalid number. Enter a value from 1 to " + MaxMaterialsAmount + ".");
                return;
            }

            ApplyMaterialsAmount(requestedAmount);
        }

        private static bool TryParseMaterialsAmount(string rawValue, out int amount)
        {
            amount = DefaultMaterialsAmount;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            int parsed;
            if (!int.TryParse(rawValue.Trim(), out parsed))
            {
                return false;
            }

            amount = Clamp(parsed, 1, MaxMaterialsAmount);
            return true;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private bool ApplyMaterialsAmount(int amount)
        {
            try
            {
                object core = GetCore();
                object map = GetMember(core, "_componentsMap");
                object values = GetMember(map, "Values");
                if (core == null || map == null)
                {
                    ModLog.Warn("Set item amount aborted: Core/components are not ready.");
                    return false;
                }

                int updated = 0;
                foreach (object rawEntry in EnumerateObject(values ?? map))
                {
                    object component = ExtractDictionaryValue(rawEntry);
                    if (component == null)
                    {
                        continue;
                    }

                    if (SetMember(component, "_availableAmount", amount))
                    {
                        updated++;
                    }
                }

                if (updated == 0)
                {
                    ModLog.Warn("Set item amount aborted: no components were found.");
                    return false;
                }

                MaterialsAmountOverride = amount;
                EnforceMaterialsAmount = true;

                InvokeInstance(core, "RefreshSharedAvailableComponents");
                InvokeInstance(core, "RefreshPrivateAvailableComponents");
                ModLog.Info("Item amounts set to " + amount + ". Updated components: " + updated + ".");
                return true;
            }
            catch (Exception ex)
            {
                ModLog.Error("Set item amount failed: " + ex);
                return false;
            }
        }

        private void TryRefreshItems(bool force)
        {
            _nextRefreshAt = UnityReflection.RealtimeSinceStartup + (force ? 0.35 : 1.5);
            try
            {
                if (!force && _cacheReady)
                {
                    return;
                }

                object core = GetCore();
                object map = GetMember(core, "_componentsMap");
                object values = GetMember(map, "Values");
                if (core == null || map == null)
                {
                    return;
                }

                List<ItemEntry> refreshedItems = new List<ItemEntry>(512);
                foreach (object rawEntry in EnumerateObject(values ?? map))
                {
                    object component = ExtractDictionaryValue(rawEntry);
                    if (component == null)
                    {
                        continue;
                    }

                    string name = ResolveItemName(component);
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = component.ToString();
                    }

                    refreshedItems.Add(new ItemEntry
                    {
                        Component = component,
                        Name = name
                    });
                }

                if (refreshedItems.Count == 0)
                {
                    return;
                }

                refreshedItems.Sort(delegate(ItemEntry a, ItemEntry b)
                {
                    return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                });

                _allItems.Clear();
                _allItems.AddRange(refreshedItems);
                _cacheReady = true;
                _itemsRevision++;

                if (force)
                {
                    ModLog.Info("Item cache rebuilt. Total items: " + _allItems.Count);
                }
            }
            catch (Exception ex)
            {
                ModLog.Warn("Refresh item list failed: " + ex.Message);
            }
        }

        private void AssignToFirstHotbar(ItemEntry item)
        {
            try
            {
                if (item.Component == null)
                {
                    return;
                }

                object handComponentsList = GetHandComponentsList();
                if (handComponentsList == null)
                {
                    ModLog.Warn("AssignToFirstHotbar aborted: UIManager/hotbar not ready.");
                    return;
                }

                InvokeInstance(handComponentsList, "SetItemAtIndex", 0, item.Component);
                InvokeInstance(handComponentsList, "SetHandComponentText", item.Name);
                PlayUiClick();
            }
            catch (Exception ex)
            {
                ModLog.Error("Assign to hotbar failed: " + ex);
            }
        }

        private void UnlockAllItems()
        {
            try
            {
                TryRefreshItems(true);

                if (_allItems.Count == 0)
                {
                    ModLog.Warn("Unlock All Items aborted: item list is empty.");
                    return;
                }

                object handComponentsList = GetHandComponentsList();
                if (handComponentsList == null)
                {
                    ModLog.Warn("Unlock All Items aborted: hotbar is not ready.");
                    return;
                }

                int countToAssign = Math.Min(10, _allItems.Count);
                int startIndex = (_hotbarPage * 10) % _allItems.Count;

                int assigned = 0;
                for (int slot = 0; slot < countToAssign; slot++)
                {
                    ItemEntry entry = _allItems[(startIndex + slot) % _allItems.Count];
                    if (entry.Component == null)
                    {
                        continue;
                    }

                    InvokeInstance(handComponentsList, "SetItemAtIndex", slot, entry.Component);
                    assigned++;
                }

                if (assigned > 0)
                {
                    string previewName = _allItems[startIndex].Name;
                    InvokeInstance(handComponentsList, "SetHandComponentText", previewName);
                    PlayUiClick();
                    _hotbarPage++;
                }

                ModLog.Info("Unlock All Items completed: assigned " + assigned + " components to slots 1-10 from item " + (startIndex + 1) + ".");
            }
            catch (Exception ex)
            {
                ModLog.Error("Unlock All Items failed: " + ex);
            }
        }

        private static object GetCore()
        {
            Type coreType = GetCoreType();
            if (coreType == null)
            {
                return null;
            }

            return InvokeStatic(coreType, "Get");
        }

        private static Type GetCoreType()
        {
            if (_coreType == null)
            {
                _coreType = FindGameType("Core");
            }

            return _coreType;
        }

        private static Type GetUiManagerType()
        {
            if (_uiManagerType == null)
            {
                _uiManagerType = FindGameType("UIManager");
            }

            return _uiManagerType;
        }

        private static object GetHandComponentsList()
        {
            Type uiManagerType = GetUiManagerType();
            if (uiManagerType == null)
            {
                return null;
            }

            object ui = GetStaticMember(uiManagerType, "_singleton");
            if (ui == null)
            {
                return null;
            }

            return GetMember(ui, "_handComponentsList");
        }

        private static void PlayUiClick()
        {
            Type uiManagerType = GetUiManagerType();
            if (uiManagerType != null)
            {
                InvokeStatic(uiManagerType, "PlaySoundUIClick");
            }
        }

        private static string ResolveItemName(object component)
        {
            object name = InvokeInstance(component, "GetName");
            if (name == null)
            {
                name = GetMember(component, "name");
            }

            return name as string;
        }

        private static object ExtractDictionaryValue(object rawEntry)
        {
            if (rawEntry == null)
            {
                return null;
            }

            object value = GetMember(rawEntry, "Value");
            return value ?? rawEntry;
        }

        private static Type FindGameType(string shortTypeName)
        {
            string namespacedName = "Il2Cpp." + shortTypeName;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly asm = assemblies[i];
                Type type = asm.GetType(namespacedName, false);
                if (type != null)
                {
                    return type;
                }

                type = asm.GetType(shortTypeName, false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static IEnumerable<object> EnumerateObject(object collection)
        {
            if (collection == null)
            {
                yield break;
            }

            IEnumerable enumerable = collection as IEnumerable;
            if (enumerable != null)
            {
                foreach (object item in enumerable)
                {
                    yield return item;
                }
                yield break;
            }

            MethodInfo getEnumerator = FindMethod(collection.GetType(), "GetEnumerator", false, 0);
            if (getEnumerator == null)
            {
                yield break;
            }

            object enumerator = getEnumerator.Invoke(collection, null);
            if (enumerator == null)
            {
                yield break;
            }

            MethodInfo moveNext = FindMethod(enumerator.GetType(), "MoveNext", false, 0);
            PropertyInfo currentProperty = enumerator.GetType().GetProperty("Current", AnyInstance);
            if (moveNext == null || currentProperty == null)
            {
                yield break;
            }

            while ((bool)moveNext.Invoke(enumerator, null))
            {
                yield return currentProperty.GetValue(enumerator, null);
            }
        }

        private static object GetMember(object target, string name)
        {
            if (target == null)
            {
                return null;
            }

            return GetMember(target.GetType(), target, name, false);
        }

        private static object GetStaticMember(Type type, string name)
        {
            if (type == null)
            {
                return null;
            }

            return GetMember(type, null, name, true);
        }

        private static object GetMember(Type type, object target, string name, bool isStatic)
        {
            BindingFlags flags = isStatic ? AnyStatic : AnyInstance;
            try
            {
                PropertyInfo property = type.GetProperty(name, flags);
                if (property != null)
                {
                    return property.GetValue(target, null);
                }

                FieldInfo field = type.GetField(name, flags);
                if (field != null)
                {
                    return field.GetValue(target);
                }

                MethodInfo getter = FindMethod(type, "get_" + name, isStatic, 0);
                if (getter != null)
                {
                    return getter.Invoke(target, null);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static bool SetMember(object target, string name, object value)
        {
            if (target == null)
            {
                return false;
            }

            Type type = target.GetType();
            try
            {
                PropertyInfo property = type.GetProperty(name, AnyInstance);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(target, value, null);
                    return true;
                }

                FieldInfo field = type.GetField(name, AnyInstance);
                if (field != null)
                {
                    field.SetValue(target, value);
                    return true;
                }

                MethodInfo setter = FindMethod(type, "set_" + name, false, 1);
                if (setter != null)
                {
                    setter.Invoke(target, new object[] { value });
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static object InvokeInstance(object target, string name, params object[] args)
        {
            if (target == null)
            {
                return null;
            }

            MethodInfo method = FindMethod(target.GetType(), name, false, args == null ? 0 : args.Length);
            if (method == null)
            {
                return null;
            }

            return method.Invoke(target, args);
        }

        private static object InvokeStatic(Type type, string name, params object[] args)
        {
            if (type == null)
            {
                return null;
            }

            MethodInfo method = FindMethod(type, name, true, args == null ? 0 : args.Length);
            if (method == null)
            {
                return null;
            }

            return method.Invoke(null, args);
        }

        private static MethodInfo FindMethod(Type type, string name, bool isStatic, int parameterCount)
        {
            if (type == null)
            {
                return null;
            }

            MethodInfo[] methods = type.GetMethods((isStatic ? AnyStatic : AnyInstance));
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name == name && method.IsStatic == isStatic && method.GetParameters().Length == parameterCount)
                {
                    return method;
                }
            }

            return null;
        }
    }
}
