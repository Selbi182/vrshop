using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private const string TINT_COLOR = "_TintColor";

    public int articleId;
    public string name;
    private string oldName;
    public decimal price;
    private decimal oldPrice;

    private const char CURRENCY_SYMBOL = '€';

    void Awake() {
        // Find all the objects from the children
        colorObject = transform.Find(COLOR_OBJECT).gameObject;

        frontObject = transform.Find(FRONT_OBJECT).gameObject;
        nameObjectFront = frontObject.transform.Find(NAME_OBJECT).gameObject;
        imageObjectFront = frontObject.transform.Find(IMAGE_OBJECT).gameObject;
        priceObjectFront = frontObject.transform.Find(PRICE_OBJECT).gameObject;

        backObject = transform.Find(BACK_OBJECT).gameObject;
        // TODO backface

        // TODO nur zum testen
        articleId = 0;
        name = "Lorem ipsum";
        price = 12.99m;
    }
    

    void Update() {
        if (oldName != name) {
            UpdateName();
            oldName = name;
        }

        if (oldPrice != price) {
            UpdatePrice();
            oldPrice = price;
        }
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
    }

    public void SetBacksideActive(bool active) {
        if (backObject != null) {
            backObject.SetActive(active);
        }
    }

    private void UpdateName() {
        if (nameObjectFront != null) {
            TMPro.TextMeshPro textMesh = nameObjectFront.transform.GetComponent<TMPro.TextMeshPro>();
            textMesh.SetText(name);
        }
    }

    private void UpdatePrice() {
        if (priceObjectFront != null) {
            string newPrice = string.Format("{0:0.00} {1}", price.ToString(), CURRENCY_SYMBOL);
            TMPro.TextMeshPro textMesh = priceObjectFront.transform.GetComponent<TMPro.TextMeshPro>();
            textMesh.SetText(newPrice);
        }
    }
}
