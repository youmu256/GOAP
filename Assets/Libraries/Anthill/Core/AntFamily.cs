using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

namespace Anthill.Core
{
	public class AntFamily<T> : IFamily<T>
	{
		private AntNodeList<T> _nodes;
		private Dictionary<AntEntity, T> _entities;
		private Dictionary<Type, PropertyInfo> _components;
		private AntNodePool<T> _pool;

		public AntFamily(AntNodePool<T> aPool = null)
		{
			_nodes = new AntNodeList<T>();
			_entities = new Dictionary<AntEntity, T>();
			_pool = (aPool != null) ? aPool : new AntNodePool<T>();

			//一般T都是某个Node类 比如HealthNode _component则是属性map
			var type = typeof(T);
			_components = type.GetProperties().ToDictionary(propInfo => propInfo.PropertyType, propInfo => propInfo);
		}

		#region Public Methods

		public void ComponentAdded(AntEntity aEntity, Type aComponentType)
		{
			if (!_entities.ContainsKey(aEntity))
			{
				AddEntity(aEntity);
			}
		}

		public void ComponentRemoved(AntEntity aEntity, Type aComponentType)
		{
			if (_entities.ContainsKey(aEntity) && _components.ContainsKey(aComponentType))
			{
				RemoveEntity(aEntity);
			}
		}

		public void EntityAdded(AntEntity aEntity)
		{
			if (!_entities.ContainsKey(aEntity))
			{
				AddEntity(aEntity);
			}
		}

		public void EntityRemoved(AntEntity aEntity)
		{
			if (_entities.ContainsKey(aEntity))
			{
				RemoveEntity(aEntity);
			}
		}

		#endregion
		#region Private Methods

		private void AddEntity(AntEntity aEntity)
		{
			//aEntity与该family做匹配检查 如果entity没有该组件 就不能加入这个family
			foreach (var pair in _components)
			{
				if (!aEntity.Has(pair.Key))
				{
					return;
				}
			}
			//封装node节点 来包装组件对象
			var node = _pool.Get();
			_entities[aEntity] = node;

			//用反射给node设置属性 node关心的组件被设置了上来
			foreach (var pair in _components)
			{
				pair.Value.SetValue(node, aEntity.Get(pair.Key), null);
			}
			
			_nodes.Add(node);
		}

		private void RemoveEntity(AntEntity aEntity)
		{
			var node = _entities[aEntity];
			_pool.Add(node);
			_entities.Remove(aEntity);
			_nodes.Remove(node);
		}

		#endregion
		#region Getters / Setters

		public AntNodeList<T> Nodes
		{
			get { return _nodes; }
		}

		#endregion
	}
}