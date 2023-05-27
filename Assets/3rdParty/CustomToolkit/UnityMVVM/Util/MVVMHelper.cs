using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CustomToolkit.UnityMVVM.Exceptions;
using UnityEngine;
using UnityWeld.Ioc;
using Component = UnityEngine.Component;

namespace CustomToolkit.UnityMVVM.Internal
{
    /// <summary>
    /// Helper class for setting up the factory for use in the editor.
    /// </summary>
    public static class MVVMHelper
    {
        private static Type[] typesWithBindingAttribute;

        /// <summary>
        /// Find all bindable properties on components attached to target
        /// </summary>
        /// <param name="target">Target object</param>
        /// <returns></returns>
        public static List<BindableMember<PropertyInfo>> FindBindableViewProperties(GameObject target)
        {
            if (target == null)
                return null;

            List<BindableMember<PropertyInfo>> bindableProperties = new List<BindableMember<PropertyInfo>>();

            Component[] components = target.GetComponents<Component>();

            foreach (Component component in components)
            {
                Type type = component.GetType();
                
                if(type.IsSubclassOf(typeof(AbstractMemberBinding)))
                    continue;
                
                IEnumerable<PropertyInfo> properties = GetPublicProperties(type);

                foreach (PropertyInfo propertyInfo in properties)
                {
                    if(propertyInfo.GetGetMethod(false) == null || propertyInfo.GetSetMethod(false) == null)
                        continue;

                    if(propertyInfo.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
                        continue;
                    
                    bindableProperties.Add(new BindableMember<PropertyInfo>(propertyInfo, type));
                }
            }

            return bindableProperties;
        }
        
        /// <summary>
        /// Find bindable properties in available view models.
        /// </summary>
        public static List<BindableMember<PropertyInfo>> FindBindableViewModelProperties(AbstractMemberBinding target)
        {
            List<Type> tentativeViewModels = FindViewModelsInHierarchy(target.transform);

            List<BindableMember<PropertyInfo>> bindableProperties = new List<BindableMember<PropertyInfo>>();

            foreach (Type viewModel in tentativeViewModels)
            {
                IEnumerable<PropertyInfo> publicProperties = GetPublicProperties(viewModel);

                foreach (PropertyInfo property in publicProperties)
                {
                    if (property.GetCustomAttributes(typeof(BindingAttribute), false).Length <= 0)
                        continue;

                    bindableProperties.Add(new BindableMember<PropertyInfo>(property, viewModel));
                }
            }

            return bindableProperties;
        }
        
        /// <summary>
        /// Find all view models upwards in hierarchy from source transform
        /// </summary>
        /// <param name="source">Source transform to start the search from</param>
        /// <returns></returns>
        private static List<Type> FindViewModelsInHierarchy(Transform source)
        {
            List<Type> foundViewModels = new List<Type>();

            var transform = source;

            while (transform != null)
            {
                MonoBehaviour[] components = transform.GetComponents<MonoBehaviour>();
                
                foreach (MonoBehaviour component in components)
                {
                    if(component == null)
                        continue;

                    if (component is INotifyPropertyChanged viewModel)
                    {
                        foundViewModels.Add(viewModel.GetType());   
                    }
                    else if (component is IViewModelProvider viewModelProvider)
                    {
                        string viewModelName = viewModelProvider.GetViewModelTypeName();
                        
                        if(string.IsNullOrEmpty(viewModelName))
                            continue;

                        Type viewModelType = GetViewModelType(viewModelName);
                        
                        if(viewModelType != null)
                            foundViewModels.Add(viewModelType);
                    }
                }
                
                transform = transform.parent;
            }

            return foundViewModels;
        }
                
        /// <summary>
        /// Find all types with the binding attribute. This uses reflection to find all
        /// types the first time it runs and caches it for every other time. We can
        /// safely cache this data because it will only change if the loaded assemblies
        /// change, in which case everthing in managed memory will be throw out anyway.
        /// </summary>
        public static IEnumerable<Type> TypesWithBindingAttribute
        {
            get
            {
                if (typesWithBindingAttribute == null)
                {
                    typesWithBindingAttribute = FindTypesMarkedByAttribute(typeof(BindingAttribute));
                }

                return typesWithBindingAttribute;
            }
        }

        private static Type[] typesWithAdapterAttribute;

        /// <summary>
        /// Find all types with the binding attribute. This uses reflection to find all
        /// types the first time it runs and caches it for every other time. We can
        /// safely cache this data because it will only change if the loaded assemblies
        /// change, in which case everthing in managed memory will be throw out anyway.
        /// </summary>
        public static IEnumerable<Type> TypesWithAdapterAttribute
        {
            get
            {
                if (typesWithAdapterAttribute == null)
                {
                    typesWithAdapterAttribute = FindTypesMarkedByAttribute(typeof(AdapterAttribute));
                }

                return typesWithAdapterAttribute;
            }
        }

        private static Type[] typesWithWeldContainerAttribute;

        /// <summary>
        /// Find all types with WeldContainerAttribute. This works in the same way as
        /// TypesWithAdapterAttribute and TypesWithBindingAttribute in that it finds it
        /// using reflection the first time and then caches for performance.
        /// </summary>
        public static IEnumerable<Type> TypesWithWeldContainerAttribute
        {
            get
            {
                if (typesWithWeldContainerAttribute == null)
                {
                    typesWithWeldContainerAttribute = FindTypesMarkedByAttribute(typeof(WeldContainerAttribute));
                }

                return typesWithWeldContainerAttribute;
            }
        }

        /// <summary>
        /// Find all types marked with the specified attribute.
        /// </summary>
        private static Type[] FindTypesMarkedByAttribute(Type attributeType)
        {
            var typesFound = new List<Type>();

            foreach (var type in GetAllTypes())
            {
                try
                {
                    if (type.GetCustomAttributes(attributeType, false).Any())
                    {
                        typesFound.Add(type);
                    }
                }
                catch (Exception)
                {
                    // Some types throw an exception when we try to use reflection on them.
                }
            }

            return typesFound.ToArray();
        }

        public static IEnumerable<Type> GetAllViewModelTypes()
        {
            return GetAllTypes().Where(
                x => !x.IsAbstract && (x.IsSubclassOf(typeof(ViewModelMonoBehaviour))));
        }
        /// <summary>
        /// Returns an enumerable of all known types.
        /// </summary>
        private static IEnumerable<Type> GetAllTypes()
        {
            var assemblies =
                AppDomain.CurrentDomain.GetAssemblies()
                    // Automatically exclude the Unity assemblies, which throw exceptions when we try to access them.
                    .Where(a =>
                        !a.FullName.StartsWith("UnityEngine") &&
                        !a.FullName.StartsWith("UnityEditor"));

            foreach (var assembly in assemblies)
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (Exception)
                {
                    // Ignore assemblies that can't be loaded.
                    continue;
                }

                foreach (var type in types)
                {
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Find a particular type by its short name.
        /// </summary>
        public static Type FindAdapterType(string typeName)
        {
            var matchingTypes = TypesWithAdapterAttribute
                .Where(type => type.ToString() == typeName)
                .ToList();

            if (!matchingTypes.Any())
            {
                return null;
            }

            if (matchingTypes.Skip(1).Any())
            {
                throw new AmbiguousTypeException("Multiple types match: " + typeName);
            }

            return matchingTypes.First();
        }

        /// <summary>
        /// Find the [Adapter] attribute for a particular type.
        /// Returns null if there is no such attribute.
        /// </summary>
        public static AdapterAttribute FindAdapterAttribute(Type adapterType)
        {
            return adapterType
                .GetCustomAttributes(typeof(AdapterAttribute), false)
                .Cast<AdapterAttribute>()
                .FirstOrDefault();
        }

        /// <summary>
        /// Return the type of a view model bound by an IViewModelBinding
        /// </summary>
        private static Type GetViewModelType(string viewModelTypeName)
        {
            var type = TypesWithBindingAttribute
                .FirstOrDefault(t => t.ToString() == viewModelTypeName);

            return type;
        }

        /// <summary>
        /// Get all the declared and inherited public properties from a class or interface.
        ///
        /// https://stackoverflow.com/questions/358835/getproperties-to-return-all-properties-for-an-interface-inheritance-hierarchy#answer-26766221
        /// </summary>
        private static IEnumerable<PropertyInfo> GetPublicProperties(Type type)
        {
            if (!type.IsInterface)
            {
                return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            }

            return (new[] { type })
                .Concat(type.GetInterfaces())
                .SelectMany(i => i.GetProperties(BindingFlags.Public | BindingFlags.Instance));
        }

        /// <summary>
        /// Get all the declared and inherited public methods from a class or interface.
        /// </summary>
        private static IEnumerable<MethodInfo> GetPublicMethods(Type type)
        {
            return type.GetMethods();
            
            if (!type.IsInterface)
            {
                return type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            }

            return (new[] { type })
                .Concat(type.GetInterfaces())
                .SelectMany(i => i.GetMethods(BindingFlags.Public | BindingFlags.Instance));
        }

        /// <summary>
        /// Get a list of methods in the view model that we can bind to.
        /// </summary>
        public static BindableMember<MethodInfo>[] FindBindableMethods(EventBinding targetScript)
        {
            return FindViewModelsInHierarchy(targetScript.transform)
                .SelectMany(type => GetPublicMethods(type)
                    .Select(m => new BindableMember<MethodInfo>(m, type))
                )
                .Where(m => m.Member.GetParameters().Length == 0)
                .Where(m => m.Member.GetCustomAttributes(typeof(BindingAttribute), false).Any() 
                    && !m.MemberName.StartsWith("get_")) // Exclude property getters, since we aren't doing anything with the return value of the bound method anyway.
                .ToArray();
        }

        /// <summary>
        /// Find collection properties that can be data-bound.
        /// </summary>
        public static BindableMember<PropertyInfo>[] FindBindableCollectionProperties(CollectionBinding target)
        {
            return FindBindableViewModelProperties(target)
                .Where(p => typeof(IEnumerable).IsAssignableFrom(p.Member.PropertyType))
                .Where(p => !typeof(string).IsAssignableFrom(p.Member.PropertyType))
                .ToArray();
        }
    }
}
