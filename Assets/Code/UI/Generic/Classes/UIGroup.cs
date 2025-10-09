using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Snowship.NUI
{
	public class UIGroup<TConfig, TView, TPresenter> : IUIGroup
		where TConfig : IUIConfig
		where TView : IUIView
		where TPresenter : IUIPresenter
	{
		private readonly TConfig config;
		private readonly TView view;
		private readonly TPresenter presenter;
		private readonly LifetimeScope scope;

		private readonly List<IUIGroup> children = new List<IUIGroup>();

		public IUIView View => view;
		public IUIPresenter Presenter => presenter;
		public IUIConfig Config => config;
		public IUIGroup Parent { get; }

		public bool IsActive => view.IsActive;

		public UIGroup(
			TConfig config,
			TView view,
			TPresenter presenter,
			LifetimeScope scope,
			IUIGroup parent
		) {
			this.config = config;
			this.view = view;
			this.presenter = presenter;
			this.scope = scope;

			Parent = parent;
		}

		public virtual async UniTask OnCreate() {
			Parent?.AddChild(this);

			view.OnOpen();
			await presenter.OnCreate();
		}

		public void OnPostCreate() {
			view.OnPostOpen();
			presenter.OnPostCreate();
		}

		public void SetViewActive(bool active) {
			view.SetActive(active);
		}

		public void AddChild(IUIGroup child) {
			children.Add(child);
		}

		public void RemoveChild(IUIGroup child) {
			children.Remove(child);
		}

		public int ChildCount(int total) {
			total += children.Count;
			foreach (IUIGroup child in children) {
				total = child.ChildCount(total);
			}
			return total;
		}

		public void Close() {
			foreach (IUIGroup child in children) {
				child.Close();
				break;
			}

			if (!config.Closeable) {
				return;
			}

			config.OnClose();
			presenter.OnClose();
			view.OnClose();
			scope.Dispose();

			Object.Destroy(view.Instance);

			Parent?.RemoveChild(this);
		}

		public IUIGroup FindGroup(IUIPresenter presenterToFind) {

			if (presenterToFind == null) {
				return null;
			}

			if (presenter.Equals(presenterToFind)) {
				return this;
			}

			foreach (IUIGroup child in children) {
				IUIGroup foundGroup = child.FindGroup(presenterToFind);
				if (foundGroup == null) {
					continue;
				}
				if (foundGroup.Presenter.Equals(presenterToFind)) {
					return foundGroup;
				}
			}

			return null;
		}

		public IUIGroup FindGroup(IUIView viewToFind) {

			if (viewToFind == null) {
				return null;
			}

			if (view.Equals(viewToFind)) {
				return this;
			}

			foreach (IUIGroup child in children) {
				IUIGroup foundGroup = child.FindGroup(viewToFind);
				if (foundGroup.View.Equals(viewToFind)) {
					return foundGroup;
				}
			}

			return null;
		}

		public IUIGroup FindGroup<TConfigToFind>() where TConfigToFind : IUIConfig {

			if (Config.GetType() == typeof(TConfigToFind)) {
				return this;
			}

			foreach (IUIGroup child in children) {
				IUIGroup foundGroup = child.FindGroup<TConfigToFind>();
				if (foundGroup?.Config.GetType() == typeof(TConfigToFind)) {
					return foundGroup;
				}
			}

			return null;
		}

	}
}
