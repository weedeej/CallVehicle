using CallVehicle.Phone;
using ScheduleOne.UI.Phone;

namespace CallVehicle
{
    public class PhoneAppCreator
    {
        public bool isReadyForUse = false;
        private bool _isAppIconCreated = false;
        private bool _isAppUICreated = false;
        private string _appName;
        private string _gameObjectName;
        private string _imageFileName;

        public PhoneAppCreator(string appName, string gameObjectName, string imageFileName)
        {
            _appName = appName;
            _gameObjectName = gameObjectName;
            _imageFileName = imageFileName;
        }
    }
}
