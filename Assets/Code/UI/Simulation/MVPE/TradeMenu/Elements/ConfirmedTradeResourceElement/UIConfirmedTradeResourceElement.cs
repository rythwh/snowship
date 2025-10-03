using Cysharp.Threading.Tasks;
using Snowship.NResource;
using Snowship.NUI;

public class UIConfirmedTradeResourceElement : UIElement<UIConfirmedTradeResourceElementComponent> {

	private readonly ConfirmedTradeResourceAmount resourceAmount;

	public UIConfirmedTradeResourceElement(ConfirmedTradeResourceAmount resourceAmount) {
		this.resourceAmount = resourceAmount;
	}

	protected override UniTask OnCreate() {
		Component.SetResource(resourceAmount);
		return UniTask.CompletedTask;
	}
}
