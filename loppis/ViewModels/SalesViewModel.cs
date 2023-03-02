using DataAccess.DataAccess;
using DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using Avalonia.Media;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;

namespace loppis.ViewModels;

public delegate void ShutDownDelegate();
//public delegate MessageBoxResult ShowMessageBoxDelegate(string message, string caption);


public partial class SalesViewModel : ObservableObject
{
    #region Constants

    private const string cDefaultSaveFileName = @".\transactions.xml";
    private const string cSellerFileName = @".\sellers.csv";

    #endregion

    #region Properties

    [ObservableProperty]
    private SaleEntry currentEntry;

    [ObservableProperty]
    private int sumTotal;

    [ObservableProperty]
    private ObservableCollection<SaleEntry> itemList;

    [ObservableProperty]
    private bool idSelected;

    [ObservableProperty]
    private Brush? sellerIdBackground;

    [ObservableProperty]
    private Brush cashierBackground;

    [ObservableProperty]
    private string cashier;

    [ObservableProperty]
    private bool sellerIdFocused;

    [ObservableProperty]
    private bool priceFocused;

    private IDataAccessCollection dataAccess;

    public ObservableCollection<Sale> LastSalesList { get; set; }
    public ShutDownDelegate ShutDownFunction { get; set; }
    //public ShowMessageBoxDelegate MsgBoxFunction { get; set; }

    private const int NumberOfSalesToShow = 3;

    #endregion

    #region Construction

    void ShutDownApplication()
    {
        var lifetime = Application.Current?.ApplicationLifetime;
        if (lifetime == null) return;

        if (lifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.Close();
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0031:Use null propagation", Justification = "Does not work in this case.")]
    public SalesViewModel(string testFileName = cDefaultSaveFileName, IDataAccessCollection dataAccessCollection = null)
    {
        currentEntry = new SaleEntry();
        currentEntry.PropertyChanged += CurrentEntry_PropertyChanged;
        itemList = new ObservableCollection<SaleEntry>();
        LastSalesList = new();
        SellerList = new Dictionary<int, Seller>();
        ShutDownFunction = ShutDownApplication;
        //MsgBoxFunction = System.Windows.MessageBox.Show;
        SellerIdBackground = new SolidColorBrush(Colors.White);
        CashierBackground = new SolidColorBrush(Colors.White);
        SellerIdFocused = true;
        SaleEntry.CardId = null;
        SaleEntry.BagId = null;
        Cashier = "Säljare";

        dataAccess = dataAccessCollection ?? new DataAccessCollection();
        dataAccess.Add(new FileDataAccess(testFileName));
    }

    private void CurrentEntry_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        ExecuteClearCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region Commands

    public Dictionary<int, Seller> SellerList { get; set; }

    #region SaveToFile Command

    private bool CanSaveToFile()
    {
        bool validCashierName = IsCashierNameValid();
        SetCashierBackground(validCashierName);

        return SumTotal > 0 && ItemList.Count > 0 && validCashierName;
    }

    private void SetCashierBackground(bool validCashierName)
    {
        if (!validCashierName && ((SolidColorBrush)CashierBackground).Color == Colors.White)
        {
            CashierBackground = new SolidColorBrush(Colors.Orange);
        }
        else if (validCashierName && ((SolidColorBrush)CashierBackground).Color == Colors.Orange)
        {
            CashierBackground = new SolidColorBrush(Colors.White);
        }
    }

    private bool IsCashierNameValid()
    {
        return Cashier != "Säljare" && Cashier.Length > 0;
    }

    [RelayCommand(CanExecute ="CanSaveToFile")]
    private void SaveToFile()
    {
        var sale = new Sale(ItemList, Cashier);
        LastSalesList.Insert(0, sale);
        if (LastSalesList.Count > NumberOfSalesToShow)
        {
            LastSalesList.RemoveAt(NumberOfSalesToShow);
        }
        dataAccess.WriteSale(sale);
        ItemList.Clear();
        SumTotal = 0;
    }

    #endregion

    #region Card Command

    [RelayCommand]
    private void ExecuteCard()
    {
        // Default values
        CurrentEntry.SellerId = 600;
        CurrentEntry.Price = 15;

        foreach (var seller in SellerList)
        {
            if (seller.Value.Name == "Vykort")
            {
                CurrentEntry.SellerId = seller.Key;
                CurrentEntry.Price = seller.Value.DefaultPrice;
            }
        }

        MoveFocus();
    }

    #endregion

    #region Bag Command

    [RelayCommand]
    private void ExecuteBag()
    {
        // Default values
        CurrentEntry.SellerId = 500;
        CurrentEntry.Price = 5;

        foreach (var seller in SellerList)
        {
            if (seller.Value.Name == "Kasse")
            {
                CurrentEntry.SellerId = seller.Key;
                CurrentEntry.Price = seller.Value.DefaultPrice;
            }
        }
        MoveFocus();
    }

    #endregion

    #region RoundUp Command

    private bool CanRoundUp()
    {
        return (SumTotal % 50) != 0;
    }

    [RelayCommand(CanExecute = nameof(CanRoundUp))]
    private void RoundUp()
    {
        var rest = SumTotal % 50;
        CurrentEntry.SellerId = 999;
        CurrentEntry.Price = (50 - rest);
        MoveFocus();
    }

    #endregion

    #region Move Command

    private bool CanMoveFocus()
    {
        bool canExecute = CurrentEntry.SellerId != null && (SellerList.ContainsKey(CurrentEntry.SellerId.Value) || CurrentEntry.SellerId.Value == SaleEntry.RoundUpId);
        if (!canExecute && ((SolidColorBrush)SellerIdBackground).Color == Colors.White)
        {
            ((SolidColorBrush)SellerIdBackground).Color = Colors.Orange;
        }
        else if (canExecute && ((SolidColorBrush)SellerIdBackground).Color == Colors.Orange)
        {
            ((SolidColorBrush)SellerIdBackground).Color = Colors.White;
        }
        return canExecute;
    }

    [RelayCommand(CanExecute = nameof(CanMoveFocus))]
    private void MoveFocus()
    {
        SellerIdFocused = false;
        PriceFocused = true;
    }

    #endregion

    #region EnterSale Command

    private bool CanEnterSale()
    {

        bool canExecute = CurrentEntry.Price != null && CurrentEntry.Price > 0 && CurrentEntry.SellerId != null &&
            (SellerList.ContainsKey(CurrentEntry.SellerId.Value) || CurrentEntry.SellerId.Value == SaleEntry.RoundUpId);

        if (!canExecute && ((SolidColorBrush)SellerIdBackground).Color == Colors.White)
        {
            ((SolidColorBrush)SellerIdBackground).Color = Colors.Orange;
        }
        else if (canExecute && ((SolidColorBrush)SellerIdBackground).Color == Colors.Orange)
        {
            ((SolidColorBrush)SellerIdBackground).Color = Colors.White;
        }

        return canExecute;
    }

    [RelayCommand(CanExecute = nameof(CanEnterSale))]
    public void EnterSale()
    {
        ItemList.Insert(0, new SaleEntry(CurrentEntry.SellerId, CurrentEntry.Price));
        UpdateSumTotal();
        CurrentEntry.Clear();
        SetFocusToSellerId();
    }

    private void UpdateSumTotal()
    {
        var total = ItemList.Sum(i => i.Price);
        SumTotal = total != null ? (int)total : 0;
    }

    private void SetFocusToSellerId()
    {
        PriceFocused = false;
        SellerIdFocused = true;
    }

    #endregion

    #region LoadCommand

    [RelayCommand]
    private void Load()
    {
        XmlDocument doc = new();
        doc.Load("./config/settings.xml");
        {
            XmlNode node = doc.DocumentElement.SelectSingleNode("//DBConfig");
            if (node != null)
            {
                var dbUser = node?.Attributes["User"].Value ?? String.Empty;
                var dbPassword = node?.Attributes["Password"].Value ?? String.Empty;

                if (!string.IsNullOrEmpty(dbUser) && !string.IsNullOrEmpty(dbPassword))
                {
                    string connectionString = $"mongodb+srv://{dbUser}:{dbPassword}@cluster0.xuvri.mongodb.net/?retryWrites=true&w=majority";
                    if (connectionString != string.Empty)
                    {
                        dataAccess.Add(new MongoDbDataAccess(connectionString));
                    }
                }
            }
        }
        LoadSellerList(cSellerFileName);
    }


    public void LoadSellerList(string sellerFileName)
    {
        try
        {
            bool bagEntryInFile = false;
            bool cardEntryInFile = false;
            SellerList.Clear();
            string sellersContent = File.ReadAllText(sellerFileName);
            foreach (string line in sellersContent.Split("\r\n"))
            {
                string[] a = line.Split(";");
                Seller seller;
                int sellerId = int.Parse(a[0]);
                if (sellerId == SaleEntry.RoundUpId)
                {
                    throw new System.FormatException($"Id 999 is reserved. Please choose another id for row: {line}");
                }
                if (a.Length > 2)
                {
                    seller = new Seller() { Name = a[1], DefaultPrice = int.Parse(a[2]) };
                    SellerList.Add(sellerId, seller);
                }
                else if (a.Length > 1)
                {
                    seller = new Seller() { Name = a[1], DefaultPrice = null };
                    SellerList.Add(sellerId, seller);
                }
                else
                {
                    throw new System.FormatException($"The line was incorrectly formatted: {line}");
                }

                if (seller.Name == "Kasse" && seller.DefaultPrice != null)
                {
                    bagEntryInFile = true;
                    SaleEntry.BagId = sellerId;
                }
                if (seller.Name == "Vykort" && seller.DefaultPrice != null)
                {
                    cardEntryInFile = true;
                    SaleEntry.CardId = sellerId;

                }
            }
            if (!bagEntryInFile)
            {
                throw new System.FormatException("File must contain entry for \"Kasse\" with default price.");
            }
            if (!cardEntryInFile)
            {
                throw new System.FormatException("File must contain entry for \"Vykort\" with default price.");
            }
        }
        catch (FileNotFoundException)
        {
            //MsgBoxFunction(ex.Message, "Error!");
            ShutDownFunction();
        }
        catch (System.FormatException)
        {
            //MsgBoxFunction(ex.Message, "Error!");
            ShutDownFunction();
        }
        catch (System.ArgumentException)
        {
            //MsgBoxFunction(ex.Message, "Error!");
            ShutDownFunction();
        }
    }


    #endregion

    #region UndoCommand

    private bool CanExecuteUndo(object param)
    {
        return CurrentEntry.SellerId == null && CurrentEntry.Price == null;
    }

    private void ExecuteUndo(object param)
    {
        CurrentEntry = ItemList[(int)param];
        ItemList.RemoveAt((int)param);
        UpdateSumTotal();
        SetFocusToSellerId();
    }

    #endregion

    #region ClearCommand

    private bool CanExecuteClear()
    {
        return CurrentEntry.SellerId != null || CurrentEntry.Price != null;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteClear))]
    private void ExecuteClear()
    {
        CurrentEntry.SellerId = null;
        CurrentEntry.Price = null;
        SetFocusToSellerId();
    }

    #endregion

    #region EditPreviousSaleCommand
    private bool CanEditPreviousSale(object param)
    {
        return LastSalesList.Count > 0 && ItemList.Count == 0;
    }

    private void EditPreviousSale(object param)
    {
        var sale = LastSalesList[(int)param];
        foreach (var entry in sale.Entries)
        {
            ItemList.Add(entry);
        }
        LastSalesList.Remove(sale);

        UpdateSumTotal();

        dataAccess.RemoveSale(sale);
    }

    #endregion

    #endregion
}
