using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LiteDbExplorer.Core
{
    // Based on: https://www.telerik.com/blogs/lightweight-datatable-for-your-silverlight-applications

    public class LookupTable : IEnumerable, INotifyCollectionChanged
    {
        private LookupDataColumnCollection _columns;
        private ObservableCollection<LookupDataRow> _rows;
        private IList _internalView;
        private Type _elementType;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public LookupDataColumnCollection Columns => _columns ?? (_columns = new LookupDataColumnCollection());

        public IList<LookupDataRow> Rows
        {
            get
            {
                if (_rows == null)
                {
                    _rows = new ObservableCollection<LookupDataRow>();
                    _rows.CollectionChanged += OnRowsCollectionChanged;
                }

                return _rows;
            }
        }

        public LookupDataRow NewRow()
        {
            return new LookupDataRow(this);
        }


        private void OnRowsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    InternalView.Insert(e.NewStartingIndex, ((LookupDataRow) e.NewItems[0]).RowObject);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    InternalView.RemoveAt(e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    InternalView.Remove(((LookupDataRow) e.OldItems[0]).RowObject);
                    InternalView.Insert(e.NewStartingIndex, ((LookupDataRow) e.NewItems[0]).RowObject);
                    break;
                case NotifyCollectionChangedAction.Reset:
                default:
                    InternalView.Clear();
                    Rows.Select(r => r.RowObject).ToList().ForEach(o => InternalView.Add(o));
                    break;
            }
        }

        private IList InternalView
        {
            get
            {
                if (_internalView == null)
                {
                    CreateInternalView();
                }

                return _internalView;
            }
        }

        private void CreateInternalView()
        {
            _internalView =
                (IList) Activator.CreateInstance(typeof(ObservableCollection<>).MakeGenericType(ElementType));
            ((INotifyCollectionChanged) _internalView).CollectionChanged += (s, e) => { OnCollectionChanged(e); };
        }

        internal Type ElementType
        {
            get
            {
                if (_elementType == null)
                {
                    InitializeElementType();
                }

                return _elementType;
            }
        }

        private void InitializeElementType()
        {
            Seal();
            _elementType = DynamicObjectBuilder.GetDynamicObjectBuilderType(Columns);
        }

        private void Seal()
        {
            _columns.Seal();
        }

        public IEnumerator GetEnumerator()
        {
            return InternalView.GetEnumerator();
        }

        public IList ToList()
        {
            return InternalView;
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            handler?.Invoke(this, e);
        }
    }

    public class LookupDataRow
    {
        private readonly LookupTable _owner;
        private DynamicObject _rowObject;

        protected internal LookupDataRow(LookupTable owner)
        {
            _owner = owner;
        }

        public object this[string columnName]
        {
            get => RowObject.GetValue<object>(columnName);
            set => RowObject.SetValue(columnName, value);
        }

        internal DynamicObject RowObject
        {
            get
            {
                EnsureRowObject();
                return _rowObject;
            }
        }

        private void EnsureRowObject()
        {
            if (_rowObject == null)
            {
                _rowObject = (DynamicObject) Activator.CreateInstance(_owner.ElementType);
            }
        }
    }

    public class LookupDataColumn
    {
        public LookupDataColumn()
        {
            DataType = typeof(object);
        }

        public Type DataType { get; set; }
        public string ColumnName { get; set; }
    }

    public class LookupDataColumnCollection : IList<LookupDataColumn>
    {
        private IList<LookupDataColumn> _list;

        public LookupDataColumnCollection()
        {
            _list = new List<LookupDataColumn>();
        }

        public void Seal()
        {
            _list = new ReadOnlyCollection<LookupDataColumn>(_list);
        }

        public LookupDataColumn this[string columnName] =>
            _list.FirstOrDefault(p => p.ColumnName.Equals(columnName, StringComparison.Ordinal));

        public IEnumerator<LookupDataColumn> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _list).GetEnumerator();
        }

        public void Add(LookupDataColumn item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(LookupDataColumn item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(LookupDataColumn[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(LookupDataColumn item)
        {
            return _list.Remove(item);
        }

        public int Count => _list.Count;

        public bool IsReadOnly => _list.IsReadOnly;

        public int IndexOf(LookupDataColumn item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, LookupDataColumn item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public LookupDataColumn this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }
    }

    internal class DynamicObjectBuilder
    {
        private static readonly Dictionary<TypeSignature, Type> TypesCache = new Dictionary<TypeSignature, Type>();

        private static readonly AssemblyBuilder MicroModelAssemblyBuilder =
            AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(@"DynamicObjects"),
                AssemblyBuilderAccess.Run);

        private static readonly ModuleBuilder MicroModelModuleBuilder =
            MicroModelAssemblyBuilder.DefineDynamicModule(@"DynamicObjectsModule");

        private static readonly MethodInfo GetValueMethod =
            typeof(DynamicObject).GetMethod(@"GetValue", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo SetValueMethod =
            typeof(DynamicObject).GetMethod(@"SetValue", BindingFlags.Instance | BindingFlags.NonPublic);

        public static Type GetDynamicObjectBuilderType(IEnumerable<LookupDataColumn> properties)
        {
            var signature = new TypeSignature(properties);

            if (!TypesCache.TryGetValue(signature, out var type))
            {
                type = CreateDynamicObjectBuilderType(properties);
                TypesCache.Add(signature, type);
            }

            return type;
        }

        private static Type CreateDynamicObjectBuilderType(IEnumerable<LookupDataColumn> columns)
        {
            var typeBuilder =
                MicroModelModuleBuilder.DefineType($"DynamicObjectBuilder_{Guid.NewGuid()}", TypeAttributes.Public,
                    typeof(DynamicObject));

            foreach (var property in columns)
            {
                var propertyBuilder = typeBuilder.DefineProperty(property.ColumnName,
                    PropertyAttributes.None, property.DataType, null);

                CreateGetter(typeBuilder, propertyBuilder, property);
                CreateSetter(typeBuilder, propertyBuilder, property);
            }

            return typeBuilder.CreateTypeInfo();
        }

        private static void CreateGetter(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder,
            LookupDataColumn column)
        {
            var getMethodBuilder = typeBuilder.DefineMethod(
                $"get_{column.ColumnName}",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                CallingConventions.HasThis,
                column.DataType, Type.EmptyTypes);

            var getMethodIL = getMethodBuilder.GetILGenerator();
            getMethodIL.Emit(OpCodes.Ldarg_0);
            getMethodIL.Emit(OpCodes.Ldstr, column.ColumnName);
            getMethodIL.Emit(OpCodes.Callvirt, GetValueMethod.MakeGenericMethod(column.DataType));
            getMethodIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethodBuilder);
        }

        private static void CreateSetter(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder,
            LookupDataColumn column)
        {
            var setMethodBuilder = typeBuilder.DefineMethod(
                $"set_{column.ColumnName}",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                CallingConventions.HasThis,
                null, new[] {column.DataType});

            var setMethodIL = setMethodBuilder.GetILGenerator();
            setMethodIL.Emit(OpCodes.Ldarg_0);
            setMethodIL.Emit(OpCodes.Ldstr, column.ColumnName);
            setMethodIL.Emit(OpCodes.Ldarg_1);
            setMethodIL.Emit(OpCodes.Callvirt, SetValueMethod.MakeGenericMethod(column.DataType));
            setMethodIL.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(setMethodBuilder);
        }
    }

    public abstract class DynamicObject : INotifyPropertyChanged
    {
        private readonly Dictionary<string, object> _valuesStorage;

        public event PropertyChangedEventHandler PropertyChanged;

        protected DynamicObject()
        {
            _valuesStorage = new Dictionary<string, object>();
        }

        protected internal virtual T GetValue<T>(string propertyName)
        {
            if (!_valuesStorage.TryGetValue(propertyName, out var value))
            {
                return default(T);
            }

            return (T) value;
        }

        protected internal virtual void SetValue<T>(string propertyName, T value)
        {
            _valuesStorage[propertyName] = value;

            RaisePropertyChanged(propertyName);
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, args);
        }
    }

    internal class TypeSignature : IEquatable<TypeSignature>
    {
        private readonly int _hashCode;

        public TypeSignature(IEnumerable<LookupDataColumn> columns)
        {
            _hashCode = 0;
            foreach (var column in columns.OrderBy(p => p.ColumnName))
            {
                _hashCode ^= column.ColumnName.GetHashCode() ^ column.DataType.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            return ((obj is TypeSignature signature) && Equals(signature));
        }

        public bool Equals(TypeSignature other)
        {
            return other != null && _hashCode.Equals(other._hashCode);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}