public class TradeProcessor
{
    private static float LotSize = 100000f;
    private ITradeInfoReader infoReader = null;
    private ITransactionExecutor transactionExecutor = null;

    TradeProcessor(ITradeInfoReader reader, ITransactionExecutor executor)
    {
      infoReader = reader;
      transactionExecutor = executor;
    }


    private IEnumerable<TradeRecord> GetTradeDetailsFromStream()
    {
      return infoReader.GetTradeDetailsFromStream();
    }
  
    private void ExecuteTrades(IEnumerable<TradeRecord> trades)
    {
        transactionExecutor.ExecuteTransactions(trades);
        
        Console.WriteLine("INFO: {0} trades processed", trades.Count);
    }
  
    public void ProcessTrades()
    {
          //Get trade details
          var trades = GetTradeDetailsFromStream();

          //Perform trade
          ExecuteTrades(trades);        
    }    
}

interface ITradeInfoReader
{
  public IEnumerable<TradeRecord> GetTradeDetailsFromStream(System.IO.Stream stream);
}
  
class TradeInfoReader : ITradeInfoReader
{
  private System.IO.Stream stream;

  TradeInfoReader(System.IO.Stream stream)
  {
    this.stream = stream;
  }

    public IEnumerable<TradeRecord> GetTradeDetailsFromStream()
    {
        //Get trade details from stream
          var rawTradeData = ReadTradesFromStream(stream);

        //Extract trade information
           var tradeDetails = ExtractTradeInfo(userInput);

        return tradeDetails;
    }

    private IEnumerable<string> ReadRawTradeInfoFromStream(System.IO.Stream stream)
    {
        // read rows
        var lines = new List<string>();
        using(var reader = new System.IO.StreamReader(stream))
        {
            string line;
            while((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }
        }

      return lines;
    }

    private IEnumerable<TradeRecord> ExtractTradeInfo(IEnumerable<string> lines)
    {
        var trades = new List<TradeRecord>();
      
        var lineCount = 1;
        foreach(var line in lines)
        {
            var fields = line.Split(new char[] { ',' });

            if(fields.Length != 3)
            {
                Console.WriteLine("WARN: Line {0} malformed. Only {1} field(s) found.", lineCount, fields.Length);
                continue;
            }

            if(fields[0].Length != 6)
            {
                Console.WriteLine("WARN: Trade currencies on line {0} malformed: '{1}'", lineCount, fields[0]);
                continue;
            }

            int tradeAmount;
            if(!int.TryParse(fields[1], out tradeAmount))
            {
                Console.WriteLine("WARN: Trade amount on line {0} not a valid integer: '{1}'", lineCount, fields[1]);
            }

            decimal tradePrice;
            if (!decimal.TryParse(fields[2], out tradePrice))
            {
                Console.WriteLine("WARN: Trade price on line {0} not a valid decimal: '{1}'", lineCount, fields[2]);
            }

            var sourceCurrencyCode = fields[0].Substring(0, 3);
            var destinationCurrencyCode = fields[0].Substring(3, 3);

            // calculate values
            var trade = new TradeRecord
            {
                SourceCurrency = sourceCurrencyCode,
                DestinationCurrency = destinationCurrencyCode,
                Lots = tradeAmount / LotSize,
                Price = tradePrice
            };

            trades.Add(trade);

            lineCount++;
        }

        return trades;
    }
}

interface ITransactionExecutor
  {
    public void ExecuteTransactions(IEnumerable<TradeRecord> trades);
  }
  

class SqlTransactionExecutor : ITransactionExecutor
{
  private string connectionString = string.Empty;

  SqlTransactionExecutor(string connString)
  {
      connectionString = connString;
  }

  public void ExecuteTransactions(IEnumerable<TradeRecord> trades)
  {
      using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                foreach(var trade in trades)
                {
                    var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = "dbo.insert_trade";
                    command.Parameters.AddWithValue("@sourceCurrency", trade.SourceCurrency);
                    command.Parameters.AddWithValue("@destinationCurrency", trade.DestinationCurrency);
                    command.Parameters.AddWithValue("@lots", trade.Lots);
                    command.Parameters.AddWithValue("@price", trade.Price);

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            connection.Close();
        }
    }
}


class TradeRecord
{
    public string SourceCurrency { get; set; }

    public string DestinationCurrency { get; set; }

    public float Lots { get; set; }

    public decimal Price { get; set; }
}


main()
{
    using (Stream inputStream = Console.OpenStandardInput())
    {
        //Retrives the value from App.config
        //"Data Source=(local);Initial Catalog=TradeDatabase;Integrated Security=True"
        
        string connString = ConfigurationManager.ConnectionStrings["SqlConnection"].ConnectionString;
        ITransactionExecutor executor = new SqlTransactionExecutor(connString);
        
        ITradeInfoReader reader = new TradeInfoReader(inputStream);

        TradeProcessor tradeProcessor = new TradeProcessor(reader, executor);
        tradeProcessor.ProcessTrades();
    }

  
}
