using System;
using System.Collections.Generic;
using System.Reflection;

namespace Shardion.Terrabreak
{
    public static class ReflectionHelper
    {
        public static IReadOnlyCollection<TType> ConstructParameterlessAssignables<TType>() where TType : class
        {
            List<TType> constructedObjects = [];
            foreach (Type type in typeof(Entrypoint).Assembly.GetTypes())
            {
                if (type.IsAssignableTo(typeof(TType)) && type.IsPublic && type.IsClass && !type.IsAbstract)
                {
                    ConstructorInfo? parameterlessCtor = type.GetConstructor([]);
                    if (parameterlessCtor is not null)
                    {
                        if (parameterlessCtor.Invoke([]) is TType objectOfTargetType)
                        {
                            constructedObjects.Add(objectOfTargetType);
                        }
                    }
                }
            }
            return constructedObjects;
        }

        public static IReadOnlyCollection<Type> GetParameterlessConstructibleAssignables<TType>()
        {
            List<Type> obtainedTypes = [];
            foreach (Type type in typeof(Entrypoint).Assembly.GetTypes())
            {
                if (type.IsAssignableTo(typeof(TType)) && type.IsPublic && type.IsClass && !type.IsAbstract)
                {
                    ConstructorInfo? parameterlessCtor = type.GetConstructor([]);
                    if (parameterlessCtor is not null)
                    {
                        obtainedTypes.Add(type);
                    }
                }
            }
            return obtainedTypes;
        }

        public static IReadOnlyCollection<Type> GetAssignables<TType>()
        {
            List<Type> obtainedTypes = [];
            foreach (Type type in typeof(Entrypoint).Assembly.GetTypes())
            {
                if (type.IsAssignableTo(typeof(TType)) && type.IsPublic && type.IsClass && !type.IsAbstract)
                {
                    obtainedTypes.Add(type);
                }
            }
            return obtainedTypes;
        }
    }
}
