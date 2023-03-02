using DataAccess.Model;
//using MongoDB.Driver;
using System.Threading.Tasks;

namespace DataAccess.DataAccess;
public class MongoDbDataAccess : IDataAccess
{
    private const string databaseName = "loppisdb";
    private const string salesCollectionName = "sales";

    public string ConnectionString { get; set; }
    public MongoDbDataAccess(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public Task WriteSale(Sale sale)
    {
        //var salesCollection = ConnectToMongo<Sale>(salesCollectionName);
        //return salesCollection.InsertOneAsync(sale);
        return Task.CompletedTask;

    }

    public Task RemoveSale(Sale sale)
    {
        //var salesCollection = ConnectToMongo<Sale>(salesCollectionName);
        //return salesCollection.DeleteOneAsync(s => s.Id == sale.Id);
        return Task.CompletedTask;
    }

    //private IMongoCollection<T> ConnectToMongo<T>(in string collection)
    //{
    //    var settings = MongoClientSettings.FromConnectionString(ConnectionString);
    //    settings.ServerApi = new ServerApi(ServerApiVersion.V1);
    //    var client = new MongoClient(settings);
    //    var db = client.GetDatabase(databaseName);
    //    return db.GetCollection<T>(collection);
    //}
}
