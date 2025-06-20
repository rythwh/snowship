﻿namespace Snowship.NUI
{
	public interface IUIGroup {
		IUIView View { get; }
		IUIPresenter Presenter { get; }
		IUIConfig Config { get; }
		IUIGroup Parent { get; }
		bool IsActive { get; }
		void OnCreate();
		void SetViewActive(bool active);
		public void AddChild(IUIGroup child);
		public void RemoveChild(IUIGroup child);
		public int ChildCount(int total);
		public void Close();
		public IUIGroup FindGroup(IUIPresenter presenterToFind);
		public IUIGroup FindGroup(IUIView viewToFind);
		public IUIGroup FindGroup<TConfig>() where TConfig : IUIConfig;
	}
}
