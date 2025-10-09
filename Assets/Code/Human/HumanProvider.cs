using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace Snowship.NHuman
{
	[UsedImplicitly]
	public class HumanProvider : IHumanQuery, IHumanWrite, IHumanEvents
	{
		private readonly List<Human> humans = new();
		private readonly Dictionary<Human, HumanView> humanToViewMap = new();
		private readonly Dictionary<Type, List<Human>> humansByType = new();

		public ReadOnlyCollection<Human> Humans => humans.AsReadOnly();

		public event Action<Human> OnHumanSelected;
		public event Action<Human> OnHumanRemoved;

		public void InvokeHumanSelected(Human human) => OnHumanSelected?.Invoke(human);
		public void InvokeHumanRemoved(Human human) => OnHumanRemoved?.Invoke(human);

		public void AddHuman<THuman>(Human human, HumanView humanView) where THuman : Human {
			Type humanType = typeof(THuman);
			if (!humansByType.TryAdd(humanType, new List<Human> { human })) {
				humansByType[humanType] ??= new List<Human>();
				humansByType[humanType].Add(human);
			}
			humanToViewMap.Add(human, humanView);
			humans.Add(human);
		}

		public void RemoveHuman(Human human) {

			humans.Remove(human);
			humansByType[human.GetType()].Remove(human);

			HumanView humanView = humanToViewMap[human];
			humanView.Unbind();
			Object.Destroy(humanView.gameObject);
			humanToViewMap.Remove(human);

			OnHumanRemoved?.Invoke(human);
		}

		/// <summary>
		/// If the return result of IEnumerable&lt;THuman&gt; is simply iterated over (foreach),
		/// no copy of the original list will be created -> better performance.
		/// If a copy is needed, can call ToList() on the result.
		/// </summary>
		/// <typeparam name="THuman"></typeparam>
		/// <returns>IEnumerable&lt;THuman&gt;</returns>
		public IEnumerable<THuman> GetHumans<THuman>() where THuman : Human {
			if (humansByType.TryGetValue(typeof(THuman), out List<Human> humansOfType)) {
				return humansOfType.Cast<THuman>();
			}
			humansByType[typeof(THuman)] = new List<Human>();
			return humansByType[typeof(THuman)].Cast<THuman>();
		}

		public int CountHumans<THuman>() where THuman : Human {
			if (humansByType.TryGetValue(typeof(THuman), out List<Human> humansOfType)) {
				return humansOfType.Count;
			}
			return 0;
		}

		public THumanView GetHumanView<THuman, THumanView>(THuman human) where THuman : Human where THumanView : HumanView {
			humanToViewMap.TryGetValue(human, out HumanView humanView);
			return humanView as THumanView;
		}

		public HumanView GetHumanView(Human human) {
			humanToViewMap.TryGetValue(human, out HumanView humanView);
			return humanView;
		}
	}

	public interface IHumanEvents
	{
		event Action<Human> OnHumanSelected;
		event Action<Human> OnHumanRemoved;

		void InvokeHumanSelected(Human human);
		void InvokeHumanRemoved(Human human);
	}

	public interface IHumanWrite
	{
		void AddHuman<THuman>(Human human, HumanView humanView) where THuman : Human;
		void RemoveHuman(Human human);
	}

	public interface IHumanQuery
	{
		ReadOnlyCollection<Human> Humans { get; }
		IEnumerable<THuman> GetHumans<THuman>() where THuman : Human;
		int CountHumans<THuman>() where THuman : Human;
		THumanView GetHumanView<THuman, THumanView>(THuman human) where THuman : Human where THumanView : HumanView;
		HumanView GetHumanView(Human human);
	}
}
