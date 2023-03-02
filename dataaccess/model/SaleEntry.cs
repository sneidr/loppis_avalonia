//using MongoDB.Bson.Serialization.Attributes;
//using MongoDB.Bson;
//using Prism.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Xml.Serialization;

namespace DataAccess.Model;

public partial class SaleEntry : ObservableObject, IEquatable<SaleEntry>
{
    [XmlIgnore]
    //[BsonId]
    //[BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public const int RoundUpId = 999;
    public static int? CardId { get; set; }
    public static int? BagId { get; set; }

    public SaleEntry() : this(null, null)
    {
    }

    public SaleEntry(int? id, int? price)
    {
        SellerId = id;
        Price = price;
    }

    public void Clear()
    {
        SellerId = null;
        Price = null;
    }

    [ObservableProperty]
    private int? sellerId;

    [ObservableProperty]
    private int? price;

    [XmlIgnoreAttribute]
    public string SellerIdListText
    {
        get
        {
            if (SellerId == CardId)
            {
                return "Vykort      ";
            }
            else if (SellerId == BagId)
            {
                return "Kasse       ";
            }
            else if (SellerId == RoundUpId)
            {
                return "Avrundning  ";
            }
            else
            {
                return $"Säljare: {SellerId,3}";
            }
        }
    }

    [XmlIgnoreAttribute]
    public string PriceListText
    {
        get
        {
            string text = string.Format("{0,3}", Price);
            return text;
        }
    }

    public bool Equals(SaleEntry other)
    {
        if (other == null)
        { return false; }

        if (SellerId == other.SellerId && Price == other.Price)
        { return true; }
        else
        {
            return false;
        }

    }
}
