using SmartSikshya.Data;
using SmartSikshya.Repo.HelperModels;
using System.Threading.Tasks;

namespace SmartSikshya.Repo
{
    public interface IPushNotificatrion
    {
        Task<string> Notify(string message);
        Task<string> SelectedNotify(string message, string[] subscribersId);
        Task<string> OneSignalIdSave(OneSignalSubscribers onesignalmodel);
        Task<OneSignalSubscribers> Get(string id);
        Task<string[]> StudentSubscriberFilter(filterStudentClass filter);
        Task<string[]> SubscriberFilterMultipleClass(filterStudentClass filter);
    }
}
