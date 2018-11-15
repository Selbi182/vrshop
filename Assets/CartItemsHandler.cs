using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartItemsHandler : MonoBehaviour {

    public TextMesh priceMesh;
    public TextMesh countMesh;

    public decimal totalPrice;
    public int totalCount;

    private Dictionary<VRShopArticle, int> cart;

	void Start () {
		foreach (TextMesh tm in transform.GetComponentsInChildren<TextMesh>()) {
            if (tm.gameObject.name.Equals("Total")) {
                priceMesh = tm;
            } else if (tm.gameObject.name.Equals("Count")) {
                countMesh = tm;
            }
        }

        totalPrice = 0.00m;
        totalCount = 0;
        cart = new Dictionary<VRShopArticle, int>();
	}
	
	void Update () {
        string price = string.Format("{0:0.00} {1}", totalPrice.ToString(), "€");
        priceMesh.text = price;

        string count = string.Format("({0} Artikel)", totalCount.ToString());
        countMesh.text = count;
    }

    public void AddToCart(VRShopArticle article, int cartQuanity) {
        if (!cart.ContainsKey(article)) {
            cart.Add(article, cartQuanity);
        } else {
            cart[article] += cartQuanity;
        }
        RecountCart();
    }

    private void RecountCart() {
        decimal newTotal = 0.00m;
        int newCount = 0;
        foreach (VRShopArticle article in cart.Keys) {
            newTotal += article.Price * cart[article];
            newCount += cart[article];
        }
        totalPrice = newTotal;
        totalCount = newCount;
    }

}
