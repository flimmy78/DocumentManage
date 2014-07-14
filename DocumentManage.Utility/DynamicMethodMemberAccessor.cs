using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DocumentManage.Utility
{
    //继承接口 DynamicMethodMemberAccessor， 实现对目标属性的访问
    public class DynamicMethodMemberAccessor : IMemberAccessor
    {
        private static readonly Dictionary<Type, IMemberAccessor> classAccessors = new Dictionary<Type, IMemberAccessor>();
        private static readonly object SyncObject = new object();  //新建目标

        //获取目标属性
        public object GetValue(object instance, string memberName)
        {
            return FindClassAccessor(instance).GetValue(instance, memberName);
        }

        //设置目标属性
        public void SetValue(object instance, string memberName, object newValue)
        {
            FindClassAccessor(instance).SetValue(instance, memberName, newValue);
        }

        private static IMemberAccessor FindClassAccessor(object instance)
        {
            lock (SyncObject)
            {
                var typekey = instance.GetType();
                IMemberAccessor classAccessor;
                classAccessors.TryGetValue(typekey, out classAccessor);
                if (classAccessor == null)
                {
                    classAccessor = Activator.CreateInstance(typeof(DynamicMethod<>).MakeGenericType(instance.GetType())) as IMemberAccessor;
                    classAccessors.Add(typekey, classAccessor);
                }

                return classAccessor;
            }
        }
    }

    //动态实现目标属性访问
    public class DynamicMethod<T> : IMemberAccessor
    {   
        //内部声明
        internal static Func<object, string, object> GetValueDelegate;
        internal static Action<object, string, object> SetValueDelegate;

        //获取目标属性
        public object GetValue(T instance, string memberName)
        {
            return GetValueDelegate(instance, memberName);
        }
        //设置目标属性
        public void SetValue(T instance, string memberName, object newValue)
        {
            SetValueDelegate(instance, memberName, newValue);
        }
        //获取目标属性
        public object GetValue(object instance, string memberName)
        {
            return GetValueDelegate(instance, memberName);
        }
        //设置目标属性
        public void SetValue(object instance, string memberName, object newValue)
        {
            SetValueDelegate(instance, memberName, newValue);
        }

        static DynamicMethod()
        {
            GetValueDelegate = GenerateGetValue();
            SetValueDelegate = GenerateSetValue();
        }

        //Func实现
        private static Func<object, string, object> GenerateGetValue()
        {
            var type = typeof(T);
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();
            foreach (var propertyInfo in type.GetProperties())
            {
                var property = Expression.Property(Expression.Convert(instance, typeof(T)), propertyInfo.Name);
                var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

                cases.Add(Expression.SwitchCase(Expression.Convert(property, typeof(object)), propertyHash));
            }
            var switchEx = Expression.Switch(nameHash, Expression.Constant(null), cases.ToArray());
            var methodBody = Expression.Block(typeof(object), new[] { nameHash }, calHash, switchEx);

            return Expression.Lambda<Func<object, string, object>>(methodBody, instance, memberName).Compile();
        }
        //Action实现
        private static Action<object, string, object> GenerateSetValue()
        {
            var type = typeof(T);
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var newValue = Expression.Parameter(typeof(object), "newValue");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();
            foreach (var propertyInfo in type.GetProperties())
            {
                var property = Expression.Property(Expression.Convert(instance, typeof(T)), propertyInfo.Name);
                var setValue = Expression.Assign(property, Expression.Convert(newValue, propertyInfo.PropertyType));
                var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));

                cases.Add(Expression.SwitchCase(Expression.Convert(setValue, typeof(object)), propertyHash));
            }
            var switchEx = Expression.Switch(nameHash, Expression.Constant(null), cases.ToArray());
            var methodBody = Expression.Block(typeof(object), new[] { nameHash }, calHash, switchEx);

            return Expression.Lambda<Action<object, string, object>>(methodBody, instance, memberName, newValue).Compile();
        }
    }

    /// <summary>
    /// Abstraction of the function of accessing member of a object at runtime.
    /// </summary>
    public interface IMemberAccessor
    {
        /// <summary>
        /// Get the member value of an object.
        /// </summary>
        /// <param name="instance">The object to get the member value from.</param>
        /// <param name="memberName">The member name, could be the name of a property of field. Must be public member.</param>
        /// <returns>The member value</returns>
        object GetValue(object instance, string memberName);

        /// <summary>
        /// Set the member value of an object.
        /// </summary>
        /// <param name="instance">The object to get the member value from.</param>
        /// <param name="memberName">The member name, could be the name of a property of field. Must be public member.</param>
        /// <param name="newValue">The new value of the property for the object instance.</param>
        void SetValue(object instance, string memberName, object newValue);
    }
}
