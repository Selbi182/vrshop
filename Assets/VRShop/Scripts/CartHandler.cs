using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartHandler : MonoBehaviour {

    public enum CartItemType {
        IncreaseCart,
        DecreaseCart,
        AddToCart
    }
    public CartItemType cartItemType;
    private ArticleMonitorWrapper wrapper;

    void Start() {
        wrapper = GetComponentInParent<ArticleMonitorWrapper>();
    }

    public void HandleCartSelection() {
        switch (cartItemType) {
            case CartItemType.AddToCart:
                break;
            case CartItemType.DecreaseCart:
                if (wrapper.cartQuanity > 1) {
                    wrapper.cartQuanity -= 1;
                }
                break;
            case CartItemType.IncreaseCart:
                wrapper.cartQuanity += 1;
                break;
        }
    }
}
