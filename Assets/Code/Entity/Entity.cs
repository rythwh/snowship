using System;
using System.Collections.Generic;
using Snowship.NUtilities;

namespace Snowship.NEntity
{
	public class Entity
	{
		public int Id { get; } = IdProvider<Entity>.Next();

		public IEnumerable<IComponent> Components => components;
		public IEnumerable<IStatProvider> StatProviders => statProviders;
		public IEnumerable<ICostProvider> CostProviders => costProviders;

		private readonly List<IComponent> components = new();
		private readonly Dictionary<Type, IComponent> componentsByType = new();
		private readonly List<IStatProvider> statProviders = new();
		private readonly List<ICostProvider> costProviders = new();

		public TComponent AddComponent<TComponent>(TComponent component) where TComponent : class, IComponent
		{
			Type type = typeof(TComponent);
			if (!componentsByType.TryAdd(type, component)) {
				throw new InvalidOperationException($"Component {type.Name} is already registered on entity {Id}");
			}

			components.Add(component);
			if (component is IStatProvider statProvider) {
				statProviders.Add(statProvider);
			}

			if (component is ICostProvider costProvider) {
				costProviders.Add(costProvider);
			}

			component.OnAttach(this);
			return component;
		}

		public bool TryGet<TComponent>(out TComponent component) where TComponent : class, IComponent
		{
			if (componentsByType.TryGetValue(typeof(TComponent), out IComponent rawComponent)) {
				component = rawComponent as TComponent;
				return true;
			}
			component = null;
			return false;
		}

		public TComponent Get<TComponent>() where TComponent : class, IComponent
		{
			if (!TryGet(out TComponent component)) {
				throw new KeyNotFoundException($"Component {typeof(TComponent)} not found on entity {Id}");
			}
			return component;
		}

		public bool TryGet(Type type, out IComponent component)
		{
			return componentsByType.TryGetValue(type, out component);
		}

		public bool Remove<T>() where T : class, IComponent
		{
			Type key = typeof(T);
			if (!componentsByType.Remove(key, out IComponent rawComponent)) {
				return false;
			}

			components.Remove(rawComponent);

			IStatProvider statProvider = rawComponent as IStatProvider;
			if (statProvider != null) {
				statProviders.Remove(statProvider);
			}

			ICostProvider costProvider = rawComponent as ICostProvider;
			if (costProvider != null) {
				costProviders.Remove(costProvider);
			}

			rawComponent.OnDetach();
			return true;
		}

		public bool Remove(Type type)
		{
			if (!componentsByType.Remove(type, out IComponent rawComponent)) {
				return false;
			}

			components.Remove(rawComponent);

			IStatProvider statProvider = rawComponent as IStatProvider;
			if (statProvider != null) {
				statProviders.Remove(statProvider);
			}

			ICostProvider costProvider = rawComponent as ICostProvider;
			if (costProvider != null) {
				costProviders.Remove(costProvider);
			}

			rawComponent.OnDetach();
			return true;
		}
	}
}
