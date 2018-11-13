using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRShopArticle {

    // Local Fields & Accessors
    public int Id { get; private set; }
    public decimal Price { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public float? Size { get; private set; }
    public byte[] Img { get; private set; }

    // Constructor
    public VRShopArticle(int id, decimal price, string name, string description, float? size, byte[] img) {
        Id = id;
        Price = price;
        Name = name;
        Description = description;
        Size = size;
        Img = img;
    }

    public bool HasModel() {
        // Having no scaling value implies the abscence of a model and can be used for quick checks
        if (Size == null) {
            return false;
        }

        // Otherwise, test for an actual existance of a model file
        
        // TODO tatsächlicher check auf die datei
        return true;
    }

    // Convencience ToString Override
    public override string ToString() {
        string text = string.Format("({0}) {1} [{2}] - {3}", Id, Name, Price, Description);
        return text;
    }
}
