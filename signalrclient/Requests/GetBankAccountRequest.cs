using signalrclient.Backend.Models;

namespace signalrclient.Requests
{
    public record GetBankAccountRequest(string bankaccountNumber) : IBffRequest<GetBankAccountResponse>;
}
