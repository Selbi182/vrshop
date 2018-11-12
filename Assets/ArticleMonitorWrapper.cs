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

    public IList<GameObject> allChildren;

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

    private VRShopArticle assignedArticle;
    private string articleName;
    private decimal articlePrice;
    private string articleDescription;
    private byte[] articleImage;

    private const int DEFAULT_QUANTITY = 1;
    public int cartQuanity = DEFAULT_QUANTITY;
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
        // Front
        TextMeshPro frontName = nameObjectFront.transform.GetComponent<TextMeshPro>();
        frontName.SetText(articleName);
        frontName.ForceMeshUpdate(true);

        // Back
        TextMeshPro backName = nameObjectBack.transform.GetComponent<TextMeshPro>();
        backName.SetText(articleName);
        backName.ForceMeshUpdate(true);
    }

    private void UpdateDescription() {
        // Back
        TextMeshPro backDescription = descriptionObjectBack.transform.GetComponent<TextMeshPro>();
        backDescription.SetText(articleDescription);
        backDescription.ForceMeshUpdate(true);
    }

    private void UpdatePrice() {
        // Front
        string newPriceFront = string.Format("{0:0.00} {1}", articlePrice.ToString(), CURRENCY_SYMBOL);
        TextMeshPro frontPrice = priceObjectFront.transform.GetComponent<TextMeshPro>();
        frontPrice.SetText(newPriceFront);
        frontPrice.ForceMeshUpdate(true);

        // Back
        //1x 2229,99€ = 2229,99€
        string newPriceBack = string.Format("{2}x {0:0.00} {1} = {3:0.00} {1}", articlePrice.ToString(), CURRENCY_SYMBOL, cartQuanity, (articlePrice*cartQuanity).ToString());
        TextMeshPro backPrice = priceObjectBack.transform.GetComponent<TextMeshPro>();
        backPrice.SetText(newPriceBack);
        backPrice.ForceMeshUpdate(true);
    }

    private void UpdateImage() {
        if (articleImage != null) {
            Texture2D thumbnail = new Texture2D(1, 1);
            thumbnail.LoadImage(articleImage);
            imageObjectFront.GetComponent<Renderer>().material.mainTexture = thumbnail;
            imageObjectBack.GetComponent<Renderer>().material.mainTexture = thumbnail;

            // TODO aspect ratio
        }
    }

    public void SetArticle(VRShopArticle article) {
        if (article == assignedArticle) {
            return;
        }
        cartQuanity = DEFAULT_QUANTITY;

        articleName = article.Name;
        articlePrice = article.Price;
        articleDescription = article.Description;
        articleImage = article.Img;
        assignedArticle = article;

        UpdateName();
        UpdatePrice();
        UpdateDescription();
        UpdateImage();
    }

    public VRShopArticle GetArticle() {
        return assignedArticle;
    }
}
