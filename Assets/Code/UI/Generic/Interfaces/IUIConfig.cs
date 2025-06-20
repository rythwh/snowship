﻿using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Snowship.NUI
{
	public interface IUIConfig {
		public bool Closeable { get; }
		public UniTask<(IUIView view, IUIPresenter presenter)> Open(Transform parent);
		public void OnClose();
	}

	public interface IUIConfigParameters : IUIConfig
	{
		public UniTask<(IUIView view, IUIPresenter presenter)> Open(Transform parent, IUIParameters parameters);
	}
}
