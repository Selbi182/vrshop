using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ArticleMonitorWrapper : MonoBehaviour {

    public GameObject colorObject;

    public GameObject frontObject;
    public GameObject nameObjectFront;
    public GameObject imageObjectFront;
    public GameObject priceObjectFront;

    public GameObject backObject;
    public GameObject nameObjectBack;
    public GameObject imageObjectBack;
    public GameObject priceObjectBack;
    public GameObject descriptionObjectBack;
    public GameObject cartObjectBack;

    private const string COLOR_OBJECT = "Color";
    private const string FRONT_OBJECT = "Front";
    private const string BACK_OBJECT  = "Back";
    private const string NAME_OBJECT  = "Name";
    private const string IMAGE_OBJECT = "Image";
    private const string PRICE_OBJECT = "Price";
    private const string DESCRIPTION_OBJECT = "Description";
    private const string CART_OBJECT = "Cart";

    private const string TINT_COLOR = "_TintColor";

    public int wallPositionId;
    public int articleItemId;

    private VRShopArticle article;
    private new string name;
    private decimal price;
    private string description;

    private int cartQuanity;
    private const int DEFAULT_QUANTITY = 1;
    private const char CURRENCY_SYMBOL = '€';

    void Awake() {
        // Find all the objects from the children
        colorObject = transform.Find(COLOR_OBJECT).gameObject;

        frontObject = transform.Find(FRONT_OBJECT).gameObject;
        nameObjectFront = frontObject.transform.Find(NAME_OBJECT).gameObject;
        imageObjectFront = frontObject.transform.Find(IMAGE_OBJECT).gameObject;
        priceObjectFront = frontObject.transform.Find(PRICE_OBJECT).gameObject;

        backObject = transform.Find(BACK_OBJECT).gameObject;
        nameObjectBack = backObject.transform.Find(NAME_OBJECT).gameObject;
        imageObjectBack = backObject.transform.Find(IMAGE_OBJECT).gameObject;
        priceObjectBack = backObject.transform.Find(PRICE_OBJECT).gameObject;
        descriptionObjectBack = backObject.transform.Find(DESCRIPTION_OBJECT).gameObject;
        cartObjectBack = backObject.transform.Find(CART_OBJECT).gameObject;

        // TODO nur zum testen
        name = "Lorem ipsum";
        price = 12.99m;
        articleItemId = 0;
        cartQuanity = 2;
    }

    public Color GetMonitorColor() {
        if (colorObject != null) {
            return transform.GetComponent<Renderer>().material.GetColor(TINT_COLOR);
        }
        return Color.black;
    }

    public void SetMonitorColor(Color color) {
        if (colorObject != null && color != null) {
            colorObject.GetComponent<Renderer>().material.SetColor(TINT_COLOR, color);
        }
    }

    public void SetMonitorAlpha(float alpha) {
        // TODO fix proper fading
        /*
        foreach (Transform child in transform) {
            Renderer r = child.GetComponent<Renderer>();
            if (r != null) {
                TMPro.TextMeshPro textMesh = child.GetComponent<TMPro.TextMeshPro>();
                if (textMesh != null) {

                } else {
                    Color transparentScreenColor = r.material.GetColor("_Color");
                    transparentScreenColor.a = alpha;
                    r.material.SetColor("_Color", transparentScreenColor);
                }
            }
        }
        */
    }

    public void SetBacksideActive(bool active) {
        if (backObject != null) {
            backObject.SetActive(active);
        }
    }

    private void UpdateName() {
        if (nameObjectFront != null && nameObjectBack != null) {
            // Front
            TextMeshPro frontName = nameObjectFront.transform.GetComponent<TextMeshPro>();
            frontName.SetText(name);
            frontName.ForceMeshUpdate(true);

            // Back
            TextMeshPro backName = nameObjectBack.transform.GetComponent<TextMeshPro>();
            backName.SetText(name);
            backName.ForceMeshUpdate(true);
        }
    }

    private void UpdateDescription() {
        if (descriptionObjectBack != null) {
            // Back
            TextMeshPro backDescription = descriptionObjectBack.transform.GetComponent<TextMeshPro>();
            backDescription.SetText(description);
            backDescription.ForceMeshUpdate(true);
        }
    }

    private void UpdatePrice() {
        if (priceObjectFront != null && nameObjectBack != null) {
            // Front
            string newPriceFront = string.Format("{0:0.00} {1}", price.ToString(), CURRENCY_SYMBOL);
            TextMeshPro frontPrice = priceObjectFront.transform.GetComponent<TextMeshPro>();
            frontPrice.SetText(newPriceFront);
            frontPrice.ForceMeshUpdate(true);

            // Back
            //1x 2229,99€ = 2229,99€
            string newPriceBack = string.Format("{2}x {0:0.00} {1} = {3:0.00} {1}", price.ToString(), CURRENCY_SYMBOL, cartQuanity, (price*cartQuanity).ToString());
            TextMeshPro backPrice = priceObjectBack.transform.GetComponent<TextMeshPro>();
            backPrice.SetText(newPriceBack);
            backPrice.ForceMeshUpdate(true);
        }
    }

    public void SetArticle(VRShopArticle article) {
        if (this.article == article) {
            return;
        }
        cartQuanity = DEFAULT_QUANTITY;

        name = article.Name;
        price = article.Price;
        description = article.Description;
        // TODO the rest

        UpdateName();
        UpdatePrice();
        UpdateDescription();
    }
}
