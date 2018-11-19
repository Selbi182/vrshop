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
    public byte[] Thumbnail { get; private set; }
    public float? ScaleFactor { get; private set; }

    // Constructor
    public VRShopArticle(int id, decimal price, string name, string description, byte[] thumbnail, float? scaleFactor) {
        Id = id;
        Price = price;
        Name = name;
        Description = description;
        ScaleFactor = scaleFactor;
        Thumbnail = thumbnail;
    }

    // Convencience ToString Override
    public override string ToString() {
        string text = string.Format("({0}) {1} [{2}] - {3}", Id, Name, Price, Description);
        return text;
    }
}
