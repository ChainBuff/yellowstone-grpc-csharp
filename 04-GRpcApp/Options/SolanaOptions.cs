namespace _04_GRpcApp.Options;

public class SolanaOptions
{
    public SolanaOptions()
    {
        
    }
    public SolanaOptions(string mainUri,string mainWsUri,string rRpc,string gRpc)
    {
        this.MainUri=mainUri;
        this.MainWsUri=mainWsUri;
        this.MainWsrRpc = rRpc;
        this.MaingRpc = gRpc;
    }
    public string MainUri { get; set; }
    public string MainWsUri { get; set; }
    
    public string MainWsrRpc { get; set; }
    
    public string MaingRpc { get; set; }
}