namespace InvestorDashboard.Backend.Services
{
    public interface IEthereumService
    {
        EthereumAccount CreateAccount();
        EthereumTransaction[] GetTransactionsByRecepientAddress(string address);
    }
}
